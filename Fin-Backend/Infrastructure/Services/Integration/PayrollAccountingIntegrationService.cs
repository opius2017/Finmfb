using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Services.Integration;
using System.Collections.Generic;
using FinTech.Core.Application.Interfaces.Integration;

namespace FinTech.Infrastructure.Services.Integration
{
    public class PayrollAccountingIntegrationService : IPayrollAccountingIntegrationService
    {
        private readonly ILogger<PayrollAccountingIntegrationService> _logger;
        private readonly IJournalEntryService _journalEntryService;
        private readonly IChartOfAccountService _chartOfAccountService;

        public PayrollAccountingIntegrationService(
            ILogger<PayrollAccountingIntegrationService> logger,
            IJournalEntryService journalEntryService,
            IChartOfAccountService chartOfAccountService)
        {
            _logger = logger;
            _journalEntryService = journalEntryService;
            _chartOfAccountService = chartOfAccountService;
        }

        public async Task ProcessSalaryPaymentAsync(
            int employeeId, 
            decimal grossAmount, 
            decimal taxAmount, 
            decimal pensionAmount, 
            decimal otherDeductions, 
            string payPeriod, 
            string reference, 
            string description)
        {
            try
            {
                decimal netSalary = grossAmount - taxAmount - pensionAmount - otherDeductions;
                
                _logger.LogInformation("Processing salary payment for employee {EmployeeId} with gross amount {GrossAmount}", 
                    employeeId, grossAmount);
                
                // Get the accounts from chart of accounts
                var payrollExpenseAccountId = await _chartOfAccountService.GetPayrollExpenseAccountIdAsync();
                var taxPayableAccountId = await _chartOfAccountService.GetTaxPayableAccountIdAsync();
                var pensionPayableAccountId = await _chartOfAccountService.GetPensionPayableAccountIdAsync();
                var payrollLiabilityAccountId = await _chartOfAccountService.GetPayrollLiabilityAccountIdAsync();
                var cashAccountId = await _chartOfAccountService.GetCashAccountIdAsync();
                
                // Create journal entry lines
                var journalLines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = payrollExpenseAccountId,
                        Description = $"Salary expense for employee {employeeId} for {payPeriod}",
                        DebitAmount = grossAmount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = taxPayableAccountId,
                        Description = $"Tax withholding for employee {employeeId} for {payPeriod}",
                        DebitAmount = 0,
                        CreditAmount = taxAmount
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = pensionPayableAccountId,
                        Description = $"Pension contribution for employee {employeeId} for {payPeriod}",
                        DebitAmount = 0,
                        CreditAmount = pensionAmount
                    }
                };
                
                // Add other deductions if any
                if (otherDeductions > 0)
                {
                    journalLines.Add(new JournalEntryLineDto
                    {
                        AccountId = payrollLiabilityAccountId,
                        Description = $"Other deductions for employee {employeeId} for {payPeriod}",
                        DebitAmount = 0,
                        CreditAmount = otherDeductions
                    });
                }
                
                // Add net salary payment
                journalLines.Add(new JournalEntryLineDto
                {
                    AccountId = cashAccountId,
                    Description = $"Net salary payment to employee {employeeId} for {payPeriod}",
                    DebitAmount = 0,
                    CreditAmount = netSalary
                });
                
                // Create the journal entry
                await _journalEntryService.CreateJournalEntryAsync(
                    new JournalEntryDto
                    {
                        TransactionDate = DateTime.UtcNow,
                        Reference = reference,
                        Description = description,
                        Source = "Payroll",
                        Lines = journalLines
                    });
                
                _logger.LogInformation("Successfully processed salary payment for employee {EmployeeId}", employeeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing salary payment for employee {EmployeeId}", employeeId);
                throw;
            }
        }

        public async Task ProcessPayrollTaxRemittanceAsync(decimal amount, string taxType, string taxPeriod, string reference, string description)
        {
            try
            {
                _logger.LogInformation("Processing payroll tax remittance of {Amount} for {TaxType} for period {TaxPeriod}", 
                    amount, taxType, taxPeriod);
                
                // Get the accounts from chart of accounts
                var taxPayableAccountId = await _chartOfAccountService.GetTaxPayableAccountIdAsync();
                var cashAccountId = await _chartOfAccountService.GetCashAccountIdAsync();
                
                // Create journal entry lines
                var journalLines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = taxPayableAccountId,
                        Description = $"{taxType} tax payable for period {taxPeriod}",
                        DebitAmount = amount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = cashAccountId,
                        Description = $"Cash payment for {taxType} tax for period {taxPeriod}",
                        DebitAmount = 0,
                        CreditAmount = amount
                    }
                };
                
