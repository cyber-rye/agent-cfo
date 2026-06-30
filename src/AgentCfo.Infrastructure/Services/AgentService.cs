using AgentCfo.Core.Common;
using AgentCfo.Core.Entities;
using AgentCfo.Core.Enums;
using AgentCfo.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AgentCfo.Infrastructure.Services;

/// <summary>
/// Agent service that analyzes financial data and generates decisions.
/// Uses actual financial data to produce reasoning-backed decisions.
/// In production, this would call an LLM; for the hackathon demo,
/// it uses deterministic analysis that produces compelling output.
/// </summary>
public class AgentService : IAgentService
{
    private readonly ITransactionRepository _transactionRepo;
    private readonly IBudgetRepository _budgetRepo;
    private readonly IForecastService _forecastService;
    private readonly IRevenueMetricsService _revenueMetrics;
    private readonly IAgentDecisionRepository _decisionRepo;
    private readonly IOrganizationRepository _orgRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AgentService> _logger;

    public AgentService(
        ITransactionRepository transactionRepo,
        IBudgetRepository budgetRepo,
        IForecastService forecastService,
        IRevenueMetricsService revenueMetrics,
        IAgentDecisionRepository decisionRepo,
        IOrganizationRepository orgRepo,
        IUnitOfWork unitOfWork,
        ILogger<AgentService> logger)
    {
        _transactionRepo = transactionRepo;
        _budgetRepo = budgetRepo;
        _forecastService = forecastService;
        _revenueMetrics = revenueMetrics;
        _decisionRepo = decisionRepo;
        _orgRepo = orgRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AgentDecision> AnalyzeTransactionAsync(Transaction transaction, CancellationToken ct = default)
    {
        _logger.LogInformation("Agent: Analyzing transaction {TransactionId} - {Amount} {Description}",
            transaction.Id, transaction.Amount, transaction.Description);

        var budgets = await _budgetRepo.GetByOrganizationAsync(transaction.OrganizationId, ct);
        var relevantBudget = budgets.FirstOrDefault(b => b.Category == transaction.Category);

        // Determine if this transaction warrants a decision
        AgentDecision decision;

        if (transaction.Type == TransactionType.Revenue)
        {
            decision = AgentDecision.Propose(
                transaction.OrganizationId,
                AgentDecisionType.ForecastUpdated,
                $"Revenue event recorded: {transaction.Amount}",
                $"New {transaction.Type} of {transaction.Amount} received. " +
                $"Description: {transaction.Description}. " +
                $"This transaction has been categorized as {transaction.Category}. " +
                $"Revenue metrics will be updated on the next forecast cycle.",
                transaction.Id);
        }
        else if (transaction.Type == TransactionType.Expense)
        {
            // Check against budget
            if (relevantBudget is not null && relevantBudget.HardLimit && !relevantBudget.CanSpend(transaction.Amount))
            {
                decision = AgentDecision.Propose(
                    transaction.OrganizationId,
                    AgentDecisionType.ExpenseDenied,
                    $"Expense exceeds {transaction.Category} budget",
                    $"Evaluating expense: {transaction.Amount} for '{transaction.Description}'. " +
                    $"{transaction.Category} budget: {relevantBudget.MonthlyLimit}, " +
                    $"current spend: {relevantBudget.CurrentSpend} ({relevantBudget.PercentUsed:F0}% used). " +
                    $"This expense would exceed the hard limit. Recommend: DENY. " +
                    $"Suggestion: Review if this expense can be deferred to next period or renegotiated.",
                    transaction.Id,
                    relevantBudget.Id);
            }
            else if (relevantBudget is not null && relevantBudget.IsNearLimit)
            {
                decision = AgentDecision.Propose(
                    transaction.OrganizationId,
                    AgentDecisionType.ExpenseApproved,
                    $"Expense approved with budget warning for {transaction.Category}",
                    $"Evaluating expense: {transaction.Amount} for '{transaction.Description}'. " +
                    $"{transaction.Category} budget: {relevantBudget.MonthlyLimit}, " +
                    $"current spend: {relevantBudget.CurrentSpend} ({relevantBudget.PercentUsed:F0}% used). " +
                    $"APPROVED, but note: this category is approaching its limit. " +
                    $"Remaining budget after this expense: {relevantBudget.Remaining.Subtract(transaction.Amount)}. " +
                    $"Recommend reviewing upcoming {transaction.Category} expenses.",
                    transaction.Id,
                    relevantBudget.Id);
            }
            else
            {
                decision = AgentDecision.Propose(
                    transaction.OrganizationId,
                    AgentDecisionType.ExpenseApproved,
                    $"Expense approved: {transaction.Description}",
                    $"Evaluating expense: {transaction.Amount} for '{transaction.Description}'. " +
                    (relevantBudget is not null
                        ? $"{transaction.Category} budget: {relevantBudget.MonthlyLimit}, " +
                          $"current utilization: {relevantBudget.PercentUsed:F0}%. " +
                          $"Well within limits. APPROVED."
                        : $"No specific budget for {transaction.Category}. APPROVED under general policy."),
                    transaction.Id);
            }
        }
        else
        {
            decision = AgentDecision.Propose(
                transaction.OrganizationId,
                AgentDecisionType.ForecastUpdated,
                $"Transaction processed: {transaction.Type}",
                $"Transaction of type {transaction.Type} for {transaction.Amount} processed. " +
                $"No action required beyond recording.",
                transaction.Id);
        }

        await _decisionRepo.AddAsync(decision, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Agent: Decision made - {Type}: {Description}", decision.Type, decision.Description);
        return decision;
    }

    public async Task<AgentDecision> EvaluateExpenseRequestAsync(Guid organizationId, Money amount, string description, CancellationToken ct = default)
    {
        _logger.LogInformation("Agent: Evaluating expense request - {Amount} for {Description}", amount, description);

        var budgets = await _budgetRepo.GetByOrganizationAsync(organizationId, ct);
        var metrics = await _revenueMetrics.GetMetricsAsync(organizationId, ct);

        var totalBudget = budgets.Sum(b => b.MonthlyLimit.Amount);
        var totalSpend = budgets.Sum(b => b.CurrentSpend.Amount);
        var expenseRatio = totalBudget > 0 ? (amount.Amount / totalBudget * 100) : 0;

        // Check per-category budget — infer category from description
        var descLower = description.ToLower();
        Budget? matchingBudget = null;
        foreach (var budget in budgets)
        {
            var cat = budget.Category.ToString().ToLower();
            if (descLower.Contains(cat) ||
                (cat == "marketing" && (descLower.Contains("ads") || descLower.Contains("facebook") || descLower.Contains("google") || descLower.Contains("linkedin") || descLower.Contains("campaign"))) ||
                (cat == "infrastructure" && (descLower.Contains("aws") || descLower.Contains("cloud") || descLower.Contains("server") || descLower.Contains("hosting"))) ||
                (cat == "tools" && (descLower.Contains("software") || descLower.Contains("saas") || descLower.Contains("license") || descLower.Contains("subscription") || descLower.Contains("figma") || descLower.Contains("slack"))) ||
                (cat == "contractors" && (descLower.Contains("freelance") || descLower.Contains("agency") || descLower.Contains("contractor"))) ||
                (cat == "office" && (descLower.Contains("rent") || descLower.Contains("coworking") || descLower.Contains("office"))))
            {
                matchingBudget = budget;
                break;
            }
        }

        // Deny if: >20% of total budget OR would exceed category budget
        bool wouldExceedCategory = matchingBudget is not null &&
            (matchingBudget.CurrentSpend.Add(amount)).IsGreaterThan(matchingBudget.MonthlyLimit);

        AgentDecision decision;

        if (expenseRatio > 20 || wouldExceedCategory)
        {
            var reason = expenseRatio > 20
                ? $"This represents {expenseRatio:F0}% of total monthly budget ({Money.From(totalBudget, amount.Currency)})."
                : $"Would exceed {matchingBudget!.Category} budget: current spend {matchingBudget.CurrentSpend} + {amount} > limit {matchingBudget.MonthlyLimit}.";

            decision = AgentDecision.Propose(
                organizationId,
                AgentDecisionType.ExpenseDenied,
                $"Expense request denied: {amount} for {description}",
                $"Evaluating expense request: {amount} for '{description}'. " +
                $"{reason} " +
                $"Current total spend: {Money.From(totalSpend, amount.Currency)}. " +
                $"Monthly recurring revenue: {metrics?.MonthlyRecurringRevenue ?? Money.Zero()}. " +
                $"DENY — budget constraint violated. " +
                $"Recommend: Reduce scope, negotiate pricing, or wait until next budget cycle.");
        }
        else
        {
            var categoryNote = matchingBudget is not null
                ? $"{matchingBudget.Category} budget impact: {matchingBudget.CurrentSpend.Add(amount)} / {matchingBudget.MonthlyLimit}."
                : "No specific category budget matched — within total budget.";

            decision = AgentDecision.Propose(
                organizationId,
                AgentDecisionType.ExpenseApproved,
                $"Expense request approved: {description}",
                $"Evaluating expense request: {amount} for '{description}'. " +
                $"Budget impact: {expenseRatio:F1}% of total monthly budget. " +
                $"{categoryNote} " +
                $"MRR: {metrics?.MonthlyRecurringRevenue ?? Money.Zero()}. " +
                $"APPROVED — within budget constraints.");
        }

        await _decisionRepo.AddAsync(decision, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return decision;
    }

    public async Task<Forecast> GenerateForecastAsync(Guid organizationId, string? scenario = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Agent: Generating forecast for {OrgId} (scenario: {Scenario})", organizationId, scenario ?? "base");

        var forecast = await _forecastService.GenerateForecastAsync(organizationId, scenario, ct);
        if (forecast is null)
            throw new InvalidOperationException($"Could not generate forecast for organization {organizationId}");

        // Log the forecast as an agent decision
        var decision = AgentDecision.Propose(
            organizationId,
            AgentDecisionType.ForecastUpdated,
            $"Forecast generated: {forecast.RunwayDays} days runway",
            $"Cash flow forecast generated. " +
            $"Current balance: {forecast.CurrentCashBalance}. " +
            $"Monthly burn: {forecast.MonthlyBurnRate}. " +
            $"Monthly revenue: {forecast.MonthlyRevenue}. " +
            $"Runway: {forecast.RunwayDays} days (ends {forecast.RunwayEndDate}). " +
            $"Scenario: {forecast.Scenario}. " +
            $"Confidence: {forecast.Confidence}. " +
            (forecast.RunwayDays < 180
                ? "WARNING: Runway below 6 months. Recommend reviewing burn rate or accelerating fundraising."
                : "Runway is healthy. Continue monitoring monthly."));

        await _decisionRepo.AddAsync(decision, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return forecast;
    }

    public async Task<AgentDecision> DetectAnomalyAsync(Guid organizationId, CancellationToken ct = default)
    {
        _logger.LogInformation("Agent: Running anomaly detection for {OrgId}", organizationId);

        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var previousMonthStart = currentMonthStart.AddMonths(-1);

        var currentExpenses = await _transactionRepo.GetTotalExpensesAsync(organizationId, currentMonthStart, now, ct);
        var previousExpenses = await _transactionRepo.GetTotalExpensesAsync(organizationId, previousMonthStart, currentMonthStart, ct);

        var budgets = await _budgetRepo.GetByOrganizationAsync(organizationId, ct);

        // Check for anomalies
        var anomalies = new List<string>();

        // Check month-over-month expense growth
        if (!previousExpenses.IsZero)
        {
            var growthRate = ((currentExpenses.Amount - previousExpenses.Amount) / previousExpenses.Amount) * 100;
            if (growthRate > 15)
            {
                anomalies.Add($"Expenses grew {growthRate:F0}% month-over-month ({previousExpenses} → {currentExpenses})");
            }
        }

        // Check budget utilization
        foreach (var budget in budgets.Where(b => b.IsNearLimit))
        {
            anomalies.Add($"{budget.Category} budget at {budget.PercentUsed:F0}% (threshold: {budget.AlertThresholdPercent}%)");
        }

        // Check for over-budget categories
        foreach (var budget in budgets.Where(b => b.IsOverBudget))
        {
            anomalies.Add($"{budget.Category} OVER BUDGET by {budget.CurrentSpend.Subtract(budget.MonthlyLimit)}");
        }

        AgentDecision decision;

        if (anomalies.Any())
        {
            decision = AgentDecision.Propose(
                organizationId,
                AgentDecisionType.AnomalyDetected,
                $"Anomaly detected: {anomalies.Count} issue(s) found",
                $"Anomaly detection scan completed. Issues found:\n" +
                string.Join("\n", anomalies.Select((a, i) => $"  {i + 1}. {a}")) +
                $"\n\nRecommendation: Review affected categories and consider adjusting budgets or spending patterns. " +
                $"Current monthly expenses: {currentExpenses}. " +
                $"Previous month: {previousExpenses}.");
        }
        else
        {
            decision = AgentDecision.Propose(
                organizationId,
                AgentDecisionType.ReportGenerated,
                "Anomaly detection: all clear",
                $"Anomaly detection scan completed. No anomalies detected. " +
                $"Current month expenses: {currentExpenses}. " +
                $"Previous month: {previousExpenses}. " +
                $"All budgets within normal parameters. No action required.");
        }

        await _decisionRepo.AddAsync(decision, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return decision;
    }

    public async Task<string> GenerateFinancialSummaryAsync(Guid organizationId, CancellationToken ct = default)
    {
        _logger.LogInformation("Agent: Generating financial summary for {OrgId}", organizationId);

        var org = await _orgRepo.GetByIdAsync(organizationId, ct);
        var metrics = await _revenueMetrics.GetMetricsAsync(organizationId, ct);
        var forecast = await _forecastService.GenerateForecastAsync(organizationId, null, ct);
        var budgets = await _budgetRepo.GetByOrganizationAsync(organizationId, ct);
        var recentDecisions = await _decisionRepo.GetRecentAsync(organizationId, 5, ct);

        var summary = $"""
            # Financial Summary — {org?.Name ?? "Unknown Organization"}
            Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC

            ## Revenue Metrics
            - Monthly Recurring Revenue: {metrics?.MonthlyRecurringRevenue}
            - Previous Month Revenue: {metrics?.PreviousMonthRevenue}
            - Growth Rate: {metrics?.GrowthRatePercent:F1}%
            - Active Subscriptions: {metrics?.ActiveSubscriptions}
            - Average Revenue Per User: {metrics?.AverageRevenuePerUser}

            ## Cash Position
            - Current Balance: {forecast?.CurrentCashBalance}
            - Monthly Burn Rate: {forecast?.MonthlyBurnRate}
            - Monthly Revenue: {forecast?.MonthlyRevenue}
            - Runway: {forecast?.RunwayDays} days (ends {forecast?.RunwayEndDate})
            - Confidence: {forecast?.Confidence}

            ## Budget Status
            {string.Join("\n", budgets.Select(b => $"- {b.Category}: {b.CurrentSpend}/{b.MonthlyLimit} ({b.PercentUsed:F0}%)"))}

            ## Recent Agent Activity
            {string.Join("\n", recentDecisions.Take(3).Select(d => $"- [{d.Type}] {d.Description}"))}

            ## Assessment
            {(forecast?.RunwayDays > 180
                ? "✅ Runway is healthy. Company is in a strong financial position."
                : forecast?.RunwayDays > 90
                    ? "⚠️ Runway is adequate but requires monitoring. Consider reducing burn rate."
                    : "🚨 Runway is critical. Immediate action required: cut expenses or accelerate fundraising.")}
            """;

        // Log as an agent decision
        var decision = AgentDecision.Propose(
            organizationId,
            AgentDecisionType.ReportGenerated,
            "Monthly financial summary generated",
            $"Comprehensive financial summary generated for {org?.Name}. " +
            $"MRR: {metrics?.MonthlyRecurringRevenue} ({metrics?.GrowthRatePercent:F1}% growth). " +
            $"Runway: {forecast?.RunwayDays} days. " +
            $"Key finding: {(forecast?.RunwayDays > 180 ? "Financial position is strong." : "Runway needs attention.")}");

        await _decisionRepo.AddAsync(decision, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return summary;
    }
}
