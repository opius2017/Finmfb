using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class ClientNotificationDto
    {
        public Guid Id { get; set; }
        public string NotificationType { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Action { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public string DeliveryChannels { get; set; }
        public string DeliveryStatus { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActionable { get; set; }
        public bool IsDismissed { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
