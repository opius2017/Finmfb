using Microsoft.EntityFrameworkCore;
using FinTech.Core.Domain.Entities.Security;
using FinTech.Core.Domain.Enums;
using FinTech.Core.Application.Common.Interfaces;
using System.Text.Json;

namespace FinTech.Core.Application.Services;

public interface IMakerCheckerService
{
    Task<MakerCheckerTransaction> CreateTransactionAsync(CreateMakerCheckerRequest request);
    Task<bool> ApproveTransactionAsync(Guid transactionId, string checkerId, string? comments = null);
    Task<bool> RejectTransactionAsync(Guid transactionId, string checkerId, string rejectionReason);
    Task<List<MakerCheckerTransaction>> GetPendingTransactionsAsync(Guid tenantId, string? checkerId = null);
    Task<bool> ExecuteApprovedTransactionAsync(Guid transactionId);
}

public class MakerCheckerService : IMakerCheckerService
{
    private readonly IApplicationDbContext _context;
    private readonly IServiceProvider _serviceProvider;

    public MakerCheckerService(IApplicationDbContext context, IServiceProvider serviceProvider)
    {
        _context = context;
        _serviceProvider = serviceProvider;
    }

    public async Task<MakerCheckerTransaction> CreateTransactionAsync(CreateMakerCheckerRequest request)
    {
        var transaction = new MakerCheckerTransaction
        {
            TransactionReference = await GenerateTransactionReferenceAsync(),
            EntityName = request.EntityName,
            EntityId = request.EntityId,
            Operation = request.Operation,
            RequestData = JsonSerializer.Serialize(request.RequestData),
            MakerId = Guid.Parse(request.MakerId),
            MakerName = request.MakerName,
            MakerTimestamp = DateTime.UtcNow,
            Status = MakerCheckerStatus.PendingApproval,
            Amount = request.Amount,
            Priority = request.Priority,
            ExpiryDate = request.ExpiryDate,
            TenantId = request.TenantId
        };

        _context.MakerCheckerTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Send notification to potential checkers
        await NotifyCheckersAsync(transaction);

        return transaction;
    }

    public async Task<bool> ApproveTransactionAsync(Guid transactionId, string checkerId, string? comments = null)
    {
        var transaction = await _context.MakerCheckerTransactions.FindAsync(transactionId);
        if (transaction == null || transaction.Status != MakerCheckerStatus.PendingApproval)
            return false;

        // Validate that checker is not the same as maker
        if (transaction.MakerId.ToString() == checkerId)
            throw new InvalidOperationException("Checker cannot be the same as maker");

        transaction.CheckerId = Guid.Parse(checkerId);
        transaction.CheckerName = await GetUserNameAsync(checkerId);
        transaction.CheckerTimestamp = DateTime.UtcNow;
        transaction.CheckerComments = comments;
        transaction.Status = MakerCheckerStatus.Approved;

        await _context.SaveChangesAsync();

        // Execute the approved transaction
        await ExecuteApprovedTransactionAsync(transactionId);

        return true;
    }

    public async Task<bool> RejectTransactionAsync(Guid transactionId, string checkerId, string rejectionReason)
    {
        var transaction = await _context.MakerCheckerTransactions.FindAsync(transactionId);
        if (transaction == null || transaction.Status != MakerCheckerStatus.PendingApproval)
            return false;

        transaction.CheckerId = Guid.Parse(checkerId);
        transaction.CheckerName = await GetUserNameAsync(checkerId);
        transaction.CheckerTimestamp = DateTime.UtcNow;
        transaction.RejectionReason = rejectionReason;
        transaction.Status = MakerCheckerStatus.Rejected;

        await _context.SaveChangesAsync();

        // Notify maker of rejection
        await NotifyMakerOfRejectionAsync(transaction);

        return true;
    }

