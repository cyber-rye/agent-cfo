using AgentCfo.Core.Entities;
using AgentCfo.Core.Enums;
using AgentCfo.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AgentCfo.Application.Common;

/// <summary>
/// Pipeline behavior that enforces spending policies before commands execute.
/// Checks budget limits, spending thresholds, and approval gates.
/// </summary>
public class PolicyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IBudgetRepository _budgetRepo;
    private readonly ILogger<PolicyBehavior<TRequest, TResponse>> _logger;

    public PolicyBehavior(IBudgetRepository budgetRepo, ILogger<PolicyBehavior<TRequest, TResponse>> logger)
    {
        _budgetRepo = budgetRepo;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        // Only enforce policy on commands that implement IPolicyEnforced
        if (request is IPolicyEnforced policyRequest)
        {
            _logger.LogInformation("Policy: Checking spending limits for {RequestType} - OrgId: {OrgId}, Amount: {Amount}, Category: {Category}",
                typeof(TRequest).Name, policyRequest.OrganizationId, policyRequest.Amount, policyRequest.Category);

            var budget = await _budgetRepo.GetByOrganizationAndCategoryAsync(
                policyRequest.OrganizationId, policyRequest.Category, ct);

            if (budget is not null)
            {
                var amount = Core.Common.Money.From(policyRequest.Amount, policyRequest.Currency);

                // Check hard limit
                if (budget.HardLimit && !budget.CanSpend(amount))
                {
                    _logger.LogWarning("Policy: BLOCKED - Budget exceeded for {Category}. Limit: {Limit}, Current: {Current}, Requested: {Requested}",
                        budget.Category, budget.MonthlyLimit, budget.CurrentSpend, amount);

                    throw new PolicyViolationException(
                        $"Budget exceeded for {budget.Category}: limit is {budget.MonthlyLimit}, " +
                        $"current spend is {budget.CurrentSpend}, requested {amount}");
                }

                // Check alert threshold
                if (budget.IsNearLimit)
                {
                    _logger.LogWarning("Policy: WARNING - Budget for {Category} at {Percent:F0}% (threshold: {Threshold}%)",
                        budget.Category, budget.PercentUsed, budget.AlertThresholdPercent);
                }
            }
        }

        return await next();
    }
}

/// <summary>
/// Marker interface for requests that should be checked against spending policies.
/// </summary>
public interface IPolicyEnforced
{
    Guid OrganizationId { get; }
    decimal Amount { get; }
    string Currency { get; }
    ExpenseCategory Category { get; }
}

public class PolicyViolationException : Exception
{
    public PolicyViolationException(string message) : base(message) { }
}
