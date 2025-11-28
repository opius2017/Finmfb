using System;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// System-wide configuration parameters for loan management
    /// Managed by Super Admin - controls interest rates, deduction limits, etc.
    /// Aligned with Central Bank and cooperative lending guidelines
    /// </summary>
    public class LoanConfiguration : AuditableEntity
    {
        /// <summary>
        /// Unique configuration key identifier
        /// E.g., "GLOBAL_INTEREST_RATE", "MAX_DEDUCTION_RATE", "LOAN_MULTIPLIER"
        /// </summary>
        public string ConfigKey { get; set; }
        
        /// <summary>
        /// Configuration value (stored as string, parsed based on type)
        /// </summary>
        public string ConfigValue { get; set; }
        
        /// <summary>
        /// Data type of the value: "Decimal", "Integer", "Boolean", "String"
        /// </summary>
        public string ValueType { get; set; }
        
        /// <summary>
        /// Descriptive label for Super Admin UI
        /// </summary>
        public string Label { get; set; }
        
        /// <summary>
        /// Description of what this configuration controls
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Category for grouping: "Interest", "Deduction", "Multiplier", "Thresholds", "Compliance"
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Minimum allowed value (for validation)
        /// </summary>
        public string MinValue { get; set; }
        
        /// <summary>
        /// Maximum allowed value (for validation)
        /// </summary>
        public string MaxValue { get; set; }
        
        /// <summary>
        /// User who last modified this configuration
        /// </summary>
        public string LastModifiedBy { get; set; }
        
        /// <summary>
        /// Date when this configuration was last modified
        /// </summary>
        public DateTime LastModifiedDate { get; set; }
        
        /// <summary>
        /// Whether this configuration is locked from editing
        /// </summary>
        public bool IsLocked { get; set; }
        
        /// <summary>
        /// Reason why configuration is locked (if applicable)
        /// </summary>
        public string LockReason { get; set; }
        
        /// <summary>
        /// Audit trail: when this configuration was activated
        /// </summary>
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Previous value for audit trail
        /// </summary>
        public string PreviousValue { get; set; }
        
        /// <summary>
        /// Whether this configuration requires board approval to change
        /// </summary>
        public bool RequiresBoardApproval { get; set; }
        
        /// <summary>
        /// Approval status: "Pending", "Approved", "Rejected"
        /// </summary>
        public string ApprovalStatus { get; set; } = "Approved";
    }
}
