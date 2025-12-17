using FinTech.Core.Application.Interfaces;
using FinTech.Core.Application.Common;
using FinTech.Core.Application.DTOs.Security;
using FinTech.Core.Application.DTOs.Auth;
using FinTech.Core.Application.Common.DTOs;
using FinTech.Core.Application.Interfaces.Services;
using SecurityDashboardDto = FinTech.Core.Application.DTOs.Auth.SecurityDashboardDto;
using ClientSessionDto = FinTech.Core.Application.DTOs.Auth.ClientSessionDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using SecurityActivityDto = FinTech.Core.Application.DTOs.Auth.SecurityActivityDto;
using SecurityPreferencesDto = FinTech.Core.Application.DTOs.Auth.SecurityPreferencesDto;
using FinTech.Core.Application.Common.Models;

namespace FinTech.Controllers.Auth
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
                    return Unauthorized(BaseResponse<SecurityDashboardDto>.Failure("User not authenticated"));
                }
                
                var dashboard = await _authService.GetSecurityDashboardAsync(userId);
                
                return Ok(BaseResponse<SecurityDashboardDto>.SuccessResponse(dashboard, "Security dashboard retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security dashboard");
                return StatusCode(500, BaseResponse<SecurityDashboardDto>.Failure("An error occurred while retrieving security dashboard"));
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
                    return Unauthorized(BaseResponse<List<SecurityActivityDto>>.Failure("User not authenticated"));
                }
                
                var history = await _mfaService.GetSecurityActivityAsync(userId, limit);
                
                return Ok(BaseResponse<List<SecurityActivityDto>>.SuccessResponse(history, "Security audit history retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security audit history");
                return StatusCode(500, BaseResponse<List<SecurityActivityDto>>.Failure("An error occurred while retrieving security audit history"));
            }
        }

        /// <summary>
        /// Gets active sessions for the current user
        /// </summary>
        /// <returns>List of active sessions</returns>
        [HttpGet("active-sessions")]
        public async Task<ActionResult<BaseResponse<List<ClientSessionDto>>>> GetActiveSessions()
        {
            try
            {
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(BaseResponse<List<ClientSessionDto>>.Failure("User not authenticated"));
                }
                
                var sessions = await _authService.GetActiveSessionsAsync(userId);
                
                return Ok(BaseResponse<List<ClientSessionDto>>.SuccessResponse(sessions, "Active sessions retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active sessions");
                return StatusCode(500, BaseResponse<List<ClientSessionDto>>.Failure("An error occurred while retrieving active sessions"));
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
                    return Unauthorized(BaseResponse<bool>.Failure("User not authenticated"));
                }
                
                // Get current session ID
                var currentSessionId = User.FindFirstValue("sid");
                if (string.IsNullOrEmpty(currentSessionId))
                {
                    return BadRequest(BaseResponse<bool>.Failure("Current session ID not found"));
                }
                
                var result = await _authService.RevokeAllSessionsExceptCurrentAsync(userId, currentSessionId);
                
                if (!result)
                {
                    return BadRequest(BaseResponse<bool>.Failure("Failed to revoke sessions"));
                }
                
                return Ok(BaseResponse<bool>.SuccessResponse(true, "All sessions except current revoked successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all sessions except current");
                return StatusCode(500, BaseResponse<bool>.Failure("An error occurred while revoking sessions"));
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
                    return Unauthorized(BaseResponse<bool>.Failure("User not authenticated"));
                }
                
                // Check if trying to revoke current session
                var currentSessionId = User.FindFirstValue("sid");
                if (string.IsNullOrEmpty(currentSessionId))
                {
                    return BadRequest(BaseResponse<bool>.Failure("Current session ID not found"));
                }
                
                if (currentSessionId == sessionId)
                {
                    return BadRequest(BaseResponse<bool>.Failure("Cannot revoke current session"));
                }
                
                var result = await _authService.RevokeSessionAsync(userId, sessionId);
                
                if (!result)
                {
                    return BadRequest(BaseResponse<bool>.Failure("Failed to revoke session"));
                }
                
                return Ok(BaseResponse<bool>.SuccessResponse(true, "Session revoked successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking session {SessionId}", sessionId);
                return StatusCode(500, BaseResponse<bool>.Failure("An error occurred while revoking session"));
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
                    return Unauthorized(BaseResponse<SecurityPreferencesDto>.Failure("User not authenticated"));
                }
                
                var preferences = await _mfaService.GetSecurityPreferencesAsync(userId);
                
                return Ok(BaseResponse<SecurityPreferencesDto>.SuccessResponse(preferences, "Security preferences retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security preferences");
                return StatusCode(500, BaseResponse<SecurityPreferencesDto>.Failure("An error occurred while retrieving security preferences"));
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
                    return BadRequest(BaseResponse<bool>.Failure("Invalid security preferences"));
                }
                
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(BaseResponse<bool>.Failure("User not authenticated"));
                }
                
                var result = await _mfaService.UpdateSecurityPreferencesAsync(userId, preferences);
                
                if (!result)
                {
                    return BadRequest(BaseResponse<bool>.Failure("Failed to update security preferences"));
                }
                
                return Ok(BaseResponse<bool>.SuccessResponse(true, "Security preferences updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating security preferences");
                return StatusCode(500, BaseResponse<bool>.Failure("An error occurred while updating security preferences"));
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
                    return BadRequest(BaseResponse<bool>.Failure("Invalid password change request"));
                }
                
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(BaseResponse<bool>.Failure("User not authenticated"));
                }
                
                var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
                
                if (!result.Succeeded)
                {
                    return BadRequest(BaseResponse<bool>.Failure(string.Join(", ", result.Errors)));
                }
                
                return Ok(BaseResponse<bool>.SuccessResponse(true, "Password changed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, BaseResponse<bool>.Failure("An error occurred while changing password"));
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
}