                // Create the journal entry
                await _journalEntryService.CreateJournalEntryAsync(
                    new JournalEntryDto
                    {
                        TransactionDate = DateTime.UtcNow,
                        Reference = reference,
                        Description = description,
                        Source = "Payroll",
                        Lines = journalLines
                    });
                
                _logger.LogInformation("Successfully processed payroll tax remittance for {TaxType}", taxType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payroll tax remittance for {TaxType}", taxType);
                throw;
            }
        }

        public async Task ProcessPensionRemittanceAsync(decimal amount, string pensionProvider, string pensionPeriod, string reference, string description)
        {
            try
            {
                _logger.LogInformation("Processing pension remittance of {Amount} to {PensionProvider} for period {PensionPeriod}", 
                    amount, pensionProvider, pensionPeriod);
                
                // Get the accounts from chart of accounts
                var pensionPayableAccountId = await _chartOfAccountService.GetPensionPayableAccountIdAsync();
                var cashAccountId = await _chartOfAccountService.GetCashAccountIdAsync();
                
                // Create journal entry lines
                var journalLines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = pensionPayableAccountId,
                        Description = $"Pension payable to {pensionProvider} for period {pensionPeriod}",
                        DebitAmount = amount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = cashAccountId,
                        Description = $"Cash payment to {pensionProvider} for period {pensionPeriod}",
                        DebitAmount = 0,
                        CreditAmount = amount
                    }
                };
                
                // Create the journal entry
                await _journalEntryService.CreateJournalEntryAsync(
                    new JournalEntryDto
                    {
                        TransactionDate = DateTime.UtcNow,
                        Reference = reference,
                        Description = description,
                        Source = "Payroll",
                        Lines = journalLines
                    });
                
                _logger.LogInformation("Successfully processed pension remittance to {PensionProvider}", pensionProvider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pension remittance to {PensionProvider}", pensionProvider);
                throw;
            }
        }

        public async Task ProcessBonusPaymentAsync(int employeeId, decimal amount, string reference, string description)
        {
            try
            {
                _logger.LogInformation("Processing bonus payment for employee {EmployeeId} with amount {Amount}", 
                    employeeId, amount);
                
                // Get the accounts from chart of accounts
                var payrollExpenseAccountId = await _chartOfAccountService.GetPayrollExpenseAccountIdAsync();
                var cashAccountId = await _chartOfAccountService.GetCashAccountIdAsync();
                
                // Create journal entry lines
                var journalLines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = payrollExpenseAccountId,
                        Description = $"Bonus expense for employee {employeeId}",
                        DebitAmount = amount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = cashAccountId,
                        Description = $"Cash payment for bonus to employee {employeeId}",
                        DebitAmount = 0,
                        CreditAmount = amount
                    }
                };
                
                // Create the journal entry
                await _journalEntryService.CreateJournalEntryAsync(
                    new JournalEntryDto
                    {
                        TransactionDate = DateTime.UtcNow,
                        Reference = reference,
                        Description = description,
                        Source = "Payroll",
                        Lines = journalLines
                    });
                
                _logger.LogInformation("Successfully processed bonus payment for employee {EmployeeId}", employeeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bonus payment for employee {EmployeeId}", employeeId);
                throw;
            }
        }

        public async Task ProcessPayrollExpenseAccrualAsync(decimal amount, string expenseType, string period, string reference, string description)
        {
            try
            {
                _logger.LogInformation("Processing payroll expense accrual of {Amount} for {ExpenseType} for period {Period}", 
                    amount, expenseType, period);
                
                // Get the accounts from chart of accounts
                var payrollExpenseAccountId = await _chartOfAccountService.GetPayrollExpenseAccountIdAsync();
                var payrollLiabilityAccountId = await _chartOfAccountService.GetPayrollLiabilityAccountIdAsync();
                
                // Create journal entry lines
                var journalLines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = payrollExpenseAccountId,
                        Description = $"{expenseType} expense accrual for period {period}",
                        DebitAmount = amount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = payrollLiabilityAccountId,
                        Description = $"{expenseType} liability accrual for period {period}",
                        DebitAmount = 0,
                        CreditAmount = amount
                    }
                };
                
                // Create the journal entry
                await _journalEntryService.CreateJournalEntryAsync(
                    new JournalEntryDto
                    {
                        TransactionDate = DateTime.UtcNow,
                        Reference = reference,
                        Description = description,
                        Source = "Payroll",
                        Lines = journalLines
                    });
                
                _logger.LogInformation("Successfully processed payroll expense accrual for {ExpenseType}", expenseType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payroll expense accrual for {ExpenseType}", expenseType);
                throw;
            }
        }

        public Task ProcessPayrollRunAsync(PayrollEntry payrollEntry, string tenantId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ProcessPayrollTaxesAsync(PayrollEntry payrollEntry, string tenantId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ProcessEmployeeBenefitsAsync(PayrollEntry payrollEntry, string tenantId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
