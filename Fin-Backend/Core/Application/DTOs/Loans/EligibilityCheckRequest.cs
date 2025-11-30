using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    /// <summary>
    /// Request for eligibility check
    /// </summary>
    public class EligibilityCheckRequest
    {
        // Loan Details
        public decimal RequestedAmount { get; set; }
        public string LoanType { get; set; } // NORMAL, COMMODITY, CAR
        public decimal InterestRate { get; set; }
        public int TenorMonths { get; set; }

        // Member Details
        public string MemberId { get; set; }
        public decimal MemberTotalSavings { get; set; }
        public DateTime MembershipDate { get; set; }
        public int? CreditScore { get; set; }

        // Salary Details (for salaried workers)
        public decimal? NetSalary { get; set; }
        public decimal ExistingMonthlyDeductions { get; set; }

        // Existing Loans
        public int ActiveLoansCount { get; set; }
        public decimal TotalOutstandingBalance { get; set; }

        // Configuration Parameters
        public int MinimumMembershipMonths { get; set; } = 6;
        public decimal MaxDeductionRatePercentage { get; set; } = 45m;
        public decimal MaxDebtToIncomeRatio { get; set; } = 50m;
        public int MaxActiveLoans { get; set; } = 3;
        public decimal MaxTotalOutstanding { get; set; } = 5000000m;
        public int MinimumCreditScore { get; set; } = 500;
    }
}
