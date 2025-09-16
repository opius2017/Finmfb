using System.Threading.Tasks;

namespace FinTech.Application.Services.Integration
{
    public interface IPayrollAccountingIntegrationService
    {
        Task ProcessSalaryPaymentAsync(int employeeId, decimal grossAmount, decimal taxAmount, decimal pensionAmount, 
            decimal otherDeductions, string payPeriod, string reference, string description);
        Task ProcessPayrollTaxRemittanceAsync(decimal amount, string taxType, string taxPeriod, string reference, string description);
        Task ProcessPensionRemittanceAsync(decimal amount, string pensionProvider, string pensionPeriod, string reference, string description);
        Task ProcessBonusPaymentAsync(int employeeId, decimal amount, string reference, string description);
        Task ProcessPayrollExpenseAccrualAsync(decimal amount, string expenseType, string period, string reference, string description);
    }
}