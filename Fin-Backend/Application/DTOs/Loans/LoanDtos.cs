
using System;
using System.Collections.Generic;

    public string? Purpose { get; set; }
    public string? PurposeDescription { get; set; }
    public string? Status { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
    public string? AssignedOfficerId { get; set; }
    public string? AssignedOfficerName { get; set; }
    public decimal PrincipalPaid { get; set; }
    public decimal InterestPaid { get; set; }
    public decimal FeesPaid { get; set; }
    public decimal PenaltiesPaid { get; set; }

    public decimal PrincipalOutstanding { get; set; }
    public decimal InterestOutstanding { get; set; }
    public decimal FeesOutstanding { get; set; }
    public decimal PenaltiesOutstanding { get; set; }

    public List<LoanStatementTransaction> Transactions { get; set; } = new List<LoanStatementTransaction>();
    public List<LoanRepaymentScheduleDto> RepaymentSchedule { get; set; } = new List<LoanRepaymentScheduleDto>();
}

{
    public string Id { get; set; }
    public string LoanNumber { get; set; }
    public string CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string LoanProductId { get; set; }
    public string LoanProductName { get; set; }
    public string LoanApplicationId { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal DisbursedAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int Term { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? MaturityDate { get; set; }
    public DateTime? DisbursementDate { get; set; }
        // LoanRepaymentDto has been moved to its own file.
    public decimal InterestPaid { get; set; }
    public decimal FeesPaid { get; set; }
    public decimal PenaltiesPaid { get; set; }
    public decimal TotalPaid { get; set; }

    public int? DaysOverdue { get; set; }
    public decimal? OverdueAmount { get; set; }
    public DateTime? NextPaymentDueDate { get; set; }
    public decimal? NextPaymentAmount { get; set; }
}
    
    
    public class CreateLoanRepaymentDto
    {
        public string LoanId { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
    
    

    
