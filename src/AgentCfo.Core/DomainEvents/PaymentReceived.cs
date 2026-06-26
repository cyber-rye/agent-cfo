using AgentCfo.Core.Common;

namespace AgentCfo.Core.DomainEvents;

public record PaymentReceived(Guid TransactionId, Money Amount, string? StripePaymentIntentId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
