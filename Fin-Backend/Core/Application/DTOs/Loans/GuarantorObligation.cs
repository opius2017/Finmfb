using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class GuarantorObligation
    {
        public string ConsentId { get; set; }
        public string LoanApplicationId { get; set; }
        public string ApplicationNumber { get; set; }
        public string ApplicantName { get; set; }
        public decimal GuaranteedAmount { get; set; }
        public string Status { get; set; } // PENDING, ACTIVE, COMPLETED
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
