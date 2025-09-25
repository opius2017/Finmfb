using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Core.Domain.Entities.FixedAssets;

namespace FinTech.Infrastructure.Data.Configuration.FixedAssets;

public class AssetInventoryCountConfiguration : IEntityTypeConfiguration<AssetInventoryCount>
{
    public void Configure(EntityTypeBuilder<AssetInventoryCount> builder)
    {
        builder.HasIndex(e => e.InventoryCountNumber).IsUnique();
        builder.HasIndex(e => e.CountDate);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.Location);
        builder.HasIndex(e => e.Department);
    }
}
