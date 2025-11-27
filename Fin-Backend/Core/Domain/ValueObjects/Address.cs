using System.Collections.Generic;
using FinTech.Core.Domain.Common;

namespace FinTech.Core.Domain.ValueObjects
{
    public sealed class Address : ValueObject
    {
        public string Street { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string Country { get; private set; }
        public string PostalCode { get; private set; }

        private Address() 
        {
            Street = string.Empty;
            City = string.Empty;
            State = string.Empty;
            Country = string.Empty;
            PostalCode = string.Empty;
        }

        private Address(string street, string city, string state, string country, string postalCode)
        {
            Street = street;
            City = city;
            State = state;
            Country = country;
            PostalCode = postalCode;
        }

        public static Address Create(string street, string city, string state, string country, string postalCode)
        {
            if (string.IsNullOrWhiteSpace(street))
                throw new System.ArgumentException("Street is required", nameof(street));
            if (string.IsNullOrWhiteSpace(city))
                throw new System.ArgumentException("City is required", nameof(city));
            if (string.IsNullOrWhiteSpace(state))
                throw new System.ArgumentException("State is required", nameof(state));
            if (string.IsNullOrWhiteSpace(country))
                throw new System.ArgumentException("Country is required", nameof(country));

            return new Address(street, city, state, country, postalCode ?? string.Empty);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return State;
            yield return Country;
            yield return PostalCode;
        }

        public override string ToString() => $"{Street}, {City}, {State}, {Country} {PostalCode}";
    }
}
