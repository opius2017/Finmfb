using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Domain.Entities.Identity;

namespace FinTech.Infrastructure.Data.Configuration.Identity;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasIndex(e => e.PermissionName).IsUnique();
        builder.HasIndex(e => new { e.Module, e.Resource, e.Action }).IsUnique();
    }
}