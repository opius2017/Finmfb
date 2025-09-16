using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FinTech.Domain.Enums;

namespace FinTech.Application.DTOs.ClientPortal
{
    // Notification DTOs
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string NotificationType { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Action { get; set; }
        public string ActionData { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public string[] DeliveryChannels { get; set; }
        public string DeliveryStatus { get; set; }
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
        public Dictionary<string, int> CountByType { get; set; }
    }

    public class MarkNotificationReadDto
    {
        public Guid Id { get; set; }
    }

    public class MarkAllNotificationsReadDto
    {
        public string NotificationType { get; set; }
    }

    public class DismissNotificationDto
    {
        public Guid Id { get; set; }
    }

    public class NotificationFilterDto
    {
        public string NotificationType { get; set; }
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
        public string NotificationType { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Message { get; set; }
        
        public string Action { get; set; }
        
        public string ActionData { get; set; }
        
        public NotificationChannel[] DeliveryChannels { get; set; }
        
        public DateTime? ExpiryDate { get; set; }
        
        public bool IsActionable { get; set; }
        
        public int Priority { get; set; }
        
        [Required]
        public Guid CustomerId { get; set; }
    }

    public class NotificationTemplateDto
    {
        public Guid Id { get; set; }
        public string TemplateCode { get; set; }
        public string NotificationType { get; set; }
        public string Title { get; set; }
        public string MessageTemplate { get; set; }
        public string DefaultAction { get; set; }
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
        public string TemplateCode { get; set; }
        
        [Required]
        public Guid CustomerId { get; set; }
        
        public Dictionary<string, string> TemplateData { get; set; }
        
        public NotificationChannel[] OverrideChannels { get; set; }
    }

    // Mapping classes for domain entities to DTOs
    public class NotificationMappingProfile
    {
        // Maps ClientNotification -> NotificationDto
        public static NotificationDto MapToDto(FinTech.Domain.Entities.ClientPortal.ClientNotification notification)
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
                DeliveryChannels = notification.DeliveryChannels?.Split(',', StringSplitOptions.RemoveEmptyEntries),
                DeliveryStatus = notification.DeliveryStatus,
                ExpiryDate = notification.ExpiryDate,
                IsActionable = notification.IsActionable,
                IsDismissed = notification.IsDismissed,
                Priority = notification.Priority,
                CreatedOn = notification.CreatedAt
            };
        }

        // Maps NotificationTemplate -> NotificationTemplateDto
        public static NotificationTemplateDto MapToDto(FinTech.Domain.Entities.ClientPortal.NotificationTemplate template)
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