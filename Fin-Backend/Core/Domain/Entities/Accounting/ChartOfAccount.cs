using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Entities.Accounting.Events;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Accounting
{
    /// <summary>
    /// Represents an account in the Chart of Accounts
    /// </summary>
    public class ChartOfAccount : AggregateRoot
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public AccountClassification Classification { get; set; }
        public AccountType AccountType { get; set; }
        public string Description { get; set; }
        public AccountStatus Status { get; set; }
        public NormalBalanceType NormalBalance { get; set; }
        public string ParentAccountId { get; set; }
        public ChartOfAccount ParentAccount { get; private set; }
        public ICollection<ChartOfAccount> ChildAccounts { get; private set; } = new List<ChartOfAccount>();
        public string CurrencyCode { get; set; }
        public bool AllowManualEntries { get; set; }
        public bool RequiresReconciliation { get; set; }
        public string CBNReportingCode { get; set; }
        public string NDICReportingCode { get; set; }
        public string IFRSCategory { get; set; }
        public int AccountLevel { get; set; }
        public string AccountMnemonic { get; set; }
        public bool IsBudgeted { get; set; }
        public bool IsRestricted { get; set; }
        public int? BranchId { get; set; }
        public string Tags { get; set; }
        public Money CurrentBalance { get; private set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string LastModifiedBy { get; set; }
        
        // Required by EF Core
        private ChartOfAccount() { }
        
        public ChartOfAccount(
            string accountNumber,
            string accountName,
            AccountClassification classification,
            AccountType accountType,
            NormalBalanceType normalBalance,
            string currencyCode,
            string description = null,
            string parentAccountId = null,
            bool allowManualEntries = true,
            bool requiresReconciliation = false,
            string cbnReportingCode = null,
            string ndicReportingCode = null,
            string ifrsCategory = null,
            int accountLevel = 4,
            string accountMnemonic = null,
            bool isBudgeted = false,
            bool isRestricted = false,
            int? branchId = null,
            string tags = null)
        {
            Id = Guid.NewGuid().ToString();
            AccountNumber = accountNumber;
            AccountName = accountName;
            Classification = classification;
            AccountType = accountType;
            Description = description;
            Status = AccountStatus.Active;
            NormalBalance = normalBalance;
            ParentAccountId = parentAccountId;
            CurrencyCode = currencyCode;
            AllowManualEntries = allowManualEntries;
            RequiresReconciliation = requiresReconciliation;
            CBNReportingCode = cbnReportingCode;
            NDICReportingCode = ndicReportingCode;
            IFRSCategory = ifrsCategory;
            AccountLevel = accountLevel;
            AccountMnemonic = accountMnemonic;
            IsBudgeted = isBudgeted;
            IsRestricted = isRestricted;
            BranchId = branchId;
            Tags = tags;
            CurrentBalance = Money.Zero(currencyCode);
            CreatedAt = DateTime.UtcNow;
            
            // Add domain event
            AddDomainEvent(new AccountCreatedEvent(Id, accountNumber, accountName));
        }
        
        public void UpdateAccountDetails(
            string accountName, 
            string description,
            bool allowManualEntries,
            bool requiresReconciliation,
            string cbnReportingCode,
            string modifiedBy,
            string ndicReportingCode = null,
            string ifrsCategory = null,
            string accountMnemonic = null,
            bool isBudgeted = false,
            bool isRestricted = false,
            string tags = null)
        {
            AccountName = accountName;
            Description = description;
            AllowManualEntries = allowManualEntries;
            RequiresReconciliation = requiresReconciliation;
            CBNReportingCode = cbnReportingCode;
            NDICReportingCode = ndicReportingCode;
            IFRSCategory = ifrsCategory;
            AccountMnemonic = accountMnemonic;
            IsBudgeted = isBudgeted;
            IsRestricted = isRestricted;
            Tags = tags;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
            
            AddDomainEvent(new AccountUpdatedEvent(Id, AccountNumber, accountName));
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
            
            LastModifiedAt = DateTime.UtcNow;
        }
        
        public void Deactivate(string modifiedBy)
        {
            if (Status == AccountStatus.Inactive)
                return;
                
            Status = AccountStatus.Inactive;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
            
            AddDomainEvent(new AccountDeactivatedEvent(Id, AccountNumber));
        }
        
        public void Activate(string modifiedBy)
        {
            if (Status == AccountStatus.Active)
                return;
                
            Status = AccountStatus.Active;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
            
            AddDomainEvent(new AccountActivatedEvent(Id, AccountNumber));
        }
    }
}
