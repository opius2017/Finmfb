using FinTech.Core.Domain.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Data.Configurations.Accounting
{
    public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
    {
        public void Configure(EntityTypeBuilder<JournalEntry> builder)
        {
            builder.ToTable("JournalEntries", "accounting");

            builder.HasKey(j => j.Id);
            
            builder.Property(j => j.Id)
                .HasMaxLength(50)
                .IsRequired();
                
            builder.Property(j => j.JournalEntryNumber)
                .HasMaxLength(50)
                .IsRequired();
                
            builder.Property(j => j.Description)
                .HasMaxLength(500)
                .IsRequired();
                
            builder.Property(j => j.EntryDate)
                .IsRequired();
                
            builder.Property(j => j.EntryType)
                .IsRequired();
                
            builder.Property(j => j.Status)
                .IsRequired();
                
            builder.Property(j => j.IsSystemGenerated)
                .HasDefaultValue(false)
                .IsRequired();
                
            builder.Property(j => j.IsRecurring)
                .HasDefaultValue(false)
                .IsRequired();
                
            builder.Property(j => j.CreatedBy)
                .HasMaxLength(100)
                .IsRequired();
                
            builder.Property(j => j.CreatedAt)
                .IsRequired();
                
            builder.Property(j => j.LastModifiedBy)
                .HasMaxLength(100);
                
            /*
            builder.Property(j => j.LastModifiedAt);
                
            builder.Property(j => j.ApprovedBy)
                .HasMaxLength(100);
                
            builder.Property(j => j.ApprovedAt);
                
            builder.Property(j => j.RejectedBy)
                .HasMaxLength(100);
                
            builder.Property(j => j.RejectedAt);
                
            builder.Property(j => j.RejectionReason)
                .HasMaxLength(500);
                
            builder.Property(j => j.PostedBy)
                .HasMaxLength(100);
                
            builder.Property(j => j.PostedAt);
                
            builder.Property(j => j.ReversalReason)
                .HasMaxLength(500);
                
            builder.Property(j => j.ReversedBy)
                .HasMaxLength(100);
                
            builder.Property(j => j.ReversedAt);
            */
                
            /*
            builder.Property(j => j.FinancialPeriodId)
                .HasMaxLength(50)
                .IsRequired();
                
            builder.Property(j => j.ReversalJournalEntryId)
                .HasMaxLength(50);
                
            builder.Property(j => j.OriginalJournalEntryId)
                .HasMaxLength(50);
            */
                
            /*
            // Relationships
            builder.HasOne(j => j.FinancialPeriod)
                .WithMany()
                .HasForeignKey(j => j.FinancialPeriodId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(j => j.ReversalJournalEntry)
                .WithMany()
                .HasForeignKey(j => j.ReversalJournalEntryId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
                
            builder.HasOne(j => j.OriginalJournalEntry)
                .WithMany()
                .HasForeignKey(j => j.OriginalJournalEntryId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
                
            // Navigation for journal entry lines
            builder.HasMany(j => j.JournalEntryLines)
                .WithOne(l => l.JournalEntry)
                .HasForeignKey(l => l.JournalEntryId)
                .OnDelete(DeleteBehavior.Cascade);
            */
                
            // Add indexes
            builder.HasIndex(j => j.JournalEntryNumber)
                .IsUnique();
                
            builder.HasIndex(j => j.FinancialPeriodId);
            
            builder.HasIndex(j => j.EntryDate);
            
            builder.HasIndex(j => j.Status);
            
            builder.HasIndex(j => j.EntryType);
        }
    }
}
