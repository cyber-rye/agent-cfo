using AgentCfo.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgentCfo.Infrastructure.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Name).IsRequired().HasMaxLength(200);
        builder.Property(o => o.StripeCustomerId).IsRequired().HasMaxLength(100);
        builder.Property(o => o.StripeAccountId).HasMaxLength(100);
        
        builder.OwnsOne(o => o.MonthlyBudget, mb =>
        {
            mb.Property(m => m.Amount).HasColumnType("decimal(18,2)");
            mb.Property(m => m.Currency).HasMaxLength(3);
        });
        
        builder.HasIndex(o => o.StripeCustomerId).IsUnique();
    }
}
