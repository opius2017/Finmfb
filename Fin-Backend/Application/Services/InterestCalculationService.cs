using FinTech.Domain.Enums;

namespace FinTech.Core.Application.Services;

public interface IInterestCalculationService
{
    decimal CalculateSimpleInterest(decimal principal, decimal rate, int days);
    decimal CalculateCompoundInterest(decimal principal, decimal rate, int days, int compoundingFrequency);
    decimal CalculateInstallmentAmount(decimal principal, decimal rate, int days, RepaymentFrequency frequency);
    decimal CalculateInterestAmount(decimal outstandingPrincipal, decimal rate, RepaymentFrequency frequency);
    int GetNumberOfInstallments(int tenorDays, RepaymentFrequency frequency);
    DateTime GetNextPaymentDate(DateTime currentDate, RepaymentFrequency frequency);
    decimal CalculateDailyInterest(decimal principal, decimal annualRate);
}

public class InterestCalculationService : IInterestCalculationService
{
    public decimal CalculateSimpleInterest(decimal principal, decimal rate, int days)
    {
        return principal * (rate / 100) * (days / 365m);
    }

    public decimal CalculateCompoundInterest(decimal principal, decimal rate, int days, int compoundingFrequency)
    {
        var periodsPerYear = compoundingFrequency;
        var years = days / 365m;
        var ratePerPeriod = (rate / 100) / periodsPerYear;
        var numberOfPeriods = periodsPerYear * years;

        return principal * (decimal)Math.Pow((double)(1 + ratePerPeriod), (double)numberOfPeriods) - principal;
    }

    public decimal CalculateInstallmentAmount(decimal principal, decimal rate, int days, RepaymentFrequency frequency)
    {
        var numberOfPayments = GetNumberOfInstallments(days, frequency);
        var periodicRate = GetPeriodicRate(rate, frequency);

        if (periodicRate == 0) return principal / numberOfPayments;

        var numerator = periodicRate * (decimal)Math.Pow((double)(1 + periodicRate), numberOfPayments);
        var denominator = (decimal)Math.Pow((double)(1 + periodicRate), numberOfPayments) - 1;

        return principal * (numerator / denominator);
    }

    public decimal CalculateInterestAmount(decimal outstandingPrincipal, decimal rate, RepaymentFrequency frequency)
    {
        var periodicRate = GetPeriodicRate(rate, frequency);
        return outstandingPrincipal * periodicRate;
    }

    public int GetNumberOfInstallments(int tenorDays, RepaymentFrequency frequency)
    {
        return frequency switch
        {
            RepaymentFrequency.Daily => tenorDays,
            RepaymentFrequency.Weekly => tenorDays / 7,
            RepaymentFrequency.BiWeekly => tenorDays / 14,
            RepaymentFrequency.Monthly => tenorDays / 30,
            RepaymentFrequency.Quarterly => tenorDays / 90,
            RepaymentFrequency.SemiAnnually => tenorDays / 180,
            RepaymentFrequency.Annually => tenorDays / 365,
            _ => tenorDays / 30
        };
    }

    public DateTime GetNextPaymentDate(DateTime currentDate, RepaymentFrequency frequency)
    {
        return frequency switch
        {
            RepaymentFrequency.Daily => currentDate.AddDays(1),
            RepaymentFrequency.Weekly => currentDate.AddDays(7),
            RepaymentFrequency.BiWeekly => currentDate.AddDays(14),
            RepaymentFrequency.Monthly => currentDate.AddMonths(1),
            RepaymentFrequency.Quarterly => currentDate.AddMonths(3),
            RepaymentFrequency.SemiAnnually => currentDate.AddMonths(6),
            RepaymentFrequency.Annually => currentDate.AddYears(1),
            _ => currentDate.AddMonths(1)
        };
    }

    public decimal CalculateDailyInterest(decimal principal, decimal annualRate)
    {
        return principal * (annualRate / 100) / 365;
    }

    private decimal GetPeriodicRate(decimal annualRate, RepaymentFrequency frequency)
    {
        var periodsPerYear = frequency switch
        {
            RepaymentFrequency.Daily => 365,
            RepaymentFrequency.Weekly => 52,
            RepaymentFrequency.BiWeekly => 26,
            RepaymentFrequency.Monthly => 12,
            RepaymentFrequency.Quarterly => 4,
            RepaymentFrequency.SemiAnnually => 2,
            RepaymentFrequency.Annually => 1,
            _ => 12
        };

        return (annualRate / 100) / periodsPerYear;
    }
}