using System.Collections.Generic;
using FinTech.Core.Domain.Entities.Accounting;

namespace FinTech.Core.Application.Services.Accounting
{
    /// <summary>
    /// Provides regulatory chart of account templates (CBN, NDIC, IFRS)
    /// </summary>
    public class ChartOfAccountTemplateService
    {
        public List<ChartOfAccount> GetCbnTemplate(string tenantId)
        {
            // Example: Add real CBN template accounts here
            return new List<ChartOfAccount>
            {
                new ChartOfAccount(
                    accountNumber: "1000",
                    accountName: "Cash and Cash Equivalents",
                    classification: AccountClassification.Asset,
                    accountType: AccountType.CurrentAsset,
                    normalBalance: NormalBalanceType.Debit,
                    currencyCode: "NGN",
                    cbnReportingCode: "A01",
                    accountLevel: 1
                ),
                new ChartOfAccount(
                    accountNumber: "2000",
                    accountName: "Customer Deposits",
                    classification: AccountClassification.Liability,
                    accountType: AccountType.CurrentLiability,
                    normalBalance: NormalBalanceType.Credit,
                    currencyCode: "NGN",
                    cbnReportingCode: "L01",
                    accountLevel: 1
                ),
                // Add more accounts as per CBN template
            };
        }

        public List<ChartOfAccount> GetNdicTemplate(string tenantId)
        {
            // Example: Add real NDIC template accounts here
            return new List<ChartOfAccount>
            {
                new ChartOfAccount(
                    accountNumber: "3000",
                    accountName: "NDIC Insurance Reserve",
                    classification: AccountClassification.CurrentAsset,
                    accountType: AccountType.OtherAssets,
                    normalBalance: NormalBalanceType.Debit,
                    currencyCode: "NGN",
                    ndicReportingCode: "N01",
                    accountLevel: 1
                ),
                new ChartOfAccount(
                    accountNumber: "4000",
                    accountName: "NDIC Premium Payable",
                    classification: AccountClassification.CurrentLiability,
                    accountType: AccountType.OtherLiability,
                    normalBalance: NormalBalanceType.Credit,
                    currencyCode: "NGN",
                    ndicReportingCode: "N02",
                    accountLevel: 1
                ),
                // Add more accounts as per NDIC template
            };
        }

        public List<ChartOfAccount> GetIfrsTemplate(string tenantId)
        {
            // Example: Add real IFRS template accounts here
            return new List<ChartOfAccount>
            {
                new ChartOfAccount(
                    accountNumber: "5000",
                    accountName: "IFRS 9 Expected Credit Loss",
                    classification: AccountClassification.OperatingExpense,
                    accountType: AccountType.Expense,
                    normalBalance: NormalBalanceType.Debit,
                    currencyCode: "NGN",
                    ifrsCategory: "IFRS9",
                    accountLevel: 1
                ),
                new ChartOfAccount(
                    accountNumber: "6000",
                    accountName: "IFRS 9 Loan Loss Provision",
                    classification: AccountClassification.CurrentLiability,
                    accountType: AccountType.LoanLossProvision,
                    normalBalance: NormalBalanceType.Credit,
                    currencyCode: "NGN",
                    ifrsCategory: "IFRS9",
                    accountLevel: 1
                ),
                // Add more accounts as per IFRS template
            };
        }
    }
}
