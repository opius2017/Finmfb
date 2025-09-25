using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    // Profile DTOs
    public class UpdateProfileDto
    {
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public string Language { get; set; }
        public string Theme { get; set; } // Light, Dark
        public string DateFormat { get; set; } // dd/MM/yyyy, MM/dd/yyyy, yyyy-MM-dd
        public string TimeFormat { get; set; } // 12-hour, 24-hour
    }

    public class ProfilePictureDto
    {
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
    }

    // Notification Preferences DTOs
    public class NotificationPreferencesDto
    {
        [Required]
        public bool EmailNotificationsEnabled { get; set; }
        
        [Required]
        public bool SmsNotificationsEnabled { get; set; }
        
        [Required]
        public bool PushNotificationsEnabled { get; set; }
        
        [Required]
        public bool AccountActivityAlerts { get; set; }
        
        [Required]
        public bool TransactionAlerts { get; set; }
        
        [Required]
        public bool LoanPaymentReminders { get; set; }
        
        [Required]
        public bool SecurityAlerts { get; set; }
        
        [Required]
        public bool MarketingCommunications { get; set; }
        
        [Required]
        public bool LowBalanceAlerts { get; set; }
        
        public decimal? LowBalanceThreshold { get; set; }
    }

    // Dashboard Preferences DTOs
    public class DashboardPreferencesDto
    {
        public string DefaultDashboard { get; set; } // Overview, Accounts, Loans, etc.
        
        [Required]
        public bool ShowAccountBalances { get; set; }
        
        [Required]
        public bool ShowRecentTransactions { get; set; }
        
        [Required]
        public bool ShowUpcomingPayments { get; set; }
        
        [Required]
        public bool ShowLoanOverview { get; set; }
        
        [Required]
        public bool ShowSpendingAnalysis { get; set; }
        
        [Required]
        public bool ShowQuickTransfer { get; set; }
        
        [Required]
        public bool ShowSavingsGoals { get; set; }
        
        public string CustomWidgets { get; set; } // JSON array of custom widgets
        
        public string WidgetLayout { get; set; } // JSON layout configuration
    }

    // Security Preferences DTOs
    public class SecurityPreferencesDto
    {
        [Required]
        public bool TwoFactorEnabled { get; set; }
        
        [Required]
        public bool RememberDevice { get; set; }
        
        [Required]
        public bool LoginNotifications { get; set; }
        
        [Required]
        public bool TransactionSigningRequired { get; set; }
        
        [Required]
        public bool AllowMultipleDevices { get; set; }
        
        public int? AutoLockTimeout { get; set; } // In minutes
        
        // For tracking security activity, populated by the controller
        public string IpAddress { get; set; }
        
        public string UserAgent { get; set; }
    }

    // Document Management DTOs
    public class DocumentUploadDto
    {
        [Required]
        public string DocumentType { get; set; } // ID, Address Proof, Income Proof, etc.
        
        [Required]
        public string DocumentName { get; set; }
        
        [Required]
        public string FileName { get; set; }
        
        [Required]
        public long FileSize { get; set; }
        
        [Required]
        public byte[] FileContent { get; set; }
        
        public string Description { get; set; }
    }

    // Activity History DTOs
    public class ActivityHistoryRequestDto
    {
        public DateTime? FromDate { get; set; }
        
        public DateTime? ToDate { get; set; }
        
        public string ActivityType { get; set; } // Login, Profile Update, Transaction, etc.
        
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 20;
    }
}
