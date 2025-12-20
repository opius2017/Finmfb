using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class ClientDashboardDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalBalance { get; set; }
        public int TotalAccounts { get; set; }
        public int TotalLoans { get; set; }
        public decimal TotalLoanBalance { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DashboardPreferencesDto Preferences { get; set; } = new();
        public List<AccountBalanceDto> AccountBalances { get; set; } = new();
        public List<TransactionSummaryDto> RecentTransactions { get; set; } = new();
        public List<LoanAccountSummaryDto> LoanAccounts { get; set; } = new();
        public List<UpcomingPaymentDto> UpcomingPayments { get; set; } = new();
        public List<SavingsGoalSummaryDto> SavingsGoals { get; set; } = new();
        public NotificationCountDto NotificationCounts { get; set; } = new();
        public List<SupportTicketSummaryDto> RecentSupportTickets { get; set; } = new();
    }

    public class TransactionSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class UpcomingPaymentDto
    {
        public string Id { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime IncludedDate { get; set; }
        public DateTime NextPaymentDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string PaymentName { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class SavingsGoalSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string GoalName { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public double ProgressPercentage { get; set; }
        public int Progress { get; set; }
        public DateTime TargetDate { get; set; }
    }

    public class SupportTicketSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string TicketNumber { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
