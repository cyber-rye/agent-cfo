using AgentCfo.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgentCfo.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Description).IsRequired().HasMaxLength(500);
        builder.Property(t => t.StripeEventId).HasMaxLength(100);
        builder.Property(t => t.StripePaymentIntentId).HasMaxLength(100);
        builder.Property(t => t.StripeInvoiceId).HasMaxLength(100);
        
        builder.OwnsOne(t => t.Amount, a =>
        {
            a.Property(m => m.Amount).HasColumnType("decimal(18,2)");
            a.Property(m => m.Currency).HasMaxLength(3);
        });
        
        builder.HasOne(t => t.Organization)
            .WithMany(o => o.Transactions)
            .HasForeignKey(t => t.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(t => t.StripeEventId).IsUnique();
        builder.HasIndex(t => new { t.OrganizationId, t.OccurredAt });
    }
}
