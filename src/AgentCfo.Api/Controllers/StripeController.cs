using AgentCfo.Core.Interfaces;
using AgentCfo.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgentCfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StripeController : ControllerBase
{
    private readonly StripeIntegrationService _stripeService;
    private readonly IOrganizationRepository _orgRepo;
    private readonly ILogger<StripeController> _logger;

    public StripeController(
        StripeIntegrationService stripeService,
        IOrganizationRepository orgRepo,
        ILogger<StripeController> logger)
    {
        _stripeService = stripeService;
        _orgRepo = orgRepo;
        _logger = logger;
    }

    /// <summary>
    /// Creates a Stripe customer for the demo organization.
    /// </summary>
    [HttpPost("{organizationId:guid}/create-customer")]
    public async Task<IActionResult> CreateCustomer(Guid organizationId, CancellationToken ct)
    {
        var org = await _orgRepo.GetByIdAsync(organizationId, ct);
        if (org is null) return NotFound("Organization not found");

        try
        {
            var customerId = await _stripeService.CreateCustomerAsync(org, ct);
            return Ok(new
            {
                CustomerId = customerId,
                Message = $"Stripe customer created for {org.Name}",
                DashboardUrl = $"https://dashboard.stripe.com/test/customers/{customerId}"
            });
        }
        catch (Stripe.StripeException ex)
        {
            _logger.LogError(ex, "Stripe customer creation failed");
            return StatusCode(502, new { error = "Stripe API error", detail = ex.Message });
        }
    }

    /// <summary>
    /// Creates a demo subscription for the organization.
    /// </summary>
    [HttpPost("{organizationId:guid}/create-subscription")]
    public async Task<IActionResult> CreateSubscription(Guid organizationId, CancellationToken ct)
    {
        var org = await _orgRepo.GetByIdAsync(organizationId, ct);
        if (org is null) return NotFound("Organization not found");

        try
        {
            var customerId = org.StripeCustomerId;
            if (string.IsNullOrEmpty(customerId) || customerId.StartsWith("cus_demo_"))
            {
                customerId = await _stripeService.CreateCustomerAsync(org, ct);
            }

            var subscription = await _stripeService.CreateDemoSubscriptionAsync(customerId, 22000, ct);
            return Ok(new
            {
                SubscriptionId = subscription.Id,
                Status = subscription.Status,
                Message = "Demo subscription created — $22,000/month",
            });
        }
        catch (Stripe.StripeException ex)
        {
            _logger.LogError(ex, "Stripe subscription creation failed");
            return StatusCode(502, new { error = "Stripe API error", detail = ex.Message });
        }
    }

    /// <summary>
    /// Creates a payment link for an approved expense.
    /// </summary>
    [HttpPost("create-payment-link")]
    public async Task<IActionResult> CreatePaymentLink([FromBody] PaymentLinkRequest request, CancellationToken ct)
    {
        try
        {
            var paymentLink = await _stripeService.CreatePaymentLinkAsync(
                request.Description,
                (long)(request.Amount * 100),
                request.Currency ?? "usd",
                ct);

            return Ok(new
            {
                Url = paymentLink.Url,
                Id = paymentLink.Id,
                Message = $"Payment link created for {request.Description}",
            });
        }
        catch (Stripe.StripeException ex)
        {
            _logger.LogError(ex, "Stripe payment link creation failed");
            return StatusCode(502, new { error = "Stripe API error", detail = ex.Message });
        }
    }

    /// <summary>
    /// Lists recent payments from Stripe.
    /// </summary>
    [HttpGet("payments")]
    public async Task<IActionResult> GetRecentPayments(CancellationToken ct, [FromQuery] int limit = 10)
    {
        try
        {
            var payments = await _stripeService.GetRecentPaymentsAsync(limit, ct);
            return Ok(payments.Select(p => new
            {
                p.Id,
                Amount = p.Amount / 100m,
                Currency = p.Currency,
                Status = p.Status,
                Description = p.Description,
                Created = p.Created
            }));
        }
        catch (Stripe.StripeException ex)
        {
            _logger.LogError(ex, "Stripe payments fetch failed");
            return StatusCode(502, new { error = "Stripe API error", detail = ex.Message });
        }
    }
}

public record PaymentLinkRequest(decimal Amount, string Description, string? Currency = null);
