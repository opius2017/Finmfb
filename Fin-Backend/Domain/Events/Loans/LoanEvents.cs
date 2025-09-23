using System;
using FinTech.Domain.Common;
using FinTech.Domain.Entities.Loans;

namespace FinTech.Domain.Events.Loans
{
    public class LoanDisbursedEvent : DomainEvent
    {
        public int LoanId { get; }
        public decimal Amount { get; }
        public string Reference { get; }
        public string Description { get; }

        public LoanDisbursedEvent(int loanId, decimal amount, string reference, string description)
        {
            LoanId = loanId;
            Amount = amount;
            Reference = reference;
            Description = description;
        }
    }

    public class LoanRepaymentReceivedEvent : DomainEvent
    {
        public int LoanId { get; }
        public decimal PrincipalAmount { get; }
        public decimal InterestAmount { get; }
        public string Reference { get; }
        public string Description { get; }

        public LoanRepaymentReceivedEvent(int loanId, decimal principalAmount, decimal interestAmount, string reference, string description)
        {
            LoanId = loanId;
            PrincipalAmount = principalAmount;
            InterestAmount = interestAmount;
            Reference = reference;
            Description = description;
        }
    }

    public class LoanWrittenOffEvent : DomainEvent
    {
        public int LoanId { get; }
        public decimal Amount { get; }
        public string Reference { get; }
        public string Description { get; }

        public LoanWrittenOffEvent(int loanId, decimal amount, string reference, string description)
        {
            LoanId = loanId;
            Amount = amount;
            Reference = reference;
            Description = description;
        }
    }

    public class LoanInterestAccruedEvent : DomainEvent
    {
        public int LoanId { get; }
        public decimal Amount { get; }
        public string Reference { get; }
        public string Description { get; }

        public LoanInterestAccruedEvent(int loanId, decimal amount, string reference, string description)
        {
            LoanId = loanId;
            Amount = amount;
            Reference = reference;
            Description = description;
        }
    }

    public class LoanFeeChargedEvent : DomainEvent
    {
        public int LoanId { get; }
        public decimal Amount { get; }
        public string FeeType { get; }
        public string Reference { get; }
        public string Description { get; }

        public LoanFeeChargedEvent(int loanId, decimal amount, string feeType, string reference, string description)
        {
            LoanId = loanId;
            Amount = amount;
            FeeType = feeType;
            Reference = reference;
            Description = description;
        }
    }
}