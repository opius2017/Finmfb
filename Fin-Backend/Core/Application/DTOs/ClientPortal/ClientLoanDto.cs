using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.ClientPortal
{

    public class ClientLoanDto
    {
        public Guid Id { get; set; }
        public required string LoanAccountNumber { get; set; }
        public required string LoanProductName { get; set; }
        public decimal DisbursedAmount { get; set; }
        public DateTime DisbursementDate { get; set; }
        public decimal InterestRate { get; set; }
        public int Tenor { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal NextInstallmentAmount { get; set; }
        public DateTime NextInstallmentDate { get; set; }
        public required string Status { get; set; }
        public int DaysPastDue { get; set; }
        public decimal TotalAmountPaid { get; set; }
    }
}