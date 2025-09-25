using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Core.Domain.Entities.FixedAssets;

namespace FinTech.Infrastructure.Data.Configuration.FixedAssets;

public class AssetCategoryConfiguration : IEntityTypeConfiguration<AssetCategory>
{
    public void Configure(EntityTypeBuilder<AssetCategory> builder)
    {
        builder.HasIndex(e => new { e.TenantId, e.CategoryCode }).IsUnique();
        
        builder.HasOne(c => c.ParentCategory)
              .WithMany(c => c.ChildCategories)
              .HasForeignKey(c => c.ParentCategoryId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
