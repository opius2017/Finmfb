namespace FinTech.Core.Application.DTOs.Loans
{
    public class LoanStatement
    {
        public string? LoanId { get; set; }
        public string? LoanNumber { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerAccountNumber { get; set; }
        public DateTime StatementFromDate { get; set; }
        public DateTime StatementToDate { get; set; }
        public DateTime StatementGenerationDate { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal DisbursedAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int Term { get; set; }
        public DateTime DisbursementDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
        public decimal PrincipalPaid { get; set; }
        public decimal InterestPaid { get; set; }
        public decimal FeesPaid { get; set; }
        public decimal PenaltiesPaid { get; set; }
        public decimal PrincipalOutstanding { get; set; }
        public decimal InterestOutstanding { get; set; }
        public decimal FeesOutstanding { get; set; }
        public decimal PenaltiesOutstanding { get; set; }
        
        // Original properties kept if needed by other parts, but ensuring we cover the errors
        public string? TransactionType { get; set; }
        public string? Description { get; set; }
        public string? Reference { get; set; }
        public string? Id { get; set; }
        public string? LoanProductId { get; set; }
        public string? LoanProductName { get; set; }
        public string? CustomerId { get; set; }
        public string? ApplicationNumber { get; set; }
        public string? Purpose { get; set; }
        public string? PurposeDescription { get; set; }
        public string? Status { get; set; }
        public string? RejectionReason { get; set; }
        public string? Notes { get; set; }
        public string? AssignedOfficerId { get; set; }
        public string? AssignedOfficerName { get; set; }
        
        public List<LoanStatementTransaction> Transactions { get; set; } = new List<LoanStatementTransaction>();
        public List<LoanRepaymentScheduleDto> RepaymentSchedule { get; set; } = new List<LoanRepaymentScheduleDto>();
    }
}
