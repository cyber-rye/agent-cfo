using AgentCfo.Core.Common;
using AgentCfo.Core.Entities;
using AgentCfo.Core.Enums;
using Microsoft.EntityFrameworkCore;
using AgentCfo.Core.Interfaces;

namespace AgentCfo.Infrastructure.Data.Repositories;

public class AgentDecisionRepository : IAgentDecisionRepository
{
    private readonly AppDbContext _db;

    public AgentDecisionRepository(AppDbContext db) => _db = db;

    public async Task<AgentDecision?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.AgentDecisions.FindAsync([id], ct);

    public async Task<IReadOnlyList<AgentDecision>> GetAllAsync(CancellationToken ct = default)
        => await _db.AgentDecisions.ToListAsync(ct);

    public async Task<IReadOnlyList<AgentDecision>> GetByOrganizationAsync(Guid organizationId, CancellationToken ct = default)
        => await _db.AgentDecisions
            .Where(d => d.OrganizationId == organizationId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AgentDecision>> GetByOrganizationAndTypeAsync(Guid organizationId, AgentDecisionType type, CancellationToken ct = default)
        => await _db.AgentDecisions
            .Where(d => d.OrganizationId == organizationId && d.Type == type)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AgentDecision>> GetRecentAsync(Guid organizationId, int limit = 10, CancellationToken ct = default)
        => await _db.AgentDecisions
            .Where(d => d.OrganizationId == organizationId)
            .OrderByDescending(d => d.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

    public async Task<AgentDecision> AddAsync(AgentDecision entity, CancellationToken ct = default)
    {
        await _db.AgentDecisions.AddAsync(entity, ct);
        return entity;
    }

    public void Update(AgentDecision entity) => _db.AgentDecisions.Update(entity);
    public void Delete(AgentDecision entity) => _db.AgentDecisions.Remove(entity);
}
