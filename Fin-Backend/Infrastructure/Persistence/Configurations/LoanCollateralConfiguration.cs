using FinTech.Core.Domain.Entities.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class LoanCollateralConfiguration : IEntityTypeConfiguration<LoanCollateral>
    {
        public void Configure(EntityTypeBuilder<LoanCollateral> builder)
        {
            builder.ToTable("LoanCollaterals", "loans");

            builder.HasKey(lc => lc.Id);
            
/*
            builder.Property(lc => lc.LoanId)
                .IsRequired()
                .HasMaxLength(100);
*/
                
            builder.Property(lc => lc.CollateralType)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(lc => lc.Description)
                .HasMaxLength(500);
                
/*
            builder.Property(lc => lc.Value)
                .HasPrecision(18, 2)
                .IsRequired();
*/
                
/*
            builder.Property(lc => lc.OwnerName)
                .HasMaxLength(150);
*/
                
            builder.Property(lc => lc.RegistrationNumber)
                .HasMaxLength(100);
                
            builder.Property(lc => lc.Location)
                .HasMaxLength(250);
                
            builder.Property(lc => lc.Status)
                .IsRequired()
                .HasMaxLength(50);
                
/*
            builder.Property(lc => lc.ApprovedBy)
                .HasMaxLength(100);
                
            builder.Property(lc => lc.ApprovalComments)
                .HasMaxLength(500);
                
            builder.Property(lc => lc.RejectedBy)
                .HasMaxLength(100);
                
            builder.Property(lc => lc.RejectionReason)
                .HasMaxLength(500);
*/
                
// builder.HasIndex(lc => lc.LoanId);
            builder.HasIndex(lc => lc.Status);
            
            // Relationships
/*
            // Relationships
            builder.HasOne<Loan>()
                .WithMany()
                .HasForeignKey(lc => lc.LoanId)
                .OnDelete(DeleteBehavior.Restrict);
*/
        }
    }
}
