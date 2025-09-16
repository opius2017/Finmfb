using FinTech.WebAPI.Application.DTOs.Auth;
using FinTech.WebAPI.Application.DTOs.Common;
using FinTech.WebAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FinTech.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MfaController : ControllerBase
    {
        private readonly IMfaService _mfaService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<MfaController> _logger;
        
        public MfaController(
            IMfaService mfaService,
            UserManager<IdentityUser> userManager,
            ILogger<MfaController> logger)
        {
            _mfaService = mfaService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("setup")]
        public async Task<ActionResult<BaseResponse<MfaSetupResponseDto>>> Setup()
        {
            try
            {
                // Get current user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<MfaSetupResponseDto>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                // Generate MFA setup
                var response = await _mfaService.GenerateMfaSetupAsync(userId);
                
                return Ok(new BaseResponse<MfaSetupResponseDto>
                {
                    Success = true,
                    Message = "MFA setup generated successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating MFA setup");
                return StatusCode(500, new BaseResponse<MfaSetupResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while generating MFA setup"
                });
            }
        }

        [HttpPost("verify-setup")]
        public async Task<ActionResult<BaseResponse<bool>>> VerifySetup([FromBody] MfaVerifyRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request"
                    });
                }
                
                // Get current user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                // Verify the code
                var isValid = await _mfaService.VerifyMfaSetupAsync(userId, request.Code);
                
                if (isValid)
                {
                    // Enable MFA
                    await _mfaService.EnableMfaAsync(userId, request.Secret);
                    
                    return Ok(new BaseResponse<bool>
                    {
                        Success = true,
                        Message = "MFA setup verified and enabled successfully",
                        Data = true
                    });
                }
                
                return BadRequest(new BaseResponse<bool>
                {
                    Success = false,
                    Message = "Invalid verification code",
                    Data = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying MFA setup");
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while verifying MFA setup"
                });
            }
        }

        [HttpPost("disable")]
        public async Task<ActionResult<BaseResponse<bool>>> Disable([FromBody] MfaVerifyRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request"
                    });
                }
                
                // Get current user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                // Disable MFA
                var result = await _mfaService.DisableMfaAsync(userId, request.Code);
                
                if (result)
                {
                    return Ok(new BaseResponse<bool>
                    {
                        Success = true,
                        Message = "MFA disabled successfully",
                        Data = true
                    });
                }
                
                return BadRequest(new BaseResponse<bool>
                {
                    Success = false,
                    Message = "Invalid verification code",
                    Data = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling MFA");
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while disabling MFA"
                });
            }
        }

        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<ActionResult<BaseResponse<bool>>> Validate([FromBody] MfaValidateRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request"
                    });
                }
                
                // Create device info for login notification
                var deviceInfo = new MfaDeviceInfoDto
                {
                    Name = request.DeviceName ?? "Unknown Device",
                    Type = request.DeviceType ?? "Unknown",
                    Browser = request.Browser ?? HttpContext.Request.Headers["User-Agent"].ToString(),
                    OperatingSystem = request.OperatingSystem ?? "Unknown OS",
                    IpAddress = request.IpAddress ?? HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0"
                };
                
                // Validate code
                bool isValid = await _mfaService.ValidateMfaCodeAsync(request.UserId, request.Code, deviceInfo);
                
                if (isValid)
                {
                    return Ok(new BaseResponse<bool>
                    {
                        Success = true,
                        Message = "MFA code validated successfully",
                        Data = true
                    });
                }
                
                return BadRequest(new BaseResponse<bool>
                {
                    Success = false,
                    Message = "Invalid verification code",
                    Data = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating MFA code");
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while validating MFA code"
                });
            }
        }

        [HttpPost("validate-backup")]
        [AllowAnonymous]
        public async Task<ActionResult<BaseResponse<bool>>> ValidateBackup([FromBody] MfaValidateBackupRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request"
                    });
                }
                
                // Validate backup code
                bool isValid = await _mfaService.ValidateBackupCodeAsync(request.UserId, request.BackupCode);
                
                if (isValid)
                {
                    // Log device info for security monitoring
                    var deviceInfo = $"{request.Browser ?? HttpContext.Request.Headers["User-Agent"].ToString()} on {request.OperatingSystem ?? "Unknown OS"}";
                    var ipAddress = request.IpAddress ?? HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                    
                    await _mfaService.LogSecurityActivityAsync(new SecurityActivityDto
                    {
                        UserId = request.UserId,
                        EventType = "backup_code_used",
                        Timestamp = DateTime.UtcNow,
                        IpAddress = ipAddress,
                        DeviceInfo = deviceInfo,
                        Status = "success"
                    });
                    
                    return Ok(new BaseResponse<bool>
                    {
                        Success = true,
                        Message = "Backup code validated successfully",
                        Data = true
                    });
                }
                
                return BadRequest(new BaseResponse<bool>
                {
                    Success = false,
                    Message = "Invalid backup code",
                    Data = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating backup code");
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while validating backup code"
                });
            }
        }

        [HttpPost("regenerate-backup-codes")]
        public async Task<ActionResult<BaseResponse<List<string>>>> RegenerateBackupCodes([FromBody] MfaVerifyRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<List<string>>
                    {
                        Success = false,
                        Message = "Invalid request"
                    });
                }
                
                // Get current user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<List<string>>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                // Regenerate backup codes
                var backupCodes = await _mfaService.RegenerateBackupCodesAsync(userId, request.Code);
                
                return Ok(new BaseResponse<List<string>>
                {
                    Success = true,
                    Message = "Backup codes regenerated successfully",
                    Data = backupCodes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating backup codes");
                return StatusCode(500, new BaseResponse<List<string>>
                {
                    Success = false,
                    Message = "An error occurred while regenerating backup codes"
                });
            }
        }

        [HttpPost("create-challenge")]
        public async Task<ActionResult<BaseResponse<MfaChallengeResponseDto>>> CreateChallenge([FromBody] MfaChallengeRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<MfaChallengeResponseDto>
                    {
                        Success = false,
                        Message = "Invalid request"
                    });
                }
                
                // Get current user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<MfaChallengeResponseDto>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                // Create challenge
                var response = await _mfaService.CreateMfaChallengeAsync(userId, request.Operation);
                
                return Ok(new BaseResponse<MfaChallengeResponseDto>
                {
                    Success = true,
                    Message = "MFA challenge created successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MFA challenge");
                return StatusCode(500, new BaseResponse<MfaChallengeResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while creating MFA challenge"
                });
            }
        }

        [HttpPost("verify-challenge")]
        [AllowAnonymous]
        public async Task<ActionResult<BaseResponse<bool>>> VerifyChallenge([FromBody] MfaVerifyChallengeRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request"
                    });
                }
                
                // Verify challenge
                bool isValid = await _mfaService.VerifyMfaChallengeAsync(request.ChallengeId, request.Code);
                
                if (isValid)
                {
                    return Ok(new BaseResponse<bool>
                    {
                        Success = true,
                        Message = "Challenge verified successfully",
                        Data = true
                    });
                }
                
                return BadRequest(new BaseResponse<bool>
                {
                    Success = false,
                    Message = "Invalid verification code or expired challenge",
                    Data = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying MFA challenge");
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while verifying MFA challenge"
                });
            }
        }

        [HttpPost("add-trusted-device")]
        public async Task<ActionResult<BaseResponse<string>>> AddTrustedDevice([FromBody] MfaDeviceInfoDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<string>
                    {
                        Success = false,
                        Message = "Invalid request"
                    });
                }
                
                // Get current user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                // Add trusted device
                var deviceId = await _mfaService.AddTrustedDeviceAsync(userId, request);
                
                return Ok(new BaseResponse<string>
                {
                    Success = true,
                    Message = "Trusted device added successfully",
                    Data = deviceId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding trusted device");
                return StatusCode(500, new BaseResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while adding trusted device"
                });
            }
        }

        [HttpGet("trusted-devices")]
        public async Task<ActionResult<BaseResponse<List<TrustedDeviceDto>>>> GetTrustedDevices()
        {
            try
            {
                // Get current user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<List<TrustedDeviceDto>>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                // Get trusted devices
                var devices = await _mfaService.GetTrustedDevicesAsync(userId);
                
                // Determine current device (this should be improved in production)
                var currentIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                
                foreach (var device in devices)
                {
                    if (device.IpAddress == currentIp)
                    {
                        device.IsCurrent = true;
                    }
                }
                
                return Ok(new BaseResponse<List<TrustedDeviceDto>>
                {
                    Success = true,
                    Message = "Trusted devices retrieved successfully",
                    Data = devices
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trusted devices");
                return StatusCode(500, new BaseResponse<List<TrustedDeviceDto>>
                {
                    Success = false,
                    Message = "An error occurred while getting trusted devices"
                });
            }
        }

        [HttpDelete("trusted-device/{deviceId}")]
        public async Task<ActionResult<BaseResponse<bool>>> RevokeTrustedDevice(string deviceId)
        {
            try
            {
                // Get current user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                // Revoke trusted device
                var result = await _mfaService.RevokeTrustedDeviceAsync(userId, deviceId);
                
                if (result)
                {
                    return Ok(new BaseResponse<bool>
                    {
                        Success = true,
                        Message = "Trusted device revoked successfully",
                        Data = true
                    });
                }
                
                return BadRequest(new BaseResponse<bool>
                {
                    Success = false,
                    Message = "Failed to revoke trusted device",
                    Data = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking trusted device");
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while revoking trusted device"
                });
            }
        }

        [HttpDelete("trusted-devices/revoke-all-except-current")]
        public async Task<ActionResult<BaseResponse<bool>>> RevokeAllTrustedDevicesExceptCurrent([FromBody] RevokeDevicesRequestDto request)
        {
            try
            {
                // Get current user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                // Revoke all trusted devices except current
                var result = await _mfaService.RevokeAllTrustedDevicesExceptCurrentAsync(userId, request.CurrentDeviceId);
                
                if (result)
                {
                    return Ok(new BaseResponse<bool>
                    {
                        Success = true,
                        Message = "All trusted devices except current revoked successfully",
                        Data = true
                    });
                }
                
                return BadRequest(new BaseResponse<bool>
                {
                    Success = false,
                    Message = "Failed to revoke trusted devices",
                    Data = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all trusted devices");
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while revoking trusted devices"
                });
            }
        }

        [HttpGet("security-activity")]
        public async Task<ActionResult<BaseResponse<List<SecurityActivityDto>>>> GetSecurityActivity([FromQuery] int limit = 20)
        {
            try
            {
                // Get current user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<List<SecurityActivityDto>>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                // Get security activity
                var activities = await _mfaService.GetSecurityActivityAsync(userId, limit);
                
                return Ok(new BaseResponse<List<SecurityActivityDto>>
                {
                    Success = true,
                    Message = "Security activity retrieved successfully",
                    Data = activities
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting security activity");
                return StatusCode(500, new BaseResponse<List<SecurityActivityDto>>
                {
                    Success = false,
                    Message = "An error occurred while getting security activity"
                });
            }
        }

        [HttpGet("security-preferences")]
        public async Task<ActionResult<BaseResponse<SecurityPreferencesDto>>> GetSecurityPreferences()
        {
            try
            {
                // Get current user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<SecurityPreferencesDto>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                // Get security preferences
                var preferences = await _mfaService.GetSecurityPreferencesAsync(userId);
                
                return Ok(new BaseResponse<SecurityPreferencesDto>
                {
                    Success = true,
                    Message = "Security preferences retrieved successfully",
                    Data = preferences
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting security preferences");
                return StatusCode(500, new BaseResponse<SecurityPreferencesDto>
                {
                    Success = false,
                    Message = "An error occurred while getting security preferences"
                });
            }
        }

        [HttpPut("security-preferences")]
        public async Task<ActionResult<BaseResponse<bool>>> UpdateSecurityPreferences([FromBody] SecurityPreferencesDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid request"
                    });
                }
                
                // Get current user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                // Update security preferences
                var result = await _mfaService.UpdateSecurityPreferencesAsync(userId, request);
                
                if (result)
                {
                    return Ok(new BaseResponse<bool>
                    {
                        Success = true,
                        Message = "Security preferences updated successfully",
                        Data = true
                    });
                }
                
                return BadRequest(new BaseResponse<bool>
                {
                    Success = false,
                    Message = "Failed to update security preferences",
                    Data = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating security preferences");
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while updating security preferences"
                });
            }
        }
    }
}