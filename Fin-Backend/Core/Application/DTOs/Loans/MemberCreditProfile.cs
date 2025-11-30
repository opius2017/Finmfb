using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    /// <summary>
    /// Comprehensive member credit profile for committee review
    /// </summary>
    public class MemberCreditProfile
    {
        public string MemberId { get; set; }
        public string MemberNumber { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        
        public DateTime MembershipDate { get; set; }
        public int MembershipDurationMonths { get; set; }
        public string MembershipStatus { get; set; }
        
        // Financial Summary
        public decimal TotalSavings { get; set; }
        public decimal ShareCapital { get; set; }
        public decimal TotalLoans { get; set; }
        public decimal OutstandingLoanBalance { get; set; }
        public int CreditScore { get; set; }
        
        // Calculated Metrics
        public int RepaymentScore { get; set; }
        public decimal SavingsConsistency { get; set; }
        public int GuarantorObligations { get; set; }
        public decimal DebtToSavingsRatio { get; set; }
        
        // Loan History
        public List<LoanHistoryItem> LoanHistory { get; set; } = new List<LoanHistoryItem>();
    }
}
