using System;
using System.Collections.Generic;
using FinTech.Domain.Common;

namespace FinTech.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a loan product offered by the financial institution
    /// </summary>
    public class LoanProduct : AuditableEntity
    {
        public string Id { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        
        // Loan terms
        public decimal MinLoanAmount { get; set; }
        public decimal MaxLoanAmount { get; set; }
        public int MinTenorInMonths { get; set; }
        public int MaxTenorInMonths { get; set; }
        public int GracePeriodInDays { get; set; }
        
        // Interest configuration
        public decimal BaseAnnualInterestRate { get; set; }
        public InterestCalculationMethod InterestCalculationMethod { get; set; }
        public InterestType InterestType { get; set; }
        public int InterestAccrualFrequencyInDays { get; set; } // How often interest is accrued (daily=1, monthly=30, etc.)
        
        // Repayment configuration
        public RepaymentFrequency RepaymentFrequency { get; set; }
        public bool AllowEarlyRepayment { get; set; }
        public decimal EarlyRepaymentFeePercentage { get; set; }
        
        // Fees and charges
        public decimal ProcessingFeePercentage { get; set; }
        public decimal AdministrativeFeePercentage { get; set; }
        public decimal InsuranceFeePercentage { get; set; }
        public decimal LateRepaymentFeePercentage { get; set; }
        public decimal LateRepaymentFixedFee { get; set; }
        
        // Risk management
        public bool RequireCollateral { get; set; }
        public decimal MinCollateralValuePercentage { get; set; }
        public int MinCreditScore { get; set; }
        public decimal MaxDebtToIncomeRatio { get; set; }
        
        // Approval configuration
        public int RequiredGuarantors { get; set; }
        public int MaxDisbursementDays { get; set; } // Max days allowed between approval and disbursement
        
        // Account mapping (for accounting integration)
        public string InterestIncomeAccountId { get; set; }
        public string InterestReceivableAccountId { get; set; }
        public string PrincipalAccountId { get; set; }
        public string FeeIncomeAccountId { get; set; }
        public string PenaltyIncomeAccountId { get; set; }
        
        // IFRS 9 configuration
        public string IfrsProductCategory { get; set; }
        public int Stage1DaysOverdue { get; set; } = 0;
        public int Stage2DaysOverdue { get; set; } = 30;
        public int Stage3DaysOverdue { get; set; } = 90;
        public decimal Stage1ProvisionPercentage { get; set; } = 0.01m; // 1%
        public decimal Stage2ProvisionPercentage { get; set; } = 0.25m; // 25%
        public decimal Stage3ProvisionPercentage { get; set; } = 1.00m; // 100%
        
        // Target market
        public CustomerSegment TargetCustomerSegment { get; set; }
        public LoanPurpose LoanPurpose { get; set; }
        
        // Regulatory reporting
        public string CbnProductCode { get; set; }
        public string NdicProductCode { get; set; }
        
        // Navigation properties
        public virtual ICollection<Loan> Loans { get; set; }
        public virtual ICollection<LoanProductDocument> RequiredDocuments { get; set; }
    }
    
    public enum InterestCalculationMethod
    {
        FlatRate,
        DecliningBalance,
        ReducingBalance,
        CompoundInterest
    }
    
    public enum InterestType
    {
        Fixed,
        Variable
    }
    
    public enum RepaymentFrequency
    {
        Daily,
        Weekly,
        BiWeekly,
        Monthly,
        Quarterly,
        Annually,
        Custom
    }
    
    public enum CustomerSegment
    {
        Individual,
        Group,
        MicroEnterprise,
        SmallBusiness,
        MediumBusiness,
        Corporate,
        All
    }
    
    public enum LoanPurpose
    {
        Agriculture,
        Business,
        Education,
        Housing,
        Personal,
        AssetFinancing,
        WorkingCapital,
        ProjectFinancing,
        Other
    }
}