using System;
using System.Collections.Generic;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a credit check performed for a loan application
    /// </summary>
    public class LoanCreditCheck : AuditableEntity
    {
    public new string? Id { get; set; }
    public string? LoanApplicationId { get; set; }
    public string? CustomerId { get; set; }
        public DateTime CheckDate { get; set; }
    public string? CreditBureau { get; set; }
    public string? ReferenceNumber { get; set; }
    public decimal? CreditScore { get; set; }
    public string? CreditRating { get; set; }
    public decimal? DebtToIncomeRatio { get; set; }
    public int? NumberOfDelinquencies { get; set; }
    public int? NumberOfEnquiries { get; set; }
    public decimal? TotalDebt { get; set; }
    public string? ReportSummary { get; set; }
    public string? ReportFilePath { get; set; }
    public string? PerformedBy { get; set; }
    public CreditCheckResult Result { get; set; }
    public string? Notes { get; set; }
        
        // Navigation property
    public virtual LoanApplication? LoanApplication { get; set; }
    }
    
    public enum CreditCheckResult
    {
        Pass,
        PassWithConditions,
        Fail,
        NeedsReview
    }
}

namespace FinTech.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a credit limit assigned to a customer
    /// </summary>
    public class LoanCreditLimit : AuditableEntity
    {
    public new string? Id { get; set; }
    public string? CustomerId { get; set; }
        public decimal TotalCreditLimit { get; set; }
        public decimal AvailableCreditLimit { get; set; }
        public decimal UtilizedAmount { get; set; }
        public DateTime AssignmentDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    public string? AssignedBy { get; set; }
    public string? ApprovedBy { get; set; }
    public string? RiskRating { get; set; }
    public string? Notes { get; set; }
        public bool IsActive { get; set; }
        
        // Approval workflow
        public bool IsApproved { get; set; }
        public DateTime? ApprovalDate { get; set; }
        
        // Business logic methods
        public bool HasSufficientLimit(decimal requestedAmount)
        {
            return AvailableCreditLimit >= requestedAmount && IsActive && (!ExpiryDate.HasValue || ExpiryDate.Value > DateTime.UtcNow);
        }
        
        public void ReserveFunds(decimal amount)
        {
            if (!HasSufficientLimit(amount))
                throw new InvalidOperationException("Insufficient credit limit");
                
            AvailableCreditLimit -= amount;
        }
        
        public void RestoreFunds(decimal amount)
        {
            UtilizedAmount = Math.Max(0, UtilizedAmount - amount);
            AvailableCreditLimit = TotalCreditLimit - UtilizedAmount;
        }
        
        public void UtilizeReservedFunds(decimal amount)
        {
            UtilizedAmount += amount;
        }
    }
}