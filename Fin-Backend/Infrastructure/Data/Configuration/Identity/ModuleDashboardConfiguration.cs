using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Core.Domain.Entities.Identity;

namespace FinTech.Infrastructure.Data.Configuration.Identity;

public class ModuleDashboardConfiguration : IEntityTypeConfiguration<ModuleDashboard>
{
    public void Configure(EntityTypeBuilder<ModuleDashboard> builder)
    {
        builder.HasIndex(e => new { e.TenantId, e.ModuleName, e.DashboardName }).IsUnique();
    }
}
