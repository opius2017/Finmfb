using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Application.Services.Accounting;
using FinTech.Domain.Entities.Accounting;
using FinTech.Domain.Entities.Payroll;
using FinTech.Domain.Repositories.Accounting;

namespace FinTech.Application.Services.Integration
{
    public interface IPayrollAccountingIntegrationService
    {
        Task<string> ProcessPayrollRunAsync(
            PayrollEntry payrollEntry, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
            
        Task<string> ProcessPayrollTaxesAsync(
            PayrollEntry payrollEntry, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
            
        Task<string> ProcessEmployeeBenefitsAsync(
            PayrollEntry payrollEntry, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
    }

    public class PayrollAccountingIntegrationService : IPayrollAccountingIntegrationService
    {
        private readonly IJournalEntryService _journalEntryService;
        private readonly IChartOfAccountService _chartOfAccountService;
        private readonly IFinancialPeriodService _financialPeriodService;
        private readonly IChartOfAccountRepository _chartOfAccountRepository;

        public PayrollAccountingIntegrationService(
            IJournalEntryService journalEntryService,
            IChartOfAccountService chartOfAccountService,
            IFinancialPeriodService financialPeriodService,
            IChartOfAccountRepository chartOfAccountRepository)
        {
            _journalEntryService = journalEntryService ?? throw new ArgumentNullException(nameof(journalEntryService));
            _chartOfAccountService = chartOfAccountService ?? throw new ArgumentNullException(nameof(chartOfAccountService));
            _financialPeriodService = financialPeriodService ?? throw new ArgumentNullException(nameof(financialPeriodService));
            _chartOfAccountRepository = chartOfAccountRepository ?? throw new ArgumentNullException(nameof(chartOfAccountRepository));
        }

        public async Task<string> ProcessPayrollRunAsync(
            PayrollEntry payrollEntry, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            // Validate inputs
            if (payrollEntry == null)
                throw new ArgumentNullException(nameof(payrollEntry));

            if (payrollEntry.TotalGrossSalary <= 0)
                throw new ArgumentException("Gross salary must be greater than zero", nameof(payrollEntry));

            // Get the relevant GL accounts
            var salaryExpenseAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("5100", cancellationToken);  // Salary expense
            var payrollPayableAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("2100", cancellationToken);  // Payroll payable
            var taxPayableAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("2200", cancellationToken);  // Tax payable
            var bankAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("1010", cancellationToken);  // Bank account

            if (salaryExpenseAccount == null || payrollPayableAccount == null || taxPayableAccount == null || bankAccount == null)
                throw new InvalidOperationException("Required GL accounts not found");

            // Calculate values
            decimal totalGrossSalary = payrollEntry.TotalGrossSalary;
            decimal totalTaxes = payrollEntry.TotalTaxes;
            decimal totalDeductions = payrollEntry.TotalDeductions;
            decimal netPayment = totalGrossSalary - totalTaxes - totalDeductions;

            // Create journal entry
            var journalEntry = new JournalEntry
            {
                Description = $"Payroll processing for period {payrollEntry.PayPeriod}",
                EntryDate = payrollEntry.PayrollDate,
                EntryType = JournalEntryType.Standard,
                FinancialPeriodId = financialPeriodId,
                IsSystemGenerated = true,
                CreatedBy = "PayrollIntegration",
                JournalEntryLines = new List<JournalEntryLine>
                {
                    // Debit Salary Expense
                    new JournalEntryLine
                    {
                        AccountId = salaryExpenseAccount.Id,
                        Description = $"Gross salaries for period {payrollEntry.PayPeriod}",
                        DebitAmount = totalGrossSalary,
                        CreditAmount = 0,
                        CreatedBy = "PayrollIntegration"
                    },
                    // Credit Tax Payable
                    new JournalEntryLine
                    {
                        AccountId = taxPayableAccount.Id,
                        Description = $"Payroll taxes for period {payrollEntry.PayPeriod}",
                        DebitAmount = 0,
                        CreditAmount = totalTaxes,
                        CreatedBy = "PayrollIntegration"
                    },
                    // Credit Payroll Payable
                    new JournalEntryLine
                    {
                        AccountId = payrollPayableAccount.Id,
                        Description = $"Other deductions for period {payrollEntry.PayPeriod}",
                        DebitAmount = 0,
                        CreditAmount = totalDeductions,
                        CreatedBy = "PayrollIntegration"
                    },
                    // Credit Bank Account
                    new JournalEntryLine
                    {
                        AccountId = bankAccount.Id,
                        Description = $"Net salary payments for period {payrollEntry.PayPeriod}",
                        DebitAmount = 0,
                        CreditAmount = netPayment,
                        CreatedBy = "PayrollIntegration"
                    }
                }
            };

            // Create, submit, approve, and post the journal entry
            var journalEntryId = await _journalEntryService.CreateJournalEntryAsync(journalEntry, cancellationToken);
            await _journalEntryService.SubmitForApprovalAsync(journalEntryId, "PayrollIntegration", cancellationToken);
            await _journalEntryService.ApproveJournalEntryAsync(journalEntryId, "PayrollIntegration", cancellationToken);
            await _journalEntryService.PostJournalEntryAsync(journalEntryId, "PayrollIntegration", cancellationToken);

            return journalEntryId;
        }

        public async Task<string> ProcessPayrollTaxesAsync(
            PayrollEntry payrollEntry, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            // Validate inputs
            if (payrollEntry == null)
                throw new ArgumentNullException(nameof(payrollEntry));

            if (payrollEntry.TotalTaxes <= 0)
                throw new ArgumentException("Tax amount must be greater than zero", nameof(payrollEntry));

            // Get the relevant GL accounts
            var taxPayableAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("2200", cancellationToken);  // Tax payable
            var bankAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("1010", cancellationToken);  // Bank account

            if (taxPayableAccount == null || bankAccount == null)
                throw new InvalidOperationException("Required GL accounts not found");

            // Create journal entry
            var journalEntry = new JournalEntry
            {
                Description = $"Payroll tax payment for period {payrollEntry.PayPeriod}",
                EntryDate = payrollEntry.PayrollDate,
                EntryType = JournalEntryType.Standard,
                FinancialPeriodId = financialPeriodId,
                IsSystemGenerated = true,
                CreatedBy = "PayrollIntegration",
                JournalEntryLines = new List<JournalEntryLine>
                {
                    // Debit Tax Payable
                    new JournalEntryLine
                    {
                        AccountId = taxPayableAccount.Id,
                        Description = $"Payment of payroll taxes for period {payrollEntry.PayPeriod}",
                        DebitAmount = payrollEntry.TotalTaxes,
                        CreditAmount = 0,
                        CreatedBy = "PayrollIntegration"
                    },
                    // Credit Bank Account
                    new JournalEntryLine
                    {
                        AccountId = bankAccount.Id,
                        Description = $"Payment of payroll taxes for period {payrollEntry.PayPeriod}",
                        DebitAmount = 0,
                        CreditAmount = payrollEntry.TotalTaxes,
                        CreatedBy = "PayrollIntegration"
                    }
                }
            };

            // Create, submit, approve, and post the journal entry
            var journalEntryId = await _journalEntryService.CreateJournalEntryAsync(journalEntry, cancellationToken);
            await _journalEntryService.SubmitForApprovalAsync(journalEntryId, "PayrollIntegration", cancellationToken);
            await _journalEntryService.ApproveJournalEntryAsync(journalEntryId, "PayrollIntegration", cancellationToken);
            await _journalEntryService.PostJournalEntryAsync(journalEntryId, "PayrollIntegration", cancellationToken);

            return journalEntryId;
        }

        public async Task<string> ProcessEmployeeBenefitsAsync(
            PayrollEntry payrollEntry, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            // Validate inputs
            if (payrollEntry == null)
                throw new ArgumentNullException(nameof(payrollEntry));

            if (payrollEntry.TotalBenefits <= 0)
                throw new ArgumentException("Benefits amount must be greater than zero", nameof(payrollEntry));

            // Get the relevant GL accounts
            var benefitsExpenseAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("5110", cancellationToken);  // Employee benefits expense
            var bankAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("1010", cancellationToken);  // Bank account

            if (benefitsExpenseAccount == null || bankAccount == null)
                throw new InvalidOperationException("Required GL accounts not found");

            // Create journal entry
            var journalEntry = new JournalEntry
            {
                Description = $"Employee benefits for period {payrollEntry.PayPeriod}",
                EntryDate = payrollEntry.PayrollDate,
                EntryType = JournalEntryType.Standard,
                FinancialPeriodId = financialPeriodId,
                IsSystemGenerated = true,
                CreatedBy = "PayrollIntegration",
                JournalEntryLines = new List<JournalEntryLine>
                {
                    // Debit Benefits Expense
                    new JournalEntryLine
                    {
                        AccountId = benefitsExpenseAccount.Id,
                        Description = $"Employee benefits for period {payrollEntry.PayPeriod}",
                        DebitAmount = payrollEntry.TotalBenefits,
                        CreditAmount = 0,
                        CreatedBy = "PayrollIntegration"
                    },
                    // Credit Bank Account
                    new JournalEntryLine
                    {
                        AccountId = bankAccount.Id,
                        Description = $"Payment of employee benefits for period {payrollEntry.PayPeriod}",
                        DebitAmount = 0,
                        CreditAmount = payrollEntry.TotalBenefits,
                        CreatedBy = "PayrollIntegration"
                    }
                }
            };

            // Create, submit, approve, and post the journal entry
            var journalEntryId = await _journalEntryService.CreateJournalEntryAsync(journalEntry, cancellationToken);
            await _journalEntryService.SubmitForApprovalAsync(journalEntryId, "PayrollIntegration", cancellationToken);
            await _journalEntryService.ApproveJournalEntryAsync(journalEntryId, "PayrollIntegration", cancellationToken);
            await _journalEntryService.PostJournalEntryAsync(journalEntryId, "PayrollIntegration", cancellationToken);

            return journalEntryId;
        }
    }
}