using AgentCfo.Core.Common;
using AgentCfo.Core.Entities;
using AgentCfo.Core.Enums;
using AgentCfo.Core.Interfaces;
using FluentValidation;
using MediatR;

namespace AgentCfo.Application.Transactions;

public static class RecordExpense
{
    public record Command(Guid OrganizationId, decimal Amount, string Currency, string Description, 
                          ExpenseCategory Category) : IRequest<Response>;
    
    public record Response(Guid TransactionId, bool Approved, string? RejectionReason);
    
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.OrganizationId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        }
    }
    
    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly IBudgetRepository _budgetRepo;
        private readonly IUnitOfWork _unitOfWork;
        
        public Handler(ITransactionRepository transactionRepo, IBudgetRepository budgetRepo, IUnitOfWork unitOfWork)
        {
            _transactionRepo = transactionRepo;
            _budgetRepo = budgetRepo;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<Response> Handle(Command request, CancellationToken ct)
        {
            var amount = Money.From(request.Amount, request.Currency);
            
            // Check budget
            var budget = await _budgetRepo.GetByOrganizationAndCategoryAsync(request.OrganizationId, request.Category, ct);
            if (budget is not null && budget.HardLimit && !budget.CanSpend(amount))
            {
                return new Response(Guid.Empty, false, 
                    $"Budget exceeded: {budget.Category} limit is {budget.MonthlyLimit}, current spend is {budget.CurrentSpend}");
            }
            
            var transaction = Transaction.CreateExpense(request.OrganizationId, amount, request.Description, request.Category);
            await _transactionRepo.AddAsync(transaction, ct);
            
            budget?.RecordSpend(amount);
            await _unitOfWork.SaveChangesAsync(ct);
            
            return new Response(transaction.Id, true, null);
        }
    }
}
