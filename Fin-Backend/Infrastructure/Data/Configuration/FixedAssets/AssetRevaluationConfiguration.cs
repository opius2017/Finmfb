using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Core.Domain.Entities.FixedAssets;

namespace FinTech.Infrastructure.Data.Configuration.FixedAssets;

public class AssetRevaluationConfiguration : IEntityTypeConfiguration<AssetRevaluation>
{
    public void Configure(EntityTypeBuilder<AssetRevaluation> builder)
    {
        builder.HasIndex(e => e.RevaluationDate);
        builder.HasIndex(e => e.IsPosted);
        
        builder.HasOne(r => r.Asset)
              .WithMany(a => a.Revaluations)
              .HasForeignKey(r => r.AssetId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
