using AgentCfo.Core.Common;
using AgentCfo.Core.Entities;
using AgentCfo.Core.Enums;
using AgentCfo.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Stripe;

namespace AgentCfo.Infrastructure.Services;

public class StripeWebhookService
{
    private readonly ITransactionRepository _transactionRepo;
    private readonly IOrganizationRepository _organizationRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StripeWebhookService> _logger;
    
    public StripeWebhookService(
        ITransactionRepository transactionRepo,
        IOrganizationRepository organizationRepo,
        IUnitOfWork unitOfWork,
        ILogger<StripeWebhookService> logger)
    {
        _transactionRepo = transactionRepo;
        _organizationRepo = organizationRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async Task ProcessWebhookAsync(string json, string stripeSignature, string webhookSecret, CancellationToken ct = default)
    {
        var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);
        
        _logger.LogInformation("Processing Stripe event: {Type} ({Id})", stripeEvent.Type, stripeEvent.Id);
        
        switch (stripeEvent.Type)
        {
            case "payment_intent.succeeded":
                await HandlePaymentIntentSucceeded(stripeEvent, ct);
                break;
            case "payment_intent.payment_failed":
                await HandlePaymentFailed(stripeEvent, ct);
                break;
            case "invoice.paid":
                await HandleInvoicePaid(stripeEvent, ct);
                break;
            case "invoice.payment_failed":
                await HandleInvoicePaymentFailed(stripeEvent, ct);
                break;
            default:
                _logger.LogDebug("Unhandled event type: {Type}", stripeEvent.Type);
                break;
        }
    }
    
    private async Task HandlePaymentIntentSucceeded(Event stripeEvent, CancellationToken ct)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent is null) return;
        
        // Idempotency check
        var existing = await _transactionRepo.GetByStripeEventIdAsync(stripeEvent.Id, ct);
        if (existing is not null)
        {
            _logger.LogInformation("Skipping duplicate event {EventId}", stripeEvent.Id);
            return;
        }
        
        // Find organization by customer
        var org = await FindOrganizationForPayment(paymentIntent, ct);
        if (org is null)
        {
            _logger.LogWarning("No organization found for payment {PaymentIntentId}", paymentIntent.Id);
            return;
        }
        
        var transaction = Transaction.CreateRevenue(
            org.Id,
            Money.From(paymentIntent.AmountReceived / 100m, paymentIntent.Currency.ToUpperInvariant()),
            paymentIntent.Description ?? "Payment received",
            stripeEvent.Id,
            paymentIntent.Id);
        
        await _transactionRepo.AddAsync(transaction, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        _logger.LogInformation("Recorded revenue: {Amount} from {Customer}", 
            transaction.Amount, paymentIntent.CustomerId);
    }
    
    private async Task HandlePaymentFailed(Event stripeEvent, CancellationToken ct)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent is null) return;
        
        var org = await FindOrganizationForPayment(paymentIntent, ct);
        if (org is null) return;
        
        var transaction = Transaction.CreateRevenue(
            org.Id,
            Money.From(paymentIntent.Amount / 100m, paymentIntent.Currency.ToUpperInvariant()),
            $"Failed payment: {paymentIntent.LastPaymentError?.Message ?? "Unknown error"}",
            stripeEvent.Id,
            paymentIntent.Id);
        
        transaction.UpdateStatus(TransactionStatus.Failed);
        await _transactionRepo.AddAsync(transaction, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        _logger.LogWarning("Payment failed: {PaymentIntentId} - {Error}", 
            paymentIntent.Id, paymentIntent.LastPaymentError?.Message);
    }
    
    private async Task HandleInvoicePaid(Event stripeEvent, CancellationToken ct)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        if (invoice is null) return;
        
        var existing = await _transactionRepo.GetByStripeEventIdAsync(stripeEvent.Id, ct);
        if (existing is not null) return;
        
        var org = await _organizationRepo.GetByStripeCustomerIdAsync(invoice.CustomerId, ct);
        if (org is null) return;
        
        var transaction = Transaction.CreateRevenue(
            org.Id,
            Money.From(invoice.AmountPaid / 100m, invoice.Currency.ToUpperInvariant()),
            $"Invoice paid: {invoice.Number ?? invoice.Id}",
            stripeEvent.Id);
        
        await _transactionRepo.AddAsync(transaction, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
    
    private async Task HandleInvoicePaymentFailed(Event stripeEvent, CancellationToken ct)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        if (invoice is null) return;
        
        var org = await _organizationRepo.GetByStripeCustomerIdAsync(invoice.CustomerId, ct);
        if (org is null) return;
        
        var transaction = Transaction.CreateRevenue(
            org.Id,
            Money.From(invoice.AmountDue / 100m, invoice.Currency.ToUpperInvariant()),
            $"Invoice payment failed: {invoice.Number ?? invoice.Id}",
            stripeEvent.Id);
        
        transaction.UpdateStatus(TransactionStatus.Failed);
        await _transactionRepo.AddAsync(transaction, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
    
    private async Task<Organization?> FindOrganizationForPayment(PaymentIntent paymentIntent, CancellationToken ct)
    {
        if (paymentIntent.CustomerId is not null)
            return await _organizationRepo.GetByStripeCustomerIdAsync(paymentIntent.CustomerId, ct);
        
        // Fallback: look for metadata
        if (paymentIntent.Metadata.TryGetValue("organization_id", out var orgIdStr) && Guid.TryParse(orgIdStr, out var orgId))
            return await _organizationRepo.GetByIdAsync(orgId, ct);
        
        return null;
    }
}
