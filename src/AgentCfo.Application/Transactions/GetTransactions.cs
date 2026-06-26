using AgentCfo.Core.Entities;
using AgentCfo.Core.Enums;
using AgentCfo.Core.Interfaces;
using MediatR;

namespace AgentCfo.Application.Transactions;

public static class GetTransactions
{
    public record Query(Guid OrganizationId, TransactionType? Type = null, int Limit = 50) : IRequest<List<Response>>;
    
    public record Response(Guid Id, TransactionType Type, decimal Amount, string Currency, string Description, 
                           ExpenseCategory Category, TransactionStatus Status, DateTime OccurredAt);
    
    public class Handler : IRequestHandler<Query, List<Response>>
    {
        private readonly ITransactionRepository _repo;
        
        public Handler(ITransactionRepository repo) => _repo = repo;
        
        public async Task<List<Response>> Handle(Query request, CancellationToken ct)
        {
            IReadOnlyList<Transaction> transactions;
            
            if (request.Type.HasValue)
                transactions = await _repo.GetByOrganizationAndTypeAsync(request.OrganizationId, request.Type.Value, ct);
            else
                transactions = await _repo.GetByOrganizationAsync(request.OrganizationId, ct);
            
            return transactions.Take(request.Limit).Select(t => new Response(
                t.Id, t.Type, t.Amount.Amount, t.Amount.Currency, t.Description,
                t.Category, t.Status, t.OccurredAt)).ToList();
        }
    }
}
