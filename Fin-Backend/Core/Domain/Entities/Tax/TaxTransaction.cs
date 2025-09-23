using System;
using FinTech.Domain.Common;

namespace FinTech.Domain.Entities.Tax
{
    /// <summary>
    /// Represents a tax transaction in the system
    /// </summary>
    using FinTech.Domain.Entities.Common;
    public class TaxTransaction : AuditableEntity
    {
        /// <summary>
        /// The ID of the tax type
        /// </summary>
        public string TaxTypeId { get; set; }
        
        /// <summary>
        /// The ID of the tax rate
        /// </summary>
        public string TaxRateId { get; set; }
        
        /// <summary>
        /// The reference to the original transaction
        /// </summary>
        public string TransactionReference { get; set; }
        
        /// <summary>
        /// The date of the transaction
        /// </summary>
        public DateTime TransactionDate { get; set; }
        
        /// <summary>
        /// The financial period ID
        /// </summary>
        public string FinancialPeriodId { get; set; }
        
        /// <summary>
        /// The taxable amount (amount before tax)
        /// </summary>
        public decimal TaxableAmount { get; set; }
        
        /// <summary>
        /// The tax amount
        /// </summary>
        public decimal TaxAmount { get; set; }
        
        /// <summary>
        /// The category of the transaction
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// The ID of the party involved in the transaction
        /// </summary>
        public string PartyId { get; set; }
        
        /// <summary>
        /// The name of the party involved in the transaction
        /// </summary>
        public string PartyName { get; set; }
        
        /// <summary>
        /// The description of the transaction
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Whether the tax has been settled with the tax authority
        /// </summary>
        public bool IsSettled { get; set; }
        
        /// <summary>
        /// The date when the tax was settled
        /// </summary>
        public DateTime? SettlementDate { get; set; }
        
        /// <summary>
        /// The ID of the journal entry for this tax transaction
        /// </summary>
        public string JournalEntryId { get; set; }
    }
}