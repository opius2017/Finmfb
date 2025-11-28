using System;

namespace FinTech.Core.Domain.Entities.Accounting
{
    /// <summary>
    /// Indicates whether an account's normal balance is debit or credit
    /// </summary>
    public enum NormalBalanceType
    {
        Undefined = 0,
        Debit = 1,
        Credit = 2
    }
}
