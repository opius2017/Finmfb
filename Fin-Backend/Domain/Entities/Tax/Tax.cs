using System;
using System.Collections.Generic;
using FinTech.Domain.Common;

namespace FinTech.Domain.Entities.Tax
{
    /// <summary>
    /// Represents a type of tax in the system
    /// </summary>
    using FinTech.Domain.Entities.Common;
    public class TaxType : AuditableEntity
    {
        /// <summary>
        /// Tax type code (e.g., VAT, WHT, CIT)
        /// </summary>
    public string? Code { get; set; }
        
        /// <summary>
        /// Tax type name (e.g., Value Added Tax)
        /// </summary>
    public string? Name { get; set; }
        
        /// <summary>
        /// Description of the tax type
        /// </summary>
    public string? Description { get; set; }
        
        /// <summary>
        /// Whether the tax type is active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// The liability account where collected tax is recorded
        /// </summary>
    public string? LiabilityAccountId { get; set; }
        
        /// <summary>
        /// The receivable account where claimable tax is recorded
        /// </summary>
    public string? ReceivableAccountId { get; set; }
        
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
    public string? RegulatoryAuthority { get; set; }
        
        /// <summary>
        /// Navigation property for tax rates associated with this tax type
        /// </summary>
    public virtual ICollection<TaxRate>? TaxRates { get; set; }
        
        /// <summary>
        /// Navigation property for tax transactions associated with this tax type
        /// </summary>
    public virtual ICollection<TaxTransaction>? TaxTransactions { get; set; }
    }

    /// <summary>
    /// Represents a tax rate for a specific tax type
    /// </summary>
    public class TaxRate : AuditableEntity
    {
        /// <summary>
        /// Tax type ID this rate belongs to
        /// </summary>
    public string? TaxTypeId { get; set; }
        
        /// <summary>
        /// Rate name or description
        /// </summary>
    public string? Name { get; set; }
        
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
    public string? ApplicableCategory { get; set; }
        
        /// <summary>
        /// Navigation property for the tax type this rate belongs to
        /// </summary>
    public virtual TaxType? TaxType { get; set; }
        
        /// <summary>
        /// Navigation property for tax transactions using this rate
        /// </summary>
    public virtual ICollection<TaxTransaction>? TaxTransactions { get; set; }
    }

    /// <summary>
    /// Represents a tax exemption for a party (customer or vendor)
    /// </summary>
    public class TaxExemption : AuditableEntity
    {
        /// <summary>
        /// Tax type ID the exemption applies to (null for all tax types)
        /// </summary>
    public string? TaxTypeId { get; set; }
        
        /// <summary>
        /// Party ID (customer or vendor) the exemption applies to
        /// </summary>
    public string? PartyId { get; set; }
        
        /// <summary>
        /// Party type (customer or vendor)
        /// </summary>
    public string? PartyType { get; set; }
        
        /// <summary>
        /// Party tax identification number
        /// </summary>
    public string? PartyTaxId { get; set; }
        
        /// <summary>
        /// Reason for the exemption
        /// </summary>
    public string? Reason { get; set; }
        
        /// <summary>
        /// Certificate or reference number for the exemption
        /// </summary>
    public string? CertificateNumber { get; set; }
        
        /// <summary>
        /// Start date of the exemption validity
        /// </summary>
        public DateTime StartDate { get; set; }
        
        /// <summary>
        /// End date of the exemption validity (null for indefinite)
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Whether the exemption is currently active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Approval details for the exemption
        /// </summary>
    public string? ApprovalDetails { get; set; }
        
        /// <summary>
        /// Notes or comments about the exemption
        /// </summary>
    public string? Notes { get; set; }
        
        /// <summary>
        /// Navigation property for the tax type this exemption applies to
        /// </summary>
    public virtual TaxType? TaxType { get; set; }
    }

    /// <summary>
    /// Represents a tax transaction in the system
    /// </summary>
    public class TaxTransaction : AuditableEntity
    {
        /// <summary>
        /// Tax type ID
        /// </summary>
    public string? TaxTypeId { get; set; }
        
        /// <summary>
        /// Tax rate ID
        /// </summary>
    public string? TaxRateId { get; set; }
        
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
        
        /// <summary>
        /// Navigation property for the tax type this transaction belongs to
        /// </summary>
        public virtual TaxType TaxType { get; set; }
        
        /// <summary>
        /// Navigation property for the tax rate used in this transaction
        /// </summary>
        public virtual TaxRate TaxRate { get; set; }
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