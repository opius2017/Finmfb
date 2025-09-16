using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using FinTech.Application.Services;
using FinTech.Application.DTOs.ClientPortal;
using System.Security.Claims;
using System.Linq;

namespace FinTech.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/client/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications([FromQuery] NotificationFilterDto filter = null)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var notifications = await _notificationService.GetUserNotificationsAsync(customerId, filter);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications");
                return StatusCode(500, "An error occurred while retrieving notifications");
            }
        }

        [HttpGet("count")]
        public async Task<ActionResult<NotificationCountDto>> GetNotificationCounts()
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var counts = await _notificationService.GetNotificationCountsAsync(customerId);
                return Ok(counts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification counts");
                return StatusCode(500, "An error occurred while retrieving notification counts");
            }
        }

        [HttpGet("{notificationId}")]
        public async Task<ActionResult<NotificationDto>> GetNotification(Guid notificationId)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var notification = await _notificationService.GetNotificationByIdAsync(notificationId, customerId);
                return Ok(notification);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification");
                return StatusCode(500, "An error occurred while retrieving the notification");
            }
        }

        [HttpPut("{notificationId}/read")]
        public async Task<ActionResult> MarkAsRead(Guid notificationId)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var result = await _notificationService.MarkNotificationAsReadAsync(notificationId, customerId);
                return Ok(new { success = result });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return StatusCode(500, "An error occurred while marking the notification as read");
            }
        }

        [HttpPut("read-all")]
        public async Task<ActionResult> MarkAllAsRead([FromQuery] string notificationType = null)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var result = await _notificationService.MarkAllNotificationsAsReadAsync(customerId, notificationType);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(500, "An error occurred while marking all notifications as read");
            }
        }

        [HttpPut("{notificationId}/dismiss")]
        public async Task<ActionResult> DismissNotification(Guid notificationId)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var result = await _notificationService.DismissNotificationAsync(notificationId, customerId);
                return Ok(new { success = result });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dismissing notification");
                return StatusCode(500, "An error occurred while dismissing the notification");
            }
        }

        [HttpPut("dismiss-all")]
        public async Task<ActionResult> DismissAllNotifications([FromQuery] string notificationType = null)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var result = await _notificationService.DismissAllNotificationsAsync(customerId, notificationType);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dismissing all notifications");
                return StatusCode(500, "An error occurred while dismissing all notifications");
            }
        }

        // Helper methods
        private Guid GetCustomerIdFromClaims()
        {
            var customerId = User.Claims.FirstOrDefault(c => c.Type == "CustomerId")?.Value;
            if (string.IsNullOrEmpty(customerId))
            {
                throw new UnauthorizedAccessException("Customer ID not found in claims");
            }
            return Guid.Parse(customerId);
        }
    }
}