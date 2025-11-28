namespace FinTech.Core.Application.DTOs.Currency
{
    /// <summary>
    /// DTO for updating an existing currency
    /// </summary>
    public class UpdateCurrencyDto
    {
        /// <summary>
        /// ISO 4217 currency code (e.g., NGN, USD)
        /// </summary>
        public string Code { get; set; }
        
        /// <summary>
        /// Currency name (e.g., Nigerian Naira, US Dollar)
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Currency symbol (e.g., ₦, $)
        /// </summary>
        public string Symbol { get; set; }
        
        /// <summary>
        /// Number of decimal places typically used (e.g., 2 for NGN, USD)
        /// </summary>
        public int DecimalPlaces { get; set; }
        
        /// <summary>
        /// Whether the currency is currently active in the system
        /// </summary>
        public bool IsActive { get; set; }
    }
}
