using System;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Common;

namespace FinTech.Core.Domain.Entities.Loans
{
    public class LoanTransaction : BaseEntity
    {
        public string LoanId { get; private set; }
        public string TransactionType { get; private set; }
        public decimal PrincipalAmount { get; private set; }
        public decimal InterestAmount { get; private set; }
        public string Reference { get; private set; }
        public string Description { get; private set; }
        public DateTime TransactionDate { get; private set; }
        public string Status { get; private set; }

        // Compatibility shims for older Application expectations
        public string ReferenceNumber
        {
            get => Reference;
            set => Reference = value;
        }

        public decimal Amount
        {
            get => PrincipalAmount + InterestAmount;
            set
            {
                PrincipalAmount = value;
                InterestAmount = 0m;
            }
        }

        // Optional fields used by Application layer; kept as simple mutable properties
        public string? PaymentMethod { get; set; }
        public string? SourceAccountNumber { get; set; }

        public DateTime CreatedAt { get => TransactionDate; set => TransactionDate = value; }
        public DateTime? UpdatedAt { get; set; }

        private LoanTransaction() { } // For EF Core

        public LoanTransaction(
            string loanId,
            string transactionType,
            decimal principalAmount,
            decimal interestAmount,
            string reference,
            string description)
        {
            LoanId = loanId;
            TransactionType = transactionType;
            PrincipalAmount = principalAmount;
            InterestAmount = interestAmount;
            Reference = reference;
            Description = description;
            TransactionDate = DateTime.UtcNow;
            Status = "COMPLETED";
        }

        public void UpdateStatus(string status)
        {
            Status = status;
        }
    }
}
