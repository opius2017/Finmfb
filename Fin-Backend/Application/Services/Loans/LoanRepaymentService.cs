using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Application.Interfaces.Repositories.Loans;
using FinTech.Core.Application.Interfaces.Loans;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinTech.Core.Application.Services.Loans
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
                var allSchedule = await _repaymentScheduleRepository.GetByLoanIdAsync(repaymentRequest.LoanAccountNumber);
                var unpaidScheduleItems = allSchedule.Where(s => (s.AmountPaid ?? 0m) < s.TotalAmount).ToList();
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

                    decimal dueAmount = scheduleItem.TotalAmount - (scheduleItem.AmountPaid ?? 0m);
                    decimal paymentAmount = Math.Min(remainingAmount, dueAmount);

                    scheduleItem.AmountPaid = (scheduleItem.AmountPaid ?? 0m) + paymentAmount;
                    scheduleItem.PaymentDate = DateTime.UtcNow;

                    if (Math.Abs((scheduleItem.AmountPaid ?? 0m) - scheduleItem.TotalAmount) < 0.01m)
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
                // Record the transaction using domain constructor (principal/interest split simplified)
                decimal principalPaid = repaymentRequest.Amount - remainingAmount;
                decimal interestPaid = 0m; // interest split not calculated here

                var transaction = new LoanTransaction(
                    loan.Id,
                    "REPAYMENT",
                    principalPaid,
                    interestPaid,
                    repaymentRequest.LoanAccountNumber ?? string.Empty,
                    repaymentRequest.LoanAccountNumber ?? string.Empty);

                await _transactionRepository.AddAsync(transaction);

                // Update loan outstanding balance using domain method
                loan.RecordRepayment(principalPaid, interestPaid, transaction.Reference, transaction.Description);

                // Persist loan changes
                await _loanRepository.UpdateAsync(loan);

                // Return a minimal response aligned with DTO
                return new LoanRepaymentResponseDto
                {
                    Success = true,
                    Message = "Repayment processed",
                    TransactionId = Guid.TryParse(transaction.Id, out var tid) ? tid : Guid.Empty
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

        // Adapter methods to satisfy ILoanRepaymentService contract
        public async Task<IEnumerable<LoanRepaymentDto>> GetRepaymentsByLoanIdAsync(string loanId)
        {
            var scheduleItems = await _repaymentScheduleRepository.GetByLoanIdAsync(loanId);
            var results = scheduleItems.Select(s => new LoanRepaymentDto
            {
                Id = s.Id,
                LoanId = s.LoanId,
                Amount = s.TotalAmount,
                Reference = string.Empty,
                Description = s.Status.ToString(),
                PaymentDate = s.LastPaymentDate,
                Status = s.Status.ToString()
            });

            return results;
        }

        public async Task<LoanRepaymentDto> RecordRepaymentAsync(string loanId, LoanRepaymentDto repaymentDto)
        {
            var request = new LoanRepaymentRequestDto
            {
                LoanAccountNumber = loanId,
                Amount = repaymentDto.Amount,
                RepaymentDate = repaymentDto.PaymentDate ?? DateTime.UtcNow
            };

            var response = await ProcessRepaymentAsync(request);

            return new LoanRepaymentDto
            {
                Id = response.TransactionId.ToString(),
                LoanId = loanId,
                Amount = repaymentDto.Amount,
                Reference = repaymentDto.Reference,
                PaymentDate = repaymentDto.PaymentDate,
                Status = "Processed"
            };
        }

        private LoanRepaymentScheduleDto MapToDto(LoanRepaymentSchedule schedule)
        {
            return new LoanRepaymentScheduleDto
            {
                Id = schedule.Id,
                LoanId = schedule.LoanAccountId.ToString(),
                InstallmentNumber = schedule.InstallmentNumber,
                DueDate = schedule.DueDate,
                PrincipalAmount = schedule.PrincipalAmount,
                InterestAmount = schedule.InterestAmount,
                TotalAmount = schedule.TotalAmount,
                PaidAmount = schedule.AmountPaid ?? 0m,
                OutstandingAmount = schedule.OutstandingBalance,
                PaymentDate = schedule.PaymentDate,
                Status = schedule.Status.ToString()
            };
        }
    }
}
