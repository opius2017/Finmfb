using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinTech.Core.Application.DTOs.Common;
using FinTech.Domain.Entities.Customers;
using FinTech.Domain.Enums;
using FinTech.Infrastructure.Data;

namespace FinTech.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CustomersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponse<List<CustomerDto>>>> GetCustomers()
    {
        var tenantId = GetTenantId();
        
        var customers = await _context.Customers
            .Where(c => c.TenantId == tenantId && !c.IsDeleted)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                CustomerNumber = c.CustomerNumber,
                Name = c.CustomerType == CustomerType.Individual 
                    ? $"{c.FirstName} {c.LastName}" 
                    : c.CompanyName!,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                CustomerType = c.CustomerType.ToString(),
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(BaseResponse<List<CustomerDto>>.SuccessResponse(customers));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BaseResponse<CustomerDetailDto>>> GetCustomer(Guid id)
    {
        var tenantId = GetTenantId();
        
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId && !c.IsDeleted);

        if (customer == null)
            return NotFound(BaseResponse<CustomerDetailDto>.ErrorResponse("Customer not found"));

        var customerDto = new CustomerDetailDto
        {
            Id = customer.Id,
            CustomerNumber = customer.CustomerNumber,
            CustomerType = customer.CustomerType,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            MiddleName = customer.MiddleName,
            CompanyName = customer.CompanyName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            Address = customer.Address,
            City = customer.City,
            State = customer.State,
            Status = customer.Status,
            BVN = customer.BVN,
            NIN = customer.NIN,
            CreatedAt = customer.CreatedAt
        };

        return Ok(BaseResponse<CustomerDetailDto>.SuccessResponse(customerDto));
    }

    private Guid GetTenantId()
    {
        var tenantIdClaim = User.Claims.FirstOrDefault(c => c.Type == "TenantId")?.Value;
        return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : Guid.Empty;
    }
}

public class CustomerDto
{
    public Guid Id { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string CustomerType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CustomerDetailDto
{
    public Guid Id { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? CompanyName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public CustomerStatus Status { get; set; }
    public string? BVN { get; set; }
    public string? NIN { get; set; }
    public DateTime CreatedAt { get; set; }
}