using System;

namespace FinTech.Core.Domain.Enums
{
    /// <summary>
    /// Specific type of an account within its classification.
    /// </summary>
    public enum AccountType
    {
        // Asset Types
        Cash = 1,
        BankAccount = 2,
        AccountsReceivable = 3,
        Inventory = 4,
        FixedAsset = 5,
        AccumulatedDepreciation = 6,
        Investment = 7,
        OtherAsset = 8,
        
        // Liability Types
        AccountsPayable = 101,
        AccruedLiability = 102,
        CustomerDeposit = 103,
        ShortTermLoan = 104,
        LongTermLoan = 105,
        OtherLiability = 106,
        
        // Equity Types
        CommonStock = 201,
        PreferredStock = 202,
        RetainedEarnings = 203,
        AdditionalPaidInCapital = 204,
        TreasuryStock = 205,
        OtherEquity = 206,
        
        // Revenue Types
        InterestIncome = 301,
        FeeIncome = 302,
        CommissionIncome = 303,
        InvestmentIncome = 304,
        OtherIncome = 305,
        
        // Expense Types
        SalaryExpense = 401,
        RentExpense = 402,
        UtilityExpense = 403,
        DepreciationExpense = 404,
        InterestExpense = 405,
        BadDebtExpense = 406,
        MarketingExpense = 407,
        TaxExpense = 408,
        OtherExpense = 409
    }
}