    public async Task<List<MakerCheckerTransaction>> GetPendingTransactionsAsync(Guid tenantId, string? checkerId = null)
    {
        var query = _context.MakerCheckerTransactions
            .Where(t => t.TenantId == tenantId && t.Status == MakerCheckerStatus.PendingApproval);

        // Exclude transactions made by the same user (can't approve own transactions)
        if (!string.IsNullOrEmpty(checkerId))
        {
            var checkerGuid = Guid.Parse(checkerId);
            query = query.Where(t => t.MakerId != checkerGuid);
        }

        return await query
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.MakerTimestamp)
            .ToListAsync();
    }

    public async Task<bool> ExecuteApprovedTransactionAsync(Guid transactionId)
    {
        var transaction = await _context.MakerCheckerTransactions.FindAsync(transactionId);
        if (transaction == null || transaction.Status != MakerCheckerStatus.Approved)
            return false;

        try
        {
            // Execute the transaction based on entity type and operation
            var success = await ExecuteTransactionByTypeAsync(transaction);
            
            if (success)
            {
                // Log successful execution
                await LogTransactionExecutionAsync(transaction, true);
            }

            return success;
        }
        catch (Exception ex)
        {
            // Log failed execution
            await LogTransactionExecutionAsync(transaction, false, ex.Message);
            return false;
        }
    }

    private async Task<bool> ExecuteTransactionByTypeAsync(MakerCheckerTransaction transaction)
    {
        return transaction.EntityName.ToLower() switch
        {
            "loanaccount" => await ExecuteLoanTransactionAsync(transaction),
            "depositaccount" => await ExecuteDepositTransactionAsync(transaction),
            "journalentry" => await ExecuteJournalEntryAsync(transaction),
            "vendor" => await ExecuteVendorTransactionAsync(transaction),
            "employee" => await ExecuteEmployeeTransactionAsync(transaction),
            _ => throw new NotSupportedException($"Entity type {transaction.EntityName} not supported")
        };
    }

    private async Task<bool> ExecuteLoanTransactionAsync(MakerCheckerTransaction transaction)
    {
        var loanService = _serviceProvider.GetRequiredService<ILoanService>();
        var requestData = JsonSerializer.Deserialize<Dictionary<string, object>>(transaction.RequestData);

        return transaction.Operation.ToLower() switch
        {
            "disburse" => await loanService.DisburseLoanAsync(
                transaction.EntityId, 
                Convert.ToDecimal(requestData["amount"]), 
                transaction.CheckerName ?? "System"),
            "repayment" => await loanService.ProcessRepaymentAsync(
                transaction.EntityId, 
                Convert.ToDecimal(requestData["amount"]), 
                transaction.CheckerName ?? "System"),
            _ => false
        };
    }

    private async Task<bool> ExecuteDepositTransactionAsync(MakerCheckerTransaction transaction)
    {
        // Implementation for deposit transactions
        return true;
    }

    private async Task<bool> ExecuteJournalEntryAsync(MakerCheckerTransaction transaction)
    {
        var glService = _serviceProvider.GetRequiredService<IGeneralLedgerService>();
        var requestData = JsonSerializer.Deserialize<CreateJournalEntryRequest>(transaction.RequestData);
        
        if (requestData != null)
        {
            return await glService.PostJournalEntryAsync(requestData);
        }

        return false;
    }

    private async Task<bool> ExecuteVendorTransactionAsync(MakerCheckerTransaction transaction)
    {
        // Implementation for vendor transactions
        return true;
    }

    private async Task<bool> ExecuteEmployeeTransactionAsync(MakerCheckerTransaction transaction)
    {
        // Implementation for employee transactions
        return true;
    }

    private async Task<string> GenerateTransactionReferenceAsync()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"MC{timestamp}{random}";
    }

    private async Task<string> GetUserNameAsync(string userId)
    {
        var user = await _context.Users.FindAsync(Guid.Parse(userId));
        return user?.UserName ?? "Unknown User";
    }

    private async Task NotifyCheckersAsync(MakerCheckerTransaction transaction)
    {
        // Implementation for notifying potential checkers
        // This would integrate with email/SMS service
    }

    private async Task NotifyMakerOfRejectionAsync(MakerCheckerTransaction transaction)
    {
        // Implementation for notifying maker of rejection
        // This would integrate with email/SMS service
    }

    private async Task LogTransactionExecutionAsync(MakerCheckerTransaction transaction, bool success, string? errorMessage = null)
    {
        // Implementation for logging transaction execution
        // This would create audit log entries
    }
}

public class CreateMakerCheckerRequest
{
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Operation { get; set; } = string.Empty;
    public object RequestData { get; set; } = new();
    public string MakerId { get; set; } = string.Empty;
    public string MakerName { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public int Priority { get; set; } = 1;
    public DateTime? ExpiryDate { get; set; }
    public Guid TenantId { get; set; }
}
