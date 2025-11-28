using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Entities.Customers;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTech.Core.Application.Services
{
    public class ClientLoanService : IClientLoanService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ILogger<ClientLoanService> _logger;

        public ClientLoanService(IApplicationDbContext dbContext, ILogger<ClientLoanService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IEnumerable<LoanAccount>> GetClientLoansAsync(Guid customerId)
        {
            try
            {
                return await _dbContext.LoanAccounts
                    .Where(l => l.CustomerId.ToString() == customerId.ToString())
                    .Include(l => l.LoanProduct)
                    .OrderByDescending(l => l.DisbursementDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving loans for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<LoanAccount> GetLoanDetailsAsync(string loanAccountNumber)
        {
            try
            {
                var loan = await _dbContext.LoanAccounts
                    .Include(l => l.LoanProduct)
                    .Include(l => l.Customer)
                    .FirstOrDefaultAsync(l => l.AccountNumber == loanAccountNumber);

                if (loan == null)
                {
                    throw new KeyNotFoundException($"Loan with account number {loanAccountNumber} not found.");
                }

                return loan;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving loan details for loan account {LoanAccountNumber}", loanAccountNumber);
                throw;
            }
        }

        public async Task<IEnumerable<LoanRepaymentSchedule>> GetLoanRepaymentScheduleAsync(string loanAccountNumber)
        {
            try
            {
                return await _dbContext.LoanRepaymentSchedules
                    .Where(s => s.LoanAccount.AccountNumber == loanAccountNumber)
                    .OrderBy(s => s.InstallmentNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving repayment schedule for loan account {LoanAccountNumber}", loanAccountNumber);
                throw;
            }
        }

        public async Task<IEnumerable<LoanTransaction>> GetLoanTransactionsAsync(string loanAccountNumber)
        {
            try
            {
                return await _dbContext.LoanTransactions
                    .Where(t => t.LoanAccount.AccountNumber == loanAccountNumber)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions for loan account {LoanAccountNumber}", loanAccountNumber);
                throw;
            }
        }

        public async Task<LoanApplicationRequest> SubmitLoanApplicationAsync(LoanApplicationDto applicationDto, Guid customerId)
        {
            try
            {
                // Validate loan product exists
                var loanProduct = await _dbContext.LoanProducts
                    .FirstOrDefaultAsync(p => p.Id == applicationDto.LoanProductId);
                
                if (loanProduct == null)
                {
                    throw new KeyNotFoundException($"Loan product with ID {applicationDto.LoanProductId} not found.");
                }
                
                // Validate customer exists
                var customer = await _dbContext.Customers
                    .FirstOrDefaultAsync(c => c.Id == customerId);
                    
                if (customer == null)
                {
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found.");
                }

                // Create loan application
                var application = new LoanApplicationRequest
                {
                    CustomerId = customerId,
                    LoanProductId = applicationDto.LoanProductId,
                    RequestedAmount = applicationDto.RequestedAmount,
                    RequestedTenor = applicationDto.RequestedTenor,
                    Purpose = applicationDto.Purpose,
                    RepaymentSource = applicationDto.RepaymentSource,
                    PreferredDisbursementDate = applicationDto.PreferredDisbursementDate,
                    Status = "Pending",
                    ApplicationDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.LoanApplicationRequests.Add(application);
                await _dbContext.SaveChangesAsync();

                return application;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting loan application for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<LoanApplicationRequest> GetLoanApplicationStatusAsync(Guid applicationId)
        {
            try
            {
                var application = await _dbContext.LoanApplicationRequests
                    .Include(a => a.LoanProduct)
                    .FirstOrDefaultAsync(a => a.Id == applicationId);

                if (application == null)
                {
                    throw new KeyNotFoundException($"Loan application with ID {applicationId} not found.");
                }

                return application;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving loan application status for application {ApplicationId}", applicationId);
                throw;
            }
        }

        public async Task<IEnumerable<LoanApplicationRequest>> GetClientLoanApplicationsAsync(Guid customerId)
        {
            try
            {
                return await _dbContext.LoanApplicationRequests
                    .Where(a => a.CustomerId == customerId)
                    .Include(a => a.LoanProduct)
                    .OrderByDescending(a => a.ApplicationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving loan applications for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<LoanEligibility> CheckLoanEligibilityAsync(LoanEligibilityCheckDto checkDto, Guid customerId)
        {
            try
            {
                // Get customer information
                var customer = await _dbContext.Customers
                    .FirstOrDefaultAsync(c => c.Id == customerId);
                
                if (customer == null)
                {
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found.");
                }

                // Get loan product
                var loanProduct = await _dbContext.LoanProducts
                    .FirstOrDefaultAsync(p => p.Id == checkDto.LoanProductId);
                
                if (loanProduct == null)
                {
                    throw new KeyNotFoundException($"Loan product with ID {checkDto.LoanProductId} not found.");
                }

                // In a real implementation, this would perform complex eligibility calculations
                // based on customer credit history, income, existing loans, etc.
                // For this example, we'll use a simplified approach
                
                // Check if amount is within product limits
                bool isAmountEligible = checkDto.RequestedAmount >= loanProduct.MinimumAmount &&
                                         checkDto.RequestedAmount <= loanProduct.MaximumAmount;
                
                // Check if tenor is within product limits
                bool isTenorEligible = checkDto.RequestedTenor >= loanProduct.MinimumTenor &&
                                       checkDto.RequestedTenor <= loanProduct.MaximumTenor;

                // Check for existing active loans
                var activeLoans = await _dbContext.LoanAccounts
                    .Where(l => l.CustomerId == customerId && 
                           l.Status != "Closed" && 
                           l.Status != "Fully Paid")
                    .ToListAsync();

                decimal totalOutstandingLoans = activeLoans.Sum(l => l.OutstandingBalance);
                
                // Basic debt service ratio calculation (simplified)
                decimal monthlyIncome = checkDto.MonthlyIncome;
                decimal existingMonthlyDebt = totalOutstandingLoans * 0.1m; // Simplified: assume 10% of outstanding is monthly payment
                decimal newLoanMonthlyPayment = checkDto.RequestedAmount * (loanProduct.InterestRate / 100 / 12) *
                                               (decimal)Math.Pow(1 + (double)(loanProduct.InterestRate / 100 / 12), checkDto.RequestedTenor) /
                                               ((decimal)Math.Pow(1 + (double)(loanProduct.InterestRate / 100 / 12), checkDto.RequestedTenor) - 1);
                
                decimal debtServiceRatio = (existingMonthlyDebt + newLoanMonthlyPayment) / monthlyIncome;
                bool isDebtRatioAcceptable = debtServiceRatio <= 0.5m; // 50% maximum debt-to-income ratio
                
                // Generate eligibility result
                var eligibility = new LoanEligibility
                {
                    IsEligible = isAmountEligible && isTenorEligible && isDebtRatioAcceptable,
                    MaximumEligibleAmount = isDebtRatioAcceptable ? 
                        Math.Min(loanProduct.MaximumAmount, monthlyIncome * 0.5m * checkDto.RequestedTenor) :
                        0,
                    RecommendedTenor = checkDto.RequestedTenor,
                    EligibilityReasons = new List<string>()
                };

                // Add reasons for ineligibility if applicable
                if (!isAmountEligible)
                {
                    eligibility.EligibilityReasons.Add($"Requested amount is outside the product limits ({loanProduct.MinimumAmount} - {loanProduct.MaximumAmount}).");
                }
                
                if (!isTenorEligible)
                {
                    eligibility.EligibilityReasons.Add($"Requested tenor is outside the product limits ({loanProduct.MinimumTenor} - {loanProduct.MaximumTenor} months).");
                }
                
                if (!isDebtRatioAcceptable)
                {
                    eligibility.EligibilityReasons.Add($"Debt-to-income ratio ({debtServiceRatio:P}) exceeds maximum allowed (50%).");
                }

                return eligibility;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking loan eligibility for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<LoanSimulation> SimulateLoanAsync(LoanSimulationDto simulationDto)
        {
            try
            {
                // Get loan product
                var loanProduct = await _dbContext.LoanProducts
                    .FirstOrDefaultAsync(p => p.Id == simulationDto.LoanProductId);
                
                if (loanProduct == null)
                {
                    throw new KeyNotFoundException($"Loan product with ID {simulationDto.LoanProductId} not found.");
                }

                decimal interestRate = loanProduct.InterestRate;
                int tenor = simulationDto.Tenor;
                decimal principal = simulationDto.Amount;
                
                // Simulate repayment schedule
                var schedule = new List<LoanRepaymentSimulation>();
                decimal balance = principal;
                decimal monthlyRate = interestRate / 100 / 12;
                
                // Calculate monthly payment (PMT formula)
                decimal monthlyPayment = principal * monthlyRate * 
                                        (decimal)Math.Pow(1 + (double)monthlyRate, tenor) /
                                        ((decimal)Math.Pow(1 + (double)monthlyRate, tenor) - 1);
                
                for (int i = 1; i <= tenor; i++)
                {
                    // Calculate interest portion
                    decimal interestPayment = balance * monthlyRate;
                    
                    // Calculate principal portion
                    decimal principalPayment = monthlyPayment - interestPayment;
                    
                    // Handle final payment rounding
                    if (i == tenor)
                    {
                        principalPayment = balance;
                        monthlyPayment = principalPayment + interestPayment;
                    }
                    
                    // Update balance
                    balance -= principalPayment;
                    if (balance < 0) balance = 0;
                    
                    // Add to schedule
                    schedule.Add(new LoanRepaymentSimulation
                    {
                        InstallmentNumber = i,
                        PaymentDate = DateTime.UtcNow.AddMonths(i),
                        PrincipalAmount = principalPayment,
                        InterestAmount = interestPayment,
                        TotalPayment = monthlyPayment,
                        RemainingBalance = balance
                    });
                }

                // Calculate totals
                decimal totalInterest = schedule.Sum(s => s.InterestAmount);
                decimal totalPayment = schedule.Sum(s => s.TotalPayment);
                
                return new LoanSimulation
                {
                    LoanProductId = simulationDto.LoanProductId,
                    LoanProductName = loanProduct.ProductName,
                    PrincipalAmount = principal,
                    InterestRate = interestRate,
                    Tenor = tenor,
                    MonthlyPayment = monthlyPayment,
                    TotalInterest = totalInterest,
                    TotalPayment = totalPayment,
                    RepaymentSchedule = schedule
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error simulating loan");
                throw;
            }
        }

        public async Task<bool> MakeLoanRepaymentAsync(LoanRepaymentDto repaymentDto, Guid customerId)
        {
            try
            {
                // Get loan account
                var loan = await _dbContext.LoanAccounts
                    .Include(l => l.Customer)
                    .FirstOrDefaultAsync(l => l.AccountNumber == repaymentDto.LoanAccountNumber);
                
                if (loan == null)
                {
                    throw new KeyNotFoundException($"Loan with account number {repaymentDto.LoanAccountNumber} not found.");
                }
                
                // Verify customer owns the loan
                if (loan.CustomerId != customerId)
                {
                    throw new UnauthorizedAccessException("Customer does not own this loan account.");
                }

                // Verify loan is active
                if (loan.Status == "Closed" || loan.Status == "Fully Paid")
                {
                    throw new InvalidOperationException("Cannot make payment on a closed or fully paid loan.");
                }

                // Create loan transaction
                var transaction = new LoanTransaction
                {
                    LoanAccountId = loan.Id,
                    TransactionDate = DateTime.UtcNow,
                    TransactionType = "Repayment",
                    Amount = repaymentDto.Amount,
                    Description = "Online repayment",
                    ReferenceNumber = Guid.NewGuid().ToString(),
                    PrincipalAmount = 0, // These will be calculated by the loan processing system
                    InterestAmount = 0,
                    PenaltyAmount = 0,
                    ProcessedBy = "Client Portal",
                    Status = "Pending",
                    PaymentMethod = repaymentDto.PaymentMethod,
                    SourceAccountNumber = repaymentDto.SourceAccountNumber,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.LoanTransactions.Add(transaction);
                
                // In a real implementation, this would integrate with a payment processor
                // and update the loan balance after the payment is confirmed
                
                await _dbContext.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making loan repayment for loan account {LoanAccountNumber}", repaymentDto.LoanAccountNumber);
                throw;
            }
        }

        // Interface adapter methods for IClientLoanService (wrappers returning BaseResponse<T>)
        public async Task<BaseResponse<List<ClientLoanDto>>> GetLoansAsync(Guid customerId)
        {
            try
            {
                var loans = await GetClientLoansAsync(customerId);
                var list = loans.Select(l => new ClientLoanDto
                {
                    Id = l.Id,
                    LoanAccountNumber = l.AccountNumber,
                    LoanProductName = l.LoanProduct?.Name ?? string.Empty,
                    DisbursedAmount = l.PrincipalAmount,
                    DisbursementDate = l.DisbursementDate,
                    InterestRate = l.InterestRate,
                    Tenor = l.Tenor,
                    OutstandingBalance = l.OutstandingBalance,
                    NextInstallmentAmount = 0m,
                    NextInstallmentDate = DateTime.MinValue,
                    Status = l.Status ?? string.Empty,
                    DaysPastDue = 0,
                    TotalAmountPaid = 0m
                }).ToList();

                return BaseResponse<List<ClientLoanDto>>.SuccessResponse(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetLoansAsync for customer {CustomerId}", customerId);
                return BaseResponse<List<ClientLoanDto>>.ErrorResponse("Failed to retrieve loans", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponse<ClientLoanDto>> GetLoanDetailsAsync(Guid customerId, Guid loanId)
        {
            try
            {
                var loan = await _dbContext.LoanAccounts
                    .Include(l => l.LoanProduct)
                    .Include(l => l.Customer)
                    .FirstOrDefaultAsync(l => l.Id == loanId && l.CustomerId == customerId.ToString());

                if (loan == null)
                    return BaseResponse<ClientLoanDto>.ErrorResponse("Loan not found");

                var dto = new ClientLoanDto
                {
                    Id = loan.Id,
                    LoanAccountNumber = loan.AccountNumber,
                    LoanProductName = loan.LoanProduct?.Name ?? string.Empty,
                    DisbursedAmount = loan.PrincipalAmount,
                    DisbursementDate = loan.DisbursementDate,
                    InterestRate = loan.InterestRate,
                    Tenor = loan.Tenor,
                    OutstandingBalance = loan.OutstandingBalance,
                    NextInstallmentAmount = 0m,
                    NextInstallmentDate = DateTime.MinValue,
                    Status = loan.Status ?? string.Empty,
                    DaysPastDue = 0,
                    TotalAmountPaid = 0m
                };

                return BaseResponse<ClientLoanDto>.SuccessResponse(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetLoanDetailsAsync for loan {LoanId}", loanId);
                return BaseResponse<ClientLoanDto>.ErrorResponse("Failed to retrieve loan details", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponse<List<LoanRepaymentScheduleDto>>> GetLoanRepaymentScheduleAsync(Guid customerId, Guid loanId)
        {
            try
            {
                var schedules = await _dbContext.LoanRepaymentSchedules
                    .Where(s => s.LoanAccountId == loanId)
                    .OrderBy(s => s.InstallmentNumber)
                    .ToListAsync();

                var list = schedules.Select(s => new LoanRepaymentScheduleDto
                {
                    InstallmentNumber = s.InstallmentNumber,
                    DueDate = s.DueDate,
                    PrincipalAmount = s.PrincipalAmount,
                    InterestAmount = s.InterestAmount,
                    TotalAmount = s.TotalAmount,
                    PaidAmount = s.AmountPaid ?? 0m,
                    Status = s.Status.ToString()
                }).ToList();

                return BaseResponse<List<LoanRepaymentScheduleDto>>.SuccessResponse(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving repayment schedule for loan {LoanId}", loanId);
                return BaseResponse<List<LoanRepaymentScheduleDto>>.ErrorResponse("Failed to retrieve repayment schedule", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponse<List<LoanTransactionDto>>> GetLoanTransactionsAsync(Guid customerId, Guid loanId)
        {
            try
            {
                var transactions = await _dbContext.LoanTransactions
                    .Where(t => t.LoanId == loanId.ToString())
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();

                var list = transactions.Select(t => new LoanTransactionDto
                {
                    ReferenceNumber = t.Reference,
                    TransactionDate = t.TransactionDate,
                    TransactionType = t.TransactionType,
                    Amount = (t.PrincipalAmount + t.InterestAmount),
                    Description = t.Description,
                    PrincipalAmount = t.PrincipalAmount,
                    InterestAmount = t.InterestAmount,
                    PenaltyAmount = 0m,
                    Status = t.Status
                }).ToList();

                return BaseResponse<List<LoanTransactionDto>>.SuccessResponse(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions for loan {LoanId}", loanId);
                return BaseResponse<List<LoanTransactionDto>>.ErrorResponse("Failed to retrieve loan transactions", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponse<LoanPaymentDto>> MakeLoanPaymentAsync(Guid customerId, Guid loanId, LoanPaymentRequestDto paymentRequest)
        {
            try
            {
                var loan = await _dbContext.LoanAccounts.FirstOrDefaultAsync(l => l.Id == loanId && l.CustomerId == customerId.ToString());
                if (loan == null)
                    return BaseResponse<LoanPaymentDto>.ErrorResponse("Loan not found");

                var transaction = new LoanTransaction(
                    loan.Id,
                    "REPAYMENT",
                    paymentRequest.Amount,
                    0m,
                    Guid.NewGuid().ToString(),
                    paymentRequest.Description ?? "Client payment"
                );

                _dbContext.LoanTransactions.Add(transaction);
                await _dbContext.SaveChangesAsync();

                var dto = new LoanPaymentDto
                {
                    Id = transaction.Id,
                    ReferenceNumber = transaction.Reference,
                    LoanAccountNumber = loan.AccountNumber,
                    Amount = transaction.PrincipalAmount + transaction.InterestAmount,
                    PaymentMethod = string.Empty,
                    PaymentDate = transaction.TransactionDate,
                    Status = transaction.Status,
                    PrincipalAmount = transaction.PrincipalAmount,
                    InterestAmount = transaction.InterestAmount,
                    PenaltyAmount = 0m
                };

                return BaseResponse<LoanPaymentDto>.SuccessResponse(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making loan payment for loan {LoanId}", loanId);
                return BaseResponse<LoanPaymentDto>.ErrorResponse("Failed to make loan payment", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponse<List<LoanProductDto>>> GetAvailableLoanProductsAsync(Guid customerId)
        {
            try
            {
                var products = await _dbContext.LoanProducts.ToListAsync();
                var list = products.Select(p => new LoanProductDto
                {
                    Id = p.Id.ToString(),
                    Name = p.ProductName,
                    Description = p.Description,
                    InterestRate = p.InterestRate,
                    MinAmount = p.MinimumAmount,
                    MaxAmount = p.MaximumAmount,
                    MinTerm = p.MinimumTenor,
                    MaxTerm = p.MaximumTenor,
                    IsActive = p.IsActive
                }).ToList();

                return BaseResponse<List<LoanProductDto>>.SuccessResponse(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving loan products");
                return BaseResponse<List<LoanProductDto>>.ErrorResponse("Failed to retrieve loan products", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponse<LoanApplicationDto>> SubmitLoanApplicationAsync(Guid customerId, LoanApplicationRequestDto applicationRequest)
        {
            try
            {
                // reuse existing SubmitLoanApplicationAsync by mapping request dto
                var dto = new LoanApplicationDto
                {
                    LoanProductId = applicationRequest.LoanProductId,
                    RequestedAmount = applicationRequest.RequestedAmount,
                    RequestedTenor = applicationRequest.RequestedTenor,
                    Purpose = applicationRequest.Purpose,
                    RepaymentSource = applicationRequest.RepaymentSource,
                    PreferredDisbursementDate = applicationRequest.PreferredDisbursementDate
                };

                var application = await SubmitLoanApplicationAsync(dto, customerId);

                var resultDto = new LoanApplicationDto
                {
                    LoanProductId = application.LoanProductId,
                    RequestedAmount = application.RequestedAmount,
                    RequestedTenor = application.RequestedTenor,
                    Purpose = application.Purpose,
                    RepaymentSource = application.RepaymentSource,
                    PreferredDisbursementDate = application.PreferredDisbursementDate
                };

                return BaseResponse<LoanApplicationDto>.SuccessResponse(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting loan application for customer {CustomerId}", customerId);
                return BaseResponse<LoanApplicationDto>.ErrorResponse("Failed to submit loan application", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponse<List<LoanApplicationDto>>> GetLoanApplicationsAsync(Guid customerId)
        {
            try
            {
                var apps = await GetClientLoanApplicationsAsync(customerId);
                var list = apps.Select(a => new LoanApplicationDto
                {
                    LoanProductId = a.LoanProductId,
                    RequestedAmount = a.RequestedAmount,
                    RequestedTenor = a.RequestedTenor,
                    Purpose = a.Purpose,
                    RepaymentSource = a.RepaymentSource,
                    PreferredDisbursementDate = a.PreferredDisbursementDate
                }).ToList();

                return BaseResponse<List<LoanApplicationDto>>.SuccessResponse(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving loan applications for customer {CustomerId}", customerId);
                return BaseResponse<List<LoanApplicationDto>>.ErrorResponse("Failed to retrieve loan applications", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponse<LoanApplicationDto>> GetLoanApplicationDetailsAsync(Guid customerId, Guid applicationId)
        {
            try
            {
                var application = await GetLoanApplicationStatusAsync(applicationId);
                if (application == null || application.CustomerId != customerId)
                    return BaseResponse<LoanApplicationDto>.ErrorResponse("Application not found");

                var dto = new LoanApplicationDto
                {
                    LoanProductId = application.LoanProductId,
                    RequestedAmount = application.RequestedAmount,
                    RequestedTenor = application.RequestedTenor,
                    Purpose = application.Purpose,
                    RepaymentSource = application.RepaymentSource,
                    PreferredDisbursementDate = application.PreferredDisbursementDate
                };

                return BaseResponse<LoanApplicationDto>.SuccessResponse(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving application details {ApplicationId}", applicationId);
                return BaseResponse<LoanApplicationDto>.ErrorResponse("Failed to retrieve application details", new List<string> { ex.Message });
            }
        }
    }

    public class LoanEligibility
    {
        public bool IsEligible { get; set; }
        public decimal MaximumEligibleAmount { get; set; }
        public int RecommendedTenor { get; set; }
        public List<string> EligibilityReasons { get; set; } = new List<string>();
    }

    public class LoanSimulation
    {
        public Guid LoanProductId { get; set; }
        public string LoanProductName { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int Tenor { get; set; }
        public decimal MonthlyPayment { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalPayment { get; set; }
        public List<LoanRepaymentSimulation> RepaymentSchedule { get; set; } = new List<LoanRepaymentSimulation>();
    }

    public class LoanRepaymentSimulation
    {
        public int InstallmentNumber { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal TotalPayment { get; set; }
        public decimal RemainingBalance { get; set; }
    }
}
