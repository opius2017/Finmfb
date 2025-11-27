using FinTech.Core.Domain.Common;

namespace FinTech.Core.Domain.Entities.Loans;

public class LoanRepayment : AuditableEntity
{
    public string LoanId { get; set; } = string.Empty;
    public string LoanAccountNumber { get; set; } = string.Empty;
    public string RepaymentReference { get; set; } = string.Empty;
    public DateTime RepaymentDate { get; set; }
    public decimal Amount { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal PenaltyAmount { get; set; }
    public decimal FeesAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // Cash, Transfer, Deduction
    public string? PaymentReference { get; set; }
    public string Status { get; set; } = "Completed"; // Pending, Completed, Reversed
    public bool IsReversed { get; set; }
    public DateTime? ReversalDate { get; set; }
    public string? ReversalReason { get; set; }
    public string? Notes { get; set; }
}
