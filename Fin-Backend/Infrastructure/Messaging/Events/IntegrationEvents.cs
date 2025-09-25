using System;
using System.Collections.Generic;

namespace Fin_Backend.Infrastructure.Messaging.Events
{
    /// <summary>
    /// Integration event for when a payment is processed
    /// </summary>
    public class PaymentProcessedIntegrationEvent : IntegrationEvent
    {
        /// <summary>
        /// Gets the payment ID
        /// </summary>
        public string PaymentId { get; }
        
        /// <summary>
        /// Gets the account ID
        /// </summary>
        public string AccountId { get; }
        
        /// <summary>
        /// Gets the amount
        /// </summary>
        public decimal Amount { get; }
        
        /// <summary>
        /// Gets the currency
        /// </summary>
        public string Currency { get; }
        
        /// <summary>
        /// Gets the payment method
        /// </summary>
        public string PaymentMethod { get; }
        
        /// <summary>
        /// Gets the payment status
        /// </summary>
        public string Status { get; }
        
        /// <summary>
        /// Gets the transaction reference
        /// </summary>
        public string TransactionReference { get; }
        
        /// <summary>
        /// Gets the payment date
        /// </summary>
        public DateTime PaymentDate { get; }

        /// <summary>
        /// Creates a new payment processed integration event
        /// </summary>
        /// <param name="paymentId">The payment ID</param>
        /// <param name="accountId">The account ID</param>
        /// <param name="amount">The amount</param>
        /// <param name="currency">The currency</param>
        /// <param name="paymentMethod">The payment method</param>
        /// <param name="status">The payment status</param>
        /// <param name="transactionReference">The transaction reference</param>
        /// <param name="paymentDate">The payment date</param>
        public PaymentProcessedIntegrationEvent(
            string paymentId,
            string accountId,
            decimal amount,
            string currency,
            string paymentMethod,
            string status,
            string transactionReference,
            DateTime paymentDate)
        {
            PaymentId = paymentId;
            AccountId = accountId;
            Amount = amount;
            Currency = currency;
            PaymentMethod = paymentMethod;
            Status = status;
            TransactionReference = transactionReference;
            PaymentDate = paymentDate;
        }
    }

    /// <summary>
    /// Integration event for when an account is created
    /// </summary>
    public class AccountCreatedIntegrationEvent : IntegrationEvent
    {
        /// <summary>
        /// Gets the account ID
        /// </summary>
        public string AccountId { get; }
        
        /// <summary>
        /// Gets the customer ID
        /// </summary>
        public string CustomerId { get; }
        
        /// <summary>
        /// Gets the account number
        /// </summary>
        public string AccountNumber { get; }
        
        /// <summary>
        /// Gets the account name
        /// </summary>
        public string AccountName { get; }
        
        /// <summary>
        /// Gets the account type
        /// </summary>
        public string AccountType { get; }
        
        /// <summary>
        /// Gets the currency
        /// </summary>
        public string Currency { get; }
        
        /// <summary>
        /// Gets the account status
        /// </summary>
        public string Status { get; }
        
        /// <summary>
        /// Gets the creation date
        /// </summary>
        public DateTime OpenDate { get; }

        /// <summary>
        /// Creates a new account created integration event
        /// </summary>
        /// <param name="accountId">The account ID</param>
        /// <param name="customerId">The customer ID</param>
        /// <param name="accountNumber">The account number</param>
        /// <param name="accountName">The account name</param>
        /// <param name="accountType">The account type</param>
        /// <param name="currency">The currency</param>
        /// <param name="status">The account status</param>
        /// <param name="openDate">The open date</param>
        public AccountCreatedIntegrationEvent(
            string accountId,
            string customerId,
            string accountNumber,
            string accountName,
            string accountType,
            string currency,
            string status,
            DateTime openDate)
        {
            AccountId = accountId;
            CustomerId = customerId;
            AccountNumber = accountNumber;
            AccountName = accountName;
            AccountType = accountType;
            Currency = currency;
            Status = status;
            OpenDate = openDate;
        }
    }

    /// <summary>
    /// Integration event for when a loan is approved
    /// </summary>
    public class LoanApprovedIntegrationEvent : IntegrationEvent
    {
        /// <summary>
        /// Gets the loan ID
        /// </summary>
        public string LoanId { get; }
        
        /// <summary>
        /// Gets the customer ID
        /// </summary>
        public string CustomerId { get; }
        
        /// <summary>
        /// Gets the account ID
        /// </summary>
        public string AccountId { get; }
        
        /// <summary>
        /// Gets the loan amount
        /// </summary>
        public decimal Amount { get; }
        
        /// <summary>
        /// Gets the currency
        /// </summary>
        public string Currency { get; }
        
        /// <summary>
        /// Gets the interest rate
        /// </summary>
        public decimal InterestRate { get; }
        
        /// <summary>
        /// Gets the loan term in months
        /// </summary>
        public int TermMonths { get; }
        
        /// <summary>
        /// Gets the approval date
        /// </summary>
        public DateTime ApprovalDate { get; }
        
        /// <summary>
        /// Gets the first payment date
        /// </summary>
        public DateTime FirstPaymentDate { get; }
        
        /// <summary>
        /// Gets the loan purpose
        /// </summary>
        public string Purpose { get; }

        /// <summary>
        /// Creates a new loan approved integration event
        /// </summary>
        /// <param name="loanId">The loan ID</param>
        /// <param name="customerId">The customer ID</param>
        /// <param name="accountId">The account ID</param>
        /// <param name="amount">The loan amount</param>
        /// <param name="currency">The currency</param>
        /// <param name="interestRate">The interest rate</param>
        /// <param name="termMonths">The loan term in months</param>
        /// <param name="approvalDate">The approval date</param>
        /// <param name="firstPaymentDate">The first payment date</param>
        /// <param name="purpose">The loan purpose</param>
        public LoanApprovedIntegrationEvent(
            string loanId,
            string customerId,
            string accountId,
            decimal amount,
            string currency,
            decimal interestRate,
            int termMonths,
            DateTime approvalDate,
            DateTime firstPaymentDate,
            string purpose)
        {
            LoanId = loanId;
            CustomerId = customerId;
            AccountId = accountId;
            Amount = amount;
            Currency = currency;
            InterestRate = interestRate;
            TermMonths = termMonths;
            ApprovalDate = approvalDate;
            FirstPaymentDate = firstPaymentDate;
            Purpose = purpose;
        }
    }
}
