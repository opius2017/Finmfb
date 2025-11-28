namespace FinTech.Core.Domain.Enums
{
    /// <summary>
    /// Classification for accounting accounts in the chart of accounts
    /// Kept here under FinTech.Core.Domain.Enums so application code using
    /// `using FinTech.Core.Domain.Enums;` resolves correctly.
    /// </summary>
    public enum AccountClassification
    {
        Undefined = 0,
        // Asset classifications
        CurrentAsset = 10,
        NonCurrentAsset = 20,

        // Liability classifications
        CurrentLiability = 30,
        NonCurrentLiability = 40,

        // Equity classifications
        OwnerEquity = 50,
        RetainedEarnings = 60,

        // Revenue classifications
        OperatingRevenue = 70,
        NonOperatingRevenue = 80,

        // Expense classifications
        OperatingExpense = 90,
        NonOperatingExpense = 100
    }
}
