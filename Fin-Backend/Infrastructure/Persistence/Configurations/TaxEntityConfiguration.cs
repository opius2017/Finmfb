using FinTech.Core.Domain.Entities.Tax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Entity Framework configuration for tax entities
    /// </summary>
    public class TaxEntityConfiguration : 
        IEntityTypeConfiguration<TaxType>,
        IEntityTypeConfiguration<TaxRate>,
        IEntityTypeConfiguration<TaxTransaction>,
        IEntityTypeConfiguration<TaxExemption>
    {
        /// <summary>
        /// Configure TaxType entity
        /// </summary>
        public void Configure(EntityTypeBuilder<TaxType> builder)
        {
            builder.ToTable("TaxTypes");
            
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(50);
            
            builder.Property(t => t.Code)
                .IsRequired()
                .HasMaxLength(20);
            
            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(t => t.Description)
                .HasMaxLength(500);
            
            builder.Property(t => t.LiabilityAccountId)
                .HasMaxLength(50);
            
            builder.Property(t => t.ReceivableAccountId)
                .HasMaxLength(50);
            
            builder.Property(t => t.RegulatoryAuthority)
                .HasMaxLength(100);
            
            // Create unique index on Code
            builder.HasIndex(t => t.Code)
                .IsUnique();
        }

        /// <summary>
        /// Configure TaxRate entity
        /// </summary>
        public void Configure(EntityTypeBuilder<TaxRate> builder)
        {
            builder.ToTable("TaxRates");
            
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).HasMaxLength(50);
            
            builder.Property(r => r.TaxTypeId)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(r => r.Rate)
                .HasPrecision(10, 4);
            
            builder.Property(r => r.MinimumAmount)
                .HasPrecision(18, 2);
            
            builder.Property(r => r.MaximumAmount)
                .HasPrecision(18, 2);
            
            builder.Property(r => r.ApplicableCategory)
                .HasMaxLength(50);
            
            // Create index on TaxTypeId for faster lookup
            builder.HasIndex(r => r.TaxTypeId);
            
            // Create index on EffectiveDate for faster lookup
            builder.HasIndex(r => r.EffectiveDate);
        }

        /// <summary>
        /// Configure TaxTransaction entity
        /// </summary>
        public void Configure(EntityTypeBuilder<TaxTransaction> builder)
        {
            builder.ToTable("TaxTransactions");
            
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasMaxLength(50);
            
            builder.Property(t => t.TaxTypeId)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(t => t.TaxRateId)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(t => t.TransactionReference)
                .HasMaxLength(100);
            
            builder.Property(t => t.FinancialPeriodId)
                .HasMaxLength(50);
            
            builder.Property(t => t.TaxableAmount)
                .HasPrecision(18, 2);
            
            builder.Property(t => t.TaxAmount)
                .HasPrecision(18, 2);
            
            builder.Property(t => t.Category)
                .HasMaxLength(50);
            
            builder.Property(t => t.PartyId)
                .HasMaxLength(50);
            
            builder.Property(t => t.PartyName)
                .HasMaxLength(200);
            
            builder.Property(t => t.Description)
                .HasMaxLength(500);
            
            builder.Property(t => t.JournalEntryId)
                .HasMaxLength(50);
            
            // Create index on TaxTypeId for faster lookup
            builder.HasIndex(t => t.TaxTypeId);
            
            // Create index on TransactionDate for faster lookup
            builder.HasIndex(t => t.TransactionDate);
            
            // Create index on FinancialPeriodId for faster lookup
            builder.HasIndex(t => t.FinancialPeriodId);
        }

        /// <summary>
        /// Configure TaxExemption entity
        /// </summary>
        public void Configure(EntityTypeBuilder<TaxExemption> builder)
        {
            builder.ToTable("TaxExemptions");
            
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasMaxLength(50);
            
            builder.Property(e => e.PartyId)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(e => e.TaxTypeId)
                .HasMaxLength(50);
            
            builder.Property(e => e.Reason)
                .IsRequired()
                .HasMaxLength(200);
            
            builder.Property(e => e.ExemptionCertificateNumber)
                .HasMaxLength(100);
            
            builder.Property(e => e.ApprovingAuthority)
                .HasMaxLength(100);
            
            builder.Property(e => e.Notes)
                .HasMaxLength(500);
            
            // Create index on PartyId for faster lookup
            builder.HasIndex(e => e.PartyId);
            
            // Create index on TaxTypeId for faster lookup
            builder.HasIndex(e => e.TaxTypeId);
        }
    }
}
