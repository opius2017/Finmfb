using FinTech.Core.Application.Interfaces.Repositories;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Identity;
using FinTech.Core.Domain.Entities.ClientPortal;
using FinTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SecurityActivity = FinTech.Core.Domain.Entities.Identity.SecurityActivity;

namespace FinTech.Infrastructure.Repositories
{
    /// <summary>
    /// Base repository implementation for authentication entities
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public abstract class BaseAuthRepository<T> : IBaseAuthRepository<T> where T : class
    {
        /// <summary>
        /// The database context
        /// </summary>
        protected readonly ApplicationDbContext _context;

        /// <summary>
        /// The entity set
        /// </summary>
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseAuthRepository{T}"/> class
        /// </summary>
        /// <param name="context">The database context</param>
        protected BaseAuthRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <inheritdoc/>
        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <inheritdoc/>
        public virtual async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public virtual async Task DeleteByIdAsync(string id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <inheritdoc/>
        public virtual async Task<T> GetByIdAsync(string id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <inheritdoc/>
        public virtual async Task<T> UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }
    }

    /// <summary>
    /// Repository implementation for the refresh token entity
    /// </summary>
    public class RefreshTokenRepository : BaseAuthRepository<RefreshToken>, IRefreshTokenRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshTokenRepository"/> class
        /// </summary>
        /// <param name="context">The database context</param>
        public RefreshTokenRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return new List<RefreshToken>();
            
            return await _dbSet.Where(rt => rt.UserId == userGuid && !rt.IsRevoked && !rt.IsUsed && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            return await _dbSet.FirstOrDefaultAsync(rt => rt.Token == token);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return new List<RefreshToken>();
            return await _dbSet.Where(rt => rt.UserId == userGuid).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task RevokeAllForUserAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return;
            var tokens = await _dbSet.Where(rt => rt.UserId == userGuid).ToListAsync();
            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task RevokeAllForUserExceptCurrentAsync(string userId, string currentToken)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return;
            var tokens = await _dbSet.Where(rt => rt.UserId == userGuid && rt.Token != currentToken).ToListAsync();
            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Repository implementation for the MFA settings entity
    /// </summary>
    public class MfaSettingsRepository : BaseAuthRepository<UserMfaSettings>, IMfaSettingsRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MfaSettingsRepository"/> class
        /// </summary>
        /// <param name="context">The database context</param>
        public MfaSettingsRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task DisableMfaAsync(string userId)
        {
            var settings = await GetByUserIdAsync(userId);
            if (settings != null)
            {
                settings.IsEnabled = false;
                settings.UpdatedAt = DateTime.UtcNow;
                await UpdateAsync(settings);
            }
        }

        /// <inheritdoc/>
        public async Task<UserMfaSettings> EnableMfaAsync(string userId, string method, string? sharedKey = null, string? recoveryEmail = null, string? recoveryPhone = null)
        {
            var settings = await GetByUserIdAsync(userId);
            var now = DateTime.UtcNow;
            
            if (settings == null)
            {
                if (!Guid.TryParse(userId, out var userGuid)) throw new ArgumentException("Invalid user ID");

                settings = new UserMfaSettings
                {
                    UserId = userGuid,
                    IsEnabled = true,
                    Method = method,
                    SecretKey = sharedKey,
                    RecoveryEmail = recoveryEmail,
                    RecoveryPhone = recoveryPhone,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                
                return await AddAsync(settings);
            }
            else
            {
                settings.IsEnabled = true;
                settings.Method = method;
                settings.SecretKey = sharedKey ?? settings.SecretKey;
                settings.RecoveryEmail = recoveryEmail ?? settings.RecoveryEmail;
                settings.RecoveryPhone = recoveryPhone ?? settings.RecoveryPhone;
                settings.UpdatedAt = now;
                
                return await UpdateAsync(settings);
            }
        }

        /// <inheritdoc/>
        public async Task<UserMfaSettings> GetByUserIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return null;
            return await _dbSet.FirstOrDefaultAsync(ms => ms.UserId == userGuid);
        }
    }

    /// <summary>
    /// Repository implementation for the backup code entity
    /// </summary>
    public class BackupCodeRepository : BaseAuthRepository<BackupCode>, IBackupCodeRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BackupCodeRepository"/> class
        /// </summary>
        /// <param name="context">The database context</param>
        public BackupCodeRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task DeleteAllForUserAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return;
            var codes = await _dbSet.Where(bc => bc.UserId == userGuid).ToListAsync();
            _dbSet.RemoveRange(codes);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BackupCode>> GenerateCodesAsync(string userId, int count = 10)
        {
            // Delete existing backup codes for the user
            await DeleteAllForUserAsync(userId);
            
            if (!Guid.TryParse(userId, out var userGuid)) return new List<BackupCode>();

            var codes = new List<BackupCode>();
            var now = DateTime.UtcNow;
            
            for (int i = 0; i < count; i++)
            {
                // Generate a random 8-character code
                var code = GenerateRandomCode();
                
                var backupCode = new BackupCode
                {
                    UserId = userGuid,
                    Code = code,
                    IsUsed = false,
                    CreatedAt = now
                };
                
                codes.Add(backupCode);
            }
            
            await _dbSet.AddRangeAsync(codes);
            await _context.SaveChangesAsync();
            
            return codes;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BackupCode>> GetByUserIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return new List<BackupCode>();
            return await _dbSet.Where(bc => bc.UserId == userGuid).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BackupCode>> GetUnusedByUserIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return new List<BackupCode>();
            return await _dbSet.Where(bc => bc.UserId == userGuid && !bc.IsUsed).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> MarkAsUsedAsync(string userId, string code)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return false;
            var backupCode = await _dbSet.FirstOrDefaultAsync(bc => bc.UserId == userGuid && bc.Code == code && !bc.IsUsed);
            if (backupCode == null)
            {
                return false;
            }
            
            backupCode.IsUsed = true;
            backupCode.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateCodeAsync(string userId, string code)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return false;
            return await _dbSet.AnyAsync(bc => bc.UserId == userGuid && bc.Code == code && !bc.IsUsed);
        }
        
        /// <summary>
        /// Generates a random backup code
        /// </summary>
        /// <returns>The backup code</returns>
        private string GenerateRandomCode()
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new byte[8];
            
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
            }
            
