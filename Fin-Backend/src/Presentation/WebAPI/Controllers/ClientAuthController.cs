using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FinTech.Application.DTOs.Auth;
using FinTech.Application.DTOs.Common;
using FinTech.Application.Services;
using FinTech.Application.Common.Models;

namespace FinTech.WebAPI.Controllers
{
    [ApiController]
    [Route("api/client/auth")]
    public class ClientAuthController : ControllerBase
    {
        private readonly IClientAuthService _authService;
        private readonly ILogger<ClientAuthController> _logger;

        public ClientAuthController(
            IClientAuthService authService,
            ILogger<ClientAuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Client login endpoint
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                var deviceId = Request.Headers["X-Device-ID"].ToString();
                
                if (string.IsNullOrEmpty(deviceId))
                {
                    deviceId = Guid.NewGuid().ToString(); // Generate a device ID if not provided
                }
                
                var response = await _authService.ClientLoginAsync(request, ipAddress, userAgent, deviceId);
                
                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return Unauthorized(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during client login");
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred during login"
                });
            }
        }

        /// <summary>
        /// Client logout endpoint
        /// </summary>
        [HttpPost("logout")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var customerId = Guid.Parse(User.FindFirst("CustomerId")?.Value);
                var sessionToken = Request.Headers["X-Refresh-Token"].ToString();
                
                var response = await _authService.ClientLogoutAsync(customerId, sessionToken);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during client logout");
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred during logout",
                    Data = false
                });
            }
        }

        /// <summary>
        /// Refresh token endpoint
        /// </summary>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(BaseResponse<string>), 200)]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var refreshToken = Request.Headers["X-Refresh-Token"].ToString();
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return BadRequest(new BaseResponse<string>
                    {
                        Success = false,
                        Message = "Refresh token is required",
                        Data = null
                    });
                }
                
                var response = await _authService.RefreshTokenAsync(refreshToken, ipAddress);
                
                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return Unauthorized(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, new BaseResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while refreshing token",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Validate client session
        /// </summary>
        [HttpPost("validate-session")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> ValidateSession()
        {
            try
            {
                var customerId = Guid.Parse(User.FindFirst("CustomerId")?.Value);
                var sessionToken = Request.Headers["X-Refresh-Token"].ToString();
                
                var response = await _authService.ValidateClientSessionAsync(customerId, sessionToken);
                
                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return Unauthorized(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating session");
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while validating session",
                    Data = false
                });
            }
        }

        /// <summary>
        /// Send password reset link
        /// </summary>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var response = await _authService.SendPasswordResetLinkAsync(request.Email);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset link");
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while sending password reset link",
                    Data = false
                });
            }
        }

        /// <summary>
        /// Reset password
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var response = await _authService.ResetPasswordAsync(request.Email, request.Token, request.Password);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while resetting password",
                    Data = false
                });
            }
        }

        /// <summary>
        /// Verify two-factor code
        /// </summary>
        [HttpPost("verify-two-factor")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorRequest request)
        {
            try
            {
                var response = await _authService.VerifyTwoFactorCodeAsync(request.CustomerId, request.Code);
                
                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying two-factor code");
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while verifying the code",
                    Data = false
                });
            }
        }

        /// <summary>
        /// Send two-factor code
        /// </summary>
        [HttpPost("send-two-factor")]
        [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
        public async Task<IActionResult> SendTwoFactor([FromBody] SendTwoFactorRequest request)
        {
            try
            {
                var response = await _authService.SendTwoFactorCodeAsync(request.CustomerId, request.Method);
                
                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending two-factor code");
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while sending verification code",
                    Data = false
                });
            }
        }
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string Password { get; set; }
    }

    public class VerifyTwoFactorRequest
    {
        public Guid CustomerId { get; set; }
        public string Code { get; set; }
    }

    public class SendTwoFactorRequest
    {
        public Guid CustomerId { get; set; }
        public string Method { get; set; }
    }
}