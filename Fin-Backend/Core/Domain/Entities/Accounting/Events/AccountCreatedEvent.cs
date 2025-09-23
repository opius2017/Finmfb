using System;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.Accounting.Events
{
    /// <summary>
    /// Domain event raised when an account is created
    /// </summary>
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
}