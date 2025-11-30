using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class GuarantorConsentResponse
    {
        public string ConsentId { get; set; }
        public string Status { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string Message { get; set; }
    }
}
