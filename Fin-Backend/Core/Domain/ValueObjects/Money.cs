using FinTech.Core.Domain.Common;

using Microsoft.EntityFrameworkCore;

namespace FinTech.Core.Domain.ValueObjects;

/// <summary>
/// Money value object with currency support
/// </summary>
[Owned]
public sealed class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "NGN")
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency cannot be empty", nameof(currency));
        }

        if (currency.Length != 3)
        {
            throw new ArgumentException("Currency must be a 3-letter ISO code", nameof(currency));
        }

        return new Money(Math.Round(amount, 2), currency.ToUpperInvariant());
    }

    public static Money Zero(string currency = "NGN") => Create(0, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException($"Cannot add money with different currencies: {Currency} and {other.Currency}");
        }

        return Create(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException($"Cannot subtract money with different currencies: {Currency} and {other.Currency}");
        }

        return Create(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal multiplier)
    {
        return Create(Amount * multiplier, Currency);
    }

    public Money Divide(decimal divisor)
    {
        if (divisor == 0)
        {
            throw new DivideByZeroException("Cannot divide money by zero");
        }

        return Create(Amount / divisor, Currency);
    }

    public bool IsPositive() => Amount > 0;
    public bool IsNegative() => Amount < 0;
    public bool IsZero() => Amount == 0;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Currency} {Amount:N2}";

    public string ToFormattedString() => Amount.ToString("N2");

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money money, decimal multiplier) => money.Multiply(multiplier);
    public static Money operator /(Money money, decimal divisor) => money.Divide(divisor);

    public static bool operator >(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException($"Cannot compare money with different currencies: {left.Currency} and {right.Currency}");
        }
        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException($"Cannot compare money with different currencies: {left.Currency} and {right.Currency}");
        }
        return left.Amount < right.Amount;
    }

    public static bool operator >=(Money left, Money right) => !(left < right);
    public static bool operator <=(Money left, Money right) => !(left > right);
}
