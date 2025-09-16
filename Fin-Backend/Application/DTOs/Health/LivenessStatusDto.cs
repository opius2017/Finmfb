using System;

namespace FinTech.Core.Application.DTOs.Health
{
    /// <summary>
    /// Liveness status data transfer object
    /// </summary>
    public class LivenessStatusDto
    {
        /// <summary>
        /// Status of the system
        /// </summary>
        /// <example>Healthy</example>
        public string Status { get; set; }
        
        /// <summary>
        /// Whether the system is alive
        /// </summary>
        /// <example>true</example>
        public bool IsAlive { get; set; }
        
        /// <summary>
        /// Message describing the liveness status
        /// </summary>
        /// <example>System is alive</example>
        public string Message { get; set; }
        
        /// <summary>
        /// Timestamp of the liveness check
        /// </summary>
        /// <example>2025-09-16T10:30:00Z</example>
        public DateTime Timestamp { get; set; }
    }
}