using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Health
{
    /// <summary>
    /// Health component data transfer object
    /// </summary>
    public class HealthComponentDto
    {
        /// <summary>
        /// Name of the health component
        /// </summary>
        /// <example>database_health_check</example>
        public string Name { get; set; }
        
        /// <summary>
        /// Status of the health component
        /// </summary>
        /// <example>Healthy</example>
        public string Status { get; set; }
        
        /// <summary>
        /// Description of the health component
        /// </summary>
        /// <example>Database connection is healthy</example>
        public string Description { get; set; }
        
        /// <summary>
        /// Time taken to check the health component in milliseconds
        /// </summary>
        /// <example>50.23</example>
        public double Duration { get; set; }
        
        /// <summary>
        /// Tags associated with the health component
        /// </summary>
        /// <example>["database", "sql", "ready"]</example>
        public List<string> Tags { get; set; }
        
        /// <summary>
        /// Additional data for the health component
        /// </summary>
        public Dictionary<string, string> Data { get; set; }
    }
}