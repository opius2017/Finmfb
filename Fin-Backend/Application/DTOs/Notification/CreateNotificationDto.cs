using FinTech.Domain.Enums;
using System;

namespace FinTech.Application.DTOs.Notification
{
    public class CreateNotificationDto
    {
        public Guid CustomerId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string NotificationType { get; set; }
        public string Action { get; set; }
        public NotificationChannel[] DeliveryChannels { get; set; }
    }
}
