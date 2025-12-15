using FinTech.Core.Domain.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Data.Configurations.Accounting
{
    public class FiscalYearConfiguration : IEntityTypeConfiguration<FiscalYear>
    {
        public void Configure(EntityTypeBuilder<FiscalYear> builder)
        {
            builder.ToTable("FiscalYears", "accounting");

            builder.HasKey(f => f.Id);
            
            builder.Property(f => f.Id)
                .HasMaxLength(50)
                .IsRequired();
                
            builder.Property(f => f.Code)
                .HasMaxLength(20)
                .IsRequired();
                
            builder.Property(f => f.Name)
                .HasMaxLength(100)
                .IsRequired();
                
            builder.Property(f => f.Year)
                .IsRequired();
                
            builder.Property(f => f.StartDate)
                .IsRequired();
                
            builder.Property(f => f.EndDate)
                .IsRequired();
                
            builder.Property(f => f.Status)
                .IsRequired();
                
            builder.Property(f => f.CreatedBy)
                .HasMaxLength(100)
                .IsRequired();
                
            builder.Property(f => f.CreatedAt)
                .IsRequired();
                
            builder.Property(f => f.LastModifiedBy)
                .HasMaxLength(100);
                
            builder.Property(f => f.LastModifiedAt);
                
            builder.Property(f => f.ClosedBy)
                .HasMaxLength(100);
                
            builder.Property(f => f.ClosedDate);
                
            // Navigation for financial periods
            builder.HasMany(f => f.FinancialPeriods)
                .WithOne(p => p.FiscalYear)
                .HasForeignKey(p => p.FiscalYearId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Add indexes
            builder.HasIndex(f => f.Code)
                .IsUnique();
                
            builder.HasIndex(f => f.Year)
                .IsUnique();
                
            builder.HasIndex(f => f.Status);
            
            builder.HasIndex(f => f.StartDate);
            
            builder.HasIndex(f => f.EndDate);
        }
    }
}
