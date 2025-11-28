using System;

namespace FinTech.Core.Application.DTOs.Currency
{
    /// <summary>
    /// DTO for updating an existing exchange rate
    /// </summary>
    public class UpdateExchangeRateDto
    {
        /// <summary>
        /// Exchange rate ID
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// From currency code
        /// </summary>
        public string FromCurrencyCode { get; set; }
        
        /// <summary>
        /// To currency code
        /// </summary>
        public string ToCurrencyCode { get; set; }
        
        /// <summary>
        /// Exchange rate date
        /// </summary>
        public DateTime RateDate { get; set; }
        
        /// <summary>
        /// Exchange rate value (how much of ToCurrency equals 1 unit of FromCurrency)
        /// </summary>
        public decimal Rate { get; set; }
        
        /// <summary>
        /// Source of the exchange rate (e.g., "CBN", "Manual", "API")
        /// </summary>
        public string Source { get; set; }
    }
}
