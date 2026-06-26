using AgentCfo.Core.Common;
using AgentCfo.Core.Entities;
using AgentCfo.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgentCfo.Infrastructure.Data;

public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<Forecast> Forecasts => Set<Forecast>();
    public DbSet<AgentDecision> AgentDecisions => Set<AgentDecision>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Audit trail: capture changes before saving
        var auditEntries = new List<AuditEntry>();
        
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            {
                var entity = entry.Entity;
                var orgId = GetOrganizationId(entity);
                
                if (orgId.HasValue)
                {
                    auditEntries.Add(AuditEntry.Create(
                        organizationId: orgId.Value,
                        actorType: Core.Enums.ActorType.System,
                        actorId: "ef-core",
                        action: entry.State.ToString().ToLowerInvariant(),
                        entityType: entity.GetType().Name,
                        entityId: entity.Id));
                }
            }
        }
        
        AuditEntries.AddRange(auditEntries);
        return await base.SaveChangesAsync(ct);
    }
    
    private static Guid? GetOrganizationId(BaseEntity entity) => entity switch
    {
        Organization org => org.Id,
        Transaction tx => tx.OrganizationId,
        Budget budget => budget.OrganizationId,
        Forecast forecast => forecast.OrganizationId,
        AgentDecision decision => decision.OrganizationId,
        AuditEntry audit => audit.OrganizationId,
        _ => null
    };
}
