using FinTech.Domain.Entities.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class LoanDocumentConfiguration : IEntityTypeConfiguration<LoanDocument>
    {
        public void Configure(EntityTypeBuilder<LoanDocument> builder)
        {
            builder.ToTable("LoanDocuments", "loans");

            builder.HasKey(ld => ld.Id);
            
            builder.Property(ld => ld.LoanId)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(ld => ld.DocumentType)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(ld => ld.FileName)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.Property(ld => ld.FileSize)
                .IsRequired();
                
            builder.Property(ld => ld.ContentType)
                .HasMaxLength(100);
                
            builder.Property(ld => ld.Description)
                .HasMaxLength(500);
                
            builder.Property(ld => ld.UploadedBy)
                .HasMaxLength(100);
                
            builder.Property(ld => ld.Status)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(ld => ld.VerifiedBy)
                .HasMaxLength(100);
                
            builder.Property(ld => ld.RejectedBy)
                .HasMaxLength(100);
                
            builder.Property(ld => ld.RejectionReason)
                .HasMaxLength(500);
                
            builder.HasIndex(ld => ld.LoanId);
            builder.HasIndex(ld => ld.DocumentType);
            
            // Relationships
            builder.HasOne<Loan>()
                .WithMany()
                .HasForeignKey(ld => ld.LoanId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}