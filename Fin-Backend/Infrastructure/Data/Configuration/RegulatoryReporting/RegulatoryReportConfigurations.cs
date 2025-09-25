using FinTech.Core.Domain.Entities.RegulatoryReporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Data.Configuration.RegulatoryReporting
{
    public class RegulatoryReportTemplateConfiguration : IEntityTypeConfiguration<RegulatoryReportTemplate>
    {
        public void Configure(EntityTypeBuilder<RegulatoryReportTemplate> builder)
        {
            builder.ToTable("RegulatoryReportTemplates");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.TemplateName)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.Property(e => e.TemplateCode)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(e => e.Description)
                .HasMaxLength(1000);
                
            builder.Property(e => e.RegulatoryBody)
                .IsRequired();
                
            builder.Property(e => e.Frequency)
                .IsRequired();
                
            builder.Property(e => e.FileFormat)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(e => e.SchemaVersion)
                .HasMaxLength(20);
                
            builder.Property(e => e.TemplateStructure)
                .HasColumnType("nvarchar(max)");
                
            // Relationships
            builder.HasMany(e => e.Sections)
                .WithOne(e => e.ReportTemplate)
                .HasForeignKey(e => e.ReportTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasMany(e => e.Submissions)
                .WithOne(e => e.ReportTemplate)
                .HasForeignKey(e => e.ReportTemplateId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
    
    public class RegulatoryReportSectionConfiguration : IEntityTypeConfiguration<RegulatoryReportSection>
    {
        public void Configure(EntityTypeBuilder<RegulatoryReportSection> builder)
        {
            builder.ToTable("RegulatoryReportSections");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.SectionName)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.Property(e => e.SectionCode)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(e => e.Description)
                .HasMaxLength(1000);
                
            builder.Property(e => e.ValidationRules)
                .HasColumnType("nvarchar(max)");
                
            // Relationships
            builder.HasMany(e => e.Fields)
                .WithOne(e => e.Section)
                .HasForeignKey(e => e.SectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class RegulatoryReportFieldConfiguration : IEntityTypeConfiguration<RegulatoryReportField>
    {
        public void Configure(EntityTypeBuilder<RegulatoryReportField> builder)
        {
            builder.ToTable("RegulatoryReportFields");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.FieldName)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.Property(e => e.FieldCode)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(e => e.Description)
                .HasMaxLength(1000);
                
            builder.Property(e => e.DataType)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(e => e.ValidationRules)
                .HasColumnType("nvarchar(max)");
                
            builder.Property(e => e.DefaultValue)
                .HasMaxLength(255);
                
            builder.Property(e => e.Formula)
                .HasColumnType("nvarchar(max)");
                
            builder.Property(e => e.MappingQuery)
                .HasColumnType("nvarchar(max)");
        }
    }
    
    public class RegulatoryReportSubmissionConfiguration : IEntityTypeConfiguration<RegulatoryReportSubmission>
    {
        public void Configure(EntityTypeBuilder<RegulatoryReportSubmission> builder)
        {
            builder.ToTable("RegulatoryReportSubmissions");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.ReportingPeriodStart)
                .IsRequired();
                
            builder.Property(e => e.ReportingPeriodEnd)
                .IsRequired();
                
            builder.Property(e => e.SubmissionDate)
                .IsRequired();
                
            builder.Property(e => e.Status)
                .IsRequired();
                
            builder.Property(e => e.Comments)
                .HasMaxLength(2000);
                
            builder.Property(e => e.SubmissionReference)
                .HasMaxLength(100);
                
            builder.Property(e => e.FilePath)
                .HasMaxLength(500);
                
            // Relationships
            builder.HasMany(e => e.ReportData)
                .WithOne(e => e.Submission)
                .HasForeignKey(e => e.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class RegulatoryReportDataConfiguration : IEntityTypeConfiguration<RegulatoryReportData>
    {
        public void Configure(EntityTypeBuilder<RegulatoryReportData> builder)
        {
            builder.ToTable("RegulatoryReportData");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Value)
                .HasColumnType("nvarchar(max)");
                
            builder.Property(e => e.Comments)
                .HasMaxLength(1000);
                
            builder.Property(e => e.ExceptionReason)
                .HasMaxLength(1000);
                
            // Relationship with Field
            builder.HasOne(e => e.Field)
                .WithMany()
                .HasForeignKey(e => e.FieldId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
    
    public class RegulatoryReportValidationConfiguration : IEntityTypeConfiguration<RegulatoryReportValidation>
    {
        public void Configure(EntityTypeBuilder<RegulatoryReportValidation> builder)
        {
            builder.ToTable("RegulatoryReportValidations");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.ErrorMessage)
                .IsRequired()
                .HasMaxLength(1000);
                
            builder.Property(e => e.ErrorCode)
                .HasMaxLength(50);
                
            builder.Property(e => e.Severity)
                .IsRequired();
                
            builder.Property(e => e.ResolutionComments)
                .HasMaxLength(1000);
                
            // Relationship with Field (optional)
            builder.HasOne(e => e.Field)
                .WithMany()
                .HasForeignKey(e => e.FieldId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }
    
    public class RegulatoryReportScheduleConfiguration : IEntityTypeConfiguration<RegulatoryReportSchedule>
    {
        public void Configure(EntityTypeBuilder<RegulatoryReportSchedule> builder)
        {
            builder.ToTable("RegulatoryReportSchedules");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.NextGenerationDate)
                .IsRequired();
                
            builder.Property(e => e.NextSubmissionDeadline)
                .IsRequired();
                
            builder.Property(e => e.NotificationEmails)
                .HasMaxLength(500);
        }
    }
}
