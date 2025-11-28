using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.ClientPortal
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

    // AccountBalanceDto defined in AccountOverviewDTOs.cs (keep single canonical definition)

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

    // LoanAccountSummaryDto defined in LoanDTOs.cs (keep single canonical definition)

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

    // DashboardPreferencesDto / DashboardPreferencesUpdateDto defined in ClientPortalDTOs.cs (keep single canonical definition)

    public class NotificationCountsDto
    {
        public int TotalNotifications { get; set; }
        public int UnreadNotifications { get; set; }
        public Dictionary<string, int> CountByType { get; set; } = new Dictionary<string, int>();
    }
}
