using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Domain.Entities.FixedAssets;

namespace FinTech.Infrastructure.Data.Configuration.FixedAssets;

public class AssetMaintenanceConfiguration : IEntityTypeConfiguration<AssetMaintenance>
{
    public void Configure(EntityTypeBuilder<AssetMaintenance> builder)
    {
        builder.HasIndex(e => e.MaintenanceNumber).IsUnique();
        builder.HasIndex(e => e.MaintenanceDate);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.MaintenanceType);
        
        builder.HasOne(m => m.Asset)
              .WithMany(a => a.MaintenanceRecords)
              .HasForeignKey(m => m.AssetId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}