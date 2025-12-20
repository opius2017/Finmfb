using FinTech.Core.Domain.Entities.FixedAssets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class AssetConfiguration : IEntityTypeConfiguration<Asset>
    {
        public void Configure(EntityTypeBuilder<Asset> builder)
        {
            builder.ToTable("Assets", "fixed_assets");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.PurchaseCost).HasPrecision(18, 2);
            builder.Property(e => e.CurrentBookValue).HasPrecision(18, 2);
            builder.Property(e => e.SalvageValue).HasPrecision(18, 2);
            builder.Property(e => e.DisposalProceeds).HasPrecision(18, 2);
            builder.Property(e => e.DisposalGainLoss).HasPrecision(18, 2);
        }
    }

    public class AssetCategoryConfiguration : IEntityTypeConfiguration<AssetCategory>
    {
        public void Configure(EntityTypeBuilder<AssetCategory> builder)
        {
            builder.ToTable("AssetCategories", "fixed_assets");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.DefaultUsefulLifeYears).HasPrecision(18, 2);
            builder.Property(e => e.DefaultSalvageValuePercent).HasPrecision(18, 4);
        }
    }

    public class AssetDepreciationScheduleConfiguration : IEntityTypeConfiguration<AssetDepreciationSchedule>
    {
        public void Configure(EntityTypeBuilder<AssetDepreciationSchedule> builder)
        {
            builder.ToTable("AssetDepreciationSchedules", "fixed_assets");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.OpeningBookValue).HasPrecision(18, 2);
            builder.Property(e => e.DepreciationAmount).HasPrecision(18, 2);
            builder.Property(e => e.AccumulatedDepreciation).HasPrecision(18, 2);
            builder.Property(e => e.ClosingBookValue).HasPrecision(18, 2);
        }
    }

    public class AssetMaintenanceConfiguration : IEntityTypeConfiguration<AssetMaintenance>
    {
        public void Configure(EntityTypeBuilder<AssetMaintenance> builder)
        {
            builder.ToTable("AssetMaintenances", "fixed_assets");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Cost).HasPrecision(18, 2);
            builder.Property(e => e.ValueIncreaseAmount).HasPrecision(18, 2);
        }
    }

    public class AssetRevaluationConfiguration : IEntityTypeConfiguration<AssetRevaluation>
    {
        public void Configure(EntityTypeBuilder<AssetRevaluation> builder)
        {
            builder.ToTable("AssetRevaluations", "fixed_assets");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.PreviousBookValue).HasPrecision(18, 2);
            builder.Property(e => e.NewBookValue).HasPrecision(18, 2);
            builder.Property(e => e.ValueChange).HasPrecision(18, 2);
        }
    }

    public class AssetDisposalConfiguration : IEntityTypeConfiguration<AssetDisposal>
    {
        public void Configure(EntityTypeBuilder<AssetDisposal> builder)
        {
            builder.ToTable("AssetDisposals", "fixed_assets");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.BookValueAtDisposal).HasPrecision(18, 2);
            builder.Property(e => e.AccumulatedDepreciationAtDisposal).HasPrecision(18, 2);
            builder.Property(e => e.DisposalProceeds).HasPrecision(18, 2);
            builder.Property(e => e.GainOrLoss).HasPrecision(18, 2);
        }
    }
}
