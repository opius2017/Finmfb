using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using FinTech.Application.Services;
using FinTech.Application.DTOs.ClientPortal;
using FinTech.Domain.Entities.ClientPortal;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace FinTech.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/client/support")]
    public class ClientSupportController : ControllerBase
    {
        private readonly IClientSupportService _supportService;
        private readonly ILogger<ClientSupportController> _logger;

        public ClientSupportController(IClientSupportService supportService, ILogger<ClientSupportController> logger)
        {
            _supportService = supportService;
            _logger = logger;
        }

        // Support Tickets
        [HttpGet("tickets")]
        public async Task<ActionResult<IEnumerable<ClientSupportTicket>>> GetClientTickets()
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var tickets = await _supportService.GetClientTicketsAsync(customerId);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving support tickets");
                return StatusCode(500, "An error occurred while retrieving your support tickets");
            }
        }

        [HttpGet("tickets/{ticketId}")]
        public async Task<ActionResult<ClientSupportTicket>> GetTicketDetails(Guid ticketId)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var ticket = await _supportService.GetTicketDetailsAsync(ticketId, customerId);
                return Ok(ticket);
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
                _logger.LogError(ex, "Error retrieving support ticket details");
                return StatusCode(500, "An error occurred while retrieving the support ticket details");
            }
        }

        [HttpPost("tickets")]
        public async Task<ActionResult<ClientSupportTicket>> CreateSupportTicket([FromBody] SupportTicketDto ticketDto)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var ticket = await _supportService.CreateSupportTicketAsync(ticketDto, customerId);
                return CreatedAtAction(nameof(GetTicketDetails), new { ticketId = ticket.Id }, ticket);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating support ticket");
                return StatusCode(500, "An error occurred while creating the support ticket");
            }
        }

        [HttpPut("tickets/{ticketId}/status")]
        public async Task<ActionResult> UpdateTicketStatus(Guid ticketId, [FromBody] TicketStatusUpdateDto statusUpdateDto)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var result = await _supportService.UpdateTicketStatusAsync(ticketId, statusUpdateDto.Status, customerId);
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
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating support ticket status");
                return StatusCode(500, "An error occurred while updating the support ticket status");
            }
        }

        [HttpPut("tickets/{ticketId}/close")]
        public async Task<ActionResult> CloseTicket(Guid ticketId)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var result = await _supportService.CloseTicketAsync(ticketId, customerId);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing support ticket");
                return StatusCode(500, "An error occurred while closing the support ticket");
            }
        }

        [HttpPut("tickets/{ticketId}/reopen")]
        public async Task<ActionResult> ReopenTicket(Guid ticketId)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var result = await _supportService.ReopenTicketAsync(ticketId, customerId);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reopening support ticket");
                return StatusCode(500, "An error occurred while reopening the support ticket");
            }
        }

        // Support Messages
        [HttpGet("tickets/{ticketId}/messages")]
        public async Task<ActionResult<IEnumerable<ClientSupportMessage>>> GetTicketMessages(Guid ticketId)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var messages = await _supportService.GetTicketMessagesAsync(ticketId, customerId);
                return Ok(messages);
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
                _logger.LogError(ex, "Error retrieving support ticket messages");
                return StatusCode(500, "An error occurred while retrieving the support ticket messages");
            }
        }

        [HttpPost("tickets/{ticketId}/messages")]
        public async Task<ActionResult<ClientSupportMessage>> AddTicketMessage(Guid ticketId, [FromBody] TicketMessageDto messageDto)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var message = await _supportService.AddTicketMessageAsync(messageDto, ticketId, customerId);
                return CreatedAtAction(nameof(GetTicketMessages), new { ticketId }, message);
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
                _logger.LogError(ex, "Error adding message to support ticket");
                return StatusCode(500, "An error occurred while adding the message to the support ticket");
            }
        }

        [HttpPut("messages/{messageId}/read")]
        public async Task<ActionResult> MarkMessageAsRead(Guid messageId)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var result = await _supportService.MarkMessageAsReadAsync(messageId, customerId);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message as read");
                return StatusCode(500, "An error occurred while marking the message as read");
            }
        }

        // Support Attachments
        [HttpPost("tickets/{ticketId}/attachments")]
        public async Task<ActionResult<ClientSupportAttachment>> AddTicketAttachment(Guid ticketId, IFormFile file)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }
                
                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".txt", ".xls", ".xlsx" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Invalid file type. Allowed file types: PDF, JPG, JPEG, PNG, DOC, DOCX, TXT, XLS, XLSX");
                }
                
                // Validate file size (10 MB limit)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest("File size exceeds the 10 MB limit");
                }
                
                // Read file content
                byte[] fileContent;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileContent = memoryStream.ToArray();
                }
                
                var attachmentDto = new TicketAttachmentDto
                {
                    FileName = file.FileName,
                    FileSize = file.Length,
                    FileContent = fileContent
                };
                
                var attachment = await _supportService.AddTicketAttachmentAsync(attachmentDto, ticketId, customerId);
                
                return CreatedAtAction(nameof(GetTicketDetails), new { ticketId }, attachment);
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
                _logger.LogError(ex, "Error adding attachment to support ticket");
                return StatusCode(500, "An error occurred while adding the attachment to the support ticket");
            }
        }

        [HttpGet("attachments/{attachmentId}")]
        public async Task<ActionResult<ClientSupportAttachment>> GetTicketAttachment(Guid attachmentId)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var attachment = await _supportService.GetTicketAttachmentAsync(attachmentId, customerId);
                return Ok(attachment);
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
                _logger.LogError(ex, "Error retrieving support ticket attachment");
                return StatusCode(500, "An error occurred while retrieving the support ticket attachment");
            }
        }

        [HttpDelete("attachments/{attachmentId}")]
        public async Task<ActionResult> DeleteTicketAttachment(Guid attachmentId)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var result = await _supportService.DeleteTicketAttachmentAsync(attachmentId, customerId);
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
                _logger.LogError(ex, "Error deleting support ticket attachment");
                return StatusCode(500, "An error occurred while deleting the support ticket attachment");
            }
        }

        // Knowledge Base
        [HttpGet("knowledge-base")]
        public async Task<ActionResult<IEnumerable<KnowledgeBaseArticle>>> GetKnowledgeBaseArticles([FromQuery] string category = null)
        {
            try
            {
                var articles = await _supportService.GetKnowledgeBaseArticlesAsync(category);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving knowledge base articles");
                return StatusCode(500, "An error occurred while retrieving knowledge base articles");
            }
        }

        [HttpGet("knowledge-base/{articleId}")]
        public async Task<ActionResult<KnowledgeBaseArticle>> GetKnowledgeBaseArticle(Guid articleId)
        {
            try
            {
                var article = await _supportService.GetKnowledgeBaseArticleAsync(articleId);
                return Ok(article);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving knowledge base article");
                return StatusCode(500, "An error occurred while retrieving the knowledge base article");
            }
        }

        [HttpGet("knowledge-base/search")]
        public async Task<ActionResult<IEnumerable<KnowledgeBaseArticle>>> SearchKnowledgeBase([FromQuery] string query)
        {
            try
            {
                var articles = await _supportService.SearchKnowledgeBaseAsync(query);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching knowledge base");
                return StatusCode(500, "An error occurred while searching the knowledge base");
            }
        }

        [HttpGet("knowledge-base/categories")]
        public async Task<ActionResult<IEnumerable<KnowledgeBaseCategory>>> GetKnowledgeBaseCategories()
        {
            try
            {
                var categories = await _supportService.GetKnowledgeBaseCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving knowledge base categories");
                return StatusCode(500, "An error occurred while retrieving knowledge base categories");
            }
        }

        // Frequently Asked Questions
        [HttpGet("faqs")]
        public async Task<ActionResult<IEnumerable<FrequentlyAskedQuestion>>> GetFAQs([FromQuery] string category = null)
        {
            try
            {
                var faqs = await _supportService.GetFAQsAsync(category);
                return Ok(faqs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving FAQs");
                return StatusCode(500, "An error occurred while retrieving FAQs");
            }
        }

        [HttpGet("faqs/{faqId}")]
        public async Task<ActionResult<FrequentlyAskedQuestion>> GetFAQ(Guid faqId)
        {
            try
            {
                var faq = await _supportService.GetFAQAsync(faqId);
                return Ok(faq);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving FAQ");
                return StatusCode(500, "An error occurred while retrieving the FAQ");
            }
        }

        [HttpGet("faqs/search")]
        public async Task<ActionResult<IEnumerable<FrequentlyAskedQuestion>>> SearchFAQs([FromQuery] string query)
        {
            try
            {
                var faqs = await _supportService.SearchFAQsAsync(query);
                return Ok(faqs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching FAQs");
                return StatusCode(500, "An error occurred while searching the FAQs");
            }
        }

        [HttpGet("faqs/categories")]
        public async Task<ActionResult<IEnumerable<string>>> GetFAQCategories()
        {
            try
            {
                var categories = await _supportService.GetFAQCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving FAQ categories");
                return StatusCode(500, "An error occurred while retrieving FAQ categories");
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
    }
}