using System;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Accounting.Events
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
