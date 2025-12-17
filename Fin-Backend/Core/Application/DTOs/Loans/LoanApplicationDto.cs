using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class LoanApplicationDto
    {
        public string Id { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string LoanProductId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Term { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ApplicationDate { get; set; }
        public string? RejectionReason { get; set; }
    }
}
