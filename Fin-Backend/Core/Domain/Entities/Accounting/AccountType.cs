using System;

namespace FinTech.Domain.Entities.Accounting
{
    /// <summary>
    /// Types of accounts in the accounting system
    /// </summary>
    public enum AccountType
    {
        Undefined = 0,
        // Main account types
        Asset = 1,
        Liability = 2,
        Equity = 3,
        Revenue = 4,
        Expense = 5,
        
        // Asset subtypes
        Cash = 10,
        Bank = 11,
        AccountsReceivable = 12,
        Loans = 13,
        LoanLossProvision = 14,
        FixedAssets = 15,
        AccumulatedDepreciation = 16,
        IntangibleAssets = 17,
        PrepaidExpenses = 18,
        Inventory = 19,
        OtherAssets = 29,
        
        // Liability subtypes
        AccountsPayable = 30,
        Deposits = 31,
        SavingsAccounts = 32,
        CurrentAccounts = 33,
        FixedDeposits = 34,
        Borrowings = 35,
        AccruedExpenses = 36,
        TaxPayable = 37,
        DeferredTaxLiability = 38,
        OtherLiabilities = 49,
        
        // Equity subtypes
        Capital = 50,
        ShareCapital = 51,
        SharePremium = 52,
        RetainedEarnings = 53,
        StatutoryReserves = 54,
        GeneralReserves = 55,
        OtherReserves = 59,
        
        // Revenue subtypes
        InterestIncome = 60,
        FeeIncome = 61,
        CommissionIncome = 62,
        InvestmentIncome = 63,
        ForeignExchangeGains = 64,
        OtherIncome = 69,
        
        // Expense subtypes
        InterestExpense = 70,
        PersonnelExpense = 71,
        AdministrativeExpense = 72,
        DepreciationAndAmortization = 73,
        LoanLossExpense = 74,
        OperatingExpenses = 75,
        MarketingExpense = 76,
        TaxExpense = 77,
        OtherExpense = 89
    }
}