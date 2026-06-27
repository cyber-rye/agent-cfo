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

        // Growth-adjusted revenue projection
        var growthRate = previousMonthRevenue.IsZero
            ? 0
            : (currentMonthRevenue.Amount - previousMonthRevenue.Amount) / previousMonthRevenue.Amount;

        var forecast = Forecast.Create(
            organizationId, cashBalance, currentMonthExpenses, currentMonthRevenue,
            ForecastSource.Agent, scenario);

        // Generate 3-month projection with growth
        var balance = cashBalance;
        var projectedRevenue = currentMonthRevenue;
        var projectedExpenses = currentMonthExpenses;

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

