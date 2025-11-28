using System;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Banking;

namespace FinTech.Core.Domain.Events.Banking
{
    public class DepositCompletedEvent : DomainEvent
    {
        public string AccountId { get; }
        public decimal Amount { get; }
        public string Reference { get; }
        public string Description { get; }

        public DepositCompletedEvent(string accountId, decimal amount, string reference, string description)
        {
            AccountId = accountId;
            Amount = amount;
            Reference = reference;
            Description = description;
        }
    }

    public class WithdrawalCompletedEvent : DomainEvent
    {
        public string AccountId { get; }
        public decimal Amount { get; }
        public string Reference { get; }
        public string Description { get; }

        public WithdrawalCompletedEvent(string accountId, decimal amount, string reference, string description)
        {
            AccountId = accountId;
            Amount = amount;
            Reference = reference;
            Description = description;
        }
    }

    public class TransferCompletedEvent : DomainEvent
    {
        public string FromAccountId { get; }
        public string ToAccountId { get; }
        public decimal Amount { get; }
        public string Reference { get; }
        public string Description { get; }

        public TransferCompletedEvent(string fromAccountId, string toAccountId, decimal amount, string reference, string description)
        {
            FromAccountId = fromAccountId;
            ToAccountId = toAccountId;
            Amount = amount;
            Reference = reference;
            Description = description;
        }
    }

    public class FeeChargedEvent : DomainEvent
    {
        public string AccountId { get; }
        public decimal Amount { get; }
        public string FeeType { get; }
        public string Reference { get; }
        public string Description { get; }

        public FeeChargedEvent(string accountId, decimal amount, string feeType, string reference, string description)
        {
            AccountId = accountId;
            Amount = amount;
            FeeType = feeType;
            Reference = reference;
            Description = description;
        }
    }

    public class InterestPaidEvent : DomainEvent
    {
        public string AccountId { get; }
        public decimal Amount { get; }
        public string Reference { get; }
        public string Description { get; }

        public InterestPaidEvent(string accountId, decimal amount, string reference, string description)
        {
            AccountId = accountId;
            Amount = amount;
            Reference = reference;
            Description = description;
        }
    }
}
