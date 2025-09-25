using FinTech.Core.Domain.Entities.RegulatoryReporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class RegulatoryReportScheduleConfiguration : IEntityTypeConfiguration<RegulatoryReportSchedule>
    {
        public void Configure(EntityTypeBuilder<RegulatoryReportSchedule> builder)
        {
            builder.ToTable("RegulatoryReportSchedules");

            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.ScheduleName)
                .IsRequired()
                .HasMaxLength(200);
                
            builder.Property(e => e.Description)
                .HasMaxLength(500);
                
            builder.Property(e => e.Frequency)
                .IsRequired();
                
            builder.Property(e => e.StartDate)
                .IsRequired();
                
            builder.Property(e => e.EndDate)
                .IsRequired(false);
                
            builder.Property(e => e.PreparationLeadTimeDays)
                .HasDefaultValue(14);
                
            builder.Property(e => e.FirstReminderDays)
                .HasDefaultValue(7);
                
            builder.Property(e => e.SecondReminderDays)
                .HasDefaultValue(3);
                
            builder.Property(e => e.EscalationDays)
                .HasDefaultValue(1);
                
            builder.Property(e => e.IsActive)
                .HasDefaultValue(true);
                
            builder.Property(e => e.LastExecutionDate)
                .IsRequired(false);
                
            builder.Property(e => e.NextExecutionDate)
                .IsRequired(false);
                
            builder.Property(e => e.CronExpression)
                .HasMaxLength(100);
                
            builder.Property(e => e.NotificationEmails)
                .HasMaxLength(1000);
                
            // Relationships
            builder.HasOne(e => e.Template)
                .WithMany()
                .HasForeignKey(e => e.RegulatoryReportTemplateId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Indexes
            builder.HasIndex(e => e.RegulatoryReportTemplateId);
                
            builder.HasIndex(e => new { e.IsActive, e.NextExecutionDate });
        }
    }
}
