using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Core.Domain.Entities.Banking;

namespace FinTech.Infrastructure.Data.Configurations.Banking;

public class BankReconciliationConfiguration : IEntityTypeConfiguration<BankReconciliation>
{
    public void Configure(EntityTypeBuilder<BankReconciliation> builder)
    {
        builder.ToTable("BankReconciliations", "Banking");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.BankAccountId)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.BankAccountName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.BankAccountNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.ReconciliationDate)
            .IsRequired();

        builder.Property(e => e.StatementOpeningBalance)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.StatementClosingBalance)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.BookOpeningBalance)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.BookClosingBalance)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.ReconciledBalance)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.Variance)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.ReconciliationReference)
            .HasMaxLength(50);

        builder.Property(e => e.ReconciledBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.ApprovedBy)
            .HasMaxLength(100);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasMany(e => e.ReconciliationItems)
            .WithOne(e => e.BankReconciliation)
            .HasForeignKey(e => e.BankReconciliationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.BankAccountId);
        builder.HasIndex(e => e.ReconciliationDate);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ReconciliationReference);
    }
}
