using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a lien on assets purchased through commodity loans
    /// Tracks asset ownership until loan is fully repaid
    /// </summary>
    public class AssetLien : BaseEntity
    {

        [Required]
        [StringLength(50)]
        public string LienNumber { get; set; } = string.Empty; // Format: LIEN/YYYY/NNNNNN

        [Required]
        [ForeignKey(nameof(Loan))]
        public string LoanId { get; set; } = string.Empty;

        public Loan? Loan { get; set; }

        [Required]
        [ForeignKey(nameof(Member))]
        public string MemberId { get; set; } = string.Empty;

        public Member? Member { get; set; }

        [Required]
        [StringLength(200)]
        public string AssetDescription { get; set; } = string.Empty;

        [StringLength(100)]
        public string? AssetSerialNumber { get; set; }

        [StringLength(100)]
        public string? AssetModel { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AssetValue { get; set; }

        public DateTime PurchaseDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "ACTIVE"; // ACTIVE, RELEASED, REPOSSESSED

        public DateTime? ReleasedDate { get; set; }

        [StringLength(450)]
        public string? ReleasedBy { get; set; }

        [StringLength(1000)]
        public string? ReleaseNotes { get; set; }

        [StringLength(500)]
        public string? DocumentPath { get; set; } // Path to lien document
    }
}
