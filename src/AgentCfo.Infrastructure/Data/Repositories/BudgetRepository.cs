using AgentCfo.Core.Entities;
using AgentCfo.Core.Enums;
using AgentCfo.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgentCfo.Infrastructure.Data.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly AppDbContext _db;
    
    public BudgetRepository(AppDbContext db) => _db = db;
    
    public async Task<Budget?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Budgets.FindAsync([id], ct);
    
    public async Task<IReadOnlyList<Budget>> GetAllAsync(CancellationToken ct = default)
        => await _db.Budgets.ToListAsync(ct);
    
    public async Task<IReadOnlyList<Budget>> GetByOrganizationAsync(Guid organizationId, CancellationToken ct = default)
        => await _db.Budgets.Where(b => b.OrganizationId == organizationId && b.IsActive).ToListAsync(ct);
    
    public async Task<Budget?> GetByOrganizationAndCategoryAsync(Guid organizationId, ExpenseCategory category, CancellationToken ct = default)
        => await _db.Budgets.FirstOrDefaultAsync(b => b.OrganizationId == organizationId && b.Category == category && b.IsActive, ct);
    
    public async Task<Budget> AddAsync(Budget entity, CancellationToken ct = default)
    {
        await _db.Budgets.AddAsync(entity, ct);
        return entity;
    }
    
    public void Update(Budget entity) => _db.Budgets.Update(entity);
    public void Delete(Budget entity) => _db.Budgets.Remove(entity);
}
