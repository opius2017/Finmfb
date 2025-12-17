using Microsoft.EntityFrameworkCore;

namespace FinTech.Core.Domain.Entities.Common
{
    /// <summary>
    /// Money value object to represent currency amounts
    /// </summary>
    [Owned]
    public class Money
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }
        
        private Money() 
        {
            Currency = string.Empty;
        }
        
        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }
        
        public static Money Create(decimal amount, string currency)
        {
            return new Money(amount, currency);
        }
        
        public static Money CreateNaira(decimal amount)
        {
            return new Money(amount, "NGN");
        }
        
        public static Money Zero(string currency)
        {
            return new Money(0, currency);
        }
        
        public static Money ZeroNaira()
        {
            return new Money(0, "NGN");
        }
        
        public Money Add(Money money)
        {
            if (Currency != money.Currency)
                throw new System.InvalidOperationException("Cannot add money with different currencies");
                
            return new Money(Amount + money.Amount, Currency);
        }
        
        public Money Subtract(Money money)
        {
            if (Currency != money.Currency)
                throw new System.InvalidOperationException("Cannot subtract money with different currencies");
                
            return new Money(Amount - money.Amount, Currency);
        }
        
        public Money Multiply(decimal multiplier)
        {
            return new Money(Amount * multiplier, Currency);
        }
    }
}
