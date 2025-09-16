using System;

namespace FinTech.Domain.Entities.Accounting
{
    /// <summary>
    /// Indicates the status of an account in the chart of accounts
    /// </summary>
    public enum AccountStatus
    {
        Undefined = 0,
        Active = 1,
        Inactive = 2,
        Closed = 3,
        Suspended = 4,
        PendingApproval = 5
    }
}