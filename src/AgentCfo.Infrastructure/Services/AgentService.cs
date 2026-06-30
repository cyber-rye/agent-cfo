using AgentCfo.Core.Common;
using AgentCfo.Core.Entities;
using AgentCfo.Core.Enums;
using AgentCfo.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AgentCfo.Infrastructure.Services;

/// <summary>
/// Agent service that analyzes financial data and generates decisions.
/// Uses Nemotron 3 Ultra (via OpenRouter) for natural-language reasoning,
/// with deterministic fallback when LLM is unavailable.
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
    private readonly ILlmService _llm;
    private readonly ILogger<AgentService> _logger;

    private const string CFO_SYSTEM_PROMPT = """
        You are AgentCFO, an autonomous financial operations agent for a startup.
        You analyze financial data, detect anomalies, evaluate expenses, and generate reports.
        Be specific — reference actual numbers, percentages, and trends.
        Be concise — 2-4 sentences max for evaluations, longer for summaries.
        Be direct — state your recommendation clearly (APPROVED/DENY/WARNING/OK).
        Format currency as $X,XXX. Use plain text, no markdown headers.
        """;

    public AgentService(
        ITransactionRepository transactionRepo,
        IBudgetRepository budgetRepo,
        IForecastService forecastService,
        IRevenueMetricsService revenueMetrics,
        IAgentDecisionRepository decisionRepo,
        IOrganizationRepository orgRepo,
        IUnitOfWork unitOfWork,
        ILlmService llm,
        ILogger<AgentService> logger)
    {
        _transactionRepo = transactionRepo;
        _budgetRepo = budgetRepo;
        _forecastService = forecastService;
        _revenueMetrics = revenueMetrics;
        _decisionRepo = decisionRepo;
        _orgRepo = orgRepo;
        _unitOfWork = unitOfWork;
        _llm = llm;
        _logger = logger;
    }

    public async Task<AgentDecision> AnalyzeTransactionAsync(Transaction transaction, CancellationToken ct = default)
    {
        _logger.LogInformation("Agent: Analyzing transaction {TransactionId} - {Amount} {Description}",
            transaction.Id, transaction.Amount, transaction.Description);

        var budgets = await _budgetRepo.GetByOrganizationAsync(transaction.OrganizationId, ct);
        var relevantBudget = budgets.FirstOrDefault(b => b.Category == transaction.Category);

        // Build context for LLM
        var budgetContext = relevantBudget is not null
            ? $"Category budget: {relevantBudget.MonthlyLimit}, current spend: {relevantBudget.CurrentSpend} ({relevantBudget.PercentUsed:F0}% used), hard limit: {relevantBudget.HardLimit}."
            : $"No specific budget for {transaction.Category}.";

        var userPrompt = $"""
            Transaction: {transaction.Type} of {transaction.Amount} for '{transaction.Description}'
            Category: {transaction.Category}
            {budgetContext}
            
            Evaluate this transaction. Should it be approved or flagged? Explain briefly.
            """;

        // Try LLM first, fall back to deterministic
        var reasoning = await _llm.TryCompleteAsync(CFO_SYSTEM_PROMPT, userPrompt, ct);

        AgentDecision decision;
        if (transaction.Type == TransactionType.Revenue)
        {
            var llmReasoning = reasoning ?? $"New {transaction.Type} of {transaction.Amount} received. Description: {transaction.Description}. This transaction has been categorized as {transaction.Category}. Revenue metrics will be updated on the next forecast cycle.";
            decision = AgentDecision.Propose(
                transaction.OrganizationId,
                AgentDecisionType.ForecastUpdated,
                $"Revenue event recorded: {transaction.Amount}",
                llmReasoning,
                transaction.Id);
        }
        else if (transaction.Type == TransactionType.Expense)
        {
            if (relevantBudget is not null && relevantBudget.HardLimit && !relevantBudget.CanSpend(transaction.Amount))
            {
                var llmReasoning = reasoning ?? $"Evaluating expense: {transaction.Amount} for '{transaction.Description}'. {transaction.Category} budget: {relevantBudget.MonthlyLimit}, current spend: {relevantBudget.CurrentSpend} ({relevantBudget.PercentUsed:F0}% used). This expense would exceed the hard limit. Recommend: DENY.";
                decision = AgentDecision.Propose(
                    transaction.OrganizationId,
                    AgentDecisionType.ExpenseDenied,
                    $"Expense exceeds {transaction.Category} budget",
                    llmReasoning,
                    transaction.Id,
                    relevantBudget.Id);
            }
            else if (relevantBudget is not null && relevantBudget.IsNearLimit)
            {
                var llmReasoning = reasoning ?? $"Evaluating expense: {transaction.Amount} for '{transaction.Description}'. {transaction.Category} budget: {relevantBudget.MonthlyLimit}, current spend: {relevantBudget.CurrentSpend} ({relevantBudget.PercentUsed:F0}% used). APPROVED, but note: this category is approaching its limit. Remaining budget after this expense: {relevantBudget.Remaining.Subtract(transaction.Amount)}.";
                decision = AgentDecision.Propose(
                    transaction.OrganizationId,
                    AgentDecisionType.ExpenseApproved,
                    $"Expense approved with budget warning for {transaction.Category}",
                    llmReasoning,
                    transaction.Id,
                    relevantBudget.Id);
            }
            else
            {
                var llmReasoning = reasoning ?? $"Evaluating expense: {transaction.Amount} for '{transaction.Description}'. " +
                    (relevantBudget is not null
                        ? $"{transaction.Category} budget: {relevantBudget.MonthlyLimit}, current utilization: {relevantBudget.PercentUsed:F0}%. Well within limits. APPROVED."
                        : $"No specific budget for {transaction.Category}. APPROVED under general policy.");
                decision = AgentDecision.Propose(
                    transaction.OrganizationId,
                    AgentDecisionType.ExpenseApproved,
                    $"Expense approved: {transaction.Description}",
                    llmReasoning,
                    transaction.Id);
            }
        }
        else
        {
            decision = AgentDecision.Propose(
                transaction.OrganizationId,
                AgentDecisionType.ForecastUpdated,
                $"Transaction processed: {transaction.Type}",
                reasoning ?? $"Transaction of type {transaction.Type} for {transaction.Amount} processed. No action required beyond recording.",
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

        // Find matching budget category
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

        bool wouldExceedCategory = matchingBudget is not null &&
            (matchingBudget.CurrentSpend.Add(amount)).IsGreaterThan(matchingBudget.MonthlyLimit);
        bool isDenied = expenseRatio > 20 || wouldExceedCategory;

        // Build rich context for LLM — include the DECISION so reasoning aligns
        var budgetSummary = string.Join("\n", budgets.Select(b =>
            $"- {b.Category}: {b.CurrentSpend}/{b.MonthlyLimit} ({b.PercentUsed:F0}% used) {(b.HardLimit ? "[HARD LIMIT]" : "[soft]")}"));

        var denialReason = isDenied
            ? expenseRatio > 20
                ? $"This expense is {expenseRatio:F0}% of total monthly budget, exceeding the 20% threshold."
                : $"Would exceed {matchingBudget!.Category} budget: {matchingBudget.CurrentSpend} + {amount} > {matchingBudget.MonthlyLimit}."
            : "";

        var categoryNote = matchingBudget is not null
            ? $"{matchingBudget.Category} budget after expense: {matchingBudget.CurrentSpend.Add(amount)} / {matchingBudget.MonthlyLimit}."
            : "No specific category budget matched.";

        var userPrompt = $"""
            Expense request: {amount} for '{description}'
            Decision: {(isDenied ? "DENIED" : "APPROVED")}
            Reason: {denialReason}
            
            Financial context:
            - MRR: {metrics?.MonthlyRecurringRevenue ?? Money.Zero()}
            - Total monthly budget: {Money.From(totalBudget, amount.Currency)}
            - Total current spend: {Money.From(totalSpend, amount.Currency)}
            - This expense as % of total budget: {expenseRatio:F1}%
            - {categoryNote}
            
            Budget breakdown:
            {budgetSummary}
            
            Explain this decision in 2-3 sentences. Start with "APPROVED" or "DENY" and explain why.
            """;

        var reasoning = await _llm.TryCompleteAsync(CFO_SYSTEM_PROMPT, userPrompt, ct);

        AgentDecision decision;
        if (isDenied)
        {
            var reason = expenseRatio > 20
                ? $"This represents {expenseRatio:F0}% of total monthly budget ({Money.From(totalBudget, amount.Currency)})."
                : $"Would exceed {matchingBudget!.Category} budget: current spend {matchingBudget.CurrentSpend} + {amount} > limit {matchingBudget.MonthlyLimit}.";

            var llmReasoning = reasoning ?? $"Evaluating expense request: {amount} for '{description}'. {reason} Current total spend: {Money.From(totalSpend, amount.Currency)}. Monthly recurring revenue: {metrics?.MonthlyRecurringRevenue ?? Money.Zero()}. DENY — budget constraint violated. Recommend: Reduce scope, negotiate pricing, or wait until next budget cycle.";

            decision = AgentDecision.Propose(
                organizationId,
                AgentDecisionType.ExpenseDenied,
                $"Expense request denied: {amount} for {description}",
                llmReasoning);
        }
        else
        {
            var llmReasoning = reasoning ?? $"Evaluating expense request: {amount} for '{description}'. Budget impact: {expenseRatio:F1}% of total monthly budget. {categoryNote} MRR: {metrics?.MonthlyRecurringRevenue ?? Money.Zero()}. APPROVED — within budget constraints.";

            decision = AgentDecision.Propose(
                organizationId,
                AgentDecisionType.ExpenseApproved,
                $"Expense request approved: {description}",
                llmReasoning);
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

        // Collect anomalies
        var anomalies = new List<string>();
        if (!previousExpenses.IsZero)
        {
            var growthRate = ((currentExpenses.Amount - previousExpenses.Amount) / previousExpenses.Amount) * 100;
            if (growthRate > 15)
                anomalies.Add($"Expenses grew {growthRate:F0}% month-over-month ({previousExpenses} → {currentExpenses})");
        }
        foreach (var budget in budgets.Where(b => b.IsNearLimit))
            anomalies.Add($"{budget.Category} budget at {budget.PercentUsed:F0}% (threshold: {budget.AlertThresholdPercent}%)");
        foreach (var budget in budgets.Where(b => b.IsOverBudget))
            anomalies.Add($"{budget.Category} OVER BUDGET by {budget.CurrentSpend.Subtract(budget.MonthlyLimit)}");

        // Build context for LLM
        var budgetBreakdown = string.Join("\n", budgets.Select(b =>
            $"- {b.Category}: {b.CurrentSpend}/{b.MonthlyLimit} ({b.PercentUsed:F0}%)"));

        var userPrompt = $"""
            Anomaly detection scan for startup financials.
            
            Current month expenses: {currentExpenses}
            Previous month expenses: {previousExpenses}
            
            Budget utilization:
            {budgetBreakdown}
            
            Issues detected: {(anomalies.Any() ? string.Join("; ", anomalies) : "none")}
            
            {"Analyze these anomalies and recommend actions. Be specific about which categories need attention."}
            """;

        var reasoning = await _llm.TryCompleteAsync(CFO_SYSTEM_PROMPT, userPrompt, ct);

        AgentDecision decision;
        if (anomalies.Any())
        {
            var templateReasoning = $"Anomaly detection scan completed. Issues found:\n" +
                string.Join("\n", anomalies.Select((a, i) => $"  {i + 1}. {a}")) +
                $"\n\nRecommendation: Review affected categories and consider adjusting budgets or spending patterns. " +
                $"Current monthly expenses: {currentExpenses}. Previous month: {previousExpenses}.";

            decision = AgentDecision.Propose(
                organizationId,
                AgentDecisionType.AnomalyDetected,
                $"Anomaly detected: {anomalies.Count} issue(s) found",
                reasoning ?? templateReasoning);
        }
        else
        {
            var templateReasoning = $"Anomaly detection scan completed. No anomalies detected. " +
                $"Current month expenses: {currentExpenses}. Previous month: {previousExpenses}. " +
                $"All budgets within normal parameters. No action required.";

            decision = AgentDecision.Propose(
                organizationId,
                AgentDecisionType.ReportGenerated,
                "Anomaly detection: all clear",
                reasoning ?? templateReasoning);
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

        // Build comprehensive context for LLM summary
        var budgetBreakdown = string.Join("\n", budgets.Select(b =>
            $"- {b.Category}: {b.CurrentSpend}/{b.MonthlyLimit} ({b.PercentUsed:F0}%)"));
        var recentActivity = string.Join("\n", recentDecisions.Take(3).Select(d =>
            $"- [{d.Type}] {d.Description}"));

        var userPrompt = $"""
            Generate a monthly financial summary for {org?.Name ?? "the company"}.
            
            Revenue:
            - MRR: {metrics?.MonthlyRecurringRevenue}
            - Previous month: {metrics?.PreviousMonthRevenue}
            - Growth rate: {metrics?.GrowthRatePercent:F1}%
            - Active subscriptions: {metrics?.ActiveSubscriptions}
            - Avg revenue per user: {metrics?.AverageRevenuePerUser}
            
            Cash position:
            - Balance: {forecast?.CurrentCashBalance}
            - Monthly burn: {forecast?.MonthlyBurnRate}
            - Monthly revenue: {forecast?.MonthlyRevenue}
            - Runway: {forecast?.RunwayDays} days
            - Confidence: {forecast?.Confidence}
            
            Budget status:
            {budgetBreakdown}
            
            Recent agent activity:
            {recentActivity}
            
            Write a concise executive summary (5-8 sentences). Include:
            1. Overall financial health assessment
            2. Key metrics and trends
            3. Areas of concern (if any)
            4. Specific recommendations
            
            Format as a markdown document with headers.
            """;

        var llmSummary = await _llm.TryCompleteAsync(CFO_SYSTEM_PROMPT, userPrompt, ct);

        var summary = llmSummary ?? $"""
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
            {budgetBreakdown}

            ## Recent Agent Activity
            {recentActivity}

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
