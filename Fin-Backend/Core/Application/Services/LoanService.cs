using Microsoft.EntityFrameworkCore;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Entities.GeneralLedger;
using FinTech.Core.Domain.Enums;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Application.Interfaces.Services;

using FinTech.Core.Application.Interfaces.Loans;

namespace FinTech.Core.Application.Services;

public class LoanService : ILoanService
{

    private readonly IApplicationDbContext _context;
    private readonly IInterestCalculationService _interestService;
    private readonly IGeneralLedgerService _glService;

    public LoanService(IApplicationDbContext context, IInterestCalculationService interestService, IGeneralLedgerService glService)
    {
        _context = context;
        _interestService = interestService;
        _glService = glService;
    }

    public async Task<IEnumerable<LoanCollateral>> GetLoanCollateralsAsync(string loanId)
    {
        return await _context.LoanCollaterals.Where(c => c.LoanId == loanId).ToListAsync();
    }

    public async Task<LoanCollateral> AddLoanCollateralAsync(string loanId, CreateLoanCollateralDto collateralDto)
    {
        var collateral = new FinTech.Core.Domain.Entities.Loans.LoanCollateral
        {
            LoanId = loanId,
            CollateralType = collateralDto.CollateralType,
            Description = collateralDto.Description,
            ValueAmount = collateralDto.Value,
            ValuationDate = collateralDto.ValuationDate,
            ValuationMethod = collateralDto.ValuationMethod,
            ValuedBy = collateralDto.ValuedBy,
            OwnerName = collateralDto.OwnerName,
            OwnerRelationshipToClient = collateralDto.OwnerRelationship,
            RegistrationNumber = collateralDto.RegistrationNumber,
            Location = collateralDto.Location,
            IsInsured = collateralDto.IsInsured,
            InsuranceCompany = collateralDto.InsuranceCompany,
            InsurancePolicyNumber = collateralDto.InsurancePolicyNumber,
            InsuranceExpiryDate = collateralDto.InsuranceExpiryDate,
            Notes = collateralDto.Notes,
            Status = FinTech.Core.Domain.Enums.CollateralStatus.Active,
            Currency = "NGN",
            ExpiryDate = DateTime.UtcNow.AddYears(1)
        };
        _context.LoanCollaterals.Add(collateral);
        await _context.SaveChangesAsync();
        return collateral;
    }

    public async Task<LoanAccount> CreateLoanAccountAsync(CreateLoanAccountRequest request)
    {
        var product = await _context.LoanProducts.FindAsync(request.ProductId);
        if (product == null) throw new ArgumentException("Invalid product ID");

        var accountNumber = await GenerateAccountNumberAsync();
        
        var loanAccount = new LoanAccount
        {
            AccountNumber = accountNumber,
            CustomerId = request.CustomerId,
            ProductId = request.ProductId,
            PrincipalAmount = request.PrincipalAmount,
            OutstandingPrincipal = request.PrincipalAmount,
            InterestRate = request.InterestRate ?? product.InterestRate,
            TenorDays = request.TenorDays,
            DisbursementDate = request.DisbursementDate,
            MaturityDate = request.DisbursementDate.AddDays(request.TenorDays),
            Status = LoanStatus.Approved,
            Classification = LoanClassification.Performing,
            Purpose = request.Purpose,
            TenantId = request.TenantId
        };

        _context.LoanAccounts.Add(loanAccount);
        await _context.SaveChangesAsync();

        // Generate repayment schedule
        await GenerateRepaymentScheduleAsync(loanAccount.Id);

        return loanAccount;
    }

