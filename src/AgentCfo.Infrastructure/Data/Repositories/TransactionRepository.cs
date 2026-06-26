using AgentCfo.Core.Common;
using AgentCfo.Core.Entities;
using AgentCfo.Core.Enums;
using AgentCfo.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgentCfo.Infrastructure.Data.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _db;
    
    public TransactionRepository(AppDbContext db) => _db = db;
    
    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Transactions.FindAsync([id], ct);
    
    public async Task<IReadOnlyList<Transaction>> GetAllAsync(CancellationToken ct = default)
        => await _db.Transactions.ToListAsync(ct);
    
    public async Task<IReadOnlyList<Transaction>> GetByOrganizationAsync(Guid organizationId, CancellationToken ct = default)
        => await _db.Transactions.Where(t => t.OrganizationId == organizationId)
            .OrderByDescending(t => t.OccurredAt).ToListAsync(ct);
    
    public async Task<IReadOnlyList<Transaction>> GetByOrganizationAndTypeAsync(Guid organizationId, TransactionType type, CancellationToken ct = default)
        => await _db.Transactions.Where(t => t.OrganizationId == organizationId && t.Type == type)
            .OrderByDescending(t => t.OccurredAt).ToListAsync(ct);
    
    public async Task<IReadOnlyList<Transaction>> GetByOrganizationAndDateRangeAsync(Guid organizationId, DateTime from, DateTime to, CancellationToken ct = default)
        => await _db.Transactions.Where(t => t.OrganizationId == organizationId && t.OccurredAt >= from && t.OccurredAt <= to)
            .OrderByDescending(t => t.OccurredAt).ToListAsync(ct);
    
    public async Task<Money> GetTotalRevenueAsync(Guid organizationId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var total = await _db.Transactions
            .Where(t => t.OrganizationId == organizationId && t.Type == TransactionType.Revenue && 
                        t.Status == TransactionStatus.Completed && t.OccurredAt >= from && t.OccurredAt <= to)
            .SumAsync(t => t.Amount.Amount, ct);
        return Money.From(total);
    }
    
    public async Task<Money> GetTotalExpensesAsync(Guid organizationId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var total = await _db.Transactions
            .Where(t => t.OrganizationId == organizationId && t.Type == TransactionType.Expense && 
                        t.Status == TransactionStatus.Completed && t.OccurredAt >= from && t.OccurredAt <= to)
            .SumAsync(t => t.Amount.Amount, ct);
        return Money.From(total);
    }
    
    public async Task<Transaction?> GetByStripeEventIdAsync(string stripeEventId, CancellationToken ct = default)
        => await _db.Transactions.FirstOrDefaultAsync(t => t.StripeEventId == stripeEventId, ct);
    
    public async Task<Transaction> AddAsync(Transaction entity, CancellationToken ct = default)
    {
        await _db.Transactions.AddAsync(entity, ct);
        return entity;
    }
    
    public void Update(Transaction entity) => _db.Transactions.Update(entity);
    public void Delete(Transaction entity) => _db.Transactions.Remove(entity);
}
