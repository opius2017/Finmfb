using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinTech.Core.Application.DTOs.Common;
using FinTech.Core.Application.Services;
using FinTech.Domain.Entities.Loans;
using FinTech.Domain.Enums;
using FinTech.Infrastructure.Data;
using FinTech.Application.Common.Models;

namespace FinTech.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoansController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILoanService _loanService;
    private readonly IMakerCheckerService _makerCheckerService;

    public LoansController(ApplicationDbContext context, ILoanService loanService, IMakerCheckerService makerCheckerService)
    {
        _context = context;
        _loanService = loanService;
        _makerCheckerService = makerCheckerService;
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponse<List<LoanAccountDto>>>> GetLoans()
    {
        var tenantId = GetTenantId();
        
        var loans = await _context.LoanAccounts
            .Include(l => l.Customer)
            .Include(l => l.Product)
            .Where(l => l.TenantId == tenantId && !l.IsDeleted)
            .Select(l => new LoanAccountDto
            {
                Id = l.Id,
                AccountNumber = l.AccountNumber,
                CustomerName = l.Customer.CustomerType == CustomerType.Individual 
                    ? $"{l.Customer.FirstName} {l.Customer.LastName}" 
                    : l.Customer.CompanyName!,
                ProductName = l.Product.ProductName,
                PrincipalAmount = l.PrincipalAmount,
                OutstandingPrincipal = l.OutstandingPrincipal,
                OutstandingInterest = l.OutstandingInterest,
                InterestRate = l.InterestRate,
                DisbursementDate = l.DisbursementDate,
                MaturityDate = l.MaturityDate,
                Status = l.Status.ToString(),
                Classification = l.Classification.ToString(),
                DaysPastDue = l.DaysPastDue,
                ProvisionAmount = l.ProvisionAmount,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return Ok(BaseResponse<List<LoanAccountDto>>.SuccessResponse(loans));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BaseResponse<LoanAccountDetailDto>>> GetLoan(Guid id)
    {
        var tenantId = GetTenantId();
        
        var loan = await _context.LoanAccounts
            .Include(l => l.Customer)
            .Include(l => l.Product)
            .Include(l => l.RepaymentSchedule)
            .Include(l => l.Collaterals)
            .Include(l => l.Guarantors)
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == tenantId && !l.IsDeleted);

        if (loan == null)
            return NotFound(BaseResponse<LoanAccountDetailDto>.ErrorResponse("Loan not found"));

        var loanDto = new LoanAccountDetailDto
        {
            Id = loan.Id,
            AccountNumber = loan.AccountNumber,
            Customer = new CustomerSummaryDto
            {
                Id = loan.Customer.Id,
                Name = loan.Customer.CustomerType == CustomerType.Individual 
                    ? $"{loan.Customer.FirstName} {loan.Customer.LastName}" 
                    : loan.Customer.CompanyName!,
                Email = loan.Customer.Email,
                PhoneNumber = loan.Customer.PhoneNumber
            },
            Product = new LoanProductSummaryDto
            {
                Id = loan.Product.Id,
                ProductName = loan.Product.ProductName,
                ProductType = loan.Product.ProductType.ToString(),
                InterestRate = loan.Product.InterestRate
            },
            PrincipalAmount = loan.PrincipalAmount,
            OutstandingPrincipal = loan.OutstandingPrincipal,
            OutstandingInterest = loan.OutstandingInterest,
            InterestRate = loan.InterestRate,
            TenorDays = loan.TenorDays,
            DisbursementDate = loan.DisbursementDate,
            MaturityDate = loan.MaturityDate,
            Status = loan.Status.ToString(),
            Classification = loan.Classification.ToString(),
            DaysPastDue = loan.DaysPastDue,
            ProvisionAmount = loan.ProvisionAmount,
            Purpose = loan.Purpose,
            RepaymentSchedule = loan.RepaymentSchedule.Select(s => new RepaymentScheduleDto
            {
                InstallmentNumber = s.InstallmentNumber,
                DueDate = s.DueDate,
                PrincipalAmount = s.PrincipalAmount,
                InterestAmount = s.InterestAmount,
                TotalAmount = s.TotalAmount,
                PaidTotal = s.PaidTotal,
                OutstandingTotal = s.OutstandingTotal,
                Status = s.Status.ToString(),
                DaysOverdue = s.DaysOverdue
            }).ToList(),
            Collaterals = loan.Collaterals.Select(c => new CollateralDto
            {
                CollateralType = c.CollateralType,
                Description = c.Description,
                EstimatedValue = c.EstimatedValue,
                Status = c.Status.ToString()
            }).ToList(),
            Guarantors = loan.Guarantors.Select(g => new GuarantorDto
            {
                FirstName = g.FirstName,
                LastName = g.LastName,
                PhoneNumber = g.PhoneNumber,
                Relationship = g.Relationship,
                Status = g.Status.ToString()
            }).ToList(),
            CreatedAt = loan.CreatedAt
        };

        return Ok(BaseResponse<LoanAccountDetailDto>.SuccessResponse(loanDto));
    }

    [HttpPost]
    public async Task<ActionResult<BaseResponse<LoanAccountDto>>> CreateLoan([FromBody] CreateLoanRequest request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var userName = GetUserName();

        var createRequest = new CreateLoanAccountRequest
        {
            CustomerId = request.CustomerId,
            ProductId = request.ProductId,
            PrincipalAmount = request.PrincipalAmount,
            InterestRate = request.InterestRate,
            TenorDays = request.TenorDays,
            DisbursementDate = request.DisbursementDate,
            Purpose = request.Purpose,
            TenantId = tenantId
        };

        // Create maker-checker transaction for loan creation
        var makerCheckerRequest = new CreateMakerCheckerRequest
        {
            EntityName = "LoanAccount",
            EntityId = Guid.NewGuid(),
            Operation = "Create",
            RequestData = createRequest,
            MakerId = userId,
            MakerName = userName,
            Amount = request.PrincipalAmount,
            Priority = 2, // Medium priority
            TenantId = tenantId
        };

        var mcTransaction = await _makerCheckerService.CreateTransactionAsync(makerCheckerRequest);

        return Ok(BaseResponse<object>.SuccessResponse(new { 
            TransactionId = mcTransaction.Id,
            Message = "Loan creation request submitted for approval"
        }));
    }

    [HttpPost("{id}/disburse")]
    public async Task<ActionResult<BaseResponse<object>>> DisburseLoan(Guid id, [FromBody] DisburseRequest request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var userName = GetUserName();

        // Create maker-checker transaction for disbursement
        var makerCheckerRequest = new CreateMakerCheckerRequest
        {
            EntityName = "LoanAccount",
            EntityId = id,
            Operation = "Disburse",
            RequestData = new { amount = request.Amount },
            MakerId = userId,
            MakerName = userName,
            Amount = request.Amount,
            Priority = 3, // High priority
            TenantId = tenantId
        };

        var mcTransaction = await _makerCheckerService.CreateTransactionAsync(makerCheckerRequest);

        return Ok(BaseResponse<object>.SuccessResponse(new { 
            TransactionId = mcTransaction.Id,
            Message = "Loan disbursement request submitted for approval"
        }));
    }

    [HttpPost("{id}/repayment")]
    public async Task<ActionResult<BaseResponse<object>>> ProcessRepayment(Guid id, [FromBody] RepaymentRequest request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var userName = GetUserName();

        // For repayments, we might allow direct processing for smaller amounts
        // or require approval for larger amounts
        if (request.Amount <= 100000) // Direct processing for amounts <= 100k
        {
            var success = await _loanService.ProcessRepaymentAsync(id, request.Amount, userName);
            
            if (success)
            {
                return Ok(BaseResponse<object>.SuccessResponse(new { 
                    Message = "Repayment processed successfully"
                }));
            }
            else
            {
                return BadRequest(BaseResponse<object>.ErrorResponse("Failed to process repayment"));
            }
        }
        else
        {
            // Require approval for larger amounts
            var makerCheckerRequest = new CreateMakerCheckerRequest
            {
                EntityName = "LoanAccount",
                EntityId = id,
                Operation = "Repayment",
                RequestData = new { amount = request.Amount },
                MakerId = userId,
                MakerName = userName,
                Amount = request.Amount,
                Priority = 2,
                TenantId = tenantId
            };

            var mcTransaction = await _makerCheckerService.CreateTransactionAsync(makerCheckerRequest);

            return Ok(BaseResponse<object>.SuccessResponse(new { 
                TransactionId = mcTransaction.Id,
                Message = "Large repayment request submitted for approval"
            }));
        }
    }

    [HttpGet("{id}/schedule")]
    public async Task<ActionResult<BaseResponse<List<RepaymentScheduleDto>>>> GetRepaymentSchedule(Guid id)
    {
        var tenantId = GetTenantId();
        
        var schedule = await _context.LoanRepaymentSchedules
            .Where(s => s.LoanAccountId == id && s.TenantId == tenantId)
            .OrderBy(s => s.InstallmentNumber)
            .Select(s => new RepaymentScheduleDto
            {
                InstallmentNumber = s.InstallmentNumber,
                DueDate = s.DueDate,
                PrincipalAmount = s.PrincipalAmount,
                InterestAmount = s.InterestAmount,
                TotalAmount = s.TotalAmount,
                PaidTotal = s.PaidTotal,
                OutstandingTotal = s.OutstandingTotal,
                Status = s.Status.ToString(),
                DaysOverdue = s.DaysOverdue
            })
            .ToListAsync();

        return Ok(BaseResponse<List<RepaymentScheduleDto>>.SuccessResponse(schedule));
    }

    [HttpPost("classify")]
    public async Task<ActionResult<BaseResponse<object>>> ClassifyLoans()
    {
        var tenantId = GetTenantId();
        
        var success = await _loanService.ClassifyLoansAsync(tenantId);
        
        if (success)
        {
            return Ok(BaseResponse<object>.SuccessResponse(new { 
                Message = "Loan classification completed successfully"
            }));
        }
        else
        {
            return BadRequest(BaseResponse<object>.ErrorResponse("Failed to classify loans"));
        }
    }

    private Guid GetTenantId()
    {
        var tenantIdClaim = User.Claims.FirstOrDefault(c => c.Type == "TenantId")?.Value;
        return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : Guid.Empty;
    }

    private string GetUserId()
    {
        return User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "userId")?.Value ?? "";
    }

    private string GetUserName()
    {
        return User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "Unknown User";
    }
}

// DTOs
public class LoanAccountDto
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal PrincipalAmount { get; set; }
    public decimal OutstandingPrincipal { get; set; }
    public decimal OutstandingInterest { get; set; }
    public decimal InterestRate { get; set; }
    public DateTime DisbursementDate { get; set; }
    public DateTime MaturityDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Classification { get; set; } = string.Empty;
    public int DaysPastDue { get; set; }
    public decimal ProvisionAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LoanAccountDetailDto
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public CustomerSummaryDto Customer { get; set; } = new();
    public LoanProductSummaryDto Product { get; set; } = new();
    public decimal PrincipalAmount { get; set; }
    public decimal OutstandingPrincipal { get; set; }
    public decimal OutstandingInterest { get; set; }
    public decimal InterestRate { get; set; }
    public int TenorDays { get; set; }
    public DateTime DisbursementDate { get; set; }
    public DateTime MaturityDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Classification { get; set; } = string.Empty;
    public int DaysPastDue { get; set; }
    public decimal ProvisionAmount { get; set; }
    public string? Purpose { get; set; }
    public List<RepaymentScheduleDto> RepaymentSchedule { get; set; } = new();
    public List<CollateralDto> Collaterals { get; set; } = new();
    public List<GuarantorDto> Guarantors { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class CustomerSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class LoanProductSummaryDto
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
    public decimal InterestRate { get; set; }
}

public class RepaymentScheduleDto
{
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidTotal { get; set; }
    public decimal OutstandingTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DaysOverdue { get; set; }
}

public class CollateralDto
{
    public string CollateralType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal EstimatedValue { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class GuarantorDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Relationship { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateLoanRequest
{
    public Guid CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal? InterestRate { get; set; }
    public int TenorDays { get; set; }
    public DateTime DisbursementDate { get; set; }
    public string? Purpose { get; set; }
}

public class DisburseRequest
{
    public decimal Amount { get; set; }
}

public class RepaymentRequest
{
    public decimal Amount { get; set; }
}