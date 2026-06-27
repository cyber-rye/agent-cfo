using AgentCfo.Core.Enums;
using AgentCfo.Core.Interfaces;
using MediatR;

namespace AgentCfo.Application.Forecasts;

public static class GetCurrentForecast
{
    public record Query(Guid OrganizationId, string? Scenario = null) : IRequest<Response?>;

    public record Response(Guid Id, decimal CashBalance, string Currency, decimal MonthlyBurnRate,
                           decimal MonthlyRevenue, int RunwayDays, DateOnly RunwayEndDate,
                           string Scenario, ForecastConfidence Confidence, List<ProjectionPointDto> Projections);

    public record ProjectionPointDto(DateOnly Date, decimal ProjectedBalance, decimal ProjectedRevenue, decimal ProjectedExpenses);

    public class Handler : IRequestHandler<Query, Response?>
    {
        private readonly IForecastService _forecastService;

        public Handler(IForecastService forecastService)
        {
            _forecastService = forecastService;
        }

        public async Task<Response?> Handle(Query request, CancellationToken ct)
        {
            var forecast = await _forecastService.GenerateForecastAsync(request.OrganizationId, request.Scenario, ct);
            if (forecast is null) return null;

            return new Response(
                forecast.Id, forecast.CurrentCashBalance.Amount, forecast.CurrentCashBalance.Currency,
                forecast.MonthlyBurnRate.Amount, forecast.MonthlyRevenue.Amount,
                forecast.RunwayDays, forecast.RunwayEndDate, forecast.Scenario ?? "base",
                forecast.Confidence,
                forecast.ProjectionPoints.Select(p => new ProjectionPointDto(
                    p.Date, p.ProjectedBalance.Amount, p.ProjectedRevenue.Amount, p.ProjectedExpenses.Amount)).ToList());
        }
    }
}
