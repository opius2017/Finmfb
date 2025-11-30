using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    /// <summary>
    /// Request for loan eligibility check
    /// </summary>
    public class LoanEligibilityRequest
    {
        public string MemberId { get; set; } = string.Empty;
        public string LoanProductId { get; set; } = string.Empty;
        public decimal RequestedAmount { get; set; }
        public int TenureMonths { get; set; }
    }

    /// <summary>
    /// Complete eligibility result
    /// </summary>
    public class LoanEligibilityResultDto
    {
        public string MemberId { get; set; } = string.Empty;
        public string MemberNumber { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public string LoanProductId { get; set; } = string.Empty;
        public string LoanProductName { get; set; } = string.Empty;
        public decimal RequestedAmount { get; set; }
        public int TenureMonths { get; set; }
        public bool IsEligible { get; set; }
        public decimal MaximumEligibleAmount { get; set; }
        public List<string> EligibilityCriteria { get; set; } = new List<string>();
        public List<string> FailureReasons { get; set; } = new List<string>();
        public SavingsMultiplierCheckDto SavingsMultiplierCheck { get; set; } = new SavingsMultiplierCheckDto();
        public MembershipDurationCheckDto MembershipDurationCheck { get; set; } = new MembershipDurationCheckDto();
        public DeductionRateHeadroomDto DeductionRateHeadroom { get; set; } = new DeductionRateHeadroomDto();
        public DebtToIncomeRatioDto DebtToIncomeRatio { get; set; } = new DebtToIncomeRatioDto();
        public DateTime CheckedAt { get; set; }
    }

    /// <summary>
    /// Savings multiplier check result
    /// </summary>
    public class SavingsMultiplierCheckDto
    {
        public decimal TotalSavings { get; set; }
        public decimal FreeEquity { get; set; }
        public decimal LockedEquity { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal RequiredSavings { get; set; }
        public decimal SavingsMultiplier { get; set; }
        public decimal ActualMultiplier { get; set; }
        public bool Passed { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Membership duration check result
    /// </summary>
    public class MembershipDurationCheckDto
    {
        public DateTime MembershipStartDate { get; set; }
        public int MembershipMonths { get; set; }
        public int RequiredMonths { get; set; }
        public bool Passed { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Deduction rate headroom result
    /// </summary>
    public class DeductionRateHeadroomDto
    {
        public decimal MonthlySalary { get; set; }
        public decimal CurrentMonthlyDeductions { get; set; }
        public decimal ProposedMonthlyDeduction { get; set; }
        public decimal TotalMonthlyDeductions { get; set; }
        public decimal CurrentDeductionRate { get; set; }
        public decimal ProposedDeductionRate { get; set; }
        public decimal MaximumAllowedRate { get; set; }
        public decimal AvailableHeadroom { get; set; }
        public bool Passed { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Debt-to-income ratio result
    /// </summary>
    public class DebtToIncomeRatioDto
    {
        public decimal MonthlySalary { get; set; }
        public decimal CurrentMonthlyDebtPayments { get; set; }
        public decimal ProposedMonthlyPayment { get; set; }
        public decimal TotalMonthlyDebtPayments { get; set; }
        public decimal DebtToIncomeRatio { get; set; }
        public decimal MaximumAllowedRatio { get; set; }
        public bool Passed { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Comprehensive eligibility report
    /// </summary>
    public class EligibilityReportDto
    {
        public string MemberId { get; set; } = string.Empty;
        public string MemberNumber { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public string LoanProductName { get; set; } = string.Empty;
        public decimal MaximumEligibleAmount { get; set; }
        public bool IsEligible { get; set; }
        public MemberFinancialSummary FinancialSummary { get; set; } = new MemberFinancialSummary();
        public List<EligibilityCriterion> Criteria { get; set; } = new List<EligibilityCriterion>();
        public List<string> Recommendations { get; set; } = new List<string>();
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Member financial summary
    /// </summary>
    public class MemberFinancialSummary
    {
        public decimal TotalSavings { get; set; }
        public decimal FreeEquity { get; set; }
        public decimal LockedEquity { get; set; }
        public decimal MonthlySalary { get; set; }
        public int ActiveLoansCount { get; set; }
        public decimal TotalOutstandingBalance { get; set; }
        public decimal CurrentMonthlyDeductions { get; set; }
        public int MembershipMonths { get; set; }
        public string RepaymentScore { get; set; } = string.Empty;
    }

    /// <summary>
    /// Individual eligibility criterion
    /// </summary>
    public class EligibilityCriterion
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Maximum eligible amount calculation
    /// </summary>
    public class MaximumEligibleAmountDto
    {
        public decimal MaximumAmount { get; set; }
        public string LimitingFactor { get; set; } = string.Empty;
        public decimal SavingsBasedLimit { get; set; }
        public decimal IncomeBasedLimit { get; set; }
        public decimal DeductionRateBasedLimit { get; set; }
        public List<string> Constraints { get; set; } = new List<string>();
    }
}
