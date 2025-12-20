using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Application.Interfaces.Loans;
using FinTech.Core.Application.Interfaces.Repositories.Loans;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Events.Loans;
using FinTech.Core.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinTech.Core.Application.Services.Loans
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILoanTransactionRepository _loanTransactionRepository;
        private readonly ILoanRepaymentScheduleRepository _loanRepaymentScheduleRepository;
        private readonly ILogger<LoanService> _logger;
        private readonly IMapper _mapper;

        public LoanService(
            ILoanRepository loanRepository,
            ILoanTransactionRepository loanTransactionRepository,
            ILoanRepaymentScheduleRepository loanRepaymentScheduleRepository,
            ILogger<LoanService> logger,
            IMapper mapper)
        {
            _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
            _loanTransactionRepository = loanTransactionRepository ?? throw new ArgumentNullException(nameof(loanTransactionRepository));
            _loanRepaymentScheduleRepository = loanRepaymentScheduleRepository ?? throw new ArgumentNullException(nameof(loanRepaymentScheduleRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<Loan>> GetAllLoansAsync()
        {
            try
            {
                _logger.LogInformation("Getting all loans");
                return await _loanRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all loans");
                throw;
            }
        }

        public async Task<Loan> GetLoanByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("Getting loan with ID: {Id}", id);
                var loan = await _loanRepository.GetByIdAsync(id);
                
                if (loan == null)
                {
                    _logger.LogWarning("Loan with ID: {Id} not found", id);
                }
                
                return loan;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Loan>> GetLoansByCustomerIdAsync(string customerId)
        {
            try
            {
                _logger.LogInformation("Getting loans for customer ID: {CustomerId}", customerId);
                return await _loanRepository.GetByCustomerIdAsync(customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loans for customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<Loan> DisburseLoanAsync(string id, decimal amount, string disbursedTo, string reference, string description)
        {
            try
            {
                _logger.LogInformation("Disbursing loan with ID: {Id}, Amount: {Amount}", id, amount);
                
                // Get loan
                var loan = await _loanRepository.GetByIdAsync(id);
                if (loan == null)
                {
                    throw new ArgumentException($"Loan with ID: {id} not found");
                }
                
                // Validate loan status
                if (loan.Status != LoanStatus.Approved.ToString().ToUpper())
                {
                    throw new InvalidOperationException("Loan must be in Approved status to disburse");
                }
                
                // Validate amount
                if (amount <= 0)
                {
                    throw new ArgumentException("Disbursement amount must be greater than zero");
                }
                
                if (amount > loan.PrincipalAmount)
                {
                    throw new ArgumentException($"Disbursement amount cannot exceed principal amount of {loan.PrincipalAmount}");
                }
                
                // Update loan
                loan.DisbursedAmount += amount;
                
                if (loan.DisbursedAmount >= loan.PrincipalAmount)
                {
                    loan.Status = LoanStatus.Active.ToString().ToUpper();
                    loan.DisbursementDate = DateTime.UtcNow;
                    loan.MaturityDate = loan.DisbursementDate.AddMonths(loan.TenureMonths);
                }
                
                // Create transaction
                var transaction = new LoanTransaction
                {
                    LoanId = loan.Id,
                    TransactionType = "DISBURSEMENT",
                    PrincipalAmount = amount,
                    InterestAmount = 0,
                    FeesAmount = 0,
                    PenaltyAmount = 0,
                    TotalAmount = amount,
                    Reference = reference,
                    Description = description,
                    TransactionDate = DateTime.UtcNow,
                    Status = "COMPLETED"
                };
                
                // Add transaction
                await _loanTransactionRepository.AddAsync(transaction);
                
                // Domain event removed (not supported by BaseEntity)
                /*
                loan.AddDomainEvent(new LoanDisbursedEvent(
                    int.Parse(loan.Id), 
                    amount, 
                    reference, 
                    description));
                */
                
                // Update loan
                await _loanRepository.UpdateAsync(loan);
                
                _logger.LogInformation("Loan disbursed with ID: {Id}, Amount: {Amount}", id, amount);
                
                return loan;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disbursing loan with ID: {Id}", id);
                throw;
            }
        }

        public async Task<LoanTransaction> RecordRepaymentAsync(
            string loanId, 
            decimal amount, 
            decimal principalAmount, 
            decimal interestAmount, 
            decimal feesAmount, 
            decimal penaltyAmount, 
            string reference, 
            string description)
        {
            try
            {
                _logger.LogInformation("Recording repayment for loan ID: {LoanId}, Amount: {Amount}", loanId, amount);
                
                // Validate amounts
                if (amount <= 0)
                {
                    throw new ArgumentException("Repayment amount must be greater than zero");
                }
                
                if (principalAmount + interestAmount + feesAmount + penaltyAmount != amount)
                {
                    throw new ArgumentException("Sum of principal, interest, fees, and penalty amounts must equal total amount");
                }
                
                // Get loan
                var loan = await _loanRepository.GetByIdAsync(loanId);
                if (loan == null)
                {
                    throw new ArgumentException($"Loan with ID: {loanId} not found");
                }
                
                // Validate loan status
                // Validate loan status
                if (loan.Status != LoanStatus.Active.ToString().ToUpper() && loan.Status != "PAST_DUE") // LoanStatus.PastDue missing in some enums, using string fallback
                {
                    throw new InvalidOperationException("Loan must be in Active or Past Due status to record repayment");
                }
                
                // Create transaction
                var transaction = new LoanTransaction
                {
                    LoanId = loanId,
                    TransactionType = "REPAYMENT",
                    PrincipalAmount = principalAmount,
                    InterestAmount = interestAmount,
                    FeesAmount = feesAmount,
                    PenaltyAmount = penaltyAmount,
                    TotalAmount = amount,
                    Reference = reference,
                    Description = description,
                    TransactionDate = DateTime.UtcNow,
                    Status = "COMPLETED"
                };
                
                // Add transaction
                var addedTransaction = await _loanTransactionRepository.AddAsync(transaction);
                
                // Update loan repayment schedules
                await UpdateLoanSchedulesAsync(loanId, principalAmount, interestAmount, feesAmount, penaltyAmount);
                
                // Check if loan is fully paid
                var schedules = await _loanRepaymentScheduleRepository.GetByLoanIdAsync(loanId);
                var allPaid = schedules.All(s => s.Status == RepaymentStatus.Paid);
                if (allPaid)
                {
                    loan.Status = LoanStatus.Closed.ToString().ToUpper();
                    await _loanRepository.UpdateAsync(loan);
                }
                
                // Domain event removed
                /*
                loan.AddDomainEvent(new LoanRepaymentReceivedEvent(
                    int.Parse(loanId), 
                    principalAmount, 
                    interestAmount, 
                    reference, 
                    description));
                */
                
                _logger.LogInformation("Repayment recorded for loan ID: {LoanId}, Amount: {Amount}", loanId, amount);
                
                return addedTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording repayment for loan ID: {LoanId}", loanId);
                throw;
            }
        }

        public async Task<LoanTransaction> WriteOffLoanAsync(string id, string reason, string approvedBy)
        {
            try
            {
                _logger.LogInformation("Writing off loan with ID: {Id}", id);
                
                // Get loan
                var loan = await _loanRepository.GetByIdAsync(id);
                if (loan == null)
                {
                    throw new ArgumentException($"Loan with ID: {id} not found");
                }
                
                // Validate loan status
                // Validate loan status
                if (loan.Status != LoanStatus.Active.ToString().ToUpper() && loan.Status != "PAST_DUE")
                {
                    throw new InvalidOperationException("Loan must be in Active or Past Due status to be written off");
                }
                
                // Calculate outstanding amounts
                decimal outstandingPrincipal = 0;
                decimal outstandingInterest = 0;
                decimal outstandingFees = 0;
                decimal outstandingPenalties = 0;
                
                var schedules = await _loanRepaymentScheduleRepository.GetByLoanIdAsync(id);
                foreach (var schedule in schedules.Where(s => s.Status != RepaymentStatus.Paid))
                {
                    outstandingPrincipal += schedule.PrincipalAmount - schedule.PaidPrincipal;
                    outstandingInterest += schedule.InterestAmount - schedule.PaidInterest;
                    outstandingFees += schedule.FeesAmount - schedule.PaidFees;
                    outstandingPenalties += (schedule.PenaltyAmount ?? 0m) - schedule.PaidPenalty;
                }
                
                decimal totalWriteOff = outstandingPrincipal + outstandingInterest + outstandingFees + outstandingPenalties;
                
                // Create transaction
                var transaction = new LoanTransaction
                {
                    LoanId = id,
                    TransactionType = "WRITE_OFF",
                    PrincipalAmount = outstandingPrincipal,
                    InterestAmount = outstandingInterest,
                    FeesAmount = outstandingFees,
                    PenaltyAmount = outstandingPenalties,
                    TotalAmount = totalWriteOff,
                    Reference = $"Write-off by {approvedBy}",
                    Description = reason,
                    TransactionDate = DateTime.UtcNow,
                    Status = "COMPLETED"
                };
                
                // Add transaction
                var addedTransaction = await _loanTransactionRepository.AddAsync(transaction);
                
                // Mark all schedules as paid
                foreach (var schedule in schedules.Where(s => s.Status != RepaymentStatus.Paid))
                {
                    schedule.Status = RepaymentStatus.WrittenOff;
                    await _loanRepaymentScheduleRepository.UpdateAsync(schedule);
                }
                
                // Update loan status
                // Update loan status
                loan.Status = LoanStatus.WrittenOff.ToString().ToUpper();
                await _loanRepository.UpdateAsync(loan);
                
                // Raise domain event
                /*
                loan.AddDomainEvent(new LoanWrittenOffEvent(
                    int.Parse(id), 
                    totalWriteOff, 
                    reason, 
                    reason));
                */
                
                _logger.LogInformation("Loan written off with ID: {Id}, Amount: {Amount}", id, totalWriteOff);
                
                return addedTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing off loan with ID: {Id}", id);
                throw;
            }
        }

        public async Task<Loan> RescheduleLoanAsync(string id, DateTime newEndDate, string reason, string approvedBy)
        {
            try
            {
                _logger.LogInformation("Rescheduling loan with ID: {Id}, New End Date: {NewEndDate}", id, newEndDate);
                
                // Get loan
                var loan = await _loanRepository.GetByIdAsync(id);
                if (loan == null)
                {
                    throw new ArgumentException($"Loan with ID: {id} not found");
                }
                
                // Validate loan status
                // Validate loan status
                if (loan.Status != LoanStatus.Active.ToString().ToUpper() && loan.Status != "PAST_DUE")
                {
                    throw new InvalidOperationException("Loan must be in Active or Past Due status to be rescheduled");
                }
                
                // Validate new end date
                if (newEndDate <= DateTime.UtcNow)
                {
                    throw new ArgumentException("New end date must be in the future");
                }
                
                // FinTech Best Practice: MaturityDate is non-nullable
                DateTime originalEndDate = loan.MaturityDate;
                
                // Update loan
                // Update loan
                loan.MaturityDate = newEndDate;
                loan.TenureMonths = (int)Math.Ceiling((newEndDate - loan.DisbursementDate).TotalDays / 30.0); // Approximate months
                
                // Regenerate repayment schedule
                // This is a simplified approach - in a real system, you would need more complex rescheduling logic
                await RegenerateRepaymentScheduleAsync(loan);
                
                // Raise domain event
                /*
                loan.AddDomainEvent(new LoanRescheduledEvent(
                    id, 
                    originalEndDate, 
                    newEndDate, 
                    reason, 
                    approvedBy));
                */
                
                // Update loan
                await _loanRepository.UpdateAsync(loan);
                
                _logger.LogInformation("Loan rescheduled with ID: {Id}, New End Date: {NewEndDate}", id, newEndDate);
                
                return loan;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rescheduling loan with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<LoanTransaction>> GetLoanTransactionsAsync(string loanId)
        {
            try
            {
                _logger.LogInformation("Getting transactions for loan ID: {LoanId}", loanId);
                return await _loanTransactionRepository.GetByLoanIdAsync(loanId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions for loan ID: {LoanId}", loanId);
                throw;
            }
        }

        public async Task<IEnumerable<LoanRepaymentSchedule>> GetLoanRepaymentScheduleAsync(string loanId)
        {
            try
            {
                _logger.LogInformation("Getting repayment schedule for loan ID: {LoanId}", loanId);
                return await _loanRepaymentScheduleRepository.GetByLoanIdAsync(loanId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting repayment schedule for loan ID: {LoanId}", loanId);
                throw;
            }
        }

        public async Task<LoanStatement> GenerateLoanStatementAsync(string loanId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                _logger.LogInformation("Generating loan statement for loan ID: {LoanId}, From: {FromDate}, To: {ToDate}", 
                    loanId, fromDate, toDate);
                
                // Get loan
                var loan = await _loanRepository.GetByIdAsync(loanId);
                if (loan == null)
                {
                    throw new ArgumentException($"Loan with ID: {loanId} not found");
                }
                
                // Get transactions
                var allTransactions = await _loanTransactionRepository.GetByLoanIdAsync(loanId);
                var transactions = allTransactions
                    .Where(t => t.TransactionDate >= fromDate && t.TransactionDate <= toDate)
                    .OrderBy(t => t.TransactionDate)
                    .ToList();
                
                // Get repayment schedule
                var schedules = await _loanRepaymentScheduleRepository.GetByLoanIdAsync(loanId);
                
                // Calculate opening balance
                decimal openingBalance = CalculateBalanceAsOf(allTransactions, fromDate);
                
                // Calculate closing balance
                decimal closingBalance = CalculateBalanceAsOf(allTransactions, toDate);
                
                // Calculate paid amounts
                decimal principalPaid = transactions
                    .Where(t => t.TransactionType == "REPAYMENT")
                    .Sum(t => t.PrincipalAmount);
                
                decimal interestPaid = transactions
                    .Where(t => t.TransactionType == "REPAYMENT")
                    .Sum(t => t.InterestAmount);
                
                decimal feesPaid = transactions
                    .Where(t => t.TransactionType == "REPAYMENT")
                    .Sum(t => t.FeesAmount);
                
                decimal penaltiesPaid = transactions
                    .Where(t => t.TransactionType == "REPAYMENT")
                    .Sum(t => t.PenaltyAmount);
                
                // Calculate outstanding amounts
                decimal principalOutstanding = schedules
                    .Where(s => s.Status != RepaymentStatus.Paid && s.Status != RepaymentStatus.WrittenOff)
                    .Sum(s => s.PrincipalAmount - s.PaidPrincipal);
                
                decimal interestOutstanding = schedules
                    .Where(s => s.Status != RepaymentStatus.Paid && s.Status != RepaymentStatus.WrittenOff)
                    .Sum(s => s.InterestAmount - s.PaidInterest);
                
                decimal feesOutstanding = schedules
                    .Where(s => s.Status != RepaymentStatus.Paid && s.Status != RepaymentStatus.WrittenOff)
                    .Sum(s => s.FeesAmount - s.PaidFees);
                
                decimal penaltiesOutstanding = schedules
                    .Where(s => s.Status != RepaymentStatus.Paid && s.Status != RepaymentStatus.WrittenOff)
                    .Sum(s => ((decimal?)s.PenaltyAmount ?? 0m) - ((decimal?)s.PaidPenalty ?? 0m));
                
                // Map transactions to statement transactions
                var statementTransactions = new List<LoanStatementTransaction>();
                decimal runningBalance = openingBalance;
                
                foreach (var transaction in transactions)
                {
                    decimal transactionAmount = 0;
                    
                    if (transaction.TransactionType == "DISBURSEMENT")
                    {
                        transactionAmount = transaction.TotalAmount;
                        runningBalance += transactionAmount;
                    }
                    else if (transaction.TransactionType == "REPAYMENT")
                    {
                        transactionAmount = -transaction.TotalAmount;
                        runningBalance += transactionAmount;
                    }
                    else if (transaction.TransactionType == "WRITE_OFF")
                    {
                        transactionAmount = -transaction.TotalAmount;
                        runningBalance = 0;
                    }
                    
                    var statementTransaction = new LoanStatementTransaction
                    {
                        TransactionDate = transaction.TransactionDate,
                        TransactionType = transaction.TransactionType,
                        Description = transaction.Description,
                        Reference = transaction.Reference,
                        PrincipalAmount = transaction.PrincipalAmount,
                        InterestAmount = transaction.InterestAmount,
                        FeesAmount = transaction.FeesAmount,
                        PenaltyAmount = transaction.PenaltyAmount,
                        TotalAmount = transaction.TotalAmount,
                        RunningBalance = runningBalance
                    };
                    
                    statementTransactions.Add(statementTransaction);
                }
                
                // Map repayment schedules
                var repaymentScheduleDtos = _mapper.Map<List<LoanRepaymentScheduleDto>>(schedules);
                
                // Create statement
                var statement = new LoanStatement
                {
                    LoanId = loanId,
                    LoanNumber = loan.LoanNumber,
                    CustomerName = "Unknown", // Would be populated from customer service
                    CustomerAccountNumber = "Unknown", // Would be populated from customer service
                    StatementFromDate = fromDate,
                    StatementToDate = toDate,
                    StatementGenerationDate = DateTime.UtcNow,
                    PrincipalAmount = loan.PrincipalAmount,
                    DisbursedAmount = loan.DisbursedAmount,
                    InterestRate = loan.InterestRate,
                    // FinTech Best Practice: Removed duplicate InterestRate assignment
                    Term = loan.TenureMonths,
                    DisbursementDate = loan.DisbursementDate,
                    // FinTech Best Practice: MaturityDate is non-nullable, no need for ?? operator
                    MaturityDate = loan.MaturityDate,
                    OpeningBalance = openingBalance,
                    ClosingBalance = closingBalance,
                    TotalDebits = transactions.Where(t => t.TransactionType == "DISBURSEMENT").Sum(t => t.TotalAmount),
                    TotalCredits = transactions.Where(t => t.TransactionType == "REPAYMENT").Sum(t => t.TotalAmount),
                    PrincipalPaid = principalPaid,
                    InterestPaid = interestPaid,
                    FeesPaid = feesPaid,
                    PenaltiesPaid = penaltiesPaid,
                    PrincipalOutstanding = principalOutstanding,
                    InterestOutstanding = interestOutstanding,
                    FeesOutstanding = feesOutstanding,
                    PenaltiesOutstanding = penaltiesOutstanding,
                    Transactions = statementTransactions,
                    RepaymentSchedule = repaymentScheduleDtos
                };
                
                _logger.LogInformation("Loan statement generated for loan ID: {LoanId}", loanId);
                
                return statement;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating loan statement for loan ID: {LoanId}", loanId);
                throw;
            }
        }

        private decimal CalculateBalanceAsOf(IEnumerable<LoanTransaction> transactions, DateTime asOfDate)
        {
            decimal balance = 0;
            
            foreach (var transaction in transactions.Where(t => t.TransactionDate < asOfDate).OrderBy(t => t.TransactionDate))
            {
                if (transaction.TransactionType == "DISBURSEMENT")
                {
                    balance += transaction.TotalAmount;
                }
                else if (transaction.TransactionType == "REPAYMENT")
                {
                    balance -= transaction.TotalAmount;
                }
                else if (transaction.TransactionType == "WRITE_OFF")
                {
                    balance = 0;
                }
            }
            
            return balance;
        }

        private async Task UpdateLoanSchedulesAsync(
            string loanId, 
            decimal principalAmount, 
            decimal interestAmount, 
            decimal feesAmount, 
            decimal penaltyAmount)
        {
            // Get repayment schedules
            var schedules = await _loanRepaymentScheduleRepository.GetByLoanIdAsync(loanId);
            var unpaidSchedules = schedules
                .Where(s => s.Status != RepaymentStatus.Paid)
                .OrderBy(s => s.DueDate)
                .ToList();
            
            decimal remainingPrincipal = principalAmount;
            decimal remainingInterest = interestAmount;
            decimal remainingFees = feesAmount;
            decimal remainingPenalty = penaltyAmount;
            
            // Apply payment to schedules
            foreach (var schedule in unpaidSchedules)
            {
                // Apply penalty payment
                decimal pendingPenalty = ((decimal?)schedule.PenaltyAmount ?? 0m) - ((decimal?)schedule.PaidPenalty ?? 0m);
                decimal penaltyPayment = Math.Min(remainingPenalty, pendingPenalty);
                schedule.PaidPenalty = schedule.PaidPenalty + penaltyPayment;
                remainingPenalty -= penaltyPayment;
                
                // Apply fees payment
                decimal pendingFees = schedule.FeesAmount - schedule.PaidFees;
                decimal feesPayment = Math.Min(remainingFees, pendingFees);
                schedule.PaidFees += feesPayment;
                remainingFees -= feesPayment;
                
                // Apply interest payment
                decimal pendingInterest = schedule.InterestAmount - schedule.PaidInterest;
                decimal interestPayment = Math.Min(remainingInterest, pendingInterest);
                schedule.PaidInterest += interestPayment;
                remainingInterest -= interestPayment;
                
                // Apply principal payment
                decimal pendingPrincipal = schedule.PrincipalAmount - schedule.PaidPrincipal;
                decimal principalPayment = Math.Min(remainingPrincipal, pendingPrincipal);
                schedule.PaidPrincipal += principalPayment;
                remainingPrincipal -= principalPayment;
                
                // Calculate total paid
                schedule.PaidAmount = schedule.PaidPrincipal + schedule.PaidInterest + schedule.PaidFees + schedule.PaidPenalty;
                
                // Update status
                if (schedule.PaidAmount >= schedule.TotalAmount)
                {
                    schedule.Status = RepaymentStatus.Paid;
                    schedule.PaymentDate = DateTime.UtcNow;
                }
                else if (schedule.PaidAmount > 0)
                {
                    schedule.Status = RepaymentStatus.PartiallyPaid;
                }
                
                // Update schedule
                await _loanRepaymentScheduleRepository.UpdateAsync(schedule);
                
                // If no more money to allocate, break
                if (remainingPrincipal <= 0 && remainingInterest <= 0 && remainingFees <= 0 && remainingPenalty <= 0)
                {
                    break;
                }
            }
        }

        private async Task RegenerateRepaymentScheduleAsync(Loan loan)
        {
            // Get existing schedules
            var existingSchedules = await _loanRepaymentScheduleRepository.GetByLoanIdAsync(loan.Id);
            
            // Delete future unpaid schedules
            foreach (var schedule in existingSchedules.Where(s => 
                s.Status != RepaymentStatus.Paid && 
                s.Status != RepaymentStatus.PartiallyPaid && 
                s.DueDate > DateTime.UtcNow))
            {
                await _loanRepaymentScheduleRepository.DeleteAsync(schedule.Id);
            }
            
            // Get remaining schedules after deletion
            var remainingSchedules = await _loanRepaymentScheduleRepository.GetByLoanIdAsync(loan.Id);
            
            // Calculate remaining principal
            decimal paidPrincipal = remainingSchedules.Sum(s => s.PaidPrincipal);
            decimal remainingPrincipal = loan.PrincipalAmount - paidPrincipal;
            
            // Calculate new installment count
            int remainingMonths = (int)Math.Ceiling((loan.MaturityDate - DateTime.UtcNow).TotalDays / 30.0); // FinTech Best Practice: MaturityDate is DateTime not DateTime?
            
            // Calculate installment amount
            decimal monthlyInterestRate = loan.InterestRate / 12 / 100;
            decimal installmentAmount = remainingPrincipal * monthlyInterestRate * 
                (decimal)Math.Pow((double)(1 + monthlyInterestRate), remainingMonths) / 
                ((decimal)Math.Pow((double)(1 + monthlyInterestRate), remainingMonths) - 1);
            
            // Create new schedules
            int lastInstallmentNumber = remainingSchedules.Any() ? 
                remainingSchedules.Max(s => s.InstallmentNumber) : 0;
            
            DateTime nextDueDate = remainingSchedules.Any() ? 
                remainingSchedules.Max(s => s.DueDate).AddMonths(1) : 
                DateTime.UtcNow.Date.AddMonths(1);
            
            decimal outstandingPrincipal = remainingPrincipal;
            
            for (int i = 1; i <= remainingMonths; i++)
            {
                decimal interestPayment = outstandingPrincipal * monthlyInterestRate;
                decimal principalPayment = Math.Min(installmentAmount - interestPayment, outstandingPrincipal);
                
                // For the last payment, ensure we pay all remaining principal
                if (i == remainingMonths)
                {
                    principalPayment = outstandingPrincipal;
                }
                
                var schedule = new LoanRepaymentSchedule
                {
                    LoanId = loan.Id,
                    InstallmentNumber = lastInstallmentNumber + i,
                    DueDate = nextDueDate.AddMonths(i - 1),
                    PrincipalAmount = principalPayment,
                    InterestAmount = interestPayment,
                    FeesAmount = 0, // No fees for rescheduled payments
                    PenaltyAmount = 0, // No penalties for rescheduled payments
                    TotalAmount = principalPayment + interestPayment,
                    PaidAmount = 0,
                    PaidPrincipal = 0,
                    PaidInterest = 0,
                    PaidFees = 0,
                    PaidPenalty = 0,
                    Status = RepaymentStatus.Pending
                };
                
                await _loanRepaymentScheduleRepository.AddAsync(schedule);
                
                outstandingPrincipal -= principalPayment;
            }
        }

        // Additional methods from FinTech.Core.Application.Interfaces.Loans.ILoanService interface
        async Task<FinTech.Core.Domain.Entities.Loans.LoanAccount> FinTech.Core.Application.Interfaces.Loans.ILoanService.CreateLoanAccountAsync(FinTech.Core.Application.DTOs.Loans.CreateLoanAccountRequest request)
        {
           // Stub implementation to satisfy interface
           await Task.CompletedTask;
           throw new NotImplementedException("Use appropriate loan service implementation. This is a stub for accounting integration.");
        }

        async Task<bool> FinTech.Core.Application.Interfaces.Loans.ILoanService.DisburseLoanAsync(Guid loanAccountId, decimal amount, string disbursedBy)
        {
            // Convert to string-based call
            var loan = await DisburseLoanAsync(loanAccountId.ToString(), amount, disbursedBy, $"DISB-{DateTime.UtcNow:yyyyMMddHHmmss}", "Loan disbursement");
            return loan != null;
        }

        async Task<bool> FinTech.Core.Application.Interfaces.Loans.ILoanService.ProcessRepaymentAsync(Guid loanAccountId, decimal amount, string processedBy)
        {
            var transaction = await RecordRepaymentAsync(loanAccountId.ToString(), amount, amount, 0, 0, 0, $"REP-{DateTime.UtcNow:yyyyMMddHHmmss}", "Loan repayment");
            return transaction != null;
        }

        async Task<List<LoanRepaymentSchedule>> FinTech.Core.Application.Interfaces.Loans.ILoanService.GenerateRepaymentScheduleAsync(Guid loanAccountId)
        {
            var schedules = await GetLoanRepaymentScheduleAsync(loanAccountId.ToString());
            return schedules.ToList();
        }

        async Task<bool> FinTech.Core.Application.Interfaces.Loans.ILoanService.ClassifyLoansAsync(Guid tenantId)
        {
            // Stub implementation
            _logger.LogWarning("ClassifyLoansAsync called but not fully implemented for tenant {TenantId}", tenantId);
            return await Task.FromResult(true);
        }

        async Task<decimal> FinTech.Core.Application.Interfaces.Loans.ILoanService.CalculateProvisionAsync(Guid loanAccountId)
        {
            // Stub implementation - calculate 5% provision
            var loan = await GetLoanByIdAsync(loanAccountId.ToString());
            if (loan == null) return 0;
            return loan.OutstandingBalance * 0.05m;
        }

        async Task<IEnumerable<LoanCollateral>> FinTech.Core.Application.Interfaces.Loans.ILoanService.GetLoanCollateralsAsync(string loanId)
        {
            // Stub implementation
            return await Task.FromResult(new List<LoanCollateral>());
        }

        async Task<LoanCollateral> FinTech.Core.Application.Interfaces.Loans.ILoanService.AddLoanCollateralAsync(string loanId, FinTech.Core.Application.DTOs.Loans.CreateLoanCollateralDto collateralDto)
        {
            // Stub implementation
            throw new NotImplementedException();
        }
    }
}
