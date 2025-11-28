using MediatR;
using FinTech.Core.Application.Common.Models;

namespace FinTech.Core.Application.Features.AccountsReceivable.Queries.GetAgingReport;

public record GetAgingReportQuery : IRequest<Result<AgingReportDto>>
{
    public DateTime AsOfDate { get; init; }
    public string? CustomerId { get; init; }
    public bool IncludeZeroBalances { get; init; }
}

public record AgingReportDto
{
    public DateTime AsOfDate { get; init; }
    public DateTime GeneratedDate { get; init; }
    public AgingSummaryDto Summary { get; init; } = new();
    public List<CustomerAgingDto> CustomerAging { get; init; } = new();
}

public record AgingSummaryDto
{
    public decimal TotalOutstanding { get; init; }
    public decimal Current { get; init; }
    public decimal Days1To30 { get; init; }
    public decimal Days31To60 { get; init; }
    public decimal Days61To90 { get; init; }
    public decimal Days91To120 { get; init; }
    public decimal Over120Days { get; init; }
    public int TotalCustomers { get; init; }
    public int CustomersOverdue { get; init; }
}

public record CustomerAgingDto
{
    public string CustomerId { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerCode { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    
    public decimal TotalOutstanding { get; init; }
    public decimal Current { get; init; }
    public decimal Days1To30 { get; init; }
    public decimal Days31To60 { get; init; }
    public decimal Days61To90 { get; init; }
    public decimal Days91To120 { get; init; }
    public decimal Over120Days { get; init; }
    
    public decimal CreditLimit { get; init; }
    public decimal AvailableCredit { get; init; }
    public int DaysOverdue { get; init; }
    public string RiskLevel { get; init; } = string.Empty;
    
    public List<InvoiceAgingDto> Invoices { get; init; } = new();
}

public record InvoiceAgingDto
{
    public string InvoiceId { get; init; } = string.Empty;
    public string InvoiceNumber { get; init; } = string.Empty;
    public DateTime InvoiceDate { get; init; }
    public DateTime DueDate { get; init; }
    public decimal InvoiceAmount { get; init; }
    public decimal AmountPaid { get; init; }
    public decimal Balance { get; init; }
    public int DaysOverdue { get; init; }
    public string Status { get; init; } = string.Empty;
}
