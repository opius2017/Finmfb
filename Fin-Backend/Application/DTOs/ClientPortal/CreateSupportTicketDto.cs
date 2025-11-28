namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class CreateSupportTicketDto
    {
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Priority { get; set; }
        public string InitialMessage { get; set; }
    }
}
