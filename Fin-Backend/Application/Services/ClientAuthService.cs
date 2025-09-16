using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using FinTech.Application.Common.Interfaces;
using FinTech.Application.DTOs.Auth;
using FinTech.Application.DTOs.Common;
using FinTech.Domain.Entities.ClientPortal;
using FinTech.Domain.Entities.Identity;
using FinTech.Infrastructure.Extensions;
using FinTech.WebAPI.Domain.Entities.Auth;

namespace FinTech.Application.Services
{
    public interface IClientAuthService
    {
        Task<LoginResponse> ClientLoginAsync(LoginRequest request, string ipAddress, string userAgent, string deviceId);
        Task<BaseResponse<bool>> ClientLogoutAsync(Guid clientId, string sessionToken);
        Task<BaseResponse<string>> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task<BaseResponse<bool>> ValidateClientSessionAsync(Guid clientId, string sessionToken);
        Task<BaseResponse<bool>> SendPasswordResetLinkAsync(string email);
        Task<BaseResponse<bool>> ResetPasswordAsync(string email, string token, string newPassword);
        Task<BaseResponse<bool>> VerifyTwoFactorCodeAsync(Guid clientId, string code);
        Task<BaseResponse<bool>> SendTwoFactorCodeAsync(Guid clientId, string method);
    }

    public class ClientAuthService : IClientAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ClientAuthService> _logger;
        private readonly IOptions<ClientPortalSettings> _clientPortalSettings;

        public ClientAuthService(
            UserManager<ApplicationUser> userManager,
            IApplicationDbContext context,
            IConfiguration configuration,
            INotificationService notificationService,
            ILogger<ClientAuthService> logger,
            IOptions<ClientPortalSettings> clientPortalSettings)
        {
            _userManager = userManager;
            _context = context;
            _configuration = configuration;
            _notificationService = notificationService;
            _logger = logger;
            _clientPortalSettings = clientPortalSettings;
        }

        public async Task<LoginResponse> ClientLoginAsync(LoginRequest request, string ipAddress, string userAgent, string deviceId)
        {
            try
            {
                // Find user by email
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                // Check if user is in client role
                var isInClientRole = await _userManager.IsInRoleAsync(user, "Client");
                if (!isInClientRole)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "User is not authorized to access client portal"
                    };
                }

