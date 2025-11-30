using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans;

/// <summary>
/// Represents a loan account for accounting integration
/// </summary>
public class LoanAccount : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    [ForeignKey(nameof(Loan))]
    public string LoanId { get; set; } = string.Empty;

    public Loan? Loan { get; set; }

    [Required]
    [StringLength(50)]
    public string AccountType { get; set; } = "LOAN_RECEIVABLE"; // LOAN_RECEIVABLE, INTEREST_RECEIVABLE

    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DebitTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CreditTotal { get; set; }

    public DateTime OpenedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ClosedDate { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, CLOSED, SUSPENDED

    [StringLength(100)]
    public string? GLAccountCode { get; set; }

    [StringLength(200)]
    public string? GLAccountName { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    // Navigation properties
    public virtual ICollection<LoanTransaction> Transactions { get; set; } = new List<LoanTransaction>();
}
