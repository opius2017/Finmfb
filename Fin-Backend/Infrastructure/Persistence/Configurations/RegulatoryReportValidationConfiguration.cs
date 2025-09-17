using FinTech.Domain.Entities.RegulatoryReporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class RegulatoryReportValidationConfiguration : IEntityTypeConfiguration<RegulatoryReportValidation>
    {
        public void Configure(EntityTypeBuilder<RegulatoryReportValidation> builder)
        {
            builder.ToTable("RegulatoryReportValidations");

            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.RuleCode)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(e => e.RuleDescription)
                .HasMaxLength(500);
                
            builder.Property(e => e.SectionCode)
                .HasMaxLength(50);
                
            builder.Property(e => e.FieldCode)
                .HasMaxLength(50);
                
            builder.Property(e => e.Severity)
                .IsRequired();
                
            builder.Property(e => e.Message)
                .IsRequired()
                .HasMaxLength(1000);
                
            builder.Property(e => e.ExpectedValue)
                .HasMaxLength(500);
                
            builder.Property(e => e.ActualValue)
                .HasMaxLength(500);
                
            builder.Property(e => e.ValidationTimestamp)
                .IsRequired();
                
            builder.Property(e => e.IsResolved)
                .HasDefaultValue(false);
                
            builder.Property(e => e.ResolvedTimestamp)
                .IsRequired(false);
                
            builder.Property(e => e.ResolutionComments)
                .HasMaxLength(1000);
                
            // Relationships
            builder.HasOne(e => e.Submission)
                .WithMany(s => s.ValidationResults)
                .HasForeignKey(e => e.RegulatoryReportSubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne(e => e.ReportData)
                .WithMany()
                .HasForeignKey(e => e.RegulatoryReportDataId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
                
            // Indexes
            builder.HasIndex(e => e.RegulatoryReportSubmissionId);
                
            builder.HasIndex(e => new { e.RegulatoryReportSubmissionId, e.Severity });
                
            builder.HasIndex(e => new { e.RegulatoryReportSubmissionId, e.IsResolved });
        }
    }
}