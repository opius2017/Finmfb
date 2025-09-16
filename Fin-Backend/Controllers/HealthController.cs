using Fin_Backend.Infrastructure.Documentation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fin_Backend.Controllers
{
    /// <summary>
    /// Health check endpoints for monitoring system status
    /// </summary>
    [ApiVersion("1.0")]
    [SwaggerTag("Health and Status Monitoring")]
    public class HealthController : ApiControllerBase
    {
        private readonly HealthCheckService _healthCheckService;

        /// <summary>
        /// Initializes a new instance of the HealthController
        /// </summary>
        /// <param name="healthCheckService">The health check service</param>
        public HealthController(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        }

        /// <summary>
        /// Gets the overall health status of the system
        /// </summary>
        /// <remarks>
        /// This endpoint returns the consolidated health status of all components.
        /// 
        /// Sample request:
        ///
        ///     GET /api/health
        ///     
        /// </remarks>
        /// <returns>Health report with details of all system components</returns>
        /// <response code="200">Returns the health report</response>
        [HttpGet]
        [Route("health")]
        [SwaggerOperation(
            Summary = "Gets the overall health status of the system",
            Description = "Returns the consolidated health status of all components including database, Redis, and external APIs",
            OperationId = "Health_GetOverallStatus",
            Tags = new[] { "Health" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Overall health status", typeof(ApiResponse<HealthReportDto>))]
        public async Task<ActionResult<ApiResponse<HealthReportDto>>> GetHealthReport()
        {
            var report = await _healthCheckService.CheckHealthAsync();
            
            var healthReportDto = new HealthReportDto
            {
                Status = report.Status.ToString(),
                TotalDuration = report.TotalDuration.TotalMilliseconds,
                Components = report.Entries.Select(entry => new HealthComponentDto
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description,
                    Duration = entry.Value.Duration.TotalMilliseconds,
                    Tags = entry.Value.Tags.ToList(),
                    Data = entry.Value.Data.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.ToString() ?? string.Empty
                    )
                }).ToList()
            };
            
            return Success(healthReportDto);
        }

        /// <summary>
        /// Gets the ready status of the system for serving requests
        /// </summary>
        /// <remarks>
        /// This endpoint returns only the readiness status which is used by load balancers.
        /// 
        /// Sample request:
        ///
        ///     GET /api/health/ready
        ///     
        /// </remarks>
        /// <returns>Readiness status report</returns>
        /// <response code="200">The system is ready to serve requests</response>
        /// <response code="503">The system is not ready to serve requests</response>
        [HttpGet]
        [Route("health/ready")]
        [SwaggerOperation(
            Summary = "Gets the ready status of the system for serving requests",
            Description = "Returns the readiness status which is used by load balancers to determine if the system can serve requests",
            OperationId = "Health_GetReadyStatus",
            Tags = new[] { "Health" })]
        [SwaggerResponse(StatusCodes.Status200OK, "The system is ready", typeof(ApiResponse<ReadinessStatusDto>))]
        [SwaggerResponse(StatusCodes.Status503ServiceUnavailable, "The system is not ready", typeof(ApiResponse<ReadinessStatusDto>))]
        public async Task<ActionResult<ApiResponse<ReadinessStatusDto>>> GetReadyStatus()
        {
            var report = await _healthCheckService.CheckHealthAsync(
                predicate: reg => reg.Tags.Contains("ready"));
            
            var isReady = report.Status != HealthStatus.Unhealthy;
            
            var response = new ReadinessStatusDto
            {
                Status = report.Status.ToString(),
                IsReady = isReady,
                Message = isReady ? "System is ready to serve requests" : "System is not ready to serve requests"
            };
            
            if (isReady)
            {
                return Success(response);
            }
            else
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, Error<ReadinessStatusDto>("System is not ready to serve requests", response));
            }
        }

        /// <summary>
        /// Gets the alive status of the system
        /// </summary>
        /// <remarks>
        /// This endpoint is used for liveness probes in Kubernetes.
        /// 
        /// Sample request:
        ///
        ///     GET /api/health/live
        ///     
        /// </remarks>
        /// <returns>Liveness status</returns>
        /// <response code="200">The system is alive</response>
        /// <response code="503">The system is not alive</response>
        [HttpGet]
        [Route("health/live")]
        [SwaggerOperation(
            Summary = "Gets the alive status of the system",
            Description = "Returns the liveness status which is used by Kubernetes to determine if the system is alive",
            OperationId = "Health_GetLiveStatus",
            Tags = new[] { "Health" })]
        [SwaggerResponse(StatusCodes.Status200OK, "The system is alive", typeof(ApiResponse<LivenessStatusDto>))]
        public ActionResult<ApiResponse<LivenessStatusDto>> GetLiveStatus()
        {
            // For liveness, we simply return OK if the application is running
            // This endpoint should be very simple and not depend on external resources
            var response = new LivenessStatusDto
            {
                Status = "Healthy",
                IsAlive = true,
                Message = "System is alive",
                Timestamp = DateTime.UtcNow
            };
            
            return Success(response);
        }
    }

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