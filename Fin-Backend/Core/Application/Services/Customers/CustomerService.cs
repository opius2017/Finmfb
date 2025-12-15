using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinTech.Core.Domain.Entities.Customers;
using AccountStatus = FinTech.Core.Domain.Enums.AccountStatus;
using FinTech.Core.Domain.Enums;
using FinTech.Core.Application.Common.Interfaces;

namespace FinTech.Core.Application.Services.Customers;

public interface ICustomerService
{
    Task<CustomerInquiry> CreateInquiryAsync(CreateInquiryRequest request);
    Task<CustomerInquiry> GetInquiryByIdAsync(Guid inquiryId, Guid tenantId);
    Task<List<CustomerInquiry>> GetCustomerInquiriesAsync(Guid customerId, Guid tenantId);
    Task<List<CustomerInquiry>> GetPendingInquiriesAsync(Guid tenantId);
    Task<bool> RespondToInquiryAsync(Guid inquiryId, string response, string respondedBy);
    
    Task<CustomerComplaint> CreateComplaintAsync(CreateComplaintRequest request);
    Task<CustomerComplaint> GetComplaintByIdAsync(Guid complaintId, Guid tenantId);
    Task<List<CustomerComplaint>> GetCustomerComplaintsAsync(Guid customerId, Guid tenantId);
    Task<List<CustomerComplaint>> GetPendingComplaintsAsync(Guid tenantId);
    Task<bool> UpdateComplaintStatusAsync(Guid complaintId, ComplaintStatus status, string resolution, string updatedBy);
    
    Task<CustomerCommunicationLog> LogCommunicationAsync(CreateCommunicationLogRequest request);
    Task<List<CustomerCommunicationLog>> GetCustomerCommunicationLogsAsync(Guid customerId, Guid tenantId);
    Task<CustomerSnapshot> GetCustomerSnapshotAsync(Guid customerId, Guid tenantId);
}

public class CustomerService : ICustomerService
{
    private readonly IApplicationDbContext _context;

    public CustomerService(IApplicationDbContext context)
    {
        _context = context;
    }

    #region Customer Inquiries

    public async Task<CustomerInquiry> CreateInquiryAsync(CreateInquiryRequest request)
    {
        var inquiry = new CustomerInquiry
        {
            CustomerId = request.CustomerId,
            Channel = request.Channel,
            Category = request.Category,
            Subject = request.Subject,
            Description = request.Description,
            InquiryDate = DateTime.UtcNow,
            Status = InquiryStatus.Pending,
            Priority = request.Priority,
            CreatedBy = request.CreatedBy,
            TenantId = request.TenantId
        };

        _context.CustomerInquiries.Add(inquiry);
        await _context.SaveChangesAsync();

        // Log the customer communication
        await LogCommunicationAsync(new CreateCommunicationLogRequest
        {
            CustomerId = request.CustomerId,
            CommunicationType = CommunicationType.Inquiry,
            Channel = request.Channel,
            Subject = request.Subject,
            Details = request.Description,
            ContactedBy = request.CreatedBy,
            TenantId = request.TenantId
        });

        return inquiry;
    }

