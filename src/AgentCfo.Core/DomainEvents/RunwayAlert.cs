using AgentCfo.Core.Common;

namespace AgentCfo.Core.DomainEvents;

public record RunwayAlert(int RunwayDays, DateOnly ProjectedEndDate, string Severity) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
