using AgentCfo.Core.Entities;
using AgentCfo.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgentCfo.Infrastructure.Data.Repositories;

public class AuditEntryRepository : IAuditEntryRepository
{
    private readonly AppDbContext _db;

    public AuditEntryRepository(AppDbContext db) => _db = db;

    public async Task<AuditEntry?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.AuditEntries.FindAsync([id], ct);

    public async Task<IReadOnlyList<AuditEntry>> GetAllAsync(CancellationToken ct = default)
        => await _db.AuditEntries.ToListAsync(ct);

    public async Task<IReadOnlyList<AuditEntry>> GetByOrganizationAsync(Guid organizationId, int limit = 50, CancellationToken ct = default)
        => await _db.AuditEntries
            .Where(a => a.OrganizationId == organizationId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AuditEntry>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken ct = default)
        => await _db.AuditEntries
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

    public async Task<AuditEntry> AddAsync(AuditEntry entity, CancellationToken ct = default)
    {
        await _db.AuditEntries.AddAsync(entity, ct);
        return entity;
    }

    public void Update(AuditEntry entity) => _db.AuditEntries.Update(entity);
    public void Delete(AuditEntry entity) => _db.AuditEntries.Remove(entity);
}
