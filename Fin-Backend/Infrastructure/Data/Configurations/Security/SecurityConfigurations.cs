using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Core.Domain.Entities.Security;
using FinTech.Core.Domain.Entities.Identity;

namespace FinTech.Infrastructure.Data.Configurations.Accounting
{
    public class ResourcePermissionConfiguration : IEntityTypeConfiguration<ResourcePermission>
    {
        public void Configure(EntityTypeBuilder<ResourcePermission> builder)
        {
            builder.ToTable("ResourcePermissions");
            
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Resource)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(x => x.Operation)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(x => x.Description)
                .HasMaxLength(500);
                
            // Create unique constraint on resource + operation
            builder.HasIndex(x => new { x.Resource, x.Operation })
                .IsUnique();
        }
    }
    
    public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
    {
        public void Configure(EntityTypeBuilder<UserPermission> builder)
        {
            builder.ToTable("UserPermissions");
            
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.UserId)
                .IsRequired();
                
            builder.Property(x => x.ResourcePermissionId)
                .IsRequired();
                
            builder.Property(x => x.IsGranted)
                .IsRequired();
                
            // Create relationship to ResourcePermission
            builder.HasOne<ResourcePermission>()
                .WithMany()
                .HasForeignKey(x => x.ResourcePermissionId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Create unique constraint on user + permission
            builder.HasIndex(x => new { x.UserId, x.ResourcePermissionId })
                .IsUnique();
        }
    }
    
    public class SecurityPolicyConfiguration : IEntityTypeConfiguration<SecurityPolicy>
    {
        public void Configure(EntityTypeBuilder<SecurityPolicy> builder)
        {
            builder.ToTable("SecurityPolicies");
            
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.PolicyName)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(x => x.PolicyValue)
                .IsRequired()
                .HasMaxLength(500);
                
            builder.Property(x => x.Description)
                .HasMaxLength(500);
                
            builder.Property(x => x.IsEnabled)
                .IsRequired();
                
            // Create unique constraint on policy name
            builder.HasIndex(x => x.PolicyName)
                .IsUnique();
        }
    }
    
    public class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
    {
        public void Configure(EntityTypeBuilder<LoginAttempt> builder)
        {
            builder.ToTable("LoginAttempts");
            
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Username)
                .IsRequired()
                .HasMaxLength(256);
                
            builder.Property(x => x.IpAddress)
                .HasMaxLength(50);
                
            builder.Property(x => x.UserAgent)
                .HasMaxLength(500);
                
            builder.Property(x => x.AttemptedAt)
                .IsRequired();
                
            builder.Property(x => x.IsSuccessful)
                .IsRequired();
                
            builder.Property(x => x.FailureReason)
                .HasMaxLength(500);
                
            // Create indexes for efficient querying
            builder.HasIndex(x => x.Username);
            builder.HasIndex(x => x.IpAddress);
            builder.HasIndex(x => x.AttemptedAt);
        }
    }
    
    public class DataAccessLogConfiguration : IEntityTypeConfiguration<DataAccessLog>
    {
        public void Configure(EntityTypeBuilder<DataAccessLog> builder)
        {
            builder.ToTable("DataAccessLogs");
            
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.UserId)
                .IsRequired();
                
            builder.Property(x => x.AccessTime)
                .IsRequired();
                
            builder.Property(x => x.EntityName)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(x => x.EntityId)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(x => x.AccessType)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(x => x.IPAddress)
                .HasMaxLength(50);
                
            // Create indexes for efficient querying
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.AccessTime);
            builder.HasIndex(x => new { x.EntityName, x.EntityId });
        }
    }
}
