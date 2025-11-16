using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Events.Loans;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Features.Loans.Enums;
using LoanStatus = FinTech.Core.Domain.Features.Loans.Enums.LoanStatus;

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a loan application from a customer
    /// </summary>
    public class LoanApplication : AuditableEntity
    {
        public int CustomerId { get; set; }
        public int LoanProductId { get; set; }
        public string ApplicationNumber { get; set; }
        public decimal RequestedAmount { get; set; }
        public int RequestedTerm { get; set; }
        public decimal InterestRate { get; set; }
        public DateTime ApplicationDate { get; set; }
        public LoanPurpose Purpose { get; set; }
        public string PurposeDescription { get; set; }
        public LoanApplicationStatus Status { get; set; }
        public string RejectionReason { get; set; }
        public string Notes { get; set; }
        public string AssignedOfficerId { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string ApprovedBy { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public int? ApprovedTerm { get; set; }
        
        // Risk assessment
        public decimal? CreditScore { get; set; }
        public string RiskRating { get; set; }
        public decimal? DebtToIncomeRatio { get; set; }
        
        // Navigation properties
        public virtual LoanProduct LoanProduct { get; set; }
        public virtual ICollection<LoanGuarantor> Guarantors { get; set; } = new List<LoanGuarantor>();
        public virtual ICollection<LoanCollateral> Collaterals { get; set; } = new List<LoanCollateral>();
        public virtual ICollection<LoanDocument> Documents { get; set; } = new List<LoanDocument>();
        public virtual Loan Loan { get; set; }
        
        // Business logic methods
        public void Submit()
        {
            if (Status != LoanApplicationStatus.Draft)
                throw new InvalidOperationException("Only draft applications can be submitted");
            
            Status = LoanApplicationStatus.Submitted;
            AddDomainEvent(new LoanApplicationSubmittedEvent(Id));
        }
        
        public void Approve(string approvedBy, decimal? approvedAmount = null, int? approvedTerm = null)
        {
            if (Status != LoanApplicationStatus.InReview)
                throw new InvalidOperationException("Only applications under review can be approved");
            
            Status = LoanApplicationStatus.Approved;
            ApprovalDate = DateTime.UtcNow;
            ApprovedBy = approvedBy;
            ApprovedAmount = approvedAmount ?? RequestedAmount;
            ApprovedTerm = approvedTerm ?? RequestedTerm;
            
            AddDomainEvent(new LoanApplicationApprovedEvent(Id));
        }
        
        public void Reject(string reason)
        {
            if (Status == LoanApplicationStatus.Approved || Status == LoanApplicationStatus.Disbursed)
                throw new InvalidOperationException("Approved or disbursed applications cannot be rejected");
            
            Status = LoanApplicationStatus.Rejected;
            RejectionReason = reason;
            
            AddDomainEvent(new LoanApplicationRejectedEvent(Id, reason));
        }
        
        public Loan CreateLoan()
        {
            if (Status != LoanApplicationStatus.Approved)
                throw new InvalidOperationException("Cannot create loan from unapproved application");
            
            if (!ApprovedAmount.HasValue)
                throw new InvalidOperationException("Approved amount is required to create loan");
                
            if (!ApprovedTerm.HasValue)
                throw new InvalidOperationException("Approved term is required to create loan");
            
            // Generate loan number - this should be done through a proper service
            string loanNumber = $"LN{DateTime.Now:yyyyMMdd}{Id:D6}";
            
            var loan = new Loan(
                loanNumber,
                CustomerId,
                ApprovedAmount.Value,
                InterestRate,
                ApprovedTerm.Value,
                DateTime.UtcNow,
                LoanProduct?.Name ?? "Standard",
                "MONTHLY",
                "NGN");
            
            Status = LoanApplicationStatus.Disbursed;
            Loan = loan;
            
            AddDomainEvent(new LoanCreatedFromApplicationEvent(Id, loan.Id));
            
            return loan;
        }
    }
}
