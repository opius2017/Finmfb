using System;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.Accounting.Events
{
    /// <summary>
    /// Domain event raised when an account is deactivated
    /// </summary>
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
}