namespace FinTech.Core.Application.DTOs.Security
{
    /// <summary>
    /// Security recommendation DTO
    /// </summary>
    public class SecurityRecommendationDto
    {
        /// <summary>
        /// Gets or sets the recommendation type
        /// </summary>
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the recommendation message
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the recommendation priority (High, Medium, Low)
        /// </summary>
        public string Priority { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the recommendation action URL
        /// </summary>
        public string ActionUrl { get; set; } = string.Empty;
    }
}
