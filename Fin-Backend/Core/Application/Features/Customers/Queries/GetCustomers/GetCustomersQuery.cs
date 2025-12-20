using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.Common.Models;
using FinTech.Core.Application.DTOs.Customers;
using FinTech.Core.Domain.Entities.Customers;

namespace FinTech.Core.Application.Features.Customers.Queries.GetCustomers
{
    public class GetCustomersQuery : IRequest<PaginatedList<CustomerDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? TenantId { get; set; }
    }

    public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PaginatedList<CustomerDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetCustomersQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedList<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Customers
                .Where(c => c.TenantId == request.TenantId)
                .AsNoTracking();

            var count = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    CustomerNumber = c.CustomerNumber,
                    FullName = c.CustomerType == FinTech.Core.Domain.Enums.CustomerType.Individual 
                        ? (c.FirstName + " " + c.LastName) 
                        : (c.CompanyName ?? ""),
                    CustomerType = c.CustomerType,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    Address = c.Address ?? "",
                    Status = c.Status,
                    RiskRating = c.RiskRating
                })
                .ToListAsync(cancellationToken);

            return new PaginatedList<CustomerDto>(items, count, request.PageNumber, request.PageSize);
        }
    }
}
