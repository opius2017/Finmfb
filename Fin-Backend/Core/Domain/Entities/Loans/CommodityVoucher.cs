using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans;

/// <summary>
/// Represents a commodity voucher issued for commodity-based loans
/// </summary>
public class CommodityVoucher : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string VoucherNumber { get; set; } = string.Empty;

    [Required]
    [ForeignKey(nameof(Loan))]
    public string LoanId { get; set; } = string.Empty;

    public Loan? Loan { get; set; }

    [Required]
    [ForeignKey(nameof(Member))]
    public string MemberId { get; set; } = string.Empty;

    public Member? Member { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal RemainingAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UsedAmount { get; set; }

    [Required]
    [StringLength(200)]
    public string Vendor { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string CommodityType { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "ISSUED"; // ISSUED, REDEEMED, EXPIRED, CANCELLED

    public DateTime IssueDate { get; set; } = DateTime.UtcNow;

    public DateTime ExpiryDate { get; set; }

    public DateTime? RedemptionDate { get; set; }

    [StringLength(500)]
    public string QrCode { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }

    // FinTech Best Practice: Alias for IssueDate
    [NotMapped]
    public DateTime IssuedDate { get => IssueDate; set => IssueDate = value; }

    // Navigation property
    public virtual ICollection<CommodityRedemption> Redemptions { get; set; } = new List<CommodityRedemption>();
}
