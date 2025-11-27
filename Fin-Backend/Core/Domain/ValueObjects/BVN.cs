using FinTech.Core.Domain.Common;

namespace FinTech.Core.Domain.ValueObjects;

/// <summary>
/// Bank Verification Number (BVN) value object
/// </summary>
public sealed class BVN : ValueObject
{
    public string Value { get; private set; }

    private BVN(string value)
    {
        Value = value;
    }

    public static BVN Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("BVN cannot be empty", nameof(value));
        }

        // Remove any spaces or dashes
        value = value.Replace(" ", "").Replace("-", "");

        // BVN must be exactly 11 digits
        if (value.Length != 11)
        {
            throw new ArgumentException("BVN must be exactly 11 digits", nameof(value));
        }

        // BVN must contain only digits
        if (!value.All(char.IsDigit))
        {
            throw new ArgumentException("BVN must contain only digits", nameof(value));
        }

        return new BVN(value);
    }

    public static bool TryCreate(string value, out BVN? bvn)
    {
        try
        {
            bvn = Create(value);
            return true;
        }
        catch
        {
            bvn = null;
            return false;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(BVN bvn) => bvn.Value;
}
