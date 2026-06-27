using AgentCfo.Core.Entities;
using AgentCfo.Core.Enums;

namespace AgentCfo.Core.Interfaces;

public interface IAgentDecisionRepository : IRepository<AgentDecision>
{
    Task<IReadOnlyList<AgentDecision>> GetByOrganizationAsync(Guid organizationId, CancellationToken ct = default);
    Task<IReadOnlyList<AgentDecision>> GetByOrganizationAndTypeAsync(Guid organizationId, AgentDecisionType type, CancellationToken ct = default);
    Task<IReadOnlyList<AgentDecision>> GetRecentAsync(Guid organizationId, int limit = 10, CancellationToken ct = default);
}
