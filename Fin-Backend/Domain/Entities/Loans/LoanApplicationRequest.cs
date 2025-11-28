using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Entities.Customers;

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a loan application request from a customer through the client portal
    /// </summary>
    public class LoanApplicationRequest : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Guid LoanProductId { get; set; }
        public decimal RequestedAmount { get; set; }
        public int RequestedTenor { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string RepaymentSource { get; set; } = string.Empty;
        public DateTime? PreferredDisbursementDate { get; set; }
        public decimal MonthlyIncome { get; set; }
        public decimal ExistingMonthlyDebt { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ApplicationDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string? ProcessedBy { get; set; }
        public string? Comments { get; set; }
        public string? RejectionReason { get; set; }

        // Navigation properties
        public virtual Customer? Customer { get; set; }
        public virtual LoanProduct? LoanProduct { get; set; }
        public virtual ICollection<LoanApplicationDocument> Documents { get; set; } = new List<LoanApplicationDocument>();
    }

    /// <summary>
    /// Represents documents uploaded with a loan application request
    /// </summary>
    public class LoanApplicationDocument : BaseEntity
    {
        public Guid LoanApplicationRequestId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }

        // Navigation properties
        public virtual LoanApplicationRequest? LoanApplicationRequest { get; set; }
    }
}