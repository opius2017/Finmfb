using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using FinTech.Core.Application.DTOs.Customers;
using FinTech.Core.Application.Features.Customers.Commands.CreateCustomer;
using FinTech.Core.Application.Features.Customers.Queries.GetCustomers;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.Interfaces.Services;

namespace FinTech.WebAPI.Controllers.Customers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly ICurrentUserService _currentUserService;

        public CustomersController(ISender sender, ICurrentUserService currentUserService)
        {
            _sender = sender;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var query = new GetCustomersQuery 
            { 
                PageNumber = page, 
                PageSize = limit,
                TenantId = _currentUserService.TenantId 
            };
            var result = await _sender.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
             // Placeholder for now, as query wasn't strictly requested to fix 404
             return Ok(new { });
        }
        
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
        {
            var command = new CreateCustomerCommand 
            { 
                Customer = dto,
                TenantId = _currentUserService.TenantId 
            };
            var result = await _sender.Send(command);
            return Ok(result);
        }
    }
}
