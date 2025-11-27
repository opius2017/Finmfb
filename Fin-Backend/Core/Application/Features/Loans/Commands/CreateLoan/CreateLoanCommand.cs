using MediatR;
using FinTech.Core.Application.Common.Models;
using System.Collections.Generic;

namespace FinTech.Core.Application.Features.Loans.Commands.CreateLoan
{
    public record CreateLoanCommand : IRequest<Result<CreateLoanResponse>>
    {
        public string CustomerId { get; init; } = string.Empty;
        public string LoanProductId { get; init; } = string.Empty;
        public decimal LoanAmount { get; init; }
        public int TenorInMonths { get; init; }
        public string Purpose { get; init; } = string.Empty;
        public List<GuarantorDto> Guarantors { get; init; } = new();
        public List<CollateralDto> Collaterals { get; init; } = new();
    }

    public record GuarantorDto
    {
        public string Name { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public string Relationship { get; init; } = string.Empty;
    }

    public record CollateralDto
    {
        public string Type { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal EstimatedValue { get; init; }
    }
}