using MediatR;
using Microsoft.EntityFrameworkCore;
using FinTech.Core.Application.Common.Models;
using FinTech.Core.Domain.Repositories;
using FinTech.Core.Domain.Entities.AccountsReceivable;

namespace FinTech.Core.Application.Features.AccountsReceivable.Queries.GetAgingReport;

public class GetAgingReportQueryHandler : IRequestHandler<GetAgingReportQuery, Result<AgingReportDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAgingReportQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AgingReportDto>> Handle(GetAgingReportQuery request, CancellationToken cancellationToken)
    {
        var asOfDate = request.AsOfDate == default ? DateTime.UtcNow.Date : request.AsOfDate.Date;

        // Get all outstanding invoices
        var invoicesQuery = _unitOfWork.Repository<Invoice>()
            .GetAll()
            .Include(i => i.Customer)
            .Where(i => i.OutstandingAmount > 0 && i.InvoiceDate <= asOfDate);

        if (!string.IsNullOrEmpty(request.CustomerId))
        {
            // Assuming request.CustomerId is a Guid string
            if (Guid.TryParse(request.CustomerId, out var customerIdGuid))
            {
                invoicesQuery = invoicesQuery.Where(i => i.CustomerId == customerIdGuid);
            }
        }

        var invoices = await invoicesQuery.ToListAsync(cancellationToken);

        // Group by customer
        var customerGroups = invoices.GroupBy(i => new
        {
            i.CustomerId,
            CustomerName = i.Customer != null ? i.Customer.GetFullName() : "Unknown"
        });

        var customerAgingList = new List<CustomerAgingDto>();

        foreach (var group in customerGroups)
        {
            var customerInvoices = group.ToList();
            var invoiceAgingList = new List<InvoiceAgingDto>();

            decimal current = 0, days1To30 = 0, days31To60 = 0, days61To90 = 0, days91To120 = 0, over120 = 0;
            int maxDaysOverdue = 0;

            foreach (var invoice in customerInvoices)
            {
                var daysOverdue = (asOfDate - invoice.DueDate).Days;
                if (daysOverdue < 0) daysOverdue = 0;

                if (daysOverdue > maxDaysOverdue)
                    maxDaysOverdue = daysOverdue;

                // Categorize by aging bucket
                if (daysOverdue == 0)
                    current += invoice.OutstandingAmount;
                else if (daysOverdue <= 30)
                    days1To30 += invoice.OutstandingAmount;
                else if (daysOverdue <= 60)
                    days31To60 += invoice.OutstandingAmount;
                else if (daysOverdue <= 90)
                    days61To90 += invoice.OutstandingAmount;
                else if (daysOverdue <= 120)
                    days91To120 += invoice.OutstandingAmount;
                else
                    over120 += invoice.OutstandingAmount;

                invoiceAgingList.Add(new InvoiceAgingDto
                {
                    InvoiceId = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    InvoiceDate = invoice.InvoiceDate,
                    DueDate = invoice.DueDate,
                    InvoiceAmount = invoice.TotalAmount,
                    AmountPaid = invoice.TotalAmount - invoice.OutstandingAmount,
                    Balance = invoice.OutstandingAmount,
                    DaysOverdue = daysOverdue,
                    Status = invoice.Status.ToString()
                });
            }

            var totalOutstanding = current + days1To30 + days31To60 + days61To90 + days91To120 + over120;

            // Determine risk level
            string riskLevel = "Low";
            if (over120 > 0 || days91To120 > totalOutstanding * 0.3m)
                riskLevel = "High";
            else if (days61To90 > totalOutstanding * 0.3m)
                riskLevel = "Medium";

            if (totalOutstanding > 0 || request.IncludeZeroBalances)
            {
                customerAgingList.Add(new CustomerAgingDto
                {
                    CustomerId = group.Key.CustomerId.ToString(),
                    CustomerName = group.Key.CustomerName ?? "Unknown",
                    CustomerCode = group.Key.CustomerId.ToString(),
                    TotalOutstanding = totalOutstanding,
                    Current = current,
                    Days1To30 = days1To30,
                    Days31To60 = days31To60,
                    Days61To90 = days61To90,
                    Days91To120 = days91To120,
                    Over120Days = over120,
                    CreditLimit = 0, // Get from customer record
                    AvailableCredit = 0,
                    DaysOverdue = maxDaysOverdue,
                    RiskLevel = riskLevel,
                    Invoices = invoiceAgingList.OrderByDescending(i => i.DaysOverdue).ToList()
                });
            }
        }

        // Calculate summary
        var summary = new AgingSummaryDto
        {
            TotalOutstanding = customerAgingList.Sum(c => c.TotalOutstanding),
            Current = customerAgingList.Sum(c => c.Current),
            Days1To30 = customerAgingList.Sum(c => c.Days1To30),
            Days31To60 = customerAgingList.Sum(c => c.Days31To60),
            Days61To90 = customerAgingList.Sum(c => c.Days61To90),
            Days91To120 = customerAgingList.Sum(c => c.Days91To120),
            Over120Days = customerAgingList.Sum(c => c.Over120Days),
            TotalCustomers = customerAgingList.Count,
            CustomersOverdue = customerAgingList.Count(c => c.DaysOverdue > 0)
        };

        var report = new AgingReportDto
        {
            AsOfDate = asOfDate,
            GeneratedDate = DateTime.UtcNow,
            Summary = summary,
            CustomerAging = customerAgingList.OrderByDescending(c => c.TotalOutstanding).ToList()
        };

        return Result<AgingReportDto>.Success(report);
    }
}
