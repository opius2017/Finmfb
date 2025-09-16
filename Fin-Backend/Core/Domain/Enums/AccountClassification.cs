using System;

namespace FinTech.Core.Domain.Enums
{
    /// <summary>
    /// Classification of an account according to accounting principles.
    /// </summary>
    public enum AccountClassification
    {
        Asset = 1,
        Liability = 2,
        Equity = 3,
        Revenue = 4,
        Expense = 5
    }
}