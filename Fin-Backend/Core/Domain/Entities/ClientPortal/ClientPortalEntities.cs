using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Entities.Customers;
using FinTech.Core.Domain.Entities.Identity;

namespace FinTech.Core.Domain.Entities.ClientPortal
{
    /// <summary>
    /// Represents a client portal user profile with additional settings beyond the base user
    /// </summary>
    public class ClientPortalProfile : BaseEntity
    {
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }
        
        public string PreferredLanguage { get; set; } = "en";
        public string TimeZone { get; set; } = "UTC";
        public bool DarkModeEnabled { get; set; } = false;
        public bool NewsletterSubscribed { get; set; } = true;
        public bool MarketingNotificationsEnabled { get; set; } = true;
        public string PushNotificationToken { get; set; }
        
        public DateTime? LastLoginDate { get; set; }
        public int LoginCount { get; set; }
        public DateTime? OnboardingCompletedDate { get; set; }
        
        public NotificationPreferences NotificationPreferences { get; set; }
        public DashboardPreferences DashboardPreferences { get; set; }
        
        public ICollection<SavedPayee> SavedPayees { get; set; } = new List<SavedPayee>();
        public ICollection<SavedTransferTemplate> SavedTransferTemplates { get; set; } = new List<SavedTransferTemplate>();
        public ICollection<ClientPortalSession> Sessions { get; set; } = new List<ClientPortalSession>();
        public ICollection<ClientDocument> Documents { get; set; } = new List<ClientDocument>();
        public ICollection<ClientSupportTicket> SupportTickets { get; set; } = new List<ClientSupportTicket>();
        public ICollection<SavingsGoal> SavingsGoals { get; set; } = new List<SavingsGoal>();
    }
    
    /// <summary>
    /// Client notification preferences for various types of alerts
    /// </summary>
    public class NotificationPreferences : BaseEntity
    {
        public Guid ClientPortalProfileId { get; set; }
        public ClientPortalProfile ClientPortalProfile { get; set; }
        
        public bool TransactionAlertsEnabled { get; set; } = true;
        public decimal TransactionAlertThreshold { get; set; } = 1000; // Alert for transactions above this amount
        
        public bool BalanceAlertsEnabled { get; set; } = true;
        public decimal LowBalanceThreshold { get; set; } = 100; // Alert when balance falls below this
        
        public bool LoanPaymentRemindersEnabled { get; set; } = true;
        public int DaysBeforeLoanPaymentDue { get; set; } = 3; // Days before due date to send reminder
        
        public bool BillPaymentRemindersEnabled { get; set; } = true;
        public int DaysBeforeBillPaymentDue { get; set; } = 3; // Days before due date to send reminder
        
        public bool SecurityAlertsEnabled { get; set; } = true;
        public bool ProductOffersEnabled { get; set; } = true;
        public bool AccountStatusChangesEnabled { get; set; } = true;
        
        public bool EmailNotificationsEnabled { get; set; } = true;
        public bool SmsNotificationsEnabled { get; set; } = true;
        public bool PushNotificationsEnabled { get; set; } = true;
        public bool InAppNotificationsEnabled { get; set; } = true;
    }
    
    /// <summary>
    /// Dashboard customization preferences for the client portal
    /// </summary>
    public class DashboardPreferences : BaseEntity
    {
        public Guid ClientPortalProfileId { get; set; }
        public ClientPortalProfile ClientPortalProfile { get; set; }
        
        public bool ShowAccountBalances { get; set; } = true;
        public bool ShowRecentTransactions { get; set; } = true;
        public bool ShowUpcomingPayments { get; set; } = true;
        public bool ShowLoanStatus { get; set; } = true;
        public bool ShowSavingsGoals { get; set; } = true;
        public bool ShowQuickActions { get; set; } = true;
        public bool ShowFinancialInsights { get; set; } = true;
        
        public string Layout { get; set; } = "default"; // default, compact, detailed
        public string[] VisibleWidgets { get; set; } = new[] { "accountSummary", "recentTransactions", "upcomingPayments", "quickActions" };
        public string[] WidgetOrder { get; set; } = new[] { "accountSummary", "recentTransactions", "upcomingPayments", "quickActions" };
    }
    
    /// <summary>
    /// Represents a client session in the portal for analytics and security monitoring
    /// </summary>
    public class ClientPortalSession : BaseEntity
    {
        public Guid ClientPortalProfileId { get; set; }
        public ClientPortalProfile ClientPortalProfile { get; set; }
        
        public string SessionId { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string DeviceType { get; set; } // mobile, tablet, desktop
        public string Browser { get; set; }
        public string OperatingSystem { get; set; }
        public string Location { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsSuccessful { get; set; } = true;
        public string FailureReason { get; set; }
        
        public ICollection<ClientPortalActivity> Activities { get; set; } = new List<ClientPortalActivity>();
    }
    
    /// <summary>
    /// Represents a user activity within a session for analytics and audit
    /// </summary>
    public class ClientPortalActivity : BaseEntity
    {
        public Guid ClientPortalSessionId { get; set; }
        public ClientPortalSession ClientPortalSession { get; set; }
        
        public string ActivityType { get; set; } // pageView, transaction, accountAccess, etc.
        public string Page { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    /// <summary>
    /// Represents a saved recipient for payments and transfers
    /// </summary>
    public class SavedPayee : BaseEntity
    {
        public Guid ClientPortalProfileId { get; set; }
        public ClientPortalProfile ClientPortalProfile { get; set; }
        
        public string PayeeName { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string PayeeType { get; set; } // person, business, utility
        public string Reference { get; set; }
        public string Category { get; set; } // family, friend, utility, etc.
        public bool IsFavorite { get; set; } = false;
        public int TransactionCount { get; set; } = 0;
        public DateTime? LastUsed { get; set; }
    }
    
    /// <summary>
    /// Represents a saved transfer template for quick transactions
    /// </summary>
    public class SavedTransferTemplate : BaseEntity
    {
        public Guid ClientPortalProfileId { get; set; }
        public ClientPortalProfile ClientPortalProfile { get; set; }
        
        public string TemplateName { get; set; }
        public string FromAccountNumber { get; set; }
        public string ToAccountNumber { get; set; }
        public string ToBankName { get; set; }
        public string ToBankCode { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "NGN";
        public string Reference { get; set; }
        public string Category { get; set; }
        public bool IsFavorite { get; set; } = false;
        public int UsageCount { get; set; } = 0;
        public DateTime? LastUsed { get; set; }
    }
    
    /// <summary>
    /// Represents a document uploaded or shared with the client
    /// </summary>
    public class ClientDocument : BaseEntity
    {
        public Guid ClientPortalProfileId { get; set; }
        public ClientPortalProfile ClientPortalProfile { get; set; }
        
        public string DocumentType { get; set; } // statement, receipt, contract, etc.
        public string DocumentName { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; } // in bytes
        public string StorageProvider { get; set; } // local, azure, aws, etc.
        public string StorageReference { get; set; }
        public bool IsSharedByBank { get; set; } = false; // true if bank shared with client
        public DateTime? ExpiryDate { get; set; }
        public bool RequiresSignature { get; set; } = false;
        public bool IsSigned { get; set; } = false;
        public DateTime? SignedDate { get; set; }
    }
    
    /// <summary>
    /// Represents a support ticket created by the client
    /// </summary>
    public class ClientSupportTicket : BaseEntity
    {
        public Guid ClientPortalProfileId { get; set; }
        public ClientPortalProfile ClientPortalProfile { get; set; }
        
        public string TicketNumber { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Category { get; set; } // account, transaction, loan, technical, etc.
        public string Priority { get; set; } // low, medium, high, urgent
        public string Status { get; set; } // open, in-progress, resolved, closed
        public Guid? AssignedToId { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string Resolution { get; set; }
        public int CustomerSatisfactionRating { get; set; } // 1-5 stars
        
        public ICollection<ClientSupportMessage> Messages { get; set; } = new List<ClientSupportMessage>();
        public ICollection<ClientSupportAttachment> Attachments { get; set; } = new List<ClientSupportAttachment>();
    }
    
    /// <summary>
    /// Represents a message in a support ticket conversation
    /// </summary>
    public class ClientSupportMessage : BaseEntity
    {
        public Guid ClientSupportTicketId { get; set; }
        public ClientSupportTicket ClientSupportTicket { get; set; }
        
        public string Message { get; set; }
        public bool IsFromClient { get; set; } // true if from client, false if from support
        public Guid? SentById { get; set; } // user ID of the sender
        public string SenderName { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
    }
    
    /// <summary>
    /// Represents an attachment to a support ticket
    /// </summary>
    public class ClientSupportAttachment : BaseEntity
    {
        public Guid ClientSupportTicketId { get; set; }
        public ClientSupportTicket ClientSupportTicket { get; set; }
        
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; } // in bytes
        public bool IsFromClient { get; set; } // true if uploaded by client
    }
    
    /// <summary>
    /// Represents a savings goal set by the client
    /// </summary>
    public class SavingsGoal : BaseEntity
    {
        public Guid ClientPortalProfileId { get; set; }
        public ClientPortalProfile ClientPortalProfile { get; set; }
        
        public string GoalName { get; set; }
        public string Description { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; } = 0;
        public string Currency { get; set; } = "NGN";
        public DateTime StartDate { get; set; }
        public DateTime TargetDate { get; set; }
        public string GoalCategory { get; set; } // vacation, education, home, car, etc.
        public string Status { get; set; } // active, completed, cancelled
        public string RecurrencePattern { get; set; } // none, daily, weekly, monthly
        public decimal AutoTransferAmount { get; set; } = 0;
        public string SourceAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
        
        public ICollection<SavingsGoalTransaction> Transactions { get; set; } = new List<SavingsGoalTransaction>();
    }
    
    /// <summary>
    /// Represents a transaction related to a savings goal
    /// </summary>
    public class SavingsGoalTransaction : BaseEntity
    {
        public Guid SavingsGoalId { get; set; }
        public SavingsGoal SavingsGoal { get; set; }
        
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "NGN";
        public string TransactionType { get; set; } // deposit, withdrawal
        public string SourceAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
        public string Reference { get; set; }
        public string Status { get; set; } // completed, pending, failed
        public bool IsAutomatic { get; set; } = false;
    }
    
    /// <summary>
    /// Represents a notification sent to the client
    /// </summary>
    public class ClientNotification : BaseEntity
    {
        public Guid ClientPortalProfileId { get; set; }
        public ClientPortalProfile ClientPortalProfile { get; set; }
        
        public string NotificationType { get; set; } // transaction, security, marketing, etc.
        public string Title { get; set; }
        public string Message { get; set; }
        public string Action { get; set; } // URL or action to take when notification is clicked
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        public string DeliveryChannels { get; set; } // comma-separated: email,sms,push,inApp
        public string DeliveryStatus { get; set; } // delivered, failed, pending
        public DateTime? ExpiryDate { get; set; }
        public bool IsActionable { get; set; } = false;
        public bool IsDismissed { get; set; } = false;
        public int Priority { get; set; } = 1; // 1 (low) to 5 (critical)
    }
}
