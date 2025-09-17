using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Application.DTOs.Loans;
using FinTech.Application.Interfaces.Repositories.Loans;
using FinTech.Application.Interfaces.Services.Loans;
using FinTech.Domain.Entities.Loans;
using Microsoft.Extensions.Logging;

namespace FinTech.Application.Services.Loans
{
    public class LoanRepaymentService : ILoanRepaymentService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILoanRepaymentScheduleRepository _repaymentScheduleRepository;
        private readonly ILoanTransactionRepository _transactionRepository;
        private readonly ILogger<LoanRepaymentService> _logger;

        public LoanRepaymentService(
            ILoanRepository loanRepository,
            ILoanRepaymentScheduleRepository repaymentScheduleRepository,
            ILoanTransactionRepository transactionRepository,
            ILogger<LoanRepaymentService> logger)
        {
            _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
            _repaymentScheduleRepository = repaymentScheduleRepository ?? throw new ArgumentNullException(nameof(repaymentScheduleRepository));
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<LoanRepaymentScheduleDto>> GetRepaymentScheduleByLoanIdAsync(string loanId)
        {
            try
            {
                var loan = await _loanRepository.GetByIdAsync(loanId);
                if (loan == null)
                {
                    throw new ApplicationException($"Loan with ID {loanId} not found");
                }

                var schedule = await _repaymentScheduleRepository.GetByLoanIdAsync(loanId);
                return schedule.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting repayment schedule for loan ID: {LoanId}", loanId);
                throw;
            }
        }

        public async Task<LoanRepaymentScheduleDto> GetRepaymentScheduleItemByIdAsync(string id)
        {
            try
            {
                var scheduleItem = await _repaymentScheduleRepository.GetByIdAsync(id);
                return scheduleItem != null ? MapToDto(scheduleItem) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting repayment schedule item with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<LoanRepaymentScheduleDto>> GetOverdueRepaymentsAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var overdueSchedules = await _repaymentScheduleRepository.GetOverdueRepaymentsAsync(today);
                return overdueSchedules.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overdue repayments");
                throw;
            }
        }

        public async Task<LoanRepaymentResponseDto> ProcessRepaymentAsync(LoanRepaymentRequestDto repaymentRequest)
        {
            try
            {
                var loan = await _loanRepository.GetByIdAsync(repaymentRequest.LoanId);
                if (loan == null)
                {
                    throw new ApplicationException($"Loan with ID {repaymentRequest.LoanId} not found");
                }

                if (loan.Status != LoanStatus.Active)
                {
                    throw new ApplicationException($"Cannot process repayment for loan with status {loan.Status}");
                }

                // Get all unpaid schedule items
                var unpaidScheduleItems = await _repaymentScheduleRepository.GetUnpaidScheduleItemsByLoanIdAsync(repaymentRequest.LoanId);
                if (!unpaidScheduleItems.Any())
                {
                    throw new ApplicationException("No unpaid schedule items found for this loan");
                }

                decimal remainingAmount = repaymentRequest.Amount;
                var updatedScheduleItems = new List<LoanRepaymentSchedule>();
                var processedScheduleItems = new List<LoanRepaymentScheduleDto>();

                // Process repayment for each unpaid schedule item (oldest first)
                foreach (var scheduleItem in unpaidScheduleItems.OrderBy(s => s.DueDate))
                {
                    if (remainingAmount <= 0) break;

                    decimal dueAmount = scheduleItem.TotalAmount - scheduleItem.PaidAmount;
                    decimal paymentAmount = Math.Min(remainingAmount, dueAmount);

                    scheduleItem.PaidAmount += paymentAmount;
                    scheduleItem.LastPaymentDate = DateTime.UtcNow;
                    
                    if (Math.Abs(scheduleItem.PaidAmount - scheduleItem.TotalAmount) < 0.01m)
                    {
                        scheduleItem.Status = RepaymentStatus.Paid;
                    }
                    else
                    {
                        scheduleItem.Status = RepaymentStatus.PartiallyPaid;
                    }

                    await _repaymentScheduleRepository.UpdateAsync(scheduleItem);
                    updatedScheduleItems.Add(scheduleItem);
                    processedScheduleItems.Add(MapToDto(scheduleItem));
                    
                    remainingAmount -= paymentAmount;
                }

                // Record the transaction
                var transaction = new LoanTransaction
                {
                    Id = Guid.NewGuid().ToString(),
                    LoanId = repaymentRequest.LoanId,
                    TransactionDate = DateTime.UtcNow,
                    TransactionType = "Repayment",
                    Amount = repaymentRequest.Amount,
                    ReferenceNumber = repaymentRequest.ReferenceNumber,
                    Description = repaymentRequest.Description ?? "Loan repayment",
                    ProcessedBy = repaymentRequest.ProcessedBy,
                    PaymentMethod = repaymentRequest.PaymentMethod,
                    Status = "Completed"
                };

                await _transactionRepository.AddAsync(transaction);

                // Update loan outstanding balance
                loan.OutstandingPrincipal -= (repaymentRequest.Amount - remainingAmount);
                
                // Check if loan is fully paid
                bool isFullyPaid = loan.OutstandingPrincipal <= 0;
                if (isFullyPaid)
                {
                    loan.Status = LoanStatus.Closed;
                    loan.ClosureDate = DateTime.UtcNow;
                    loan.ClosureReason = "Fully paid";
                }

                await _loanRepository.UpdateAsync(loan);

                // Return the response
                return new LoanRepaymentResponseDto
                {
                    TransactionId = transaction.Id,
                    LoanId = loan.Id,
                    AmountPaid = repaymentRequest.Amount,
                    RemainingBalance = loan.OutstandingPrincipal,
                    TransactionDate = transaction.TransactionDate,
                    IsFullyPaid = isFullyPaid,
                    ProcessedScheduleItems = processedScheduleItems,
                    ReferenceNumber = transaction.ReferenceNumber
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing repayment for loan ID: {LoanId}", repaymentRequest.LoanId);
                throw;
            }
        }

        public async Task<LoanRepaymentScheduleDto> UpdateRepaymentScheduleItemAsync(LoanRepaymentScheduleDto scheduleDto)
        {
            try
            {
                var existingSchedule = await _repaymentScheduleRepository.GetByIdAsync(scheduleDto.Id);
                if (existingSchedule == null)
                {
                    throw new ApplicationException($"Repayment schedule item with ID {scheduleDto.Id} not found");
                }

                existingSchedule.DueDate = scheduleDto.DueDate;
                existingSchedule.PrincipalAmount = scheduleDto.PrincipalAmount;
                existingSchedule.InterestAmount = scheduleDto.InterestAmount;
                existingSchedule.TotalAmount = scheduleDto.PrincipalAmount + scheduleDto.InterestAmount;
                existingSchedule.Status = scheduleDto.Status;
                existingSchedule.LastModifiedDate = DateTime.UtcNow;

                var result = await _repaymentScheduleRepository.UpdateAsync(existingSchedule);
                return MapToDto(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating repayment schedule item with ID: {Id}", scheduleDto.Id);
                throw;
            }
        }

        public async Task<IEnumerable<LoanRepaymentScheduleDto>> GenerateRepaymentScheduleAsync(string loanId)
        {
            try
            {
                var loan = await _loanRepository.GetByIdAsync(loanId);
                if (loan == null)
                {
                    throw new ApplicationException($"Loan with ID {loanId} not found");
                }

                // Delete existing schedule if any
                var existingSchedule = await _repaymentScheduleRepository.GetByLoanIdAsync(loanId);
                foreach (var item in existingSchedule)
                {
                    await _repaymentScheduleRepository.DeleteAsync(item.Id);
                }

                // Generate new schedule
                var scheduleItems = new List<LoanRepaymentSchedule>();
                
                // Get loan terms
                decimal principal = loan.LoanAmount;
                decimal interestRate = loan.InterestRate / 100;
                int term = loan.LoanTerm;
                DateTime startDate = loan.DisbursementDate ?? DateTime.UtcNow;
                
                // Calculate payments based on loan type
                if (loan.RepaymentFrequency == "Monthly")
                {
                    // For amortized loans (equal payments)
                    if (loan.InterestType == "Flat")
                    {
                        // Flat interest - equal principal payments + flat interest
                        decimal monthlyPrincipal = principal / term;
                        decimal monthlyInterest = principal * interestRate / 12;
                        
                        for (int i = 1; i <= term; i++)
                        {
                            var scheduleItem = new LoanRepaymentSchedule
                            {
                                Id = Guid.NewGuid().ToString(),
                                LoanId = loanId,
                                InstallmentNumber = i,
                                DueDate = startDate.AddMonths(i),
                                PrincipalAmount = monthlyPrincipal,
                                InterestAmount = monthlyInterest,
                                TotalAmount = monthlyPrincipal + monthlyInterest,
                                PaidAmount = 0,
                                Status = RepaymentStatus.Pending,
                                CreatedDate = DateTime.UtcNow,
                                LastModifiedDate = DateTime.UtcNow
                            };
                            
                            scheduleItems.Add(scheduleItem);
                            await _repaymentScheduleRepository.AddAsync(scheduleItem);
                        }
                    }
                    else if (loan.InterestType == "Reducing Balance")
                    {
                        // Reducing balance - amortized payments
                        decimal monthlyRate = interestRate / 12;
                        decimal payment = principal * monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), term) / 
                                         ((decimal)Math.Pow((double)(1 + monthlyRate), term) - 1);
                        
                        decimal remainingPrincipal = principal;
                        
                        for (int i = 1; i <= term; i++)
                        {
                            decimal interestPayment = remainingPrincipal * monthlyRate;
                            decimal principalPayment = payment - interestPayment;
                            
                            remainingPrincipal -= principalPayment;
                            
                            var scheduleItem = new LoanRepaymentSchedule
                            {
                                Id = Guid.NewGuid().ToString(),
                                LoanId = loanId,
                                InstallmentNumber = i,
                                DueDate = startDate.AddMonths(i),
                                PrincipalAmount = principalPayment,
                                InterestAmount = interestPayment,
                                TotalAmount = payment,
                                PaidAmount = 0,
                                Status = RepaymentStatus.Pending,
                                CreatedDate = DateTime.UtcNow,
                                LastModifiedDate = DateTime.UtcNow
                            };
                            
                            scheduleItems.Add(scheduleItem);
                            await _repaymentScheduleRepository.AddAsync(scheduleItem);
                        }
                    }
                }
                else if (loan.RepaymentFrequency == "Weekly")
                {
                    // Implement weekly repayment schedule logic
                    int totalWeeks = term * 4; // Approximate
                    decimal weeklyPrincipal = principal / totalWeeks;
                    decimal weeklyInterest = principal * interestRate / 52;
                    
                    for (int i = 1; i <= totalWeeks; i++)
                    {
                        var scheduleItem = new LoanRepaymentSchedule
                        {
                            Id = Guid.NewGuid().ToString(),
                            LoanId = loanId,
                            InstallmentNumber = i,
                            DueDate = startDate.AddDays(7 * i),
                            PrincipalAmount = weeklyPrincipal,
                            InterestAmount = weeklyInterest,
                            TotalAmount = weeklyPrincipal + weeklyInterest,
                            PaidAmount = 0,
                            Status = RepaymentStatus.Pending,
                            CreatedDate = DateTime.UtcNow,
                            LastModifiedDate = DateTime.UtcNow
                        };
                        
                        scheduleItems.Add(scheduleItem);
                        await _repaymentScheduleRepository.AddAsync(scheduleItem);
                    }
                }
                // Implement other frequency types as needed
                
                return scheduleItems.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating repayment schedule for loan ID: {LoanId}", loanId);
                throw;
            }
        }

        private LoanRepaymentScheduleDto MapToDto(LoanRepaymentSchedule schedule)
        {
            return new LoanRepaymentScheduleDto
            {
                Id = schedule.Id,
                LoanId = schedule.LoanId,
                InstallmentNumber = schedule.InstallmentNumber,
                DueDate = schedule.DueDate,
                PrincipalAmount = schedule.PrincipalAmount,
                InterestAmount = schedule.InterestAmount,
                TotalAmount = schedule.TotalAmount,
                PaidAmount = schedule.PaidAmount,
                LastPaymentDate = schedule.LastPaymentDate,
                Status = schedule.Status,
                CreatedDate = schedule.CreatedDate,
                LastModifiedDate = schedule.LastModifiedDate
            };
        }
    }
}