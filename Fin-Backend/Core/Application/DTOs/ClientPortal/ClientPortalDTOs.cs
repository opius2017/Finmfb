using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    // Profile DTOs
    public class ClientPortalProfileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CustomerId { get; set; }
        public string PreferredLanguage { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
        public bool DarkModeEnabled { get; set; }
        public bool NewsletterSubscribed { get; set; }
        public bool MarketingNotificationsEnabled { get; set; }
        public string PushNotificationToken { get; set; } = string.Empty;
        public DateTime? LastLoginDate { get; set; }
        public int LoginCount { get; set; }
        public DateTime? OnboardingCompletedDate { get; set; }
        public NotificationPreferencesDto NotificationPreferences { get; set; } = new();
        public DashboardPreferencesDto DashboardPreferences { get; set; } = new();
        public DateTime CreatedOn { get; set; }
        public DateTime LastModifiedOn { get; set; }
    }

    public class UpdateClientPortalProfileDto
    {
        public string PreferredLanguage { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
        public bool? DarkModeEnabled { get; set; }
        public bool? NewsletterSubscribed { get; set; }
        public bool? MarketingNotificationsEnabled { get; set; }
        public string PushNotificationToken { get; set; } = string.Empty;
        public NotificationPreferencesUpdateDto NotificationPreferences { get; set; } = new();
        public DashboardPreferencesUpdateDto DashboardPreferences { get; set; } = new();
    }

    // Notification Preferences DTOs
    public class NotificationPreferencesDto
    {
        public Guid Id { get; set; }
        public bool TransactionAlertsEnabled { get; set; }
        public decimal TransactionAlertThreshold { get; set; }
        public bool BalanceAlertsEnabled { get; set; }
        public decimal LowBalanceThreshold { get; set; }
        public bool LoanPaymentRemindersEnabled { get; set; }
        public int DaysBeforeLoanPaymentDue { get; set; }
        public bool BillPaymentRemindersEnabled { get; set; }
        public int DaysBeforeBillPaymentDue { get; set; }
        public bool SecurityAlertsEnabled { get; set; }
        public bool ProductOffersEnabled { get; set; }
        public bool AccountStatusChangesEnabled { get; set; }
        public bool EmailNotificationsEnabled { get; set; }
        public bool SmsNotificationsEnabled { get; set; }
        public bool PushNotificationsEnabled { get; set; }
        public bool InAppNotificationsEnabled { get; set; }
    }

    public class NotificationPreferencesUpdateDto
    {
        public bool? TransactionAlertsEnabled { get; set; }
        public decimal? TransactionAlertThreshold { get; set; }
        public bool? BalanceAlertsEnabled { get; set; }
        public decimal? LowBalanceThreshold { get; set; }
        public bool? LoanPaymentRemindersEnabled { get; set; }
        public int? DaysBeforeLoanPaymentDue { get; set; }
        public bool? BillPaymentRemindersEnabled { get; set; }
        public int? DaysBeforeBillPaymentDue { get; set; }
        public bool? SecurityAlertsEnabled { get; set; }
        public bool? ProductOffersEnabled { get; set; }
        public bool? AccountStatusChangesEnabled { get; set; }
        public bool? EmailNotificationsEnabled { get; set; }
        public bool? SmsNotificationsEnabled { get; set; }
        public bool? PushNotificationsEnabled { get; set; }
        public bool? InAppNotificationsEnabled { get; set; }
    }

    // Dashboard Preferences DTOs
    public class DashboardPreferencesDto
    {
        public Guid Id { get; set; }
        public bool ShowAccountBalances { get; set; }
        public bool ShowRecentTransactions { get; set; }
        public bool ShowUpcomingPayments { get; set; }
        public bool ShowLoanStatus { get; set; }
        public bool ShowSavingsGoals { get; set; }
        public bool ShowQuickActions { get; set; }
        public bool ShowFinancialInsights { get; set; }
        public string Layout { get; set; } = string.Empty;
        public string[] VisibleWidgets { get; set; } = Array.Empty<string>();
        public string[] WidgetOrder { get; set; } = Array.Empty<string>();
    }

    public class DashboardPreferencesUpdateDto
    {
        public bool? ShowAccountBalances { get; set; }
        public bool? ShowRecentTransactions { get; set; }
        public bool? ShowUpcomingPayments { get; set; }
        public bool? ShowLoanStatus { get; set; }
        public bool? ShowSavingsGoals { get; set; }
        public bool? ShowQuickActions { get; set; }
        public bool? ShowFinancialInsights { get; set; }
        public string Layout { get; set; }
        public string[] VisibleWidgets { get; set; }
        public string[] WidgetOrder { get; set; }
    }

    // Session DTOs
    public class ClientPortalSessionDto
    {
        public Guid Id { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string Browser { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsSuccessful { get; set; }
        public string FailureReason { get; set; } = string.Empty;
    }

    public class ClientPortalActivityDto
    {
        public Guid Id { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string Page { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    // Document DTOs
    public class ClientDocumentDto
    {
        public Guid Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public bool IsSharedByBank { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool RequiresSignature { get; set; }
        public bool IsSigned { get; set; }
        public DateTime? SignedDate { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class UploadDocumentDto
    {
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }

    // Support Ticket DTOs
    public class ClientSupportTicketDto
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? ClosedDate { get; set; }
        public string Resolution { get; set; } = string.Empty;
        public int CustomerSatisfactionRating { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<ClientSupportMessageDto> Messages { get; set; } = new();
    }

    public class CreateSupportTicketDto
    {
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string InitialMessage { get; set; } = string.Empty;
    }

    public class ClientSupportMessageDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsFromClient { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class AddSupportMessageDto
    {
        public string Message { get; set; } = string.Empty;
    }

    // Savings Goal DTOs
    public class SavingsGoalDto
    {
        public Guid Id { get; set; }
        public string GoalName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime TargetDate { get; set; }
        public string GoalCategory { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string RecurrencePattern { get; set; } = string.Empty;
        public decimal AutoTransferAmount { get; set; }
        public string SourceAccountNumber { get; set; } = string.Empty;
        public string DestinationAccountNumber { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public List<SavingsGoalTransactionDto> Transactions { get; set; } = new();
    }

    public class CreateSavingsGoalDto
    {
        public string GoalName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public string Currency { get; set; } = "NGN";
        public DateTime StartDate { get; set; }
        public DateTime TargetDate { get; set; }
        public string GoalCategory { get; set; } = string.Empty;
        public string RecurrencePattern { get; set; } = string.Empty;
        public decimal AutoTransferAmount { get; set; }
        public string SourceAccountNumber { get; set; } = string.Empty;
        public string DestinationAccountNumber { get; set; } = string.Empty;
    }

    public class SavingsGoalTransactionDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public string SourceAccountNumber { get; set; } = string.Empty;
        public string DestinationAccountNumber { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsAutomatic { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    // Payee DTOs
    public class SavedPayeeDto
    {
        public Guid Id { get; set; }
        public string PayeeName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string PayeeType { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
        public int TransactionCount { get; set; }
        public DateTime? LastUsed { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class CreateSavedPayeeDto
    {
        public string PayeeName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string PayeeType { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
    }

    // Transfer Template DTOs
    public class SavedTransferTemplateDto
    {
        public Guid Id { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string FromAccountNumber { get; set; } = string.Empty;
        public string ToAccountNumber { get; set; } = string.Empty;
        public string ToBankName { get; set; } = string.Empty;
        public string ToBankCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
        public int UsageCount { get; set; }
        public DateTime? LastUsed { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class CreateSavedTransferTemplateDto
    {
        public string TemplateName { get; set; } = string.Empty;
        public string FromAccountNumber { get; set; } = string.Empty;
        public string ToAccountNumber { get; set; } = string.Empty;
        public string ToBankName { get; set; } = string.Empty;
        public string ToBankCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "NGN";
        public string Reference { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
    }

    // Notification DTOs
    public class ClientNotificationDto
    {
        public Guid Id { get; set; }
        public string NotificationType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public string DeliveryChannels { get; set; } = string.Empty;
        public string DeliveryStatus { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public bool IsActionable { get; set; }
        public bool IsDismissed { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
