using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans;

/// <summary>
/// Represents delinquency tracking for loans
/// </summary>
public class LoanDelinquency : BaseEntity
{
    [Required]
    [ForeignKey(nameof(Loan))]
    public string LoanId { get; set; } = string.Empty;

    public Loan? Loan { get; set; }

    public int DaysOverdue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OverdueAmount { get; set; }

    [Required]
    [StringLength(20)]
    public string DelinquencyStage { get; set; } = "CURRENT"; // CURRENT, EARLY, MODERATE, SEVERE, DEFAULT

    public DateTime? FirstOverdueDate { get; set; }

    public DateTime? LastContactDate { get; set; }

    [StringLength(1000)]
    public string? ContactNotes { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, RESOLVED, WRITTEN_OFF

    public DateTime? ResolvedDate { get; set; }

    [StringLength(1000)]
    public string? ResolutionNotes { get; set; }

    public int ContactAttempts { get; set; }

    public DateTime? NextFollowUpDate { get; set; }

    // FinTech Best Practice: Track when delinquency was last checked
    public DateTime CheckDate { get; set; } = DateTime.UtcNow;
}
