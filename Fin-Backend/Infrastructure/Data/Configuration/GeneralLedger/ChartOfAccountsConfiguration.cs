using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Domain.Entities.GeneralLedger;

namespace FinTech.Infrastructure.Data.Configuration.GeneralLedger;

public class ChartOfAccountsConfiguration : IEntityTypeConfiguration<ChartOfAccounts>
{
    public void Configure(EntityTypeBuilder<ChartOfAccounts> builder)
    {
        builder.HasIndex(e => new { e.TenantId, e.AccountCode }).IsUnique();
        
        builder.HasOne(c => c.ParentAccount)
               .WithMany(c => c.SubAccounts)
               .HasForeignKey(c => c.ParentAccountId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}