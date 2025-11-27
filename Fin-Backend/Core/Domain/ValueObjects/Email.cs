using System.Collections.Generic;
using System.Linq;
using FinTech.Core.Domain.Common;

namespace FinTech.Core.Domain.ValueObjects
{
    public sealed class Email : ValueObject
    {
        public string Value { get; private set; }

        private Email() { Value = string.Empty; }

        private Email(string value)
        {
            Value = value;
        }

        public static Email Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new System.ArgumentException("Email is required", nameof(email));

            email = email.Trim().ToLowerInvariant();

            if (!IsValidEmail(email))
                throw new System.ArgumentException("Invalid email format", nameof(email));

            return new Email(email);
        }

        private static bool IsValidEmail(string email)
        {
            var emailRegex = new System.Text.RegularExpressions.Regex(
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return emailRegex.IsMatch(email);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        public static implicit operator string(Email email) => email.Value;
    }
}
