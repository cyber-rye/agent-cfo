using AgentCfo.Core.Entities;

namespace AgentCfo.Core.Interfaces;

public interface IAuditEntryRepository : IRepository<AuditEntry>
{
    Task<IReadOnlyList<AuditEntry>> GetByOrganizationAsync(Guid organizationId, int limit = 50, CancellationToken ct = default);
    Task<IReadOnlyList<AuditEntry>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken ct = default);
}
