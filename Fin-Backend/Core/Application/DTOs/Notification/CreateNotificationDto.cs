using System;
using FinTech.Core.Domain.Enums.Notifications;

namespace FinTech.Core.Application.DTOs.Notification
{
    public class CreateNotificationDto
    {
        public Guid CustomerId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string NotificationType { get; set; }
        public string Action { get; set; }
        public NotificationPriority Priority { get; set; }
        public NotificationChannel[] DeliveryChannels { get; set; }
    }
}
