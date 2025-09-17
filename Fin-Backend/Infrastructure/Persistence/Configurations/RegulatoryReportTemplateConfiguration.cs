using FinTech.Domain.Entities.RegulatoryReporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class RegulatoryReportTemplateConfiguration : IEntityTypeConfiguration<RegulatoryReportTemplate>
    {
        public void Configure(EntityTypeBuilder<RegulatoryReportTemplate> builder)
        {
            builder.ToTable("RegulatoryReportTemplates");

            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.TemplateName)
                .IsRequired()
                .HasMaxLength(200);
                
            builder.Property(e => e.TemplateCode)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(e => e.Description)
                .HasMaxLength(500);
                
            builder.Property(e => e.RegulatoryBody)
                .IsRequired();
                
            builder.Property(e => e.Frequency)
                .IsRequired();
                
            builder.Property(e => e.FileFormat)
                .HasMaxLength(50);
                
            builder.Property(e => e.IsActive)
                .HasDefaultValue(true);
                
            builder.Property(e => e.SchemaVersion)
                .HasMaxLength(20);
                
            builder.Property(e => e.TemplateStructure)
                .HasColumnType("nvarchar(max)");
                
            // Add index on template code for faster lookups
            builder.HasIndex(e => e.TemplateCode)
                .IsUnique();
                
            // Add index on regulatory body and frequency for filtering
            builder.HasIndex(e => new { e.RegulatoryBody, e.Frequency });
        }
    }
}