using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Domain.Entities.FixedAssets;

namespace FinTech.Infrastructure.Data.Configuration.FixedAssets;

public class AssetInventoryCountItemConfiguration : IEntityTypeConfiguration<AssetInventoryCountItem>
{
    public void Configure(EntityTypeBuilder<AssetInventoryCountItem> builder)
    {
        builder.HasIndex(e => new { e.InventoryCountId, e.AssetId }).IsUnique();
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.WasFound);
        
        builder.HasOne(i => i.InventoryCount)
              .WithMany(c => c.CountItems)
              .HasForeignKey(i => i.InventoryCountId)
              .OnDelete(DeleteBehavior.Cascade);
              
        builder.HasOne(i => i.Asset)
              .WithMany()
              .HasForeignKey(i => i.AssetId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}