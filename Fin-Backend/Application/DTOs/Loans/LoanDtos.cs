using System;
using System.Collections.Generic;

namespace FinTech.Application.DTOs.Loans
{
    public class LoanStatement
    {
        public string LoanId { get; set; }
        public string LoanNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAccountNumber { get; set; }
        public DateTime StatementFromDate { get; set; }
        public DateTime StatementToDate { get; set; }
        public DateTime StatementGenerationDate { get; set; }
        
        public decimal PrincipalAmount { get; set; }
        public decimal DisbursedAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int Term { get; set; }
        public DateTime DisbursementDate { get; set; }
        public DateTime MaturityDate { get; set; }
        
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
        
        public decimal PrincipalPaid { get; set; }
        public decimal InterestPaid { get; set; }
        public decimal FeesPaid { get; set; }
        public decimal PenaltiesPaid { get; set; }
        
        public decimal PrincipalOutstanding { get; set; }
        public decimal InterestOutstanding { get; set; }
        public decimal FeesOutstanding { get; set; }
        public decimal PenaltiesOutstanding { get; set; }
        
        public List<LoanStatementTransaction> Transactions { get; set; } = new List<LoanStatementTransaction>();
        public List<LoanRepaymentScheduleDto> RepaymentSchedule { get; set; } = new List<LoanRepaymentScheduleDto>();
    }
    
    public class LoanStatementTransaction
    {
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal FeesAmount { get; set; }
        public decimal PenaltyAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal RunningBalance { get; set; }
    }
    