    public async Task<bool> DisburseLoanAsync(Guid loanAccountId, decimal amount, string disbursedBy)
    {
        var loanAccount = await _context.LoanAccounts.FindAsync(loanAccountId);
        if (loanAccount == null) return false;

        // Create disbursement transaction
        var transaction = new LoanTransaction
        {
            LoanAccountId = loanAccountId,
            TransactionReference = await GenerateTransactionReferenceAsync(),
            TransactionType = LoanTransactionType.Disbursement,
            Amount = amount,
            PrincipalAmount = amount,
            TransactionDate = DateTime.UtcNow,
            ValueDate = DateTime.UtcNow,
            Description = "Loan Disbursement",
            Status = TransactionStatus.Posted,
            ProcessedBy = disbursedBy,
            ProcessedDate = DateTime.UtcNow,
            TenantId = loanAccount.TenantId
        };

        _context.LoanTransactions.Add(transaction);

        // Update loan account status
        loanAccount.Status = LoanStatus.Disbursed;
        loanAccount.DisbursedBy = disbursedBy;
        loanAccount.DisbursedDate = DateTime.UtcNow;

        // Post to General Ledger
        await _glService.PostLoanDisbursementAsync(loanAccount, amount, disbursedBy);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ProcessRepaymentAsync(Guid loanAccountId, decimal amount, string processedBy)
    {
        var loanAccount = await _context.LoanAccounts
            .Include(l => l.RepaymentSchedule)
            .FirstOrDefaultAsync(l => l.Id == loanAccountId);
        
        if (loanAccount == null) return false;

        var remainingAmount = amount;
        var interestPaid = 0m;
        var principalPaid = 0m;

        // Apply payment to outstanding interest first, then principal
        if (loanAccount.OutstandingInterest > 0)
        {
            interestPaid = Math.Min(remainingAmount, loanAccount.OutstandingInterest);
            loanAccount.OutstandingInterest -= interestPaid;
            remainingAmount -= interestPaid;
        }

        if (remainingAmount > 0 && loanAccount.OutstandingPrincipal > 0)
        {
            principalPaid = Math.Min(remainingAmount, loanAccount.OutstandingPrincipal);
            loanAccount.OutstandingPrincipal -= principalPaid;
        }

        // Create repayment transaction
        var transaction = new LoanTransaction
        {
            LoanAccountId = loanAccountId,
            TransactionReference = await GenerateTransactionReferenceAsync(),
            TransactionType = LoanTransactionType.Repayment,
            Amount = amount,
            PrincipalAmount = principalPaid,
            InterestAmount = interestPaid,
            TransactionDate = DateTime.UtcNow,
            ValueDate = DateTime.UtcNow,
            Description = "Loan Repayment",
            Status = TransactionStatus.Posted,
            ProcessedBy = processedBy,
            ProcessedDate = DateTime.UtcNow,
            TenantId = loanAccount.TenantId
        };

        _context.LoanTransactions.Add(transaction);

        // Update repayment schedule
        await UpdateRepaymentScheduleAsync(loanAccount, amount);

        // Post to General Ledger
        await _glService.PostLoanRepaymentAsync(loanAccount, principalPaid, interestPaid, processedBy);

        // Check if loan is fully paid
        if (loanAccount.OutstandingPrincipal <= 0 && loanAccount.OutstandingInterest <= 0)
        {
            loanAccount.Status = LoanStatus.Closed;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<LoanRepaymentSchedule>> GenerateRepaymentScheduleAsync(Guid loanAccountId)
    {
        var loanAccount = await _context.LoanAccounts
            .Include(l => l.Product)
            .FirstOrDefaultAsync(l => l.Id == loanAccountId);
        
        if (loanAccount == null) return new List<LoanRepaymentSchedule>();

        var schedules = new List<LoanRepaymentSchedule>();
        var installmentAmount = _interestService.CalculateInstallmentAmount(
            loanAccount.PrincipalAmount, 
            loanAccount.InterestRate, 
            loanAccount.TenorDays,
            loanAccount.Product.RepaymentFrequency);

        var numberOfInstallments = _interestService.GetNumberOfInstallments(
            loanAccount.TenorDays, 
            loanAccount.Product.RepaymentFrequency);

        var currentDate = loanAccount.DisbursementDate;
        var outstandingPrincipal = loanAccount.PrincipalAmount;

        for (int i = 1; i <= numberOfInstallments; i++)
        {
            currentDate = _interestService.GetNextPaymentDate(currentDate, loanAccount.Product.RepaymentFrequency);
            
            var interestAmount = _interestService.CalculateInterestAmount(
                outstandingPrincipal, 
                loanAccount.InterestRate, 
                loanAccount.Product.RepaymentFrequency);
            
            var principalAmount = installmentAmount - interestAmount;
            outstandingPrincipal -= principalAmount;

            var schedule = new LoanRepaymentSchedule
            {
                LoanAccountId = loanAccountId,
                InstallmentNumber = i,
                DueDate = currentDate,
                PrincipalAmount = principalAmount,
                InterestAmount = interestAmount,
                TotalAmount = installmentAmount,
                OutstandingPrincipal = Math.Max(0, outstandingPrincipal),
                OutstandingInterest = interestAmount,
                OutstandingTotal = installmentAmount,
                Status = RepaymentStatus.Pending,
                TenantId = loanAccount.TenantId
            };

            schedules.Add(schedule);
        }

        _context.LoanRepaymentSchedules.AddRange(schedules);
        await _context.SaveChangesAsync();

        return schedules;
    }

    public async Task<bool> ClassifyLoansAsync(Guid tenantId)
    {
        var loans = await _context.LoanAccounts
            .Where(l => l.TenantId == tenantId && l.Status == LoanStatus.Active)
            .ToListAsync();

        foreach (var loan in loans)
        {
            // Calculate days past due
            var overdueDays = await CalculateDaysPastDueAsync(loan.Id);
            loan.DaysPastDue = overdueDays;

            // IFRS 9 Classification based on days past due
            loan.Classification = overdueDays switch
            {
                <= 0 => LoanClassification.Performing,
                <= 30 => LoanClassification.SpecialMention,
                <= 90 => LoanClassification.Substandard,
                <= 180 => LoanClassification.Doubtful,
                _ => LoanClassification.Lost
            };

            // Calculate provision
            loan.ProvisionAmount = await CalculateProvisionAsync(loan.Id);
            loan.ProvisionRate = loan.OutstandingPrincipal > 0 
                ? (loan.ProvisionAmount / loan.OutstandingPrincipal) * 100 
                : 0;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> CalculateProvisionAsync(Guid loanAccountId)
    {
        var loan = await _context.LoanAccounts.FindAsync(loanAccountId);
        if (loan == null) return 0;

        // IFRS 9 Provision Rates
        var provisionRate = loan.Classification switch
        {
            LoanClassification.Performing => 0.01m, // 1%
            LoanClassification.SpecialMention => 0.03m, // 3%
            LoanClassification.Substandard => 0.20m, // 20%
            LoanClassification.Doubtful => 0.50m, // 50%
            LoanClassification.Lost => 1.00m, // 100%
            _ => 0.01m
        };

        return loan.OutstandingPrincipal * provisionRate;
    }

    private async Task<string> GenerateAccountNumberAsync()
    {
        var lastAccount = await _context.LoanAccounts
            .OrderByDescending(l => l.CreatedAt)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastAccount != null && lastAccount.AccountNumber.StartsWith("LN"))
        {
            if (int.TryParse(lastAccount.AccountNumber.Substring(2), out var lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"LN{nextNumber:D9}";
    }

    private async Task<string> GenerateTransactionReferenceAsync()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"LTX{timestamp}{random}";
    }

    private async Task<int> CalculateDaysPastDueAsync(Guid loanAccountId)
    {
        var overdueSchedule = await _context.LoanRepaymentSchedules
            .Where(s => s.LoanAccountId == loanAccountId && 
                       s.Status == RepaymentStatus.Pending && 
                       s.DueDate < DateTime.UtcNow)
            .OrderBy(s => s.DueDate)
            .FirstOrDefaultAsync();

        return overdueSchedule != null 
            ? (DateTime.UtcNow - overdueSchedule.DueDate).Days 
            : 0;
    }

    private async Task UpdateRepaymentScheduleAsync(LoanAccount loanAccount, decimal paymentAmount)
    {
        var pendingSchedules = await _context.LoanRepaymentSchedules
            .Where(s => s.LoanAccountId == loanAccount.Id && s.Status == RepaymentStatus.Pending)
            .OrderBy(s => s.DueDate)
            .ToListAsync();

        var remainingPayment = paymentAmount;

        foreach (var schedule in pendingSchedules)
        {
            if (remainingPayment <= 0) break;

            var paymentForThisSchedule = Math.Min(remainingPayment, schedule.OutstandingTotal);
            
            schedule.PaidTotal += paymentForThisSchedule;
            schedule.OutstandingTotal -= paymentForThisSchedule;
            
            if (schedule.OutstandingTotal <= 0)
            {
                schedule.Status = RepaymentStatus.Paid;
                schedule.PaidDate = DateTime.UtcNow;
            }
            else
            {
                schedule.Status = RepaymentStatus.PartiallyPaid;
            }

            remainingPayment -= paymentForThisSchedule;
        }
    }
}

public class CreateLoanAccountRequest
{
    public Guid CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal? InterestRate { get; set; }
    public int TenorDays { get; set; }
    public DateTime DisbursementDate { get; set; }
    public string? Purpose { get; set; }
    public Guid TenantId { get; set; }
}
