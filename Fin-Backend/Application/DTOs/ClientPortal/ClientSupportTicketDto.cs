using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class ClientSupportTicketDto
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string Resolution { get; set; }
        public int CustomerSatisfactionRating { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<ClientSupportMessageDto> Messages { get; set; }
    }
}
