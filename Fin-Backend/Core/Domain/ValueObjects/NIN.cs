using FinTech.Core.Domain.Common;

namespace FinTech.Core.Domain.ValueObjects;

/// <summary>
/// National Identification Number (NIN) value object
/// </summary>
public sealed class NIN : ValueObject
{
    public string Value { get; private set; }

    private NIN(string value)
    {
        Value = value;
    }

    public static NIN Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("NIN cannot be empty", nameof(value));
        }

        // Remove any spaces or dashes
        value = value.Replace(" ", "").Replace("-", "");

        // NIN must be exactly 11 digits
        if (value.Length != 11)
        {
            throw new ArgumentException("NIN must be exactly 11 digits", nameof(value));
        }

        // NIN must contain only digits
        if (!value.All(char.IsDigit))
        {
            throw new ArgumentException("NIN must contain only digits", nameof(value));
        }

        return new NIN(value);
    }

    public static bool TryCreate(string value, out NIN? nin)
    {
        try
        {
            nin = Create(value);
            return true;
        }
        catch
        {
            nin = null;
            return false;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(NIN nin) => nin.Value;
}
