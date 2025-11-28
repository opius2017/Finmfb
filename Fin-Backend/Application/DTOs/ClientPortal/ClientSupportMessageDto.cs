using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class ClientSupportMessageDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public bool IsFromClient { get; set; }
        public string SenderName { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
