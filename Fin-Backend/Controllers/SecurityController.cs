using FinTech.WebAPI.Application.DTOs.Auth;
using FinTech.WebAPI.Application.DTOs.Common;
using FinTech.WebAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FinTech.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SecurityController : ControllerBase
    {
        private readonly IAdvancedAuthService _authService;
        private readonly IMfaService _mfaService;
        private readonly ILogger<SecurityController> _logger;
        
        public SecurityController(
            IAdvancedAuthService authService,
            IMfaService mfaService,
            ILogger<SecurityController> logger)
        {
            _authService = authService;
            _mfaService = mfaService;
            _logger = logger;
        }

        /// <summary>
        /// Gets security dashboard information for the current user
        /// </summary>
        /// <returns>The security dashboard data</returns>
        [HttpGet("dashboard")]
        public async Task<ActionResult<BaseResponse<SecurityDashboardDto>>> GetSecurityDashboard()
        {
            try
            {
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<SecurityDashboardDto>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                var dashboard = await _authService.GetSecurityDashboardAsync(userId);
                
                return Ok(new BaseResponse<SecurityDashboardDto>
                {
                    Success = true,
                    Message = "Security dashboard retrieved successfully",
                    Data = dashboard
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security dashboard");
                return StatusCode(500, new BaseResponse<SecurityDashboardDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving security dashboard"
                });
            }
        }

        /// <summary>
        /// Gets the security audit history for the current user
        /// </summary>
        /// <param name="limit">Maximum number of items to return</param>
        /// <returns>The security audit history</returns>
        [HttpGet("audit-history")]
        public async Task<ActionResult<BaseResponse<List<SecurityActivityDto>>>> GetSecurityAuditHistory([FromQuery] int limit = 20)
        {
            try
            {
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<List<SecurityActivityDto>>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                var history = await _mfaService.GetSecurityActivityAsync(userId, limit);
                
                return Ok(new BaseResponse<List<SecurityActivityDto>>
                {
                    Success = true,
                    Message = "Security audit history retrieved successfully",
                    Data = history
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security audit history");
                return StatusCode(500, new BaseResponse<List<SecurityActivityDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving security audit history"
                });
            }
        }

        /// <summary>
        /// Gets active sessions for the current user
        /// </summary>
        /// <returns>List of active sessions</returns>
        [HttpGet("active-sessions")]
        public async Task<ActionResult<BaseResponse<List<ActiveSessionDto>>>> GetActiveSessions()
        {
            try
            {
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<List<ActiveSessionDto>>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                var sessions = await _authService.GetActiveSessionsAsync(userId);
                
                return Ok(new BaseResponse<List<ActiveSessionDto>>
                {
                    Success = true,
                    Message = "Active sessions retrieved successfully",
                    Data = sessions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active sessions");
                return StatusCode(500, new BaseResponse<List<ActiveSessionDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving active sessions"
                });
            }
        }

        /// <summary>
        /// Revokes all active sessions except the current one
        /// </summary>
        /// <returns>Success or failure</returns>
        [HttpPost("active-sessions/revoke-all-except-current")]
        public async Task<ActionResult<BaseResponse<bool>>> RevokeAllSessionsExceptCurrent()
        {
            try
            {
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                // Get current session ID
                var currentSessionId = User.FindFirstValue("sid");
                if (string.IsNullOrEmpty(currentSessionId))
                {
                    return BadRequest(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Current session ID not found"
                    });
                }
                
                var result = await _authService.RevokeAllSessionsExceptCurrentAsync(userId, currentSessionId);
                
                if (!result)
                {
                    return BadRequest(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Failed to revoke sessions",
                        Data = false
                    });
                }
                
                return Ok(new BaseResponse<bool>
                {
                    Success = true,
                    Message = "All sessions except current revoked successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all sessions except current");
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while revoking sessions"
                });
            }
        }

        /// <summary>
        /// Revokes a specific session
        /// </summary>
        /// <param name="sessionId">The session ID to revoke</param>
        /// <returns>Success or failure</returns>
        [HttpDelete("active-sessions/{sessionId}")]
        public async Task<ActionResult<BaseResponse<bool>>> RevokeSession(string sessionId)
        {
            try
            {
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                // Check if trying to revoke current session
                var currentSessionId = User.FindFirstValue("sid");
                if (string.IsNullOrEmpty(currentSessionId))
                {
                    return BadRequest(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Current session ID not found"
                    });
                }
                
                if (currentSessionId == sessionId)
                {
                    return BadRequest(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Cannot revoke current session",
                        Data = false
                    });
                }
                
                var result = await _authService.RevokeSessionAsync(userId, sessionId);
                
                if (!result)
                {
                    return BadRequest(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Failed to revoke session",
                        Data = false
                    });
                }
                
                return Ok(new BaseResponse<bool>
                {
                    Success = true,
                    Message = "Session revoked successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking session {SessionId}", sessionId);
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while revoking session"
                });
            }
        }

        /// <summary>
        /// Gets security preferences for the current user
        /// </summary>
        /// <returns>The security preferences</returns>
        [HttpGet("preferences")]
        public async Task<ActionResult<BaseResponse<SecurityPreferencesDto>>> GetSecurityPreferences()
        {
            try
            {
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<SecurityPreferencesDto>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
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
                _logger.LogError(ex, "Error retrieving security preferences");
                return StatusCode(500, new BaseResponse<SecurityPreferencesDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving security preferences"
                });
            }
        }

        /// <summary>
        /// Updates security preferences for the current user
        /// </summary>
        /// <param name="preferences">The updated security preferences</param>
        /// <returns>Success or failure</returns>
        [HttpPut("preferences")]
        public async Task<ActionResult<BaseResponse<bool>>> UpdateSecurityPreferences([FromBody] SecurityPreferencesDto preferences)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid security preferences"
                    });
                }
                
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                var result = await _mfaService.UpdateSecurityPreferencesAsync(userId, preferences);
                
                if (!result)
                {
                    return BadRequest(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Failed to update security preferences",
                        Data = false
                    });
                }
                
                return Ok(new BaseResponse<bool>
                {
                    Success = true,
                    Message = "Security preferences updated successfully",
                    Data = true
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

        /// <summary>
        /// Changes the password for the current user
        /// </summary>
        /// <param name="request">The password change request</param>
        /// <returns>Success or failure</returns>
        [HttpPost("change-password")]
        public async Task<ActionResult<BaseResponse<bool>>> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid password change request"
                    });
                }
                
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
                
                if (!result.Succeeded)
                {
                    return BadRequest(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = string.Join(", ", result.Errors),
                        Data = false
                    });
                }
                
                return Ok(new BaseResponse<bool>
                {
                    Success = true,
                    Message = "Password changed successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while changing password"
                });
            }
        }

        /// <summary>
        /// Requests a data export for the current user
        /// </summary>
        /// <returns>Success or failure</returns>
        [HttpPost("request-data-export")]
        public async Task<ActionResult<BaseResponse<bool>>> RequestDataExport()
        {
            try
            {
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                var result = await _authService.RequestDataExportAsync(userId);
                
                if (!result)
                {
                    return BadRequest(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Failed to request data export",
                        Data = false
                    });
                }
                
                return Ok(new BaseResponse<bool>
                {
                    Success = true,
                    Message = "Data export requested successfully. You will receive an email when it's ready.",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting data export");
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while requesting data export"
                });
            }
        }
    }

    /// <summary>
    /// Request to change password
    /// </summary>
    public class ChangePasswordRequestDto
    {
        /// <summary>
        /// Gets or sets the current password
        /// </summary>
        public string CurrentPassword { get; set; }
        
        /// <summary>
        /// Gets or sets the new password
        /// </summary>
        public string NewPassword { get; set; }
    }

    /// <summary>
    /// Security dashboard DTO
    /// </summary>
    public class SecurityDashboardDto
    {
        /// <summary>
        /// Gets or sets whether MFA is enabled
        /// </summary>
        public bool MfaEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets the last password change date
        /// </summary>
        public DateTime? LastPasswordChange { get; set; }
        
        /// <summary>
        /// Gets or sets the number of active sessions
        /// </summary>
        public int ActiveSessionCount { get; set; }
        
        /// <summary>
        /// Gets or sets the number of trusted devices
        /// </summary>
        public int TrustedDeviceCount { get; set; }
        
        /// <summary>
        /// Gets or sets the number of linked social accounts
        /// </summary>
        public int LinkedSocialAccountCount { get; set; }
        
        /// <summary>
        /// Gets or sets the number of recent suspicious activities
        /// </summary>
        public int RecentSuspiciousActivityCount { get; set; }
        
        /// <summary>
        /// Gets or sets the most recent login date
        /// </summary>
        public DateTime? MostRecentLogin { get; set; }
        
        /// <summary>
        /// Gets or sets the security score (0-100)
        /// </summary>
        public int SecurityScore { get; set; }
        
        /// <summary>
        /// Gets or sets security recommendations
        /// </summary>
        public List<SecurityRecommendationDto> Recommendations { get; set; }
    }

    /// <summary>
    /// Security recommendation DTO
    /// </summary>
    public class SecurityRecommendationDto
    {
        /// <summary>
        /// Gets or sets the recommendation type
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// Gets or sets the recommendation message
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Gets or sets the recommendation priority (High, Medium, Low)
        /// </summary>
        public string Priority { get; set; }
        
        /// <summary>
        /// Gets or sets the recommendation action URL
        /// </summary>
        public string ActionUrl { get; set; }
    }

    /// <summary>
    /// Active session DTO
    /// </summary>
    public class ActiveSessionDto
    {
        /// <summary>
        /// Gets or sets the session ID
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Gets or sets the device information
        /// </summary>
        public string Device { get; set; }
        
        /// <summary>
        /// Gets or sets the browser information
        /// </summary>
        public string Browser { get; set; }
        
        /// <summary>
        /// Gets or sets the operating system
        /// </summary>
        public string OperatingSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the IP address
        /// </summary>
        public string IpAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the login date
        /// </summary>
        public DateTime LoginDate { get; set; }
        
        /// <summary>
        /// Gets or sets the approximate location
        /// </summary>
        public string Location { get; set; }
        
        /// <summary>
        /// Gets or sets whether this is the current session
        /// </summary>
        public bool IsCurrent { get; set; }
    }
}