using FinTech.Core.Domain.Entities.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.WebAPI.Infrastructure.Data.Configurations
{
    public class UserMfaSettingsConfiguration : IEntityTypeConfiguration<MfaSettings>
    {
        public void Configure(EntityTypeBuilder<MfaSettings> builder)
        {
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.UserId)
                .IsRequired();
                
            builder.Property(x => x.SecretKey)
                .IsRequired()
                .HasMaxLength(128);
                
            builder.Property(x => x.IsEnabled)
                .IsRequired();
                
            builder.Property(x => x.CreatedAt)
                .IsRequired();
                
            // Index for faster lookups by UserId
            builder.HasIndex(x => x.UserId)
                .IsUnique();
                
            // Relationships
            builder.HasMany(x => x.BackupCodes)
                .WithOne(x => x.MfaSettings)
                .HasForeignKey(x => x.MfaSettingsId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasMany(x => x.Challenges)
                .WithOne(x => x.UserMfaSettings)
                .HasForeignKey(x => x.UserMfaSettingsId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasMany(x => x.TrustedDevices)
                .WithOne(x => x.UserMfaSettings)
                .HasForeignKey(x => x.UserMfaSettingsId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class MfaBackupCodeConfiguration : IEntityTypeConfiguration<BackupCode>
    {
        public void Configure(EntityTypeBuilder<BackupCode> builder)
        {
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.MfaSettingsId)
                .IsRequired();
                
            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(128); // Store as hash
                
            builder.Property(x => x.IsUsed)
                .IsRequired();
                
            builder.Property(x => x.CreatedAt)
                .IsRequired();
                
            // Index for faster lookup by UserMfaSettingsId
            builder.HasIndex(x => x.MfaSettingsId);
            
            // Unique constraint on the code (no duplicate backup codes)
            builder.HasIndex(x => x.Code)
                .IsUnique();
        }
    }
    
    public class MfaChallengeConfiguration : IEntityTypeConfiguration<MfaChallenge>
    {
        public void Configure(EntityTypeBuilder<MfaChallenge> builder)
        {
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.MfaSettingsId)
                .IsRequired();
                
            builder.Property(x => x.Operation)
                .IsRequired()
                .HasMaxLength(256);
                
            builder.Property(x => x.CreatedAt)
                .IsRequired();
                
            builder.Property(x => x.ExpiresAt)
                .IsRequired();
                
            builder.Property(x => x.IsVerified)
                .IsRequired();
                
            // Index for faster lookup by UserMfaSettingsId
            builder.HasIndex(x => x.MfaSettingsId);
            
            // Index for expiration cleanup
            builder.HasIndex(x => x.ExpiresAt);
        }
    }
    
    public class TrustedDeviceConfiguration : IEntityTypeConfiguration<TrustedDevice>
    {
        public void Configure(EntityTypeBuilder<TrustedDevice> builder)
        {
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.MfaSettingsId)
                .IsRequired();
                
            builder.Property(x => x.DeviceName)
                .IsRequired()
                .HasMaxLength(256);
                
            builder.Property(x => x.DeviceType)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(x => x.Browser)
                .IsRequired()
                .HasMaxLength(128);
                
            builder.Property(x => x.OperatingSystem)
                .IsRequired()
                .HasMaxLength(128);
                
            builder.Property(x => x.IpAddress)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(x => x.Location)
                .HasMaxLength(256);
                
            builder.Property(x => x.CreatedAt)
                .IsRequired();
                
            builder.Property(x => x.LastUsedAt)
                .IsRequired();
                
            builder.Property(x => x.IsRevoked)
                .IsRequired();
                
            // Index for faster lookup by UserMfaSettingsId
            builder.HasIndex(x => x.MfaSettingsId);
            
            // Index for revoked devices
            builder.HasIndex(x => x.IsRevoked);
        }
    }
    
    public class SecurityActivityConfiguration : IEntityTypeConfiguration<SecurityActivity>
    {
        public void Configure(EntityTypeBuilder<SecurityActivity> builder)
        {
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.UserId)
                .IsRequired();
                
            builder.Property(x => x.EventType)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(x => x.Timestamp)
                .IsRequired();
                
            builder.Property(x => x.IpAddress)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(x => x.Location)
                .HasMaxLength(256);
                
            builder.Property(x => x.DeviceInfo)
                .HasMaxLength(512);
                
            builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(20);
                
            builder.Property(x => x.Details)
                .HasMaxLength(1024);
                
            // Indexes for common queries
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.Timestamp);
            builder.HasIndex(x => new { x.UserId, x.EventType });
        }
    }
    
    public class SecurityPreferencesConfiguration : IEntityTypeConfiguration<SecurityPreferences>
    {
        public void Configure(EntityTypeBuilder<SecurityPreferences> builder)
        {
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.UserId)
                .IsRequired();
                
            builder.Property(x => x.EmailNotificationsEnabled)
                .IsRequired();
                
            builder.Property(x => x.LoginNotificationsEnabled)
                .IsRequired();
                
            builder.Property(x => x.UnusualActivityNotificationsEnabled)
                .IsRequired();
                
            builder.Property(x => x.LastUpdated)
                .IsRequired();
                
            // Unique constraint on UserId (one preferences record per user)
            builder.HasIndex(x => x.UserId)
                .IsUnique();
        }
    }
}