    public class LoanApplicationDto
    {
        public string Id { get; set; }
        public string LoanProductId { get; set; }
        public string LoanProductName { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string ApplicationNumber { get; set; }
        public decimal RequestedAmount { get; set; }
        public int RequestedTerm { get; set; }
        public decimal InterestRate { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Purpose { get; set; }
        public string PurposeDescription { get; set; }
        public string Status { get; set; }
        public string RejectionReason { get; set; }
        public string Notes { get; set; }
        public string AssignedOfficerId { get; set; }
        public string AssignedOfficerName { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string ApprovedBy { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public int? ApprovedTerm { get; set; }
        
        // Risk assessment
        public decimal? CreditScore { get; set; }
        public string RiskRating { get; set; }
        public decimal? DebtToIncomeRatio { get; set; }
        
        public List<LoanGuarantorDto> Guarantors { get; set; } = new List<LoanGuarantorDto>();
        public List<LoanCollateralDto> Collaterals { get; set; } = new List<LoanCollateralDto>();
        public List<LoanDocumentDto> Documents { get; set; } = new List<LoanDocumentDto>();
    }
    
    public class CreateLoanApplicationDto
    {
        public string LoanProductId { get; set; }
        public string CustomerId { get; set; }
        public decimal RequestedAmount { get; set; }
        public int RequestedTerm { get; set; }
        public string Purpose { get; set; }
        public string PurposeDescription { get; set; }
        public string Notes { get; set; }
    }
    
    public class LoanProductDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Currency { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public int MinTerm { get; set; }
        public int MaxTerm { get; set; }
        public decimal InterestRate { get; set; }
        public string InterestCalculationMethod { get; set; }
        public string RepaymentFrequency { get; set; }
        public int GracePeriod { get; set; }
        public bool AllowEarlyRepayment { get; set; }
        public decimal? EarlyRepaymentFeePercentage { get; set; }
        public bool IsSecured { get; set; }
        public decimal CollateralCoverageRatio { get; set; }
        public string CustomerSegment { get; set; }
        public bool RequiresGuarantor { get; set; }
        public int MinNumberOfGuarantors { get; set; }
        public bool IsActive { get; set; }
        
        public List<LoanProductFeeDto> Fees { get; set; } = new List<LoanProductFeeDto>();
        public List<LoanProductDocumentDto> RequiredDocuments { get; set; } = new List<LoanProductDocumentDto>();
    }
    
    public class CreateLoanProductDto
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Currency { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public int MinTerm { get; set; }
        public int MaxTerm { get; set; }
        public decimal InterestRate { get; set; }
        public string InterestCalculationMethod { get; set; }
        public string RepaymentFrequency { get; set; }
        public int GracePeriod { get; set; }
        public bool AllowEarlyRepayment { get; set; }
        public decimal? EarlyRepaymentFeePercentage { get; set; }
        public bool IsSecured { get; set; }
        public decimal CollateralCoverageRatio { get; set; }
        public string CustomerSegment { get; set; }
        public bool RequiresGuarantor { get; set; }
        public int MinNumberOfGuarantors { get; set; }
    }
    
    public class LoanDto
    {
        public string Id { get; set; }
        public string LoanNumber { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string LoanProductId { get; set; }
        public string LoanProductName { get; set; }
        public string LoanApplicationId { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal DisbursedAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int Term { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? MaturityDate { get; set; }
        public DateTime? DisbursementDate { get; set; }
        public string Status { get; set; }
        public string RepaymentFrequency { get; set; }
        
        public decimal PrincipalOutstanding { get; set; }
        public decimal InterestOutstanding { get; set; }
        public decimal FeesOutstanding { get; set; }
        public decimal PenaltiesOutstanding { get; set; }
        public decimal TotalOutstanding { get; set; }
        
        public decimal PrincipalPaid { get; set; }
        public decimal InterestPaid { get; set; }
        public decimal FeesPaid { get; set; }
        public decimal PenaltiesPaid { get; set; }
        public decimal TotalPaid { get; set; }
        
        public int? DaysOverdue { get; set; }
        public decimal? OverdueAmount { get; set; }
        public DateTime? NextPaymentDueDate { get; set; }
        public decimal? NextPaymentAmount { get; set; }
    }
    
    public class LoanTransactionDto
    {
        public string Id { get; set; }
        public string LoanId { get; set; }
        public string TransactionType { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal FeesAmount { get; set; }
        public decimal PenaltyAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; }
    }
    
    public class CreateLoanRepaymentDto
    {
        public string LoanId { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
    
    public class LoanRepaymentScheduleDto
    {
        public string Id { get; set; }
        public string LoanId { get; set; }
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal FeesAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public string Status { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
    
    public class LoanGuarantorDto
    {
        public string Id { get; set; }
        public string LoanApplicationId { get; set; }
        public string LoanId { get; set; }
        public string FullName { get; set; }
        public string Relationship { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string IdentificationType { get; set; }
        public string IdentificationNumber { get; set; }
        public DateTime? IdentificationExpiryDate { get; set; }
        public string EmploymentStatus { get; set; }
        public string Employer { get; set; }
        public decimal? MonthlyIncome { get; set; }
        public string VerificationStatus { get; set; }
        public DateTime? VerificationDate { get; set; }
        public string VerifiedBy { get; set; }
        public string Notes { get; set; }
    }
    
    public class CreateLoanGuarantorDto
    {
        public string LoanApplicationId { get; set; }
        public string FullName { get; set; }
        public string Relationship { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string IdentificationType { get; set; }
        public string IdentificationNumber { get; set; }
        public DateTime? IdentificationExpiryDate { get; set; }
        public string EmploymentStatus { get; set; }
        public string Employer { get; set; }
        public decimal? MonthlyIncome { get; set; }
        public string Notes { get; set; }
    }
    
    public class LoanCollateralDto
    {
        public string Id { get; set; }
        public string LoanApplicationId { get; set; }
        public string LoanId { get; set; }
        public string CollateralType { get; set; }
        public string Description { get; set; }
        public decimal Value { get; set; }
        public DateTime ValuationDate { get; set; }
        public string ValuationMethod { get; set; }
        public string ValuedBy { get; set; }
        public string OwnerName { get; set; }
        public string OwnerRelationship { get; set; }
        public string RegistrationNumber { get; set; }
        public string Location { get; set; }
        public bool IsInsured { get; set; }
        public string InsuranceCompany { get; set; }
        public string InsurancePolicyNumber { get; set; }
        public DateTime? InsuranceExpiryDate { get; set; }
        public string VerificationStatus { get; set; }
        public DateTime? VerificationDate { get; set; }
        public string VerifiedBy { get; set; }
        public string Notes { get; set; }
    }
    
    public class CreateLoanCollateralDto
    {
        public string LoanApplicationId { get; set; }
        public string CollateralType { get; set; }
        public string Description { get; set; }
        public decimal Value { get; set; }
        public DateTime ValuationDate { get; set; }
        public string ValuationMethod { get; set; }
        public string ValuedBy { get; set; }
        public string OwnerName { get; set; }
        public string OwnerRelationship { get; set; }
        public string RegistrationNumber { get; set; }
        public string Location { get; set; }
        public bool IsInsured { get; set; }
        public string InsuranceCompany { get; set; }
        public string InsurancePolicyNumber { get; set; }
        public DateTime? InsuranceExpiryDate { get; set; }
        public string Notes { get; set; }
    }
    
    public class LoanDocumentDto
    {
        public string Id { get; set; }
        public string LoanId { get; set; }
        public string DocumentType { get; set; }
        public string DocumentName { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public string Status { get; set; }
        public DateTime UploadDate { get; set; }
        public string UploadedBy { get; set; }
        public DateTime? VerificationDate { get; set; }
        public string VerifiedBy { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Notes { get; set; }
    }
    
    public class LoanProductFeeDto
    {
        public string Id { get; set; }
        public string LoanProductId { get; set; }
        public string FeeType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string CalculationMethod { get; set; }
        public bool IsOneTime { get; set; }
        public string ChargeEvent { get; set; }
        public bool IsOptional { get; set; }
        public bool IsRefundable { get; set; }
        public string AccountingCode { get; set; }
        public int? GracePeriodDays { get; set; }
        public bool IsActive { get; set; }
    }
    
    public class LoanProductDocumentDto
    {
        public string Id { get; set; }
        public string LoanProductId { get; set; }
        public string DocumentType { get; set; }
        public string DocumentName { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
        public string ApplicableFor { get; set; }
    }
    
    public class LoanCollectionDto
    {
        public string Id { get; set; }
        public string LoanId { get; set; }
        public string LoanNumber { get; set; }
        public string CustomerName { get; set; }
        public DateTime OverdueDate { get; set; }
        public int DaysOverdue { get; set; }
        public decimal OverdueAmount { get; set; }
        public string Status { get; set; }
        public string AssignedTo { get; set; }
        public string AssignedToName { get; set; }
        public DateTime? ResolutionDate { get; set; }
        public string ResolutionNotes { get; set; }
        public string Priority { get; set; }
        public string ReasonForDelinquency { get; set; }
        public string Notes { get; set; }
        public string ActionPlan { get; set; }
        
        public List<LoanCollectionActionDto> CollectionActions { get; set; } = new List<LoanCollectionActionDto>();
    }
    
    public class LoanCollectionActionDto
    {
        public string Id { get; set; }
        public string CollectionId { get; set; }
        public string ActionType { get; set; }
        public DateTime ActionDate { get; set; }
        public string PerformedBy { get; set; }
        public string PerformedByName { get; set; }
        public string Description { get; set; }
        public string Result { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public bool RequiresFollowUp { get; set; }
        public string ContactPerson { get; set; }
        public string ContactDetails { get; set; }
    }
    
    public class CreateLoanCollectionActionDto
    {
        public string CollectionId { get; set; }
        public string ActionType { get; set; }
        public string Description { get; set; }
        public string Result { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public bool RequiresFollowUp { get; set; }
        public string ContactPerson { get; set; }
        public string ContactDetails { get; set; }
    }
}