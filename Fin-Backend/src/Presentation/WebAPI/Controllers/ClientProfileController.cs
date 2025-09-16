using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using FinTech.Application.Services;
using FinTech.Application.DTOs.ClientPortal;
using FinTech.Domain.Entities.ClientPortal;
using FinTech.Domain.Entities.Security;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace FinTech.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/client/profile")]
    public class ClientProfileController : ControllerBase
    {
        private readonly IClientProfileService _profileService;
        private readonly ILogger<ClientProfileController> _logger;

        public ClientProfileController(IClientProfileService profileService, ILogger<ClientProfileController> logger)
        {
            _profileService = profileService;
            _logger = logger;
        }

        // Profile Management
        [HttpGet]
        public async Task<ActionResult<ClientPortalProfile>> GetClientProfile()
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var profile = await _profileService.GetClientProfileAsync(customerId);
                return Ok(profile);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client profile");
                return StatusCode(500, "An error occurred while retrieving your profile");
            }
        }

        [HttpPut]
        public async Task<ActionResult<ClientPortalProfile>> UpdateClientProfile([FromBody] UpdateProfileDto profileDto)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var profile = await _profileService.UpdateClientProfileAsync(profileDto, customerId);
                return Ok(profile);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client profile");
                return StatusCode(500, "An error occurred while updating your profile");
            }
        }

        [HttpPost("picture")]
        public async Task<ActionResult> UpdateProfilePicture(IFormFile file)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }
                
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Invalid file type. Only JPG, JPEG, and PNG files are allowed.");
                }
                
                // Validate file size
                if (file.Length > 5 * 1024 * 1024) // 5 MB limit
                {
                    return BadRequest("File size exceeds the 5 MB limit.");
                }
                
                // Read file content
                byte[] fileContent;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileContent = memoryStream.ToArray();
                }
                
                var pictureDto = new ProfilePictureDto
                {
                    FileName = file.FileName,
                    FileContent = fileContent
                };
                
                var result = await _profileService.UpdateProfilePictureAsync(pictureDto, customerId);
                
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile picture");
                return StatusCode(500, "An error occurred while updating your profile picture");
            }
        }

        [HttpDelete("picture")]
        public async Task<ActionResult> DeleteProfilePicture()
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var result = await _profileService.DeleteProfilePictureAsync(customerId);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting profile picture");
                return StatusCode(500, "An error occurred while deleting your profile picture");
            }
        }

        // Preference Management
        [HttpGet("notifications")]
        public async Task<ActionResult<NotificationPreferences>> GetNotificationPreferences()
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var preferences = await _profileService.GetNotificationPreferencesAsync(customerId);
                return Ok(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification preferences");
                return StatusCode(500, "An error occurred while retrieving your notification preferences");
            }
        }

        [HttpPut("notifications")]
        public async Task<ActionResult<NotificationPreferences>> UpdateNotificationPreferences([FromBody] NotificationPreferencesDto preferencesDto)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var preferences = await _profileService.UpdateNotificationPreferencesAsync(preferencesDto, customerId);
                return Ok(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification preferences");
                return StatusCode(500, "An error occurred while updating your notification preferences");
            }
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardPreferences>> GetDashboardPreferences()
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var preferences = await _profileService.GetDashboardPreferencesAsync(customerId);
                return Ok(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard preferences");
                return StatusCode(500, "An error occurred while retrieving your dashboard preferences");
            }
        }

        [HttpPut("dashboard")]
        public async Task<ActionResult<DashboardPreferences>> UpdateDashboardPreferences([FromBody] DashboardPreferencesDto preferencesDto)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var preferences = await _profileService.UpdateDashboardPreferencesAsync(preferencesDto, customerId);
                return Ok(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dashboard preferences");
                return StatusCode(500, "An error occurred while updating your dashboard preferences");
            }
        }

        // Security Settings
        [HttpGet("security")]
        public async Task<ActionResult<SecurityPreferences>> GetSecurityPreferences()
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var preferences = await _profileService.GetSecurityPreferencesAsync(customerId);
                return Ok(preferences);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security preferences");
                return StatusCode(500, "An error occurred while retrieving your security preferences");
            }
        }

        [HttpPut("security")]
        public async Task<ActionResult<SecurityPreferences>> UpdateSecurityPreferences([FromBody] SecurityPreferencesDto preferencesDto)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                
                // Add IP and user agent for security activity logging
                preferencesDto.IpAddress = GetClientIpAddress();
                preferencesDto.UserAgent = GetUserAgent();
                
                var preferences = await _profileService.UpdateSecurityPreferencesAsync(preferencesDto, customerId);
                return Ok(preferences);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating security preferences");
                return StatusCode(500, "An error occurred while updating your security preferences");
            }
        }

        [HttpGet("security/activities")]
        public async Task<ActionResult<IEnumerable<SecurityActivity>>> GetRecentSecurityActivities([FromQuery] int count = 10)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var activities = await _profileService.GetRecentSecurityActivitiesAsync(customerId, count);
                return Ok(activities);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security activities");
                return StatusCode(500, "An error occurred while retrieving your security activities");
            }
        }

        // Document Management
        [HttpGet("documents")]
        public async Task<ActionResult<IEnumerable<ClientDocument>>> GetClientDocuments()
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var documents = await _profileService.GetClientDocumentsAsync(customerId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client documents");
                return StatusCode(500, "An error occurred while retrieving your documents");
            }
        }

        [HttpPost("documents")]
        public async Task<ActionResult<ClientDocument>> UploadDocument(IFormFile file, [FromForm] string documentType, [FromForm] string documentName, [FromForm] string description)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }
                
                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Invalid file type. Only PDF, JPG, JPEG, PNG, DOC, and DOCX files are allowed.");
                }
                
                // Validate file size
                if (file.Length > 10 * 1024 * 1024) // 10 MB limit
                {
                    return BadRequest("File size exceeds the 10 MB limit.");
                }
                
                // Read file content
                byte[] fileContent;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileContent = memoryStream.ToArray();
                }
                
                var documentDto = new DocumentUploadDto
                {
                    DocumentType = documentType,
                    DocumentName = documentName ?? file.FileName,
                    FileName = file.FileName,
                    FileSize = file.Length,
                    FileContent = fileContent,
                    Description = description
                };
                
                var document = await _profileService.UploadDocumentAsync(documentDto, customerId);
                
                return CreatedAtAction(nameof(GetDocument), new { documentId = document.Id }, document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                return StatusCode(500, "An error occurred while uploading your document");
            }
        }

        [HttpDelete("documents/{documentId}")]
        public async Task<ActionResult> DeleteDocument(Guid documentId)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var result = await _profileService.DeleteDocumentAsync(documentId, customerId);
                return Ok(new { success = result });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex.Message);
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document");
                return StatusCode(500, "An error occurred while deleting your document");
            }
        }

        [HttpGet("documents/{documentId}")]
        public async Task<ActionResult<ClientDocument>> GetDocument(Guid documentId)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var document = await _profileService.GetDocumentAsync(documentId, customerId);
                return Ok(document);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex.Message);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document");
                return StatusCode(500, "An error occurred while retrieving your document");
            }
        }

        // Activity History
        [HttpGet("activities")]
        public async Task<ActionResult<IEnumerable<ClientPortalActivity>>> GetClientActivities([FromQuery] ActivityHistoryRequestDto requestDto)
        {
            try
            {
                // Set default values if not provided
                if (requestDto.Page <= 0) requestDto.Page = 1;
                if (requestDto.PageSize <= 0) requestDto.PageSize = 20;
                
                var customerId = GetCustomerIdFromClaims();
                var activities = await _profileService.GetClientActivitiesAsync(requestDto, customerId);
                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity history");
                return StatusCode(500, "An error occurred while retrieving your activity history");
            }
        }

        // Helper methods
        private Guid GetCustomerIdFromClaims()
        {
            var customerId = User.Claims.FirstOrDefault(c => c.Type == "CustomerId")?.Value;
            if (string.IsNullOrEmpty(customerId))
            {
                throw new UnauthorizedAccessException("Customer ID not found in claims");
            }
            return Guid.Parse(customerId);
        }

        private string GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private string GetUserAgent()
        {
            return HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown";
        }
    }
}