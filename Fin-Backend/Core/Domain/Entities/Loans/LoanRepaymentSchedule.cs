using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.Loans
{
    public class LoanRepaymentSchedule : BaseEntity
    {
        [Required]
        public Guid LoanAccountId { get; set; }
        public virtual LoanAccount LoanAccount { get; set; } = null!;

        public int InstallmentNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrincipalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal InterestAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OutstandingBalance { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? PaymentDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AmountPaid { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidPrincipal { get; set; }

        public RepaymentStatus Status { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PenaltyAmount { get; set; }

        public int? DaysOverdue { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
