namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class CreateSavedTransferTemplateDto
    {
        public string TemplateName { get; set; }
        public string FromAccountNumber { get; set; }
        public string ToAccountNumber { get; set; }
        public string ToBankName { get; set; }
        public string ToBankCode { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "NGN";
        public string Reference { get; set; }
        public string Category { get; set; }
        public bool IsFavorite { get; set; }
    }
}
