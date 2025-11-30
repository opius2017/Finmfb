using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class TransactionTrackingResult
    {
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Message { get; set; }
    }
}
