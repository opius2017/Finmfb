using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class CreateLoanProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal InterestRate { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public int MinTenureMonths { get; set; }
        public int MaxTenureMonths { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateLoanProductDto : CreateLoanProductDto
    {
        public string Id { get; set; } = string.Empty;
    }
}
