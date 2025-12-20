using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Entities.Customers;

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

    public string? CustomerId { get; set; } // Added to match usage
    public virtual Customer? Customer { get; set; } // Added to match usage
    public string TenantId { get; set; } = string.Empty; // Added to match usage

    public string? LoanProductId { get; set; }
    public LoanProduct? LoanProduct { get; set; } // Added to match usage
    
    // FinTech Best Practice: Alias for backward compatibility
    [NotMapped]
    public LoanProduct? Product => LoanProduct;

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
    public DateTime? DisbursementDate { get; set; } // Added to match usage

    [NotMapped]
    public decimal OutstandingBalance { get => Balance; set => Balance = value; } // Alias to Balance

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

    [Column(TypeName = "decimal(18,2)")]
    public decimal? NextPaymentAmount { get; set; }
    public DateTime? NextPaymentDate { get; set; }

    // Navigation properties
    public virtual ICollection<LoanTransaction> Transactions { get; set; } = new List<LoanTransaction>();
}
