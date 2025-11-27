using FinTech.Application.Common.Settings;
using FinTech.Application.Interfaces;
using FinTech.Application.Interfaces.Repositories;
using FinTech.Domain.Entities.Authentication;
using FinTech.Infrastructure.Security.Authentication;
using FinTech.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using FinTech.Core.Application.Interfaces.Shared;

namespace FinTech.Infrastructure.Services
{
    /// <summary>
    /// Advanced authentication service implementation
    /// </summary>
    public class AdvancedAuthService : IAdvancedAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly MfaServiceFactory _mfaServiceFactory;
        private readonly IMfaNotificationService _notificationService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IMfaSettingsRepository _mfaSettingsRepository;
        private readonly IMfaChallengeRepository _mfaChallengeRepository;
        private readonly IBackupCodeRepository _backupCodeRepository;
        private readonly ITrustedDeviceRepository _trustedDeviceRepository;
        private readonly ILoginAttemptRepository _loginAttemptRepository;
        private readonly ISocialLoginProfileRepository _socialLoginRepository;
        private readonly ISecurityAlertRepository _securityAlertRepository;
        private readonly IUserSecurityPreferencesRepository _securityPreferencesRepository;
        private readonly JwtSettings _jwtSettings;
        private readonly SocialLoginSettings _socialLoginSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvancedAuthService"/> class
        /// </summary>
        /// <param name="userManager">The user manager</param>
        /// <param name="signInManager">The sign-in manager</param>
        /// <param name="jwtTokenService">The JWT token service</param>
        /// <param name="mfaServiceFactory">The MFA service factory</param>
        /// <param name="notificationService">The notification service</param>
        /// <param name="refreshTokenRepository">The refresh token repository</param>
        /// <param name="mfaSettingsRepository">The MFA settings repository</param>
        /// <param name="mfaChallengeRepository">The MFA challenge repository</param>
        /// <param name="backupCodeRepository">The backup code repository</param>
        /// <param name="trustedDeviceRepository">The trusted device repository</param>
        /// <param name="loginAttemptRepository">The login attempt repository</param>
        /// <param name="socialLoginRepository">The social login repository</param>
        /// <param name="securityAlertRepository">The security alert repository</param>
        /// <param name="securityPreferencesRepository">The security preferences repository</param>
        /// <param name="jwtSettings">The JWT settings</param>
        /// <param name="socialLoginSettings">The social login settings</param>
        public AdvancedAuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtTokenService jwtTokenService,
            MfaServiceFactory mfaServiceFactory,
            IMfaNotificationService notificationService,
            IRefreshTokenRepository refreshTokenRepository,
            IMfaSettingsRepository mfaSettingsRepository,
            IMfaChallengeRepository mfaChallengeRepository,
            IBackupCodeRepository backupCodeRepository,
            ITrustedDeviceRepository trustedDeviceRepository,
            ILoginAttemptRepository loginAttemptRepository,
            ISocialLoginProfileRepository socialLoginRepository,
            ISecurityAlertRepository securityAlertRepository,
            IUserSecurityPreferencesRepository securityPreferencesRepository,
            IOptions<JwtSettings> jwtSettings,
            IOptions<SocialLoginSettings> socialLoginSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _mfaServiceFactory = mfaServiceFactory;
            _notificationService = notificationService;
            _refreshTokenRepository = refreshTokenRepository;
            _mfaSettingsRepository = mfaSettingsRepository;
            _mfaChallengeRepository = mfaChallengeRepository;
            _backupCodeRepository = backupCodeRepository;
            _trustedDeviceRepository = trustedDeviceRepository;
            _loginAttemptRepository = loginAttemptRepository;
            _socialLoginRepository = socialLoginRepository;
            _securityAlertRepository = securityAlertRepository;
            _securityPreferencesRepository = securityPreferencesRepository;
            _jwtSettings = jwtSettings.Value;
            _socialLoginSettings = socialLoginSettings.Value;
        }

        /// <inheritdoc/>
        public async Task<string> AddTrustedDeviceAsync(string userId, DeviceInfo deviceInfo)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var trustedDevice = await _trustedDeviceRepository.AddTrustedDeviceAsync(
                userId,
                deviceInfo.DeviceId,
                deviceInfo.DeviceName,
                deviceInfo.DeviceType,
                deviceInfo.OperatingSystem,
                deviceInfo.Browser,
                deviceInfo.BrowserVersion,
                deviceInfo.ClientIp,
                deviceInfo.Location?.Country,
                deviceInfo.Location?.City,
                deviceInfo.Location?.Region);

            // Send notification about new trusted device
            await _notificationService.SendSecurityAlertAsync(
                user.Email,
                "New Trusted Device",
                "A new device has been added to your trusted devices list.",
                $"Device: {deviceInfo.DeviceName} ({deviceInfo.DeviceType}) - {deviceInfo.Browser} on {deviceInfo.OperatingSystem}");

            return trustedDevice.DeviceId;
        }

        /// <inheritdoc/>
        public async Task<AuthResult> AuthenticateAsync(AuthRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(request.Username);
                if (user == null)
                {
                    // Record failed login attempt
                    await _loginAttemptRepository.RecordLoginAttemptAsync(
                        request.Username,
                        null,
                        false,
                        "User not found",
                        request.DeviceInfo?.ClientIp,
                        request.DeviceInfo?.Browser,
                        "Password",
                        request.DeviceInfo?.Location?.Country,
                        request.DeviceInfo?.Location?.City);

                    return new AuthResult
                    {
                        Succeeded = false,
                        ErrorMessage = "Invalid username or password."
                    };
                }
            }

            // Check if account is locked for security reasons
            if (user.IsSecurityLocked)
            {
                if (user.SecurityLockoutEnd.HasValue && user.SecurityLockoutEnd.Value > DateTime.UtcNow)
                {
                    await _loginAttemptRepository.RecordLoginAttemptAsync(
                        request.Username,
                        user.Id,
                        false,
                        "Account locked for security reasons",
                        request.DeviceInfo?.ClientIp,
                        request.DeviceInfo?.Browser,
                        "Password",
                        request.DeviceInfo?.Location?.Country,
                        request.DeviceInfo?.Location?.City);

                    return new AuthResult
                    {
                        Succeeded = false,
                        ErrorMessage = $"Account locked for security reasons: {user.SecurityLockoutReason}. Please contact support."
                    };
                }
                else
                {
                    // Unlock the account if the lockout period has passed
                    user.IsSecurityLocked = false;
                    user.SecurityLockoutEnd = null;
                    user.SecurityLockoutReason = null;
                    await _userManager.UpdateAsync(user);
                }
            }

            // Check if user is locked out due to failed login attempts
            if (await _userManager.IsLockedOutAsync(user))
            {
                await _loginAttemptRepository.RecordLoginAttemptAsync(
                    request.Username,
                    user.Id,
                    false,
                    "Account locked out due to failed login attempts",
                    request.DeviceInfo?.ClientIp,
                    request.DeviceInfo?.Browser,
                    "Password",
                    request.DeviceInfo?.Location?.Country,
                    request.DeviceInfo?.Location?.City);

                return new AuthResult
                {
                    Succeeded = false,
                    ErrorMessage = "Account locked out due to too many failed login attempts. Please try again later."
                };
            }

            // Verify password
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);
            if (!result.Succeeded)
            {
                // Check if we need to notify about suspicious activity
                var failedLoginCount = await _loginAttemptRepository.GetFailedLoginAttemptsCountAsync(user.UserName, 30);
                if (failedLoginCount >= 3)
                {
                    // Send notification about suspicious login attempts
                    await _notificationService.SendSuspiciousActivityAlertAsync(
                        user.Email,
                        "Multiple failed login attempts",
                        request.DeviceInfo?.ClientIp,
                        $"{request.DeviceInfo?.Location?.City}, {request.DeviceInfo?.Location?.Country}",
                        $"{request.DeviceInfo?.DeviceType} - {request.DeviceInfo?.Browser} on {request.DeviceInfo?.OperatingSystem}",
                        DateTime.UtcNow);

                    await _securityAlertRepository.CreateAlertAsync(
                        user.Id,
                        "Failed Login Attempts",
                        "Multiple failed login attempts detected",
                        $"There have been {failedLoginCount} failed login attempts in the last 30 minutes.",
                        "High",
                        request.DeviceInfo?.ClientIp,
                        request.DeviceInfo?.DeviceId);
                }

                await _loginAttemptRepository.RecordLoginAttemptAsync(
                    request.Username,
                    user.Id,
                    false,
                    "Invalid password",
                    request.DeviceInfo?.ClientIp,
                    request.DeviceInfo?.Browser,
                    "Password",
                    request.DeviceInfo?.Location?.Country,
                    request.DeviceInfo?.Location?.City);

                return new AuthResult
                {
                    Succeeded = false,
                    ErrorMessage = "Invalid username or password."
                };
            }

            // Password is correct, check for MFA
            if (user.IsMfaEnabled)
            {
                // Check if the device is trusted
                bool isTrusted = false;
                if (!string.IsNullOrEmpty(request.DeviceInfo?.DeviceId))
                {
                    isTrusted = await _trustedDeviceRepository.IsDeviceTrustedAsync(user.Id, request.DeviceInfo.DeviceId);
                }

                // Only require MFA if the device is not trusted
                if (!isTrusted)
                {
                    // Check if we should require MFA for suspicious login
                    var securityPreferences = await _securityPreferencesRepository.GetByUserIdAsync(user.Id);
                    bool requireMfa = true;

                    if (securityPreferences.UseLocationBasedSecurity)
                    {
                        // Get recent successful logins from different locations
                        var recentLogins = await _loginAttemptRepository.GetByUserIdAsync(user.Id);
                        var successfulLogins = recentLogins
                            .Where(la => la.Success && !string.IsNullOrEmpty(la.Country))
                            .OrderByDescending(la => la.AttemptTime)
                            .Take(5)
                            .ToList();

                        // If the current location matches a recent successful login location, and the preferences
                        // don't require MFA for suspicious logins, we can skip MFA
                        if (successfulLogins.Any(la => la.Country == request.DeviceInfo?.Location?.Country) &&
                            !securityPreferences.RequireMfaForSuspiciousLogins)
                        {
                            requireMfa = false;
                        }
                    }

                    if (requireMfa)
                    {
                        // Initiate MFA challenge
                        var mfaChallenge = await InitiateMfaChallengeAsync(user.Id);
                        if (mfaChallenge.Succeeded)
                        {
                            await _loginAttemptRepository.RecordLoginAttemptAsync(
                                request.Username,
                                user.Id,
                                true,
                                null,
                                request.DeviceInfo?.ClientIp,
                                request.DeviceInfo?.Browser,
                                "Password",
                                request.DeviceInfo?.Location?.Country,
                                request.DeviceInfo?.Location?.City);

                            return new AuthResult
                            {
                                Succeeded = true,
                                RequiresMfa = true,
                                MfaChallengeId = mfaChallenge.ChallengeId,
                                UserId = user.Id,
                                Username = user.UserName,
                                Email = user.Email
                            };
                        }
                        else
                        {
                            return new AuthResult
                            {
                                Succeeded = false,
                                ErrorMessage = "Failed to initiate MFA challenge. Please try again."
                            };
                        }
                    }
                }
            }

            // No MFA required or device is trusted, generate tokens
            return await GenerateAuthResultForUserAsync(user, request.DeviceInfo, request.RememberDevice);
        }

        /// <inheritdoc/>
        public async Task<bool> DisableMfaAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsMfaEnabled)
            {
                return false;
            }

            var mfaSettings = await _mfaSettingsRepository.GetByUserIdAsync(userId);
            if (mfaSettings == null)
            {
                return false;
            }

            // Verify the code
            var mfaService = _mfaServiceFactory.GetMfaService(mfaSettings.Method);
            if (!mfaService.ValidateCode(mfaSettings.SharedKey, code))
            {
                // Try backup code
                if (!await _backupCodeRepository.ValidateCodeAsync(userId, code))
                {
                    return false;
                }

                // Mark the backup code as used
                await _backupCodeRepository.MarkAsUsedAsync(userId, code);
            }

            // Disable MFA
            await _mfaSettingsRepository.DisableMfaAsync(userId);
            
            // Update user entity
            user.IsMfaEnabled = false;
            await _userManager.UpdateAsync(user);

            // Send notification
            await _notificationService.SendSecurityAlertAsync(
                user.Email,
                "MFA Disabled",
                "Two-factor authentication has been disabled for your account.",
                "If you did not make this change, please contact support immediately as your account may be compromised.");

            return true;
        }

        /// <inheritdoc/>
        public async Task<List<string>> GenerateBackupCodesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsMfaEnabled)
            {
                return null;
            }

            // Generate new backup codes
            var backupCodes = await _backupCodeRepository.GenerateCodesAsync(userId);
            var codes = backupCodes.Select(bc => bc.Code).ToList();

            // Send notification
            await _notificationService.SendBackupCodesGeneratedEmailAsync(user.Email);

            return codes;
        }

        /// <inheritdoc/>
        public async Task<List<AuthHistoryItem>> GetAuthHistoryAsync(string userId, int limit = 10)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new List<AuthHistoryItem>();
            }

            var loginAttempts = await _loginAttemptRepository.GetByUserIdAsync(userId);
            
            return loginAttempts
                .OrderByDescending(la => la.AttemptTime)
                .Take(limit)
                .Select(la => new AuthHistoryItem
                {
                    Id = la.Id,
                    UserId = la.UserId,
                    LoginTime = la.AttemptTime,
                    IpAddress = la.IpAddress,
                    UserAgent = la.UserAgent,
                    Success = la.Success,
                    LoginMethod = la.LoginMethod,
                    Location = new LocationInfo
                    {
                        Country = la.Country,
                        City = la.City
                    },
                    Device = new DeviceInfo
                    {
                        Browser = la.UserAgent
                    }
                })
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<List<TrustedDeviceInfo>> GetTrustedDevicesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new List<TrustedDeviceInfo>();
            }

            var trustedDevices = await _trustedDeviceRepository.GetByUserIdAsync(userId);
            
            return trustedDevices
                .Select(td => new TrustedDeviceInfo
                {
                    DeviceId = td.DeviceId,
                    DeviceName = td.DeviceName,
                    DeviceType = td.DeviceType,
                    OperatingSystem = td.OperatingSystem,
                    Browser = td.Browser,
                    LastUsed = td.LastUsedAt,
                    CreatedAt = td.CreatedAt,
                    IpAddress = td.IpAddress,
                    Location = new LocationInfo
                    {
                        Country = td.Country,
                        City = td.City,
                        Region = td.Region
                    }
                })
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<MfaChallengeResult> InitiateMfaChallengeAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsMfaEnabled)
            {
                return new MfaChallengeResult
                {
                    Succeeded = false,
                    ErrorMessage = "User not found or MFA not enabled."
                };
            }

            var mfaSettings = await _mfaSettingsRepository.GetByUserIdAsync(userId);
            if (mfaSettings == null)
            {
                return new MfaChallengeResult
                {
                    Succeeded = false,
                    ErrorMessage = "MFA settings not found."
                };
            }

            // Create MFA challenge
            string verificationCode = null;
            string maskedDestination = null;

            // For app-based MFA, we don't need to send a code
            if (mfaSettings.Method.ToLower() == "app")
            {
                // App-based MFA doesn't require sending a code
                verificationCode = null;
            }
            else if (mfaSettings.Method.ToLower() == "email")
            {
                // Send verification code via email
                var emailMfaService = _mfaServiceFactory.GetMfaService("email") as EmailMfaService;
                verificationCode = await emailMfaService.SendMfaCodeAsync(mfaSettings.RecoveryEmail);
                
                // Mask the email address
                maskedDestination = MaskEmail(mfaSettings.RecoveryEmail);
            }
            else if (mfaSettings.Method.ToLower() == "sms")
            {
                // Send verification code via SMS
                var smsMfaService = _mfaServiceFactory.GetMfaService("sms") as SmsMfaService;
                verificationCode = await smsMfaService.SendMfaCodeAsync(mfaSettings.RecoveryPhone);
                
                // Mask the phone number
                maskedDestination = MaskPhoneNumber(mfaSettings.RecoveryPhone);
            }

            // Create a challenge in the database
            var challenge = await _mfaChallengeRepository.CreateChallengeAsync(
                userId,
                mfaSettings.Method,
                null, // IP address will be set when verifying
                null, // Device ID will be set when verifying
                15); // 15 minutes expiry

            return new MfaChallengeResult
            {
                Succeeded = true,
                ChallengeId = challenge.Id,
                Method = (MfaMethod)Enum.Parse(typeof(MfaMethod), mfaSettings.Method, true),
                MaskedDeliveryDestination = maskedDestination,
                VerificationCode = verificationCode
            };
        }

        /// <inheritdoc/>
        public async Task<MfaSetupResult> InitiateMfaSetupAsync(string userId, MfaMethod method)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new MfaSetupResult
                {
                    Succeeded = false,
                    ErrorMessage = "User not found."
                };
            }

            var mfaService = _mfaServiceFactory.GetMfaService(method.ToString());
            string secretKey = mfaService.GenerateSecretKey();
            string verificationCode = null;

            // Handle different MFA methods
            if (method == MfaMethod.App)
            {
                // For app-based MFA, generate QR code URL
                var qrCodeUrl = mfaService.GetMfaSetupQrCodeUri(secretKey, user.Email);
                
                return new MfaSetupResult
                {
                    Succeeded = true,
                    SharedKey = secretKey,
                    QrCodeUrl = qrCodeUrl
                };
            }
            else if (method == MfaMethod.Email)
            {
                // For email-based MFA, send verification code to user's email
                var emailMfaService = mfaService as EmailMfaService;
                verificationCode = await emailMfaService.SendMfaCodeAsync(user.Email);
                
                return new MfaSetupResult
                {
                    Succeeded = true,
                    SharedKey = secretKey,
                    VerificationCode = verificationCode
                };
            }
            else if (method == MfaMethod.Sms)
            {
                // For SMS-based MFA, send verification code to user's phone number
                if (string.IsNullOrEmpty(user.PhoneNumber))
                {
                    return new MfaSetupResult
                    {
                        Succeeded = false,
                        ErrorMessage = "Phone number not found."
                    };
                }
                
                var smsMfaService = mfaService as SmsMfaService;
                verificationCode = await smsMfaService.SendMfaCodeAsync(user.PhoneNumber);
                
                return new MfaSetupResult
                {
                    Succeeded = true,
                    SharedKey = secretKey,
                    VerificationCode = verificationCode
                };
            }

            return new MfaSetupResult
            {
                Succeeded = false,
                ErrorMessage = "Unsupported MFA method."
            };
        }

        /// <inheritdoc/>
        public async Task<bool> InitiatePasswordResetAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Return true to prevent user enumeration
                return true;
            }

            // Check if the user has security preferences that allow password reset
            var securityPreferences = await _securityPreferencesRepository.GetByUserIdAsync(user.Id);
            if (!securityPreferences.AllowPasswordReset)
            {
                // If password reset is disabled, log the attempt but don't allow it
                await _securityAlertRepository.CreateAlertAsync(
                    user.Id,
                    "Password Reset Attempt",
                    "Password reset attempted but is disabled in security preferences",
                    "Password reset has been disabled for this account in security preferences.",
                    "Medium",
                    null,
                    null);

                return false;
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // In a real implementation, we would send an email with the token
            // For now, just log it
            Console.WriteLine($"Password reset token for {email}: {token}");
            
            // Create a security alert for the password reset
            await _securityAlertRepository.CreateAlertAsync(
                user.Id,
                "Password Reset Requested",
                "A password reset has been requested for your account",
                "If you did not request this password reset, please contact support immediately.",
                "Medium",
                null,
                null);

            return true;
        }

        /// <inheritdoc/>
        public async Task<SocialLoginInitiationResult> InitiateSocialLoginAsync(string provider, string returnUrl)
        {
            if (string.IsNullOrEmpty(provider))
            {
                throw new ArgumentException("Provider cannot be null or empty", nameof(provider));
            }

            // Get provider settings
            var providerSettings = _socialLoginSettings.Providers.FirstOrDefault(p => 
                p.Name.Equals(provider, StringComparison.OrdinalIgnoreCase));
                
            if (providerSettings == null)
            {
                throw new ArgumentException($"Provider '{provider}' is not configured", nameof(provider));
            }

            // Generate state parameter for CSRF protection
            var state = Guid.NewGuid().ToString();
            
            // Construct authorization URL
            var authUrl = providerSettings.AuthorizationEndpoint;
            authUrl += $"?client_id={providerSettings.ClientId}";
            authUrl += $"&redirect_uri={HttpUtility.UrlEncode(providerSettings.RedirectUri)}";
            authUrl += "&response_type=code";
            authUrl += $"&scope={HttpUtility.UrlEncode(providerSettings.Scope)}";
            authUrl += $"&state={state}";
            
            return new SocialLoginInitiationResult
            {
                AuthorizationUrl = authUrl,
                State = state
            };
        }

        /// <inheritdoc/>
        public async Task<bool> IsTrustedDeviceAsync(string userId, string deviceId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(deviceId))
            {
                return false;
            }

            return await _trustedDeviceRepository.IsDeviceTrustedAsync(userId, deviceId);
        }

        /// <inheritdoc/>
        public async Task<AuthResult> ProcessSocialLoginCallbackAsync(string provider, string code, string state)
        {
            if (string.IsNullOrEmpty(provider) || string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                return new AuthResult
                {
                    Succeeded = false,
                    ErrorMessage = "Invalid provider, code, or state parameter."
                };
            }

            // Get provider settings
            var providerSettings = _socialLoginSettings.Providers.FirstOrDefault(p => 
                p.Name.Equals(provider, StringComparison.OrdinalIgnoreCase));
                
            if (providerSettings == null)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    ErrorMessage = $"Provider '{provider}' is not configured."
                };
            }

            // In a real implementation, we would exchange the code for an access token
            // and then use the access token to get user information from the provider
            // For this example, we'll simulate a successful exchange with a fake user profile
            
            // Simulated provider profile
            var providerUserInfo = new
            {
                Id = Guid.NewGuid().ToString(),
                Email = $"user_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
                Name = "John Doe",
                FirstName = "John",
                LastName = "Doe"
            };

            // Check if we have a user with this provider and ID
            var socialProfile = await _socialLoginRepository.GetByProviderAndKeyAsync(provider, providerUserInfo.Id);
            if (socialProfile != null)
            {
                // User exists, authenticate them
                var user = await _userManager.FindByIdAsync(socialProfile.UserId);
                if (user == null)
                {
                    return new AuthResult
                    {
                        Succeeded = false,
                        ErrorMessage = "User account not found."
                    };
                }

                // Update last used date
                await _socialLoginRepository.UpdateLastUsedAsync(user.Id, provider);
                
                // Record login attempt
                await _loginAttemptRepository.RecordLoginAttemptAsync(
                    user.UserName,
                    user.Id,
                    true,
                    null,
                    null,
                    null,
                    $"Social: {provider}",
                    null,
                    null);

                // Generate auth result
                return await GenerateAuthResultForUserAsync(user, null, false);
            }
            else
            {
                // User doesn't exist, check if we have a user with the same email
                var user = await _userManager.FindByEmailAsync(providerUserInfo.Email);
                if (user != null)
                {
                    // Link the social profile to the existing user
                    await _socialLoginRepository.AddSocialLoginAsync(
                        user.Id,
                        provider,
                        providerUserInfo.Id,
                        providerSettings.DisplayName);
                    
                    // Record login attempt
                    await _loginAttemptRepository.RecordLoginAttemptAsync(
                        user.UserName,
                        user.Id,
                        true,
                        null,
                        null,
                        null,
                        $"Social: {provider}",
                        null,
                        null);

                    // Generate auth result
                    return await GenerateAuthResultForUserAsync(user, null, false);
                }
                else
                {
                    // Create a new user
                    user = new ApplicationUser
                    {
                        UserName = providerUserInfo.Email,
                        Email = providerUserInfo.Email,
                        FirstName = providerUserInfo.FirstName,
                        LastName = providerUserInfo.LastName,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        return new AuthResult
                        {
                            Succeeded = false,
                            ErrorMessage = "Failed to create user account."
                        };
                    }

                    // Add to "User" role
                    await _userManager.AddToRoleAsync(user, "User");

                    // Link the social profile to the new user
                    await _socialLoginRepository.AddSocialLoginAsync(
                        user.Id,
                        provider,
                        providerUserInfo.Id,
                        providerSettings.DisplayName);
                    
                    // Record login attempt
                    await _loginAttemptRepository.RecordLoginAttemptAsync(
                        user.UserName,
                        user.Id,
                        true,
                        null,
                        null,
                        null,
                        $"Social: {provider}",
                        null,
                        null);

                    // Generate auth result
                    return await GenerateAuthResultForUserAsync(user, null, false);
                }
            }
        }

        /// <inheritdoc/>
        public async Task<AuthResult> RefreshTokenAsync(RefreshTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return new AuthResult
                {
                    Succeeded = false,
                    ErrorMessage = "Refresh token is required."
                };
            }

            // Get the refresh token from the database
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
            if (refreshToken == null)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    ErrorMessage = "Invalid refresh token."
                };
            }

            // Check if token is expired, used, or revoked
            if (refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    ErrorMessage = "Refresh token has expired."
                };
            }

            if (refreshToken.IsUsed)
            {
                // This could be a token reuse attempt - revoke all tokens for this user
                await _refreshTokenRepository.RevokeAllForUserAsync(refreshToken.UserId);
                
                // Log security alert
                await _securityAlertRepository.CreateAlertAsync(
                    refreshToken.UserId,
                    "Token Reuse Attempt",
                    "An attempt was made to reuse a refresh token",
                    "This could indicate a potential token theft. All refresh tokens have been revoked as a security measure.",
                    "High",
                    request.DeviceInfo?.ClientIp,
                    request.DeviceInfo?.DeviceId);

                return new AuthResult
                {
                    Succeeded = false,
                    ErrorMessage = "Refresh token has been used. All tokens have been revoked for security."
                };
            }

            if (refreshToken.IsRevoked)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    ErrorMessage = "Refresh token has been revoked."
                };
            }

            // Mark the token as used
            refreshToken.IsUsed = true;
            await _refreshTokenRepository.UpdateAsync(refreshToken);

            // Get the user
            var user = await _userManager.FindByIdAsync(refreshToken.UserId);
            if (user == null)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    ErrorMessage = "User not found."
                };
            }

            // Check if the device info matches
            if (request.DeviceInfo != null && !string.IsNullOrEmpty(request.DeviceInfo.DeviceId) && 
                !string.IsNullOrEmpty(refreshToken.DeviceId) && 
                refreshToken.DeviceId != request.DeviceInfo.DeviceId)
            {
                // Potential token theft - device ID mismatch
                await _refreshTokenRepository.RevokeAllForUserAsync(refreshToken.UserId);
                
                // Log security alert
                await _securityAlertRepository.CreateAlertAsync(
                    refreshToken.UserId,
                    "Device Mismatch",
                    "Refresh token used from a different device",
                    "This could indicate a potential token theft. All refresh tokens have been revoked as a security measure.",
                    "High",
                    request.DeviceInfo?.ClientIp,
                    request.DeviceInfo?.DeviceId);

                // Notify the user
                await _notificationService.SendSuspiciousActivityAlertAsync(
                    user.Email,
                    "Refresh token used from different device",
                    request.DeviceInfo?.ClientIp,
                    $"{request.DeviceInfo?.Location?.City}, {request.DeviceInfo?.Location?.Country}",
                    $"{request.DeviceInfo?.DeviceType} - {request.DeviceInfo?.Browser} on {request.DeviceInfo?.OperatingSystem}",
                    DateTime.UtcNow);

                return new AuthResult
                {
                    Succeeded = false,
                    ErrorMessage = "Refresh token device mismatch. All tokens have been revoked for security."
                };
            }

            // Generate new tokens
            return await GenerateAuthResultForUserAsync(user, request.DeviceInfo);
        }

        /// <inheritdoc/>
        public async Task<RegistrationResult> RegisterAsync(RegistrationRequest request)
        {
            // Check if user with email already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new RegistrationResult
                {
                    Succeeded = false,
                    Errors = new List<string> { "Email is already registered." }
                };
            }

            // Check if username is already taken
            existingUser = await _userManager.FindByNameAsync(request.Username);
            if (existingUser != null)
            {
                return new RegistrationResult
                {
                    Succeeded = false,
                    Errors = new List<string> { "Username is already taken." }
                };
            }

            // Create new user
            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                IsMfaEnabled = false
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return new RegistrationResult
                {
                    Succeeded = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            // Add to "User" role
            await _userManager.AddToRoleAsync(user, "User");

            // Create default security preferences
            await _securityPreferencesRepository.GetByUserIdAsync(user.Id);

            // Check if email confirmation is required
            var requiresEmailConfirmation = _userManager.Options.SignIn.RequireConfirmedEmail;
            string emailConfirmationToken = null;
            
            if (requiresEmailConfirmation)
            {
                emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                
                // In a real implementation, we would send an email with the confirmation link
                // For now, just log it
                Console.WriteLine($"Email confirmation token for {user.Email}: {emailConfirmationToken}");
            }

            return new RegistrationResult
            {
                Succeeded = true,
                UserId = user.Id,
                RequiresEmailConfirmation = requiresEmailConfirmation,
                EmailConfirmationToken = emailConfirmationToken
            };
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveTrustedDeviceAsync(string userId, string deviceId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(deviceId))
            {
                return false;
            }

            await _trustedDeviceRepository.RemoveTrustedDeviceAsync(userId, deviceId);
            
            // Send notification about removed trusted device
            await _notificationService.SendSecurityAlertAsync(
                user.Email,
                "Trusted Device Removed",
                "A device has been removed from your trusted devices list.",
                "If you did not make this change, please contact support immediately as your account may be compromised.");

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> RevokeTokenAsync(RevokeTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return false;
            }

            // Get the refresh token from the database
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
            if (refreshToken == null)
            {
                return false;
            }

            // Check if the user ID matches (if provided)
            if (!string.IsNullOrEmpty(request.UserId) && refreshToken.UserId != request.UserId)
            {
                return false;
            }

            // Mark the token as revoked
            refreshToken.IsRevoked = true;
            await _refreshTokenRepository.UpdateAsync(refreshToken);

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateBackupCodeAsync(string userId, string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            {
                return false;
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsMfaEnabled)
            {
                return false;
            }

            // Validate and mark the backup code as used in a single operation
            return await _backupCodeRepository.MarkAsUsedAsync(userId, code);
        }

        /// <inheritdoc/>
        public async Task<bool> VerifyMfaSetupAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            // For MFA setup verification, we need to know which MFA method is being set up
            // This would typically be stored in a temporary state during setup, but for this example,
            // we'll retrieve it from a request to the InitiateMfaSetupAsync method
            
            // For simplicity, we'll assume app-based MFA for this example
            var mfaService = _mfaServiceFactory.GetMfaService("app");
            
            // For app-based MFA, we would validate the code against the shared key
            // For email/SMS MFA, we would validate against the sent verification code
            if (mfaService.ValidateCode("SHARED_KEY_FROM_SETUP", code))
            {
                // Enable MFA for the user
                await _mfaSettingsRepository.EnableMfaAsync(userId, "app", "SHARED_KEY_FROM_SETUP");
                
                // Update user entity
                user.IsMfaEnabled = true;
                await _userManager.UpdateAsync(user);

                // Generate backup codes
                await _backupCodeRepository.GenerateCodesAsync(userId);

                // Send notification
                await _notificationService.SendMfaSetupSuccessEmailAsync(user.Email, "app");

                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<AuthResult> VerifyMfaChallengeAsync(string userId, string code, string challengeId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsMfaEnabled)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    ErrorMessage = "User not found or MFA not enabled."
                };
            }

            // Get the MFA challenge
            var challenge = await _mfaChallengeRepository.GetByIdAsync(challengeId);
            if (challenge == null || challenge.UserId != userId || challenge.IsUsed || challenge.ExpiresAt < DateTime.UtcNow)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    ErrorMessage = "Invalid or expired MFA challenge."
                };
            }

            // Get MFA settings
            var mfaSettings = await _mfaSettingsRepository.GetByUserIdAsync(userId);
            if (mfaSettings == null)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    ErrorMessage = "MFA settings not found."
                };
            }

            // Validate the code
            bool isCodeValid = false;
            
            // Try primary MFA method
            var mfaService = _mfaServiceFactory.GetMfaService(mfaSettings.Method);
            isCodeValid = mfaService.ValidateCode(mfaSettings.SharedKey, code);
            
            // If primary method fails, try backup code
            if (!isCodeValid)
            {
                isCodeValid = await _backupCodeRepository.ValidateCodeAsync(userId, code);
                if (isCodeValid)
                {
                    await _backupCodeRepository.MarkAsUsedAsync(userId, code);
                }
            }

            if (!isCodeValid)
            {
                // Log failed MFA attempt
                await _loginAttemptRepository.RecordLoginAttemptAsync(
                    user.UserName,
                    userId,
                    false,
                    "Invalid MFA code",
                    challenge.IpAddress,
                    null,
                    $"MFA: {mfaSettings.Method}",
                    null,
                    null);

                return new AuthResult
                {
                    Succeeded = false,
                    ErrorMessage = "Invalid MFA code."
                };
            }

            // Mark the challenge as used
            challenge.IsUsed = true;
            challenge.UsedAt = DateTime.UtcNow;
            await _mfaChallengeRepository.UpdateAsync(challenge);

            // Record successful MFA login
            await _loginAttemptRepository.RecordLoginAttemptAsync(
                user.UserName,
                userId,
                true,
                null,
                challenge.IpAddress,
                null,
                $"MFA: {mfaSettings.Method}",
                null,
                null);

            // Create device info from the challenge
            var deviceInfo = new DeviceInfo
            {
                DeviceId = challenge.DeviceId,
                ClientIp = challenge.IpAddress
            };

            // Generate auth result
            return await GenerateAuthResultForUserAsync(user, deviceInfo);
        }

        /// <inheritdoc/>
        public async Task<bool> VerifyPasswordResetAsync(PasswordResetVerificationRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || 
                string.IsNullOrEmpty(request.Token) || 
                string.IsNullOrEmpty(request.NewPassword))
            {
                return false;
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return false;
            }

            // Check if the user has security preferences that allow password reset
            var securityPreferences = await _securityPreferencesRepository.GetByUserIdAsync(user.Id);
            if (!securityPreferences.AllowPasswordReset)
            {
                // If password reset is disabled, log the attempt but don't allow it
                await _securityAlertRepository.CreateAlertAsync(
                    user.Id,
                    "Password Reset Attempt",
                    "Password reset verification attempted but is disabled in security preferences",
                    "Password reset has been disabled for this account in security preferences.",
                    "Medium",
                    null,
                    null);

                return false;
            }

            // Reset the password
            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                return false;
            }

            // Log the password change
            await _securityAlertRepository.CreateAlertAsync(
                user.Id,
                "Password Changed",
                "Your password has been changed successfully",
                "Your password was reset using the password reset feature.",
                "Medium",
                null,
                null);

            return true;
        }

        /// <summary>
        /// Generates an authentication result for a user
        /// </summary>
        /// <param name="user">The user</param>
        /// <param name="deviceInfo">The device information</param>
        /// <param name="rememberDevice">Whether to remember the device</param>
        /// <returns>The authentication result</returns>
        private async Task<AuthResult> GenerateAuthResultForUserAsync(ApplicationUser user, DeviceInfo deviceInfo, bool rememberDevice = false)
        {
            // Generate JWT token
            var jwtToken = await _jwtTokenService.GenerateJwtTokenAsync(user);
            var jwtTokenString = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            // Generate refresh token
            var refreshToken = _jwtTokenService.GenerateRefreshToken(
                user.Id,
                jwtToken.Id,
                deviceInfo?.ClientIp,
                deviceInfo?.DeviceId,
                _jwtSettings.RefreshTokenExpirationInDays);

            // Save refresh token to database
            await _refreshTokenRepository.AddAsync(refreshToken);

            // Update user's last login information
            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = deviceInfo?.ClientIp;
            await _userManager.UpdateAsync(user);

            // Get user's roles
            var roles = await _userManager.GetRolesAsync(user);

            // Add device to trusted devices if requested
            string trustedDeviceId = null;
            if (rememberDevice && deviceInfo != null && !string.IsNullOrEmpty(deviceInfo.DeviceId))
            {
                var trustedDevice = await _trustedDeviceRepository.AddTrustedDeviceAsync(
                    user.Id,
                    deviceInfo.DeviceId,
                    deviceInfo.DeviceName,
                    deviceInfo.DeviceType,
                    deviceInfo.OperatingSystem,
                    deviceInfo.Browser,
                    deviceInfo.BrowserVersion,
                    deviceInfo.ClientIp,
                    deviceInfo.Location?.Country,
                    deviceInfo.Location?.City,
                    deviceInfo.Location?.Region);

                trustedDeviceId = trustedDevice.DeviceId;
            }
            else if (deviceInfo != null && !string.IsNullOrEmpty(deviceInfo.DeviceId))
            {
                // Update last used date for existing trusted device
                var isTrusted = await _trustedDeviceRepository.IsDeviceTrustedAsync(user.Id, deviceInfo.DeviceId);
                if (isTrusted)
                {
                    await _trustedDeviceRepository.UpdateLastUsedAsync(user.Id, deviceInfo.DeviceId);
                    trustedDeviceId = deviceInfo.DeviceId;
                }
            }

            // Check if we need to send login notification
            var securityPreferences = await _securityPreferencesRepository.GetByUserIdAsync(user.Id);
            if (securityPreferences.NotifyOnNewLogin)
            {
                await _notificationService.SendNewLoginNotificationAsync(
                    user.Email,
                    deviceInfo?.ClientIp,
                    $"{deviceInfo?.Location?.City}, {deviceInfo?.Location?.Country}",
                    $"{deviceInfo?.DeviceType} - {deviceInfo?.Browser} on {deviceInfo?.OperatingSystem}",
                    DateTime.UtcNow);
            }

            return new AuthResult
            {
                Succeeded = true,
                AccessToken = jwtTokenString,
                RefreshToken = refreshToken.Token,
                Expiration = jwtToken.ValidTo,
                UserId = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Roles = roles.ToList(),
                RequiresMfa = false, // MFA is already verified at this point
                TrustedDeviceId = trustedDeviceId
            };
        }

        /// <summary>
        /// Masks an email address for privacy
        /// </summary>
        /// <param name="email">The email address</param>
        /// <returns>The masked email address</returns>
        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            var parts = email.Split('@');
            if (parts.Length != 2)
            {
                return email;
            }

            var name = parts[0];
            var domain = parts[1];

            if (name.Length <= 3)
            {
                return $"{name.Substring(0, 1)}***@{domain}";
            }

            return $"{name.Substring(0, 2)}***@{domain}";
        }

        /// <summary>
        /// Masks a phone number for privacy
        /// </summary>
        /// <param name="phoneNumber">The phone number</param>
        /// <returns>The masked phone number</returns>
        private string MaskPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return null;
            }

            if (phoneNumber.Length <= 4)
            {
                return phoneNumber;
            }

            return $"***{phoneNumber.Substring(phoneNumber.Length - 4)}";
        }
    }
}
