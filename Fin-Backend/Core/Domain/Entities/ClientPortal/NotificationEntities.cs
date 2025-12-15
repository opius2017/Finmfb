using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Customers;

namespace FinTech.Core.Domain.Entities.ClientPortal
{
    public class ClientNotification
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid CustomerId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string NotificationType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Action { get; set; }
        
        [StringLength(200)]
        public string? ActionData { get; set; }
        
        public bool IsRead { get; set; }
        
        public DateTime? ReadAt { get; set; }
        
        [StringLength(100)]
        public string? DeliveryChannels { get; set; }
        
        [StringLength(50)]
        public string? DeliveryStatus { get; set; }
        
        public DateTime? ExpiryDate { get; set; }
        
        public bool IsActionable { get; set; }
        
        public bool IsDismissed { get; set; }
        
        public int Priority { get; set; }
        
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }

    public class NotificationTemplate
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string TemplateCode { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string NotificationType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string MessageTemplate { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? DefaultAction { get; set; }
        
        public bool IsActive { get; set; }
        
        public bool RequiresSms { get; set; }
        
        public bool RequiresEmail { get; set; }
        
        public bool RequiresPush { get; set; }
        
        public bool RequiresInApp { get; set; }
        
        public int DefaultPriority { get; set; }
        
        public int DefaultExpiryDays { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }

    public class NotificationDeliveryRecord
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid NotificationId { get; set; }
        
        [Required]
        [StringLength(20)]
        public string ChannelType { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Recipient { get; set; }
        
        [StringLength(50)]
        public string? Status { get; set; }
        
        [StringLength(500)]
        public string? ErrorMessage { get; set; }
        
        public int RetryCount { get; set; }
        
        public DateTime? SentAt { get; set; }
        
        public DateTime? DeliveredAt { get; set; }
        
        [ForeignKey("NotificationId")]
        public virtual ClientNotification? Notification { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }
}
