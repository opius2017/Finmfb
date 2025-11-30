using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class LoanAgreementDocument
    {
        public string LoanId { get; set; }
        public string SerialNumber { get; set; }
        public string DocumentUrl { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string Content { get; set; }
    }
}
