using AgentCfo.Core.Common;
using AgentCfo.Core.Entities;
using AgentCfo.Core.Enums;
using AgentCfo.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgentCfo.Infrastructure.Services;

public class RevenueMetricsService : IRevenueMetricsService
{
    private readonly ITransactionRepository _transactionRepo;

    public RevenueMetricsService(ITransactionRepository transactionRepo)
    {
        _transactionRepo = transactionRepo;
    }

    public async Task<RevenueMetrics> GetMetricsAsync(Guid organizationId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var previousMonthStart = currentMonthStart.AddMonths(-1);
        var sixMonthsAgo = currentMonthStart.AddMonths(-6);

        // Current and previous month revenue
        var currentMonthRevenue = await _transactionRepo.GetTotalRevenueAsync(
            organizationId, currentMonthStart, now, ct);
        var previousMonthRevenue = await _transactionRepo.GetTotalRevenueAsync(
            organizationId, previousMonthStart, currentMonthStart, ct);

        // Growth rate
        var growthRate = previousMonthRevenue.IsZero
            ? (currentMonthRevenue.IsZero ? 0 : 100)
            : ((currentMonthRevenue.Amount - previousMonthRevenue.Amount) / previousMonthRevenue.Amount) * 100;

        // Get all revenue transactions for the last 6 months for trend analysis
        var recentTransactions = await _transactionRepo.GetByOrganizationAndDateRangeAsync(
            organizationId, sixMonthsAgo, now, ct);

        var revenueTransactions = recentTransactions
            .Where(t => t.Type == TransactionType.Revenue && t.Status == TransactionStatus.Completed)
            .ToList();

        // Monthly trend
        var monthlyTrend = new List<MonthlyRevenuePoint>();
        for (int i = 5; i >= 0; i--)
        {
            var monthStart = currentMonthStart.AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1);
            var monthTransactions = revenueTransactions
                .Where(t => t.OccurredAt >= monthStart && t.OccurredAt < monthEnd)
                .ToList();
            var monthTotal = monthTransactions.Aggregate(Money.Zero(), (acc, t) => acc.Add(t.Amount));
            monthlyTrend.Add(new MonthlyRevenuePoint(
                DateOnly.FromDateTime(monthStart), monthTotal, monthTransactions.Count));
        }

        // Churn estimation: count transactions that appeared in previous month but not current
        // (simplified — real churn tracking would need subscription-level data)
        var previousMonthTransactions = revenueTransactions
            .Where(t => t.OccurredAt >= previousMonthStart && t.OccurredAt < currentMonthStart)
            .ToList();
        var currentMonthTransactions = revenueTransactions
            .Where(t => t.OccurredAt >= currentMonthStart)
            .ToList();

        var previousCustomers = previousMonthTransactions
            .Where(t => t.StripePaymentIntentId != null)
            .Select(t => t.StripePaymentIntentId)
            .Distinct()
            .Count();
        var currentCustomers = currentMonthTransactions
            .Where(t => t.StripePaymentIntentId != null)
            .Select(t => t.StripePaymentIntentId)
            .Distinct()
            .Count();

        var churnedCount = Math.Max(0, previousCustomers - currentCustomers);
        var churnRate = previousCustomers > 0
            ? (decimal)churnedCount / previousCustomers * 100
            : 0;

        // ARPU
        var activeCustomers = Math.Max(1, currentCustomers);
        var arpu = Money.From(currentMonthRevenue.Amount / activeCustomers, currentMonthRevenue.Currency);

        // Net Revenue Retention (simplified: current MRR / previous MRR * 100)
        var nrr = previousMonthRevenue.IsZero
            ? currentMonthRevenue
            : Money.From((currentMonthRevenue.Amount / previousMonthRevenue.Amount) * 100, currentMonthRevenue.Currency);

        return new RevenueMetrics
        {
            MonthlyRecurringRevenue = currentMonthRevenue,
            PreviousMonthRevenue = previousMonthRevenue,
            GrowthRatePercent = Math.Round(growthRate, 2),
            ChurnRatePercent = Math.Round(churnRate, 2),
            AverageRevenuePerUser = arpu,
            ActiveSubscriptions = currentCustomers,
            ChurnedSubscriptions = churnedCount,
            NetRevenueRetention = nrr,
            MonthlyTrend = monthlyTrend
        };
    }
}
