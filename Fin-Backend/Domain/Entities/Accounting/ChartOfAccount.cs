using System;
using System.Collections.Generic;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.Accounting
{
    public enum AccountClassification
    {
        Asset = 1,
        Liability = 2,
        Equity = 3,
        Income = 4,
        Expense = 5
    }
    
    public enum AccountType
    {
        Cash = 1,
        Bank = 2,
        AccountsReceivable = 3,
        Loans = 4,
        FixedAssets = 5,
        OtherAssets = 6,
        AccountsPayable = 7,
        Deposits = 8,
        Borrowings = 9,
        OtherLiabilities = 10,
        Capital = 11,
        Reserves = 12,
        RetainedEarnings = 13,
        InterestIncome = 14,
        FeeIncome = 15,
        OtherIncome = 16,
        InterestExpense = 17,
        PersonnelExpense = 18,
        AdministrativeExpense = 19,
        OtherExpense = 20
    }
    
    public enum NormalBalanceType
    {
        Debit = 1,
        Credit = 2
    }
    
    /// <summary>
    /// Represents an account in the Chart of Accounts
    /// </summary>
    public class ChartOfAccount : AggregateRoot
    {
        public string AccountCode { get; private set; }
        public string AccountName { get; private set; }
        public AccountClassification Classification { get; private set; }
        public AccountType AccountType { get; private set; }
        public string Description { get; private set; }
        public bool IsActive { get; private set; }
        public NormalBalanceType NormalBalance { get; private set; }
        public string ParentAccountId { get; private set; }
        public ChartOfAccount ParentAccount { get; private set; }
        public ICollection<ChartOfAccount> ChildAccounts { get; private set; } = new List<ChartOfAccount>();
        public string CurrencyCode { get; private set; }
        public bool AllowManualEntries { get; private set; }
        public bool RequiresReconciliation { get; private set; }
        public string CBNReportingCode { get; private set; }
        public Money CurrentBalance { get; private set; }
        
        // Required by EF Core
        private ChartOfAccount() { }
        
        public ChartOfAccount(
            string accountCode,
            string accountName,
            AccountClassification classification,
            AccountType accountType,
            NormalBalanceType normalBalance,
            string currencyCode,
            string description = null,
            string parentAccountId = null,
            bool allowManualEntries = true,
            bool requiresReconciliation = false,
            string cbnReportingCode = null)
        {
            AccountCode = accountCode;
            AccountName = accountName;
            Classification = classification;
            AccountType = accountType;
            Description = description;
            IsActive = true;
            NormalBalance = normalBalance;
            ParentAccountId = parentAccountId;
            CurrencyCode = currencyCode;
            AllowManualEntries = allowManualEntries;
            RequiresReconciliation = requiresReconciliation;
            CBNReportingCode = cbnReportingCode;
            CurrentBalance = Money.Zero(currencyCode);
            
            // Add domain event
            AddDomainEvent(new AccountCreatedEvent(Id, accountCode, accountName));
        }
        
        public void UpdateAccountDetails(
            string accountName, 
            string description,
            bool isActive,
            bool allowManualEntries,
            bool requiresReconciliation,
            string cbnReportingCode)
        {
            AccountName = accountName;
            Description = description;
            IsActive = isActive;
            AllowManualEntries = allowManualEntries;
            RequiresReconciliation = requiresReconciliation;
            CBNReportingCode = cbnReportingCode;
            LastModifiedDate = DateTime.UtcNow;
            
            AddDomainEvent(new AccountUpdatedEvent(Id, AccountCode, accountName));
        }
        
        public void UpdateBalance(Money amount, bool isDebit)
        {
            // For debit normal balance accounts (Assets, Expenses)
            if (NormalBalance == NormalBalanceType.Debit)
            {
                if (isDebit)
                {
                    CurrentBalance = CurrentBalance.Add(amount);
                }
                else
                {
                    CurrentBalance = CurrentBalance.Subtract(amount);
                }
            }
            // For credit normal balance accounts (Liabilities, Equity, Income)
            else
            {
                if (isDebit)
                {
                    CurrentBalance = CurrentBalance.Subtract(amount);
                }
                else
                {
                    CurrentBalance = CurrentBalance.Add(amount);
                }
            }
            
            LastModifiedDate = DateTime.UtcNow;
        }
        
        public void Deactivate()
        {
            if (!IsActive)
                return;
                
            IsActive = false;
            LastModifiedDate = DateTime.UtcNow;
            
            AddDomainEvent(new AccountDeactivatedEvent(Id, AccountCode));
        }
        
        public void Activate()
        {
            if (IsActive)
                return;
                
            IsActive = true;
            LastModifiedDate = DateTime.UtcNow;
            
            AddDomainEvent(new AccountActivatedEvent(Id, AccountCode));
        }
    }
    
    // Domain Events
    public class AccountCreatedEvent : DomainEvent
    {
        public string AccountId { get; }
        public string AccountCode { get; }
        public string AccountName { get; }
        
        public AccountCreatedEvent(string accountId, string accountCode, string accountName)
        {
            AccountId = accountId;
            AccountCode = accountCode;
            AccountName = accountName;
        }
    }
    
    public class AccountUpdatedEvent : DomainEvent
    {
        public string AccountId { get; }
        public string AccountCode { get; }
        public string AccountName { get; }
        
        public AccountUpdatedEvent(string accountId, string accountCode, string accountName)
        {
            AccountId = accountId;
            AccountCode = accountCode;
            AccountName = accountName;
        }
    }
    
    public class AccountDeactivatedEvent : DomainEvent
    {
        public string AccountId { get; }
        public string AccountCode { get; }
        
        public AccountDeactivatedEvent(string accountId, string accountCode)
        {
            AccountId = accountId;
            AccountCode = accountCode;
        }
    }
    
    public class AccountActivatedEvent : DomainEvent
    {
        public string AccountId { get; }
        public string AccountCode { get; }
        
        public AccountActivatedEvent(string accountId, string accountCode)
        {
            AccountId = accountId;
            AccountCode = accountCode;
        }
    }
}