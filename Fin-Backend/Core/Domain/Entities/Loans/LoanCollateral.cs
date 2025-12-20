using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Features.Loans.Enums;

namespace FinTech.Core.Domain.Entities.Loans
{
    public class LoanCollateral : BaseEntity
    {
        [Required]
        public Guid LoanApplicationId { get; set; }
        public virtual LoanApplication LoanApplication { get; set; } = null!;

        public string? LoanId { get; set; }
        public virtual Loan? Loan { get; set; }

        [Required]
        [StringLength(100)]
        public string CollateralType { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AppraisedValue { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [StringLength(100)]
        public string? RegistrationNumber { get; set; }

        public DateTime? ValuationDate { get; set; }

        [StringLength(200)]
        public string? ValuedBy { get; set; }

        public CollateralStatus Status { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Navigation properties
        public virtual ICollection<LoanCollateralDocument> Documents { get; set; } = new List<LoanCollateralDocument>();
    }
}