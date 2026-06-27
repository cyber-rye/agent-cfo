using AgentCfo.Core.Common;

namespace AgentCfo.Core.Interfaces;

public interface IRevenueMetricsService
{
    Task<RevenueMetrics> GetMetricsAsync(Guid organizationId, CancellationToken ct = default);
}

public record RevenueMetrics
{
    public Money MonthlyRecurringRevenue { get; init; } = Money.Zero();
    public Money PreviousMonthRevenue { get; init; } = Money.Zero();
    public decimal GrowthRatePercent { get; init; }
    public decimal ChurnRatePercent { get; init; }
    public Money AverageRevenuePerUser { get; init; } = Money.Zero();
    public int ActiveSubscriptions { get; init; }
    public int ChurnedSubscriptions { get; init; }
    public Money NetRevenueRetention { get; init; } = Money.Zero();
    public List<MonthlyRevenuePoint> MonthlyTrend { get; init; } = new();
}

public record MonthlyRevenuePoint(DateOnly Month, Money Revenue, int TransactionCount);
