using AgentCfo.Core.Common;
using AgentCfo.Core.Enums;

namespace AgentCfo.Core.Entities;

public class AgentDecision : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public AgentDecisionType Type { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string Reasoning { get; private set; } = string.Empty;
    public Guid? RelatedTransactionId { get; private set; }
    public Guid? RelatedBudgetId { get; private set; }
    public DecisionStatus Status { get; private set; } = DecisionStatus.Proposed;
    public DateTime? ExecutedAt { get; private set; }
    public string? OverriddenBy { get; private set; }
    public string? Metadata { get; private set; }
    
    public Organization Organization { get; private set; } = null!;
    public Transaction? RelatedTransaction { get; private set; }
    public Budget? RelatedBudget { get; private set; }
    
    private AgentDecision() { }
    
    public static AgentDecision Propose(
        Guid organizationId,
        AgentDecisionType type,
        string description,
        string reasoning,
        Guid? relatedTransactionId = null,
        Guid? relatedBudgetId = null)
    {
        return new AgentDecision
        {
            OrganizationId = organizationId,
            Type = type,
            Description = description,
            Reasoning = reasoning,
            RelatedTransactionId = relatedTransactionId,
            RelatedBudgetId = relatedBudgetId,
            Status = DecisionStatus.Proposed
        };
    }
    
    public void Execute()
    {
        Status = DecisionStatus.Executed;
        ExecutedAt = DateTime.UtcNow;
        MarkUpdated();
    }
    
    public void Override(string overriddenBy)
    {
        Status = DecisionStatus.Overridden;
        OverriddenBy = overriddenBy;
        MarkUpdated();
    }
    
    public void Fail()
    {
        Status = DecisionStatus.Failed;
        MarkUpdated();
    }
}
