using System;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.Accounting.Events
{
    /// <summary>
    /// Domain event raised when an account is activated
    /// </summary>
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