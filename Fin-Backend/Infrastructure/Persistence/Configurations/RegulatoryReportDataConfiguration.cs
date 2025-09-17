using FinTech.Domain.Entities.RegulatoryReporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class RegulatoryReportDataConfiguration : IEntityTypeConfiguration<RegulatoryReportData>
    {
        public void Configure(EntityTypeBuilder<RegulatoryReportData> builder)
        {
            builder.ToTable("RegulatoryReportData");

            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.SectionCode)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(e => e.SectionName)
                .IsRequired()
                .HasMaxLength(200);
                
            builder.Property(e => e.FieldCode)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(e => e.FieldName)
                .IsRequired()
                .HasMaxLength(200);
                
            builder.Property(e => e.RowIndex)
                .IsRequired(false);
                
            builder.Property(e => e.ColumnIndex)
                .IsRequired(false);
                
            builder.Property(e => e.RawValue)
                .HasMaxLength(4000);
                
            builder.Property(e => e.DataType)
                .HasMaxLength(50);
                
            builder.Property(e => e.NumericValue)
                .IsRequired(false)
                .HasColumnType("decimal(28, 8)");
                
            builder.Property(e => e.DateValue)
                .IsRequired(false);
                
            builder.Property(e => e.IsCalculated)
                .HasDefaultValue(false);
                
            builder.Property(e => e.CalculationFormula)
                .HasMaxLength(1000);
                
            builder.Property(e => e.Metadata)
                .HasColumnType("nvarchar(max)");
                
            builder.Property(e => e.Comments)
                .HasMaxLength(1000);
                
            builder.Property(e => e.HasValidationErrors)
                .HasDefaultValue(false);
                
            // Relationships
            builder.HasOne(e => e.Submission)
                .WithMany(s => s.ReportData)
                .HasForeignKey(e => e.RegulatoryReportSubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Indexes
            builder.HasIndex(e => e.RegulatoryReportSubmissionId);
                
            builder.HasIndex(e => new { e.RegulatoryReportSubmissionId, e.SectionCode, e.FieldCode });
                
            builder.HasIndex(e => new { e.RegulatoryReportSubmissionId, e.HasValidationErrors });
        }
    }
}