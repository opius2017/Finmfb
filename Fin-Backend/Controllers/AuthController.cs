using FinTech.WebAPI.Application.DTOs.Auth;
using FinTech.WebAPI.Application.DTOs.Common;
using FinTech.WebAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FinTech.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IMfaService _mfaService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        
        public AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IMfaService mfaService,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mfaService = mfaService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<BaseResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Validate request
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Invalid login request"
                    });
                }
                
                // Find user by email
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return Unauthorized(new BaseResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    });
                }
                
                // Check password
                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!result.Succeeded)
                {
                    // Log failed login attempt for security
                    await _mfaService.LogSecurityActivityAsync(new SecurityActivityDto
                    {
                        UserId = user.Id,
                        EventType = "login_failed",
                        Timestamp = DateTime.UtcNow,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0",
                        DeviceInfo = HttpContext.Request.Headers["User-Agent"].ToString(),
                        Status = "failure"
                    });
                    
                    return Unauthorized(new BaseResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    });
                }
                
                // Check if MFA is enabled for the user
                var mfaSettings = await CheckMfaStatus(user.Id);
                
                if (mfaSettings.IsEnabled)
                {
                    // Generate MFA challenge
                    var challenge = await _mfaService.CreateMfaChallengeAsync(user.Id, "login");
                    
                    // Return response with MFA required
                    return Ok(new BaseResponse<LoginResponse>
                    {
                        Success = true,
                        Message = "MFA verification required",
                        Data = new LoginResponse
                        {
                            RequiresMfa = true,
                            MfaType = "totp", // Currently only supporting TOTP
                            UserId = user.Id,
                            Email = user.Email,
                            Username = user.UserName,
                            MfaChallengeId = challenge.ChallengeId,
                            MfaChallenge = challenge
                        }
                    });
                }
                
                // User is authenticated, generate JWT token
                var token = await GenerateJwtToken(user);
                
                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);
                
                // Log successful login
                await LogSuccessfulLogin(user.Id);
                
                // Return response with token
                return Ok(new BaseResponse<LoginResponse>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = new LoginResponse
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        Username = user.UserName,
                        Token = token,
                        RequiresMfa = false,
                        Roles = roles.ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new BaseResponse<LoginResponse>
                {
                    Success = false,
                    Message = "An error occurred during login"
                });
            }
        }

        [HttpPost("verify-mfa")]
        public async Task<ActionResult<BaseResponse<LoginResponse>>> VerifyMfa([FromBody] MfaLoginRequestDto request)
        {
            try
            {
                // Validate request
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Invalid MFA verification request"
                    });
                }
                
                // Find user
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return Unauthorized(new BaseResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Invalid user"
                    });
                }
                
                // Verify MFA challenge
                bool isValid = await _mfaService.VerifyMfaChallengeAsync(request.ChallengeId, request.Code);
                
                if (!isValid)
                {
                    // Log failed MFA attempt
                    await _mfaService.LogSecurityActivityAsync(new SecurityActivityDto
                    {
                        UserId = user.Id,
                        EventType = "mfa_verification_failed",
                        Timestamp = DateTime.UtcNow,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0",
                        DeviceInfo = HttpContext.Request.Headers["User-Agent"].ToString(),
                        Status = "failure"
                    });
                    
                    return Unauthorized(new BaseResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Invalid verification code or expired challenge"
                    });
                }
                
                // User is authenticated, generate JWT token
                var token = await GenerateJwtToken(user);
                
                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);
                
                // If requested, add current device as trusted
                if (request.RememberDevice)
                {
                    await AddTrustedDevice(user.Id);
                }
                
                // Log successful login
                await LogSuccessfulLogin(user.Id);
                
                // Return response with token
                return Ok(new BaseResponse<LoginResponse>
                {
                    Success = true,
                    Message = "MFA verification successful",
                    Data = new LoginResponse
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        Username = user.UserName,
                        Token = token,
                        RequiresMfa = false,
                        Roles = roles.ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during MFA verification");
                return StatusCode(500, new BaseResponse<LoginResponse>
                {
                    Success = false,
                    Message = "An error occurred during MFA verification"
                });
            }
        }

        [HttpPost("verify-backup-code")]
        public async Task<ActionResult<BaseResponse<LoginResponse>>> VerifyBackupCode([FromBody] MfaBackupLoginRequestDto request)
        {
            try
            {
                // Validate request
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Invalid backup code verification request"
                    });
                }
                
                // Find user
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return Unauthorized(new BaseResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Invalid user"
                    });
                }
                
                // Verify backup code
                bool isValid = await _mfaService.ValidateBackupCodeAsync(user.Id, request.BackupCode);
                
                if (!isValid)
                {
                    // Log failed backup code attempt
                    await _mfaService.LogSecurityActivityAsync(new SecurityActivityDto
                    {
                        UserId = user.Id,
                        EventType = "backup_code_verification_failed",
                        Timestamp = DateTime.UtcNow,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0",
                        DeviceInfo = HttpContext.Request.Headers["User-Agent"].ToString(),
                        Status = "failure"
                    });
                    
                    return Unauthorized(new BaseResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Invalid backup code"
                    });
                }
                
                // User is authenticated, generate JWT token
                var token = await GenerateJwtToken(user);
                
                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);
                
                // If requested, add current device as trusted
                if (request.RememberDevice)
                {
                    await AddTrustedDevice(user.Id);
                }
                
                // Log successful login
                await LogSuccessfulLogin(user.Id);
                
                // Return response with token
                return Ok(new BaseResponse<LoginResponse>
                {
                    Success = true,
                    Message = "Backup code verification successful",
                    Data = new LoginResponse
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        Username = user.UserName,
                        Token = token,
                        RequiresMfa = false,
                        Roles = roles.ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during backup code verification");
                return StatusCode(500, new BaseResponse<LoginResponse>
                {
                    Success = false,
                    Message = "An error occurred during backup code verification"
                });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<BaseResponse<RegisterResponse>>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Validate request
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<RegisterResponse>
                    {
                        Success = false,
                        Message = "Invalid registration request"
                    });
                }
                
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new BaseResponse<RegisterResponse>
                    {
                        Success = false,
                        Message = "User with this email already exists"
                    });
                }
                
                // Create new user
                var user = new IdentityUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    EmailConfirmed = true // For simplicity, consider adding email confirmation in production
                };
                
                var result = await _userManager.CreateAsync(user, request.Password);
                
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return BadRequest(new BaseResponse<RegisterResponse>
                    {
                        Success = false,
                        Message = "Registration failed",
                        Errors = errors
                    });
                }
                
                // Assign default role
                await _userManager.AddToRoleAsync(user, "User");
                
                // Log registration
                await _mfaService.LogSecurityActivityAsync(new SecurityActivityDto
                {
                    UserId = user.Id,
                    EventType = "registration",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0",
                    DeviceInfo = HttpContext.Request.Headers["User-Agent"].ToString(),
                    Status = "success"
                });
                
                // Return success response
                return Ok(new BaseResponse<RegisterResponse>
                {
                    Success = true,
                    Message = "Registration successful",
                    Data = new RegisterResponse
                    {
                        UserId = user.Id,
                        Email = user.Email
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new BaseResponse<RegisterResponse>
                {
                    Success = false,
                    Message = "An error occurred during registration"
                });
            }
        }

        [HttpGet("mfa-status")]
        [Authorize]
        public async Task<ActionResult<BaseResponse<MfaStatusResponseDto>>> GetMfaStatus()
        {
            try
            {
                // Get current user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<MfaStatusResponseDto>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                // Get MFA status
                var status = await CheckMfaStatus(userId);
                
                return Ok(new BaseResponse<MfaStatusResponseDto>
                {
                    Success = true,
                    Message = "MFA status retrieved successfully",
                    Data = new MfaStatusResponseDto
                    {
                        IsEnabled = status.IsEnabled,
                        LastVerified = status.LastVerified
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting MFA status");
                return StatusCode(500, new BaseResponse<MfaStatusResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while getting MFA status"
                });
            }
        }

        #region Helper Methods

        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            
            // Add roles as claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3), // Token valid for 3 hours
                signingCredentials: creds
            );
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<(bool IsEnabled, DateTime? LastVerified)> CheckMfaStatus(string userId)
        {
            try
            {
                var context = HttpContext.RequestServices.GetService(typeof(FinTech.Infrastructure.Data.ApplicationDbContext)) as FinTech.Infrastructure.Data.ApplicationDbContext;
                
                var settings = await context.UserMfaSettings
                    .FirstOrDefaultAsync(m => m.UserId == userId);
                
                if (settings == null || !settings.IsEnabled)
                {
                    return (false, null);
                }
                
                return (true, settings.LastVerifiedAt);
            }
            catch (Exception)
            {
                return (false, null);
            }
        }

        private async Task AddTrustedDevice(string userId)
        {
            try
            {
                // Get device info
                var deviceInfo = new MfaDeviceInfoDto
                {
                    Name = "Unknown Device",
                    Type = "Browser",
                    Browser = HttpContext.Request.Headers["User-Agent"].ToString(),
                    OperatingSystem = GetOperatingSystem(HttpContext.Request.Headers["User-Agent"].ToString()),
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0"
                };
                
                // Add as trusted device
                await _mfaService.AddTrustedDeviceAsync(userId, deviceInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding trusted device for user {UserId}", userId);
                // Don't rethrow - this should not disrupt the login flow
            }
        }

        private string GetOperatingSystem(string userAgent)
        {
            // Simple OS detection from user agent - can be improved
            if (userAgent.Contains("Windows"))
                return "Windows";
            if (userAgent.Contains("Mac"))
                return "MacOS";
            if (userAgent.Contains("Linux"))
                return "Linux";
            if (userAgent.Contains("Android"))
                return "Android";
            if (userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
                return "iOS";
            
            return "Unknown";
        }

        private async Task LogSuccessfulLogin(string userId)
        {
            try
            {
                await _mfaService.LogSecurityActivityAsync(new SecurityActivityDto
                {
                    UserId = userId,
                    EventType = "login_success",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0",
                    DeviceInfo = HttpContext.Request.Headers["User-Agent"].ToString(),
                    Status = "success"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging successful login for user {UserId}", userId);
                // Don't rethrow - logging should not disrupt the login flow
            }
        }

        #endregion
    }
}