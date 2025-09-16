using System;
using System.Collections.Generic;

namespace FinTech.Application.DTOs.ClientPortal
{
    public class ClientDashboardDto
    {
        public string CustomerName { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DashboardPreferencesDto Preferences { get; set; }
        public List<AccountBalanceDto> AccountBalances { get; set; } = new List<AccountBalanceDto>();
        public List<TransactionSummaryDto> RecentTransactions { get; set; } = new List<TransactionSummaryDto>();
        public List<LoanAccountSummaryDto> LoanAccounts { get; set; } = new List<LoanAccountSummaryDto>();
        public List<UpcomingPaymentDto> UpcomingPayments { get; set; } = new List<UpcomingPaymentDto>();
        public List<SavingsGoalSummaryDto> SavingsGoals { get; set; } = new List<SavingsGoalSummaryDto>();
        public NotificationCountsDto NotificationCounts { get; set; }
        public List<SupportTicketSummaryDto> RecentSupportTickets { get; set; } = new List<SupportTicketSummaryDto>();
    }

    public class AccountBalanceDto
    {
        public Guid AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string Currency { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public string Status { get; set; }
    }

    public class TransactionSummaryDto
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string AccountNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string TransactionType { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
    }

    public class LoanAccountSummaryDto
    {
        public Guid Id { get; set; }
        public string LoanNumber { get; set; }
        public string LoanType { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal NextPaymentAmount { get; set; }
        public DateTime NextPaymentDate { get; set; }
        public string Status { get; set; }
    }

    public class UpcomingPaymentDto
    {
        public Guid Id { get; set; }
        public string PaymentName { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime NextPaymentDate { get; set; }
        public string PaymentType { get; set; }
        public string Status { get; set; }
    }

    public class SavingsGoalSummaryDto
    {
        public Guid Id { get; set; }
        public string GoalName { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public string Currency { get; set; }
        public DateTime TargetDate { get; set; }
        public int Progress { get; set; }
    }

    public class SupportTicketSummaryDto
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; }
        public string Subject { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }

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
        public string Layout { get; set; }
        public string[] VisibleWidgets { get; set; }
        public string[] WidgetOrder { get; set; }
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

    public class NotificationCountsDto
    {
        public int TotalNotifications { get; set; }
        public int UnreadNotifications { get; set; }
        public Dictionary<string, int> CountByType { get; set; } = new Dictionary<string, int>();
    }
}