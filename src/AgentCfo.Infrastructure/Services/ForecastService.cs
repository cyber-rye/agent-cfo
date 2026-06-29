using AgentCfo.Core.Common;
using AgentCfo.Core.Entities;
using AgentCfo.Core.Enums;
using AgentCfo.Core.Interfaces;

namespace AgentCfo.Infrastructure.Services;

public class ForecastService : IForecastService
{
    private readonly ITransactionRepository _transactionRepo;
    private readonly IOrganizationRepository _organizationRepo;
    private readonly IAgentDecisionRepository _decisionRepo;

    public ForecastService(
        ITransactionRepository transactionRepo,
        IOrganizationRepository organizationRepo,
        IAgentDecisionRepository decisionRepo)
    {
        _transactionRepo = transactionRepo;
        _organizationRepo = organizationRepo;
        _decisionRepo = decisionRepo;
    }

    public async Task<Forecast?> GenerateForecastAsync(Guid organizationId, string? scenario = null, CancellationToken ct = default)
    {
        var org = await _organizationRepo.GetByIdAsync(organizationId, ct);
        if (org is null) return null;

        var now = DateTime.UtcNow;
        var thirtyDaysAgo = now.AddDays(-30);
        var sixtyDaysAgo = now.AddDays(-60);

        // Calculate current month metrics
        var currentMonthRevenue = await _transactionRepo.GetTotalRevenueAsync(organizationId, thirtyDaysAgo, now, ct);
        var currentMonthExpenses = await _transactionRepo.GetTotalExpensesAsync(organizationId, thirtyDaysAgo, now, ct);

        // Calculate previous month for trend
        var previousMonthRevenue = await _transactionRepo.GetTotalRevenueAsync(organizationId, sixtyDaysAgo, thirtyDaysAgo, ct);

        // All-time cash balance (revenue - expenses)
        var allTimeRevenue = await _transactionRepo.GetTotalRevenueAsync(organizationId, DateTime.MinValue, now, ct);
        var allTimeExpenses = await _transactionRepo.GetTotalExpensesAsync(organizationId, DateTime.MinValue, now, ct);
        var cashBalance = allTimeRevenue.Subtract(allTimeExpenses);

        // Apply scenario adjustments to monthly figures
        var adjustedRevenue = currentMonthRevenue;
        var adjustedExpenses = currentMonthExpenses;
        var scenarioLabel = scenario ?? "base";

        switch (scenarioLabel)
        {
            case "hire-2-engineers":
                // Two senior engineers at ~$10K/mo each
                adjustedExpenses = Money.From(currentMonthExpenses.Amount + 20_000m, currentMonthExpenses.Currency);
                break;
            case "hire-1-engineer":
                // One senior engineer
                adjustedExpenses = Money.From(currentMonthExpenses.Amount + 10_000m, currentMonthExpenses.Currency);
                break;
            case "expand-mrr-15":
                // 15% MRR growth from new customers
                adjustedRevenue = Money.From(currentMonthRevenue.Amount * 1.15m, currentMonthRevenue.Currency);
                break;
            case "cut-marketing-50":
                // Cut marketing spend in half (~$2K/mo for a startup this size)
                adjustedExpenses = Money.From(currentMonthExpenses.Amount - 2_000m, currentMonthExpenses.Currency);
                break;
        }

        // Growth-adjusted revenue projection
        var growthRate = previousMonthRevenue.IsZero
            ? 0
            : (currentMonthRevenue.Amount - previousMonthRevenue.Amount) / previousMonthRevenue.Amount;

        var forecast = Forecast.Create(
            organizationId, cashBalance, adjustedExpenses, adjustedRevenue,
            ForecastSource.Agent, scenarioLabel);

        // Generate 3-month projection with growth
        var balance = cashBalance;
        var projectedRevenue = adjustedRevenue;
        var projectedExpenses = adjustedExpenses;

        for (int i = 1; i <= 3; i++)
        {
            // Apply growth trend
            projectedRevenue = Money.From(
                projectedRevenue.Amount * (1 + (decimal)growthRate * 0.5m), // Dampened growth
                projectedRevenue.Currency);

            balance = balance.Add(projectedRevenue).Subtract(projectedExpenses);
            var projectionDate = DateOnly.FromDateTime(now.AddMonths(i));

            forecast.AddProjectionPoint(projectionDate, balance, projectedRevenue, projectedExpenses);
        }

        return forecast;
    }
}
