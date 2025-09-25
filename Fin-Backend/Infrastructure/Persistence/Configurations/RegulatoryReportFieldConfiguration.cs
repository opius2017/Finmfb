using FinTech.Core.Domain.Entities.RegulatoryReporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class RegulatoryReportFieldConfiguration : IEntityTypeConfiguration<RegulatoryReportField>
    {
        public void Configure(EntityTypeBuilder<RegulatoryReportField> builder)
        {
            builder.ToTable("RegulatoryReportFields");

            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.FieldCode)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(e => e.FieldName)
                .IsRequired()
                .HasMaxLength(200);
                
            builder.Property(e => e.Description)
                .HasMaxLength(500);
                
            builder.Property(e => e.DataType)
                .IsRequired();
                
            builder.Property(e => e.DisplayOrder)
                .HasDefaultValue(0);
                
            builder.Property(e => e.DefaultValue)
                .HasMaxLength(500);
                
            builder.Property(e => e.Placeholder)
                .HasMaxLength(200);
                
            builder.Property(e => e.HelpText)
                .HasMaxLength(500);
                
            builder.Property(e => e.IsRequired)
                .HasDefaultValue(false);
                
            builder.Property(e => e.IsReadOnly)
                .HasDefaultValue(false);
                
            builder.Property(e => e.IsHidden)
                .HasDefaultValue(false);
                
            builder.Property(e => e.MinValue)
                .HasMaxLength(50);
                
            builder.Property(e => e.MaxValue)
                .HasMaxLength(50);
                
            builder.Property(e => e.ValidationPattern)
                .HasMaxLength(200);
                
            builder.Property(e => e.ValidationMessage)
                .HasMaxLength(200);
                
            builder.Property(e => e.Options)
                .HasColumnType("nvarchar(max)");
                
            builder.Property(e => e.Formula)
                .HasMaxLength(1000);
                
            builder.Property(e => e.Format)
                .HasMaxLength(50);
                
            builder.Property(e => e.Unit)
                .HasMaxLength(20);
                
            builder.Property(e => e.DecimalPlaces)
                .IsRequired(false);
                
            builder.Property(e => e.ShowUnit)
                .HasDefaultValue(false);
                
            builder.Property(e => e.Metadata)
                .HasColumnType("nvarchar(max)");
                
            builder.Property(e => e.RowIndex)
                .IsRequired(false);
                
            builder.Property(e => e.ColumnIndex)
                .IsRequired(false);
                
            builder.Property(e => e.ColumnSpan)
                .HasDefaultValue(1);
                
            builder.Property(e => e.RowSpan)
                .HasDefaultValue(1);
                
            // Relationships
            builder.HasOne(e => e.Section)
                .WithMany(s => s.Fields)
                .HasForeignKey(e => e.RegulatoryReportSectionId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Indexes
            builder.HasIndex(e => e.RegulatoryReportSectionId);
                
            builder.HasIndex(e => new { e.RegulatoryReportSectionId, e.FieldCode })
                .IsUnique();
                
            builder.HasIndex(e => new { e.RegulatoryReportSectionId, e.DisplayOrder });
                
            builder.HasIndex(e => new { e.RegulatoryReportSectionId, e.RowIndex, e.ColumnIndex });
        }
    }
}
