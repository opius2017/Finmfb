using System;

namespace FinTech.Core.Application.DTOs.Currency
{
    /// <summary>
    /// DTO for currency conversion results
    /// </summary>
    public class ConversionResultDto
    {
        /// <summary>
        /// Original amount in source currency
        /// </summary>
        public decimal OriginalAmount { get; set; }
        
        /// <summary>
        /// Source currency code
        /// </summary>
        public string FromCurrencyCode { get; set; }
        
        /// <summary>
        /// Converted amount in target currency
        /// </summary>
        public decimal ConvertedAmount { get; set; }
        
        /// <summary>
        /// Target currency code
        /// </summary>
        public string ToCurrencyCode { get; set; }
        
        /// <summary>
        /// Exchange rate used for conversion
        /// </summary>
        public decimal ExchangeRate { get; set; }
        
        /// <summary>
        /// Date of the exchange rate used
        /// </summary>
        public DateTime ConversionDate { get; set; }
    }
}
