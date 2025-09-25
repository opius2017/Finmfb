using FinTech.Core.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Data.Configuration
{
    /// <summary>
    /// Entity Framework configuration for the AuditLog entity
    /// </summary>
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.EntityName)
                .IsRequired()
                .HasMaxLength(256);
                
            builder.Property(e => e.EntityId)
                .IsRequired()
                .HasMaxLength(256);
                
            builder.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(e => e.UserId)
                .HasMaxLength(256);
                
            builder.Property(e => e.Changes)
                .HasColumnType("nvarchar(max)");
                
            // Create indexes for better query performance
            builder.HasIndex(e => e.EntityName);
            builder.HasIndex(e => e.EntityId);
            builder.HasIndex(e => e.Timestamp);
            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.TenantId);
        }
    }
}
