using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Core.Domain.Entities.Banking;

namespace FinTech.Infrastructure.Data.Configurations.Banking;

public class BankStatementConfiguration : IEntityTypeConfiguration<BankStatement>
{
    public void Configure(EntityTypeBuilder<BankStatement> builder)
    {
        builder.ToTable("BankStatements", "Banking");

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

        builder.Property(e => e.OpeningBalance)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.ClosingBalance)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.TotalDebits)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.TotalCredits)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.StatementReference)
            .HasMaxLength(50);

        builder.Property(e => e.FileName)
            .HasMaxLength(255);

        builder.Property(e => e.FileUrl)
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.ImportedBy)
            .HasMaxLength(100)
            .IsRequired();

        // Relationships
        builder.HasMany(e => e.StatementLines)
            .WithOne(e => e.BankStatement)
            .HasForeignKey(e => e.BankStatementId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.BankAccountId);
        builder.HasIndex(e => e.StatementDate);
        builder.HasIndex(e => e.Status);
    }
}
