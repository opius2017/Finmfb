using System;
using System.Collections.Generic;
using System.Linq;
using FinTech.Infrastructure.Jobs;
using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinTech.Controllers.Admin
{
    /// <summary>
    /// Controller for managing background jobs
    /// </summary>
    [ApiController]
    [Route("api/admin/background-jobs")]
    [Authorize(Roles = "Admin")]
    public class BackgroundJobsController : ControllerBase
    {
        private readonly ILogger<BackgroundJobsController> _logger;

        public BackgroundJobsController(ILogger<BackgroundJobsController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get all recurring jobs status
        /// </summary>
        [HttpGet("recurring")]
        [ProducesResponseType(typeof(List<RecurringJobDto>), StatusCodes.Status200OK)]
        public ActionResult<List<RecurringJobDto>> GetRecurringJobs()
        {
            try
            {
                using var connection = JobStorage.Current.GetConnection();
                var recurringJobs = connection.GetRecurringJobs();

                var jobDtos = recurringJobs.Select(job => new RecurringJobDto
                {
                    Id = job.Id,
                    Cron = job.Cron,
                    NextExecution = job.NextExecution,
                    LastExecution = job.LastExecution,
                    LastJobState = job.LastJobState,
                    CreatedAt = job.CreatedAt
                }).ToList();

                return Ok(jobDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recurring jobs");
                return StatusCode(500, new { message = "Error retrieving recurring jobs" });
            }
        }

        /// <summary>
        /// Trigger daily delinquency check immediately
        /// </summary>
        [HttpPost("trigger/delinquency-check")]
        [ProducesResponseType(typeof(JobTriggerResponse), StatusCodes.Status200OK)]
        public ActionResult<JobTriggerResponse> TriggerDelinquencyCheck()
        {
            try
            {
                _logger.LogInformation("Manually triggering delinquency check job by {User}", User.Identity?.Name);
                
                var jobId = DailyDelinquencyCheckJob.ExecuteNow();

                return Ok(new JobTriggerResponse
                {
                    JobId = jobId,
                    JobName = "Daily Delinquency Check",
                    TriggeredAt = DateTime.UtcNow,
                    TriggeredBy = User.Identity?.Name ?? "Unknown",
                    Message = "Delinquency check job has been queued for execution"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering delinquency check job");
                return StatusCode(500, new { message = "Error triggering job" });
            }
        }

        /// <summary>
        /// Trigger voucher expiry check immediately
        /// </summary>
        [HttpPost("trigger/voucher-expiry")]
        [ProducesResponseType(typeof(JobTriggerResponse), StatusCodes.Status200OK)]
        public ActionResult<JobTriggerResponse> TriggerVoucherExpiry()
        {
            try
            {
                _logger.LogInformation("Manually triggering voucher expiry job by {User}", User.Identity?.Name);
                
                var jobId = VoucherExpiryJob.ExecuteNow();

                return Ok(new JobTriggerResponse
                {
                    JobId = jobId,
                    JobName = "Voucher Expiry Check",
                    TriggeredAt = DateTime.UtcNow,
                    TriggeredBy = User.Identity?.Name ?? "Unknown",
                    Message = "Voucher expiry job has been queued for execution"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering voucher expiry job");
                return StatusCode(500, new { message = "Error triggering job" });
            }
        }

        /// <summary>
        /// Trigger monthly schedule generation immediately
        /// </summary>
        [HttpPost("trigger/schedule-generation")]
        [ProducesResponseType(typeof(JobTriggerResponse), StatusCodes.Status200OK)]
        public ActionResult<JobTriggerResponse> TriggerScheduleGeneration()
        {
            try
            {
                _logger.LogInformation("Manually triggering schedule generation job by {User}", User.Identity?.Name);
                
                var jobId = MonthlyDeductionScheduleJob.ExecuteNow();

                return Ok(new JobTriggerResponse
                {
                    JobId = jobId,
                    JobName = "Monthly Schedule Generation",
                    TriggeredAt = DateTime.UtcNow,
                    TriggeredBy = User.Identity?.Name ?? "Unknown",
                    Message = "Schedule generation job has been queued for execution"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering schedule generation job");
                return StatusCode(500, new { message = "Error triggering job" });
            }
        }

        /// <summary>
        /// Generate schedule for specific month
        /// </summary>
        [HttpPost("trigger/schedule-generation/{year}/{month}")]
        [ProducesResponseType(typeof(JobTriggerResponse), StatusCodes.Status200OK)]
        public ActionResult<JobTriggerResponse> TriggerScheduleGenerationForMonth(int year, int month)
        {
            try
            {
                if (month < 1 || month > 12)
                {
                    return BadRequest(new { message = "Invalid month. Must be between 1 and 12" });
                }

                if (year < 2020 || year > 2100)
                {
                    return BadRequest(new { message = "Invalid year" });
                }

                var userName = User.Identity?.Name ?? "Unknown";
                _logger.LogInformation(
                    "Manually triggering schedule generation for {Month}/{Year} by {User}", 
                    month, year, userName);
                
                var jobId = MonthlyDeductionScheduleJob.ExecuteForMonth(year, month, userName);

                return Ok(new JobTriggerResponse
                {
                    JobId = jobId,
                    JobName = $"Schedule Generation for {month}/{year}",
                    TriggeredAt = DateTime.UtcNow,
                    TriggeredBy = userName,
                    Message = $"Schedule generation for {month}/{year} has been queued for execution"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering schedule generation for {Month}/{Year}", month, year);
                return StatusCode(500, new { message = "Error triggering job" });
            }
        }

        /// <summary>
        /// Get job execution details
        /// </summary>
        [HttpGet("job/{jobId}")]
        [ProducesResponseType(typeof(JobDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<JobDetailsDto> GetJobDetails(string jobId)
        {
            try
            {
                using var connection = JobStorage.Current.GetConnection();
                var jobData = connection.GetJobData(jobId);

                if (jobData == null)
                {
                    return NotFound(new { message = "Job not found" });
                }

                var details = new JobDetailsDto
                {
                    JobId = jobId,
                    State = jobData.State,
                    CreatedAt = jobData.CreatedAt,
                    Job = jobData.Job?.ToString() ?? "Unknown"
                };

                return Ok(details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving job details for {JobId}", jobId);
                return StatusCode(500, new { message = "Error retrieving job details" });
            }
        }

        /// <summary>
        /// Delete a recurring job
        /// </summary>
        [HttpDelete("recurring/{jobId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteRecurringJob(string jobId)
        {
            try
            {
                _logger.LogWarning("Deleting recurring job {JobId} by {User}", jobId, User.Identity?.Name);
                
                RecurringJob.RemoveIfExists(jobId);

                return Ok(new { message = $"Recurring job {jobId} has been removed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting recurring job {JobId}", jobId);
                return StatusCode(500, new { message = "Error deleting job" });
            }
        }

        /// <summary>
        /// Re-register all recurring jobs
        /// </summary>
        [HttpPost("recurring/re-register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult ReRegisterRecurringJobs()
        {
            try
            {
                _logger.LogInformation("Re-registering all recurring jobs by {User}", User.Identity?.Name);
                
                JobConfiguration.RegisterRecurringJobs(HttpContext.RequestServices);

                return Ok(new { message = "All recurring jobs have been re-registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error re-registering recurring jobs");
                return StatusCode(500, new { message = "Error re-registering jobs" });
            }
        }
    }

    #region DTOs

    public class RecurringJobDto
    {
        public string Id { get; set; } = string.Empty;
        public string Cron { get; set; } = string.Empty;
        public DateTime? NextExecution { get; set; }
        public DateTime? LastExecution { get; set; }
        public string? LastJobState { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class JobTriggerResponse
    {
        public string JobId { get; set; } = string.Empty;
        public string JobName { get; set; } = string.Empty;
        public DateTime TriggeredAt { get; set; }
        public string TriggeredBy { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class JobDetailsDto
    {
        public string JobId { get; set; } = string.Empty;
        public string? State { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Job { get; set; } = string.Empty;
    }

    #endregion
}
