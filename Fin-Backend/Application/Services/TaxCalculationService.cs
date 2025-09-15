using FinTech.Domain.Entities.Payroll;

namespace FinTech.Core.Application.Services;

public interface ITaxCalculationService
{
    decimal CalculatePAYE(decimal grossSalary, decimal annualRelief = 200000m);
    decimal CalculatePension(decimal grossSalary, decimal rate = 0.08m);
    decimal CalculateNHF(decimal grossSalary, decimal rate = 0.025m);
    decimal CalculateVAT(decimal amount, decimal rate = 0.075m);
    decimal CalculateWHT(decimal amount, decimal rate = 0.05m);
    PayrollTaxBreakdown CalculatePayrollTaxes(Employee employee, decimal grossEarnings);
}

public class TaxCalculationService : ITaxCalculationService
{
    public decimal CalculatePAYE(decimal grossSalary, decimal annualRelief = 200000m)
    {
        var annualSalary = grossSalary * 12;
        var taxableIncome = Math.Max(0, annualSalary - annualRelief);

        decimal tax = 0;

        // Nigerian PAYE Tax Bands (2024)
        if (taxableIncome > 0)
        {
            // First ₦300,000 at 7%
            var band1 = Math.Min(taxableIncome, 300000);
            tax += band1 * 0.07m;
            taxableIncome -= band1;
        }

        if (taxableIncome > 0)
        {
            // Next ₦300,000 at 11%
            var band2 = Math.Min(taxableIncome, 300000);
            tax += band2 * 0.11m;
            taxableIncome -= band2;
        }

        if (taxableIncome > 0)
        {
            // Next ₦500,000 at 15%
            var band3 = Math.Min(taxableIncome, 500000);
            tax += band3 * 0.15m;
            taxableIncome -= band3;
        }

        if (taxableIncome > 0)
        {
            // Next ₦500,000 at 19%
            var band4 = Math.Min(taxableIncome, 500000);
            tax += band4 * 0.19m;
            taxableIncome -= band4;
        }

        if (taxableIncome > 0)
        {
            // Next ₦1,600,000 at 21%
            var band5 = Math.Min(taxableIncome, 1600000);
            tax += band5 * 0.21m;
            taxableIncome -= band5;
        }

        if (taxableIncome > 0)
        {
            // Above ₦3,200,000 at 24%
            tax += taxableIncome * 0.24m;
        }

        return Math.Round(tax / 12, 2); // Monthly PAYE
    }

    public decimal CalculatePension(decimal grossSalary, decimal rate = 0.08m)
    {
        // Employee contribution is 8% of gross salary
        return Math.Round(grossSalary * rate, 2);
    }

    public decimal CalculateNHF(decimal grossSalary, decimal rate = 0.025m)
    {
        // National Housing Fund is 2.5% of gross salary
        return Math.Round(grossSalary * rate, 2);
    }

    public decimal CalculateVAT(decimal amount, decimal rate = 0.075m)
    {
        // Value Added Tax is 7.5%
        return Math.Round(amount * rate, 2);
    }

    public decimal CalculateWHT(decimal amount, decimal rate = 0.05m)
    {
        // Withholding Tax varies by transaction type, default 5%
        return Math.Round(amount * rate, 2);
    }

    public PayrollTaxBreakdown CalculatePayrollTaxes(Employee employee, decimal grossEarnings)
    {
        var breakdown = new PayrollTaxBreakdown
        {
            EmployeeId = employee.Id,
            GrossEarnings = grossEarnings,
            PAYEDeduction = CalculatePAYE(grossEarnings),
            PensionDeduction = CalculatePension(grossEarnings),
            NHFDeduction = CalculateNHF(grossEarnings)
        };

        // Add other deductions if applicable
        breakdown.TotalDeductions = breakdown.PAYEDeduction + 
                                   breakdown.PensionDeduction + 
                                   breakdown.NHFDeduction;

        breakdown.NetPay = grossEarnings - breakdown.TotalDeductions;

        return breakdown;
    }
}

public class PayrollTaxBreakdown
{
    public Guid EmployeeId { get; set; }
    public decimal GrossEarnings { get; set; }
    public decimal PAYEDeduction { get; set; }
    public decimal PensionDeduction { get; set; }
    public decimal NHFDeduction { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay { get; set; }
}