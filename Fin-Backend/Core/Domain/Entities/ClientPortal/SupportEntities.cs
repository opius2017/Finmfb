using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Customers;

namespace FinTech.Core.Domain.Entities.ClientPortal
{
    public class ClientSupportTicket
    {
        [Key]
        public Guid Id { get; set; }
        
        public string ClientPortalProfileId { get; set; } = string.Empty;
        
        [Required]
        public Guid CustomerId { get; set; }
        
        [Required]
        [StringLength(20)]
        public string TicketNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Subject { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;
        
        public DateTime? ClosedDate { get; set; }
        
        [StringLength(1000)]
        public string? Resolution { get; set; }
        
        public int CustomerSatisfactionRating { get; set; }
        
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }
        
        public virtual ICollection<ClientSupportMessage> Messages { get; set; } = new List<ClientSupportMessage>();
        
        public virtual ICollection<ClientSupportAttachment> Attachments { get; set; } = new List<ClientSupportAttachment>();
        
        public DateTime CreatedDate { get; set; }
        
        public DateTime LastModifiedDate { get; set; }

        [NotMapped]
        public DateTime CreatedAt { get => CreatedDate; set => CreatedDate = value; }
        [NotMapped]
        public DateTime UpdatedAt { get => LastModifiedDate; set => LastModifiedDate = value; }
        [NotMapped]
        public DateTime CreatedOn { get => CreatedDate; set => CreatedDate = value; }
        [NotMapped]
        public DateTime LastModifiedOn { get => LastModifiedDate; set => LastModifiedDate = value; }
    }

    public class ClientSupportMessage
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid TicketId { get; set; }
        
        public Guid? CustomerId { get; set; }
        
        public Guid? StaffId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string SenderName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string SenderType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;
        
        public bool IsRead { get; set; }
        
        public bool IsReadByStaff { get; set; }
        
        public DateTime? ReadAt { get; set; }
        
        [ForeignKey("TicketId")]
        public virtual ClientSupportTicket? Ticket { get; set; }
        
        public DateTime CreatedDate { get; set; }
        
        public DateTime LastModifiedDate { get; set; }
        
        [NotMapped]
        public DateTime CreatedAt { get => CreatedDate; set => CreatedDate = value; }
        [NotMapped]
        public DateTime UpdatedAt { get => LastModifiedDate; set => LastModifiedDate = value; }
        [NotMapped]
        public DateTime CreatedOn { get => CreatedDate; set => CreatedDate = value; }
        [NotMapped]
        public DateTime LastModifiedOn { get => LastModifiedDate; set => LastModifiedDate = value; }
        [NotMapped]
        public Guid ClientSupportTicketId { get => TicketId; set => TicketId = value; }
    }

    public class ClientSupportAttachment
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid TicketId { get; set; }
        
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string FileType { get; set; } = string.Empty;
        
        [Required]
        public long FileSize { get; set; }
        
        [Required]
        [StringLength(500)]
        public string FileUrl { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string UploadedBy { get; set; } = string.Empty;
        
        public DateTime UploadDate { get; set; }
        
        [ForeignKey("TicketId")]
        public virtual ClientSupportTicket? Ticket { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }

    public class KnowledgeBaseCategory
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        public int DisplayOrder { get; set; }
        
        public virtual ICollection<KnowledgeBaseArticle> Articles { get; set; } = new List<KnowledgeBaseArticle>();
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }

    public class KnowledgeBaseArticle
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid CategoryId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Summary { get; set; }
        
        [StringLength(500)]
        public string? Tags { get; set; }
        
        public bool IsPublished { get; set; }
        
        public DateTime? PublishedDate { get; set; }
        
        public DateTime? LastUpdatedDate { get; set; }
        
        public Guid LastUpdatedBy { get; set; }
        
        public int ViewCount { get; set; }
        
        public int HelpfulCount { get; set; }
        
        public int NotHelpfulCount { get; set; }
        
        [ForeignKey("CategoryId")]
        public virtual KnowledgeBaseCategory? Category { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }

    public class FrequentlyAskedQuestion
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string Question { get; set; } = string.Empty;
        
        [Required]
        public string Answer { get; set; } = string.Empty;
        
        public int DisplayOrder { get; set; }
        
        public bool IsPublished { get; set; }
        
        public int ViewCount { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }
}
