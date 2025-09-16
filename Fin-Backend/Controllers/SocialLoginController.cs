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
    public class SocialLoginController : ControllerBase
    {
        private readonly IAdvancedAuthService _authService;
        private readonly ILogger<SocialLoginController> _logger;
        
        public SocialLoginController(
            IAdvancedAuthService authService,
            ILogger<SocialLoginController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all configured social login providers
        /// </summary>
        /// <returns>List of available social login providers</returns>
        [HttpGet("providers")]
        public ActionResult<BaseResponse<List<SocialLoginProviderDto>>> GetProviders()
        {
            try
            {
                var providers = _authService.GetSocialLoginProviders();
                
                return Ok(new BaseResponse<List<SocialLoginProviderDto>>
                {
                    Success = true,
                    Message = "Social login providers retrieved successfully",
                    Data = providers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving social login providers");
                return StatusCode(500, new BaseResponse<List<SocialLoginProviderDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving social login providers"
                });
            }
        }

        /// <summary>
        /// Initiates the social login process
        /// </summary>
        /// <param name="provider">The social login provider</param>
        /// <param name="returnUrl">The return URL after authentication</param>
        /// <returns>The social login URL</returns>
        [HttpGet("{provider}/authorize")]
        public async Task<ActionResult<BaseResponse<SocialLoginUrlDto>>> InitiateSocialLogin(string provider, [FromQuery] string returnUrl)
        {
            try
            {
                var result = await _authService.InitiateSocialLoginAsync(provider, returnUrl);
                
                return Ok(new BaseResponse<SocialLoginUrlDto>
                {
                    Success = true,
                    Message = $"Social login URL for {provider} generated successfully",
                    Data = new SocialLoginUrlDto
                    {
                        Provider = provider,
                        AuthorizationUrl = result.AuthorizationUrl,
                        State = result.State
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating social login for provider {Provider}", provider);
                return StatusCode(500, new BaseResponse<SocialLoginUrlDto>
                {
                    Success = false,
                    Message = $"An error occurred while initiating social login for {provider}"
                });
            }
        }

        /// <summary>
        /// Processes a social login callback
        /// </summary>
        /// <param name="provider">The social login provider</param>
        /// <param name="code">The authorization code</param>
        /// <param name="state">The state parameter</param>
        /// <returns>The authentication result</returns>
        [HttpGet("{provider}/callback")]
        public async Task<ActionResult<BaseResponse<AuthResult>>> ProcessSocialLoginCallback(string provider, [FromQuery] string code, [FromQuery] string state)
        {
            try
            {
                // Extract device info from request
                var deviceInfo = new DeviceInfo
                {
                    ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    DeviceId = Request.Headers["X-Device-Id"].ToString(),
                    Browser = Request.Headers["User-Agent"].ToString(),
                    DeviceName = Request.Headers["X-Device-Name"].ToString(),
                    DeviceType = Request.Headers["X-Device-Type"].ToString(),
                    OperatingSystem = Request.Headers["X-OS"].ToString(),
                    BrowserVersion = Request.Headers["X-Browser-Version"].ToString()
                };
                
                var result = await _authService.ProcessSocialLoginCallbackAsync(provider, code, state, deviceInfo);
                
                if (!result.Succeeded)
                {
                    return BadRequest(new BaseResponse<AuthResult>
                    {
                        Success = false,
                        Message = result.ErrorMessage,
                        Data = null
                    });
                }
                
                return Ok(new BaseResponse<AuthResult>
                {
                    Success = true,
                    Message = "Social login successful",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing social login callback for provider {Provider}", provider);
                return StatusCode(500, new BaseResponse<AuthResult>
                {
                    Success = false,
                    Message = $"An error occurred while processing social login for {provider}"
                });
            }
        }

        /// <summary>
        /// Links a social login account to the current user
        /// </summary>
        /// <param name="provider">The social login provider</param>
        /// <param name="token">The access token from the provider</param>
        /// <returns>Success or failure</returns>
        [HttpPost("link/{provider}")]
        [Authorize]
        public async Task<ActionResult<BaseResponse<bool>>> LinkSocialAccount(string provider, [FromBody] SocialLinkRequest request)
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
                
                var result = await _authService.LinkSocialLoginAsync(userId, provider, request.AccessToken, request.TokenSecret);
                
                if (!result)
                {
                    return BadRequest(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = $"Failed to link {provider} account",
                        Data = false
                    });
                }
                
                return Ok(new BaseResponse<bool>
                {
                    Success = true,
                    Message = $"{provider} account linked successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking social account for provider {Provider}", provider);
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while linking {provider} account"
                });
            }
        }

        /// <summary>
        /// Unlinks a social login account from the current user
        /// </summary>
        /// <param name="provider">The social login provider</param>
        /// <returns>Success or failure</returns>
        [HttpDelete("unlink/{provider}")]
        [Authorize]
        public async Task<ActionResult<BaseResponse<bool>>> UnlinkSocialAccount(string provider)
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
                
                var result = await _authService.UnlinkSocialLoginAsync(userId, provider);
                
                if (!result)
                {
                    return BadRequest(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = $"Failed to unlink {provider} account",
                        Data = false
                    });
                }
                
                return Ok(new BaseResponse<bool>
                {
                    Success = true,
                    Message = $"{provider} account unlinked successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlinking social account for provider {Provider}", provider);
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred while unlinking {provider} account"
                });
            }
        }

        /// <summary>
        /// Gets all social accounts linked to the current user
        /// </summary>
        /// <returns>List of linked social accounts</returns>
        [HttpGet("linked-accounts")]
        [Authorize]
        public async Task<ActionResult<BaseResponse<List<LinkedSocialAccountDto>>>> GetLinkedAccounts()
        {
            try
            {
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<List<LinkedSocialAccountDto>>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }
                
                var accounts = await _authService.GetLinkedSocialAccountsAsync(userId);
                
                return Ok(new BaseResponse<List<LinkedSocialAccountDto>>
                {
                    Success = true,
                    Message = "Linked social accounts retrieved successfully",
                    Data = accounts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving linked social accounts");
                return StatusCode(500, new BaseResponse<List<LinkedSocialAccountDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving linked social accounts"
                });
            }
        }
    }

    /// <summary>
    /// Social login authorization URL DTO
    /// </summary>
    public class SocialLoginUrlDto
    {
        /// <summary>
        /// Gets or sets the provider name
        /// </summary>
        public string Provider { get; set; }
        
        /// <summary>
        /// Gets or sets the authorization URL
        /// </summary>
        public string AuthorizationUrl { get; set; }
        
        /// <summary>
        /// Gets or sets the state parameter
        /// </summary>
        public string State { get; set; }
    }

    /// <summary>
    /// Request to link a social account
    /// </summary>
    public class SocialLinkRequest
    {
        /// <summary>
        /// Gets or sets the access token
        /// </summary>
        public string AccessToken { get; set; }
        
        /// <summary>
        /// Gets or sets the token secret (required for some providers like Twitter)
        /// </summary>
        public string TokenSecret { get; set; }
    }
}