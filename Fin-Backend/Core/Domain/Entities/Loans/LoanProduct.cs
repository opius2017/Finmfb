using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.Loans
{
    public class LoanProduct : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(20)]
        public string ProductCode { get; set; } = string.Empty;

        public LoanProductType ProductType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal MinInterestRate { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal MaxInterestRate { get; set; }

        public int MinTermMonths { get; set; }

        public int MaxTermMonths { get; set; }

        public RepaymentFrequency RepaymentFrequency { get; set; }

        public InterestCalculationMethod InterestCalculationMethod { get; set; }

        public bool IsActive { get; set; } = true;

        public bool RequiresCollateral { get; set; }

        // Navigation properties
        public virtual ICollection<LoanAccount> LoanAccounts { get; set; } = new List<LoanAccount>();
        public virtual ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
        public virtual ICollection<LoanProductFee> LoanProductFees { get; set; } = new List<LoanProductFee>();
        public virtual ICollection<LoanProductDocument> LoanProductDocuments { get; set; } = new List<LoanProductDocument>();
    }
}
