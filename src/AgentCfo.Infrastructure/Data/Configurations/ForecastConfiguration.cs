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

        builder.OwnsMany(f => f.ProjectionPoints, pp =>
        {
            pp.WithOwner().HasForeignKey("ForecastId");
            pp.Property<Guid>("Id").ValueGeneratedOnAdd();
            pp.HasKey("Id");
            pp.ToTable("ProjectionPoints");

            pp.OwnsOne(p => p.ProjectedBalance, pb =>
            {
                pb.Property(m => m.Amount).HasColumnType("decimal(18,2)");
                pb.Property(m => m.Currency).HasMaxLength(3);
            });
            pp.OwnsOne(p => p.ProjectedRevenue, pr =>
            {
                pr.Property(m => m.Amount).HasColumnType("decimal(18,2)");
                pr.Property(m => m.Currency).HasMaxLength(3);
            });
            pp.OwnsOne(p => p.ProjectedExpenses, pe =>
            {
                pe.Property(m => m.Amount).HasColumnType("decimal(18,2)");
                pe.Property(m => m.Currency).HasMaxLength(3);
            });
        });

        builder.HasOne(f => f.Organization)
            .WithMany(o => o.Forecasts)
            .HasForeignKey(f => f.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
