using AgentCfo.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgentCfo.Infrastructure.Data.Configurations;

public class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.ActorId).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
        
        builder.HasOne(a => a.Organization)
            .WithMany(o => o.AuditEntries)
            .HasForeignKey(a => a.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(a => new { a.OrganizationId, a.CreatedAt });
        builder.HasIndex(a => a.CorrelationId);
    }
}
