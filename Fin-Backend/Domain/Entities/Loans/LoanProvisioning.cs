using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans;

public class LoanProvisioning : AuditableEntity
{
    public string LoanId { get; set; } = string.Empty;
    public string LoanAccountNumber { get; set; } = string.Empty;
    public DateTime ProvisionDate { get; set; }
    public string ProvisionType { get; set; } = string.Empty; // Specific, General, IFRS9
    public decimal OutstandingBalance { get; set; }
    public decimal ProvisionRate { get; set; }
    public decimal ProvisionAmount { get; set; }
    public decimal PreviousProvision { get; set; }
    public decimal ProvisionMovement { get; set; }
    public string ClassificationCategory { get; set; } = string.Empty; // Performing, NPL, etc.
    public int DaysInArrears { get; set; }
    public string ProvisionBasis { get; set; } = string.Empty; // CBN, IFRS9
    public string? Notes { get; set; }
    public bool IsReversed { get; set; }
    public DateTime? ReversalDate { get; set; }
    public string? ReversalReason { get; set; }
}
