using FinTech.Core.Domain.Entities.RegulatoryReporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class RegulatoryReportValidationRuleConfiguration : IEntityTypeConfiguration<RegulatoryReportValidationRule>
    {
        public void Configure(EntityTypeBuilder<RegulatoryReportValidationRule> builder)
        {
            builder.ToTable("RegulatoryReportValidationRules");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.RuleCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.RuleName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Description)
                .HasMaxLength(500);

            builder.Property(e => e.Expression)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(e => e.ErrorMessage)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(e => e.Severity)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.HasOne(e => e.Section)
                .WithMany(s => s.ValidationRules)
                .HasForeignKey(e => e.RegulatoryReportSectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
