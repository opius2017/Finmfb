using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.Accounting
{
    public class Budget : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public Guid FinancialPeriodId { get; set; }
        public virtual FinancialPeriod FinancialPeriod { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // Operating, Capital, etc.

        public BudgetStatus Status { get; set; }

        public virtual ICollection<BudgetItem> BudgetItems { get; set; } = new List<BudgetItem>();
    }

    public class BudgetItem : BaseEntity
    {
        [Required]
        public Guid BudgetId { get; set; }
        public virtual Budget Budget { get; set; } = null!;

        [Required]
        public Guid ChartOfAccountId { get; set; }
        public virtual ChartOfAccount ChartOfAccount { get; set; } = null!;

        [Required]
        public decimal BudgetedAmount { get; set; }

        public decimal ActualAmount { get; set; }

        public decimal VarianceAmount => ActualAmount - BudgetedAmount;

        public decimal VariancePercentage => BudgetedAmount != 0 ? (VarianceAmount / BudgetedAmount) * 100 : 0;

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public enum BudgetStatus
    {
        Draft = 1,
        PendingApproval = 2,
        Approved = 3,
        Active = 4,
        Closed = 5,
        Rejected = 6
    }
}