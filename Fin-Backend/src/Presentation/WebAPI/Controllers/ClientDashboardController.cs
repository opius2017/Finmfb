using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FinTech.Application.DTOs.ClientPortal;
using FinTech.Application.DTOs.Common;
using FinTech.Application.Services;
using FinTech.Application.Common.Models;

namespace FinTech.WebAPI.Controllers
{
    [ApiController]
    [Route("api/client/dashboard")]
    [Authorize(Roles = "Client")]
    public class ClientDashboardController : ControllerBase
    {
        private readonly IClientDashboardService _dashboardService;
        private readonly ILogger<ClientDashboardController> _logger;

        public ClientDashboardController(
            IClientDashboardService dashboardService,
            ILogger<ClientDashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Get the client dashboard data including account balances, transactions, loans, and notifications
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(BaseResponse<ClientDashboardDto>), 200)]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                // Get customer ID from the authenticated user
                var customerId = Guid.Parse(User.FindFirst("CustomerId")?.Value);

                var dashboardData = await _dashboardService.GetDashboardDataAsync(customerId);

                return Ok(new BaseResponse<ClientDashboardDto>
                {
                    Success = true,
                    Message = "Dashboard data retrieved successfully",
                    Data = dashboardData
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Client dashboard data not found");
                return NotFound(new BaseResponse<ClientDashboardDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client dashboard data");
                return StatusCode(500, new BaseResponse<ClientDashboardDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving dashboard data"
                });
            }
        }

        /// <summary>
        /// Get the client's dashboard preferences
        /// </summary>
        [HttpGet("preferences")]
        [ProducesResponseType(typeof(BaseResponse<DashboardPreferencesDto>), 200)]
        public async Task<IActionResult> GetDashboardPreferences()
        {
            try
            {
                var customerId = Guid.Parse(User.FindFirst("CustomerId")?.Value);
                var preferences = await _dashboardService.GetDashboardPreferencesAsync(customerId);

                return Ok(new BaseResponse<DashboardPreferencesDto>
                {
                    Success = true,
                    Message = "Dashboard preferences retrieved successfully",
                    Data = preferences
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Client dashboard preferences not found");
                return NotFound(new BaseResponse<DashboardPreferencesDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client dashboard preferences");
                return StatusCode(500, new BaseResponse<DashboardPreferencesDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving dashboard preferences"
                });
            }
        }

        /// <summary>
        /// Update the client's dashboard preferences
        /// </summary>
        [HttpPut("preferences")]
        [ProducesResponseType(typeof(BaseResponse<DashboardPreferencesDto>), 200)]
        public async Task<IActionResult> UpdateDashboardPreferences([FromBody] DashboardPreferencesUpdateDto preferencesDto)
        {
            try
            {
                var customerId = Guid.Parse(User.FindFirst("CustomerId")?.Value);
                var updatedPreferences = await _dashboardService.UpdateDashboardPreferencesAsync(customerId, preferencesDto);

                return Ok(new BaseResponse<DashboardPreferencesDto>
                {
                    Success = true,
                    Message = "Dashboard preferences updated successfully",
                    Data = updatedPreferences
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Client dashboard preferences not found");
                return NotFound(new BaseResponse<DashboardPreferencesDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client dashboard preferences");
                return StatusCode(500, new BaseResponse<DashboardPreferencesDto>
                {
                    Success = false,
                    Message = "An error occurred while updating dashboard preferences"
                });
            }
        }
    }
}