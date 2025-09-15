using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinTech.Core.Application.DTOs.Common;
using FinTech.Core.Application.Services;
using FinTech.Domain.Enums;
using FinTech.Infrastructure.Data;

namespace FinTech.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MakerCheckerController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMakerCheckerService _makerCheckerService;

    public MakerCheckerController(ApplicationDbContext context, IMakerCheckerService makerCheckerService)
    {
        _context = context;
        _makerCheckerService = makerCheckerService;
    }

    [HttpGet("pending")]
    public async Task<ActionResult<BaseResponse<List<MakerCheckerTransactionDto>>>> GetPendingTransactions()
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var transactions = await _makerCheckerService.GetPendingTransactionsAsync(tenantId, userId);

        var transactionDtos = transactions.Select(t => new MakerCheckerTransactionDto
        {
            Id = t.Id,
            TransactionReference = t.TransactionReference,
            EntityName = t.EntityName,
            Operation = t.Operation,
            Amount = t.Amount,
            Priority = t.Priority,
            MakerName = t.MakerName,
            MakerTimestamp = t.MakerTimestamp,
            Status = t.Status.ToString(),
            ExpiryDate = t.ExpiryDate,
            Description = GetTransactionDescription(t)
        }).ToList();

        return Ok(BaseResponse<List<MakerCheckerTransactionDto>>.SuccessResponse(transactionDtos));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BaseResponse<MakerCheckerTransactionDetailDto>>> GetTransaction(Guid id)
    {
        var tenantId = GetTenantId();

        var transaction = await _context.MakerCheckerTransactions
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);

        if (transaction == null)
            return NotFound(BaseResponse<MakerCheckerTransactionDetailDto>.ErrorResponse("Transaction not found"));

        var transactionDto = new MakerCheckerTransactionDetailDto
        {
            Id = transaction.Id,
            TransactionReference = transaction.TransactionReference,
            EntityName = transaction.EntityName,
            EntityId = transaction.EntityId,
            Operation = transaction.Operation,
            RequestData = transaction.RequestData,
            Amount = transaction.Amount,
            Priority = transaction.Priority,
            MakerName = transaction.MakerName,
            MakerTimestamp = transaction.MakerTimestamp,
            CheckerName = transaction.CheckerName,
            CheckerTimestamp = transaction.CheckerTimestamp,
            CheckerComments = transaction.CheckerComments,
            Status = transaction.Status.ToString(),
            RejectionReason = transaction.RejectionReason,
            ExpiryDate = transaction.ExpiryDate,
            Description = GetTransactionDescription(transaction)
        };

        return Ok(BaseResponse<MakerCheckerTransactionDetailDto>.SuccessResponse(transactionDto));
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult<BaseResponse<object>>> ApproveTransaction(Guid id, [FromBody] ApprovalRequest request)
    {
        var userId = GetUserId();
        var userName = GetUserName();

        var success = await _makerCheckerService.ApproveTransactionAsync(id, userId, request.Comments);

        if (success)
        {
            return Ok(BaseResponse<object>.SuccessResponse(new { 
                Message = "Transaction approved and executed successfully"
            }));
        }
        else
        {
            return BadRequest(BaseResponse<object>.ErrorResponse("Failed to approve transaction"));
        }
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<BaseResponse<object>>> RejectTransaction(Guid id, [FromBody] RejectionRequest request)
    {
        var userId = GetUserId();

        var success = await _makerCheckerService.RejectTransactionAsync(id, userId, request.RejectionReason);

        if (success)
        {
            return Ok(BaseResponse<object>.SuccessResponse(new { 
                Message = "Transaction rejected successfully"
            }));
        }
        else
        {
            return BadRequest(BaseResponse<object>.ErrorResponse("Failed to reject transaction"));
        }
    }

    [HttpGet("my-transactions")]
    public async Task<ActionResult<BaseResponse<List<MakerCheckerTransactionDto>>>> GetMyTransactions()
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var transactions = await _context.MakerCheckerTransactions
            .Where(t => t.TenantId == tenantId && t.MakerId.ToString() == userId)
            .OrderByDescending(t => t.MakerTimestamp)
            .Take(50)
            .ToListAsync();

        var transactionDtos = transactions.Select(t => new MakerCheckerTransactionDto
        {
            Id = t.Id,
            TransactionReference = t.TransactionReference,
            EntityName = t.EntityName,
            Operation = t.Operation,
            Amount = t.Amount,
            Priority = t.Priority,
            MakerName = t.MakerName,
            MakerTimestamp = t.MakerTimestamp,
            CheckerName = t.CheckerName,
            CheckerTimestamp = t.CheckerTimestamp,
            Status = t.Status.ToString(),
            ExpiryDate = t.ExpiryDate,
            Description = GetTransactionDescription(t)
        }).ToList();

        return Ok(BaseResponse<List<MakerCheckerTransactionDto>>.SuccessResponse(transactionDtos));
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<BaseResponse<MakerCheckerStatisticsDto>>> GetStatistics()
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var totalPending = await _context.MakerCheckerTransactions
            .CountAsync(t => t.TenantId == tenantId && 
                            t.Status == MakerCheckerStatus.PendingApproval &&
                            t.MakerId.ToString() != userId);

        var myPendingTransactions = await _context.MakerCheckerTransactions
            .CountAsync(t => t.TenantId == tenantId && 
                            t.MakerId.ToString() == userId &&
                            t.Status == MakerCheckerStatus.PendingApproval);

        var approvedToday = await _context.MakerCheckerTransactions
            .CountAsync(t => t.TenantId == tenantId && 
                            t.Status == MakerCheckerStatus.Approved &&
                            t.CheckerTimestamp.HasValue &&
                            t.CheckerTimestamp.Value.Date == DateTime.Today);

        var rejectedToday = await _context.MakerCheckerTransactions
            .CountAsync(t => t.TenantId == tenantId && 
                            t.Status == MakerCheckerStatus.Rejected &&
                            t.CheckerTimestamp.HasValue &&
                            t.CheckerTimestamp.Value.Date == DateTime.Today);

        var highPriorityPending = await _context.MakerCheckerTransactions
            .CountAsync(t => t.TenantId == tenantId && 
                            t.Status == MakerCheckerStatus.PendingApproval &&
                            t.Priority >= 3 &&
                            t.MakerId.ToString() != userId);

        var statistics = new MakerCheckerStatisticsDto
        {
            TotalPendingApprovals = totalPending,
            MyPendingTransactions = myPendingTransactions,
            ApprovedToday = approvedToday,
            RejectedToday = rejectedToday,
            HighPriorityPending = highPriorityPending
        };

        return Ok(BaseResponse<MakerCheckerStatisticsDto>.SuccessResponse(statistics));
    }

    private string GetTransactionDescription(Domain.Entities.Security.MakerCheckerTransaction transaction)
    {
        return transaction.EntityName.ToLower() switch
        {
            "loanaccount" => transaction.Operation.ToLower() switch
            {
                "create" => "New loan account creation",
                "disburse" => $"Loan disbursement of ₦{transaction.Amount:N2}",
                "repayment" => $"Loan repayment of ₦{transaction.Amount:N2}",
                _ => $"Loan {transaction.Operation}"
            },
            "depositaccount" => transaction.Operation.ToLower() switch
            {
                "create" => "New deposit account creation",
                "deposit" => $"Deposit transaction of ₦{transaction.Amount:N2}",
                "withdrawal" => $"Withdrawal transaction of ₦{transaction.Amount:N2}",
                _ => $"Deposit {transaction.Operation}"
            },
            "journalentry" => $"Journal entry posting of ₦{transaction.Amount:N2}",
            "vendor" => transaction.Operation.ToLower() switch
            {
                "create" => "New vendor registration",
                "payment" => $"Vendor payment of ₦{transaction.Amount:N2}",
                _ => $"Vendor {transaction.Operation}"
            },
            "employee" => transaction.Operation.ToLower() switch
            {
                "create" => "New employee registration",
                "payroll" => $"Payroll processing of ₦{transaction.Amount:N2}",
                _ => $"Employee {transaction.Operation}"
            },
            _ => $"{transaction.EntityName} {transaction.Operation}"
        };
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
public class MakerCheckerTransactionDto
{
    public Guid Id { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public int Priority { get; set; }
    public string MakerName { get; set; } = string.Empty;
    public DateTime MakerTimestamp { get; set; }
    public string? CheckerName { get; set; }
    public DateTime? CheckerTimestamp { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class MakerCheckerTransactionDetailDto
{
    public Guid Id { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Operation { get; set; } = string.Empty;
    public string RequestData { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public int Priority { get; set; }
    public string MakerName { get; set; } = string.Empty;
    public DateTime MakerTimestamp { get; set; }
    public string? CheckerName { get; set; }
    public DateTime? CheckerTimestamp { get; set; }
    public string? CheckerComments { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class MakerCheckerStatisticsDto
{
    public int TotalPendingApprovals { get; set; }
    public int MyPendingTransactions { get; set; }
    public int ApprovedToday { get; set; }
    public int RejectedToday { get; set; }
    public int HighPriorityPending { get; set; }
}

public class ApprovalRequest
{
    public string? Comments { get; set; }
}

public class RejectionRequest
{
    public string RejectionReason { get; set; } = string.Empty;
}