using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Enums;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a template for payment schedules of a loan product
    /// </summary>
    public class LoanPaymentScheduleTemplate : AuditableEntity
    {
        public new string Id { get; set; }
        public string LoanProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public RepaymentFrequency RepaymentFrequency { get; set; }
        public int GracePeriodDays { get; set; }
        public bool AllowEarlyRepayment { get; set; }
        public decimal? EarlyRepaymentFeePercentage { get; set; }
        public InterestCalculationMethod InterestCalculationMethod { get; set; }
        public InterestApplicationMethod InterestApplicationMethod { get; set; }
        public bool ApplyLateFees { get; set; }
        public decimal? LateFeeFixed { get; set; }
        public decimal? LateFeePercentage { get; set; }
        public int LateFeeGracePeriodDays { get; set; }
        
        // Navigation property
        public virtual LoanProduct LoanProduct { get; set; }
    }
    
    public enum InterestApplicationMethod
    {
        Declining,            // Interest calculated on outstanding balance
        FlatRate,             // Interest calculated on original principal
        CompoundDaily,        // Interest calculated daily and compounded
        CompoundMonthly       // Interest calculated monthly and compounded
    }
}

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a scheduled payment notification for a loan
    /// </summary>
    public class LoanPaymentReminder : AuditableEntity
    {
        public new string Id { get; set; }
        public string LoanId { get; set; }
        public string RepaymentScheduleId { get; set; }
        public DateTime DueDate { get; set; }
        public decimal AmountDue { get; set; }
        public ReminderType ReminderType { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? SentDate { get; set; }
        public string RecipientChannels { get; set; } // Email, SMS, Push, etc.
        public string MessageTemplate { get; set; }
        public bool IsSent { get; set; }
        public string DeliveryStatus { get; set; }
        public string Notes { get; set; }
        
        // Navigation properties
        public virtual Loan Loan { get; set; }
        public virtual LoanRepaymentSchedule RepaymentSchedule { get; set; }
    }
    
    public enum ReminderType
    {
        Upcoming,         // Before due date
        DueToday,         // On due date
        Overdue,          // After due date
        SecondReminder,   // Several days after due date
        FinalNotice       // Final notice before further action
    }
}
