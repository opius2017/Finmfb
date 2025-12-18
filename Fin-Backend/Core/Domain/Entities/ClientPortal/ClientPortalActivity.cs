using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.ClientPortal
{
    /// <summary>
    /// Represents client portal activity tracking.
    /// Refactored for FSD and DDD compliance.
    /// </summary>
    public sealed class ClientPortalActivity : BaseEntity, IAuditable
    {
        public string? ClientPortalProfileId { get; private set; }
        
        [Required]
        public Guid UserId { get; set; }
        
        public string? SessionId { get; set; }
        
        // Navigation property
        public ClientSession? Session { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string ActivityType { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; }
        
        [MaxLength(45)]
        public string? IpAddress { get; set; }
        
        public bool IsSuccessful { get; set; }
        
        [MaxLength(1000)]
        public string? AdditionalData { get; set; }
        
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        // Compatibility Properties (Legacy Support)
        [NotMapped]
        public Guid CustomerId { get => UserId; set => UserId = value; }

        [NotMapped]
        public DateTime ActivityDate { get => Timestamp; set => Timestamp = value; }

        [NotMapped]
        public string Status 
        { 
            get => IsSuccessful ? "Success" : "Failed"; 
            set => IsSuccessful = value?.Equals("Success", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        // Public parameterless constructor for EF Core and service layer
        public ClientPortalActivity() { }

        // Private constructor for Factory
        private ClientPortalActivity(
            string? clientPortalProfileId,
            Guid userId,
            string? sessionId,
            string activityType,
            string description,
            string ipAddress,
            string userAgent,
            string? additionalData)
        {
            ClientPortalProfileId = clientPortalProfileId;
            UserId = userId;
            SessionId = sessionId;
            ActivityType = activityType ?? throw new ArgumentNullException(nameof(activityType));
            Description = description;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            AdditionalData = additionalData;
            Timestamp = DateTime.UtcNow;
            IsSuccessful = true; // Default to success, mark failure explicitly
            
            // Add Domain Event
            // AddDomainEvent(new ClientPortalActivityCreatedEvent(this));
        }

        public static ClientPortalActivity Create(
            string? clientPortalProfileId,
            Guid userId,
            string? sessionId,
            string activityType,
            string description,
            string ipAddress,
            string userAgent,
            string? additionalData = null)
        {
            return new ClientPortalActivity(
                clientPortalProfileId,
                userId,
                sessionId,
                activityType,
                description,
                ipAddress,
                userAgent,
                additionalData);
        }

        public void MarkAsFailure(string reason)
        {
            IsSuccessful = false;
            if (!string.IsNullOrEmpty(reason))
            {
                Description = string.IsNullOrEmpty(Description) ? reason : $"{Description} | Failure: {reason}";
            }
        }
    }
}
