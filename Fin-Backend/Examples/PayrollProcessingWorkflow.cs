using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Domain.Entities.Payroll;
using FinTech.Infrastructure.Data;
using FinTech.Application.Services.Integration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTech.Examples
{
    public class PayrollProcessingWorkflow
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IPayrollAccountingIntegrationService _payrollAccountingService;
        private readonly ILogger<PayrollProcessingWorkflow> _logger;

        public PayrollProcessingWorkflow(
            ApplicationDbContext dbContext,
            IPayrollAccountingIntegrationService payrollAccountingService,
            ILogger<PayrollProcessingWorkflow> logger)
        {
            _dbContext = dbContext;
            _payrollAccountingService = payrollAccountingService;
            _logger = logger;
        }

        public async Task RunPayrollCycleWorkflowAsync()
        {
            _logger.LogInformation("Starting Payroll Cycle Workflow Example");
            
            try
            {
                // 1. Create a new payroll period
                var currentMonth = DateTime.UtcNow.ToString("MMM-yyyy");
                var startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);
                var processingDate = endDate.AddDays(2);

                var payrollPeriod = new PayrollPeriod(
                    period: currentMonth,
                    startDate: startDate,
                    endDate: endDate,
                    processingDate: processingDate);

                _logger.LogInformation("Created payroll period for {Period}", currentMonth);
                
                // 2. Add payroll period to context
                _dbContext.Add(payrollPeriod);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Saved new payroll period with ID: {PayrollId}", payrollPeriod.Id);

                // 3. Add employee salary payments
                var employees = new List<(int Id, string Name, decimal Salary)>
                {
                    (1, "John Smith", 120000),
                    (2, "Jane Doe", 150000),
                    (3, "Bob Johnson", 85000),
                    (4, "Alice Williams", 200000),
                    (5, "David Brown", 175000)
                };

                foreach (var employee in employees)
                {
                    // Calculate deductions (simplified for demo)
                    decimal taxRate = 0.1m; // 10% tax rate
                    decimal pensionRate = 0.08m; // 8% pension contribution
                    decimal otherDeductions = employee.Salary * 0.02m; // 2% other deductions

                    decimal taxAmount = employee.Salary * taxRate;
                    decimal pensionAmount = employee.Salary * pensionRate;

                    var salaryPayment = new SalaryPayment(
                        payrollPeriodId: payrollPeriod.Id,
                        employeeId: employee.Id,
                        grossAmount: employee.Salary,
                        taxAmount: taxAmount,
                        pensionAmount: pensionAmount,
                        otherDeductions: otherDeductions,
                        reference: $"SAL{employee.Id}-{currentMonth}");

                    payrollPeriod.AddSalaryPayment(salaryPayment);
                    _logger.LogInformation("Added salary payment for employee {EmployeeName} (ID: {EmployeeId}) with gross amount {GrossAmount} NGN",
                        employee.Name, employee.Id, employee.Salary);
                }

                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Saved salary payments for {Count} employees", employees.Count);

                // 4. Process the payroll
                payrollPeriod.ProcessPayroll();
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Started processing payroll period {Period}", currentMonth);

                // 5. Process individual salary payments
                var salaryPayments = await _dbContext.Set<SalaryPayment>()
                    .Where(sp => sp.PayrollPeriodId == payrollPeriod.Id)
                    .ToListAsync();

                foreach (var payment in salaryPayments)
                {
                    payment.Process($"Salary payment for {currentMonth}");
                }

                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Processed {Count} salary payments", salaryPayments.Count);

                // NOTE: At this point, the SalaryPaymentProcessedEvent has been raised for each payment
                // and automatically processed by the DomainEventService during SaveChangesAsync
                // The PayrollAccountingIntegrationService has created journal entries for each payment

                // 6. Complete the payroll processing
                payrollPeriod.CompleteProcessing();
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Completed payroll processing for period {Period}", currentMonth);

                // 7. Accrue expense for next month's payroll
                payrollPeriod.AccrueExpense(
                    amount: payrollPeriod.TotalGross,
                    expenseType: "SALARY_ACCRUAL",
                    reference: $"ACCR-{DateTime.UtcNow.AddMonths(1).ToString("MMM-yyyy")}",
                    description: $"Salary accrual for {DateTime.UtcNow.AddMonths(1).ToString("MMMM yyyy")}");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Accrued expense for next month's payroll: {Amount} NGN", payrollPeriod.TotalGross);

                // 8. Remit taxes
                payrollPeriod.RemitTaxes(
                    amount: payrollPeriod.TotalTax,
                    taxType: "PAYE",
                    reference: $"TAX-{currentMonth}",
                    description: $"PAYE tax remittance for {currentMonth}");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Remitted taxes: {Amount} NGN", payrollPeriod.TotalTax);

                // 9. Remit pension
                payrollPeriod.RemitPension(
                    amount: payrollPeriod.TotalPension,
                    pensionProvider: "PensionCo",
                    reference: $"PEN-{currentMonth}",
                    description: $"Pension remittance for {currentMonth}");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Remitted pension: {Amount} NGN", payrollPeriod.TotalPension);

                // 10. Create a bonus payment for a specific employee
                var bonusPayment = new BonusPayment(
                    employeeId: 4, // Alice Williams
                    amount: 50000,
                    bonusType: "PERFORMANCE",
                    reference: $"BON-{currentMonth}-EMP4");

                _dbContext.Add(bonusPayment);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Created bonus payment of 50,000 NGN for employee ID 4 (Alice Williams)");

                bonusPayment.Process("Performance bonus for exceptional project delivery");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Processed bonus payment for employee ID 4");

                // 11. Retrieve payroll period with transactions to verify final state
                var updatedPayrollPeriod = await _dbContext.Set<PayrollPeriod>()
                    .Include(p => p.SalaryPayments)
                    .Include(p => p.Transactions)
                    .FirstOrDefaultAsync(p => p.Id == payrollPeriod.Id);

                _logger.LogInformation("Final payroll summary for period {Period}:", currentMonth);
                _logger.LogInformation("Total Gross: {TotalGross} NGN", updatedPayrollPeriod.TotalGross);
                _logger.LogInformation("Total Tax: {TotalTax} NGN", updatedPayrollPeriod.TotalTax);
                _logger.LogInformation("Total Pension: {TotalPension} NGN", updatedPayrollPeriod.TotalPension);
                _logger.LogInformation("Total Other Deductions: {TotalOtherDeductions} NGN", updatedPayrollPeriod.TotalOtherDeductions);
                _logger.LogInformation("Total Net: {TotalNet} NGN", updatedPayrollPeriod.TotalNet);
                _logger.LogInformation("Number of salary payments: {Count}", updatedPayrollPeriod.SalaryPayments.Count);
                _logger.LogInformation("Number of transactions: {Count}", updatedPayrollPeriod.Transactions.Count);

                // For verification, you can query the accounting journal entries created by the integration service
                // var journalEntries = await _dbContext.JournalEntries
                //     .Where(je => je.Reference.StartsWith("SAL") || je.Reference.StartsWith("BON"))
                //     .ToListAsync();
                // _logger.LogInformation("Number of payroll-related journal entries: {Count}", journalEntries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Payroll Cycle Workflow Example");
                throw;
            }
        }
    }

    public static class PayrollProcessingWorkflowExtensions
    {
        public static async Task RunPayrollWorkflowExample(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var workflow = scope.ServiceProvider.GetRequiredService<PayrollProcessingWorkflow>();
            await workflow.RunPayrollCycleWorkflowAsync();
        }

        public static IServiceCollection AddPayrollWorkflowExample(this IServiceCollection services)
        {
            services.AddTransient<PayrollProcessingWorkflow>();
            return services;
        }
    }
}