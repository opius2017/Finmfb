using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Core.Domain.Entities.FixedAssets;

namespace FinTech.Infrastructure.Data.Configuration.FixedAssets;

public class AssetTransferConfiguration : IEntityTypeConfiguration<AssetTransfer>
{
    public void Configure(EntityTypeBuilder<AssetTransfer> builder)
    {
        builder.HasIndex(e => e.TransferNumber).IsUnique();
        builder.HasIndex(e => e.TransferDate);
        builder.HasIndex(e => e.Status);
        
        builder.HasOne(t => t.Asset)
              .WithMany(a => a.Transfers)
              .HasForeignKey(t => t.AssetId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
