using FinTech.Core.Domain.Common;

namespace FinTech.Core.Domain.ValueObjects;

/// <summary>
/// Bank Account Number value object
/// </summary>
public sealed class AccountNumber : ValueObject
{
    public string Value { get; private set; }

    private AccountNumber(string value)
    {
        Value = value;
    }

    public static AccountNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Account number cannot be empty", nameof(value));
        }

        // Remove any spaces or dashes
        value = value.Replace(" ", "").Replace("-", "");

        // Account number must be exactly 10 digits (NUBAN standard)
        if (value.Length != 10)
        {
            throw new ArgumentException("Account number must be exactly 10 digits", nameof(value));
        }

        // Account number must contain only digits
        if (!value.All(char.IsDigit))
        {
            throw new ArgumentException("Account number must contain only digits", nameof(value));
        }

        // Validate NUBAN check digit
        if (!ValidateNubanCheckDigit(value))
        {
            throw new ArgumentException("Invalid account number check digit", nameof(value));
        }

        return new AccountNumber(value);
    }

    public static bool TryCreate(string value, out AccountNumber? accountNumber)
    {
        try
        {
            accountNumber = Create(value);
            return true;
        }
        catch
        {
            accountNumber = null;
            return false;
        }
    }

    private static bool ValidateNubanCheckDigit(string accountNumber)
    {
        // NUBAN validation algorithm
        // The last digit is a check digit calculated using modulus 10
        int[] weights = { 3, 7, 3, 3, 7, 3, 3, 7, 3 };
        int sum = 0;

        for (int i = 0; i < 9; i++)
        {
            sum += int.Parse(accountNumber[i].ToString()) * weights[i];
        }

        int checkDigit = (10 - (sum % 10)) % 10;
        return checkDigit == int.Parse(accountNumber[9].ToString());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public string ToFormattedString() => Value; // Can add formatting if needed

    public static implicit operator string(AccountNumber accountNumber) => accountNumber.Value;
}
