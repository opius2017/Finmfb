using FinTech.Core.Domain.Entities.RegulatoryReporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class RegulatoryReportSubmissionConfiguration : IEntityTypeConfiguration<RegulatoryReportSubmission>
    {
        public void Configure(EntityTypeBuilder<RegulatoryReportSubmission> builder)
        {
            builder.ToTable("RegulatoryReportSubmissions");

            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.ReferenceNumber)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(e => e.Status)
                .IsRequired();
                
            builder.Property(e => e.ReportingPeriodStart)
                .IsRequired();
                
            builder.Property(e => e.ReportingPeriodEnd)
                .IsRequired();
                
            builder.Property(e => e.DueDate)
                .IsRequired();
                
            builder.Property(e => e.SubmissionDate)
                .IsRequired(false);
                
            builder.Property(e => e.AcknowledgementDate)
                .IsRequired(false);
                
            builder.Property(e => e.AcknowledgementReference)
                .HasMaxLength(100);
                
            builder.Property(e => e.RegulatoryComments)
                .HasMaxLength(1000);
                
            builder.Property(e => e.InternalComments)
                .HasMaxLength(1000);
                
            builder.Property(e => e.PreparedById)
                .HasMaxLength(450);
                
            builder.Property(e => e.ReviewedById)
                .HasMaxLength(450);
                
            builder.Property(e => e.ApprovedById)
                .HasMaxLength(450);
                
            builder.Property(e => e.SubmittedById)
                .HasMaxLength(450);
                
            // Relationships
            builder.HasOne(e => e.Template)
                .WithMany()
                .HasForeignKey(e => e.RegulatoryReportTemplateId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(e => e.PreparedBy)
                .WithMany()
                .HasForeignKey(e => e.PreparedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
                
            builder.HasOne(e => e.ReviewedBy)
                .WithMany()
                .HasForeignKey(e => e.ReviewedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
                
            builder.HasOne(e => e.ApprovedBy)
                .WithMany()
                .HasForeignKey(e => e.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
                
            builder.HasOne(e => e.SubmittedBy)
                .WithMany()
                .HasForeignKey(e => e.SubmittedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
                
            // Indexes
            builder.HasIndex(e => e.ReferenceNumber)
                .IsUnique();
                
            builder.HasIndex(e => new { e.Status, e.DueDate });
                
            builder.HasIndex(e => e.RegulatoryReportTemplateId);
        }
    }
}
