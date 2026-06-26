using AgentCfo.Core.Enums;
using AgentCfo.Core.Interfaces;
using MediatR;

namespace AgentCfo.Application.Budgets;

public static class GetBudgets
{
    public record Query(Guid OrganizationId) : IRequest<List<Response>>;
    
    public record Response(Guid Id, ExpenseCategory Category, decimal MonthlyLimit, string Currency,
                           decimal CurrentSpend, decimal PercentUsed, bool IsNearLimit, bool IsOverBudget);
    
    public class Handler : IRequestHandler<Query, List<Response>>
    {
        private readonly IBudgetRepository _repo;
        
        public Handler(IBudgetRepository repo) => _repo = repo;
        
        public async Task<List<Response>> Handle(Query request, CancellationToken ct)
        {
            var budgets = await _repo.GetByOrganizationAsync(request.OrganizationId, ct);
            return budgets.Select(b => new Response(
                b.Id, b.Category, b.MonthlyLimit.Amount, b.MonthlyLimit.Currency,
                b.CurrentSpend.Amount, b.PercentUsed, b.IsNearLimit, b.IsOverBudget)).ToList();
        }
    }
}
