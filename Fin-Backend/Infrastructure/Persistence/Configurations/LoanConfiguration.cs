using FinTech.Core.Domain.Entities.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class LoanConfiguration : IEntityTypeConfiguration<Loan>
    {
        public void Configure(EntityTypeBuilder<Loan> builder)
        {
            builder.ToTable("Loans", "loans");

            builder.HasKey(l => l.Id);
            builder.Property(l => l.Id).HasMaxLength(100);
            
            builder.Property(l => l.CustomerId)
                .IsRequired();
                
            builder.Property(l => l.LoanApplicationId)
                .IsRequired();
                
            builder.Property(l => l.LoanProductId)
                .IsRequired();
                
/*
            builder.Property(l => l.LoanAmount)
                .HasPrecision(18, 2)
                .IsRequired();
                
            builder.Property(l => l.LoanTerm)
                .IsRequired();
*/
                
            builder.Property(l => l.InterestRate)
                .HasPrecision(18, 4)
                .IsRequired();
                
/*
            builder.Property(l => l.InterestType)
                .IsRequired()
                .HasMaxLength(50);
*/
                
            builder.Property(l => l.RepaymentFrequency)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(l => l.Status)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(l => l.OutstandingPrincipal)
                .HasPrecision(18, 2)
                .IsRequired();
                
            builder.Property(l => l.OutstandingInterest)
                .HasPrecision(18, 2)
                .IsRequired();
                
/*
            builder.Property(l => l.AccountNumber)
                .HasMaxLength(50);
                
            builder.Property(l => l.Purpose)
                .HasMaxLength(500);
*/
                
            builder.Property(l => l.CreatedBy)
                .HasMaxLength(100);
                
            builder.Property(l => l.LastModifiedBy)
                .HasMaxLength(100);
                
/*
            builder.Property(l => l.ApprovedBy)
                .HasMaxLength(100);
                
            builder.Property(l => l.DisbursedBy)
                .HasMaxLength(100);
                
            builder.Property(l => l.ClosureReason)
                .HasMaxLength(500);
*/
                
            builder.HasIndex(l => l.CustomerId);
            builder.HasIndex(l => l.Status);
// builder.HasIndex(l => l.AccountNumber).IsUnique();
            
            // Relationships
            builder.HasOne<LoanProduct>()
                .WithMany()
                .HasForeignKey(l => l.LoanProductId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne<LoanApplication>()
                .WithMany()
                .HasForeignKey(l => l.LoanApplicationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
