using FinTech.Core.Application.Interfaces;
using FinTech.Core.Application.Interfaces.Repositories;
using FinTech.Core.Domain.Entities.Identity;
using FinTech.Core.Domain.Entities.ClientPortal;
using FinTech.Core.Application.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using SecurityActivity = FinTech.Core.Domain.Entities.Identity.SecurityActivity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FinTech.Infrastructure.Data;
using FinTech.Infrastructure.Data;
using FinTech.Infrastructure.Services.Security;

namespace FinTech.Infrastructure.Services
{
    public class MfaService : IMfaService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<MfaService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMfaNotificationService _notificationService;
        
        public MfaService(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<MfaService> logger,
            IConfiguration configuration,
            IMfaNotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
            _notificationService = notificationService;
        }

        public async Task<MfaSetupResponseDto> GenerateMfaSetupAsync(string userId)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userGuid)) throw new ArgumentException("Invalid user ID format");

                // Generate a new secret key
                var secretKey = GenerateSecretKey();
                
                // Get user info for QR code
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    throw new Exception("User not found");
                }
                
                // Generate QR code URL (for use with authenticator apps)
                var companyName = _configuration["Application:Name"] ?? "FinTech";
                var qrCodeUrl = GenerateQrCodeUrl(secretKey, user.Email, companyName);
                
                // Generate backup codes
                var backupCodes = GenerateBackupCodes();
                
                // Create or update MFA settings
                var existingSettings = await _context.UserMfaSettings
                    .FirstOrDefaultAsync(m => m.UserId == userGuid);
                
                if (existingSettings != null)
                {
                    // Remove existing backup codes
                    var existingCodes = await _context.MfaBackupCodes
                        .Where(b => b.UserMfaSettingsId == existingSettings.Id)
                        .ToListAsync();
                    
                    _context.MfaBackupCodes.RemoveRange(existingCodes);
                    
                    // Update settings
                    existingSettings.SecretKey = secretKey;
                    existingSettings.IsEnabled = false; // Will be enabled after verification
                    existingSettings.CreatedAt = DateTime.UtcNow;
                    existingSettings.LastVerifiedAt = null;
                }
                else
                {
                    // Create new settings
                    var newSettings = new UserMfaSettings
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userGuid,
                        SecretKey = secretKey,
                        IsEnabled = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    _context.UserMfaSettings.Add(newSettings);
                    await _context.SaveChangesAsync();
                    
                    existingSettings = newSettings;
                }
                
                // Add backup codes
                foreach (var code in backupCodes)
                {
                    _context.MfaBackupCodes.Add(new MfaBackupCode
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserMfaSettingsId = existingSettings.Id,
                        Code = HashBackupCode(code), // Store hashed codes only
                        IsUsed = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                
                await _context.SaveChangesAsync();
                
                // Return setup information
                return new MfaSetupResponseDto
                {
                    Secret = secretKey,
                    QrCodeUrl = qrCodeUrl,
                    BackupCodes = backupCodes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating MFA setup for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> VerifyMfaSetupAsync(string userId, string code)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userGuid)) return false;

                var settings = await _context.UserMfaSettings
                    .FirstOrDefaultAsync(m => m.UserId == userGuid);
                
                if (settings == null || string.IsNullOrEmpty(settings.SecretKey))
                {
                    return false;
                }
                
                // Verify the provided code against the user's secret
                bool isValid = ValidateTotp(settings.SecretKey, code);
                
                if (isValid)
                {
                    // Mark as verified
                    settings.LastVerifiedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying MFA setup for user {UserId}", userId);
                return false;
            }
        }

        public async Task EnableMfaAsync(string userId, string secret)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userGuid)) throw new ArgumentException("Invalid user ID format");

                var settings = await _context.UserMfaSettings
                    .FirstOrDefaultAsync(m => m.UserId == userGuid);
                
                if (settings == null)
                {
                    throw new Exception("MFA settings not found for user");
                }
                
                // Validate that this is the same secret that was generated
                if (settings.SecretKey != secret)
                {
                    throw new Exception("Invalid secret key");
                }
                
                // Enable MFA
                settings.IsEnabled = true;
                
                // Log security activity
                await LogSecurityActivityAsync(new SecurityActivityDto
                {
                    UserId = userId,
                    EventType = "mfa_update",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = "0.0.0.0", // Should be provided by caller
                    DeviceInfo = "Unknown", // Should be provided by caller
                    Status = "success"
                });
                
                // Send MFA setup success notification
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await _notificationService.SendMfaSetupSuccessEmailAsync(userId, user.UserName, user.Email);
                }
                
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling MFA for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DisableMfaAsync(string userId, string code)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userGuid)) return false;

                var settings = await _context.UserMfaSettings
                    .FirstOrDefaultAsync(m => m.UserId == userGuid && m.IsEnabled);
                
                if (settings == null)
                {
                    return false;
                }
                
                // Verify the code before disabling
                bool isValid = ValidateTotp(settings.SecretKey, code);
                
                if (isValid)
                {
                    // Disable MFA
                    settings.IsEnabled = false;
                    
                    // Log security activity
                    await LogSecurityActivityAsync(new SecurityActivityDto
                    {
                        UserId = userId,
                        EventType = "mfa_update",
                        Timestamp = DateTime.UtcNow,
                        IpAddress = "0.0.0.0", // Should be provided by caller
                        DeviceInfo = "Unknown", // Should be provided by caller
                        Status = "success"
                    });
                    
                    // Send MFA disabled notification
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        await _notificationService.SendSecurityAlertEmailAsync(
                            userId,
                            user.Email, 
                            "Two-Factor Authentication Disabled",
                            "Your account's two-factor authentication has been disabled. If you did not make this change, please contact support immediately."
                        );
                    }
                    
                    await _context.SaveChangesAsync();
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling MFA for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ValidateMfaCodeAsync(string userId, string code, MfaDeviceInfoDto deviceInfo = null)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userGuid)) return false;

                var settings = await _context.UserMfaSettings
                    .FirstOrDefaultAsync(m => m.UserId == userGuid && m.IsEnabled);
                
                if (settings == null)
                {
                    // User doesn't have MFA enabled
                    return false;
                }
                
                // Validate the TOTP code
                bool isValid = ValidateTotp(settings.SecretKey, code);
                
                if (isValid)
                {
                    // Update last verified timestamp
                    settings.LastVerifiedAt = DateTime.UtcNow;
                    
                    // Send login notification if device info is provided
                    if (deviceInfo != null)
                    {
                        var user = await _userManager.FindByIdAsync(userId);
                        if (user != null)
                        {
                            // Check user preferences for notifications
                            var preferences = await GetSecurityPreferencesAsync(userId);
                            if (preferences.LoginNotificationsEnabled)
                            {
                                // Determine if this is an unusual login (different device/location)
                                bool isUnusual = await IsUnusualLoginAsync(userId, deviceInfo);
                                
                                if (isUnusual && preferences.UnusualActivityNotificationsEnabled)
                                {
                                    // Send suspicious activity alert
                                    await _notificationService.SendSuspiciousActivityAlertAsync(
                                        userId,
                                        user.Email,
                                        "Suspicious Login",
                                        deviceInfo.IpAddress,
                                        $"{deviceInfo.Browser} on {deviceInfo.OperatingSystem}",
                                        "Unknown"
                                    );
                                }
                                else
                                {
                                    // Send regular login notification
                                    await _notificationService.SendNewLoginNotificationAsync(
                                        userId,
                                        user.Email,
                                        deviceInfo.IpAddress,
                                        $"{deviceInfo.Browser} on {deviceInfo.OperatingSystem}",
                                        "Unknown"
                                    );
                                }
                            }
                        }
                    }
                    
                    await _context.SaveChangesAsync();
                }
                
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating MFA code for user {UserId}", userId);
                return false;
            }
        }
        
        // Helper method to determine if a login is unusual based on device/location history
        private async Task<bool> IsUnusualLoginAsync(string userId, MfaDeviceInfoDto deviceInfo)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userGuid)) return true;

                var settings = await _context.UserMfaSettings
                    .FirstOrDefaultAsync(m => m.UserId == userGuid);
                
                if (settings == null)
                {
                    return true; // No settings means this is first login, so consider unusual
                }
                
                // Check if this is a device that has been used before
                var previousLogins = await _context.TrustedDevices
                    .Where(d => 
                        d.UserMfaSettingsId == settings.Id && 
                        !d.IsRevoked &&
                        d.Browser == deviceInfo.Browser &&
                        d.OperatingSystem == deviceInfo.OperatingSystem)
                    .ToListAsync();
                
                if (previousLogins.Count > 0)
                {
                    return false; // Device has been used before
                }
                
                // Check recent login patterns (IP address range, time of day, etc.)
                var recentActivities = await _context.SecurityActivities
                    .Where(a => 
                        a.UserId == userGuid && 
                        a.EventType == "login_success" &&
                        a.Timestamp > DateTime.UtcNow.AddDays(-30))
                    .ToListAsync();
                
                // IP address pattern matching could be implemented here
                // For now, just consider unusual if no recent logins or no device match
                return recentActivities.Count == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for unusual login for user {UserId}", userId);
                return true; // Consider unusual on error
            }
        }

        public async Task<bool> ValidateBackupCodeAsync(string userId, string backupCode)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userGuid)) return false;

                var settings = await _context.UserMfaSettings
                    .FirstOrDefaultAsync(m => m.UserId == userGuid && m.IsEnabled);
                
                if (settings == null)
                {
                    return false;
                }
                
                // Hash the provided backup code
                var hashedCode = HashBackupCode(backupCode);
                
                // Find the backup code
                var code = await _context.MfaBackupCodes
                    .FirstOrDefaultAsync(b => 
                        b.UserMfaSettingsId == settings.Id && 
                        b.Code == hashedCode && 
                        !b.IsUsed);
                
                if (code == null)
                {
                    return false;
                }
                
                // Mark code as used
                code.IsUsed = true;
                code.UsedAt = DateTime.UtcNow;
                
                // Update last verified timestamp
                settings.LastVerifiedAt = DateTime.UtcNow;
                
                // Log security activity
                await LogSecurityActivityAsync(new SecurityActivityDto
                {
                    UserId = userId,
                    EventType = "mfa_backup_used",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = "0.0.0.0", // Should be provided by caller
                    DeviceInfo = "Unknown", // Should be provided by caller
                    Status = "success"
                });
                
                // Send backup code used notification
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                     await _notificationService.SendSecurityAlertEmailAsync(
                        userId,
                        user.Email,
                        "Backup Code Used for Login",
                        "A backup code was just used to access your account. If this wasn't you, please secure your account immediately."
                    );
                }
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating backup code for user {UserId}", userId);
                return false;
            }
        }

        public async Task<List<string>> RegenerateBackupCodesAsync(string userId, string currentCode)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userGuid)) throw new ArgumentException("Invalid user ID format");

                var settings = await _context.UserMfaSettings
                    .FirstOrDefaultAsync(m => m.UserId == userGuid && m.IsEnabled);
                
                if (settings == null)
                {
                    throw new Exception("MFA not enabled for user");
                }
                
                // Verify current code
                bool isValid = ValidateTotp(settings.SecretKey, currentCode);
                
                if (!isValid)
                {
                    throw new Exception("Invalid verification code");
                }
                
                // Remove existing backup codes
                var existingCodes = await _context.MfaBackupCodes
                    .Where(b => b.UserMfaSettingsId == settings.Id)
                    .ToListAsync();
                
                _context.MfaBackupCodes.RemoveRange(existingCodes);
                
                // Generate new backup codes
                var backupCodes = GenerateBackupCodes();
                
                // Add new backup codes
                foreach (var code in backupCodes)
                {
                    _context.MfaBackupCodes.Add(new MfaBackupCode
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserMfaSettingsId = settings.Id,
                        Code = HashBackupCode(code), // Store hashed codes only
                        IsUsed = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                
                // Log security activity
                await LogSecurityActivityAsync(new SecurityActivityDto
                {
                    UserId = userId,
                    EventType = "mfa_backup_regenerated",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = "0.0.0.0", // Should be provided by caller
                    DeviceInfo = "Unknown", // Should be provided by caller
                    Status = "success"
                });
                
                // Send backup codes regenerated notification
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await _notificationService.SendBackupCodesGeneratedEmailAsync(
                        userId, 
                        user.UserName, 
                        // We don't send codes via email for security
                        user.Email
                    );
                }
                
                await _context.SaveChangesAsync();
                return backupCodes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating backup codes for user {UserId}", userId);
                throw;
            }
        }

        public async Task<MfaChallengeResponseDto> CreateMfaChallengeAsync(string userId, string operation)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userGuid)) throw new ArgumentException("Invalid user ID format");

                var settings = await _context.UserMfaSettings
                    .FirstOrDefaultAsync(m => m.UserId == userGuid && m.IsEnabled);
                
                if (settings == null)
                {
                    throw new Exception("MFA not enabled for user");
                }
                
                // Create a new MFA challenge
                var challenge = new MfaChallenge
                {
                    Id = Guid.NewGuid().ToString(),
                    UserMfaSettingsId = settings.Id,
                    Operation = operation,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5), // 5 minute expiry
                    IsVerified = false
                };
                
                _context.MfaChallenges.Add(challenge);
                await _context.SaveChangesAsync();
                
                return new MfaChallengeResponseDto
                {
                    Success = true,
                    ChallengeId = challenge.Id,
                    ExpiresAt = challenge.ExpiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MFA challenge for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> VerifyMfaChallengeAsync(string challengeId, string code)
        {
            try
            {
                var challenge = await _context.MfaChallenges
                    .Include(c => c.UserMfaSettings)
                    .FirstOrDefaultAsync(c => 
                        c.Id == challengeId && 
                        !c.IsVerified && 
                        c.ExpiresAt > DateTime.UtcNow);
                
                if (challenge == null || challenge.UserMfaSettings == null)
                {
                    return false;
                }
                
                // Validate the TOTP code
                bool isValid = ValidateTotp(challenge.UserMfaSettings.SecretKey, code);
                
                if (isValid)
                {
                    // Mark challenge as verified
                    challenge.IsVerified = true;
                    challenge.VerifiedAt = DateTime.UtcNow;
                    
                    // Update last verified timestamp
                    challenge.UserMfaSettings.LastVerifiedAt = DateTime.UtcNow;
                    
                    await _context.SaveChangesAsync();
                }
                
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying MFA challenge {ChallengeId}", challengeId);
                return false;
            }
        }

        public async Task<string> AddTrustedDeviceAsync(string userId, MfaDeviceInfoDto deviceInfo)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userGuid)) throw new ArgumentException("Invalid user ID format");

                var settings = await _context.UserMfaSettings
                    .FirstOrDefaultAsync(m => m.UserId == userGuid);
                
                if (settings == null)
                {
                    // Create MFA settings if not exists
                    settings = new UserMfaSettings
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userGuid,
                        IsEnabled = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    _context.UserMfaSettings.Add(settings);
                }
                
                // Create a new trusted device
                var device = new TrustedDevice
                {
                    Id = Guid.NewGuid().ToString(),
                    UserMfaSettingsId = settings.Id,
                    DeviceName = deviceInfo.Name,
                    DeviceType = deviceInfo.Type,
                    Browser = deviceInfo.Browser,
                    OperatingSystem = deviceInfo.OperatingSystem,
                    IpAddress = deviceInfo.IpAddress,
                    Location = "Unknown", // Can be updated with geolocation service
                    CreatedAt = DateTime.UtcNow,
                    LastUsedAt = DateTime.UtcNow,
                    IsRevoked = false
                };
                
                _context.TrustedDevices.Add(device);
                
                // Log security activity
                await LogSecurityActivityAsync(new SecurityActivityDto
                {
                    UserId = userId,
                    EventType = "device_added",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = deviceInfo.IpAddress,
                    DeviceInfo = $"{deviceInfo.Browser} on {deviceInfo.OperatingSystem}",
                    Status = "success"
                });
                
                // Send trusted device added notification
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await _notificationService.SendTrustedDeviceAddedEmailAsync(
                        userId,
                        user.Email,
                        $"{deviceInfo.Browser} on {deviceInfo.OperatingSystem}"
                    );
                }
                
                await _context.SaveChangesAsync();
                return device.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding trusted device for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<TrustedDeviceDto>> GetTrustedDevicesAsync(string userId)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userGuid)) return new List<TrustedDeviceDto>();

                var settings = await _context.UserMfaSettings
                    .FirstOrDefaultAsync(m => m.UserId == userGuid);
                
                if (settings == null)
                {
                    return new List<TrustedDeviceDto>();
                }
                
                var devices = await _context.TrustedDevices
                    .Where(d => d.UserMfaSettingsId == settings.Id && !d.IsRevoked)
                    .OrderByDescending(d => d.LastUsedAt)
                    .ToListAsync();
                
                return devices.Select(d => new TrustedDeviceDto
                {
                    Id = d.Id,
                    DeviceName = d.DeviceName,
                    DeviceType = d.DeviceType,
                    Browser = d.Browser,
                    OperatingSystem = d.OperatingSystem,
                    LastUsed = d.LastUsedAt,
                    Location = d.Location,
                    IpAddress = d.IpAddress,
                    IsCurrent = false // Should be set by caller
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trusted devices for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> RevokeTrustedDeviceAsync(string userId, string deviceId)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userGuid)) return false;

                var settings = await _context.UserMfaSettings
                    .FirstOrDefaultAsync(m => m.UserId == userGuid);
                
                if (settings == null)
                {
                    return false;
                }
                
                var device = await _context.TrustedDevices
                    .FirstOrDefaultAsync(d => 
                        d.Id == deviceId && 
                        d.UserMfaSettingsId == settings.Id && 
                        !d.IsRevoked);
                
                if (device == null)
                {
                    return false;
                }
                
                // Revoke the device
                device.IsRevoked = true;
                device.RevokedAt = DateTime.UtcNow;
                
                // Log security activity
                await LogSecurityActivityAsync(new SecurityActivityDto
                {
                    UserId = userId,
                    EventType = "device_removed",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = "0.0.0.0", // Should be provided by caller
                    DeviceInfo = $"{device.Browser} on {device.OperatingSystem}",
                    Status = "success"
                });
                
                // Send device revoked notification
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await _notificationService.SendTrustedDeviceRemovedEmailAsync(
                        userId,
                        user.Email,
                        $"{device.Browser} on {device.OperatingSystem}"
                    );
                }
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking trusted device {DeviceId} for user {UserId}", deviceId, userId);
                return false;
            }
        }

        public async Task<bool> RevokeAllTrustedDevicesExceptCurrentAsync(string userId, string currentDeviceId)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userGuid)) return false;

                var settings = await _context.UserMfaSettings
                    .FirstOrDefaultAsync(m => m.UserId == userGuid);
                
                if (settings == null)
                {
                    return false;
                }
                
                var devices = await _context.TrustedDevices
                    .Where(d => 
                        d.UserMfaSettingsId == settings.Id && 
                        !d.IsRevoked &&
                        d.Id != currentDeviceId)
                    .ToListAsync();
                
                foreach (var device in devices)
                {
                    device.IsRevoked = true;
                    device.RevokedAt = DateTime.UtcNow;
                }
                
                // Log security activity
                await LogSecurityActivityAsync(new SecurityActivityDto
                {
                    UserId = userId,
                    EventType = "devices_revoked_all",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = "0.0.0.0", // Should be provided by caller
                    DeviceInfo = "All devices except current",
                    Status = "success"
                });
                
                // Send revoked devices notification
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null && devices.Count > 0)
                {
                    await _notificationService.SendSecurityAlertEmailAsync(
                        user.Id.ToString(),
                        user.Email,
                        "All Other Devices Signed Out",
                        $"You have signed out from all devices except your current one. {devices.Count} device(s) were affected. If you did not perform this action, please secure your account immediately."
                    );
                }
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all trusted devices for user {UserId}", userId);
                return false;
            }
        }

        public async Task LogSecurityActivityAsync(SecurityActivityDto activityDto)
        {
            try
            {
                var activity = new SecurityActivity
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = Guid.Parse(activityDto.UserId),
                    EventType = activityDto.EventType,
                    Timestamp = activityDto.Timestamp,
                    IpAddress = activityDto.IpAddress,
                    Location = activityDto.Location,
                    DeviceInfo = activityDto.DeviceInfo,
                    Status = activityDto.Status,
                    Details = activityDto.EventType
                };
                
                _context.SecurityActivities.Add(activity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging security activity for user {UserId}", activityDto.UserId);
                // Don't rethrow - logging should not disrupt main functionality
            }
        }

        public async Task<List<SecurityActivityDto>> GetSecurityActivityAsync(string userId, int limit = 20)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userGuid)) return new List<SecurityActivityDto>();

                var activities = await _context.SecurityActivities
                    .Where(a => a.UserId == userGuid)
                    .OrderByDescending(a => a.Timestamp)
                    .Take(limit)
                    .ToListAsync();
                
                return activities.Select(a => new SecurityActivityDto
                {
                    Id = a.Id,
                    UserId = a.UserId.ToString(),
                    EventType = a.EventType,
                    Timestamp = a.Timestamp,
                    IpAddress = a.IpAddress,
                    Location = a.Location,
                    DeviceInfo = a.DeviceInfo,
                    Status = a.Status
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting security activity for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateSecurityPreferencesAsync(string userId, SecurityPreferencesDto preferences)
        {
            try
            {
                var existingPreferences = await _context.SecurityPreferences
                    .FirstOrDefaultAsync(p => p.UserId == Guid.Parse(userId));
                
                if (existingPreferences != null)
                {
                    // Update existing preferences
                    existingPreferences.EmailNotificationsEnabled = preferences.EmailNotificationsEnabled;
                    existingPreferences.LoginNotificationsEnabled = preferences.LoginNotificationsEnabled;
                    existingPreferences.UnusualActivityNotificationsEnabled = preferences.UnusualActivityNotificationsEnabled;
                    existingPreferences.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    // Create new preferences
                    _context.SecurityPreferences.Add(new UserSecurityPreferences
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = Guid.Parse(userId),
                        EmailNotificationsEnabled = preferences.EmailNotificationsEnabled,
                        LoginNotificationsEnabled = preferences.LoginNotificationsEnabled,
                        UnusualActivityNotificationsEnabled = preferences.UnusualActivityNotificationsEnabled,
                        LastUpdated = DateTime.UtcNow
                    });
                }
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating security preferences for user {UserId}", userId);
                return false;
            }
        }

        public async Task<SecurityPreferencesDto> GetSecurityPreferencesAsync(string userId)
        {
            try
            {
                UserSecurityPreferences? preferences = await _context.SecurityPreferences
                    .FirstOrDefaultAsync(p => p.UserId == Guid.Parse(userId));
                
                if (preferences == null)
                {
                    // Return default preferences
                    return new SecurityPreferencesDto
                    {
                        EmailNotificationsEnabled = true,
                        LoginNotificationsEnabled = true,
                        UnusualActivityNotificationsEnabled = true
                    };
                }
                
                return new SecurityPreferencesDto
                {
                    EmailNotificationsEnabled = preferences.EmailNotificationsEnabled,
                    LoginNotificationsEnabled = preferences.LoginNotificationsEnabled,
                    UnusualActivityNotificationsEnabled = preferences.UnusualActivityNotificationsEnabled
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting security preferences for user {UserId}", userId);
                throw;
            }
        }

        #region Helper Methods

        private string GenerateSecretKey()
        {
            // Generate a random 160-bit (20-byte) secret key
            var bytes = new byte[20];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            
            // Convert to Base32 for compatibility with authenticator apps
            return Base32Encode(bytes);
        }

        private string GenerateQrCodeUrl(string secret, string email, string issuer)
        {
            // Format: otpauth://totp/{issuer}:{account}?secret={secret}&issuer={issuer}
            return $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}";
        }

        private List<string> GenerateBackupCodes()
        {
            // Generate 10 backup codes (8 digits each)
            var codes = new List<string>();
            using (var rng = RandomNumberGenerator.Create())
            {
                for (int i = 0; i < 10; i++)
                {
                    var bytes = new byte[4];
                    rng.GetBytes(bytes);
                    uint number = BitConverter.ToUInt32(bytes, 0) % 100000000; // 8 digits
                    codes.Add(number.ToString("D8")); // Pad to 8 digits
                }
            }
            
            return codes;
        }

        private string HashBackupCode(string code)
        {
            // Simple hash for backup codes (in production, use a proper password hash)
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(code);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private bool ValidateTotp(string secret, string code)
        {
            // Simple TOTP implementation for demonstration
            // In production, use a proper TOTP library
            
            // Convert the secret from Base32 to bytes
            var secretBytes = Base32Decode(secret);
            
            // Get the current timestamp (30-second window)
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
            
            // Check the current window and one window before/after (to account for clock skew)
            for (long window = -1; window <= 1; window++)
            {
                var currentCode = GenerateTotpCode(secretBytes, timestamp + window);
                if (currentCode == code)
                {
                    return true;
                }
            }
            
            return false;
        }

        private string GenerateTotpCode(byte[] secret, long timestamp)
        {
            // Convert timestamp to bytes (big-endian)
            var timestampBytes = BitConverter.GetBytes(timestamp);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(timestampBytes);
            }
            
            // Pad to 8 bytes
            var counter = new byte[8];
            Array.Copy(timestampBytes, 0, counter, 8 - timestampBytes.Length, timestampBytes.Length);
            
            // Compute HMAC-SHA1
            using (var hmac = new HMACSHA1(secret))
            {
                var hash = hmac.ComputeHash(counter);
                
                // Get offset based on last nibble
                var offset = hash[hash.Length - 1] & 0x0F;
                
                // Get 4 bytes at the offset
                var selectedBytes = new byte[4];
                Array.Copy(hash, offset, selectedBytes, 0, 4);
                
                // Convert to integer (ignore most significant bit)
                uint value = BitConverter.ToUInt32(selectedBytes, 0);
                if (BitConverter.IsLittleEndian)
                {
                    value = (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                           (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
                }
                value &= 0x7FFFFFFF;
                
                // Get 6 digits
                value %= 1000000;
                
                // Format as 6-digit code
                return value.ToString("D6");
            }
        }

        private string Base32Encode(byte[] data)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var result = new StringBuilder();
            
            int bits = 0;
            int buffer = 0;
            
            foreach (byte b in data)
            {
                buffer = (buffer << 8) | b;
                bits += 8;
                
                while (bits >= 5)
                {
                    bits -= 5;
                    result.Append(alphabet[(buffer >> bits) & 31]);
                }
            }
            
            if (bits > 0)
            {
                result.Append(alphabet[(buffer << (5 - bits)) & 31]);
            }
            
            return result.ToString();
        }

        private byte[] Base32Decode(string data)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var result = new List<byte>();
            
            int buffer = 0;
            int bits = 0;
            
            foreach (char c in data)
            {
                int value = alphabet.IndexOf(char.ToUpper(c));
                if (value < 0)
                {
                    continue; // Skip invalid characters
                }
                
                buffer = (buffer << 5) | value;
                bits += 5;
                
                if (bits >= 8)
                {
                    bits -= 8;
                    result.Add((byte)(buffer >> bits));
                }
            }
            
            return result.ToArray();
        }

        #endregion
    }
}
