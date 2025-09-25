using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Core.Domain.Entities.Identity;

namespace FinTech.Infrastructure.Data.Configuration.Identity;

public class UserDashboardPreferenceConfiguration : IEntityTypeConfiguration<UserDashboardPreference>
{
    public void Configure(EntityTypeBuilder<UserDashboardPreference> builder)
    {
        builder.HasOne(udp => udp.User)
               .WithMany()
               .HasForeignKey(udp => udp.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(udp => udp.ModuleDashboard)
               .WithMany(md => md.UserPreferences)
               .HasForeignKey(udp => udp.ModuleDashboardId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.UserId, e.ModuleDashboardId }).IsUnique();
    }
}
