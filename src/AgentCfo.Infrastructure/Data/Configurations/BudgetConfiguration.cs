using AgentCfo.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgentCfo.Infrastructure.Data.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasKey(b => b.Id);
        
        builder.OwnsOne(b => b.MonthlyLimit, ml =>
        {
            ml.Property(m => m.Amount).HasColumnType("decimal(18,2)");
            ml.Property(m => m.Currency).HasMaxLength(3);
        });
        
        builder.OwnsOne(b => b.CurrentSpend, cs =>
        {
            cs.Property(m => m.Amount).HasColumnType("decimal(18,2)");
            cs.Property(m => m.Currency).HasMaxLength(3);
        });
        
        builder.HasOne(b => b.Organization)
            .WithMany(o => o.Budgets)
            .HasForeignKey(b => b.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(b => new { b.OrganizationId, b.Category }).IsUnique();
    }
}
