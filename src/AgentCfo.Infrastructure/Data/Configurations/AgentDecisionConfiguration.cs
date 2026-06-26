using AgentCfo.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgentCfo.Infrastructure.Data.Configurations;

public class AgentDecisionConfiguration : IEntityTypeConfiguration<AgentDecision>
{
    public void Configure(EntityTypeBuilder<AgentDecision> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Description).IsRequired().HasMaxLength(1000);
        builder.Property(d => d.Reasoning).IsRequired().HasMaxLength(5000);
        builder.Property(d => d.OverriddenBy).HasMaxLength(200);
        
        builder.HasOne(d => d.Organization)
            .WithMany(o => o.Decisions)
            .HasForeignKey(d => d.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(d => d.RelatedTransaction)
            .WithMany()
            .HasForeignKey(d => d.RelatedTransactionId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasOne(d => d.RelatedBudget)
            .WithMany()
            .HasForeignKey(d => d.RelatedBudgetId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