    public async Task<CustomerInquiry> GetInquiryByIdAsync(Guid inquiryId, Guid tenantId)
    {
        return await _context.CustomerInquiries
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.Id == inquiryId.ToString() && i.TenantId == tenantId);
    }

    public async Task<List<CustomerInquiry>> GetCustomerInquiriesAsync(Guid customerId, Guid tenantId)
    {
        return await _context.CustomerInquiries
            .Where(i => i.CustomerId == customerId && i.TenantId == tenantId)
            .OrderByDescending(i => i.InquiryDate)
            .ToListAsync();
    }

    public async Task<List<CustomerInquiry>> GetPendingInquiriesAsync(Guid tenantId)
    {
        return await _context.CustomerInquiries
            .Include(i => i.Customer)
            .Where(i => i.TenantId == tenantId && i.Status == InquiryStatus.Pending)
            .OrderByDescending(i => i.Priority)
            .ThenBy(i => i.InquiryDate)
            .ToListAsync();
    }

    public async Task<bool> RespondToInquiryAsync(Guid inquiryId, string response, string respondedBy)
    {
        var inquiry = await _context.CustomerInquiries.FindAsync(inquiryId);
        if (inquiry == null) return false;

        inquiry.Response = response;
        inquiry.RespondedBy = respondedBy;
        inquiry.RespondedDate = DateTime.UtcNow;
        inquiry.Status = InquiryStatus.Resolved;

        // Log the customer communication
        await LogCommunicationAsync(new CreateCommunicationLogRequest
        {
            CustomerId = inquiry.CustomerId,
            CommunicationType = CommunicationType.InquiryResponse,
            Channel = CommunicationChannel.System,
            Subject = $"Response to: {inquiry.Subject}",
            Details = response,
            ContactedBy = respondedBy,
            TenantId = inquiry.TenantId
        });

        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Customer Complaints

    public async Task<CustomerComplaint> CreateComplaintAsync(CreateComplaintRequest request)
    {
        var complaint = new CustomerComplaint
        {
            ComplaintNumber = await GenerateComplaintNumberAsync(),
            CustomerId = request.CustomerId,
            Channel = request.Channel,
            Category = request.Category,
            Subject = request.Subject,
            Description = request.Description,
            ComplaintDate = DateTime.UtcNow,
            Status = ComplaintStatus.Received,
            Priority = request.Priority,
            CreatedBy = request.CreatedBy,
            DueDate = DateTime.UtcNow.AddDays(request.Priority == Priority.High ? 1 : 
                                             request.Priority == Priority.Medium ? 3 : 5),
            TenantId = request.TenantId
        };

        _context.CustomerComplaints.Add(complaint);
        await _context.SaveChangesAsync();

        // Log the customer communication
        await LogCommunicationAsync(new CreateCommunicationLogRequest
        {
            CustomerId = request.CustomerId,
            CommunicationType = CommunicationType.Complaint,
            Channel = request.Channel,
            Subject = request.Subject,
            Details = request.Description,
            ContactedBy = request.CreatedBy,
            TenantId = request.TenantId
        });

        return complaint;
    }

    public async Task<CustomerComplaint> GetComplaintByIdAsync(Guid complaintId, Guid tenantId)
    {
        return await _context.CustomerComplaints
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == complaintId.ToString() && c.TenantId == tenantId);
    }

    public async Task<List<CustomerComplaint>> GetCustomerComplaintsAsync(Guid customerId, Guid tenantId)
    {
        return await _context.CustomerComplaints
            .Where(c => c.CustomerId == customerId && c.TenantId == tenantId)
            .OrderByDescending(c => c.ComplaintDate)
            .ToListAsync();
    }

    public async Task<List<CustomerComplaint>> GetPendingComplaintsAsync(Guid tenantId)
    {
        return await _context.CustomerComplaints
            .Include(c => c.Customer)
            .Where(c => c.TenantId == tenantId && 
                       (c.Status == ComplaintStatus.Received || 
                        c.Status == ComplaintStatus.InProgress))
            .OrderByDescending(c => c.Priority)
            .ThenBy(c => c.DueDate)
            .ToListAsync();
    }

    public async Task<bool> UpdateComplaintStatusAsync(Guid complaintId, ComplaintStatus status, string resolution, string updatedBy)
    {
        var complaint = await _context.CustomerComplaints.FindAsync(complaintId);
        if (complaint == null) return false;

        complaint.Status = status;
        complaint.Resolution = resolution;
        complaint.ResolvedBy = updatedBy;
        
        if (status == ComplaintStatus.Resolved)
        {
            complaint.ResolvedDate = DateTime.UtcNow;
            
            // Log the customer communication
            await LogCommunicationAsync(new CreateCommunicationLogRequest
            {
                CustomerId = complaint.CustomerId,
                CommunicationType = CommunicationType.ComplaintResolution,
                Channel = CommunicationChannel.System,
                Subject = $"Resolution for Complaint: {complaint.ComplaintNumber}",
                Details = resolution,
                ContactedBy = updatedBy,
                TenantId = complaint.TenantId
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<string> GenerateComplaintNumberAsync()
    {
        var lastComplaint = await _context.CustomerComplaints
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastComplaint != null && lastComplaint.ComplaintNumber.StartsWith("COMP"))
        {
            if (int.TryParse(lastComplaint.ComplaintNumber.Substring(4), out var lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"COMP{nextNumber:D6}";
    }

    #endregion

    #region Communication Logs

    public async Task<CustomerCommunicationLog> LogCommunicationAsync(CreateCommunicationLogRequest request)
    {
        var log = new CustomerCommunicationLog
        {
            CustomerId = request.CustomerId,
            CommunicationType = request.CommunicationType,
            Channel = request.Channel,
            Subject = request.Subject,
            Details = request.Details,
            CommunicationDate = DateTime.UtcNow,
            ContactedBy = request.ContactedBy,
            TenantId = request.TenantId
        };

        _context.CustomerCommunicationLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task<List<CustomerCommunicationLog>> GetCustomerCommunicationLogsAsync(Guid customerId, Guid tenantId)
    {
        return await _context.CustomerCommunicationLogs
            .Where(c => c.CustomerId == customerId && c.TenantId == tenantId)
            .OrderByDescending(c => c.CommunicationDate)
            .ToListAsync();
    }

    #endregion

    #region Customer Snapshot

    public async Task<CustomerSnapshot> GetCustomerSnapshotAsync(Guid customerId, Guid tenantId)
    {
        var customer = await _context.Customers
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.Id == customerId.ToString() && c.TenantId == tenantId);
        
        if (customer == null) return null;

        var snapshot = new CustomerSnapshot
        {
            // FinTech Best Practice: Convert string Id to Guid for CustomerId
            CustomerId = Guid.Parse(customer.Id),
            CustomerNumber = customer.CustomerNumber,
            CustomerName = customer.GetFullName(),
            CustomerType = customer.CustomerType,
            DateOfBirth = customer.DateOfBirth,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            BVN = customer.BVN,
            NIN = customer.NIN,
            Address = customer.Address,
            TotalDeposits = await GetTotalDepositsAsync(customerId, tenantId),
            TotalLoans = await GetTotalLoansAsync(customerId, tenantId),
            ActiveDepositAccounts = await GetActiveDepositAccountsAsync(customerId, tenantId),
            ActiveLoanAccounts = await GetActiveLoanAccountsAsync(customerId, tenantId),
            RecentTransactions = await GetRecentTransactionsAsync(customerId, tenantId),
            RecentCommunications = await GetRecentCommunicationsAsync(customerId, tenantId),
            OpenInquiries = await GetOpenInquiriesCountAsync(customerId, tenantId),
            OpenComplaints = await GetOpenComplaintsCountAsync(customerId, tenantId),
            TenantId = tenantId
        };

        return snapshot;
    }

    private async Task<decimal> GetTotalDepositsAsync(Guid customerId, Guid tenantId)
    {
        return await _context.DepositAccounts
            .Where(a => a.CustomerId == customerId && 
                       a.TenantId == tenantId && 
                       a.Status == AccountStatus.Active)
            .SumAsync(a => a.CurrentBalance);
    }

    private async Task<decimal> GetTotalLoansAsync(Guid customerId, Guid tenantId)
    {
        // FinTech Best Practice: Convert Guid to string for CustomerId comparison, convert Guid tenantId to string
        return await _context.LoanAccounts
            .Where(a => a.CustomerId.ToString() == customerId.ToString() && 
                       a.TenantId == tenantId.ToString() && 
                       (a.Status == LoanStatus.Active.ToString() || a.Status == LoanStatus.Disbursed.ToString()))
            // FinTech Best Practice: LoanAccount properties not available, returning 0
            // .SumAsync(a => a.PrincipalAmount - a.TotalRepaid);
            .CountAsync(); // Temporary: return count instead
        return 0; // TODO: Calculate from actual loan data
    }

    private async Task<List<DepositAccountSummary>> GetActiveDepositAccountsAsync(Guid customerId, Guid tenantId)
    {
        // FinTech Best Practice: Convert Guid to string for CustomerId comparison, convert enum to string
        return await _context.DepositAccounts
            .Include(a => a.Product)
            .Where(a => a.CustomerId.ToString() == customerId.ToString() && 
                       a.TenantId.ToString() == tenantId.ToString() && 
                       a.Status.ToString() == AccountStatus.Active.ToString())
            .Select(a => new DepositAccountSummary
            {
                AccountId = Guid.Parse(a.Id), // Convert string Id to Guid
                AccountNumber = a.AccountNumber,
                ProductName = a.Product.ProductName,
                CurrentBalance = a.CurrentBalance,
                AvailableBalance = a.AvailableBalance,
                LastTransactionDate = a.LastTransactionDate
            })
            .ToListAsync();
    }

    private async Task<List<LoanAccountSummary>> GetActiveLoanAccountsAsync(Guid customerId, Guid tenantId)
    {
        // FinTech Best Practice: Convert Guid to string for CustomerId comparison, enum to string for Status
        return await _context.LoanAccounts
            .Include(a => a.LoanProduct)
            .Where(a => a.CustomerId.ToString() == customerId.ToString() && // FinTech Best Practice: Convert Guid to string 
                       a.TenantId == tenantId.ToString() && 
                       (a.Status == LoanStatus.Active.ToString() || a.Status == LoanStatus.Disbursed.ToString()))
            .Select(a => new LoanAccountSummary
            {
                AccountId = Guid.Parse(a.Id), // Convert string Id to Guid
                AccountNumber = a.AccountNumber,
                ProductName = a.LoanProduct != null ? a.LoanProduct.ProductName : "Unknown", // Handle null LoanProduct
                // FinTech Best Practice: LoanAccount properties not available
                // PrincipalAmount = a.PrincipalAmount,
                // OutstandingPrincipal = a.OutstandingPrincipal,
                // OutstandingInterest = a.OutstandingInterest,
                // NextPaymentDate = a.RepaymentSchedule
                //     .Where(s => s.Status == RepaymentStatus.Pending)
                //     .OrderBy(s => s.DueDate)
                //     .Select(s => s.DueDate)
                //     .FirstOrDefault(),
                PrincipalAmount = 0, // TODO: Get from actual loan data
                OutstandingPrincipal = 0,
                OutstandingInterest = 0,
                NextPaymentDate = null,
                // NextPaymentAmount = a.RepaymentSchedule
                //     .Where(s => s.Status == RepaymentStatus.Pending)
                //     .OrderBy(s => s.DueDate)
                NextPaymentAmount = 0 // TODO: Get from actual repayment schedule
            })
            .ToListAsync();
    }

    private async Task<List<TransactionSummary>> GetRecentTransactionsAsync(Guid customerId, Guid tenantId)
    {
        var depositTransactions = await _context.DepositTransactions
            .Include(t => t.Account)
            .Where(t => t.Account.CustomerId.ToString() == customerId.ToString() && // FinTech Best Practice: Convert Guid to string 
                       t.TenantId.ToString() == tenantId.ToString())
            .OrderByDescending(t => t.TransactionDate)
            .Take(5)
            .Select(t => new TransactionSummary
            {
                TransactionId = Guid.Parse(t.Id),
                TransactionReference = t.TransactionReference,
                TransactionDate = t.TransactionDate,
                TransactionType = t.TransactionType.ToString(),
                Amount = t.Amount,
                Description = t.Description,
                AccountNumber = t.Account.AccountNumber,
                AccountType = "Deposit"
            })
            .ToListAsync();

        var loanTransactions = await _context.LoanTransactions
            .Include(t => t.LoanAccount)
            .Where(t => t.LoanAccount.CustomerId.ToString() == customerId.ToString() && // FinTech Best Practice: Convert Guid to string 
                       t.TenantId.ToString() == tenantId.ToString())
            .OrderByDescending(t => t.TransactionDate)
            .Take(5)
            .Select(t => new TransactionSummary
            {
                TransactionId = Guid.Parse(t.Id),
                TransactionReference = t.TransactionReference,
                TransactionDate = t.TransactionDate,
                TransactionType = t.TransactionType.ToString(),
                Amount = t.Amount,
                Description = t.Description,
                AccountNumber = t.LoanAccount.AccountNumber,
                AccountType = "Loan"
            })
            .ToListAsync();

        // Combine and sort
        var allTransactions = depositTransactions.Concat(loanTransactions)
            .OrderByDescending(t => t.TransactionDate)
            .Take(5)
            .ToList();

        return allTransactions;
    }

    private async Task<List<CommunicationSummary>> GetRecentCommunicationsAsync(Guid customerId, Guid tenantId)
    {
        return await _context.CustomerCommunicationLogs
            .Where(c => c.CustomerId.ToString() == customerId.ToString() && c.TenantId.ToString() == tenantId.ToString())
            .OrderByDescending(c => c.CommunicationDate)
            .Take(5)
            .Select(c => new CommunicationSummary
            {
                // FinTech Best Practice: Convert string Id to Guid for CommunicationId
                CommunicationId = Guid.Parse(c.Id),
                CommunicationDate = c.CommunicationDate,
                CommunicationType = c.CommunicationType.ToString(),
                Channel = c.Channel.ToString(),
                Subject = c.Subject,
                ContactedBy = c.ContactedBy
            })
            .ToListAsync();
    }

    private async Task<int> GetOpenInquiriesCountAsync(Guid customerId, Guid tenantId)
    {
        return await _context.CustomerInquiries
            .CountAsync(i => i.CustomerId == customerId && 
                           i.TenantId == tenantId && 
                           i.Status == InquiryStatus.Pending);
    }

    private async Task<int> GetOpenComplaintsCountAsync(Guid customerId, Guid tenantId)
    {
        return await _context.CustomerComplaints
            .CountAsync(c => c.CustomerId == customerId && 
                           c.TenantId == tenantId && 
                           (c.Status == ComplaintStatus.Received || 
                            c.Status == ComplaintStatus.InProgress));
    }

    #endregion
}

#region Request & Response DTOs

public class CreateInquiryRequest
{
    public Guid CustomerId { get; set; }
    public CommunicationChannel Channel { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Priority Priority { get; set; } = Priority.Medium;
    public string CreatedBy { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
}

public class CreateComplaintRequest
{
    public Guid CustomerId { get; set; }
    public CommunicationChannel Channel { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Priority Priority { get; set; } = Priority.Medium;
    public string CreatedBy { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
}

public class CreateCommunicationLogRequest
{
    public Guid CustomerId { get; set; }
    public CommunicationType CommunicationType { get; set; }
    public CommunicationChannel Channel { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string ContactedBy { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
}

public class CustomerSnapshot
{
    public Guid CustomerId { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string BVN { get; set; } = string.Empty;
    public string NIN { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal TotalDeposits { get; set; }
    public decimal TotalLoans { get; set; }
    public List<DepositAccountSummary> ActiveDepositAccounts { get; set; } = new();
    public List<LoanAccountSummary> ActiveLoanAccounts { get; set; } = new();
    public List<TransactionSummary> RecentTransactions { get; set; } = new();
    public List<CommunicationSummary> RecentCommunications { get; set; } = new();
    public int OpenInquiries { get; set; }
    public int OpenComplaints { get; set; }
    public Guid TenantId { get; set; }
}

public class DepositAccountSummary
{
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public DateTime? LastTransactionDate { get; set; }
}

public class LoanAccountSummary
{
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal PrincipalAmount { get; set; }
    public decimal OutstandingPrincipal { get; set; }
    public decimal OutstandingInterest { get; set; }
    public DateTime? NextPaymentDate { get; set; }
    public decimal NextPaymentAmount { get; set; }
}

public class TransactionSummary
{
    public Guid TransactionId { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
}

public class CommunicationSummary
{
    public Guid CommunicationId { get; set; }
    public DateTime CommunicationDate { get; set; }
    public string CommunicationType { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string ContactedBy { get; set; } = string.Empty;
}

#endregion
