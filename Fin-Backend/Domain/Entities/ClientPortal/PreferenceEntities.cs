using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Domain.Entities.ClientPortal
{
    /// <summary>
    /// Represents user notification preferences
    /// </summary>
    public class NotificationPreferences : BaseEntity, IAuditable
    {
        [Required]
        public Guid UserId { get; set; }
        
        // Email preferences
        public bool EmailEnabled { get; set; } = true;
        public bool EmailTransactionAlerts { get; set; } = true;
        public bool EmailAccountStatements { get; set; } = true;
        public bool EmailSecurityAlerts { get; set; } = true;
        public bool EmailPromotionalOffers { get; set; }
        public bool EmailMaintenanceNotices { get; set; } = true;
        
        // SMS preferences
        public bool SmsEnabled { get; set; }
        public bool SmsTransactionAlerts { get; set; }
        public bool SmsSecurityAlerts { get; set; } = true;
        public bool SmsLowBalanceAlerts { get; set; }
        public bool SmsPaymentReminders { get; set; }
        
        // Push notification preferences
        public bool PushEnabled { get; set; }
        public bool PushTransactionAlerts { get; set; }
        public bool PushSecurityAlerts { get; set; } = true;
        public bool PushAccountUpdates { get; set; }
        
        // Thresholds
        public decimal? LowBalanceThreshold { get; set; }
        public decimal? HighTransactionThreshold { get; set; }
        
        // Frequency settings
        [MaxLength(50)]
        public string? DigestFrequency { get; set; } = "Daily"; // Daily, Weekly, Monthly, None
        
        [MaxLength(10)]
        public string? PreferredTimeZone { get; set; }
        
        [MaxLength(5)]
        public string? PreferredLanguage { get; set; } = "en";
        
        public bool DoNotDisturb { get; set; }
        public TimeSpan? DoNotDisturbStart { get; set; }
        public TimeSpan? DoNotDisturbEnd { get; set; }
    }

    /// <summary>
    /// Represents user dashboard preferences
    /// </summary>
    public class DashboardPreferences : BaseEntity, IAuditable
    {
        [Required]
        public Guid UserId { get; set; }
        
        [MaxLength(50)]
        public string Theme { get; set; } = "Default"; // Default, Dark, Light
        
        [MaxLength(10)]
        public string Language { get; set; } = "en";
        
        [MaxLength(20)]
        public string Currency { get; set; } = "USD";
        
        [MaxLength(50)]
        public string DateFormat { get; set; } = "MM/dd/yyyy";
        
        [MaxLength(50)]
        public string TimeFormat { get; set; } = "12-hour";
        
        public int DefaultPageSize { get; set; } = 25;
        
        public bool ShowAccountBalances { get; set; } = true;
        public bool ShowRecentTransactions { get; set; } = true;
        public bool ShowUpcomingPayments { get; set; } = true;
        public bool ShowBudgetOverview { get; set; } = true;
        public bool ShowGoalsProgress { get; set; } = true;
        public bool ShowQuickActions { get; set; } = true;
        
        [MaxLength(1000)]
        public string? DashboardLayout { get; set; } // JSON string for widget positions
        
        [MaxLength(500)]
        public string? DefaultAccount { get; set; } // Default account for quick actions
        
        public bool AutoRefresh { get; set; } = true;
        public int RefreshInterval { get; set; } = 300; // seconds
        
        public bool EnableAnimations { get; set; } = true;
        public bool CompactView { get; set; }
        
        [MaxLength(1000)]
        public string? HiddenWidgets { get; set; } // JSON array of hidden widget IDs
        
        [MaxLength(1000)]
        public string? CustomSettings { get; set; } // JSON for additional custom settings
    }
}