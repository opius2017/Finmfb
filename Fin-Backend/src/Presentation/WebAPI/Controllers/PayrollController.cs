using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinTech.Core.Application.DTOs.Common;
using FinTech.Core.Application.Services;
using FinTech.Domain.Entities.Payroll;
using FinTech.Domain.Enums;
using FinTech.Infrastructure.Data;

namespace FinTech.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PayrollController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITaxCalculationService _taxService;

    public PayrollController(ApplicationDbContext context, ITaxCalculationService taxService)
    {
        _context = context;
        _taxService = taxService;
    }

    [HttpGet("employees")]
    public async Task<ActionResult<BaseResponse<List<EmployeeDto>>>> GetEmployees()
    {
        var tenantId = GetTenantId();
        
        var employees = await _context.Employees
            .Where(e => e.TenantId == tenantId && !e.IsDeleted)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                EmployeeNumber = e.EmployeeNumber,
                FullName = e.FullName,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                Department = e.Department,
                Position = e.Position,
                BasicSalary = e.BasicSalary,
                GrossSalary = e.GrossSalary,
                Status = e.Status.ToString(),
                HireDate = e.HireDate,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        return Ok(BaseResponse<List<EmployeeDto>>.SuccessResponse(employees));
    }

    [HttpGet("employees/{id}")]
    public async Task<ActionResult<BaseResponse<EmployeeDetailDto>>> GetEmployee(Guid id)
    {
        var tenantId = GetTenantId();
        
        var employee = await _context.Employees
            .Include(e => e.PayrollEntries.OrderByDescending(p => p.PayrollDate).Take(6))
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId && !e.IsDeleted);

        if (employee == null)
            return NotFound(BaseResponse<EmployeeDetailDto>.ErrorResponse("Employee not found"));

        var employeeDto = new EmployeeDetailDto
        {
            Id = employee.Id,
            EmployeeNumber = employee.EmployeeNumber,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            MiddleName = employee.MiddleName,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            DateOfBirth = employee.DateOfBirth,
            Gender = employee.Gender?.ToString(),
            MaritalStatus = employee.MaritalStatus,
            Address = employee.Address,
            City = employee.City,
            State = employee.State,
            BVN = employee.BVN,
            NIN = employee.NIN,
            TaxID = employee.TaxID,
            PensionPIN = employee.PensionPIN,
            Department = employee.Department,
            Position = employee.Position,
            Grade = employee.Grade,
            HireDate = employee.HireDate,
            TerminationDate = employee.TerminationDate,
            Status = employee.Status.ToString(),
            EmploymentType = employee.EmploymentType.ToString(),
            BasicSalary = employee.BasicSalary,
            HousingAllowance = employee.HousingAllowance,
            TransportAllowance = employee.TransportAllowance,
            MedicalAllowance = employee.MedicalAllowance,
            OtherAllowances = employee.OtherAllowances,
            GrossSalary = employee.GrossSalary,
            BankName = employee.BankName,
            AccountNumber = employee.AccountNumber,
            AccountName = employee.AccountName,
            NextOfKinName = employee.NextOfKinName,
            NextOfKinPhone = employee.NextOfKinPhone,
            NextOfKinAddress = employee.NextOfKinAddress,
            NextOfKinRelationship = employee.NextOfKinRelationship,
            RecentPayrolls = employee.PayrollEntries.Select(p => new PayrollSummaryDto
            {
                PayrollNumber = p.PayrollNumber,
                PayrollDate = p.PayrollDate,
                GrossEarnings = p.GrossEarnings,
                TotalDeductions = p.TotalDeductions,
                NetPay = p.NetPay,
                Status = p.Status.ToString()
            }).ToList(),
            CreatedAt = employee.CreatedAt
        };

        return Ok(BaseResponse<EmployeeDetailDto>.SuccessResponse(employeeDto));
    }

    [HttpPost("employees")]
    public async Task<ActionResult<BaseResponse<EmployeeDto>>> CreateEmployee([FromBody] CreateEmployeeRequest request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        // Check if employee number already exists
        var existingEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeNumber == request.EmployeeNumber && e.TenantId == tenantId);

        if (existingEmployee != null)
            return BadRequest(BaseResponse<EmployeeDto>.ErrorResponse("Employee number already exists"));

        var employee = new Employee
        {
            EmployeeNumber = request.EmployeeNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            MaritalStatus = request.MaritalStatus,
            Address = request.Address,
            City = request.City,
            State = request.State,
            BVN = request.BVN,
            NIN = request.NIN,
            TaxID = request.TaxID,
            PensionPIN = request.PensionPIN,
            Department = request.Department,
            Position = request.Position,
            Grade = request.Grade,
            HireDate = request.HireDate,
            Status = EmployeeStatus.Active,
            EmploymentType = request.EmploymentType,
            BasicSalary = request.BasicSalary,
            HousingAllowance = request.HousingAllowance,
            TransportAllowance = request.TransportAllowance,
            MedicalAllowance = request.MedicalAllowance,
            OtherAllowances = request.OtherAllowances,
            GrossSalary = request.BasicSalary + request.HousingAllowance + request.TransportAllowance + request.MedicalAllowance + request.OtherAllowances,
            BankName = request.BankName,
            AccountNumber = request.AccountNumber,
            AccountName = request.AccountName,
            NextOfKinName = request.NextOfKinName,
            NextOfKinPhone = request.NextOfKinPhone,
            NextOfKinAddress = request.NextOfKinAddress,
            NextOfKinRelationship = request.NextOfKinRelationship,
            Notes = request.Notes,
            TenantId = tenantId,
            CreatedBy = userId
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        var employeeDto = new EmployeeDto
        {
            Id = employee.Id,
            EmployeeNumber = employee.EmployeeNumber,
            FullName = employee.FullName,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            Department = employee.Department,
            Position = employee.Position,
            BasicSalary = employee.BasicSalary,
            GrossSalary = employee.GrossSalary,
            Status = employee.Status.ToString(),
            HireDate = employee.HireDate,
            CreatedAt = employee.CreatedAt
        };

        return Ok(BaseResponse<EmployeeDto>.SuccessResponse(employeeDto, "Employee created successfully"));
    }

    [HttpGet("payroll")]
    public async Task<ActionResult<BaseResponse<List<PayrollEntryDto>>>> GetPayrollEntries([FromQuery] string? period = null)
    {
        var tenantId = GetTenantId();
        
        var query = _context.PayrollEntries
            .Include(p => p.Employee)
            .Where(p => p.TenantId == tenantId && !p.IsDeleted);

        if (!string.IsNullOrEmpty(period))
        {
            if (DateTime.TryParse($"{period}-01", out var periodDate))
            {
                query = query.Where(p => p.PayPeriodStart.Year == periodDate.Year && 
                                        p.PayPeriodStart.Month == periodDate.Month);
            }
        }

        var payrollEntries = await query
            .Select(p => new PayrollEntryDto
            {
                Id = p.Id,
                PayrollNumber = p.PayrollNumber,
                EmployeeName = p.Employee.FullName,
                EmployeeNumber = p.Employee.EmployeeNumber,
                Department = p.Employee.Department,
                Position = p.Employee.Position,
                BasicSalary = p.BasicSalary,
                GrossEarnings = p.GrossEarnings,
                TotalDeductions = p.TotalDeductions,
                NetPay = p.NetPay,
                PayPeriod = $"{p.PayPeriodStart:MMM yyyy}",
                Status = p.Status.ToString(),
                PayrollDate = p.PayrollDate,
                CreatedAt = p.CreatedAt
            })
            .OrderByDescending(p => p.PayrollDate)
            .ToListAsync();

        return Ok(BaseResponse<List<PayrollEntryDto>>.SuccessResponse(payrollEntries));
    }

    [HttpPost("payroll/process")]
    public async Task<ActionResult<BaseResponse<object>>> ProcessPayroll([FromBody] ProcessPayrollRequest request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var employees = await _context.Employees
            .Where(e => e.TenantId == tenantId && 
                       e.Status == EmployeeStatus.Active && 
                       !e.IsDeleted)
            .ToListAsync();

        var payrollEntries = new List<PayrollEntry>();

        foreach (var employee in employees)
        {
            // Check if payroll already exists for this period
            var existingPayroll = await _context.PayrollEntries
                .FirstOrDefaultAsync(p => p.EmployeeId == employee.Id && 
                                         p.PayPeriodStart == request.PayPeriodStart && 
                                         p.PayPeriodEnd == request.PayPeriodEnd);

            if (existingPayroll != null) continue;

            // Calculate tax breakdown
            var taxBreakdown = _taxService.CalculatePayrollTaxes(employee, employee.GrossSalary);

            var payrollEntry = new PayrollEntry
            {
                PayrollNumber = await GeneratePayrollNumberAsync(),
                EmployeeId = employee.Id,
                PayrollDate = request.PayrollDate,
                PayPeriodStart = request.PayPeriodStart,
                PayPeriodEnd = request.PayPeriodEnd,
                BasicSalary = employee.BasicSalary,
                HousingAllowance = employee.HousingAllowance,
                TransportAllowance = employee.TransportAllowance,
                MedicalAllowance = employee.MedicalAllowance,
                OtherAllowances = employee.OtherAllowances,
                OvertimeAmount = 0, // Calculate based on overtime records
                BonusAmount = 0, // Calculate based on bonus records
                GrossEarnings = employee.GrossSalary,
                PAYEDeduction = taxBreakdown.PAYEDeduction,
                PensionDeduction = taxBreakdown.PensionDeduction,
                NHFDeduction = taxBreakdown.NHFDeduction,
                CooperativeDeduction = 0, // Calculate based on cooperative records
                LoanDeduction = 0, // Calculate based on loan records
                OtherDeductions = 0,
                TotalDeductions = taxBreakdown.TotalDeductions,
                NetPay = taxBreakdown.NetPay,
                Status = PayrollStatus.Calculated,
                ProcessedBy = userId,
                ProcessedDate = DateTime.UtcNow,
                TenantId = tenantId,
                CreatedBy = userId
            };

            payrollEntries.Add(payrollEntry);
        }

        _context.PayrollEntries.AddRange(payrollEntries);
        await _context.SaveChangesAsync();

        return Ok(BaseResponse<object>.SuccessResponse(new { 
            ProcessedCount = payrollEntries.Count,
            Message = $"Payroll processed for {payrollEntries.Count} employees"
        }));
    }

    [HttpPost("payroll/{id}/approve")]
    public async Task<ActionResult<BaseResponse<object>>> ApprovePayroll(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var payrollEntry = await _context.PayrollEntries
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

        if (payrollEntry == null)
            return NotFound(BaseResponse<object>.ErrorResponse("Payroll entry not found"));

        if (payrollEntry.Status != PayrollStatus.Calculated)
            return BadRequest(BaseResponse<object>.ErrorResponse("Payroll entry is not in calculated status"));

        payrollEntry.Status = PayrollStatus.Approved;
        payrollEntry.ApprovedBy = userId;
        payrollEntry.ApprovedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(BaseResponse<object>.SuccessResponse(new { 
            Message = "Payroll entry approved successfully"
        }));
    }

    [HttpPost("payroll/batch-approve")]
    public async Task<ActionResult<BaseResponse<object>>> BatchApprovePayroll([FromBody] BatchApproveRequest request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var payrollEntries = await _context.PayrollEntries
            .Where(p => request.PayrollIds.Contains(p.Id) && 
                       p.TenantId == tenantId && 
                       p.Status == PayrollStatus.Calculated)
            .ToListAsync();

        foreach (var entry in payrollEntries)
        {
            entry.Status = PayrollStatus.Approved;
            entry.ApprovedBy = userId;
            entry.ApprovedDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Ok(BaseResponse<object>.SuccessResponse(new { 
            ApprovedCount = payrollEntries.Count,
            Message = $"{payrollEntries.Count} payroll entries approved successfully"
        }));
    }

    [HttpGet("reports/payroll-summary")]
    public async Task<ActionResult<BaseResponse<PayrollSummaryReportDto>>> GetPayrollSummary([FromQuery] string period)
    {
        var tenantId = GetTenantId();
        
        if (!DateTime.TryParse($"{period}-01", out var periodDate))
            return BadRequest(BaseResponse<PayrollSummaryReportDto>.ErrorResponse("Invalid period format"));

        var payrollEntries = await _context.PayrollEntries
            .Include(p => p.Employee)
            .Where(p => p.TenantId == tenantId && 
                       p.PayPeriodStart.Year == periodDate.Year && 
                       p.PayPeriodStart.Month == periodDate.Month)
            .ToListAsync();

        var summary = new PayrollSummaryReportDto
        {
            Period = period,
            TotalEmployees = payrollEntries.Count,
            TotalGrossEarnings = payrollEntries.Sum(p => p.GrossEarnings),
            TotalDeductions = payrollEntries.Sum(p => p.TotalDeductions),
            TotalNetPay = payrollEntries.Sum(p => p.NetPay),
            TotalPAYE = payrollEntries.Sum(p => p.PAYEDeduction),
            TotalPension = payrollEntries.Sum(p => p.PensionDeduction),
            TotalNHF = payrollEntries.Sum(p => p.NHFDeduction),
            DepartmentBreakdown = payrollEntries
                .GroupBy(p => p.Employee.Department)
                .Select(g => new DepartmentPayrollDto
                {
                    Department = g.Key,
                    EmployeeCount = g.Count(),
                    TotalGross = g.Sum(p => p.GrossEarnings),
                    TotalNet = g.Sum(p => p.NetPay)
                })
                .ToList()
        };

        return Ok(BaseResponse<PayrollSummaryReportDto>.SuccessResponse(summary));
    }

    private async Task<string> GeneratePayrollNumberAsync()
    {
        var lastPayroll = await _context.PayrollEntries
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastPayroll != null && lastPayroll.PayrollNumber.StartsWith("PAY"))
        {
            if (int.TryParse(lastPayroll.PayrollNumber.Substring(3), out var lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"PAY{DateTime.UtcNow:yyyyMM}{nextNumber:D3}";
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
public class EmployeeDto
{
    public Guid Id { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal GrossSalary { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EmployeeDetailDto
{
    public Guid Id { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? BVN { get; set; }
    public string? NIN { get; set; }
    public string? TaxID { get; set; }
    public string? PensionPIN { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? Grade { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal HousingAllowance { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal GrossSalary { get; set; }
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountName { get; set; }
    public string? NextOfKinName { get; set; }
    public string? NextOfKinPhone { get; set; }
    public string? NextOfKinAddress { get; set; }
    public string? NextOfKinRelationship { get; set; }
    public List<PayrollSummaryDto> RecentPayrolls { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class PayrollEntryDto
{
    public Guid Id { get; set; }
    public string PayrollNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal GrossEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay { get; set; }
    public string PayPeriod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime PayrollDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PayrollSummaryDto
{
    public string PayrollNumber { get; set; } = string.Empty;
    public DateTime PayrollDate { get; set; }
    public decimal GrossEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PayrollSummaryReportDto
{
    public string Period { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public decimal TotalGrossEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNetPay { get; set; }
    public decimal TotalPAYE { get; set; }
    public decimal TotalPension { get; set; }
    public decimal TotalNHF { get; set; }
    public List<DepartmentPayrollDto> DepartmentBreakdown { get; set; } = new();
}

public class DepartmentPayrollDto
{
    public string Department { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalNet { get; set; }
}

public class CreateEmployeeRequest
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? BVN { get; set; }
    public string? NIN { get; set; }
    public string? TaxID { get; set; }
    public string? PensionPIN { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? Grade { get; set; }
    public DateTime HireDate { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal HousingAllowance { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountName { get; set; }
    public string? NextOfKinName { get; set; }
    public string? NextOfKinPhone { get; set; }
    public string? NextOfKinAddress { get; set; }
    public string? NextOfKinRelationship { get; set; }
    public string? Notes { get; set; }
}

public class ProcessPayrollRequest
{
    public DateTime PayrollDate { get; set; }
    public DateTime PayPeriodStart { get; set; }
    public DateTime PayPeriodEnd { get; set; }
}

public class BatchApproveRequest
{
    public List<Guid> PayrollIds { get; set; } = new();
}