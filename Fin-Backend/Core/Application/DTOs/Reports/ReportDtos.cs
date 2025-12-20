using System;

namespace FinTech.Core.Application.DTOs.Reports
{
    public class PortfolioReportDto
    {
        public decimal TotalPortfolioValue { get; set; }
        public int TotalActiveLoans { get; set; }
        public decimal PortfolioAtRisk { get; set; }
        public decimal Par30 { get; set; }
        public decimal Par60 { get; set; }
        public decimal Par90 { get; set; }
    }

    public class ReportRequestDto
    {
        public string ReportType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Format { get; set; } // PDF, Excel, etc.
    }

    public class DashboardMetricsDto
    {
        public int ActiveCustomers { get; set; }
        public int PendingApplications { get; set; }
        public decimal TotalDisbursed { get; set; }
    }
}
