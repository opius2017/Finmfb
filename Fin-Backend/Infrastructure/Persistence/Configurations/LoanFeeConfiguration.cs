using FinTech.Core.Domain.Entities.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class LoanFeeConfiguration : IEntityTypeConfiguration<LoanFee>
    {
        public void Configure(EntityTypeBuilder<LoanFee> builder)
        {
            builder.ToTable("LoanFees", "loans");

            builder.HasKey(lf => lf.Id);
            
            builder.Property(lf => lf.LoanId)
                .HasMaxLength(100);
                
            builder.Property(lf => lf.LoanProductId);
                
            builder.Property(lf => lf.FeeType)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(lf => lf.Name)
                .IsRequired()
                .HasMaxLength(150);
                
            builder.Property(lf => lf.Description)
                .HasMaxLength(500);
                
            builder.Property(lf => lf.Amount)
                .HasPrecision(18, 2);
                
            builder.Property(lf => lf.Percentage)
                .HasPrecision(18, 4);
                
            builder.Property(lf => lf.IsRequired)
                .IsRequired();
                
            builder.Property(lf => lf.AppliedAt)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.HasIndex(lf => lf.LoanId);
            builder.HasIndex(lf => lf.LoanProductId);
            builder.HasIndex(lf => lf.FeeType);
            
            // Relationships
            builder.HasOne<Loan>()
                .WithMany()
                .HasForeignKey(lf => lf.LoanId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
                
            builder.HasOne<LoanProduct>()
                .WithMany()
                .HasForeignKey(lf => lf.LoanProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }
}
