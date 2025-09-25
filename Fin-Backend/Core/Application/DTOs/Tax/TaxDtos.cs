using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Tax
{
    /// <summary>
    /// DTO for tax type information
    /// </summary>
    public class TaxTypeDto
    {
        /// <summary>
        /// Unique identifier for the tax type
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Tax type code (e.g., VAT, WHT, CIT)
        /// </summary>
        public string Code { get; set; }
        
        /// <summary>
        /// Tax type name (e.g., Value Added Tax)
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Description of the tax type
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Whether the tax type is active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// The liability account where collected tax is recorded
        /// </summary>
        public string LiabilityAccountId { get; set; }
        
        /// <summary>
        /// The receivable account where claimable tax is recorded
        /// </summary>
        public string ReceivableAccountId { get; set; }
        
        /// <summary>
        /// Whether the tax is collected from customers or paid to vendors
        /// </summary>
        public TaxDirection Direction { get; set; }
        
        /// <summary>
        /// Whether the tax can be reclaimed/offset
        /// </summary>
        public bool IsReclaimable { get; set; }
        
        /// <summary>
        /// The regulatory authority for this tax type
        /// </summary>
        public string RegulatoryAuthority { get; set; }
    }

    /// <summary>
    /// DTO for creating a new tax type
    /// </summary>
    public class CreateTaxTypeDto
    {
        /// <summary>
        /// Tax type code (e.g., VAT, WHT, CIT)
        /// </summary>
        public string Code { get; set; }
        
        /// <summary>
        /// Tax type name (e.g., Value Added Tax)
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Description of the tax type
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// The liability account where collected tax is recorded
        /// </summary>
        public string LiabilityAccountId { get; set; }
        
        /// <summary>
        /// The receivable account where claimable tax is recorded
        /// </summary>
        public string ReceivableAccountId { get; set; }
        
        /// <summary>
        /// Whether the tax is collected from customers or paid to vendors
        /// </summary>
        public TaxDirection Direction { get; set; }
        
        /// <summary>
        /// Whether the tax can be reclaimed/offset
        /// </summary>
        public bool IsReclaimable { get; set; }
        
        /// <summary>
        /// The regulatory authority for this tax type
        /// </summary>
        public string RegulatoryAuthority { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing tax type
    /// </summary>
    public class UpdateTaxTypeDto
    {
        /// <summary>
        /// Unique identifier for the tax type
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Tax type name (e.g., Value Added Tax)
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Description of the tax type
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Whether the tax type is active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// The liability account where collected tax is recorded
        /// </summary>
        public string LiabilityAccountId { get; set; }
        
        /// <summary>
        /// The receivable account where claimable tax is recorded
        /// </summary>
        public string ReceivableAccountId { get; set; }
        
        /// <summary>
        /// Whether the tax is collected from customers or paid to vendors
        /// </summary>
        public TaxDirection Direction { get; set; }
        
        /// <summary>
        /// Whether the tax can be reclaimed/offset
        /// </summary>
        public bool IsReclaimable { get; set; }
        
        /// <summary>
        /// The regulatory authority for this tax type
        /// </summary>
        public string RegulatoryAuthority { get; set; }
    }

    /// <summary>
    /// DTO for tax rate information
    /// </summary>
    public class TaxRateDto
    {
        /// <summary>
        /// Unique identifier for the tax rate
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Tax type ID this rate belongs to
        /// </summary>
        public string TaxTypeId { get; set; }
        
        /// <summary>
        /// Tax type code (e.g., VAT, WHT)
        /// </summary>
        public string TaxTypeCode { get; set; }
        
        /// <summary>
        /// Rate name or description
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Rate percentage value
        /// </summary>
        public decimal Rate { get; set; }
        
        /// <summary>
        /// Minimum taxable amount for this rate
        /// </summary>
        public decimal? MinimumAmount { get; set; }
        
        /// <summary>
        /// Maximum taxable amount for this rate
        /// </summary>
        public decimal? MaximumAmount { get; set; }
        
        /// <summary>
        /// Effective date when this rate becomes valid
        /// </summary>
        public DateTime EffectiveDate { get; set; }
        
        /// <summary>
        /// End date when this rate expires (null for indefinite)
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Whether this rate is currently active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Category or type of items this rate applies to
        /// </summary>
        public string ApplicableCategory { get; set; }
    }

    /// <summary>
    /// DTO for creating a new tax rate
    /// </summary>
    public class CreateTaxRateDto
    {
        /// <summary>
        /// Tax type ID this rate belongs to
        /// </summary>
        public string TaxTypeId { get; set; }
        
        /// <summary>
        /// Rate name or description
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Rate percentage value
        /// </summary>
        public decimal Rate { get; set; }
        
        /// <summary>
        /// Minimum taxable amount for this rate
        /// </summary>
        public decimal? MinimumAmount { get; set; }
        
        /// <summary>
        /// Maximum taxable amount for this rate
        /// </summary>
        public decimal? MaximumAmount { get; set; }
        
        /// <summary>
        /// Effective date when this rate becomes valid
        /// </summary>
        public DateTime EffectiveDate { get; set; }
        
        /// <summary>
        /// End date when this rate expires (null for indefinite)
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Category or type of items this rate applies to
        /// </summary>
        public string ApplicableCategory { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing tax rate
    /// </summary>
    public class UpdateTaxRateDto
    {
        /// <summary>
        /// Unique identifier for the tax rate
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Rate name or description
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Rate percentage value
        /// </summary>
        public decimal Rate { get; set; }
        
        /// <summary>
        /// Minimum taxable amount for this rate
        /// </summary>
        public decimal? MinimumAmount { get; set; }
        
        /// <summary>
        /// Maximum taxable amount for this rate
        /// </summary>
        public decimal? MaximumAmount { get; set; }
        
        /// <summary>
        /// Effective date when this rate becomes valid
        /// </summary>
        public DateTime EffectiveDate { get; set; }
        
        /// <summary>
        /// End date when this rate expires (null for indefinite)
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Whether this rate is currently active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Category or type of items this rate applies to
        /// </summary>
        public string ApplicableCategory { get; set; }
    }

    /// <summary>
    /// DTO for tax calculation request
    /// </summary>
    public class TaxCalculationRequestDto
    {
        /// <summary>
        /// Tax type ID to calculate
        /// </summary>
        public string TaxTypeId { get; set; }
        
        /// <summary>
        /// Specific tax rate ID to use (optional, will use applicable rate if not specified)
        /// </summary>
        public string TaxRateId { get; set; }
        
        /// <summary>
        /// Taxable amount before tax
        /// </summary>
        public decimal TaxableAmount { get; set; }
        
        /// <summary>
        /// Category of the taxable item (e.g., goods, services)
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Transaction date for determining applicable tax rate
        /// </summary>
        public DateTime TransactionDate { get; set; }
        
        /// <summary>
        /// Whether the input amount is tax inclusive
        /// </summary>
        public bool IsTaxInclusive { get; set; }
        
        /// <summary>
        /// Optional customer/vendor ID for exemption checking
        /// </summary>
        public string PartyId { get; set; }
        
        /// <summary>
        /// Optional reference to related transaction
        /// </summary>
        public string TransactionReference { get; set; }
        
        /// <summary>
        /// Optional additional information for tax calculation
        /// </summary>
        public Dictionary<string, string> AdditionalData { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// DTO for tax calculation result
    /// </summary>
    public class TaxCalculationResultDto
    {
        /// <summary>
        /// Tax type ID used for calculation
        /// </summary>
        public string TaxTypeId { get; set; }
        
        /// <summary>
        /// Tax type code (e.g., VAT, WHT)
        /// </summary>
        public string TaxTypeCode { get; set; }
        
        /// <summary>
        /// Tax type name
        /// </summary>
        public string TaxTypeName { get; set; }
        
        /// <summary>
        /// Tax rate ID used for calculation
        /// </summary>
        public string TaxRateId { get; set; }
        
        /// <summary>
        /// Applied tax rate percentage
        /// </summary>
        public decimal AppliedRate { get; set; }
        
        /// <summary>
        /// Original taxable amount
        /// </summary>
        public decimal TaxableAmount { get; set; }
        
        /// <summary>
        /// Calculated tax amount
        /// </summary>
        public decimal TaxAmount { get; set; }
        
        /// <summary>
        /// Total amount including tax
        /// </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// Whether the item is exempt from this tax
        /// </summary>
        public bool IsExempt { get; set; }
        
        /// <summary>
        /// Reason for exemption, if applicable
        /// </summary>
        public string ExemptionReason { get; set; }
        
        /// <summary>
        /// Transaction date used for tax calculation
        /// </summary>
        public DateTime TransactionDate { get; set; }
        
        /// <summary>
        /// Account ID for recording this tax
        /// </summary>
        public string TaxAccountId { get; set; }
        
        /// <summary>
        /// Calculation notes or messages
        /// </summary>
        public string Notes { get; set; }
    }

    /// <summary>
    /// DTO for withholding tax calculation request
    /// </summary>
    public class WithholdingTaxRequestDto
    {
        /// <summary>
        /// Gross amount subject to withholding tax
        /// </summary>
        public decimal GrossAmount { get; set; }
        
        /// <summary>
        /// Type of income for WHT (e.g., professional-services, rent, dividend)
        /// </summary>
        public string IncomeType { get; set; }
        
        /// <summary>
        /// ID of the vendor/payee
        /// </summary>
        public string VendorId { get; set; }
        
        /// <summary>
        /// Whether the vendor is a resident entity
        /// </summary>
        public bool IsResident { get; set; }
        
        /// <summary>
        /// Optional tax identification number
        /// </summary>
        public string TaxIdentificationNumber { get; set; }
        
        /// <summary>
        /// Transaction date for determining applicable rate
        /// </summary>
        public DateTime TransactionDate { get; set; }
        
        /// <summary>
        /// Optional reference to related transaction
        /// </summary>
        public string TransactionReference { get; set; }
    }

    /// <summary>
    /// DTO for withholding tax calculation result
    /// </summary>
    public class WithholdingTaxResultDto
    {
        /// <summary>
        /// Applied WHT rate percentage
        /// </summary>
        public decimal AppliedRate { get; set; }
        
        /// <summary>
        /// Gross amount subject to WHT
        /// </summary>
        public decimal GrossAmount { get; set; }
        
        /// <summary>
        /// Calculated WHT amount to withhold
        /// </summary>
        public decimal WithholdingAmount { get; set; }
        
        /// <summary>
        /// Net amount after withholding tax
        /// </summary>
        public decimal NetAmount { get; set; }
        
        /// <summary>
        /// Tax type ID used (typically WHT)
        /// </summary>
        public string TaxTypeId { get; set; }
        
        /// <summary>
        /// Tax type code (typically WHT)
        /// </summary>
        public string TaxTypeCode { get; set; }
        
        /// <summary>
        /// Specific WHT tax rate ID used
        /// </summary>
        public string TaxRateId { get; set; }
        
        /// <summary>
        /// Type of income for WHT
        /// </summary>
        public string IncomeType { get; set; }
        
        /// <summary>
        /// Liability account ID for recording WHT
        /// </summary>
        public string WhtLiabilityAccountId { get; set; }
        
        /// <summary>
        /// Calculation notes or messages
        /// </summary>
        public string Notes { get; set; }
    }

    /// <summary>
    /// DTO for tax transaction information
    /// </summary>
    public class TaxTransactionDto
    {
        /// <summary>
        /// Unique identifier for the tax transaction
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Tax type ID
        /// </summary>
        public string TaxTypeId { get; set; }
        
        /// <summary>
        /// Tax type code (e.g., VAT, WHT)
        /// </summary>
        public string TaxTypeCode { get; set; }
        
        /// <summary>
        /// Tax rate ID
        /// </summary>
        public string TaxRateId { get; set; }
        
        /// <summary>
        /// Applied tax rate
        /// </summary>
        public decimal AppliedRate { get; set; }
        
        /// <summary>
        /// Taxable amount
        /// </summary>
        public decimal TaxableAmount { get; set; }
        
        /// <summary>
        /// Tax amount
        /// </summary>
        public decimal TaxAmount { get; set; }
        
        /// <summary>
        /// Transaction date
        /// </summary>
        public DateTime TransactionDate { get; set; }
        
        /// <summary>
        /// Description of the transaction
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Reference to related source transaction
        /// </summary>
        public string SourceTransactionId { get; set; }
        
        /// <summary>
        /// Source transaction type
        /// </summary>
        public string SourceTransactionType { get; set; }
        
        /// <summary>
        /// Financial period ID
        /// </summary>
        public string FinancialPeriodId { get; set; }
        
        /// <summary>
        /// Journal entry ID for this tax transaction
        /// </summary>
        public string JournalEntryId { get; set; }
        
        /// <summary>
        /// Whether the tax is paid/settled
        /// </summary>
        public bool IsSettled { get; set; }
        
        /// <summary>
        /// Settlement date if settled
        /// </summary>
        public DateTime? SettlementDate { get; set; }
        
        /// <summary>
        /// Customer or vendor ID related to this transaction
        /// </summary>
        public string PartyId { get; set; }
        
        /// <summary>
        /// Party name (customer or vendor)
        /// </summary>
        public string PartyName { get; set; }
        
        /// <summary>
        /// Party tax ID number
        /// </summary>
        public string PartyTaxId { get; set; }
    }

    /// <summary>
    /// DTO for creating a new tax transaction
    /// </summary>
    public class CreateTaxTransactionDto
    {
        /// <summary>
        /// Tax type ID
        /// </summary>
        public string TaxTypeId { get; set; }
        
        /// <summary>
        /// Tax rate ID
        /// </summary>
        public string TaxRateId { get; set; }
        
        /// <summary>
        /// Applied tax rate
        /// </summary>
        public decimal AppliedRate { get; set; }
        
        /// <summary>
        /// Taxable amount
        /// </summary>
        public decimal TaxableAmount { get; set; }
        
        /// <summary>
        /// Tax amount
        /// </summary>
        public decimal TaxAmount { get; set; }
        
        /// <summary>
        /// Transaction date
        /// </summary>
        public DateTime TransactionDate { get; set; }
        
        /// <summary>
        /// Description of the transaction
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Reference to related source transaction
        /// </summary>
        public string SourceTransactionId { get; set; }
        
        /// <summary>
        /// Source transaction type
        /// </summary>
        public string SourceTransactionType { get; set; }
        
        /// <summary>
        /// Financial period ID
        /// </summary>
        public string FinancialPeriodId { get; set; }
        
        /// <summary>
        /// Customer or vendor ID related to this transaction
        /// </summary>
        public string PartyId { get; set; }
        
        /// <summary>
        /// Party name (customer or vendor)
        /// </summary>
        public string PartyName { get; set; }
        
        /// <summary>
        /// Party tax ID number
        /// </summary>
        public string PartyTaxId { get; set; }
        
        /// <summary>
        /// Whether to automatically create journal entries
        /// </summary>
        public bool CreateJournalEntry { get; set; } = true;
    }

    /// <summary>
    /// DTO for tax report request
    /// </summary>
    public class TaxReportRequestDto
    {
        /// <summary>
        /// Tax type ID to report on (null for all tax types)
        /// </summary>
        public string TaxTypeId { get; set; }
        
        /// <summary>
        /// Start date for the report period
        /// </summary>
        public DateTime StartDate { get; set; }
        
        /// <summary>
        /// End date for the report period
        /// </summary>
        public DateTime EndDate { get; set; }
        
        /// <summary>
        /// Financial period ID (alternative to date range)
        /// </summary>
        public string FinancialPeriodId { get; set; }
        
        /// <summary>
        /// Whether to include only settled transactions
        /// </summary>
        public bool? IsSettled { get; set; }
        
        /// <summary>
        /// How to group the report data (by date, tax type, etc.)
        /// </summary>
        public string GroupBy { get; set; }
        
        /// <summary>
        /// Report format (detailed or summary)
        /// </summary>
        public string Format { get; set; }
    }

    /// <summary>
    /// DTO for tax report result
    /// </summary>
    public class TaxReportDto
    {
        /// <summary>
        /// Report title
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Report period description
        /// </summary>
        public string PeriodDescription { get; set; }
        
        /// <summary>
        /// Start date of the report period
        /// </summary>
        public DateTime StartDate { get; set; }
        
        /// <summary>
        /// End date of the report period
        /// </summary>
        public DateTime EndDate { get; set; }
        
        /// <summary>
        /// Date the report was generated
        /// </summary>
        public DateTime GeneratedDate { get; set; }
        
        /// <summary>
        /// Summaries by tax type
        /// </summary>
        public List<TaxReportSummaryDto> Summaries { get; set; } = new List<TaxReportSummaryDto>();
        
        /// <summary>
        /// Details of tax transactions
        /// </summary>
        public List<TaxTransactionDto> Details { get; set; } = new List<TaxTransactionDto>();
        
        /// <summary>
        /// Total taxable amount across all tax types
        /// </summary>
        public decimal TotalTaxableAmount { get; set; }
        
        /// <summary>
        /// Total tax amount across all tax types
        /// </summary>
        public decimal TotalTaxAmount { get; set; }
        
        /// <summary>
        /// Total settled tax amount
        /// </summary>
        public decimal TotalSettledAmount { get; set; }
        
        /// <summary>
        /// Total outstanding tax amount
        /// </summary>
        public decimal TotalOutstandingAmount { get; set; }
    }

    /// <summary>
    /// DTO for tax report summary by tax type
    /// </summary>
    public class TaxReportSummaryDto
    {
        /// <summary>
        /// Tax type ID
        /// </summary>
        public string TaxTypeId { get; set; }
        
        /// <summary>
        /// Tax type code (e.g., VAT, WHT)
        /// </summary>
        public string TaxTypeCode { get; set; }
        
        /// <summary>
        /// Tax type name
        /// </summary>
        public string TaxTypeName { get; set; }
        
        /// <summary>
        /// Total taxable amount for this tax type
        /// </summary>
        public decimal TaxableAmount { get; set; }
        
        /// <summary>
        /// Total tax amount for this tax type
        /// </summary>
        public decimal TaxAmount { get; set; }
        
        /// <summary>
        /// Total settled amount for this tax type
        /// </summary>
        public decimal SettledAmount { get; set; }
        
        /// <summary>
        /// Total outstanding amount for this tax type
        /// </summary>
        public decimal OutstandingAmount { get; set; }
        
        /// <summary>
        /// Number of transactions for this tax type
        /// </summary>
        public int TransactionCount { get; set; }
    }

    /// <summary>
    /// Enum representing tax direction (input or output)
    /// </summary>
    public enum TaxDirection
    {
        /// <summary>
        /// Input tax (paid to vendors)
        /// </summary>
        Input = 1,
        
        /// <summary>
        /// Output tax (collected from customers)
        /// </summary>
        Output = 2,
        
        /// <summary>
        /// Both input and output
        /// </summary>
        Both = 3
    }
}
