namespace FinTech.Core.Application.DTOs.Health
{
    /// <summary>
    /// Readiness status data transfer object
    /// </summary>
    public class ReadinessStatusDto
    {
        /// <summary>
        /// Status of the system
        /// </summary>
        /// <example>Healthy</example>
        public string Status { get; set; }
        
        /// <summary>
        /// Whether the system is ready to serve requests
        /// </summary>
        /// <example>true</example>
        public bool IsReady { get; set; }
        
        /// <summary>
        /// Message describing the readiness status
        /// </summary>
        /// <example>System is ready to serve requests</example>
        public string Message { get; set; }
    }
}
