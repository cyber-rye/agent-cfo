using AgentCfo.Core.Entities;
using AgentCfo.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgentCfo.Infrastructure.Data.Repositories;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly AppDbContext _db;
    
    public OrganizationRepository(AppDbContext db) => _db = db;
    
    public async Task<Organization?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Organizations.FindAsync([id], ct);
    
    public async Task<IReadOnlyList<Organization>> GetAllAsync(CancellationToken ct = default)
        => await _db.Organizations.ToListAsync(ct);
    
    public async Task<Organization?> GetByStripeCustomerIdAsync(string stripeCustomerId, CancellationToken ct = default)
        => await _db.Organizations.FirstOrDefaultAsync(o => o.StripeCustomerId == stripeCustomerId, ct);
    
    public async Task<Organization> AddAsync(Organization entity, CancellationToken ct = default)
    {
        await _db.Organizations.AddAsync(entity, ct);
        return entity;
    }
    
    public void Update(Organization entity) => _db.Organizations.Update(entity);
    public void Delete(Organization entity) => _db.Organizations.Remove(entity);
}
