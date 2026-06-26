using AgentCfo.Core.Common;
using AgentCfo.Core.Entities;
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
        private readonly ITransactionRepository _transactionRepo;
        private readonly IOrganizationRepository _organizationRepo;
        
        public Handler(ITransactionRepository transactionRepo, IOrganizationRepository organizationRepo)
        {
            _transactionRepo = transactionRepo;
            _organizationRepo = organizationRepo;
        }
        
        public async Task<Response?> Handle(Query request, CancellationToken ct)
        {
            var org = await _organizationRepo.GetByIdAsync(request.OrganizationId, ct);
            if (org is null) return null;
            
            var now = DateTime.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30);
            
            var monthlyRevenue = await _transactionRepo.GetTotalRevenueAsync(request.OrganizationId, thirtyDaysAgo, now, ct);
            var monthlyExpenses = await _transactionRepo.GetTotalExpensesAsync(request.OrganizationId, thirtyDaysAgo, now, ct);
            
            // Simple runway calculation: assume current balance is the sum of all revenue minus all expenses
            var allTimeRevenue = await _transactionRepo.GetTotalRevenueAsync(request.OrganizationId, DateTime.MinValue, now, ct);
            var allTimeExpenses = await _transactionRepo.GetTotalExpensesAsync(request.OrganizationId, DateTime.MinValue, now, ct);
            var cashBalance = allTimeRevenue.Subtract(allTimeExpenses);
            
            var forecast = Forecast.Create(request.OrganizationId, cashBalance, monthlyExpenses, monthlyRevenue, ForecastSource.Agent, request.Scenario);
            
            // Generate 3-month projection
            var balance = cashBalance;
            for (int i = 1; i <= 3; i++)
            {
                var projectionDate = DateOnly.FromDateTime(now.AddMonths(i));
                var projectedRevenue = monthlyRevenue;
                var projectedExpenses = monthlyExpenses;
                balance = balance.Add(projectedRevenue).Subtract(projectedExpenses);
                forecast.AddProjectionPoint(projectionDate, balance, projectedRevenue, projectedExpenses);
            }
            
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
