using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
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
}
