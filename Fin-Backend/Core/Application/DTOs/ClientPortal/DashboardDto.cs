using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class ClientDashboardDto
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalBalance { get; set; }
        public int TotalAccounts { get; set; }
        public int TotalLoans { get; set; }
        public decimal TotalLoanBalance { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DashboardPreferencesDto Preferences { get; set; }
        public List<AccountBalanceDto> AccountBalances { get; set; }
        public List<TransactionSummaryDto> RecentTransactions { get; set; }
        public List<LoanAccountSummaryDto> LoanAccounts { get; set; }
        public List<UpcomingPaymentDto> UpcomingPayments { get; set; }
        public List<SavingsGoalSummaryDto> SavingsGoals { get; set; }
        public NotificationCountDto NotificationCounts { get; set; }
        public List<SupportTicketSummaryDto> RecentSupportTickets { get; set; }
    }

    public class TransactionSummaryDto
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string TransactionType { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
    }

    public class UpcomingPaymentDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime IncludedDate { get; set; }
        public DateTime NextPaymentDate { get; set; }
        public string Description { get; set; }
        public string PaymentName { get; set; }
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
        public double ProgressPercentage { get; set; }
        public int Progress { get; set; }
        public DateTime TargetDate { get; set; }
    }

    public class SupportTicketSummaryDto
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; }
        public string Subject { get; set; }
        public string Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
