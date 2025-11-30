using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class GuarantorConsentRequest
    {
        public string ConsentId { get; set; }
        public string LoanApplicationId { get; set; }
        public string GuarantorMemberId { get; set; }
        public decimal GuaranteedAmount { get; set; }
        public string Status { get; set; }
        public DateTime RequestedAt { get; set; }
    }
}
