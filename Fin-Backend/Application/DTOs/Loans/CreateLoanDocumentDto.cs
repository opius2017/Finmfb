namespace FinTech.Application.DTOs.Loans
{
    public class CreateLoanDocumentDto
    {
        public string? DocumentType { get; set; }
        public string? FileName { get; set; }
        public string? Url { get; set; }
        public string? Description { get; set; }
    }
}