using FinTech.Core.Domain.Entities.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class LoanGuarantorConfiguration : IEntityTypeConfiguration<LoanGuarantor>
    {
        public void Configure(EntityTypeBuilder<LoanGuarantor> builder)
        {
            builder.ToTable("LoanGuarantors", "loans");

            builder.HasKey(lg => lg.Id);
            
            builder.Property(lg => lg.LoanId)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(lg => lg.GuarantorCustomerId)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(lg => lg.Relationship)
                .HasMaxLength(100);
                
            builder.Property(lg => lg.Comments)
                .HasMaxLength(500);
                
            builder.Property(lg => lg.IsApproved)
                .IsRequired();
                
            builder.Property(lg => lg.ApprovedBy)
                .HasMaxLength(100);
                
            builder.Property(lg => lg.RejectedBy)
                .HasMaxLength(100);
                
            builder.Property(lg => lg.RejectionReason)
                .HasMaxLength(500);
                
            builder.HasIndex(lg => lg.LoanId);
            builder.HasIndex(lg => lg.GuarantorCustomerId);
            builder.HasIndex(lg => lg.IsApproved);
            
            // Relationships
            builder.HasOne<Loan>()
                .WithMany()
                .HasForeignKey(lg => lg.LoanId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