                // Validate password
                var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!isPasswordValid)
                {
                    // Increment failed login attempts
                    var profile = await _context.ClientPortalProfiles
                        .FirstOrDefaultAsync(p => p.CustomerId == Guid.Parse(user.CustomerId));
                    
                    if (profile != null)
                    {
                        profile.FailedLoginAttempts += 1;
                        
                        // Lock account if max attempts reached
                        if (profile.FailedLoginAttempts >= _clientPortalSettings.Value.MaxFailedLoginAttempts)
                        {
                            profile.IsLocked = true;
                            profile.LockoutEnd = DateTime.UtcNow.AddMinutes(_clientPortalSettings.Value.LockoutDurationMinutes);
                            
                            // Notify user about account lockout
                            await _notificationService.SendNotificationAsync(
                                Guid.Parse(user.CustomerId),
                                "Account Security Alert",
                                $"Your account has been locked due to multiple failed login attempts. " +
                                $"It will be unlocked after {_clientPortalSettings.Value.LockoutDurationMinutes} minutes, " +
                                $"or you can reset your password to unlock it immediately.",
                                "security",
                                new Dictionary<string, string> { { "action", "account_locked" } }
                            );
                        }
                        
                        await _context.SaveChangesAsync();
                    }
                    
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                // Get customer ID
                var customerId = Guid.Parse(user.CustomerId);

                // Check if account is locked
                var clientProfile = await _context.ClientPortalProfiles
                    .FirstOrDefaultAsync(p => p.CustomerId == customerId);
                
                if (clientProfile == null)
                {
                    // Create new profile if it doesn't exist
                    clientProfile = new ClientPortalProfile
                    {
                        CustomerId = customerId,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    _context.ClientPortalProfiles.Add(clientProfile);
                    await _context.SaveChangesAsync();
                }
                else if (clientProfile.IsLocked)
                {
                    if (clientProfile.LockoutEnd.HasValue && clientProfile.LockoutEnd.Value > DateTime.UtcNow)
                    {
                        var remainingMinutes = (int)Math.Ceiling((clientProfile.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                        return new LoginResponse
                        {
                            Success = false,
                            Message = $"Your account is locked. Please try again after {remainingMinutes} minutes or reset your password."
                        };
                    }
                    else
                    {
                        // Unlock account if lockout period has expired
                        clientProfile.IsLocked = false;
                        clientProfile.LockoutEnd = null;
                        clientProfile.FailedLoginAttempts = 0;
                        await _context.SaveChangesAsync();
                    }
                }

                // Reset failed login attempts
                clientProfile.FailedLoginAttempts = 0;
                clientProfile.LastLoginDate = DateTime.UtcNow;
                clientProfile.DeviceInfo = $"{userAgent} | {deviceId}";
                await _context.SaveChangesAsync();

                // Generate JWT token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Secret"]);
                var tokenExpiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"]);
                
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("CustomerId", user.CustomerId),
                    new Claim(ClaimTypes.Role, "Client")
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(tokenExpiryMinutes),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwt = tokenHandler.WriteToken(token);

                // Generate refresh token
                var refreshToken = Guid.NewGuid().ToString();
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

                // Create new client session
                var session = new ClientSession
                {
                    ClientPortalProfileId = clientProfile.Id,
                    SessionToken = refreshToken,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    DeviceId = deviceId,
                    LoginTime = DateTime.UtcNow,
                    IsActive = true,
                    ExpiresAt = refreshTokenExpiry
                };

                // Try to get location info based on IP (simplified)
                try
                {
                    session.Location = "Unknown"; // In a real app, use an IP geolocation service
                }
                catch { /* ignore geolocation errors */ }

                _context.ClientSessions.Add(session);
                await _context.SaveChangesAsync();

                // Check if this is a new device
                var existingDevice = await _context.ClientDevices
                    .FirstOrDefaultAsync(d => d.ClientPortalProfileId == clientProfile.Id && d.DeviceId == deviceId);

                if (existingDevice == null)
                {
                    // Create new device record
                    var newDevice = new ClientDevice
                    {
                        ClientPortalProfileId = clientProfile.Id,
                        DeviceId = deviceId,
                        DeviceName = "Unknown Device",
                        DeviceType = "Unknown",
                        OperatingSystem = "Unknown",
                        Browser = "Unknown",
                        IpAddress = ipAddress,
                        Location = session.Location,
                        CreatedAt = DateTime.UtcNow,
                        LastUsed = DateTime.UtcNow
                    };

                    // Parse user agent to get device info (simplified)
                    if (!string.IsNullOrEmpty(userAgent))
                    {
                        if (userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
                        {
                            newDevice.DeviceType = "Mobile";
                            newDevice.OperatingSystem = "iOS";
                            newDevice.DeviceName = userAgent.Contains("iPhone") ? "iPhone" : "iPad";
                        }
                        else if (userAgent.Contains("Android"))
                        {
                            newDevice.DeviceType = "Mobile";
                            newDevice.OperatingSystem = "Android";
                            newDevice.DeviceName = "Android Device";
                        }
                        else if (userAgent.Contains("Windows"))
                        {
                            newDevice.DeviceType = "Desktop";
                            newDevice.OperatingSystem = "Windows";
                            newDevice.DeviceName = "Windows PC";
                        }
                        else if (userAgent.Contains("Mac"))
                        {
                            newDevice.DeviceType = "Desktop";
                            newDevice.OperatingSystem = "macOS";
                            newDevice.DeviceName = "Mac";
                        }
                        else if (userAgent.Contains("Linux"))
                        {
                            newDevice.DeviceType = "Desktop";
                            newDevice.OperatingSystem = "Linux";
                            newDevice.DeviceName = "Linux PC";
                        }

                        if (userAgent.Contains("Chrome"))
                            newDevice.Browser = "Chrome";
                        else if (userAgent.Contains("Firefox"))
                            newDevice.Browser = "Firefox";
                        else if (userAgent.Contains("Safari"))
                            newDevice.Browser = "Safari";
                        else if (userAgent.Contains("Edge"))
                            newDevice.Browser = "Edge";
                    }

                    _context.ClientDevices.Add(newDevice);
                    await _context.SaveChangesAsync();

                    // Send notification about new device login
                    await _notificationService.SendNotificationAsync(
                        customerId,
                        "New Device Login",
                        $"A new device was used to log into your account. Device: {newDevice.DeviceName}, Location: {newDevice.Location}. " +
                        "If this wasn't you, please contact customer support immediately.",
                        "security",
                        new Dictionary<string, string> { { "action", "new_device_login" } }
                    );
                }
                else
                {
                    // Update existing device info
                    existingDevice.LastUsed = DateTime.UtcNow;
                    existingDevice.IpAddress = ipAddress;
                    existingDevice.Location = session.Location;
                    await _context.SaveChangesAsync();
                }

                // Check if two-factor authentication is required
                bool requiresTwoFactor = clientProfile.TwoFactorAuthEnabled;
                string twoFactorToken = null;

                if (requiresTwoFactor)
                {
                    // Generate and send two-factor code
                    twoFactorToken = await GenerateTwoFactorTokenAsync(user, clientProfile.TwoFactorAuthType);
                }

                // Return login response
                return new LoginResponse
                {
                    Success = true,
                    Message = requiresTwoFactor ? "Two-factor authentication required" : "Login successful",
                    Token = jwt,
                    RefreshToken = refreshToken,
                    ExpiresAt = tokenDescriptor.Expires.Value,
                    RequiresTwoFactor = requiresTwoFactor,
                    TwoFactorMethod = requiresTwoFactor ? clientProfile.TwoFactorAuthType : null,
                    UserId = user.Id,
                    CustomerId = customerId,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during client login");
                return new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred during login. Please try again."
                };
            }
        }

        public async Task<BaseResponse<bool>> ClientLogoutAsync(Guid clientId, string sessionToken)
        {
            try
            {
                // Find client profile
                var profile = await _context.ClientPortalProfiles
                    .FirstOrDefaultAsync(p => p.CustomerId == clientId);

                if (profile == null)
                {
                    return new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Client profile not found",
                        Data = false
                    };
                }

                // Find and update the session
                var session = await _context.ClientSessions
                    .FirstOrDefaultAsync(s => s.ClientPortalProfileId == profile.Id && s.SessionToken == sessionToken);

                if (session == null)
                {
                    return new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Session not found",
                        Data = false
                    };
                }

                // Update session
                session.IsActive = false;
                session.LogoutTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new BaseResponse<bool>
                {
                    Success = true,
                    Message = "Logged out successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during client logout");
                return new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred during logout",
                    Data = false
                };
            }
        }

        public async Task<BaseResponse<string>> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            try
            {
                // Find session by refresh token
                var session = await _context.ClientSessions
                    .Include(s => s.ClientPortalProfile)
                    .FirstOrDefaultAsync(s => s.SessionToken == refreshToken && s.IsActive);

                if (session == null || session.ExpiresAt < DateTime.UtcNow)
                {
                    return new BaseResponse<string>
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token",
                        Data = null
                    };
                }

                // Get user by customer ID
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == session.ClientPortalProfile.CustomerId);

                if (customer == null)
                {
                    return new BaseResponse<string>
                    {
                        Success = false,
                        Message = "Customer not found",
                        Data = null
                    };
                }

                var user = await _userManager.FindByIdAsync(customer.UserId.ToString());
                if (user == null)
                {
                    return new BaseResponse<string>
                    {
                        Success = false,
                        Message = "User not found",
                        Data = null
                    };
                }

                // Generate new JWT token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Secret"]);
                var tokenExpiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"]);
                
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("CustomerId", user.CustomerId),
                    new Claim(ClaimTypes.Role, "Client")
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(tokenExpiryMinutes),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwt = tokenHandler.WriteToken(token);

                // Update session's IP address
                session.IpAddress = ipAddress;
                await _context.SaveChangesAsync();

                return new BaseResponse<string>
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    Data = jwt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return new BaseResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while refreshing token",
                    Data = null
                };
            }
        }

        public async Task<BaseResponse<bool>> ValidateClientSessionAsync(Guid clientId, string sessionToken)
        {
            try
            {
                // Find client profile
                var profile = await _context.ClientPortalProfiles
                    .FirstOrDefaultAsync(p => p.CustomerId == clientId);

                if (profile == null)
                {
                    return new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Client profile not found",
                        Data = false
                    };
                }

                // Find session
                var session = await _context.ClientSessions
                    .FirstOrDefaultAsync(s => 
                        s.ClientPortalProfileId == profile.Id && 
                        s.SessionToken == sessionToken && 
                        s.IsActive);

                if (session == null || session.ExpiresAt < DateTime.UtcNow)
                {
                    return new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid or expired session",
                        Data = false
                    };
                }

                return new BaseResponse<bool>
                {
                    Success = true,
                    Message = "Session is valid",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating client session");
                return new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while validating session",
                    Data = false
                };
            }
        }

        public async Task<BaseResponse<bool>> SendPasswordResetLinkAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Return success even if user not found to prevent email enumeration
                    return new BaseResponse<bool>
                    {
                        Success = true,
                        Message = "If your email is registered, you will receive a password reset link shortly",
                        Data = true
                    };
                }

                // Check if user is in client role
                var isInClientRole = await _userManager.IsInRoleAsync(user, "Client");
                if (!isInClientRole)
                {
                    // Return success even if user is not a client to prevent role enumeration
                    return new BaseResponse<bool>
                    {
                        Success = true,
                        Message = "If your email is registered, you will receive a password reset link shortly",
                        Data = true
                    };
                }

                // Generate password reset token
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                
                // Encode token to make it URL-safe
                var encodedToken = System.Web.HttpUtility.UrlEncode(token);
                
                // Create reset link
                var resetLink = $"{_clientPortalSettings.Value.BaseUrl}/reset-password?email={email}&token={encodedToken}";
                
                // Send email notification
                await _notificationService.SendNotificationAsync(
                    Guid.Parse(user.CustomerId),
                    "Password Reset Request",
                    $"You have requested to reset your password. Please click the link below to reset your password. If you did not request this, please ignore this email.<br><br><a href='{resetLink}'>Reset Password</a><br><br>This link will expire in 24 hours.",
                    "security",
                    new Dictionary<string, string> { { "action", "reset_password" } },
                    new[] { "email" } // Only send via email
                );

                return new BaseResponse<bool>
                {
                    Success = true,
                    Message = "If your email is registered, you will receive a password reset link shortly",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset link");
                return new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while sending the password reset link",
                    Data = false
                };
            }
        }

        public async Task<BaseResponse<bool>> ResetPasswordAsync(string email, string token, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request",
                        Data = false
                    };
                }

                // Check if user is in client role
                var isInClientRole = await _userManager.IsInRoleAsync(user, "Client");
                if (!isInClientRole)
                {
                    return new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request",
                        Data = false
                    };
                }

                // Reset password
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new BaseResponse<bool>
                    {
                        Success = false,
                        Message = $"Failed to reset password: {errors}",
                        Data = false
                    };
                }

                // Unlock account if it was locked
                var profile = await _context.ClientPortalProfiles
                    .FirstOrDefaultAsync(p => p.CustomerId == Guid.Parse(user.CustomerId));
                
                if (profile != null && profile.IsLocked)
                {
                    profile.IsLocked = false;
                    profile.LockoutEnd = null;
                    profile.FailedLoginAttempts = 0;
                    await _context.SaveChangesAsync();
                }

                // Invalidate all existing sessions
                var sessions = await _context.ClientSessions
                    .Where(s => s.ClientPortalProfile.CustomerId == Guid.Parse(user.CustomerId) && s.IsActive)
                    .ToListAsync();
                
                foreach (var session in sessions)
                {
                    session.IsActive = false;
                    session.LogoutTime = DateTime.UtcNow;
                }
                
                await _context.SaveChangesAsync();

                // Send notification about password change
                await _notificationService.SendNotificationAsync(
                    Guid.Parse(user.CustomerId),
                    "Password Reset Successful",
                    "Your password has been successfully reset. If you did not make this change, please contact customer support immediately.",
                    "security",
                    new Dictionary<string, string> { { "action", "password_reset_success" } }
                );

                return new BaseResponse<bool>
                {
                    Success = true,
                    Message = "Password has been reset successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while resetting password",
                    Data = false
                };
            }
        }

        public async Task<BaseResponse<bool>> VerifyTwoFactorCodeAsync(Guid clientId, string code)
        {
            try
            {
                // Find user by customer ID
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == clientId);
                if (customer == null)
                {
                    return new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Customer not found",
                        Data = false
                    };
                }

                var user = await _userManager.FindByIdAsync(customer.UserId.ToString());
                if (user == null)
                {
                    return new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "User not found",
                        Data = false
                    };
                }

                // Get client profile
                var profile = await _context.ClientPortalProfiles
                    .FirstOrDefaultAsync(p => p.CustomerId == clientId);
                
                if (profile == null || !profile.TwoFactorAuthEnabled)
                {
                    return new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Two-factor authentication is not enabled",
                        Data = false
                    };
                }

                // Verify code based on the 2FA type
                bool isValid = false;
                
                if (profile.TwoFactorAuthType == "sms" || profile.TwoFactorAuthType == "email")
                {
                    // Verify token
                    isValid = await _userManager.VerifyTwoFactorTokenAsync(
                        user,
                        "Default", // Default provider for email/SMS
                        code
                    );
                }
                else if (profile.TwoFactorAuthType == "authenticator")
                {
                    // Verify authenticator app code
                    isValid = await _userManager.VerifyTwoFactorTokenAsync(
                        user,
                        _userManager.Options.Tokens.AuthenticatorTokenProvider,
                        code
                    );
                }

                if (!isValid)
                {
                    return new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid verification code",
                        Data = false
                    };
                }

                return new BaseResponse<bool>
                {
                    Success = true,
                    Message = "Two-factor authentication successful",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying two-factor code");
                return new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while verifying the code",
                    Data = false
                };
            }
        }

        public async Task<BaseResponse<bool>> SendTwoFactorCodeAsync(Guid clientId, string method)
        {
            try
            {
                // Find user by customer ID
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == clientId);
                if (customer == null)
                {
                    return new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Customer not found",
                        Data = false
                    };
                }

                var user = await _userManager.FindByIdAsync(customer.UserId.ToString());
                if (user == null)
                {
                    return new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "User not found",
                        Data = false
                    };
                }

                // Get client profile
                var profile = await _context.ClientPortalProfiles
                    .FirstOrDefaultAsync(p => p.CustomerId == clientId);
                
                if (profile == null)
                {
                    return new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Client profile not found",
                        Data = false
                    };
                }

                // Generate token
                var token = await GenerateTwoFactorTokenAsync(user, method);
                if (string.IsNullOrEmpty(token))
                {
                    return new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Failed to generate verification code",
                        Data = false
                    };
                }

                return new BaseResponse<bool>
                {
                    Success = true,
                    Message = $"Verification code sent via {method}",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending two-factor code");
                return new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while sending verification code",
                    Data = false
                };
            }
        }

        private async Task<string> GenerateTwoFactorTokenAsync(ApplicationUser user, string method)
        {
            if (string.IsNullOrEmpty(method))
            {
                return null;
            }

            string token = null;

            if (method == "sms" || method == "email")
            {
                // Generate numeric code for SMS/email
                token = await _userManager.GenerateTwoFactorTokenAsync(user, "Default");

                // Send token via the appropriate channel
                if (method == "sms")
                {
                    // Send SMS notification with token
                    await _notificationService.SendNotificationAsync(
                        Guid.Parse(user.CustomerId),
                        "Your verification code",
                        $"Your verification code is: {token}",
                        "security",
                        new Dictionary<string, string> { { "action", "two_factor_code" } },
                        new[] { "sms" } // Only send via SMS
                    );
                }
                else if (method == "email")
                {
                    // Send email notification with token
                    await _notificationService.SendNotificationAsync(
                        Guid.Parse(user.CustomerId),
                        "Your verification code",
                        $"Your verification code is: {token}<br><br>This code will expire in 10 minutes.",
                        "security",
                        new Dictionary<string, string> { { "action", "two_factor_code" } },
                        new[] { "email" } // Only send via email
                    );
                }
            }
            else if (method == "authenticator")
            {
                // For authenticator apps, we don't generate or send a code
                // The user will use their authenticator app to generate a code
                // This is just a placeholder to indicate success
                token = "generated";
            }

            return token;
        }
    }
}