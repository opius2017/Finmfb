using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Domain.Entities.FixedAssets;

namespace FinTech.Infrastructure.Data.Configuration.FixedAssets;

public class AssetDisposalConfiguration : IEntityTypeConfiguration<AssetDisposal>
{
    public void Configure(EntityTypeBuilder<AssetDisposal> builder)
    {
        builder.HasIndex(e => e.DisposalNumber).IsUnique();
        builder.HasIndex(e => e.DisposalDate);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.DisposalMethod);
        builder.HasIndex(e => e.IsPosted);
        
        builder.HasOne(d => d.Asset)
              .WithMany()
              .HasForeignKey(d => d.AssetId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}