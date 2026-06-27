using AgentCfo.Core.Enums;
using AgentCfo.Core.Interfaces;
using MediatR;

namespace AgentCfo.Application.Forecasts;

public static class GetDashboardSummary
{
    public record Query(Guid OrganizationId) : IRequest<Response>;

    public record Response(
        decimal TotalRevenue,
        decimal TotalExpenses,
        decimal NetIncome,
        string Currency,
        int TransactionCount,
        int BudgetCount,
        int OverBudgetCount,
        List<AgentDecisionDto> RecentDecisions);

    public record AgentDecisionDto(Guid Id, AgentDecisionType Type, string Description, string Reasoning, DecisionStatus Status, DateTime CreatedAt);

    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly IBudgetRepository _budgetRepo;
        private readonly IAgentDecisionRepository _decisionRepo;

        public Handler(
            ITransactionRepository transactionRepo,
            IBudgetRepository budgetRepo,
            IAgentDecisionRepository decisionRepo)
        {
            _transactionRepo = transactionRepo;
            _budgetRepo = budgetRepo;
            _decisionRepo = decisionRepo;
        }

        public async Task<Response> Handle(Query request, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var revenue = await _transactionRepo.GetTotalRevenueAsync(request.OrganizationId, startOfMonth, now, ct);
            var expenses = await _transactionRepo.GetTotalExpensesAsync(request.OrganizationId, startOfMonth, now, ct);
            var transactions = await _transactionRepo.GetByOrganizationAsync(request.OrganizationId, ct);
            var budgets = await _budgetRepo.GetByOrganizationAsync(request.OrganizationId, ct);
            var recentDecisions = await _decisionRepo.GetRecentAsync(request.OrganizationId, 10, ct);

            return new Response(
                revenue.Amount, expenses.Amount, revenue.Subtract(expenses).Amount,
                revenue.Currency, transactions.Count, budgets.Count,
                budgets.Count(b => b.IsOverBudget),
                recentDecisions.Select(d => new AgentDecisionDto(
                    d.Id, d.Type, d.Description, d.Reasoning, d.Status, d.CreatedAt)).ToList());
        }
    }
}
