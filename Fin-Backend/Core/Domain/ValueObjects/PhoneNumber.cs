using System.Collections.Generic;
using FinTech.Core.Domain.Common;

namespace FinTech.Core.Domain.ValueObjects
{
    public sealed class PhoneNumber : ValueObject
    {
        public string Value { get; private set; }

        private PhoneNumber() { Value = string.Empty; }

        private PhoneNumber(string value)
        {
            Value = value;
        }

        public static PhoneNumber Create(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new System.ArgumentException("Phone number is required", nameof(phoneNumber));

            phoneNumber = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"[^\d]", "");

            if (phoneNumber.Length < 10 || phoneNumber.Length > 15)
                throw new System.ArgumentException("Phone number must be between 10 and 15 digits", nameof(phoneNumber));

            return new PhoneNumber(phoneNumber);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
    }
}
