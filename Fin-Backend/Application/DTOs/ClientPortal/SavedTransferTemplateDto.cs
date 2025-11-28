using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class SavedTransferTemplateDto
    {
        public Guid Id { get; set; }
        public string TemplateName { get; set; }
        public string FromAccountNumber { get; set; }
        public string ToAccountNumber { get; set; }
        public string ToBankName { get; set; }
        public string ToBankCode { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Reference { get; set; }
        public string Category { get; set; }
        public bool IsFavorite { get; set; }
        public int UsageCount { get; set; }
        public DateTime? LastUsed { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
