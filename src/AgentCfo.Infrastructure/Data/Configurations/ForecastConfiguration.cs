using AgentCfo.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgentCfo.Infrastructure.Data.Configurations;

public class ForecastConfiguration : IEntityTypeConfiguration<Forecast>
{
    public void Configure(EntityTypeBuilder<Forecast> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Scenario).HasMaxLength(100);
        
        builder.OwnsOne(f => f.CurrentCashBalance, cb =>
        {
            cb.Property(m => m.Amount).HasColumnType("decimal(18,2)");
            cb.Property(m => m.Currency).HasMaxLength(3);
        });
        
        builder.OwnsOne(f => f.MonthlyBurnRate, br =>
        {
            br.Property(m => m.Amount).HasColumnType("decimal(18,2)");
            br.Property(m => m.Currency).HasMaxLength(3);
        });
        
        builder.OwnsOne(f => f.MonthlyRevenue, mr =>
        {
            mr.Property(m => m.Amount).HasColumnType("decimal(18,2)");
            mr.Property(m => m.Currency).HasMaxLength(3);
        });
        
        builder.HasOne(f => f.Organization)
            .WithMany(o => o.Forecasts)
            .HasForeignKey(f => f.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
