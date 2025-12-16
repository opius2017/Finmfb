using FinTech.Infrastructure.Services;
using FinTech.Application.Common.Settings; // Corrected namespace
using FinTech.Core.Application.Interfaces;
using FinTech.Core.Application.Interfaces.Repositories;
using FinTech.Core.Application.Interfaces.Services; // Added for IAdvancedAuthService
using FinTech.Core.Application.DTOs.Auth;
using FinTech.Infrastructure.Repositories;
using FinTech.Infrastructure.Services.Security; // Added for MfaServiceFactory

using FinTech.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FinTech.Infrastructure.Security
{
    /// <summary>
    /// Extension methods for registering advanced authentication services
    /// </summary>
    public static class AdvancedAuthServicesRegistration
    {
        /// <summary>
        /// Registers advanced authentication services
        /// </summary>
        public static IServiceCollection AddAdvancedAuthServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure JWT settings
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            
            // Configure social login settings
            services.Configure<SocialLoginSettings>(configuration.GetSection("SocialLoginSettings"));

            // Register JWT service (No Interface)
            services.AddScoped<JwtTokenService>();
            
            // Register repositories
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IMfaSettingsRepository, MfaSettingsRepository>();
            services.AddScoped<IMfaChallengeRepository, MfaChallengeRepository>(); // Added
            services.AddScoped<IBackupCodeRepository, BackupCodeRepository>();
            services.AddScoped<ITrustedDeviceRepository, TrustedDeviceRepository>(); // Fixed from UserDeviceRepository
            services.AddScoped<ILoginAttemptRepository, LoginAttemptRepository>(); // Added
            services.AddScoped<ISecurityActivityRepository, SecurityActivityRepository>();
            services.AddScoped<ISocialLoginProfileRepository, SocialLoginProfileRepository>();
            services.AddScoped<IUserSecurityPreferencesRepository, UserSecurityPreferencesRepository>();
            
            // Register MFA providers (Concrete classes used by Factory)
            services.AddScoped<AppBasedMfaService>();
            services.AddScoped<EmailMfaService>();
            services.AddScoped<SmsMfaService>();
            
            // Register MFA factories
            services.AddScoped<MfaServiceFactory>();
            services.AddScoped<IMfaProviderFactory, MfaProviderFactory>();
            
            // Register MFA service
            services.AddScoped<IMfaService, MfaService>();
            
            // Register MFA notification service
            services.AddScoped<IMfaNotificationService, MfaNotificationService>();
            
            // Register advanced authentication service
            services.AddScoped<IAdvancedAuthService, AdvancedAuthService>();
            
            // Configure identity options
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;
                
                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                
                // User settings
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
                
                // Sign-in settings
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            });
            
            // Configure JWT Bearer authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    ValidAudience = configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"])),
                    ClockSkew = TimeSpan.Zero // Don't add any tolerance for token expiration
                };
                
                // Add custom validation logic for JWTs
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var authService = context.HttpContext.RequestServices.GetRequiredService<IAdvancedAuthService>();
                        
                        // Get token from authorization header
                        var token = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                        
                        // Get device info
                        var deviceInfo = new DeviceInfo
                        {
                            ClientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                            Browser = context.HttpContext.Request.Headers["User-Agent"].ToString(),
                            DeviceId = context.HttpContext.Request.Headers["X-Device-Id"].ToString(),
                            DeviceName = context.HttpContext.Request.Headers["X-Device-Name"].ToString(),
                            DeviceType = context.HttpContext.Request.Headers["X-Device-Type"].ToString(),
                            OperatingSystem = context.HttpContext.Request.Headers["X-OS"].ToString(),
                            BrowserVersion = context.HttpContext.Request.Headers["X-Browser-Version"].ToString(),
                        };
                        
                        // Validate token with advanced security checks
                        var validationResult = await authService.ValidateTokenWithSecurityChecksAsync(token, deviceInfo);
                        
                        if (!validationResult.IsValid)
                        {
                            context.Fail(validationResult.FailureReason);
                        }
                    }
                };
            });
            
            

            
            return services;
        }
    }
}
