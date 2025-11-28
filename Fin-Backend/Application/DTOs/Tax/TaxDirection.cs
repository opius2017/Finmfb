namespace FinTech.Core.Application.DTOs.Tax
{
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
