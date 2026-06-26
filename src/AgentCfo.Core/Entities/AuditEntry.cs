using AgentCfo.Core.Common;
using AgentCfo.Core.Enums;

namespace AgentCfo.Core.Entities;

public class AuditEntry : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public ActorType ActorType { get; private set; }
    public string ActorId { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string? BeforeState { get; private set; }
    public string? AfterState { get; private set; }
    public Guid CorrelationId { get; private set; }
    
    public Organization Organization { get; private set; } = null!;
    
    private AuditEntry() { }
    
    public static AuditEntry Create(
        Guid organizationId,
        ActorType actorType,
        string actorId,
        string action,
        string entityType,
        Guid entityId,
        Guid? correlationId = null,
        string? beforeState = null,
        string? afterState = null)
    {
        return new AuditEntry
        {
            OrganizationId = organizationId,
            ActorType = actorType,
            ActorId = actorId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            BeforeState = beforeState,
            AfterState = afterState,
            CorrelationId = correlationId ?? Guid.NewGuid()
        };
    }
}
