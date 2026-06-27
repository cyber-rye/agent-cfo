using AgentCfo.Core.Interfaces;
using MediatR;

namespace AgentCfo.Application.Forecasts;

public static class GetRevenueMetrics
{
    public record Query(Guid OrganizationId) : IRequest<Response?>;

    public record Response(
        decimal MonthlyRecurringRevenue,
        string Currency,
        decimal PreviousMonthRevenue,
        decimal GrowthRatePercent,
        decimal ChurnRatePercent,
        decimal AverageRevenuePerUser,
        int ActiveSubscriptions,
        int ChurnedSubscriptions,
        decimal NetRevenueRetentionPercent,
        List<MonthlyRevenuePointDto> MonthlyTrend);

    public record MonthlyRevenuePointDto(DateOnly Month, decimal Revenue, int TransactionCount);

    public class Handler : IRequestHandler<Query, Response?>
    {
        private readonly IRevenueMetricsService _metricsService;

        public Handler(IRevenueMetricsService metricsService)
        {
            _metricsService = metricsService;
        }

        public async Task<Response?> Handle(Query request, CancellationToken ct)
        {
            var metrics = await _metricsService.GetMetricsAsync(request.OrganizationId, ct);

            return new Response(
                metrics.MonthlyRecurringRevenue.Amount,
                metrics.MonthlyRecurringRevenue.Currency,
                metrics.PreviousMonthRevenue.Amount,
                metrics.GrowthRatePercent,
                metrics.ChurnRatePercent,
                metrics.AverageRevenuePerUser.Amount,
                metrics.ActiveSubscriptions,
                metrics.ChurnedSubscriptions,
                metrics.NetRevenueRetention.Amount,
                metrics.MonthlyTrend.Select(m => new MonthlyRevenuePointDto(
                    m.Month, m.Revenue.Amount, m.TransactionCount)).ToList());
        }
    }
}
