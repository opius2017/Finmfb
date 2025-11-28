using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class TransactionExportDto
    {
        public string AccountNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Format { get; set; } = "csv"; // csv, excel, pdf
        public bool IncludeRunningBalance { get; set; } = true;
    }
}
