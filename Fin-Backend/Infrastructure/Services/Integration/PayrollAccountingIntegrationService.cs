using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Interfaces.Services.Accounting;
using System.Collections.Generic;
// using FinTech.Core.Application.Interfaces.Integration;
using FinTech.Core.Domain.Entities.Payroll;
using FinTech.Core.Application.DTOs.GeneralLedger.Journal;

namespace FinTech.Infrastructure.Services.Integration
{
    public class PayrollAccountingIntegrationService : FinTech.Core.Application.Interfaces.Services.Integration.IPayrollAccountingIntegrationService
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

        public async Task ProcessPayrollRunAsync(PayrollEntry payrollEntry, string tenantId, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing payroll run for employee {EmployeeId}, Period: {Period}", 
                    payrollEntry.EmployeeId, payrollEntry.PayPeriod);

                // Get chart of accounts
                var payrollExpenseAccountId = await _chartOfAccountService.GetPayrollExpenseAccountIdAsync();
                var cashAccountId = await _chartOfAccountService.GetCashAccountIdAsync();
                var taxPayableAccountId = await _chartOfAccountService.GetTaxPayableAccountIdAsync();
                var pensionPayableAccountId = await _chartOfAccountService.GetPensionPayableAccountIdAsync();

                // Create journal entry for payroll run
                var journalEntry = new
                {
                    Description = $"Payroll run for {payrollEntry.EmployeeName} - {payrollEntry.PayPeriod}",
                    Reference = payrollEntry.PayrollRunId,
                    TransactionDate = payrollEntry.PaymentDate,
                    Lines = new List<object>
                    {
                        // Debit: Payroll Expense
                        new { AccountId = payrollExpenseAccountId, Debit = payrollEntry.GrossSalary, Credit = 0m },
                        // Credit: Tax Payable
                        new { AccountId = taxPayableAccountId, Debit = 0m, Credit = payrollEntry.TaxAmount },
                        // Credit: Pension Payable
                        new { AccountId = pensionPayableAccountId, Debit = 0m, Credit = payrollEntry.PensionAmount },
                        // Credit: Cash (Net Pay)
                        new { AccountId = cashAccountId, Debit = 0m, Credit = payrollEntry.NetSalary }
                    }
                };

                await _journalEntryService.CreateJournalEntryAsync(journalEntry, tenantId);
                
                _logger.LogInformation("Successfully processed payroll run for employee {EmployeeId}", payrollEntry.EmployeeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payroll run for employee {EmployeeId}", payrollEntry.EmployeeId);
                throw;
            }
        }

        public async Task ProcessPayrollTaxesAsync(PayrollEntry payrollEntry, string tenantId, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing payroll taxes for employee {EmployeeId}, Amount: {TaxAmount}", 
                    payrollEntry.EmployeeId, payrollEntry.TaxAmount);

                if (payrollEntry.TaxAmount <= 0)
                {
                    _logger.LogInformation("No tax amount to process for employee {EmployeeId}", payrollEntry.EmployeeId);
                    return;
                }

                // Get chart of accounts
                var taxExpenseAccountId = await _chartOfAccountService.GetTaxExpenseAccountIdAsync();
                var taxPayableAccountId = await _chartOfAccountService.GetTaxPayableAccountIdAsync();

                // Create journal entry for tax payment
                var journalEntry = new
                {
                    Description = $"Payroll tax payment for {payrollEntry.EmployeeName} - {payrollEntry.PayPeriod}",
                    Reference = $"TAX-{payrollEntry.PayrollRunId}",
                    TransactionDate = payrollEntry.PaymentDate,
                    Lines = new List<object>
                    {
                        // Debit: Tax Expense
                        new { AccountId = taxExpenseAccountId, Debit = payrollEntry.TaxAmount, Credit = 0m },
                        // Credit: Tax Payable
                        new { AccountId = taxPayableAccountId, Debit = 0m, Credit = payrollEntry.TaxAmount }
                    }
                };

                await _journalEntryService.CreateJournalEntryAsync(journalEntry, tenantId);
                
                _logger.LogInformation("Successfully processed payroll taxes for employee {EmployeeId}", payrollEntry.EmployeeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payroll taxes for employee {EmployeeId}", payrollEntry.EmployeeId);
                throw;
            }
        }

        public async Task ProcessEmployeeBenefitsAsync(PayrollEntry payrollEntry, string tenantId, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing employee benefits for employee {EmployeeId}", payrollEntry.EmployeeId);

                // Calculate total benefits (pension + other benefits)
                var totalBenefits = payrollEntry.PensionAmount + payrollEntry.OtherDeductions;

                if (totalBenefits <= 0)
                {
                    _logger.LogInformation("No benefits to process for employee {EmployeeId}", payrollEntry.EmployeeId);
                    return;
                }

                // Get chart of accounts
                var benefitsExpenseAccountId = await _chartOfAccountService.GetBenefitsExpenseAccountIdAsync();
                var pensionPayableAccountId = await _chartOfAccountService.GetPensionPayableAccountIdAsync();
                var benefitsPayableAccountId = await _chartOfAccountService.GetBenefitsPayableAccountIdAsync();

                var lines = new List<object>
                {
                    // Debit: Benefits Expense
                    new { AccountId = benefitsExpenseAccountId, Debit = totalBenefits, Credit = 0m }
                };

                // Credit: Pension Payable
                if (payrollEntry.PensionAmount > 0)
                {
                    lines.Add(new { AccountId = pensionPayableAccountId, Debit = 0m, Credit = payrollEntry.PensionAmount });
                }

                // Credit: Other Benefits Payable
                if (payrollEntry.OtherDeductions > 0)
                {
                    lines.Add(new { AccountId = benefitsPayableAccountId, Debit = 0m, Credit = payrollEntry.OtherDeductions });
                }

                // Create journal entry for benefits
                var journalEntry = new
                {
                    Description = $"Employee benefits for {payrollEntry.EmployeeName} - {payrollEntry.PayPeriod}",
                    Reference = $"BEN-{payrollEntry.PayrollRunId}",
                    TransactionDate = payrollEntry.PaymentDate,
                    Lines = lines
                };

                await _journalEntryService.CreateJournalEntryAsync(journalEntry, tenantId);
                
                _logger.LogInformation("Successfully processed employee benefits for employee {EmployeeId}", payrollEntry.EmployeeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing employee benefits for employee {EmployeeId}", payrollEntry.EmployeeId);
                throw;
            }
        }
    }
}
