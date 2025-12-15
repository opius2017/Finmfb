using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    // Support Ticket DTOs
    public class SupportTicketDto
    {
        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string Subject { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Description { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        public string Priority { get; set; }

        public List<TicketAttachmentDto> Attachments { get; set; }
    }

    public class TicketStatusUpdateDto
    {
        [Required]
        public string Status { get; set; }
    }

    public class TicketMessageDto
    {
        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string Message { get; set; }
    }

    public class TicketAttachmentDto
    {
        [Required]
        public string FileName { get; set; }

        [Required]
        public long FileSize { get; set; }

        [Required]
        public byte[] FileContent { get; set; }
    }

    // Support Knowledge Base DTOs
    public class KnowledgeBaseArticleDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Summary { get; set; }
        public string Category { get; set; }
        public string[] Tags { get; set; }
        public DateTime PublishedDate { get; set; }
        public int ViewCount { get; set; }
        public bool IsHelpful { get; set; }
        public int HelpfulCount { get; set; }
        public int NotHelpfulCount { get; set; }
    }

    public class KnowledgeBaseCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ArticleCount { get; set; }
        public int DisplayOrder { get; set; }
    }

    // FAQ DTOs
    public class FAQDto
    {
        public Guid Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public string Category { get; set; }
        public int DisplayOrder { get; set; }
    }

    // Mapping classes for domain entities to DTOs
    public class SupportTicketMappingProfile
    {
        // Maps ClientSupportTicket -> ClientSupportTicketDto
        public static ClientSupportTicketDto MapToDto(FinTech.Core.Domain.Entities.ClientPortal.ClientSupportTicket ticket)
        {
            if (ticket == null)
                return null;

            var dto = new ClientSupportTicketDto
            {
                Id = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                Subject = ticket.Subject,
                Description = ticket.Description,
                Category = ticket.Category,
                Priority = ticket.Priority,
                Status = ticket.Status,
                ClosedDate = ticket.ClosedDate,
                Resolution = ticket.Resolution,
                CustomerSatisfactionRating = ticket.CustomerSatisfactionRating,
                CreatedOn = ticket.CreatedAt,
                Messages = new List<ClientSupportMessageDto>()
            };

            if (ticket.Messages != null)
            {
                foreach (var message in ticket.Messages)
                {
                    dto.Messages.Add(new ClientSupportMessageDto
                    {
                        Id = message.Id,
                        Message = message.Message,
                        IsFromClient = message.SenderType == "Customer",
                        SenderName = message.SenderName,
                        IsRead = message.IsRead,
                        ReadAt = message.ReadAt,
                        CreatedOn = message.CreatedAt
                    });
                }
            }

            return dto;
        }

        // Maps ClientSupportMessage -> ClientSupportMessageDto
        public static ClientSupportMessageDto MapToDto(FinTech.Core.Domain.Entities.ClientPortal.ClientSupportMessage message)
        {
            if (message == null)
                return null;

            return new ClientSupportMessageDto
            {
                Id = message.Id,
                Message = message.Message,
                IsFromClient = message.SenderType == "Customer",
                SenderName = message.SenderName,
                IsRead = message.IsRead,
                ReadAt = message.ReadAt,
                CreatedOn = message.CreatedAt
            };
        }

        // Maps KnowledgeBaseArticle -> KnowledgeBaseArticleDto
        public static KnowledgeBaseArticleDto MapToDto(FinTech.Core.Domain.Entities.ClientPortal.KnowledgeBaseArticle article)
        {
            if (article == null)
                return null;

            return new KnowledgeBaseArticleDto
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                Summary = article.Summary,
                Category = article.Category?.Name,
                Tags = article.Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries),
                PublishedDate = article.PublishedDate ?? DateTime.MinValue,
                ViewCount = article.ViewCount,
                HelpfulCount = article.HelpfulCount,
                NotHelpfulCount = article.NotHelpfulCount
            };
        }

        // Maps KnowledgeBaseCategory -> KnowledgeBaseCategoryDto
        public static KnowledgeBaseCategoryDto MapToDto(FinTech.Core.Domain.Entities.ClientPortal.KnowledgeBaseCategory category)
        {
            if (category == null)
                return null;

            return new KnowledgeBaseCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder,
                ArticleCount = category.Articles?.Count ?? 0
            };
        }

        // Maps FrequentlyAskedQuestion -> FAQDto
        public static FAQDto MapToDto(FinTech.Core.Domain.Entities.ClientPortal.FrequentlyAskedQuestion faq)
        {
            if (faq == null)
                return null;

            return new FAQDto
            {
                Id = faq.Id,
                Question = faq.Question,
                Answer = faq.Answer,
                Category = faq.Category,
                DisplayOrder = faq.DisplayOrder
            };
        }
    }
}
