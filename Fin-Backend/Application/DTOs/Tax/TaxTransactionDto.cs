using System;

namespace FinTech.Core.Application.DTOs.Tax
{
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
}
