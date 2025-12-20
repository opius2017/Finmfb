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
                .WithMany(t => t.Submissions) // Assuming Submissions collection exists on template?
                .HasForeignKey(e => e.RegulatoryReportTemplateId)
                .OnDelete(DeleteBehavior.Restrict);
                

                
            // Indexes
            builder.HasIndex(e => e.ReferenceNumber)
                .IsUnique();
                
            builder.HasIndex(e => new { e.Status, e.DueDate });
                
            builder.HasIndex(e => e.RegulatoryReportTemplateId);
        }
    }
}
