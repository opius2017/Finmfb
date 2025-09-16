using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Domain.Entities.FixedAssets;

namespace FinTech.Infrastructure.Data.Configuration.FixedAssets;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.HasIndex(e => new { e.TenantId, e.AssetNumber }).IsUnique();
        builder.HasIndex(e => e.AssetTag);
        builder.HasIndex(e => e.SerialNumber);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.AcquisitionDate);
        
        builder.HasOne(a => a.AssetCategory)
              .WithMany(c => c.Assets)
              .HasForeignKey(a => a.AssetCategoryId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}