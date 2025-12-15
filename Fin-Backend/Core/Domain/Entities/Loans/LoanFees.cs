using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a fee charged to a loan
    /// </summary>
    public class LoanFee : AuditableEntity
    {
        public new string Id { get; set; } = Guid.NewGuid().ToString();
        public string LoanId { get; set; } = string.Empty;
        public string? LoanProductId { get; set; } // Added to match config
        public string FeeType { get; set; } = string.Empty;
        public string? Name { get; set; } // Added to match config
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal? Percentage { get; set; } // Added to match config
        public CalculationMethod CalculationMethod { get; set; }
        public bool IsOneTime { get; set; }
        public bool IsRequired { get; set; } // Added to match config
        public DateTime? DueDate { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? AppliedAt { get; set; } // Added to match config
        public string Reference { get; set; } = string.Empty;
        
        // Navigation property
        public virtual Loan Loan { get; set; }
    }
    
    public enum CalculationMethod
    {
        FixedAmount,
        PercentageOfPrincipal,
        PercentageOfOutstandingBalance
    }
}

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a fee configuration for a loan product
    /// </summary>
    public class LoanProductFee : AuditableEntity
    {
        public new string Id { get; set; } = Guid.NewGuid().ToString();
        public string LoanProductId { get; set; } = string.Empty;
        public string FeeType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public CalculationMethod CalculationMethod { get; set; }
        public bool IsOneTime { get; set; }
        public ChargeEvent ChargeEvent { get; set; }
        public bool IsOptional { get; set; }
        public bool IsRefundable { get; set; }
        public string AccountingCode { get; set; } = string.Empty;
        public int? GracePeriodDays { get; set; }
        public bool IsActive { get; set; }
        
        // Navigation property
        public virtual LoanProduct LoanProduct { get; set; }
    }
    
    public enum ChargeEvent
    {
        OnDisbursement,
        OnRepayment,
        OnLatePayment,
        OnEarlyRepayment,
        OnRescheduling,
        OnRenewal,
        OnWriteOff,
        OnMaturity
    }
}
