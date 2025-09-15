using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinTech.Core.Application.DTOs.Common;
using FinTech.Domain.Entities.Inventory;
using FinTech.Domain.Enums;
using FinTech.Infrastructure.Data;

namespace FinTech.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InventoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponse<List<InventoryItemDto>>>> GetInventoryItems()
    {
        var tenantId = GetTenantId();
        
        var items = await _context.InventoryItems
            .Where(i => i.TenantId == tenantId && i.IsActive && !i.IsDeleted)
            .Select(i => new InventoryItemDto
            {
                Id = i.Id,
                ItemCode = i.ItemCode,
                ItemName = i.ItemName,
                Category = i.Category.ToString(),
                CurrentStock = i.CurrentStock,
                ReorderLevel = i.ReorderLevel,
                UnitCost = i.UnitCost,
                SellingPrice = i.SellingPrice,
                TotalValue = i.CurrentStock * i.UnitCost,
                Status = GetStockStatus(i.CurrentStock, i.ReorderLevel),
                LastUpdated = i.UpdatedAt ?? i.CreatedAt
            })
            .ToListAsync();

        return Ok(BaseResponse<List<InventoryItemDto>>.SuccessResponse(items));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BaseResponse<InventoryItemDetailDto>>> GetInventoryItem(Guid id)
    {
        var tenantId = GetTenantId();
        
        var item = await _context.InventoryItems
            .Include(i => i.Transactions.OrderByDescending(t => t.TransactionDate).Take(10))
            .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId && !i.IsDeleted);

        if (item == null)
            return NotFound(BaseResponse<InventoryItemDetailDto>.ErrorResponse("Item not found"));

        var itemDto = new InventoryItemDetailDto
        {
            Id = item.Id,
            ItemCode = item.ItemCode,
            ItemName = item.ItemName,
            Description = item.Description,
            Category = item.Category.ToString(),
            Brand = item.Brand,
            Model = item.Model,
            SKU = item.SKU,
            Barcode = item.Barcode,
            UnitOfMeasure = item.UnitOfMeasure,
            UnitCost = item.UnitCost,
            SellingPrice = item.SellingPrice,
            CurrentStock = item.CurrentStock,
            ReorderLevel = item.ReorderLevel,
            MaximumLevel = item.MaximumLevel,
            ReservedStock = item.ReservedStock,
            AvailableStock = item.AvailableStock,
            ValuationMethod = item.ValuationMethod.ToString(),
            Status = GetStockStatus(item.CurrentStock, item.ReorderLevel),
            Supplier = item.Supplier,
            Location = item.Location,
            Notes = item.Notes,
            RecentTransactions = item.Transactions.Select(t => new InventoryTransactionDto
            {
                TransactionNumber = t.TransactionNumber,
                TransactionType = t.TransactionType.ToString(),
                TransactionDate = t.TransactionDate,
                Quantity = t.Quantity,
                UnitCost = t.UnitCost,
                TotalCost = t.TotalCost,
                Description = t.Description
            }).ToList(),
            CreatedAt = item.CreatedAt
        };

        return Ok(BaseResponse<InventoryItemDetailDto>.SuccessResponse(itemDto));
    }

    [HttpPost]
    public async Task<ActionResult<BaseResponse<InventoryItemDto>>> CreateInventoryItem([FromBody] CreateInventoryItemRequest request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        // Check if item code already exists
        var existingItem = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.ItemCode == request.ItemCode && i.TenantId == tenantId);

        if (existingItem != null)
            return BadRequest(BaseResponse<InventoryItemDto>.ErrorResponse("Item code already exists"));

        var item = new InventoryItem
        {
            ItemCode = request.ItemCode,
            ItemName = request.ItemName,
            Description = request.Description,
            ItemType = request.ItemType,
            Category = request.Category,
            Brand = request.Brand,
            Model = request.Model,
            SKU = request.SKU,
            Barcode = request.Barcode,
            UnitOfMeasure = request.UnitOfMeasure,
            UnitCost = request.UnitCost,
            SellingPrice = request.SellingPrice,
            ReorderLevel = request.ReorderLevel,
            MaximumLevel = request.MaximumLevel,
            CurrentStock = request.InitialStock,
            AvailableStock = request.InitialStock,
            ValuationMethod = request.ValuationMethod,
            TrackStock = request.TrackStock,
            Supplier = request.Supplier,
            Location = request.Location,
            Notes = request.Notes,
            InventoryGLAccountId = request.InventoryGLAccountId,
            COGSGLAccountId = request.COGSGLAccountId,
            TenantId = tenantId,
            CreatedBy = userId
        };

        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync();

        // Create initial stock transaction if there's opening stock
        if (request.InitialStock > 0)
        {
            var openingTransaction = new InventoryTransaction
            {
                TransactionNumber = await GenerateTransactionNumberAsync(),
                InventoryItemId = item.Id,
                TransactionType = InventoryTransactionType.Opening,
                TransactionDate = DateTime.UtcNow,
                Quantity = request.InitialStock,
                UnitCost = request.UnitCost,
                TotalCost = request.InitialStock * request.UnitCost,
                RunningBalance = request.InitialStock,
                Description = "Opening stock",
                Status = TransactionStatus.Posted,
                TenantId = tenantId,
                CreatedBy = userId
            };

            _context.InventoryTransactions.Add(openingTransaction);
            await _context.SaveChangesAsync();
        }

        var itemDto = new InventoryItemDto
        {
            Id = item.Id,
            ItemCode = item.ItemCode,
            ItemName = item.ItemName,
            Category = item.Category.ToString(),
            CurrentStock = item.CurrentStock,
            ReorderLevel = item.ReorderLevel,
            UnitCost = item.UnitCost,
            SellingPrice = item.SellingPrice,
            TotalValue = item.CurrentStock * item.UnitCost,
            Status = GetStockStatus(item.CurrentStock, item.ReorderLevel),
            LastUpdated = item.CreatedAt
        };

        return Ok(BaseResponse<InventoryItemDto>.SuccessResponse(itemDto, "Item created successfully"));
    }

    [HttpPost("{id}/adjustment")]
    public async Task<ActionResult<BaseResponse<object>>> CreateStockAdjustment(Guid id, [FromBody] StockAdjustmentRequest request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var item = await _context.InventoryItems.FindAsync(id);
        if (item == null || item.TenantId != tenantId)
            return NotFound(BaseResponse<object>.ErrorResponse("Item not found"));

        var adjustment = new StockAdjustment
        {
            AdjustmentNumber = await GenerateAdjustmentNumberAsync(),
            InventoryItemId = id,
            AdjustmentDate = DateTime.UtcNow,
            AdjustmentType = request.AdjustmentType,
            SystemQuantity = item.CurrentStock,
            ActualQuantity = request.ActualQuantity,
            AdjustmentQuantity = request.ActualQuantity - item.CurrentStock,
            UnitCost = item.UnitCost,
            AdjustmentValue = (request.ActualQuantity - item.CurrentStock) * item.UnitCost,
            Reason = request.Reason,
            Notes = request.Notes,
            Status = AdjustmentStatus.Pending,
            TenantId = tenantId,
            CreatedBy = userId
        };

        _context.StockAdjustments.Add(adjustment);
        await _context.SaveChangesAsync();

        return Ok(BaseResponse<object>.SuccessResponse(new { 
            AdjustmentId = adjustment.Id,
            Message = "Stock adjustment created successfully"
        }));
    }

    [HttpPost("{id}/transaction")]
    public async Task<ActionResult<BaseResponse<object>>> CreateInventoryTransaction(Guid id, [FromBody] InventoryTransactionRequest request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var item = await _context.InventoryItems.FindAsync(id);
        if (item == null || item.TenantId != tenantId)
            return NotFound(BaseResponse<object>.ErrorResponse("Item not found"));

        // Calculate new stock level
        var newStock = request.TransactionType switch
        {
            InventoryTransactionType.Purchase => item.CurrentStock + request.Quantity,
            InventoryTransactionType.Sale => item.CurrentStock - request.Quantity,
            InventoryTransactionType.Adjustment => item.CurrentStock + request.Quantity,
            InventoryTransactionType.Transfer => item.CurrentStock - request.Quantity,
            InventoryTransactionType.Return => item.CurrentStock + request.Quantity,
            InventoryTransactionType.Damage => item.CurrentStock - request.Quantity,
            InventoryTransactionType.Theft => item.CurrentStock - request.Quantity,
            _ => item.CurrentStock
        };

        if (newStock < 0)
            return BadRequest(BaseResponse<object>.ErrorResponse("Insufficient stock"));

        var transaction = new InventoryTransaction
        {
            TransactionNumber = await GenerateTransactionNumberAsync(),
            InventoryItemId = id,
            TransactionType = request.TransactionType,
            TransactionDate = DateTime.UtcNow,
            Quantity = request.Quantity,
            UnitCost = request.UnitCost ?? item.UnitCost,
            TotalCost = request.Quantity * (request.UnitCost ?? item.UnitCost),
            RunningBalance = newStock,
            Reference = request.Reference,
            Description = request.Description,
            Location = request.Location,
            RelatedDocumentId = request.RelatedDocumentId,
            RelatedDocumentType = request.RelatedDocumentType,
            Status = TransactionStatus.Posted,
            TenantId = tenantId,
            CreatedBy = userId
        };

        _context.InventoryTransactions.Add(transaction);

        // Update item stock levels
        item.CurrentStock = newStock;
        item.AvailableStock = newStock - item.ReservedStock;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        return Ok(BaseResponse<object>.SuccessResponse(new { 
            TransactionId = transaction.Id,
            NewStockLevel = newStock,
            Message = "Transaction processed successfully"
        }));
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<BaseResponse<List<InventoryItemDto>>>> GetLowStockItems()
    {
        var tenantId = GetTenantId();
        
        var items = await _context.InventoryItems
            .Where(i => i.TenantId == tenantId && 
                       i.IsActive && 
                       !i.IsDeleted && 
                       i.CurrentStock <= i.ReorderLevel)
            .Select(i => new InventoryItemDto
            {
                Id = i.Id,
                ItemCode = i.ItemCode,
                ItemName = i.ItemName,
                Category = i.Category.ToString(),
                CurrentStock = i.CurrentStock,
                ReorderLevel = i.ReorderLevel,
                UnitCost = i.UnitCost,
                SellingPrice = i.SellingPrice,
                TotalValue = i.CurrentStock * i.UnitCost,
                Status = GetStockStatus(i.CurrentStock, i.ReorderLevel),
                LastUpdated = i.UpdatedAt ?? i.CreatedAt
            })
            .ToListAsync();

        return Ok(BaseResponse<List<InventoryItemDto>>.SuccessResponse(items));
    }

    private string GetStockStatus(decimal currentStock, decimal reorderLevel)
    {
        if (currentStock <= 0) return "Out of Stock";
        if (currentStock <= reorderLevel) return "Low Stock";
        return "In Stock";
    }

    private async Task<string> GenerateTransactionNumberAsync()
    {
        var lastTransaction = await _context.InventoryTransactions
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastTransaction != null && lastTransaction.TransactionNumber.StartsWith("INV"))
        {
            if (int.TryParse(lastTransaction.TransactionNumber.Substring(3), out var lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"INV{nextNumber:D6}";
    }

    private async Task<string> GenerateAdjustmentNumberAsync()
    {
        var lastAdjustment = await _context.StockAdjustments
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastAdjustment != null && lastAdjustment.AdjustmentNumber.StartsWith("ADJ"))
        {
            if (int.TryParse(lastAdjustment.AdjustmentNumber.Substring(3), out var lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"ADJ{nextNumber:D6}";
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
}

// DTOs
public class InventoryItemDto
{
    public Guid Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal UnitCost { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal TotalValue { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}

public class InventoryItemDetailDto
{
    public Guid Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal CurrentStock { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal MaximumLevel { get; set; }
    public decimal ReservedStock { get; set; }
    public decimal AvailableStock { get; set; }
    public string ValuationMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Supplier { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public List<InventoryTransactionDto> RecentTransactions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class InventoryTransactionDto
{
    public string TransactionNumber { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? Description { get; set; }
}

public class CreateInventoryItemRequest
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ItemType ItemType { get; set; }
    public ItemCategory Category { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SKU { get; set; }
    public string? Barcode { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal MaximumLevel { get; set; }
    public decimal InitialStock { get; set; }
    public ValuationMethod ValuationMethod { get; set; }
    public bool TrackStock { get; set; } = true;
    public string? Supplier { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public Guid InventoryGLAccountId { get; set; }
    public Guid COGSGLAccountId { get; set; }
}

public class StockAdjustmentRequest
{
    public StockAdjustmentType AdjustmentType { get; set; }
    public decimal ActualQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class InventoryTransactionRequest
{
    public InventoryTransactionType TransactionType { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Reference { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public Guid? RelatedDocumentId { get; set; }
    public string? RelatedDocumentType { get; set; }
}