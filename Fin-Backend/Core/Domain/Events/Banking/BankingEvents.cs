using System;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Banking;

namespace FinTech.Core.Domain.Events.Banking
{
    public class DepositCompletedEvent : DomainEvent
    {
        public int AccountId { get; }
        public decimal Amount { get; }
        public string Reference { get; }
        public string Description { get; }

        public DepositCompletedEvent(int accountId, decimal amount, string reference, string description)
        {
            AccountId = accountId;
            Amount = amount;
            Reference = reference;
            Description = description;
        }
    }

    public class WithdrawalCompletedEvent : DomainEvent
    {
        public int AccountId { get; }
        public decimal Amount { get; }
        public string Reference { get; }
        public string Description { get; }

        public WithdrawalCompletedEvent(int accountId, decimal amount, string reference, string description)
        {
            AccountId = accountId;
            Amount = amount;
            Reference = reference;
            Description = description;
        }
    }

    public class TransferCompletedEvent : DomainEvent
    {
        public int FromAccountId { get; }
        public int ToAccountId { get; }
        public decimal Amount { get; }
        public string Reference { get; }
        public string Description { get; }

        public TransferCompletedEvent(int fromAccountId, int toAccountId, decimal amount, string reference, string description)
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
        public int AccountId { get; }
        public decimal Amount { get; }
        public string FeeType { get; }
        public string Reference { get; }
        public string Description { get; }

        public FeeChargedEvent(int accountId, decimal amount, string feeType, string reference, string description)
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
        public int AccountId { get; }
        public decimal Amount { get; }
        public string Reference { get; }
        public string Description { get; }

        public InterestPaidEvent(int accountId, decimal amount, string reference, string description)
        {
            AccountId = accountId;
            Amount = amount;
            Reference = reference;
            Description = description;
        }
    }
}
