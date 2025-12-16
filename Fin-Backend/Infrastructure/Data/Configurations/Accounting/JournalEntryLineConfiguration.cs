using FinTech.Core.Domain.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Data.Configurations.Accounting
{
    public class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
    {
        public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
        {
            builder.ToTable("JournalEntryLines", "accounting");

            builder.HasKey(l => l.Id);
            
            builder.Property(l => l.Id)
                .HasMaxLength(50)
                .IsRequired();
                
            /*
            builder.Property(l => l.LineNumber)
                .IsRequired();
            */
                
            builder.Property(l => l.Description)
                .HasMaxLength(255);
                
            builder.Property(l => l.DebitAmount)
                .HasPrecision(19, 4)
                .IsRequired();
                
            builder.Property(l => l.CreditAmount)
                .HasPrecision(19, 4)
                .IsRequired();
                
            builder.Property(l => l.CreatedBy)
                .HasMaxLength(100)
                .IsRequired();
                
            builder.Property(l => l.CreatedAt)
                .IsRequired();
                
            builder.Property(l => l.LastModifiedBy)
                .HasMaxLength(100);
                
            /*
            builder.Property(l => l.LastModifiedAt);
            */
                
            // Foreign key references
            /*
            builder.Property(l => l.JournalEntryId)
                .HasMaxLength(50)
                .IsRequired();
            */
                
            builder.Property(l => l.AccountId)
                .HasMaxLength(50)
                .IsRequired();
                
            // Relationships
            /*
            builder.HasOne(l => l.JournalEntry)
                .WithMany(j => j.JournalEntryLines)
                .HasForeignKey(l => l.JournalEntryId)
                .OnDelete(DeleteBehavior.Cascade);
            */
                
            builder.HasOne(l => l.Account)
                .WithMany()
                .HasForeignKey(l => l.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Add indexes
            builder.HasIndex(l => l.JournalEntryId);
            
            builder.HasIndex(l => l.AccountId);
        }
    }
}
