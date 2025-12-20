using FinTech.Core.Domain.Entities.RegulatoryReporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class RegulatoryReportSectionConfiguration : IEntityTypeConfiguration<RegulatoryReportSection>
    {
        public void Configure(EntityTypeBuilder<RegulatoryReportSection> builder)
        {
            builder.ToTable("RegulatoryReportSections");

            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.SectionCode)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(e => e.SectionName)
                .IsRequired()
                .HasMaxLength(200);
                
            builder.Property(e => e.Description)
                .HasMaxLength(500);
                
            builder.Property(e => e.DisplayOrder)
                .HasDefaultValue(0);
                
            builder.Property(e => e.IsTable)
                .HasDefaultValue(false);
                
            builder.Property(e => e.RowCount)
                .IsRequired(false);
                
            builder.Property(e => e.Instructions)
                .HasMaxLength(1000);
                
            builder.Property(e => e.IsRequired)
                .HasDefaultValue(false);
                
            builder.Property(e => e.IsHidden)
                .HasDefaultValue(false);
                
            builder.Property(e => e.Metadata)
                .HasColumnType("nvarchar(max)");
                
            builder.Property(e => e.ParentSectionId)
                .IsRequired(false);
                
            // Relationships
            builder.HasOne(e => e.Template)
                .WithMany(t => t.Sections)
                .HasForeignKey(e => e.RegulatoryReportTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne(e => e.ParentSection)
                .WithMany(s => s.ChildSections)
                .HasForeignKey(e => e.ParentSectionId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
                
            // Indexes
            builder.HasIndex(e => e.RegulatoryReportTemplateId);
                
            builder.HasIndex(e => new { e.RegulatoryReportTemplateId, e.SectionCode })
                .IsUnique();
                
            builder.HasIndex(e => new { e.RegulatoryReportTemplateId, e.DisplayOrder });
        }
    }
}
