using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FinTech.Application.Common.Interfaces;
using FinTech.Application.DTOs.ClientPortal;
using FinTech.Domain.Entities.ClientPortal;
using FinTech.Domain.Enums;
using System.Text.Json;
using FinTech.Core.Application.Common.Interfaces;

namespace FinTech.Application.Services
{
    public interface INotificationService
    {
        // Core notification methods
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid customerId, NotificationFilterDto filter = null);
        Task<NotificationDto> GetNotificationByIdAsync(Guid notificationId, Guid customerId);
        Task<NotificationCountDto> GetNotificationCountsAsync(Guid customerId);
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto notificationDto);
        Task<bool> MarkNotificationAsReadAsync(Guid notificationId, Guid customerId);
        Task<bool> MarkAllNotificationsAsReadAsync(Guid customerId, string notificationType = null);
        Task<bool> DismissNotificationAsync(Guid notificationId, Guid customerId);
        Task<bool> DismissAllNotificationsAsync(Guid customerId, string notificationType = null);
        
        // Templated notifications
        Task<IEnumerable<NotificationTemplateDto>> GetNotificationTemplatesAsync(string notificationType = null);
        Task<NotificationDto> SendTemplatedNotificationAsync(SendTemplatedNotificationDto notificationDto);
        
        // External channel delivery (email, SMS, push)
        Task<bool> DeliverExternalNotificationsAsync(Guid notificationId);
        Task<bool> ProcessPendingNotificationDeliveriesAsync();
    }

    public class NotificationService : INotificationService
    {
        private readonly IApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly IPushNotificationService _pushService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IApplicationDbContext context,
            IEmailService emailService,
            ISmsService smsService,
            IPushNotificationService pushService,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _emailService = emailService;
            _smsService = smsService;
            _pushService = pushService;
            _logger = logger;
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid customerId, NotificationFilterDto filter = null)
        {
            try
            {
                IQueryable<ClientNotification> query = _context.ClientNotifications
                    .Where(n => n.CustomerId == customerId);

                if (filter != null)
                {
                    // Apply filters
                    if (!string.IsNullOrEmpty(filter.NotificationType))
                    {
                        query = query.Where(n => n.NotificationType == filter.NotificationType);
                    }

                    if (filter.IsRead.HasValue)
                    {
                        query = query.Where(n => n.IsRead == filter.IsRead.Value);
                    }

                    if (filter.IsActionable.HasValue)
                    {
                        query = query.Where(n => n.IsActionable == filter.IsActionable.Value);
                    }

                    if (filter.IsDismissed.HasValue)
                    {
                        query = query.Where(n => n.IsDismissed == filter.IsDismissed.Value);
                    }

                    if (filter.MinPriority.HasValue)
                    {
                        query = query.Where(n => n.Priority >= filter.MinPriority.Value);
                    }

                    if (filter.FromDate.HasValue)
                    {
                        query = query.Where(n => n.CreatedAt >= filter.FromDate.Value);
                    }

                    if (filter.ToDate.HasValue)
                    {
                        query = query.Where(n => n.CreatedAt <= filter.ToDate.Value);
                    }
                }

                // Get only non-expired notifications
                query = query.Where(n => !n.ExpiryDate.HasValue || n.ExpiryDate > DateTime.UtcNow);

                // Order by priority and creation date
                var notifications = await query
                    .OrderByDescending(n => n.Priority)
                    .ThenByDescending(n => n.CreatedAt)
                    .ToListAsync();

                return notifications.Select(n => NotificationMappingProfile.MapToDto(n));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<NotificationDto> GetNotificationByIdAsync(Guid notificationId, Guid customerId)
        {
            try
            {
                var notification = await _context.ClientNotifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.CustomerId == customerId);

                if (notification == null)
                {
                    throw new KeyNotFoundException($"Notification with ID {notificationId} not found for this customer.");
                }

                return NotificationMappingProfile.MapToDto(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification {NotificationId} for customer {CustomerId}", notificationId, customerId);
                throw;
            }
        }

        public async Task<NotificationCountDto> GetNotificationCountsAsync(Guid customerId)
        {
            try
            {
                var notifications = await _context.ClientNotifications
                    .Where(n => n.CustomerId == customerId)
                    .Where(n => !n.IsDismissed)
                    .Where(n => !n.ExpiryDate.HasValue || n.ExpiryDate > DateTime.UtcNow)
                    .ToListAsync();

                var totalCount = notifications.Count;
                var unreadCount = notifications.Count(n => !n.IsRead);
                
                var countByType = notifications
                    .GroupBy(n => n.NotificationType)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Count()
                    );

                return new NotificationCountDto
                {
                    TotalCount = totalCount,
                    UnreadCount = unreadCount,
                    CountByType = countByType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification counts for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto notificationDto)
        {
            try
            {
                // Get customer information
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == notificationDto.CustomerId);

                if (customer == null)
                {
                    throw new KeyNotFoundException($"Customer with ID {notificationDto.CustomerId} not found.");
                }

                // Create the notification
                var notification = new ClientNotification
                {
                    CustomerId = notificationDto.CustomerId,
                    NotificationType = notificationDto.NotificationType,
                    Title = notificationDto.Title,
                    Message = notificationDto.Message,
                    Action = notificationDto.Action,
                    ActionData = notificationDto.ActionData,
                    IsRead = false,
                    ReadAt = null,
                    DeliveryChannels = notificationDto.DeliveryChannels != null ? string.Join(",", notificationDto.DeliveryChannels.Select(c => c.ToString())) : "InApp",
                    DeliveryStatus = NotificationDeliveryStatus.Pending.ToString(),
                    ExpiryDate = notificationDto.ExpiryDate,
                    IsActionable = notificationDto.IsActionable,
                    IsDismissed = false,
                    Priority = notificationDto.Priority,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ClientNotifications.Add(notification);
                await _context.SaveChangesAsync();

                // Create delivery records for each channel
                if (notificationDto.DeliveryChannels != null && notificationDto.DeliveryChannels.Length > 0)
                {
                    foreach (var channel in notificationDto.DeliveryChannels)
                    {
                        var deliveryRecord = new NotificationDeliveryRecord
                        {
                            NotificationId = notification.Id,
                            ChannelType = channel.ToString(),
                            Recipient = GetRecipientForChannel(customer, channel),
                            Status = NotificationDeliveryStatus.Pending.ToString(),
                            RetryCount = 0,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.NotificationDeliveryRecords.Add(deliveryRecord);
                    }

                    await _context.SaveChangesAsync();
                }

                // Attempt to deliver external notifications (email, SMS, push)
                if (notificationDto.DeliveryChannels != null && 
                    notificationDto.DeliveryChannels.Any(c => c != NotificationChannel.InApp))
                {
                    // Non-blocking call to deliver external notifications
                    _ = Task.Run(() => DeliverExternalNotificationsAsync(notification.Id));
                }

                return NotificationMappingProfile.MapToDto(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for customer {CustomerId}", notificationDto.CustomerId);
                throw;
            }
        }

        public async Task<bool> MarkNotificationAsReadAsync(Guid notificationId, Guid customerId)
        {
            try
            {
                var notification = await _context.ClientNotifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.CustomerId == customerId);

                if (notification == null)
                {
                    throw new KeyNotFoundException($"Notification with ID {notificationId} not found for this customer.");
                }

                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    notification.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read for customer {CustomerId}", notificationId, customerId);
                throw;
            }
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(Guid customerId, string notificationType = null)
        {
            try
            {
                IQueryable<ClientNotification> query = _context.ClientNotifications
                    .Where(n => n.CustomerId == customerId && !n.IsRead);

                if (!string.IsNullOrEmpty(notificationType))
                {
                    query = query.Where(n => n.NotificationType == notificationType);
                }

                var notifications = await query.ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    notification.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> DismissNotificationAsync(Guid notificationId, Guid customerId)
        {
            try
            {
                var notification = await _context.ClientNotifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.CustomerId == customerId);

                if (notification == null)
                {
                    throw new KeyNotFoundException($"Notification with ID {notificationId} not found for this customer.");
                }

                if (!notification.IsDismissed)
                {
                    notification.IsDismissed = true;
                    notification.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dismissing notification {NotificationId} for customer {CustomerId}", notificationId, customerId);
                throw;
            }
        }

        public async Task<bool> DismissAllNotificationsAsync(Guid customerId, string notificationType = null)
        {
            try
            {
                IQueryable<ClientNotification> query = _context.ClientNotifications
                    .Where(n => n.CustomerId == customerId && !n.IsDismissed);

                if (!string.IsNullOrEmpty(notificationType))
                {
                    query = query.Where(n => n.NotificationType == notificationType);
                }

                var notifications = await query.ToListAsync();

                foreach (var notification in notifications)
                {
                    notification.IsDismissed = true;
                    notification.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dismissing all notifications for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<NotificationTemplateDto>> GetNotificationTemplatesAsync(string notificationType = null)
        {
            try
            {
                IQueryable<NotificationTemplate> query = _context.NotificationTemplates
                    .Where(t => t.IsActive);

                if (!string.IsNullOrEmpty(notificationType))
                {
                    query = query.Where(t => t.NotificationType == notificationType);
                }

                var templates = await query.ToListAsync();
                return templates.Select(t => NotificationMappingProfile.MapToDto(t));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification templates");
                throw;
            }
        }

        public async Task<NotificationDto> SendTemplatedNotificationAsync(SendTemplatedNotificationDto notificationDto)
        {
            try
            {
                // Get template
                var template = await _context.NotificationTemplates
                    .FirstOrDefaultAsync(t => t.TemplateCode == notificationDto.TemplateCode && t.IsActive);

                if (template == null)
                {
                    throw new KeyNotFoundException($"Notification template with code {notificationDto.TemplateCode} not found or inactive.");
                }

                // Get customer information
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == notificationDto.CustomerId);

                if (customer == null)
                {
                    throw new KeyNotFoundException($"Customer with ID {notificationDto.CustomerId} not found.");
                }

                // Prepare delivery channels
                var deliveryChannels = notificationDto.OverrideChannels != null && notificationDto.OverrideChannels.Length > 0
                    ? notificationDto.OverrideChannels
                    : GetChannelsFromTemplate(template);

                // Process template
                var title = ProcessTemplate(template.Title, notificationDto.TemplateData);
                var message = ProcessTemplate(template.MessageTemplate, notificationDto.TemplateData);

                // Create the notification
                var expiryDate = template.DefaultExpiryDays > 0
                    ? DateTime.UtcNow.AddDays(template.DefaultExpiryDays)
                    : (DateTime?)null;

                var createDto = new CreateNotificationDto
                {
                    CustomerId = notificationDto.CustomerId,
                    NotificationType = template.NotificationType,
                    Title = title,
                    Message = message,
                    Action = template.DefaultAction,
                    ActionData = null, // Could be derived from template data if needed
                    DeliveryChannels = deliveryChannels,
                    ExpiryDate = expiryDate,
                    IsActionable = !string.IsNullOrEmpty(template.DefaultAction),
                    Priority = template.DefaultPriority
                };

                return await CreateNotificationAsync(createDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending templated notification to customer {CustomerId}", notificationDto.CustomerId);
                throw;
            }
        }

        public async Task<bool> DeliverExternalNotificationsAsync(Guid notificationId)
        {
            try
            {
                var notification = await _context.ClientNotifications
                    .Include(n => n.Customer)
                    .FirstOrDefaultAsync(n => n.Id == notificationId);

                if (notification == null)
                {
                    throw new KeyNotFoundException($"Notification with ID {notificationId} not found.");
                }

                var deliveryRecords = await _context.NotificationDeliveryRecords
                    .Where(d => d.NotificationId == notificationId && d.Status == NotificationDeliveryStatus.Pending.ToString())
                    .ToListAsync();

                if (!deliveryRecords.Any())
                {
                    return true; // No external deliveries pending
                }

                foreach (var record in deliveryRecords)
                {
                    try
                    {
                        bool deliverySuccess = false;

                        switch (record.ChannelType)
                        {
                            case "Email":
                                deliverySuccess = await DeliverViaEmail(notification, record);
                                break;
                            case "SMS":
                                deliverySuccess = await DeliverViaSms(notification, record);
                                break;
                            case "Push":
                                deliverySuccess = await DeliverViaPush(notification, record);
                                break;
                            default:
                                record.Status = NotificationDeliveryStatus.Failed.ToString();
                                record.ErrorMessage = "Invalid channel type";
                                break;
                        }

                        record.SentAt = DateTime.UtcNow;
                        record.UpdatedAt = DateTime.UtcNow;

                        if (deliverySuccess)
                        {
                            record.Status = NotificationDeliveryStatus.Sent.ToString();
                            record.DeliveredAt = DateTime.UtcNow;
                        }
                        else
                        {
                            record.Status = NotificationDeliveryStatus.Failed.ToString();
                            record.RetryCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        record.Status = NotificationDeliveryStatus.Failed.ToString();
                        record.ErrorMessage = ex.Message;
                        record.RetryCount++;
                        _logger.LogError(ex, "Error delivering notification {NotificationId} via {ChannelType}", notificationId, record.ChannelType);
                    }
                }

                // Update notification delivery status
                UpdateNotificationDeliveryStatus(notification, deliveryRecords);
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delivering external notifications for notification {NotificationId}", notificationId);
                throw;
            }
        }

        public async Task<bool> ProcessPendingNotificationDeliveriesAsync()
        {
            try
            {
                // Get notifications with pending deliveries
                var notificationIds = await _context.NotificationDeliveryRecords
                    .Where(d => d.Status == NotificationDeliveryStatus.Pending.ToString() && d.RetryCount < 3)
                    .Select(d => d.NotificationId)
                    .Distinct()
                    .ToListAsync();

                foreach (var notificationId in notificationIds)
                {
                    await DeliverExternalNotificationsAsync(notificationId);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pending notification deliveries");
                throw;
            }
        }

        #region Helper Methods

        private string GetRecipientForChannel(Domain.Entities.Customers.Customer customer, NotificationChannel channel)
        {
            switch (channel)
            {
                case NotificationChannel.Email:
                    return customer.Email;
                case NotificationChannel.SMS:
                    return customer.PhoneNumber;
                case NotificationChannel.Push:
                    // Get push notification token from client portal profile
                    var profile = _context.ClientPortalProfiles
                        .FirstOrDefault(p => p.CustomerId == customer.Id);
                    return profile?.PushNotificationToken;
                default:
                    return null;
            }
        }

        private NotificationChannel[] GetChannelsFromTemplate(NotificationTemplate template)
        {
            var channels = new List<NotificationChannel>();

            if (template.RequiresInApp)
                channels.Add(NotificationChannel.InApp);
            if (template.RequiresEmail)
                channels.Add(NotificationChannel.Email);
            if (template.RequiresSms)
                channels.Add(NotificationChannel.SMS);
            if (template.RequiresPush)
                channels.Add(NotificationChannel.Push);

            return channels.ToArray();
        }

        private string ProcessTemplate(string templateText, Dictionary<string, string> templateData)
        {
            if (string.IsNullOrEmpty(templateText) || templateData == null)
                return templateText;

            string result = templateText;
            foreach (var item in templateData)
            {
                result = result.Replace($"{{{item.Key}}}", item.Value);
            }

            return result;
        }

        private async Task<bool> DeliverViaEmail(ClientNotification notification, NotificationDeliveryRecord record)
        {
            if (string.IsNullOrEmpty(record.Recipient))
                return false;

            return await _emailService.SendEmailAsync(
                record.Recipient,
                notification.Title,
                notification.Message);
        }

        private async Task<bool> DeliverViaSms(ClientNotification notification, NotificationDeliveryRecord record)
        {
            if (string.IsNullOrEmpty(record.Recipient))
                return false;

            return await _smsService.SendSmsAsync(
                record.Recipient,
                notification.Message);
        }

        private async Task<bool> DeliverViaPush(ClientNotification notification, NotificationDeliveryRecord record)
        {
            if (string.IsNullOrEmpty(record.Recipient))
                return false;

            var payload = new
            {
                title = notification.Title,
                body = notification.Message,
                notificationId = notification.Id.ToString(),
                notificationType = notification.NotificationType,
                action = notification.Action
            };

            return await _pushService.SendPushNotificationAsync(
                record.Recipient,
                notification.Title,
                notification.Message,
                JsonSerializer.Serialize(payload));
        }

        private void UpdateNotificationDeliveryStatus(ClientNotification notification, List<NotificationDeliveryRecord> deliveryRecords)
        {
            if (deliveryRecords.Any(d => d.Status == NotificationDeliveryStatus.Sent.ToString()))
            {
                notification.DeliveryStatus = NotificationDeliveryStatus.Sent.ToString();
            }
            else if (deliveryRecords.All(d => d.Status == NotificationDeliveryStatus.Failed.ToString()))
            {
                notification.DeliveryStatus = NotificationDeliveryStatus.Failed.ToString();
            }
            else
            {
                notification.DeliveryStatus = NotificationDeliveryStatus.Pending.ToString();
            }

            notification.UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}