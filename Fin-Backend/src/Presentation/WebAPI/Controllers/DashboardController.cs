using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinTech.Domain.Enums;
using FinTech.Core.Application.DTOs.Common;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Application.Common.Models;

namespace FinTech.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public DashboardController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<BaseResponse<DashboardOverviewDto>>> GetDashboardOverview()
    {
        var tenantId = GetTenantId();

        var overview = new DashboardOverviewDto
        {
            TotalCustomers = await _context.Customers.CountAsync(c => c.TenantId == tenantId),
            TotalDepositAccounts = await _context.DepositAccounts.CountAsync(a => a.TenantId == tenantId),
            TotalDeposits = await _context.DepositAccounts
                .Where(a => a.TenantId == tenantId)
                .SumAsync(a => a.Balance),
            TotalTransactionsToday = await _context.DepositTransactions
                .Where(t => t.TenantId == tenantId && t.TransactionDate.Date == DateTime.Today)
                .CountAsync()
        };

        return Ok(BaseResponse<DashboardOverviewDto>.SuccessResponse(overview));
    }

    [HttpGet("executive")]
    public async Task<ActionResult<BaseResponse<ExecutiveDashboardDto>>> GetExecutiveDashboard()
    {
        var tenantId = GetTenantId();
        
        var dashboard = new ExecutiveDashboardDto
        {
            TotalAssets = await CalculateTotalAssets(tenantId),
            TotalLiabilities = await CalculateTotalLiabilities(tenantId),
            NetWorth = 0, // Will be calculated
            MonthlyRevenue = await CalculateMonthlyRevenue(tenantId),
            MonthlyExpenses = await CalculateMonthlyExpenses(tenantId),
            NetIncome = 0, // Will be calculated
            CustomerGrowthRate = await CalculateCustomerGrowthRate(tenantId),
            PortfolioAtRisk = await CalculatePortfolioAtRisk(tenantId),
            CapitalAdequacyRatio = await CalculateCapitalAdequacyRatio(tenantId),
            LiquidityRatio = await CalculateLiquidityRatio(tenantId),
            RevenueByMonth = await GetRevenueByMonth(tenantId),
            TopPerformingBranches = await GetTopPerformingBranches(tenantId),
            RiskMetrics = await GetRiskMetrics(tenantId)
        };
        
        dashboard.NetWorth = dashboard.TotalAssets - dashboard.TotalLiabilities;
        dashboard.NetIncome = dashboard.MonthlyRevenue - dashboard.MonthlyExpenses;
        
        return Ok(BaseResponse<ExecutiveDashboardDto>.SuccessResponse(dashboard));
    }

    [HttpGet("loans")]
    public async Task<ActionResult<BaseResponse<LoanDashboardDto>>> GetLoanDashboard()
    {
        var tenantId = GetTenantId();
        
        var dashboard = new LoanDashboardDto
        {
            TotalPortfolio = await _context.LoanAccounts
                .Where(l => l.TenantId == tenantId)
                .SumAsync(l => l.OutstandingPrincipal),
            ActiveLoans = await _context.LoanAccounts
                .CountAsync(l => l.TenantId == tenantId && l.Status == LoanStatus.Active),
            PerformingLoans = await _context.LoanAccounts
                .CountAsync(l => l.TenantId == tenantId && l.Classification == LoanClassification.Performing),
            NonPerformingLoans = await _context.LoanAccounts
                .CountAsync(l => l.TenantId == tenantId && l.Classification != LoanClassification.Performing),
            PortfolioAtRisk = await CalculatePortfolioAtRisk(tenantId),
            AverageInterestRate = await _context.LoanAccounts
                .Where(l => l.TenantId == tenantId && l.Status == LoanStatus.Active)
                .AverageAsync(l => (double?)l.InterestRate) ?? 0,
            TotalProvisions = await _context.LoanAccounts
                .Where(l => l.TenantId == tenantId)
                .SumAsync(l => l.ProvisionAmount),
            LoansByClassification = await GetLoansByClassification(tenantId),
            MonthlyDisbursements = await GetMonthlyDisbursements(tenantId),
            RepaymentTrends = await GetRepaymentTrends(tenantId)
        };
        
        return Ok(BaseResponse<LoanDashboardDto>.SuccessResponse(dashboard));
    }

    [HttpGet("deposits")]
    public async Task<ActionResult<BaseResponse<DepositDashboardDto>>> GetDepositDashboard()
    {
        var tenantId = GetTenantId();
        
        var dashboard = new DepositDashboardDto
        {
            TotalDeposits = await _context.DepositAccounts
                .Where(d => d.TenantId == tenantId)
                .SumAsync(d => d.Balance),
            ActiveAccounts = await _context.DepositAccounts
                .CountAsync(d => d.TenantId == tenantId && d.Status == AccountStatus.Active),
            NewAccountsThisMonth = await _context.DepositAccounts
                .CountAsync(d => d.TenantId == tenantId && d.DateOpened.Month == DateTime.Now.Month),
            AverageAccountBalance = await _context.DepositAccounts
                .Where(d => d.TenantId == tenantId && d.Status == AccountStatus.Active)
                .AverageAsync(d => (double?)d.Balance) ?? 0,
            TotalInterestPaid = await CalculateTotalInterestPaid(tenantId),
            DormantAccounts = await _context.DepositAccounts
                .CountAsync(d => d.TenantId == tenantId && d.Status == AccountStatus.Dormant),
            DepositsByProduct = await GetDepositsByProduct(tenantId),
            MonthlyGrowth = await GetMonthlyDepositGrowth(tenantId),
            TransactionVolume = await GetTransactionVolume(tenantId)
        };
        
        return Ok(BaseResponse<DepositDashboardDto>.SuccessResponse(dashboard));
    }

    [HttpGet("inventory")]
    public async Task<ActionResult<BaseResponse<InventoryDashboardDto>>> GetInventoryDashboard()
    {
        var tenantId = GetTenantId();
        
        var dashboard = new InventoryDashboardDto
        {
            TotalItems = await _context.InventoryItems.CountAsync(i => i.TenantId == tenantId && i.IsActive),
            TotalValue = await _context.InventoryItems
                .Where(i => i.TenantId == tenantId && i.IsActive)
                .SumAsync(i => i.CurrentStock * i.UnitCost),
            LowStockItems = await _context.InventoryItems
                .CountAsync(i => i.TenantId == tenantId && i.CurrentStock <= i.ReorderLevel),
            OutOfStockItems = await _context.InventoryItems
                .CountAsync(i => i.TenantId == tenantId && i.CurrentStock == 0),
            TopSellingItems = await GetTopSellingItems(tenantId),
            StockMovements = await GetStockMovements(tenantId),
            CategoryBreakdown = await GetInventoryCategoryBreakdown(tenantId)
        };
        
        return Ok(BaseResponse<InventoryDashboardDto>.SuccessResponse(dashboard));
    }

    [HttpGet("payroll")]
    public async Task<ActionResult<BaseResponse<PayrollDashboardDto>>> GetPayrollDashboard()
    {
        var tenantId = GetTenantId();
        
        var dashboard = new PayrollDashboardDto
        {
            TotalEmployees = await _context.Employees.CountAsync(e => e.TenantId == tenantId && e.Status == EmployeeStatus.Active),
            MonthlyPayroll = await _context.PayrollEntries
                .Where(p => p.TenantId == tenantId && p.PayrollDate.Month == DateTime.Now.Month)
                .SumAsync(p => p.NetPay),
            AverageSalary = await _context.Employees
                .Where(e => e.TenantId == tenantId && e.Status == EmployeeStatus.Active)
                .AverageAsync(e => (double?)e.GrossSalary) ?? 0,
            TotalDeductions = await _context.PayrollEntries
                .Where(p => p.TenantId == tenantId && p.PayrollDate.Month == DateTime.Now.Month)
                .SumAsync(p => p.TotalDeductions),
            PayrollByDepartment = await GetPayrollByDepartment(tenantId),
            MonthlyTrends = await GetPayrollTrends(tenantId),
            StatutoryCompliance = await GetStatutoryCompliance(tenantId)
        };
        
        return Ok(BaseResponse<PayrollDashboardDto>.SuccessResponse(dashboard));
    }

    // Helper methods for calculations
    private async Task<decimal> CalculateTotalAssets(Guid tenantId)
    {
        // Implementation for calculating total assets from GL
        return await _context.GeneralLedgerEntries
            .Where(g => g.TenantId == tenantId && g.Account.AccountType == AccountType.Assets)
            .SumAsync(g => g.EntryType == EntryType.Debit ? g.Amount : -g.Amount);
    }

    private async Task<decimal> CalculateTotalLiabilities(Guid tenantId)
    {
        return await _context.GeneralLedgerEntries
            .Where(g => g.TenantId == tenantId && g.Account.AccountType == AccountType.Liabilities)
            .SumAsync(g => g.EntryType == EntryType.Credit ? g.Amount : -g.Amount);
    }

    private async Task<decimal> CalculateMonthlyRevenue(Guid tenantId)
    {
        return await _context.GeneralLedgerEntries
            .Where(g => g.TenantId == tenantId && 
                       g.Account.AccountType == AccountType.Revenue &&
                       g.TransactionDate.Month == DateTime.Now.Month)
            .SumAsync(g => g.EntryType == EntryType.Credit ? g.Amount : -g.Amount);
    }

    private async Task<decimal> CalculateMonthlyExpenses(Guid tenantId)
    {
        return await _context.GeneralLedgerEntries
            .Where(g => g.TenantId == tenantId && 
                       g.Account.AccountType == AccountType.Expenses &&
                       g.TransactionDate.Month == DateTime.Now.Month)
            .SumAsync(g => g.EntryType == EntryType.Debit ? g.Amount : -g.Amount);
    }

    private async Task<decimal> CalculateCustomerGrowthRate(Guid tenantId)
    {
        var thisMonth = await _context.Customers
            .CountAsync(c => c.TenantId == tenantId && c.CreatedAt.Month == DateTime.Now.Month);
        var lastMonth = await _context.Customers
            .CountAsync(c => c.TenantId == tenantId && c.CreatedAt.Month == DateTime.Now.AddMonths(-1).Month);
        
        return lastMonth == 0 ? 0 : ((decimal)(thisMonth - lastMonth) / lastMonth) * 100;
    }

    private async Task<decimal> CalculatePortfolioAtRisk(Guid tenantId)
    {
        var totalPortfolio = await _context.LoanAccounts
            .Where(l => l.TenantId == tenantId)
            .SumAsync(l => l.OutstandingPrincipal);
        
        var nonPerformingPortfolio = await _context.LoanAccounts
            .Where(l => l.TenantId == tenantId && l.Classification != LoanClassification.Performing)
            .SumAsync(l => l.OutstandingPrincipal);
        
        return totalPortfolio == 0 ? 0 : (nonPerformingPortfolio / totalPortfolio) * 100;
    }

    private async Task<decimal> CalculateCapitalAdequacyRatio(Guid tenantId)
    {
        // Simplified CAR calculation - should be more complex in production
        var totalCapital = await CalculateTotalAssets(tenantId) - await CalculateTotalLiabilities(tenantId);
        var riskWeightedAssets = await _context.LoanAccounts
            .Where(l => l.TenantId == tenantId)
            .SumAsync(l => l.OutstandingPrincipal * 1.0m); // Simplified risk weight
        
        return riskWeightedAssets == 0 ? 0 : (totalCapital / riskWeightedAssets) * 100;
    }

    private async Task<decimal> CalculateLiquidityRatio(Guid tenantId)
    {
        // Simplified liquidity ratio calculation
        var liquidAssets = await _context.DepositAccounts
            .Where(d => d.TenantId == tenantId)
            .SumAsync(d => d.Balance);
        var totalDeposits = liquidAssets; // Simplified
        
        return totalDeposits == 0 ? 0 : (liquidAssets / totalDeposits) * 100;
    }

    // Additional helper methods would be implemented here...
    private async Task<List<MonthlyDataDto>> GetRevenueByMonth(Guid tenantId)
    {
        return new List<MonthlyDataDto>(); // Placeholder
    }

    private async Task<List<BranchPerformanceDto>> GetTopPerformingBranches(Guid tenantId)
    {
        return new List<BranchPerformanceDto>(); // Placeholder
    }

    private async Task<RiskMetricsDto> GetRiskMetrics(Guid tenantId)
    {
        return new RiskMetricsDto(); // Placeholder
    }

    private async Task<List<ClassificationDataDto>> GetLoansByClassification(Guid tenantId)
    {
        return new List<ClassificationDataDto>(); // Placeholder
    }

    private async Task<List<MonthlyDataDto>> GetMonthlyDisbursements(Guid tenantId)
    {
        return new List<MonthlyDataDto>(); // Placeholder
    }

    private async Task<List<MonthlyDataDto>> GetRepaymentTrends(Guid tenantId)
    {
        return new List<MonthlyDataDto>(); // Placeholder
    }

    private async Task<decimal> CalculateTotalInterestPaid(Guid tenantId)
    {
        return 0; // Placeholder
    }

    private async Task<List<ProductDataDto>> GetDepositsByProduct(Guid tenantId)
    {
        return new List<ProductDataDto>(); // Placeholder
    }

    private async Task<List<MonthlyDataDto>> GetMonthlyDepositGrowth(Guid tenantId)
    {
        return new List<MonthlyDataDto>(); // Placeholder
    }

    private async Task<List<MonthlyDataDto>> GetTransactionVolume(Guid tenantId)
    {
        return new List<MonthlyDataDto>(); // Placeholder
    }

    private async Task<List<ItemDataDto>> GetTopSellingItems(Guid tenantId)
    {
        return new List<ItemDataDto>(); // Placeholder
    }

    private async Task<List<MonthlyDataDto>> GetStockMovements(Guid tenantId)
    {
        return new List<MonthlyDataDto>(); // Placeholder
    }

    private async Task<List<CategoryDataDto>> GetInventoryCategoryBreakdown(Guid tenantId)
    {
        return new List<CategoryDataDto>(); // Placeholder
    }

    private async Task<List<DepartmentDataDto>> GetPayrollByDepartment(Guid tenantId)
    {
        return new List<DepartmentDataDto>(); // Placeholder
    }

    private async Task<List<MonthlyDataDto>> GetPayrollTrends(Guid tenantId)
    {
        return new List<MonthlyDataDto>(); // Placeholder
    }

    private async Task<ComplianceDataDto> GetStatutoryCompliance(Guid tenantId)
    {
        return new ComplianceDataDto(); // Placeholder
    }

    private Guid GetTenantId()
    {
        var tenantIdClaim = User.Claims.FirstOrDefault(c => c.Type == "TenantId")?.Value;
        return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : Guid.Empty;
    }
}

