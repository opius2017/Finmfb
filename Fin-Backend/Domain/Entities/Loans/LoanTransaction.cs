using System;
using FinTech.Domain.Common;

namespace FinTech.Domain.Entities.Loans
{
    public class LoanTransaction : BaseEntity
    {
        public int LoanId { get; private set; }
        public string TransactionType { get; private set; }
        public decimal PrincipalAmount { get; private set; }
        public decimal InterestAmount { get; private set; }
        public string Reference { get; private set; }
        public string Description { get; private set; }
        public DateTime TransactionDate { get; private set; }
        public string Status { get; private set; }

        private LoanTransaction() { } // For EF Core

        public LoanTransaction(
            int loanId,
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