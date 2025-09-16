using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Domain.Entities.Identity;

namespace FinTech.Infrastructure.Data.Configuration.Identity;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasIndex(e => new { e.TenantId, e.RoleName }).IsUnique();
    }
}