            var result = new char[8];
            for (int i = 0; i < 8; i++)
            {
                result[i] = validChars[random[i] % validChars.Length];
            }
            
            return new string(result);
        }
    }

    /// <summary>
    /// Repository implementation for the MFA challenge entity
    /// </summary>
    public class MfaChallengeRepository : BaseAuthRepository<MfaChallenge>, IMfaChallengeRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MfaChallengeRepository"/> class
        /// </summary>
        /// <param name="context">The database context</param>
        public MfaChallengeRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<MfaChallenge> CreateChallengeAsync(string userId, string method, string ipAddress, string deviceId, int expiresInMinutes = 15)
        {
            if (!Guid.TryParse(userId, out var userGuid)) throw new ArgumentException("Invalid user ID");
            var now = DateTime.UtcNow;
            var verificationCode = GenerateVerificationCode();
            
            var challenge = new MfaChallenge
            {
                UserId = userGuid,
                VerificationCode = verificationCode,
                Method = method,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(expiresInMinutes),
                IsUsed = false,
                IpAddress = ipAddress,
                DeviceId = deviceId
            };
            
            return await AddAsync(challenge);
        }

        /// <inheritdoc/>
        public async Task DeleteExpiredAsync()
        {
            var expiredChallenges = await _dbSet.Where(mc => mc.ExpiresAt < DateTime.UtcNow).ToListAsync();
            _dbSet.RemoveRange(expiredChallenges);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MfaChallenge>> GetActiveByUserIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return new List<MfaChallenge>();
            var now = DateTime.UtcNow;
            return await _dbSet.Where(mc => mc.UserId == userGuid && !mc.IsUsed && mc.ExpiresAt > now).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MfaChallenge>> GetByUserIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return new List<MfaChallenge>();
            return await _dbSet.Where(mc => mc.UserId == userGuid).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<MfaChallenge> GetByVerificationCodeAsync(string code)
        {
            return await _dbSet.FirstOrDefaultAsync(mc => mc.VerificationCode == code);
        }

        /// <inheritdoc/>
        public async Task<bool> MarkAsUsedAsync(string code, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return false;

            var challenge = await _dbSet.FirstOrDefaultAsync(mc => 
                mc.VerificationCode == code && 
                mc.UserId == userGuid && 
                !mc.IsUsed && 
                mc.ExpiresAt > DateTime.UtcNow);
                
            if (challenge == null)
            {
                return false;
            }
            
            challenge.IsUsed = true;
            challenge.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateChallengeAsync(string code, string userId, string ipAddress)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return false;
            var now = DateTime.UtcNow;
            
            return await _dbSet.AnyAsync(mc => 
                mc.VerificationCode == code && 
                mc.UserId == userGuid && 
                !mc.IsUsed && 
                mc.ExpiresAt > now);
        }
        
        /// <summary>
        /// Generates a random verification code
        /// </summary>
        /// <returns>The verification code</returns>
        private string GenerateVerificationCode()
        {
            const string validChars = "0123456789";
            var random = new byte[6];
            
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
            }
            
            var result = new char[6];
            for (int i = 0; i < 6; i++)
            {
                result[i] = validChars[random[i] % validChars.Length];
            }
            
            return new string(result);
        }
    }

    /// <summary>
    /// Repository implementation for the trusted device entity
    /// </summary>
    public class TrustedDeviceRepository : BaseAuthRepository<TrustedDevice>, ITrustedDeviceRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrustedDeviceRepository"/> class
        /// </summary>
        /// <param name="context">The database context</param>
        public TrustedDeviceRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<TrustedDevice> AddTrustedDeviceAsync(string userId, string deviceId, string deviceName, string deviceType, string operatingSystem, string browser, string browserVersion, string ipAddress, string? country = null, string? city = null, string? region = null)
        {
            var existingDevice = await GetByDeviceIdAsync(deviceId);
            
            if (existingDevice != null && existingDevice.UserId == userId)
            {
                // Update existing device
                existingDevice.DeviceName = deviceName;
                existingDevice.DeviceType = deviceType;
                existingDevice.OperatingSystem = operatingSystem;
                existingDevice.Browser = browser;
                existingDevice.BrowserVersion = browserVersion;
                existingDevice.IpAddress = ipAddress;
                existingDevice.LastUsedAt = DateTime.UtcNow;
                existingDevice.Country = country ?? existingDevice.Country;
                existingDevice.City = city ?? existingDevice.City;
                existingDevice.Region = region ?? existingDevice.Region;
                
                return await UpdateAsync(existingDevice);
            }
            
            // Create new trusted device
            if (!Guid.TryParse(userId, out var userGuid)) throw new ArgumentException("Invalid user ID");
            var device = new TrustedDevice
            {
                UserId = userGuid,
                DeviceId = deviceId,
                DeviceName = deviceName,
                DeviceType = deviceType,
                OperatingSystem = operatingSystem,
                Browser = browser,
                BrowserVersion = browserVersion,
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow,
                Country = country,
                City = city,
                Region = region
            };
            
            return await AddAsync(device);
        }

        /// <inheritdoc/>
        public async Task<TrustedDevice> GetByDeviceIdAsync(string deviceId)
        {
            return await _dbSet.FirstOrDefaultAsync(td => td.DeviceId == deviceId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TrustedDevice>> GetByUserIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return new List<TrustedDevice>();
            return await _dbSet.Where(td => td.UserId == userGuid).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> IsDeviceTrustedAsync(string userId, string deviceId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return false;
            return await _dbSet.AnyAsync(td => td.UserId == userGuid && td.DeviceId == deviceId);
        }

        /// <inheritdoc/>
        public async Task RemoveTrustedDeviceAsync(string userId, string deviceId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return;
            var device = await _dbSet.FirstOrDefaultAsync(td => td.UserId == userGuid && td.DeviceId == deviceId);
            if (device != null)
            {
                await DeleteAsync(device);
            }
        }

        /// <inheritdoc/>
        public async Task UpdateLastUsedAsync(string userId, string deviceId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return;
            var device = await _dbSet.FirstOrDefaultAsync(td => td.UserId == userGuid && td.DeviceId == deviceId);
            if (device != null)
            {
                device.LastUsedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }

    /// <summary>
    /// Repository implementation for the login attempt entity
    /// </summary>
    public class LoginAttemptRepository : BaseAuthRepository<LoginAttempt>, ILoginAttemptRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginAttemptRepository"/> class
        /// </summary>
        /// <param name="context">The database context</param>
        public LoginAttemptRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<int> GetFailedLoginAttemptsCountAsync(string username, int timeRange = 30)
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-timeRange);
            
            return await _dbSet.CountAsync(la => 
                la.Username == username && 
                !la.Success && 
                la.AttemptTime >= cutoffTime);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<LoginAttempt>> GetByIpAddressAsync(string ipAddress)
        {
            return await _dbSet.Where(la => la.IpAddress == ipAddress)
                .OrderByDescending(la => la.AttemptTime)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<LoginAttempt>> GetByIpAddressAndTimeRangeAsync(string ipAddress, DateTime startTime, DateTime endTime)
        {
            return await _dbSet.Where(la => 
                la.IpAddress == ipAddress && 
                la.AttemptTime >= startTime && 
                la.AttemptTime <= endTime)
                .OrderByDescending(la => la.AttemptTime)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<LoginAttempt>> GetByUserIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return new List<LoginAttempt>();
            return await _dbSet.Where(la => la.UserId == userGuid)
                .OrderByDescending(la => la.AttemptTime)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<LoginAttempt>> GetByUserIdAndTimeRangeAsync(string userId, DateTime startTime, DateTime endTime)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return new List<LoginAttempt>();
            return await _dbSet.Where(la => 
                la.UserId == userGuid &&  
                la.AttemptTime >= startTime && 
                la.AttemptTime <= endTime)
                .OrderByDescending(la => la.AttemptTime)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<LoginAttempt>> GetByUsernameAsync(string username)
        {
            return await _dbSet.Where(la => la.Username == username)
                .OrderByDescending(la => la.AttemptTime)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<LoginAttempt>> GetByUsernameAndTimeRangeAsync(string username, DateTime startTime, DateTime endTime)
        {
            return await _dbSet.Where(la => 
                la.Username == username && 
                la.AttemptTime >= startTime && 
                la.AttemptTime <= endTime)
                .OrderByDescending(la => la.AttemptTime)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<LoginAttempt> RecordLoginAttemptAsync(string username, string userId, bool success, string failureReason, string ipAddress, string userAgent, string loginMethod, string? country = null, string? city = null)
        {
            Guid.TryParse(userId, out var userGuid); // Optional if UserId is nullable in LoginAttempt
            var attempt = new LoginAttempt
            {
                Username = username,
                UserId = userGuid != Guid.Empty ? userGuid : null,
                Success = success,
                FailureReason = failureReason,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                AttemptTime = DateTime.UtcNow,
                Country = country,
                City = city,
                LoginMethod = loginMethod
            };
            
            return await AddAsync(attempt);
        }
    }

    /// <summary>
    /// Repository implementation for the social login profile entity
    /// </summary>
    public class SocialLoginProfileRepository : BaseAuthRepository<SocialLoginProfile>, ISocialLoginProfileRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SocialLoginProfileRepository"/> class
        /// </summary>
        /// <param name="context">The database context</param>
        public SocialLoginProfileRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<SocialLoginProfile> AddSocialLoginAsync(string userId, string provider, string providerKey, string providerDisplayName)
        {
            var existingProfile = await GetByProviderAndKeyAsync(provider, providerKey);
            
            if (existingProfile != null)
            {
                if (existingProfile.UserId != Guid.Parse(userId))
                {
                    throw new InvalidOperationException("This social account is already linked to a different user.");
                }
                
                existingProfile.LastUsedAt = DateTime.UtcNow;
                return await UpdateAsync(existingProfile);
            }
            
            var profile = new SocialLoginProfile
            {
                UserId = Guid.Parse(userId),
                Provider = provider,
                ProviderKey = providerKey,
                ProviderDisplayName = providerDisplayName,
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow
            };
            
            return await AddAsync(profile);
        }

        /// <inheritdoc/>
        public async Task<SocialLoginProfile> GetByProviderAndKeyAsync(string provider, string providerKey)
        {
            return await _dbSet.FirstOrDefaultAsync(slp => 
                slp.Provider == provider && 
                slp.ProviderKey == providerKey);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SocialLoginProfile>> GetByUserIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return new List<SocialLoginProfile>();
            return await _dbSet.Where(slp => slp.UserId == userGuid).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task RemoveSocialLoginAsync(string userId, string provider)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return;
            var profile = await _dbSet.FirstOrDefaultAsync(slp => 
                slp.UserId == userGuid && 
                slp.Provider == provider);
                
            if (profile != null)
            {
                await DeleteAsync(profile);
            }
        }

        /// <inheritdoc/>
        public async Task UpdateLastUsedAsync(string userId, string provider)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return;
            var profile = await _dbSet.FirstOrDefaultAsync(slp => 
                slp.UserId == userGuid && 
                slp.Provider == provider);
                
            if (profile != null)
            {
                profile.LastUsedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }

    /// <summary>
    /// Repository implementation for the security alert entity
    /// </summary>
    public class SecurityAlertRepository : BaseAuthRepository<SecurityAlert>, ISecurityAlertRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAlertRepository"/> class
        /// </summary>
        /// <param name="context">The database context</param>
        public SecurityAlertRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<SecurityAlert> CreateAlertAsync(string userId, string alertType, string message, string details, string severity, string? ipAddress = null, string? deviceId = null)
        {
            Enum.TryParse<SecurityAlertType>(alertType, true, out var typeEnum);
            Enum.TryParse<SecurityAlertSeverity>(severity, true, out var severityEnum);
            Guid.TryParse(userId, out var userGuid);

            var alert = new SecurityAlert
            {
                UserId = userGuid,
                AlertType = typeEnum,
                Message = message,
                Details = details,
                Severity = severityEnum,
                IpAddress = ipAddress,
                DeviceId = deviceId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            
            return await AddAsync(alert);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SecurityAlert>> GetByUserIdAndSeverityAsync(string userId, string severity)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return new List<SecurityAlert>();
            Enum.TryParse<SecurityAlertSeverity>(severity, true, out var severityEnum);

            return await _dbSet.Where(sa => 
                sa.UserId == userGuid && 
                sa.Severity == severityEnum)
                .OrderByDescending(sa => sa.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SecurityAlert>> GetByUserIdAndTypeAsync(string userId, string alertType)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return new List<SecurityAlert>();
            Enum.TryParse<SecurityAlertType>(alertType, true, out var typeEnum);

            return await _dbSet.Where(sa => 
                sa.UserId == userGuid && 
                sa.AlertType == typeEnum)
                .OrderByDescending(sa => sa.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SecurityAlert>> GetByUserIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return new List<SecurityAlert>();

            return await _dbSet.Where(sa => sa.UserId == userGuid)
                .OrderByDescending(sa => sa.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SecurityAlert>> GetUnreadByUserIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return new List<SecurityAlert>();

            return await _dbSet.Where(sa => 
                sa.UserId == userGuid && 
                !sa.IsRead)
                .OrderByDescending(sa => sa.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task MarkAllAsReadAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return;

            var alerts = await _dbSet.Where(sa => 
                sa.UserId == userGuid && 
                !sa.IsRead)
                .ToListAsync();
                
            var now = DateTime.UtcNow;
            
            foreach (var alert in alerts)
            {
                alert.IsRead = true;
                alert.ReadAt = now;
            }
            
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> MarkAsReadAsync(string alertId, string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return false;
            var alert = await _dbSet.FirstOrDefaultAsync(sa => 
                sa.Id == alertId && 
                sa.UserId == userGuid && 
                !sa.IsRead);
                
            if (alert == null)
            {
                return false;
            }
            
            alert.IsRead = true;
            alert.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            return true;
        }
    }

    /// <summary>
    /// Repository implementation for the user security preferences entity
    /// </summary>
    public class UserSecurityPreferencesRepository : BaseAuthRepository<UserSecurityPreferences>, IUserSecurityPreferencesRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserSecurityPreferencesRepository"/> class
        /// </summary>
        /// <param name="context">The database context</param>
        public UserSecurityPreferencesRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<UserSecurityPreferences> GetByUserIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return null;
            var preferences = await _dbSet.FirstOrDefaultAsync(usp => usp.UserId == userGuid);
            
            if (preferences == null)
            {
                // Create default preferences if none exist
                preferences = new UserSecurityPreferences
                {
                    UserId = userGuid,
                    NotifyOnNewLogin = true,
                    NotifyOnSuspiciousActivity = true,
                    UseLocationBasedSecurity = true,
                    RequireMfaForSuspiciousLogins = true,
                    AllowPasswordReset = true,
                    SessionTimeoutMinutes = 30,
                    MaxConcurrentSessions = 5,
                    MaxFailedLoginAttempts = 5,
                    LockoutDurationMinutes = 15,
                    UsePasswordlessLogin = false
                };
                
                await AddAsync(preferences);
            }
            
            return preferences;
        }

        /// <inheritdoc/>
        public async Task<UserSecurityPreferences> UpdatePreferencesAsync(string userId, UserSecurityPreferences preferences)
        {
            if (!Guid.TryParse(userId, out var userGuid)) throw new ArgumentException("Invalid User ID");
            var existingPreferences = await _dbSet.FirstOrDefaultAsync(usp => usp.UserId == userGuid);
            
            if (existingPreferences == null)
            {
                preferences.UserId = userGuid;
                return await AddAsync(preferences);
            }
            
            // Update existing preferences
            existingPreferences.NotifyOnNewLogin = preferences.NotifyOnNewLogin;
            existingPreferences.NotifyOnSuspiciousActivity = preferences.NotifyOnSuspiciousActivity;
            existingPreferences.UseLocationBasedSecurity = preferences.UseLocationBasedSecurity;
            existingPreferences.RequireMfaForSuspiciousLogins = preferences.RequireMfaForSuspiciousLogins;
            existingPreferences.AllowPasswordReset = preferences.AllowPasswordReset;
            existingPreferences.SessionTimeoutMinutes = preferences.SessionTimeoutMinutes;
            existingPreferences.MaxConcurrentSessions = preferences.MaxConcurrentSessions;
            existingPreferences.MaxFailedLoginAttempts = preferences.MaxFailedLoginAttempts;
            existingPreferences.LockoutDurationMinutes = preferences.LockoutDurationMinutes;
            existingPreferences.UsePasswordlessLogin = preferences.UsePasswordlessLogin;
            
            return await UpdateAsync(existingPreferences);
        }
    }

    /// <summary>
    /// Repository implementation for security activities
    /// </summary>
    public class SecurityActivityRepository : BaseAuthRepository<SecurityActivity>, ISecurityActivityRepository
    {
        public SecurityActivityRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<SecurityActivity> LogActivityAsync(string userId, string eventType, string details, string ipAddress, string userAgent)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return null;

            var activity = new SecurityActivity
            {
                UserId = userGuid, // Guid
                EventType = eventType,
                Details = details,
                IpAddress = ipAddress,
                DeviceInfo = userAgent,
                Timestamp = DateTime.UtcNow,
                Status = "Success",
                Location = "Unknown" 
            };
            
            return await AddAsync(activity);
        }

        public async Task<IEnumerable<SecurityActivity>> GetRecentActivityAsync(string userId, int count = 10)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return new List<SecurityActivity>();
            
            return await _dbSet.Where(a => a.UserId == userGuid)
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<SecurityActivity>> GetByUserIdAsync(string userId)
        {
             if (!Guid.TryParse(userId, out var userGuid)) return new List<SecurityActivity>();
            
            return await _dbSet.Where(a => a.UserId == userGuid)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
    }
}
