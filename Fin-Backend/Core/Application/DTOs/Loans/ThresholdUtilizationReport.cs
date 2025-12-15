using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class ThresholdUtilizationReport
    {
        public int Year { get; set; }
        public int? Month { get; set; }
        public decimal TotalThresholdAmount { get; set; }
        public decimal TotalAllocatedAmount { get; set; }
        public decimal TotalRemainingAmount { get; set; }
        public int TotalLoansRegistered { get; set; }
        public int TotalApplicationsQueued { get; set; }
        public decimal AverageUtilizationPercentage { get; set; }
        public List<MonthlyUtilization> MonthlyBreakdown { get; set; } = new List<MonthlyUtilization>();
    }

    public class MonthlyUtilization
    {
        public int Month { get; set; }
        public string MonthName { get; set; }
        public decimal MaximumAmount { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal UtilizationPercentage { get; set; }
        public int LoansRegistered { get; set; }
        public int ApplicationsQueued { get; set; }
        public string Status { get; set; }
    }
}
