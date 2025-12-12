using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Application.Interfaces.Loans;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FinTech.Core.Application.Services.Loans
{
    /// <summary>
    /// Service for processing loan repayments using reducing balance method
    /// </summary>
    public class LoanRepaymentService : ILoanRepaymentService
    {
        private readonly IRepository<Loan> _loanRepository;
        private readonly IRepository<LoanTransaction> _transactionRepository;
        private readonly IRepository<LoanRepaymentSchedule> _scheduleRepository;
        private readonly IRepository<Member> _memberRepository;
        private readonly ILoanCalculatorService _calculatorService;
        private readonly ILoanRegisterService _registerService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LoanRepaymentService> _logger;

        public LoanRepaymentService(
            IRepository<Loan> loanRepository,
            IRepository<LoanTransaction> transactionRepository,
            IRepository<LoanRepaymentSchedule> scheduleRepository,
            IRepository<Member> memberRepository,
            ILoanCalculatorService calculatorService,
            ILoanRegisterService registerService,
            IUnitOfWork unitOfWork,
            ILogger<LoanRepaymentService> logger)
        {
            _loanRepository = loanRepository;
            _transactionRepository = transactionRepository;
            _scheduleRepository = scheduleRepository;
            _memberRepository = memberRepository;
            _calculatorService = calculatorService;
            _registerService = registerService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Processes a loan repayment with interest-first allocation
        /// </summary>
        public async Task<RepaymentResult> ProcessRepaymentAsync(RepaymentRequest request)
        {
            _logger.LogInformation(
                "Processing repayment of ₦{Amount:N2} for loan {LoanId}",
                request.Amount, request.LoanId);

            var loan = await _loanRepository.GetByIdAsync(request.LoanId);
            if (loan == null)
                throw new InvalidOperationException("Loan not found");

            if (loan.LoanStatus != "ACTIVE" && loan.LoanStatus != "DISBURSED")
                throw new InvalidOperationException("Loan is not active");

            // Calculate accrued interest
            var accruedInterest = CalculateAccruedInterest(loan);

            // Allocate payment (penalty -> interest -> principal)
            var allocation = _calculatorService.AllocatePayment(
                request.Amount,
                loan.OutstandingBalance - (loan.TotalRepayableAmount - loan.PrincipalAmount - loan.InterestPaid),
                accruedInterest,
                loan.PenaltyAmount);

            // Create transaction record
            var transaction = new LoanTransaction
            {
                Id = Guid.NewGuid().ToString(),
                LoanId = loan.Id,
                TransactionType = "REPAYMENT",
                Amount = request.Amount,
                PrincipalAmount = allocation.PrincipalPaid,
                InterestAmount = allocation.InterestPaid,
                PenaltyAmount = allocation.PenaltyPaid,
                TransactionDate = DateTime.UtcNow,
                PaymentMethod = request.PaymentMethod,
                TransactionReference = request.TransactionReference,
                Status = "COMPLETED",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.ProcessedBy
            };

            await _transactionRepository.AddAsync(transaction);

            // Update loan balances
            loan.PrincipalPaid += allocation.PrincipalPaid;
            loan.InterestPaid += allocation.InterestPaid;
            loan.PenaltyAmount = Math.Max(0, loan.PenaltyAmount - allocation.PenaltyPaid);
            
            // Recalculate outstanding balance using reducing balance method
            loan.OutstandingBalance = loan.TotalRepayableAmount - loan.PrincipalPaid - loan.InterestPaid;
            
            loan.LastPaymentDate = DateTime.UtcNow;
            loan.UpdatedAt = DateTime.UtcNow;

            // Update repayment schedule
            await UpdateRepaymentScheduleAsync(loan.Id, allocation);

            // Check if loan is fully paid
            if (loan.OutstandingBalance <= 0.01m) // Allow for rounding
            {
                loan.OutstandingBalance = 0;
                loan.LoanStatus = "CLOSED";
                loan.Classification = "CLOSED";
                
                _logger.LogInformation("Loan {LoanId} fully repaid and closed", loan.Id);
            }
            else
            {
                // Update next payment date
                loan.NextPaymentDate = CalculateNextPaymentDate(loan);
                
                // Check for arrears
                if (loan.NextPaymentDate < DateTime.UtcNow)
                {
                    loan.DaysInArrears = (DateTime.UtcNow - loan.NextPaymentDate.Value).Days;
                    loan.ArrearsAmount = CalculateArrearsAmount(loan);
                }
                else
                {
                    loan.DaysInArrears = 0;
                    loan.ArrearsAmount = 0;
                }
            }

            await _loanRepository.UpdateAsync(loan);

            // Update member outstanding balance
            var member = await _memberRepository.GetByIdAsync(loan.MemberId);
            if (member != null)
            {
                member.OutstandingLoanBalance = Math.Max(0, member.OutstandingLoanBalance - request.Amount);
                member.UpdatedAt = DateTime.UtcNow;
                await _memberRepository.UpdateAsync(member);
            }

            // Add entry to loan register
            await _registerService.AddRegisterEntryAsync(
                loan.Id,
                "REPAYMENT",
                $"Repayment of ₦{request.Amount:N2}",
                request.Amount,
                request.TransactionReference,
                request.ProcessedBy);

            await _unitOfWork.SaveChangesAsync();

            // Generate receipt
            var receipt = await GenerateRepaymentReceiptAsync(loan, transaction, allocation);

            _logger.LogInformation(
                "Repayment processed successfully for loan {LoanId}. Transaction ID: {TransactionId}",
                loan.Id, transaction.Id);

            return new RepaymentResult
            {
                Success = true,
                TransactionId = transaction.Id,
                LoanId = loan.Id,
                AmountPaid = request.Amount,
                PrincipalPaid = allocation.PrincipalPaid,
                InterestPaid = allocation.InterestPaid,
                PenaltyPaid = allocation.PenaltyPaid,
                RemainingBalance = loan.OutstandingBalance,
                NextPaymentDate = loan.NextPaymentDate,
                NextPaymentAmount = loan.MonthlyInstallment,
                ReceiptUrl = receipt.ReceiptUrl,
                IsLoanFullyPaid = loan.LoanStatus == "CLOSED",
                Message = loan.LoanStatus == "CLOSED" 
                    ? "Loan fully repaid and closed" 
                    : "Repayment processed successfully"
            };
        }

        /// <summary>
        /// Processes partial payment
        /// </summary>
        public async Task<RepaymentResult> ProcessPartialPaymentAsync(RepaymentRequest request)
        {
            _logger.LogInformation(
                "Processing partial payment of ₦{Amount:N2} for loan {LoanId}",
                request.Amount, request.LoanId);

            // Partial payments use the same logic as full payments
            // The allocation algorithm handles any amount
            return await ProcessRepaymentAsync(request);
        }

        /// <summary>
        /// Updates amortization schedule after payment
        /// </summary>
        public async Task UpdateRepaymentScheduleAsync(string loanId, PaymentAllocation allocation)
        {
            var schedules = (await _scheduleRepository.GetAllAsync())
                .Where(s => s.LoanId == loanId && s.Status == "PENDING")
                .OrderBy(s => s.DueDate)
                .ToList();

            if (!schedules.Any())
                return;

            decimal remainingPayment = allocation.PrincipalPaid + allocation.InterestPaid;

            foreach (var schedule in schedules)
            {
                if (remainingPayment <= 0)
                    break;

                decimal scheduledAmount = schedule.PrincipalAmount + schedule.InterestAmount;
                
                if (remainingPayment >= scheduledAmount)
                {
                    // Full installment paid
                    schedule.Status = "PAID";
                    schedule.PaidDate = DateTime.UtcNow;
                    schedule.AmountPaid = scheduledAmount;
                    remainingPayment -= scheduledAmount;
                }
                else
                {
                    // Partial installment paid
                    schedule.AmountPaid = (schedule.AmountPaid ?? 0) + remainingPayment;
                    remainingPayment = 0;
                }

                schedule.UpdatedAt = DateTime.UtcNow;
                await _scheduleRepository.UpdateAsync(schedule);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Generates repayment receipt
        /// </summary>
        public async Task<RepaymentReceipt> GenerateRepaymentReceiptAsync(
            Loan loan,
            LoanTransaction transaction,
            PaymentAllocation allocation)
        {
            _logger.LogInformation("Generating repayment receipt for transaction {TransactionId}", transaction.Id);

            var member = await _memberRepository.GetByIdAsync(loan.MemberId);

            var receipt = new RepaymentReceipt
            {
                ReceiptNumber = $"RCP{DateTime.UtcNow:yyyyMMddHHmmss}",
                TransactionId = transaction.Id,
                LoanSerialNumber = loan.LoanNumber,
                MemberName = member != null ? $"{member.FirstName} {member.LastName}" : "Unknown",
                MemberNumber = member?.MemberNumber,
                PaymentDate = transaction.TransactionDate,
                AmountPaid = transaction.Amount,
                PrincipalPaid = allocation.PrincipalPaid,
                InterestPaid = allocation.InterestPaid,
                PenaltyPaid = allocation.PenaltyPaid,
                RemainingBalance = loan.OutstandingBalance,
                PaymentMethod = transaction.PaymentMethod,
                TransactionReference = transaction.TransactionReference,
                ReceiptUrl = $"/receipts/{transaction.Id}.pdf",
                GeneratedAt = DateTime.UtcNow
            };

            return receipt;
        }

        /// <summary>
        /// Gets repayment history for a loan
        /// </summary>
        public async Task<RepaymentHistory> GetLoanRepaymentHistoryAsync(string loanId)
        {
            var loan = await _loanRepository.GetByIdAsync(loanId);
            if (loan == null)
                throw new InvalidOperationException("Loan not found");

            var transactions = (await _transactionRepository.GetAllAsync())
                .Where(t => t.LoanId == loanId && t.TransactionType == "REPAYMENT")
                .OrderByDescending(t => t.TransactionDate)
                .ToList();

            var history = new RepaymentHistory
            {
                LoanId = loanId,
                LoanSerialNumber = loan.LoanNumber,
                TotalRepayments = transactions.Count,
                TotalAmountPaid = transactions.Sum(t => t.Amount),
                TotalPrincipalPaid = loan.PrincipalPaid,
                TotalInterestPaid = loan.InterestPaid,
                RemainingBalance = loan.OutstandingBalance,
                Transactions = transactions.Select(t => new RepaymentTransaction
                {
                    TransactionId = t.Id,
                    PaymentDate = t.TransactionDate,
                    Amount = t.Amount,
                    PrincipalPaid = t.PrincipalAmount,
                    InterestPaid = t.InterestAmount,
                    PenaltyPaid = t.PenaltyAmount,
                    PaymentMethod = t.PaymentMethod,
                    TransactionReference = t.TransactionReference
                }).ToList()
            };

            return history;
        }

        /// <summary>
        /// Gets repayment schedule for a loan
        /// </summary>
        public async Task<List<RepaymentScheduleItem>> GetRepaymentScheduleAsync(string loanId)
        {
            var schedules = (await _scheduleRepository.GetAllAsync())
                .Where(s => s.LoanId == loanId)
                .OrderBy(s => s.InstallmentNumber)
                .ToList();

            return schedules.Select(s => new RepaymentScheduleItem
            {
                InstallmentNumber = s.InstallmentNumber,
                DueDate = s.DueDate,
                PrincipalAmount = s.PrincipalAmount,
                InterestAmount = s.InterestAmount,
                TotalAmount = s.PrincipalAmount + s.InterestAmount,
                Status = s.Status,
                PaidDate = s.PaidDate,
                AmountPaid = s.AmountPaid
            }).ToList();
        }

        #region Helper Methods

        private decimal CalculateAccruedInterest(Loan loan)
        {
            // Calculate interest on current outstanding principal
            DateTime lastPaymentDate = loan.LastPaymentDate ?? loan.DisbursementDate;
            int daysSinceLastPayment = (DateTime.UtcNow - lastPaymentDate).Days;
            
            decimal dailyRate = (loan.InterestRate / 100) / 365;
            decimal outstandingPrincipal = loan.PrincipalAmount - loan.PrincipalPaid;
            decimal accruedInterest = outstandingPrincipal * dailyRate * daysSinceLastPayment;

            return Math.Round(accruedInterest, 2);
        }

        private DateTime? CalculateNextPaymentDate(Loan loan)
        {
            if (loan.PaymentFrequency == "MONTHLY")
            {
                DateTime lastPayment = loan.LastPaymentDate ?? loan.DisbursementDate;
                return lastPayment.AddMonths(1);
            }

            return loan.NextPaymentDate;
        }

        private decimal CalculateArrearsAmount(Loan loan)
        {
            // Calculate how many installments are overdue
            if (!loan.NextPaymentDate.HasValue)
                return 0;

            int monthsOverdue = ((DateTime.UtcNow.Year - loan.NextPaymentDate.Value.Year) * 12) +
                               DateTime.UtcNow.Month - loan.NextPaymentDate.Value.Month;

            return Math.Max(0, monthsOverdue * loan.MonthlyInstallment);
        }

        #endregion
    }
}
