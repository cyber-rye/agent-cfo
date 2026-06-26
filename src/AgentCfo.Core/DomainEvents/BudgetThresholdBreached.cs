using AgentCfo.Core.Common;

namespace AgentCfo.Core.DomainEvents;

public record BudgetThresholdBreached(Guid BudgetId, Money CurrentSpend, Money Limit, decimal PercentUsed) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
