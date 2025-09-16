using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Domain.Entities.FixedAssets;

namespace FinTech.Infrastructure.Data.Configuration.FixedAssets;

public class AssetDepreciationScheduleConfiguration : IEntityTypeConfiguration<AssetDepreciationSchedule>
{
    public void Configure(EntityTypeBuilder<AssetDepreciationSchedule> builder)
    {
        builder.HasIndex(e => new { e.AssetId, e.PeriodNumber }).IsUnique();
        builder.HasIndex(e => e.PeriodStartDate);
        builder.HasIndex(e => e.PeriodEndDate);
        builder.HasIndex(e => e.IsPosted);
        
        builder.HasOne(s => s.Asset)
              .WithMany(a => a.DepreciationSchedules)
              .HasForeignKey(s => s.AssetId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}