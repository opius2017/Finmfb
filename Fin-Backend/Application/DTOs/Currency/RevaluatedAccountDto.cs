namespace FinTech.Core.Application.DTOs.Currency
{
    /// <summary>
    /// DTO for revaluated account information
    /// </summary>
    public class RevaluatedAccountDto
    {
        /// <summary>
        /// Account ID
        /// </summary>
        public string AccountId { get; set; }
        
        /// <summary>
        /// Account number
        /// </summary>
        public string AccountNumber { get; set; }
        
        /// <summary>
        /// Account name
        /// </summary>
        public string AccountName { get; set; }
        
        /// <summary>
        /// Foreign currency balance
        /// </summary>
        public decimal ForeignCurrencyBalance { get; set; }
        
        /// <summary>
        /// Previous base currency value
        /// </summary>
        public decimal PreviousBaseCurrencyValue { get; set; }
        
        /// <summary>
        /// Current base currency value
        /// </summary>
        public decimal CurrentBaseCurrencyValue { get; set; }
        
        /// <summary>
        /// Unrealized gain or loss amount (positive for gain, negative for loss)
        /// </summary>
        public decimal RevaluationEffect { get; set; }
    }
}
