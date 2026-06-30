using AgentCfo.Core.Common;
using AgentCfo.Core.Entities;
using AgentCfo.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Stripe;

namespace AgentCfo.Infrastructure.Services;

/// <summary>
/// Stripe integration for AgentCFO. Creates real test-mode Stripe objects:
/// customers, subscriptions, and payment links. Demonstrates the agent's
/// ability to interact with financial infrastructure.
/// </summary>
public class StripeIntegrationService
{
    private readonly ILogger<StripeIntegrationService> _logger;
    private readonly IOrganizationRepository _orgRepo;
    private readonly IUnitOfWork _unitOfWork;

    public StripeIntegrationService(
        ILogger<StripeIntegrationService> logger,
        IOrganizationRepository orgRepo,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _orgRepo = orgRepo;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Creates a Stripe customer for the demo organization.
    /// Returns the Stripe customer ID.
    /// </summary>
    public async Task<string> CreateCustomerAsync(Organization org, CancellationToken ct = default)
    {
        _logger.LogInformation("Stripe: Creating customer for {OrgName}", org.Name);

        var service = new CustomerService();
        var customer = await service.CreateAsync(new CustomerCreateOptions
        {
            Name = org.Name,
            Email = $"demo@{org.Name.ToLower().Replace(" ", "")}.io",
            Description = $"AgentCFO demo organization — {org.Name}",
            Metadata = new Dictionary<string, string>
            {
                ["organization_id"] = org.Id.ToString(),
                ["source"] = "agentcfo-demo"
            }
        }, cancellationToken: ct);

        // Update org with Stripe customer ID
        org.UpdateStripeCustomerId(customer.Id);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Stripe: Created customer {CustomerId} for {OrgName}", customer.Id, org.Name);
        return customer.Id;
    }

    /// <summary>
    /// Creates a Stripe product + price + subscription for demo MRR.
    /// Simulates the organization's recurring revenue stream.
    /// </summary>
    public async Task<Subscription> CreateDemoSubscriptionAsync(
        string customerId, decimal monthlyAmount, CancellationToken ct = default)
    {
        _logger.LogInformation("Stripe: Creating ${Amount}/mo subscription for {Customer}", monthlyAmount, customerId);

        // Create a product
        var productService = new ProductService();
        var product = await productService.CreateAsync(new ProductCreateOptions
        {
            Name = "AgentCFO Pro Plan",
            Description = "Monthly subscription for AgentCFO financial management",
            Metadata = new Dictionary<string, string>
            {
                ["source"] = "agentcfo-demo"
            }
        }, cancellationToken: ct);

        // Create a price
        var priceService = new PriceService();
        var price = await priceService.CreateAsync(new PriceCreateOptions
        {
            Product = product.Id,
            UnitAmount = (long)(monthlyAmount * 100), // Stripe uses cents
            Currency = "usd",
            Recurring = new PriceRecurringOptions
            {
                Interval = "month"
            }
        }, cancellationToken: ct);

        // Create the subscription
        var subscriptionService = new SubscriptionService();
        var subscription = await subscriptionService.CreateAsync(new SubscriptionCreateOptions
        {
            Customer = customerId,
            Items = new List<SubscriptionItemOptions>
            {
                new() { Price = price.Id }
            },
            PaymentBehavior = "default_incomplete",
            Metadata = new Dictionary<string, string>
            {
                ["source"] = "agentcfo-demo",
                ["organization_id"] = customerId
            }
        }, cancellationToken: ct);

        _logger.LogInformation("Stripe: Created subscription {SubId} (${Amount}/mo)", subscription.Id, monthlyAmount);
        return subscription;
    }

    /// <summary>
    /// Creates a Stripe Payment Link for an approved expense.
    /// This is the "agent buys what it needs" moment.
    /// </summary>
    public async Task<PaymentLink> CreatePaymentLinkAsync(
        string description, long amountCents, string currency = "usd", CancellationToken ct = default)
    {
        _logger.LogInformation("Stripe: Creating payment link for {Description} (${Amount})", description, amountCents / 100m);

        // Create a product for this expense
        var productService = new ProductService();
        var product = await productService.CreateAsync(new ProductCreateOptions
        {
            Name = description,
            Metadata = new Dictionary<string, string>
            {
                ["source"] = "agentcfo-expense",
                ["type"] = "one-time"
            }
        }, cancellationToken: ct);

        // Create a price
        var priceService = new PriceService();
        var price = await priceService.CreateAsync(new PriceCreateOptions
        {
            Product = product.Id,
            UnitAmount = amountCents,
            Currency = currency,
        }, cancellationToken: ct);

        // Create payment link
        var paymentLinkService = new PaymentLinkService();
        var paymentLink = await paymentLinkService.CreateAsync(new PaymentLinkCreateOptions
        {
            LineItems = new List<PaymentLinkLineItemOptions>
            {
                new()
                {
                    Price = price.Id,
                    Quantity = 1,
                }
            },
            Metadata = new Dictionary<string, string>
            {
                ["source"] = "agentcfo-expense",
                ["description"] = description
            }
        }, cancellationToken: ct);

        _logger.LogInformation("Stripe: Created payment link {Url} for {Description}", paymentLink.Url, description);
        return paymentLink;
    }

    /// <summary>
    /// Lists recent Stripe test transactions for verification.
    /// </summary>
    public async Task<List<PaymentIntent>> GetRecentPaymentsAsync(int limit = 10, CancellationToken ct = default)
    {
        var service = new PaymentIntentService();
        var result = await service.ListAsync(new PaymentIntentListOptions
        {
            Limit = limit,
        }, cancellationToken: ct);

        return result.Data.ToList();
    }
}
