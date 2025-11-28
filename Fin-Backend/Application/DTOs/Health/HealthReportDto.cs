using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Health
{
    /// <summary>
    /// Health report data transfer object
    /// </summary>
    public class HealthReportDto
    {
        /// <summary>
        /// Overall health status
        /// </summary>
        /// <example>Healthy</example>
        public string Status { get; set; }
        
        /// <summary>
        /// Time taken to generate the health report in milliseconds
        /// </summary>
        /// <example>123.45</example>
        public double TotalDuration { get; set; }
        
        /// <summary>
        /// List of health components
        /// </summary>
        public List<HealthComponentDto> Components { get; set; }
    }
}
