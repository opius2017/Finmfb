using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTech.Domain.Entities.ClientPortal
{
    public class ClientPortalProfile
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [StringLength(50)]
        public string PreferredLanguage { get; set; } = "en";

        [StringLength(20)]
        public string Theme { get; set; } = "light";

        public bool TwoFactorAuthEnabled { get; set; }

        [StringLength(20)]
        public string TwoFactorAuthType { get; set; } = "sms";

        public DateTime? LastLoginDate { get; set; }

        public string DeviceInfo { get; set; }

        public bool NotificationsEnabled { get; set; } = true;

        public bool MarketingCommunicationsEnabled { get; set; } = true;

        public int FailedLoginAttempts { get; set; } = 0;

        public bool IsLocked { get; set; } = false;

        public DateTime? LockoutEnd { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public DashboardPreferences DashboardPreferences { get; set; }
        public ICollection<ClientSession> ClientSessions { get; set; } = new List<ClientSession>();
        public ICollection<ClientDevice> ClientDevices { get; set; } = new List<ClientDevice>();
    }

    public class DashboardPreferences
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ClientPortalProfileId { get; set; }

        [ForeignKey("ClientPortalProfileId")]
        public ClientPortalProfile ClientPortalProfile { get; set; }

        public bool ShowAccountBalances { get; set; } = true;
        public bool ShowRecentTransactions { get; set; } = true;
        public bool ShowUpcomingPayments { get; set; } = true;
        public bool ShowLoanStatus { get; set; } = true;
        public bool ShowSavingsGoals { get; set; } = true;
        public bool ShowQuickActions { get; set; } = true;
        public bool ShowFinancialInsights { get; set; } = true;

        [StringLength(50)]
        public string Layout { get; set; } = "default";

        [StringLength(500)]
        public string VisibleWidgets { get; set; } = "accounts,transactions,loans,payments,goals";

        [StringLength(500)]
        public string WidgetOrder { get; set; } = "accounts,transactions,loans,payments,goals";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public class ClientSession
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ClientPortalProfileId { get; set; }

        [ForeignKey("ClientPortalProfileId")]
        public ClientPortalProfile ClientPortalProfile { get; set; }

        [Required]
        [StringLength(100)]
        public string SessionToken { get; set; }

        [StringLength(50)]
        public string IpAddress { get; set; }

        [StringLength(200)]
        public string UserAgent { get; set; }

        [StringLength(100)]
        public string DeviceId { get; set; }

        [StringLength(100)]
        public string Location { get; set; }

        public DateTime LoginTime { get; set; } = DateTime.UtcNow;
        public DateTime? LogoutTime { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime ExpiresAt { get; set; }
    }

    public class ClientDevice
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ClientPortalProfileId { get; set; }

        [ForeignKey("ClientPortalProfileId")]
        public ClientPortalProfile ClientPortalProfile { get; set; }

        [Required]
        [StringLength(100)]
        public string DeviceId { get; set; }

        [StringLength(100)]
        public string DeviceName { get; set; }

        [StringLength(50)]
        public string DeviceType { get; set; }

        [StringLength(50)]
        public string OperatingSystem { get; set; }

        [StringLength(100)]
        public string Browser { get; set; }

        [StringLength(50)]
        public string IpAddress { get; set; }

        [StringLength(100)]
        public string Location { get; set; }

        public bool IsTrusted { get; set; } = false;
        public DateTime LastUsed { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}