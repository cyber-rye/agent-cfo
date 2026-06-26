using AgentCfo.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AgentCfo.Api.Controllers;

[ApiController]
[Route("api/webhooks/stripe")]
public class StripeWebhookController : ControllerBase
{
    private readonly StripeWebhookService _webhookService;
    private readonly StripeOptions _options;
    private readonly ILogger<StripeWebhookController> _logger;
    
    public StripeWebhookController(
        StripeWebhookService webhookService,
        IOptions<StripeOptions> options,
        ILogger<StripeWebhookController> logger)
    {
        _webhookService = webhookService;
        _options = options.Value;
        _logger = logger;
    }
    
    [HttpPost]
    public async Task<IActionResult> HandleWebhook(CancellationToken ct)
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(ct);
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Missing Stripe-Signature header");
            return BadRequest();
        }
        
        try
        {
            await _webhookService.ProcessWebhookAsync(json, signature, _options.WebhookSecret, ct);
            return Ok();
        }
        catch (Stripe.StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook processing failed");
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class StripeOptions
{
    public string WebhookSecret { get; set; } = string.Empty;
}
