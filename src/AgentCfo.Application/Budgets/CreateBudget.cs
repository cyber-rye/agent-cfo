using AgentCfo.Core.Common;
using AgentCfo.Core.Entities;
using AgentCfo.Core.Enums;
using AgentCfo.Core.Interfaces;
using FluentValidation;
using MediatR;

namespace AgentCfo.Application.Budgets;

public static class CreateBudget
{
    public record Command(Guid OrganizationId, ExpenseCategory Category, decimal MonthlyLimit, 
                          string Currency, bool HardLimit = false, int AlertThresholdPercent = 80) : IRequest<Response>;
    
    public record Response(Guid BudgetId);
    
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.OrganizationId).NotEmpty();
            RuleFor(x => x.MonthlyLimit).GreaterThan(0);
            RuleFor(x => x.AlertThresholdPercent).InclusiveBetween(1, 100);
        }
    }
    
    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly IBudgetRepository _repo;
        private readonly IUnitOfWork _unitOfWork;
        
        public Handler(IBudgetRepository repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<Response> Handle(Command request, CancellationToken ct)
        {
            var budget = Budget.Create(
                request.OrganizationId, request.Category, 
                Money.From(request.MonthlyLimit, request.Currency),
                request.HardLimit, request.AlertThresholdPercent);
            
            await _repo.AddAsync(budget, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            
            return new Response(budget.Id);
        }
    }
}
