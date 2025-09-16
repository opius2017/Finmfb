using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Domain.Entities.Identity;

namespace FinTech.Infrastructure.Data.Configuration.Identity;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasIndex(e => e.Code).IsUnique();
        builder.HasIndex(e => e.Name);
    }
}