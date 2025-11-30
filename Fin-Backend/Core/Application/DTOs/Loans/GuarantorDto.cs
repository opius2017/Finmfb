using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class GuarantorDto
    {
        public string Id { get; set; } = string.Empty;
        public string LoanApplicationId { get; set; } = string.Empty;
        public string MemberId { get; set; } = string.Empty;
        public string MemberNumber { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public decimal GuaranteeAmount { get; set; }
        public string ConsentStatus { get; set; } = string.Empty; // PENDING, APPROVED, REJECTED
        public DateTime? ConsentDate { get; set; }
        public string? ConsentNotes { get; set; }
        public decimal LockedEquity { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AddGuarantorRequest
    {
        public string LoanApplicationId { get; set; } = string.Empty;
        public string MemberId { get; set; } = string.Empty;
        public decimal GuaranteeAmount { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
    }

    public class GuarantorEligibilityDto
    {
        public string MemberId { get; set; } = string.Empty;
        public string MemberNumber { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public decimal TotalSavings { get; set; }
        public decimal FreeEquity { get; set; }
        public decimal LockedEquity { get; set; }
        public decimal RequestedGuaranteeAmount { get; set; }
        public decimal AvailableForGuarantee { get; set; }
        public bool IsEligible { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Constraints { get; set; } = new List<string>();
    }

    public class ConsentRequestDto
    {
        public string Id { get; set; } = string.Empty;
        public string GuarantorId { get; set; } = string.Empty;
        public string LoanApplicationId { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public decimal LoanAmount { get; set; }
        public decimal GuaranteeAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string NotificationChannel { get; set; } = string.Empty;
    }

    public class ProcessConsentRequest
    {
        public string GuarantorId { get; set; } = string.Empty;
        public string ConsentStatus { get; set; } = string.Empty; // APPROVED, REJECTED
        public string? Notes { get; set; }
        public string ProcessedBy { get; set; } = string.Empty;
    }

    public class EquityLockDto
    {
        public string MemberId { get; set; } = string.Empty;
        public string LoanId { get; set; } = string.Empty;
        public decimal LockedAmount { get; set; }
        public decimal FreeEquityBefore { get; set; }
        public decimal FreeEquityAfter { get; set; }
        public DateTime LockedAt { get; set; }
        public DateTime? UnlockedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class GuarantorDashboardDto
    {
        public string MemberId { get; set; } = string.Empty;
        public string MemberNumber { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public decimal TotalSavings { get; set; }
        public decimal FreeEquity { get; set; }
        public decimal LockedEquity { get; set; }
        public int ActiveGuaranteesCount { get; set; }
        public decimal TotalGuaranteedAmount { get; set; }
        public int PendingConsentRequests { get; set; }
        public List<GuaranteedLoanDto> ActiveGuarantees { get; set; } = new List<GuaranteedLoanDto>();
        public List<ConsentRequestDto> PendingRequests { get; set; } = new List<ConsentRequestDto>();
    }

    public class GuaranteedLoanDto
    {
        public string LoanId { get; set; } = string.Empty;
        public string LoanNumber { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public string BorrowerMemberNumber { get; set; } = string.Empty;
        public decimal LoanAmount { get; set; }
        public decimal GuaranteeAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
        public string LoanStatus { get; set; } = string.Empty;
        public DateTime DisbursementDate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public string RepaymentStatus { get; set; } = string.Empty;
    }
}
