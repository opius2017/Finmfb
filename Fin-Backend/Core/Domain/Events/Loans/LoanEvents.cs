using System;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Loans;

namespace FinTech.Core.Domain.Events.Loans
{
    public class LoanDisbursedEvent : DomainEvent
    {
        public string LoanId { get; }
        public decimal Amount { get; }
        public string Reference { get; }
        public string Description { get; }

        public LoanDisbursedEvent(string loanId, decimal amount, string reference, string description)
        {
            LoanId = loanId;
            Amount = amount;
            Reference = reference;
            Description = description;
        }
    }

    public class LoanRepaymentReceivedEvent : DomainEvent
    {
        public string LoanId { get; }
        public decimal PrincipalAmount { get; }
        public decimal InterestAmount { get; }
        public string Reference { get; }
        public string Description { get; }

        public LoanRepaymentReceivedEvent(string loanId, decimal principalAmount, decimal interestAmount, string reference, string description)
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
        public string LoanId { get; }
        public decimal Amount { get; }
        public string Reference { get; }
        public string Description { get; }

        public LoanWrittenOffEvent(string loanId, decimal amount, string reference, string description)
        {
            LoanId = loanId;
            Amount = amount;
            Reference = reference;
            Description = description;
        }
    }

    public class LoanInterestAccruedEvent : DomainEvent
    {
        public string LoanId { get; }
        public decimal Amount { get; }
        public string Reference { get; }
        public string Description { get; }

        public LoanInterestAccruedEvent(string loanId, decimal amount, string reference, string description)
        {
            LoanId = loanId;
            Amount = amount;
            Reference = reference;
            Description = description;
        }
    }

    public class LoanFeeChargedEvent : DomainEvent
    {
        public string LoanId { get; }
        public decimal Amount { get; }
        public string FeeType { get; }
        public string Reference { get; }
        public string Description { get; }

        public LoanFeeChargedEvent(string loanId, decimal amount, string feeType, string reference, string description)
        {
            LoanId = loanId;
            Amount = amount;
            FeeType = feeType;
            Reference = reference;
            Description = description;
        }
    }
}
