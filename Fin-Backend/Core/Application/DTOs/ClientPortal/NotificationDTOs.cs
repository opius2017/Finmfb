using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FinTech.Core.Domain.Enums.Notifications;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    // Notification DTOs
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string NotificationType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string ActionData { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public string[] DeliveryChannels { get; set; } = Array.Empty<string>();
        public string DeliveryStatus { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public bool IsActionable { get; set; }
        public bool IsDismissed { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class NotificationCountDto
    {
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
        public Dictionary<string, int> CountByType { get; set; } = new Dictionary<string, int>();
    }

    public class MarkNotificationReadDto
    {
        public Guid Id { get; set; }
    }

    public class MarkAllNotificationsReadDto
    {
        public string NotificationType { get; set; } = string.Empty;
    }

    public class DismissNotificationDto
    {
        public Guid Id { get; set; }
    }

    public class NotificationFilterDto
    {
        public string? NotificationType { get; set; }
        public bool? IsRead { get; set; }
        public bool? IsActionable { get; set; }
        public bool? IsDismissed { get; set; }
        public int? MinPriority { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class CreateNotificationDto
    {
        [Required]
        public string NotificationType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;
        
        public string Action { get; set; } = string.Empty;
        
        public string ActionData { get; set; } = string.Empty;
        
        public NotificationChannel[] DeliveryChannels { get; set; } = Array.Empty<NotificationChannel>();
        
        public DateTime? ExpiryDate { get; set; }
        
        public bool IsActionable { get; set; }
        
        public int Priority { get; set; }
        
        [Required]
        public string CustomerId { get; set; } = string.Empty;
    }

    public class NotificationTemplateDto
    {
        public Guid Id { get; set; }
        public string TemplateCode { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string MessageTemplate { get; set; } = string.Empty;
        public string DefaultAction { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool RequiresSms { get; set; }
        public bool RequiresEmail { get; set; }
        public bool RequiresPush { get; set; }
        public bool RequiresInApp { get; set; }
        public int DefaultPriority { get; set; }
        public int DefaultExpiryDays { get; set; }
    }

    public class SendTemplatedNotificationDto
    {
        [Required]
        public string TemplateCode { get; set; } = string.Empty;
        
        [Required]
        public string CustomerId { get; set; } = string.Empty;
        
        public Dictionary<string, string> TemplateData { get; set; } = new Dictionary<string, string>();
        
        public NotificationChannel[] OverrideChannels { get; set; } = Array.Empty<NotificationChannel>();
    }

    // Mapping classes for domain entities to DTOs
    public class NotificationMappingProfile
    {
        // Maps ClientNotification -> NotificationDto
        public static NotificationDto? MapToDto(FinTech.Core.Domain.Entities.ClientPortal.ClientNotification notification)
        {
            if (notification == null)
                return null;

            return new NotificationDto
            {
                Id = notification.Id,
                NotificationType = notification.NotificationType,
                Title = notification.Title,
                Message = notification.Message,
                Action = notification.Action,
                ActionData = notification.ActionData,
                IsRead = notification.IsRead,
                ReadAt = notification.ReadAt,
                DeliveryChannels = notification.DeliveryChannels?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
                DeliveryStatus = notification.DeliveryStatus,
                ExpiryDate = notification.ExpiryDate,
                IsActionable = notification.IsActionable,
                IsDismissed = notification.IsDismissed,
                Priority = notification.Priority,
                CreatedOn = notification.CreatedAt
            };
        }

        // Maps NotificationTemplate -> NotificationTemplateDto
        public static NotificationTemplateDto? MapToDto(FinTech.Core.Domain.Entities.ClientPortal.NotificationTemplate template)
        {
            if (template == null)
                return null;

            return new NotificationTemplateDto
            {
                Id = template.Id,
                TemplateCode = template.TemplateCode,
                NotificationType = template.NotificationType,
                Title = template.Title,
                MessageTemplate = template.MessageTemplate,
                DefaultAction = template.DefaultAction,
                IsActive = template.IsActive,
                RequiresSms = template.RequiresSms,
                RequiresEmail = template.RequiresEmail,
                RequiresPush = template.RequiresPush,
                RequiresInApp = template.RequiresInApp,
                DefaultPriority = template.DefaultPriority,
                DefaultExpiryDays = template.DefaultExpiryDays
            };
        }
    }
}
