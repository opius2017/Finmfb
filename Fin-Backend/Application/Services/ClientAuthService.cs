using FinTech.Application.Common;
using FinTech.Application.Common.Interfaces;
using FinTech.Application.Common.Models;
using FinTech.Application.DTOs.Auth;
using FinTech.Application.DTOs.Notification;
using FinTech.Domain.Entities.ClientPortal;
using FinTech.Domain.Entities.Identity;
using FinTech.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace FinTech.Application.Services
{
    public interface IClientAuthService
    {
        Task<BaseResponse<LoginResponse>> ClientLoginAsync(LoginRequest request, string ipAddress, string userAgent, string deviceId);
        Task<BaseResponse<bool>> ClientLogoutAsync(Guid customerId, string sessionToken);
        Task<BaseResponse<string>> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task<BaseResponse<bool>> ValidateClientSessionAsync(Guid customerId, string sessionToken);
        Task<BaseResponse<bool>> SendPasswordResetLinkAsync(string email);
        Task<BaseResponse<bool>> ResetPasswordAsync(string email, string token, string newPassword);
        Task<BaseResponse<bool>> VerifyTwoFactorCodeAsync(Guid customerId, string code);
        Task<BaseResponse<bool>> SendTwoFactorCodeAsync(Guid customerId, string method);
    }

    public class ClientAuthService : IClientAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationDbContext _context;
        private readonly ILogger<ClientAuthService> _logger;
        private readonly IOptions<ClientPortalSettings> _clientPortalSettings;
        private readonly IJwtService _jwtService;
        private readonly INotificationService _notificationService;

        public ClientAuthService(
            UserManager<ApplicationUser> userManager,
            IApplicationDbContext context,
            ILogger<ClientAuthService> logger,
            IOptions<ClientPortalSettings> clientPortalSettings,
            IJwtService jwtService,
            INotificationService notificationService)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _clientPortalSettings = clientPortalSettings;
            _jwtService = jwtService;
            _notificationService = notificationService;
        }

        public async Task<BaseResponse<LoginResponse>> ClientLoginAsync(LoginRequest request, string ipAddress, string userAgent, string deviceId)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return BaseResponse<LoginResponse>.Failure("Invalid email or password");
                }

                var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!isPasswordValid)
                {
                    return BaseResponse<LoginResponse>.Failure("Invalid email or password");
                }

                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("Client"))
                {
                    return BaseResponse<LoginResponse>.Failure("User is not authorized to access client portal");
                }

                var customerId = Guid.Parse(user.CustomerId);
                var clientProfile = await _context.ClientPortalProfiles.FirstOrDefaultAsync(p => p.CustomerId == customerId);

                if (clientProfile == null)
                {
                    clientProfile = new ClientPortalProfile { CustomerId = customerId, CreatedAt = DateTime.UtcNow };
                    _context.ClientPortalProfiles.Add(clientProfile);
                    await _context.SaveChangesAsync(CancellationToken.None);
                }
                else if (clientProfile.IsLocked)
                {
                    if (clientProfile.LockoutEnd.HasValue && clientProfile.LockoutEnd.Value > DateTime.UtcNow)
                    {
                        var remainingMinutes = (int)Math.Ceiling((clientProfile.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                        return BaseResponse<LoginResponse>.Failure($"Your account is locked. Please try again after {remainingMinutes} minutes or reset your password.");
                    }
                    else
                    {
                        clientProfile.IsLocked = false;
                        clientProfile.LockoutEnd = null;
                        clientProfile.FailedLoginAttempts = 0;
                    }
                }

                clientProfile.FailedLoginAttempts = 0;
                clientProfile.LastLoginDate = DateTime.UtcNow;
                clientProfile.DeviceInfo = $"{userAgent} | {deviceId}";
                await _context.SaveChangesAsync(CancellationToken.None);

                var (jwt, refreshToken, expiresAt) = await _jwtService.GenerateTokensAsync(user, roles);

                var session = new ClientSession
                {
                    ClientPortalProfileId = clientProfile.Id,
                    Token = refreshToken,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    ExpiresAt = DateTime.UtcNow.AddDays(7) // Refresh token expiry
                };
                _context.ClientSessions.Add(session);
                await _context.SaveChangesAsync(CancellationToken.None);

                var existingDevice = await _context.ClientDevices.FirstOrDefaultAsync(d => d.ClientPortalProfileId == clientProfile.Id && d.DeviceId == deviceId);
                if (existingDevice == null)
                {
                    var newDevice = new ClientDevice
                    {
                        ClientPortalProfileId = clientProfile.Id,
                        DeviceId = deviceId,
                        DeviceType = "Unknown", // Or use a library to parse user agent
                        IsActive = true,
                        LastLoginDate = DateTime.UtcNow
                    };
                    _context.ClientDevices.Add(newDevice);
                    await _context.SaveChangesAsync(CancellationToken.None);

                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        CustomerId = customerId,
                        Title = "New Device Login",
                        Message = $"A new device was used to log into your account. If this wasn't you, please contact customer support immediately.",
                        NotificationType = "security",
                        Action = "new_device_login"
                    });
                }

                bool requiresTwoFactor = clientProfile.TwoFactorEnabled;
                if (requiresTwoFactor)
                {
                    await SendTwoFactorCodeAsync(user.Id, clientProfile.TwoFactorMethod);
                }

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    Roles = roles
                };

                var loginResponse = new LoginResponse
                {
                    Token = jwt,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt,
                    User = userDto,
                    RequiresTwoFactor = requiresTwoFactor,
                };

                return BaseResponse<LoginResponse>.Success(loginResponse, requiresTwoFactor ? "Two-factor authentication required" : "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during client login for email {Email}", request.Email);
                return BaseResponse<LoginResponse>.Failure("An unexpected error occurred during login. Please try again.");
            }
        }

        public async Task<BaseResponse<bool>> ClientLogoutAsync(Guid customerId, string sessionToken)
        {
            try
            {
                var profile = await _context.ClientPortalProfiles.FirstOrDefaultAsync(p => p.CustomerId == customerId);
                if (profile == null)
                {
                    return BaseResponse<bool>.Failure("Client profile not found");
                }

                var session = await _context.ClientSessions.FirstOrDefaultAsync(s => s.ClientPortalProfileId == profile.Id && s.Token == sessionToken);
                if (session != null)
                {
                    _context.ClientSessions.Remove(session);
                    await _context.SaveChangesAsync(CancellationToken.None);
                }

                return BaseResponse<bool>.Success(true, "Logged out successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during client logout for customer {CustomerId}", customerId);
                return BaseResponse<bool>.Failure("An error occurred during logout");
            }
        }

        public async Task<BaseResponse<string>> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            try
            {
                var session = await _context.ClientSessions
                    .Include(s => s.ClientPortalProfile)
                    .ThenInclude(p => p.Customer)
                    .FirstOrDefaultAsync(s => s.Token == refreshToken && s.ExpiresAt > DateTime.UtcNow);

                if (session == null)
                {
                    return BaseResponse<string>.Failure("Invalid or expired refresh token");
                }

                var user = await _userManager.FindByIdAsync(session.ClientPortalProfile.Customer.Id.ToString());
                if (user == null)
                {
                    return BaseResponse<string>.Failure("User not found");
                }
                var roles = await _userManager.GetRolesAsync(user);

                var (jwt, newRefreshToken, expiresAt) = await _jwtService.GenerateTokensAsync(user, roles);

                session.Token = newRefreshToken;
                session.IpAddress = ipAddress;
                session.ExpiresAt = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync(CancellationToken.None);

                return BaseResponse<string>.Success(jwt, "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return BaseResponse<string>.Failure("An error occurred while refreshing token");
            }
        }

        public async Task<BaseResponse<bool>> ValidateClientSessionAsync(Guid customerId, string sessionToken)
        {
            try
            {
                var profile = await _context.ClientPortalProfiles.FirstOrDefaultAsync(p => p.CustomerId == customerId);
                if (profile == null)
                {
                    return BaseResponse<bool>.Failure("Client profile not found");
                }

                var session = await _context.ClientSessions.FirstOrDefaultAsync(s => s.ClientPortalProfileId == profile.Id && s.Token == sessionToken && s.ExpiresAt > DateTime.UtcNow);
                if (session == null)
                {
                    return BaseResponse<bool>.Failure("Invalid or expired session");
                }

                return BaseResponse<bool>.Success(true, "Session is valid");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating client session for customer {CustomerId}", customerId);
                return BaseResponse<bool>.Failure("An error occurred while validating session");
            }
        }

        public async Task<BaseResponse<bool>> SendPasswordResetLinkAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null || !(await _userManager.GetRolesAsync(user)).Contains("Client"))
                {
                    // Don't reveal that the user does not exist or is not a client.
                    return BaseResponse<bool>.Success(true, "If your email is registered, you will receive a password reset link shortly.");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = HttpUtility.UrlEncode(token);
                var resetLink = $"{_clientPortalSettings.Value.BaseUrl}/reset-password?email={email}&token={encodedToken}";

                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    CustomerId = Guid.Parse(user.CustomerId),
                    Title = "Password Reset Request",
                    Message = $"You have requested to reset your password. Please click the link below to reset your password. If you did not request this, please ignore this email.<br><br><a href='{resetLink}'>Reset Password</a><br><br>This link will expire in 24 hours.",
                    NotificationType = "security",
                    Action = "reset_password",
                    DeliveryChannels = new[] { NotificationChannel.Email }
                });

                return BaseResponse<bool>.Success(true, "If your email is registered, you will receive a password reset link shortly.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset link for email {Email}", email);
                return BaseResponse<bool>.Failure("An error occurred while sending the password reset link.");
            }
        }

        public async Task<BaseResponse<bool>> ResetPasswordAsync(string email, string token, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null || !(await _userManager.GetRolesAsync(user)).Contains("Client"))
                {
                    return BaseResponse<bool>.Failure("Invalid request. Please try the password reset process again.");
                }

                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BaseResponse<bool>.Failure($"Failed to reset password: {errors}");
                }

                var profile = await _context.ClientPortalProfiles.FirstOrDefaultAsync(p => p.CustomerId == Guid.Parse(user.CustomerId));
                if (profile != null && profile.IsLocked)
                {
                    profile.IsLocked = false;
                    profile.LockoutEnd = null;
                    profile.FailedLoginAttempts = 0;
                }

                // Invalidate all existing sessions for the user
                var sessions = await _context.ClientSessions.Where(s => s.ClientPortalProfile.CustomerId == Guid.Parse(user.CustomerId)).ToListAsync();
                if (sessions.Any())
                {
                    _context.ClientSessions.RemoveRange(sessions);
                }

                await _context.SaveChangesAsync(CancellationToken.None);

                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    CustomerId = Guid.Parse(user.CustomerId),
                    Title = "Password Reset Successful",
                    Message = "Your password has been successfully reset. If you did not make this change, please contact customer support immediately.",
                    NotificationType = "security",
                    Action = "password_reset_success"
                });

                return BaseResponse<bool>.Success(true, "Password has been reset successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for email {Email}", email);
                return BaseResponse<bool>.Failure("An error occurred while resetting password.");
            }
        }
        public async Task<BaseResponse<bool>> VerifyTwoFactorCodeAsync(Guid customerId, string code)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.CustomerId == customerId.ToString());
            if (user == null)
            {
                return BaseResponse<bool>.Failure("User not found.");
            }

            var profile = await _context.ClientPortalProfiles.FirstOrDefaultAsync(p => p.CustomerId == customerId);
            if (profile == null || !profile.TwoFactorEnabled)
            {
                return BaseResponse<bool>.Failure("Two-factor authentication is not enabled for this account.");
            }

            var result = await _userManager.VerifyTwoFactorTokenAsync(user, profile.TwoFactorMethod, code);

            if (result)
            {
                return BaseResponse<bool>.Success(true, "Two-factor code verified successfully.");
            }

            return BaseResponse<bool>.Failure("Invalid two-factor code.");
        }

        public async Task<BaseResponse<bool>> SendTwoFactorCodeAsync(Guid customerId, string method)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.CustomerId == customerId.ToString());
            if (user == null)
            {
                return BaseResponse<bool>.Failure("User not found.");
            }

            var profile = await _context.ClientPortalProfiles.FirstOrDefaultAsync(p => p.CustomerId == customerId);
            if (profile == null || !profile.TwoFactorEnabled)
            {
                return BaseResponse<bool>.Failure("Two-factor authentication is not enabled for this account.");
            }

            // Ensure the requested method is valid for the user
            if (string.IsNullOrEmpty(method) || !profile.TwoFactorMethod.Equals(method, StringComparison.OrdinalIgnoreCase))
            {
                method = profile.TwoFactorMethod; // Default to the user's preferred method
            }

            var token = await _userManager.GenerateTwoFactorTokenAsync(user, method);

            var notificationDto = new CreateNotificationDto
            {
                CustomerId = customerId,
                Title = "Your Two-Factor Authentication Code",
                Message = $"Your authentication code is: {token}",
                NotificationType = "security",
                Action = "2fa_code"
            };

            if (method.Equals("Email", StringComparison.OrdinalIgnoreCase))
            {
                notificationDto.DeliveryChannels = new[] { NotificationChannel.Email };
            }
            else if (method.Equals("SMS", StringComparison.OrdinalIgnoreCase))
            {
                notificationDto.DeliveryChannels = new[] { NotificationChannel.SMS };
            }
            else
            {
                return BaseResponse<bool>.Failure("Unsupported two-factor authentication method.");
            }

            await _notificationService.CreateNotificationAsync(notificationDto);

            return BaseResponse<bool>.Success(true, $"A two-factor authentication code has been sent to your {method}.");
        }
    }
}