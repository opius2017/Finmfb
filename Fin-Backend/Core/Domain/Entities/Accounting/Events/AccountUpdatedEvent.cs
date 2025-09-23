using System;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.Accounting.Events
{
    /// <summary>
    /// Domain event raised when an account is updated
    /// </summary>
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
}