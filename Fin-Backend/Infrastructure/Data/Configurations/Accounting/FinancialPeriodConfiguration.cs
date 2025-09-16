using FinTech.Domain.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Data.Configurations.Accounting
{
    public class FinancialPeriodConfiguration : IEntityTypeConfiguration<FinancialPeriod>
    {
        public void Configure(EntityTypeBuilder<FinancialPeriod> builder)
        {
            builder.ToTable("FinancialPeriods", "accounting");

            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.Id)
                .HasMaxLength(50)
                .IsRequired();
                
            builder.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();
                
            builder.Property(p => p.StartDate)
                .IsRequired();
                
            builder.Property(p => p.EndDate)
                .IsRequired();
                
            builder.Property(p => p.Status)
                .IsRequired();
                
            builder.Property(p => p.CreatedBy)
                .HasMaxLength(100)
                .IsRequired();
                
            builder.Property(p => p.CreatedAt)
                .IsRequired();
                
            builder.Property(p => p.LastModifiedBy)
                .HasMaxLength(100);
                
            builder.Property(p => p.LastModifiedAt);
                
            builder.Property(p => p.ClosedBy)
                .HasMaxLength(100);
                
            builder.Property(p => p.ClosedAt);
                
            // Foreign key references
            builder.Property(p => p.FiscalYearId)
                .HasMaxLength(50)
                .IsRequired();
                
            // Relationships
            builder.HasOne(p => p.FiscalYear)
                .WithMany(f => f.FinancialPeriods)
                .HasForeignKey(p => p.FiscalYearId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Add indexes
            builder.HasIndex(p => p.FiscalYearId);
            
            builder.HasIndex(p => p.Status);
            
            builder.HasIndex(p => p.StartDate);
            
            builder.HasIndex(p => p.EndDate);
            
            // Add unique constraint for period name within fiscal year
            builder.HasIndex(p => new { p.Name, p.FiscalYearId })
                .IsUnique();
        }
    }
}