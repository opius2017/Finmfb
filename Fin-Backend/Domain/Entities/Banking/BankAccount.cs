using System;
using System.Collections.Generic;
using FinTech.Domain.Common;
using FinTech.Domain.Events.Banking;

namespace FinTech.Domain.Entities.Banking
{
    public class BankAccount : BaseEntity
    {
        public string AccountNumber { get; private set; }
        public string AccountName { get; private set; }
        public decimal Balance { get; private set; }
        public string AccountType { get; private set; }
        public bool IsActive { get; private set; }
        public int CustomerId { get; private set; }
        public string Currency { get; private set; }
        public virtual ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();

        private BankAccount() { } // For EF Core

        public BankAccount(
            string accountNumber, 
            string accountName, 
            string accountType, 
            int customerId, 
            string currency, 
            decimal initialDeposit = 0)
        {
            AccountNumber = accountNumber;
            AccountName = accountName;
            AccountType = accountType;
            CustomerId = customerId;
            Currency = currency;
            Balance = 0;
            IsActive = true;
            
            if (initialDeposit > 0)
            {
                Deposit(initialDeposit, "Initial deposit", "INIT");
            }
        }

        public void Deposit(decimal amount, string description, string reference)
        {
            if (amount <= 0)
                throw new ArgumentException("Deposit amount must be positive", nameof(amount));

            if (!IsActive)
                throw new InvalidOperationException("Cannot deposit to an inactive account");

            Balance += amount;

            var transaction = new Transaction(
                this.Id,
                "DEPOSIT",
                amount,
                reference,
                description,
                Balance);
            
            Transactions.Add(transaction);

            // Raise domain event
            AddDomainEvent(new DepositCompletedEvent(
                this.Id,
                amount,
                reference,
                description));
        }

        public void Withdraw(decimal amount, string description, string reference)
        {
            if (amount <= 0)
                throw new ArgumentException("Withdrawal amount must be positive", nameof(amount));

            if (!IsActive)
                throw new InvalidOperationException("Cannot withdraw from an inactive account");

            if (Balance < amount)
                throw new InvalidOperationException("Insufficient funds");

            Balance -= amount;

            var transaction = new Transaction(
                this.Id,
                "WITHDRAWAL",
                -amount,
                reference,
                description,
                Balance);
            
            Transactions.Add(transaction);

            // Raise domain event
            AddDomainEvent(new WithdrawalCompletedEvent(
                this.Id,
                amount,
                reference,
                description));
        }

        public void Transfer(BankAccount toAccount, decimal amount, string description, string reference)
        {
            if (toAccount == null)
                throw new ArgumentNullException(nameof(toAccount));

            if (amount <= 0)
                throw new ArgumentException("Transfer amount must be positive", nameof(amount));

            if (!IsActive)
                throw new InvalidOperationException("Cannot transfer from an inactive account");

            if (!toAccount.IsActive)
                throw new InvalidOperationException("Cannot transfer to an inactive account");

            if (Balance < amount)
                throw new InvalidOperationException("Insufficient funds");

            Balance -= amount;
            toAccount.Balance += amount;

            var withdrawalTransaction = new Transaction(
                this.Id,
                "TRANSFER_OUT",
                -amount,
                reference,
                description,
                Balance);
            
            var depositTransaction = new Transaction(
                toAccount.Id,
                "TRANSFER_IN",
                amount,
                reference,
                description,
                toAccount.Balance);
            
            Transactions.Add(withdrawalTransaction);
            toAccount.Transactions.Add(depositTransaction);

            // Raise domain event
            AddDomainEvent(new TransferCompletedEvent(
                this.Id,
                toAccount.Id,
                amount,
                reference,
                description));
        }

        public void ChargeFee(decimal amount, string feeType, string description, string reference)
        {
            if (amount <= 0)
                throw new ArgumentException("Fee amount must be positive", nameof(amount));

            if (!IsActive)
                throw new InvalidOperationException("Cannot charge fee to an inactive account");

            if (Balance < amount)
                throw new InvalidOperationException("Insufficient funds");

            Balance -= amount;

            var transaction = new Transaction(
                this.Id,
                "FEE",
                -amount,
                reference,
                description,
                Balance);
            
            Transactions.Add(transaction);

            // Raise domain event
            AddDomainEvent(new FeeChargedEvent(
                this.Id,
                amount,
                feeType,
                reference,
                description));
        }

        public void PayInterest(decimal amount, string description, string reference)
        {
            if (amount <= 0)
                throw new ArgumentException("Interest amount must be positive", nameof(amount));

            if (!IsActive)
                throw new InvalidOperationException("Cannot pay interest to an inactive account");

            Balance += amount;

            var transaction = new Transaction(
                this.Id,
                "INTEREST",
                amount,
                reference,
                description,
                Balance);
            
            Transactions.Add(transaction);

            // Raise domain event
            AddDomainEvent(new InterestPaidEvent(
                this.Id,
                amount,
                reference,
                description));
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}