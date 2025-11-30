using FinTech.Core.Domain.Entities.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Data.Configurations.Loans
{
    public class MonthlyThresholdConfiguration : IEntityTypeConfiguration<MonthlyThreshold>
    {
        public void Configure(EntityTypeBuilder<MonthlyThreshold> builder)
        {
            builder.ToTable("MonthlyThresholds");
            
            builder.HasKey(mt => mt.Id);
            
            builder.Property(mt => mt.MaximumAmount)
                .HasColumnType("decimal(18,2)");
            
            builder.Property(mt => mt.AllocatedAmount)
                .HasColumnType("decimal(18,2)");
            
            builder.Property(mt => mt.RemainingAmount)
                .HasColumnType("decimal(18,2)");
            
            builder.Property(mt => mt.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);
            
            builder.Property(mt => mt.ClosedBy)
                .HasMaxLength(100);
            
            builder.Property(mt => mt.Notes)
                .HasMaxLength(1000);
            
            // Unique constraint on Year and Month
            builder.HasIndex(mt => new { mt.Year, mt.Month })
                .IsUnique();
            
            // Indexes
            builder.HasIndex(mt => mt.Year);
            builder.HasIndex(mt => mt.Status);
            builder.HasIndex(mt => mt.ClosedAt);
        }
    }
}
