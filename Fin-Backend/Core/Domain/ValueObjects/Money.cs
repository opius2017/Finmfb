namespace FinTech.Core.Domain.ValueObjects
{
    /// <summary>
    /// Represents a monetary value with currency information.
    /// </summary>
    public class Money
    {
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }

        public Money(decimal amount, string currencyCode = "NGN")
        {
            Amount = amount;
            CurrencyCode = currencyCode;
        }

        public static Money operator +(Money a, Money b)
        {
            if (a.CurrencyCode != b.CurrencyCode)
            {
                throw new System.InvalidOperationException("Cannot add money values with different currencies");
            }
            return new Money(a.Amount + b.Amount, a.CurrencyCode);
        }

        public static Money operator -(Money a, Money b)
        {
            if (a.CurrencyCode != b.CurrencyCode)
            {
                throw new System.InvalidOperationException("Cannot subtract money values with different currencies");
            }
            return new Money(a.Amount - b.Amount, a.CurrencyCode);
        }

        public static Money operator *(Money a, decimal factor)
        {
            return new Money(a.Amount * factor, a.CurrencyCode);
        }

        public static Money operator /(Money a, decimal divisor)
        {
            return new Money(a.Amount / divisor, a.CurrencyCode);
        }

        public override string ToString()
        {
            return $"{Amount:N2} {CurrencyCode}";
        }
    }
}