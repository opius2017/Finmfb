using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class LoanRegistrationResult
    {
        public string LoanId { get; set; }
        public string SerialNumber { get; set; }
        public DateTime RegisterDate { get; set; }
        public string RegisterEntryId { get; set; }
        public string Message { get; set; }
    }
}
