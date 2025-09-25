using FinTech.Core.Domain.Entities.Loan;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class LoanApplicationConfiguration : IEntityTypeConfiguration<LoanApplication>
    {
        public void Configure(EntityTypeBuilder<LoanApplication> builder)
        {
            builder.ToTable("LoanApplications", "loans");

            builder.HasKey(la => la.Id);
            
            builder.Property(la => la.CustomerId)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(la => la.LoanProductId)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(la => la.RequestedAmount)
                .HasPrecision(18, 2)
                .IsRequired();
                
            builder.Property(la => la.RequestedTerm)
                .IsRequired();
                
            builder.Property(la => la.Purpose)
                .HasMaxLength(500);
                
            builder.Property(la => la.Status)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(la => la.ApprovedAmount)
                .HasPrecision(18, 2);
                
            builder.Property(la => la.ApprovedTerm);
                
            builder.Property(la => la.InterestRate)
                .HasPrecision(18, 4);
                
            builder.Property(la => la.ApprovedBy)
                .HasMaxLength(100);
                
            builder.Property(la => la.RejectedBy)
                .HasMaxLength(100);
                
            builder.Property(la => la.RejectionReason)
                .HasMaxLength(500);
                
            builder.Property(la => la.CreatedBy)
                .HasMaxLength(100);
                
            builder.Property(la => la.LastModifiedBy)
                .HasMaxLength(100);
                
            builder.HasIndex(la => new { la.CustomerId, la.ApplicationDate });
            builder.HasIndex(la => la.Status);
            
            // Relationships
            builder.HasOne<LoanProduct>()
                .WithMany()
                .HasForeignKey(la => la.LoanProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
