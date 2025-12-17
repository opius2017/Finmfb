using FinTech.Core.Domain.Entities.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class LoanProductConfiguration : IEntityTypeConfiguration<LoanProduct>
    {
        public void Configure(EntityTypeBuilder<LoanProduct> builder)
        {
            builder.ToTable("LoanProducts", "loans");

            builder.HasKey(lp => lp.Id);
            
            builder.Property(lp => lp.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(lp => lp.Description)
                .HasMaxLength(500);
                
            builder.Property(lp => lp.InterestRate)
                .HasPrecision(18, 4)
                .IsRequired();
                
            builder.Property(lp => lp.MinAmount)
                .HasPrecision(18, 2)
                .IsRequired();
                
            builder.Property(lp => lp.MaxAmount)
                .HasPrecision(18, 2)
                .IsRequired();
                
            builder.Property(lp => lp.MinTerm)
                .IsRequired();
                
            builder.Property(lp => lp.MaxTerm)
                .IsRequired();
                
            builder.Property(lp => lp.InterestType)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(lp => lp.RepaymentFrequency)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(lp => lp.ProcessingFeePercentage)
                .HasPrecision(18, 4);
                
            builder.Property(lp => lp.Status)
                .IsRequired()
                .HasMaxLength(20);
                

                
            builder.Property(lp => lp.EligibilityCriteria)
                .HasMaxLength(1000);
                
            builder.Property(lp => lp.CreatedBy)
                .HasMaxLength(100);
                
            builder.Property(lp => lp.LastModifiedBy)
                .HasMaxLength(100);
                
            builder.HasIndex(lp => lp.Name)
                .IsUnique();
        }
    }
}
