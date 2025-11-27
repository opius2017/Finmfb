using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinTech.Tests.Common
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            // Clear existing data
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            // Seed roles
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new IdentityRole { Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = Guid.NewGuid().ToString() },
                    new IdentityRole { Name = "User", NormalizedName = "USER", ConcurrencyStamp = Guid.NewGuid().ToString() },
                    new IdentityRole { Name = "Staff", NormalizedName = "STAFF", ConcurrencyStamp = Guid.NewGuid().ToString() }
                );
                await context.SaveChangesAsync();
            }

            // Seed test user
            if (!context.Users.Any())
            {
                var hasher = new PasswordHasher<IdentityUser>();
                var testUser = new IdentityUser
                {
                    UserName = "test@example.com",
                    NormalizedUserName = "TEST@EXAMPLE.COM",
                    Email = "test@example.com",
                    NormalizedEmail = "TEST@EXAMPLE.COM",
                    EmailConfirmed = true,
                    PhoneNumber = "1234567890",
                    PhoneNumberConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                testUser.PasswordHash = hasher.HashPassword(testUser, "Test@123");
                context.Users.Add(testUser);
                await context.SaveChangesAsync();

                // Assign role to user
                var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                if (userRole != null)
                {
                    context.UserRoles.Add(new IdentityUserRole<string>
                    {
                        UserId = testUser.Id,
                        RoleId = userRole.Id
                    });
                    await context.SaveChangesAsync();
                }

                // Add MFA settings for test user (disabled by default)
                context.UserMfaSettings.Add(new Domain.Entities.Authentication.MfaSettings
                {
                    UserId = testUser.Id,
                    IsEnabled = false,
                    Secret = "TESTSECRETFORTESTUSER23456",
                    PreferredMethod = "App",
                    LastVerifiedAt = DateTime.UtcNow.AddDays(-10)
                });
                await context.SaveChangesAsync();
                
                // Add security preferences for test user
                context.SecurityPreferences.Add(new Domain.Entities.Authentication.UserSecurityPreferences
                {
                    UserId = testUser.Id,
                    NotifyOnLogin = true,
                    NotifyOnPasswordChange = true,
                    NotifyOnProfileUpdate = true,
                    EnableLoginAlerts = true,
                    EnableSuspiciousActivityAlerts = true,
                    UseStrictDeviceVerification = false,
                    LastUpdated = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }

            // Add sample backup codes for test user
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
            if (user != null && !context.BackupCodes.Any(b => b.UserId == user.Id))
            {
                var backupCodes = new List<Domain.Entities.Authentication.BackupCode>
                {
                    new Domain.Entities.Authentication.BackupCode { UserId = user.Id, Code = "123456", IsUsed = false, CreatedAt = DateTime.UtcNow },
                    new Domain.Entities.Authentication.BackupCode { UserId = user.Id, Code = "654321", IsUsed = false, CreatedAt = DateTime.UtcNow },
                    new Domain.Entities.Authentication.BackupCode { UserId = user.Id, Code = "789012", IsUsed = false, CreatedAt = DateTime.UtcNow },
                    new Domain.Entities.Authentication.BackupCode { UserId = user.Id, Code = "210987", IsUsed = false, CreatedAt = DateTime.UtcNow },
                    new Domain.Entities.Authentication.BackupCode { UserId = user.Id, Code = "345678", IsUsed = false, CreatedAt = DateTime.UtcNow },
                };
                
                context.BackupCodes.AddRange(backupCodes);
                await context.SaveChangesAsync();
            }

            // Add sample security activity for test user
            if (user != null && !context.SecurityActivities.Any(s => s.UserId == user.Id))
            {
                var activities = new List<Domain.Entities.Authentication.SecurityActivity>
                {
                    new Domain.Entities.Authentication.SecurityActivity 
                    { 
                        UserId = user.Id, 
                        EventType = "login_success", 
                        Timestamp = DateTime.UtcNow.AddDays(-5), 
                        IpAddress = "192.168.1.1", 
                        DeviceInfo = "Chrome 98.0.4758.102 on Windows 10", 
                        Status = "success" 
                    },
                    new Domain.Entities.Authentication.SecurityActivity 
                    { 
                        UserId = user.Id, 
                        EventType = "password_change", 
                        Timestamp = DateTime.UtcNow.AddDays(-10), 
                        IpAddress = "192.168.1.1", 
                        DeviceInfo = "Chrome 98.0.4758.102 on Windows 10", 
                        Status = "success" 
                    },
                    new Domain.Entities.Authentication.SecurityActivity 
                    { 
                        UserId = user.Id, 
                        EventType = "login_failed", 
                        Timestamp = DateTime.UtcNow.AddDays(-2), 
                        IpAddress = "192.168.1.100", 
                        DeviceInfo = "Firefox 97.0 on Ubuntu", 
                        Status = "failure" 
                    }
                };
                
                context.SecurityActivities.AddRange(activities);
                await context.SaveChangesAsync();
            }
        }
    }
}