using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Core.Domain.Entities.Identity;

namespace FinTech.Infrastructure.Data.Configuration.Identity;

public class TenantModuleConfiguration : IEntityTypeConfiguration<TenantModule>
{
    public void Configure(EntityTypeBuilder<TenantModule> builder)
    {
        builder.HasOne(tm => tm.Tenant)
               .WithMany(t => t.TenantModules)
               .HasForeignKey(tm => tm.TenantId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.TenantId, e.Module }).IsUnique();
    }
}
