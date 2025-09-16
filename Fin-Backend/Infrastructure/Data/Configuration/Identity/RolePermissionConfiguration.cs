using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Domain.Entities.Identity;

namespace FinTech.Infrastructure.Data.Configuration.Identity;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasOne(rp => rp.Role)
               .WithMany(r => r.RolePermissions)
               .HasForeignKey(rp => rp.RoleId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Permission)
               .WithMany(p => p.RolePermissions)
               .HasForeignKey(rp => rp.PermissionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
    }
}