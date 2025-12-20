using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.ClientPortal;
using FinTech.Core.Domain.Entities.Customers;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTech.Core.Application.Services.ClientPortal
{
    public interface IClientSupportService
    {
        // Support Tickets
        Task<IEnumerable<ClientSupportTicket>> GetClientTicketsAsync(string customerId);
        Task<ClientSupportTicket> GetTicketDetailsAsync(string ticketId, string customerId);
        Task<ClientSupportTicket> CreateSupportTicketAsync(SupportTicketDto ticketDto, string customerId);
        Task<bool> UpdateTicketStatusAsync(string ticketId, string status, string customerId);
        Task<bool> CloseTicketAsync(string ticketId, string customerId);
        Task<bool> ReopenTicketAsync(string ticketId, string customerId);
        
        // Support Messages
        Task<IEnumerable<ClientSupportMessage>> GetTicketMessagesAsync(string ticketId, string customerId);
        Task<ClientSupportMessage> AddTicketMessageAsync(TicketMessageDto messageDto, string ticketId, string customerId);
        Task<bool> MarkMessageAsReadAsync(string messageId, string customerId);
        
        // Support Attachments
        Task<ClientSupportAttachment> AddTicketAttachmentAsync(TicketAttachmentDto attachmentDto, string ticketId, string customerId);
        Task<ClientSupportAttachment> GetTicketAttachmentAsync(string attachmentId, string customerId);
        Task<bool> DeleteTicketAttachmentAsync(string attachmentId, string customerId);
        
        // Knowledge Base
        Task<IEnumerable<KnowledgeBaseArticle>> GetKnowledgeBaseArticlesAsync(string category = null);
        Task<KnowledgeBaseArticle> GetKnowledgeBaseArticleAsync(string articleId);
        Task<IEnumerable<KnowledgeBaseArticle>> SearchKnowledgeBaseAsync(string searchTerm);
        Task<IEnumerable<KnowledgeBaseCategory>> GetKnowledgeBaseCategoriesAsync();
        
        // Frequently Asked Questions
        Task<IEnumerable<FrequentlyAskedQuestion>> GetFAQsAsync(string category = null);
        Task<FrequentlyAskedQuestion> GetFAQAsync(string faqId);
        Task<IEnumerable<FrequentlyAskedQuestion>> SearchFAQsAsync(string searchTerm);
        Task<IEnumerable<string>> GetFAQCategoriesAsync();
    }

    public class ClientSupportService : IClientSupportService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ILogger<ClientSupportService> _logger;
        private readonly IFileStorageService _fileStorage;

        public ClientSupportService(
            IApplicationDbContext dbContext,
            ILogger<ClientSupportService> logger,
            IFileStorageService fileStorage)
        {
            _dbContext = dbContext;
            _logger = logger;
            _fileStorage = fileStorage;
        }

        // Support Tickets
        public async Task<IEnumerable<ClientSupportTicket>> GetClientTicketsAsync(string customerId)
        {
            try
            {
                return await _dbContext.ClientSupportTickets
                    .Where(t => t.CustomerId == customerId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving support tickets for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<ClientSupportTicket> GetTicketDetailsAsync(string ticketId, string customerId)
        {
            try
            {
                var ticket = await _dbContext.ClientSupportTickets
                    .Include(t => t.Messages)
                    .Include(t => t.Attachments)
                    .FirstOrDefaultAsync(t => t.Id == ticketId);
                
                if (ticket == null)
                {
                    throw new KeyNotFoundException($"Support ticket with ID {ticketId} not found.");
                }
                
                if (ticket.CustomerId != customerId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to view this ticket.");
                }
                
                return ticket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving support ticket {TicketId} for customer {CustomerId}", ticketId, customerId);
                throw;
            }
        }

        public async Task<ClientSupportTicket> CreateSupportTicketAsync(SupportTicketDto ticketDto, string customerId)
        {
            try
            {
                // Get customer information
                var customer = await _dbContext.Customers
                    .FirstOrDefaultAsync(c => c.Id == customerId);
                
                if (customer == null)
                {
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found.");
                }
                
                // Create ticket
                var ticket = new ClientSupportTicket
                {
                    CustomerId = customerId,
                    Subject = ticketDto.Subject,
                    Category = ticketDto.Category,
                    Priority = ticketDto.Priority,
                    Description = ticketDto.Description,
                    Status = "Open",
                    TicketNumber = GenerateTicketNumber(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.ClientSupportTickets.Add(ticket);
                
                // Create initial message
                var message = new ClientSupportMessage
                {
                    TicketId = ticket.Id,
                    CustomerId = customerId,
                    SenderName = $"{customer.FirstName} {customer.LastName}",
                    SenderType = "Customer",
                    Message = ticketDto.Description,
                    IsRead = true, // Read by customer
                    IsReadByStaff = false, // Not read by staff
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.ClientSupportMessages.Add(message);
                
                // Create activity log
                var activity = new ClientPortalActivity
                {
                    CustomerId = customerId,
                    ActivityType = "Support Ticket Created",
                    Description = $"Created support ticket: {ticketDto.Subject}",
                    ActivityDate = DateTime.UtcNow,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.ClientPortalActivities.Add(activity);
                
                await _dbContext.SaveChangesAsync();
                
                return ticket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating support ticket for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> UpdateTicketStatusAsync(string ticketId, string status, string customerId)
        {
            try
            {
                var ticket = await _dbContext.ClientSupportTickets
                    .FirstOrDefaultAsync(t => t.Id == ticketId);
                
                if (ticket == null)
                {
                    throw new KeyNotFoundException($"Support ticket with ID {ticketId} not found.");
                }
                
                if (ticket.CustomerId != customerId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to update this ticket.");
                }
                
                // Validate status
                var validStatuses = new[] { "Open", "In Progress", "Resolved", "Closed" };
                if (!validStatuses.Contains(status))
                {
                    throw new ArgumentException($"Invalid status: {status}. Valid statuses are: {string.Join(", ", validStatuses)}");
                }
                
                // Update ticket status
                ticket.Status = status;
                ticket.UpdatedAt = DateTime.UtcNow;
                
                // Create activity log
                var activity = new ClientPortalActivity
                {
                    CustomerId = customerId,
                    ActivityType = "Support Ticket Updated",
                    Description = $"Updated ticket status to {status}: {ticket.Subject}",
                    ActivityDate = DateTime.UtcNow,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.ClientPortalActivities.Add(activity);
                
                await _dbContext.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating support ticket {TicketId} status for customer {CustomerId}", ticketId, customerId);
                throw;
            }
        }

        public async Task<bool> CloseTicketAsync(string ticketId, string customerId)
        {
            return await UpdateTicketStatusAsync(ticketId, "Closed", customerId);
        }

        public async Task<bool> ReopenTicketAsync(string ticketId, string customerId)
        {
            return await UpdateTicketStatusAsync(ticketId, "Open", customerId);
        }

        // Support Messages
        public async Task<IEnumerable<ClientSupportMessage>> GetTicketMessagesAsync(string ticketId, string customerId)
        {
            try
            {
                // Verify ticket ownership
                var ticket = await _dbContext.ClientSupportTickets
                    .FirstOrDefaultAsync(t => t.Id == ticketId);
                
                if (ticket == null)
                {
                    throw new KeyNotFoundException($"Support ticket with ID {ticketId} not found.");
                }
                
                if (ticket.CustomerId != customerId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to view messages for this ticket.");
                }
                
                // Get messages
                var messages = await _dbContext.ClientSupportMessages
                    .Where(m => m.TicketId == ticketId)
                    .OrderBy(m => m.CreatedAt)
                    .ToListAsync();
                
                // Mark unread customer messages as read
                var unreadMessages = messages.Where(m => m.SenderType != "Customer" && !m.IsRead).ToList();
                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                    message.UpdatedAt = DateTime.UtcNow;
                }
                
                if (unreadMessages.Any())
                {
                    await _dbContext.SaveChangesAsync();
                }
                
                return messages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages for ticket {TicketId} for customer {CustomerId}", ticketId, customerId);
                throw;
            }
        }

        public async Task<ClientSupportMessage> AddTicketMessageAsync(TicketMessageDto messageDto, string ticketId, string customerId)
        {
            try
            {
                // Verify ticket ownership
                var ticket = await _dbContext.ClientSupportTickets
                    .FirstOrDefaultAsync(t => t.Id == ticketId);
                
                if (ticket == null)
                {
                    throw new KeyNotFoundException($"Support ticket with ID {ticketId} not found.");
                }
                
                if (ticket.CustomerId != customerId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to add messages to this ticket.");
                }
                
                // Get customer information
                var customer = await _dbContext.Customers
                    .FirstOrDefaultAsync(c => c.Id == customerId); // FinTech Best Practice: Convert Guid to string
                
                if (customer == null)
                {
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found.");
                }
                
                // Create message
                var message = new ClientSupportMessage
                {
                    TicketId = ticketId,
                    CustomerId = customerId,
                    SenderName = $"{customer.FirstName} {customer.LastName}",
                    SenderType = "Customer",
                    Message = messageDto.Message,
                    IsRead = true, // Read by customer
                    IsReadByStaff = false, // Not read by staff
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.ClientSupportMessages.Add(message);
                
                // Update ticket
                ticket.Status = "Open"; // Reopen ticket if it was closed
                ticket.UpdatedAt = DateTime.UtcNow;
                
                await _dbContext.SaveChangesAsync();
                
                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding message to ticket {TicketId} for customer {CustomerId}", ticketId, customerId);
                throw;
            }
        }

        public async Task<bool> MarkMessageAsReadAsync(string messageId, string customerId)
        {
            try
            {
                var message = await _dbContext.ClientSupportMessages
                    .Include(m => m.Ticket)
                    .FirstOrDefaultAsync(m => m.Id == messageId);
                
                if (message == null)
                {
                    throw new KeyNotFoundException($"Support message with ID {messageId} not found.");
                }
                
                if (message.Ticket.CustomerId != customerId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to access this message.");
                }
                
                // Only mark as read if not already read
                if (!message.IsRead)
                {
                    message.IsRead = true;
                    message.UpdatedAt = DateTime.UtcNow;
                    
                    await _dbContext.SaveChangesAsync();
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message {MessageId} as read for customer {CustomerId}", messageId, customerId);
                throw;
            }
        }

        // Support Attachments
        public async Task<ClientSupportAttachment> AddTicketAttachmentAsync(TicketAttachmentDto attachmentDto, string ticketId, string customerId)
        {
            try
            {
                // Verify ticket ownership
                var ticket = await _dbContext.ClientSupportTickets
                    .FirstOrDefaultAsync(t => t.Id == ticketId);
                
                if (ticket == null)
                {
                    throw new KeyNotFoundException($"Support ticket with ID {ticketId} not found.");
                }
                
                if (ticket.CustomerId != customerId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to add attachments to this ticket.");
                }
                
                // Upload file to storage
                string fileName = $"ticket-{ticketId}-{DateTime.UtcNow.Ticks}{Path.GetExtension(attachmentDto.FileName)}";
                string fileUrl = await _fileStorage.UploadFileAsync(attachmentDto.FileContent, fileName, "support-attachments", "support-attachments"); // FinTech Best Practice: Specify container name
                
                // Create attachment record
                var attachment = new ClientSupportAttachment
                {
                    TicketId = ticketId,
                    FileName = attachmentDto.FileName,
                    FileSize = attachmentDto.FileSize,
                    FileType = Path.GetExtension(attachmentDto.FileName).TrimStart('.'),
                    FileUrl = fileUrl,
                    UploadedBy = "Customer",
                    UploadDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.ClientSupportAttachments.Add(attachment);
                
                // Update ticket
                ticket.UpdatedAt = DateTime.UtcNow;
                
                await _dbContext.SaveChangesAsync();
                
                return attachment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding attachment to ticket {TicketId} for customer {CustomerId}", ticketId, customerId);
                throw;
            }
        }

        public async Task<ClientSupportAttachment> GetTicketAttachmentAsync(string attachmentId, string customerId)
        {
            try
            {
                var attachment = await _dbContext.ClientSupportAttachments
                    .Include(a => a.Ticket)
                    .FirstOrDefaultAsync(a => a.Id == attachmentId);
                
                if (attachment == null)
                {
                    throw new KeyNotFoundException($"Support attachment with ID {attachmentId} not found.");
                }
                
                if (attachment.Ticket.CustomerId != customerId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to access this attachment.");
                }
                
                return attachment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attachment {AttachmentId} for customer {CustomerId}", attachmentId, customerId);
                throw;
            }
        }

        public async Task<bool> DeleteTicketAttachmentAsync(string attachmentId, string customerId)
        {
            try
            {
                var attachment = await _dbContext.ClientSupportAttachments
                    .Include(a => a.Ticket)
                    .FirstOrDefaultAsync(a => a.Id == attachmentId);
                
                if (attachment == null)
                {
                    throw new KeyNotFoundException($"Support attachment with ID {attachmentId} not found.");
                }
                
                if (attachment.Ticket.CustomerId != customerId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to delete this attachment.");
                }
                
                // Only allow deletion if uploaded by the customer
                if (attachment.UploadedBy != "Customer")
                {
                    throw new InvalidOperationException("You can only delete attachments that you uploaded.");
                }
                
                // Delete the file from storage
                await _fileStorage.DeleteFileAsync(attachment.FileUrl, "support-attachments"); // FinTech Best Practice: Specify container name
                
                // Remove the attachment record
                _dbContext.ClientSupportAttachments.Remove(attachment);
                
                // Update ticket
                attachment.Ticket.UpdatedAt = DateTime.UtcNow;
                
                await _dbContext.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment {AttachmentId} for customer {CustomerId}", attachmentId, customerId);
                throw;
            }
        }

        // Knowledge Base
        public async Task<IEnumerable<KnowledgeBaseArticle>> GetKnowledgeBaseArticlesAsync(string category = null)
        {
            try
            {
                var query = _dbContext.KnowledgeBaseArticles
                    .Where(a => a.IsPublished);
                
                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(a => a.Category.Name == category);
                }
                
                return await query
                    .Include(a => a.Category)
                    .OrderByDescending(a => a.PublishedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving knowledge base articles");
                throw;
            }
        }

        public async Task<KnowledgeBaseArticle> GetKnowledgeBaseArticleAsync(string articleId)
        {
            try
            {
                var article = await _dbContext.KnowledgeBaseArticles
                    .Include(a => a.Category)
                    .FirstOrDefaultAsync(a => a.Id == articleId && a.IsPublished);
                
                if (article == null)
                {
                    throw new KeyNotFoundException($"Knowledge base article with ID {articleId} not found.");
                }
                
                // Increment view count
                article.ViewCount += 1;
                await _dbContext.SaveChangesAsync();
                
                return article;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving knowledge base article {ArticleId}", articleId);
                throw;
            }
        }

        public async Task<IEnumerable<KnowledgeBaseArticle>> SearchKnowledgeBaseAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                {
                    return await GetKnowledgeBaseArticlesAsync();
                }
                
                // Basic search implementation - in a production environment, consider using full-text search
                searchTerm = searchTerm.ToLower();
                
                return await _dbContext.KnowledgeBaseArticles
                    .Where(a => a.IsPublished &&
                           (a.Title.ToLower().Contains(searchTerm) ||
                            a.Content.ToLower().Contains(searchTerm)))
                    .Include(a => a.Category)
                    .OrderByDescending(a => a.PublishedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching knowledge base for {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<IEnumerable<KnowledgeBaseCategory>> GetKnowledgeBaseCategoriesAsync()
        {
            try
            {
                return await _dbContext.KnowledgeBaseCategories
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving knowledge base categories");
                throw;
            }
        }

        // Frequently Asked Questions
        public async Task<IEnumerable<FrequentlyAskedQuestion>> GetFAQsAsync(string category = null)
        {
            try
            {
                var query = _dbContext.FrequentlyAskedQuestions
                    .Where(f => f.IsPublished);
                
                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(f => f.Category == category);
                }
                
                return await query
                    .OrderBy(f => f.DisplayOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving FAQs");
                throw;
            }
        }

        public async Task<FrequentlyAskedQuestion> GetFAQAsync(string faqId)
        {
            try
            {
                var faq = await _dbContext.FrequentlyAskedQuestions
                    .FirstOrDefaultAsync(f => f.Id == faqId && f.IsPublished);
                
                if (faq == null)
                {
                    throw new KeyNotFoundException($"FAQ with ID {faqId} not found.");
                }
                
                return faq;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving FAQ {FaqId}", faqId);
                throw;
            }
        }

        public async Task<IEnumerable<FrequentlyAskedQuestion>> SearchFAQsAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                {
                    return await GetFAQsAsync();
                }
                
                // Basic search implementation
                searchTerm = searchTerm.ToLower();
                
                return await _dbContext.FrequentlyAskedQuestions
                    .Where(f => f.IsPublished &&
                           (f.Question.ToLower().Contains(searchTerm) ||
                            f.Answer.ToLower().Contains(searchTerm)))
                    .OrderBy(f => f.DisplayOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching FAQs for {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetFAQCategoriesAsync()
        {
            try
            {
                return await _dbContext.FrequentlyAskedQuestions
                    .Where(f => f.IsPublished)
                    .Select(f => f.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving FAQ categories");
                throw;
            }
        }

        // Helper method to generate ticket numbers
        private string GenerateTicketNumber()
        {
            return $"TKT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}
