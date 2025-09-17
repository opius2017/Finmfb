using System;

namespace FinTech.Core.Domain.Entities.Loans
{
    public class LoanCollateral
    {
        public Guid Id { get; set; }
        public string? LoanId { get; set; }
        public string? CollateralType { get; set; }
        public decimal Value { get; set; }
        public string? Description { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
