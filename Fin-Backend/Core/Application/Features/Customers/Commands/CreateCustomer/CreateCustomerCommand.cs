using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.DTOs.Customers;
using FinTech.Core.Domain.Entities.Customers;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Application.Features.Customers.Commands.CreateCustomer
{
    public class CreateCustomerCommand : IRequest<CustomerDto>
    {
        public CreateCustomerDto Customer { get; set; }
        public string TenantId { get; set; }
    }

    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
    {
        private readonly IApplicationDbContext _context;

        public CreateCustomerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var entity = new Customer
            {
                CustomerType = request.Customer.CustomerType,
                CustomerNumber = "CUST-" + DateTime.UtcNow.Ticks.ToString().Substring(10), // Simple mock generation
                TenantId = request.TenantId,
                Email = request.Customer.Email,
                PhoneNumber = request.Customer.PhoneNumber,
                Address = request.Customer.Address,
                Status = CustomerStatus.Active,
                RiskRating = RiskRating.Low,
                
                // Individual
                FirstName = request.Customer.FirstName,
                LastName = request.Customer.LastName,
                MiddleName = request.Customer.MiddleName,
                DateOfBirth = request.Customer.DateOfBirth,
                Gender = request.Customer.Gender,
                MaritalStatus = request.Customer.MaritalStatus,

                // Corporate
                CompanyName = request.Customer.CompanyName,
                RCNumber = request.Customer.RCNumber
            };

            _context.Customers.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return new CustomerDto
            {
                Id = entity.Id,
                CustomerNumber = entity.CustomerNumber,
                FullName = entity.GetFullName(),
                CustomerType = entity.CustomerType,
                PhoneNumber = entity.PhoneNumber,
                Email = entity.Email,
                Address = entity.Address,
                Status = entity.Status,
                RiskRating = entity.RiskRating
            };
        }
    }
}
