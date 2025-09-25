using FinTech.Core.Domain.Entities.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class LoanCreditCheckConfiguration : IEntityTypeConfiguration<LoanCreditCheck>
    {
        public void Configure(EntityTypeBuilder<LoanCreditCheck> builder)
        {
            builder.ToTable("LoanCreditChecks", "loans");

            builder.HasKey(lcc => lcc.Id);
            
            builder.Property(lcc => lcc.LoanId)
                .HasMaxLength(100);
                
            builder.Property(lcc => lcc.CustomerId)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(lcc => lcc.CreditBureauId)
                .HasMaxLength(100);
                
            builder.Property(lcc => lcc.Status)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(lcc => lcc.CreditScore)
                .HasPrecision(10, 2);
                
            builder.Property(lcc => lcc.MaxEligibleAmount)
                .HasPrecision(18, 2);
                
            builder.Property(lcc => lcc.ExistingLoans)
                .HasPrecision(6, 0);
                
            builder.Property(lcc => lcc.OutstandingDebt)
                .HasPrecision(18, 2);
                
            builder.Property(lcc => lcc.ReferenceNumber)
                .HasMaxLength(100);
                
            builder.Property(lcc => lcc.CheckedBy)
                .HasMaxLength(100);
                
            builder.Property(lcc => lcc.Comments)
                .HasMaxLength(1000);
                
            builder.HasIndex(lcc => lcc.LoanId);
            builder.HasIndex(lcc => lcc.CustomerId);
            builder.HasIndex(lcc => lcc.Status);
            builder.HasIndex(lcc => lcc.CheckDate);
            
            // Relationships
            builder.HasOne<Loan>()
                .WithMany()
                .HasForeignKey(lcc => lcc.LoanId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }
}
