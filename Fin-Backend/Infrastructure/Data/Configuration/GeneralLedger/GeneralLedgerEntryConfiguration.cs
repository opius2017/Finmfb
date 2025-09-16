using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Domain.Entities.GeneralLedger;

namespace FinTech.Infrastructure.Data.Configuration.GeneralLedger;

public class GeneralLedgerEntryConfiguration : IEntityTypeConfiguration<GeneralLedgerEntry>
{
    public void Configure(EntityTypeBuilder<GeneralLedgerEntry> builder)
    {
        builder.HasOne(g => g.Account)
               .WithMany(a => a.GeneralLedgerEntries)
               .HasForeignKey(g => g.AccountId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.TransactionReference);
        builder.HasIndex(e => e.TransactionDate);
    }
}