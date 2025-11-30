using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class DisbursementResult
    {
        public bool Success { get; set; }
        public string LoanId { get; set; }
        public string SerialNumber { get; set; }
        public decimal DisbursedAmount { get; set; }
        public DateTime DisbursementDate { get; set; }
        public string BankTransferReference { get; set; }
        public string AgreementDocumentUrl { get; set; }
        public DateTime? FirstPaymentDate { get; set; }
        public decimal MonthlyEMI { get; set; }
        public string Message { get; set; }
    }
}
