using FinTech.Core.Domain.Entities.RegulatoryReporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class RegulatoryReportAttachmentConfiguration : IEntityTypeConfiguration<RegulatoryReportAttachment>
    {
        public void Configure(EntityTypeBuilder<RegulatoryReportAttachment> builder)
        {
            builder.ToTable("RegulatoryReportAttachments");

            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.Property(e => e.OriginalFileName)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.Property(e => e.FileSize)
                .IsRequired();
                
            builder.Property(e => e.MimeType)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(e => e.AttachmentType)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(e => e.Description)
                .HasMaxLength(500);
                
            builder.Property(e => e.StoragePath)
                .IsRequired()
                .HasMaxLength(1000);
                
            builder.Property(e => e.IsSubmittedToRegulator)
                .HasDefaultValue(false);
                
            builder.Property(e => e.UploadedById)
                .HasMaxLength(450);
                
            builder.Property(e => e.UploadTimestamp)
                .IsRequired();
                
            // Relationships
            builder.HasOne(e => e.Submission)
                .WithMany(s => s.Attachments)
                .HasForeignKey(e => e.RegulatoryReportSubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne(e => e.UploadedBy)
                .WithMany()
                .HasForeignKey(e => e.UploadedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
                
            // Indexes
            builder.HasIndex(e => e.RegulatoryReportSubmissionId);
                
            builder.HasIndex(e => new { e.RegulatoryReportSubmissionId, e.AttachmentType });
        }
    }
}