public class DashboardOverviewDto
{
    public int TotalCustomers { get; set; }
    public int TotalDepositAccounts { get; set; }
    public decimal TotalDeposits { get; set; }
    public int TotalTransactionsToday { get; set; }
}

public class ExecutiveDashboardDto
{
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal NetWorth { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal MonthlyExpenses { get; set; }
    public decimal NetIncome { get; set; }
    public decimal CustomerGrowthRate { get; set; }
    public decimal PortfolioAtRisk { get; set; }
    public decimal CapitalAdequacyRatio { get; set; }
    public decimal LiquidityRatio { get; set; }
    public List<MonthlyDataDto> RevenueByMonth { get; set; } = [];
    public List<BranchPerformanceDto> TopPerformingBranches { get; set; } = [];
    public RiskMetricsDto RiskMetrics { get; set; } = new();
}

public class LoanDashboardDto
{
    public decimal TotalPortfolio { get; set; }
    public int ActiveLoans { get; set; }
    public int PerformingLoans { get; set; }
    public int NonPerformingLoans { get; set; }
    public decimal PortfolioAtRisk { get; set; }
    public double AverageInterestRate { get; set; }
    public decimal TotalProvisions { get; set; }
    public List<ClassificationDataDto> LoansByClassification { get; set; } = [];
    public List<MonthlyDataDto> MonthlyDisbursements { get; set; } = [];
    public List<MonthlyDataDto> RepaymentTrends { get; set; } = [];
}

public class DepositDashboardDto
{
    public decimal TotalDeposits { get; set; }
    public int ActiveAccounts { get; set; }
    public int NewAccountsThisMonth { get; set; }
    public double AverageAccountBalance { get; set; }
    public decimal TotalInterestPaid { get; set; }
    public int DormantAccounts { get; set; }
    public List<ProductDataDto> DepositsByProduct { get; set; } = [];
    public List<MonthlyDataDto> MonthlyGrowth { get; set; } = [];
    public List<MonthlyDataDto> TransactionVolume { get; set; } = [];
}

public class InventoryDashboardDto
{
    public int TotalItems { get; set; }
    public decimal TotalValue { get; set; }
    public int LowStockItems { get; set; }
    public int OutOfStockItems { get; set; }
    public List<ItemDataDto> TopSellingItems { get; set; } = [];
    public List<MonthlyDataDto> StockMovements { get; set; } = [];
    public List<CategoryDataDto> CategoryBreakdown { get; set; } = [];
}

public class PayrollDashboardDto
{
    public int TotalEmployees { get; set; }
    public decimal MonthlyPayroll { get; set; }
    public double AverageSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public List<DepartmentDataDto> PayrollByDepartment { get; set; } = [];
    public List<MonthlyDataDto> MonthlyTrends { get; set; } = [];
    public ComplianceDataDto StatutoryCompliance { get; set; } = new();
}

// Supporting DTOs
public class MonthlyDataDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class BranchPerformanceDto
{
    public string BranchName { get; set; } = string.Empty;
    public decimal Performance { get; set; }
}

public class RiskMetricsDto
{
    public decimal CreditRisk { get; set; }
    public decimal OperationalRisk { get; set; }
    public decimal MarketRisk { get; set; }
}

public class ClassificationDataDto
{
    public string Classification { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

public class ProductDataDto
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

public class ItemDataDto
{
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Value { get; set; }
}

public class CategoryDataDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Value { get; set; }
}

public class DepartmentDataDto
{
    public string Department { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal TotalPayroll { get; set; }
}

public class ComplianceDataDto
{
    public decimal PAYECompliance { get; set; }
    public decimal PensionCompliance { get; set; }
    public decimal NHFCompliance { get; set; }
}