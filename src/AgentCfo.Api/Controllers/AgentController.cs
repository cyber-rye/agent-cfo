using AgentCfo.Core.Common;
using AgentCfo.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgentCfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly IAgentService _agentService;
    private readonly IAgentDecisionRepository _decisionRepo;
    private readonly IAuditEntryRepository _auditRepo;
    private readonly ITransactionRepository _transactionRepo;

    public AgentController(
        IAgentService agentService,
        IAgentDecisionRepository decisionRepo,
        IAuditEntryRepository auditRepo,
        ITransactionRepository transactionRepo)
    {
        _agentService = agentService;
        _decisionRepo = decisionRepo;
        _auditRepo = auditRepo;
        _transactionRepo = transactionRepo;
    }

    // --- Read endpoints ---

    [HttpGet("{organizationId:guid}/decisions")]
    public async Task<IActionResult> GetDecisions(Guid organizationId, [FromQuery] int limit = 20)
    {
        var decisions = await _decisionRepo.GetRecentAsync(organizationId, limit);
        return Ok(decisions.Select(d => new
        {
            d.Id,
            Type = d.Type.ToString(),
            d.Description,
            d.Reasoning,
            Status = d.Status.ToString(),
            d.CreatedAt,
            d.ExecutedAt
        }));
    }

    [HttpGet("{organizationId:guid}/audit")]
    public async Task<IActionResult> GetAuditTrail(Guid organizationId, [FromQuery] int limit = 50)
    {
        var entries = await _auditRepo.GetByOrganizationAsync(organizationId, limit);
        return Ok(entries.Select(a => new
        {
            a.Id,
            Actor = a.ActorType.ToString(),
            a.ActorId,
            a.Action,
            a.EntityType,
            a.EntityId,
            a.CreatedAt,
            a.CorrelationId
        }));
    }

    // --- Agent action endpoints ---

    [HttpPost("{organizationId:guid}/analyze-transaction")]
    public async Task<IActionResult> AnalyzeTransaction(Guid organizationId, [FromBody] AnalyzeTransactionRequest request)
    {
        var transaction = await _transactionRepo.GetByIdAsync(request.TransactionId);
        if (transaction is null) return NotFound("Transaction not found");

        var decision = await _agentService.AnalyzeTransactionAsync(transaction);
        return Ok(new
        {
            decision.Id,
            Type = decision.Type.ToString(),
            decision.Description,
            decision.Reasoning,
            Status = decision.Status.ToString(),
            decision.CreatedAt
        });
    }

    [HttpPost("{organizationId:guid}/evaluate-expense")]
    public async Task<IActionResult> EvaluateExpense(Guid organizationId, [FromBody] EvaluateExpenseRequest request)
    {
        var amount = Money.From(request.Amount, request.Currency ?? "USD");
        var decision = await _agentService.EvaluateExpenseRequestAsync(organizationId, amount, request.Description);
        return Ok(new
        {
            decision.Id,
            Type = decision.Type.ToString(),
            decision.Description,
            decision.Reasoning,
            Status = decision.Status.ToString(),
            decision.CreatedAt
        });
    }

    [HttpPost("{organizationId:guid}/detect-anomalies")]
    public async Task<IActionResult> DetectAnomalies(Guid organizationId)
    {
        var decision = await _agentService.DetectAnomalyAsync(organizationId);
        return Ok(new
        {
            decision.Id,
            Type = decision.Type.ToString(),
            decision.Description,
            decision.Reasoning,
            Status = decision.Status.ToString(),
            decision.CreatedAt
        });
    }

    [HttpPost("{organizationId:guid}/generate-forecast")]
    public async Task<IActionResult> GenerateForecast(Guid organizationId, [FromQuery] string? scenario = null)
    {
        var forecast = await _agentService.GenerateForecastAsync(organizationId, scenario);
        return Ok(new
        {
            forecast.Id,
            CashBalance = forecast.CurrentCashBalance.Amount,
            Currency = forecast.CurrentCashBalance.Currency,
            BurnRate = forecast.MonthlyBurnRate.Amount,
            Revenue = forecast.MonthlyRevenue.Amount,
            forecast.RunwayDays,
            forecast.RunwayEndDate,
            forecast.Scenario,
            forecast.Confidence,
            Projections = forecast.ProjectionPoints.Select(p => new
            {
                p.Date,
                Balance = p.ProjectedBalance.Amount,
                Revenue = p.ProjectedRevenue.Amount,
                Expenses = p.ProjectedExpenses.Amount
            })
        });
    }

    [HttpPost("{organizationId:guid}/generate-summary")]
    public async Task<IActionResult> GenerateSummary(Guid organizationId)
    {
        var summary = await _agentService.GenerateFinancialSummaryAsync(organizationId);
        return Ok(new { Summary = summary });
    }

    [HttpPost("{organizationId:guid}/run-full-analysis")]
    public async Task<IActionResult> RunFullAnalysis(Guid organizationId)
    {
        // Run all agent analyses in sequence — this is the "agent wakes up" flow
        var results = new List<object>();

        // 1. Detect anomalies
        var anomalyDecision = await _agentService.DetectAnomalyAsync(organizationId);
        results.Add(new { Step = "AnomalyDetection", anomalyDecision.Description, anomalyDecision.Reasoning });

        // 2. Generate forecast
        var forecast = await _agentService.GenerateForecastAsync(organizationId);
        results.Add(new { Step = "Forecast", Description = $"Runway: {forecast.RunwayDays} days", forecast.Scenario });

        // 3. Generate summary
        var summary = await _agentService.GenerateFinancialSummaryAsync(organizationId);
        results.Add(new { Step = "Summary", Description = "Financial summary generated" });

        return Ok(new
        {
            StepsCompleted = results.Count,
            Results = results,
            Message = "Full analysis complete. Check /decisions for detailed reasoning."
        });
    }
}

public record AnalyzeTransactionRequest(Guid TransactionId);
public record EvaluateExpenseRequest(decimal Amount, string Description, string? Currency = null);
