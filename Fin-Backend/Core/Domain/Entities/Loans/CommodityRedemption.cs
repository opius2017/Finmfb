using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans;

/// <summary>
/// Represents the redemption of a commodity voucher
/// </summary>
public class CommodityRedemption : BaseEntity
{
    [Required]
    [ForeignKey(nameof(Voucher))]
    public Guid VoucherId { get; set; }

    public CommodityVoucher? Voucher { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal RedeemedAmount { get; set; }

    public DateTime RedemptionDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(200)]
    public string RedeemedBy { get; set; } = string.Empty;

    [StringLength(200)]
    public string? VendorName { get; set; }

    [StringLength(1000)]
    public string? Items { get; set; }

    [StringLength(500)]
    public string? ReceiptNumber { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [StringLength(500)]
    public string? DocumentPath { get; set; }
}
