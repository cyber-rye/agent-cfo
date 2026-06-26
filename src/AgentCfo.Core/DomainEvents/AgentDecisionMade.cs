using AgentCfo.Core.Common;
using AgentCfo.Core.Enums;

namespace AgentCfo.Core.DomainEvents;

public record AgentDecisionMade(Guid DecisionId, AgentDecisionType Type, string Reasoning) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
