namespace FinTech.Core.Application.DTOs.Email
{
    public class EmailResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? EmailId { get; set; }
        public string? BatchId { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
    }
}
