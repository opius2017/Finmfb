using FinTech.Domain.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Data.Configurations.Accounting
{
    public class ChartOfAccountConfiguration : IEntityTypeConfiguration<ChartOfAccount>
    {
        public void Configure(EntityTypeBuilder<ChartOfAccount> builder)
        {
            builder.ToTable("ChartOfAccounts", "accounting");

            builder.HasKey(c => c.Id);
            
            builder.Property(c => c.Id)
                .HasMaxLength(50)
                .IsRequired();
                
            builder.Property(c => c.AccountNumber)
                .HasMaxLength(50)
                .IsRequired();
                
            builder.Property(c => c.AccountName)
                .HasMaxLength(255)
                .IsRequired();
                
            builder.Property(c => c.Description)
                .HasMaxLength(500);
                
            builder.Property(c => c.AccountType)
                .IsRequired();
                
            builder.Property(c => c.AccountClassification)
                .IsRequired();
                
            builder.Property(c => c.Status)
                .IsRequired();
                
            builder.Property(c => c.IsSystemAccount)
                .HasDefaultValue(false)
                .IsRequired();
                
            builder.Property(c => c.AllowManualEntry)
                .HasDefaultValue(true)
                .IsRequired();
                
            builder.Property(c => c.IsControlAccount)
                .HasDefaultValue(false)
                .IsRequired();
                
            builder.Property(c => c.RequiresReconciliation)
                .HasDefaultValue(false)
                .IsRequired();
                
            builder.Property(c => c.CreatedBy)
                .HasMaxLength(100)
                .IsRequired();
                
            builder.Property(c => c.CreatedAt)
                .IsRequired();
                
            builder.Property(c => c.LastModifiedBy)
                .HasMaxLength(100);
                
            builder.Property(c => c.LastModifiedAt);
                
            // Add indexes
            builder.HasIndex(c => c.AccountNumber)
                .IsUnique();
                
            builder.HasIndex(c => c.AccountType);
            
            builder.HasIndex(c => c.AccountClassification);
            
            builder.HasIndex(c => c.Status);
        }
    }
}