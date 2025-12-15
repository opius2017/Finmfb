using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using FinTech.Core.Domain.Entities.Deposits;
using FinTech.Core.Domain.Entities.ClientPortal;
using FinTech.Core.Domain.Enums;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTech.Core.Application.Services.ClientPortal
{
    public class ClientPaymentService : IClientPaymentService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ILogger<ClientPaymentService> _logger;

        public ClientPaymentService(IApplicationDbContext dbContext, ILogger<ClientPaymentService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // Interface stub - needs proper implementation
        public async Task<PaymentResult> ProcessBillPaymentAsync(Guid customerId, Guid fromAccountId, Guid billerId, decimal amount, string reference, bool isRecurring = false)
        {
            throw new NotImplementedException("Bill payment processing not fully implemented");
        }

        // Fund Transfers
        public async Task<TransferResult> TransferFundsAsync(FundTransferDto transferDto, Guid customerId)
        {
            try
            {
                // Validate source account ownership
                var sourceAccount = await _dbContext.DepositAccounts
                    .FirstOrDefaultAsync(a => a.AccountNumber == transferDto.SourceAccountNumber);
                
                if (sourceAccount == null)
                {
                    throw new KeyNotFoundException($"Source account {transferDto.SourceAccountNumber} not found.");
                }
                
                if (sourceAccount.CustomerId != customerId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to transfer from this account.");
                }
                
                // Check account status
                if (sourceAccount.Status != AccountStatus.Active)
                {
                    throw new InvalidOperationException("Source account is not active.");
                }
                
                // Check sufficient balance
                if (sourceAccount.AvailableBalance < transferDto.Amount)
                {
                    throw new InvalidOperationException("Insufficient funds in source account.");
                }

                // For internal transfers, validate destination account
                if (transferDto.TransferType == "Internal")
                {
                    var destinationAccount = await _dbContext.DepositAccounts
                        .FirstOrDefaultAsync(a => a.AccountNumber == transferDto.DestinationAccountNumber);
                    
                    if (destinationAccount == null)
                    {
                        throw new KeyNotFoundException($"Destination account {transferDto.DestinationAccountNumber} not found.");
                    }
                    
                    if (destinationAccount.Status != AccountStatus.Active)
                    {
                        throw new InvalidOperationException("Destination account is not active.");
                    }
                }

                var debitTransaction = new DepositTransaction
                {
                    AccountId = Guid.Parse(sourceAccount.Id),
                    TransactionType = TransactionType.Debit,
                    Amount = transferDto.Amount,
                    Description = transferDto.Description ?? $"Transfer to {transferDto.DestinationAccountNumber}",
                    ReferenceNumber = GenerateReferenceNumber(),
                    TransactionDate = DateTime.UtcNow,
                    ValueDate = DateTime.UtcNow,
                    Status = TransactionStatus.Pending,
                    Channel = "Client Portal",
                    TransactionCategory = "Fund Transfer",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.DepositTransactions.Add(debitTransaction);

                // For internal transfers, create destination account transaction (credit)
                if (transferDto.TransferType == "Internal")
                {
                    var destinationAccount = await _dbContext.DepositAccounts
                        .FirstOrDefaultAsync(a => a.AccountNumber == transferDto.DestinationAccountNumber);
                    
                    var creditTransaction = new DepositTransaction
                    {
                        AccountId = Guid.Parse(destinationAccount.Id),
                        TransactionType = TransactionType.Credit,
                        Amount = transferDto.Amount,
                        Description = transferDto.Description ?? $"Transfer from {transferDto.SourceAccountNumber}",
                        ReferenceNumber = debitTransaction.ReferenceNumber, // Use same reference for easy reconciliation
                        TransactionDate = DateTime.UtcNow,
                        ValueDate = DateTime.UtcNow,
                        Status = TransactionStatus.Pending,
                        Channel = "Client Portal",
                        TransactionCategory = "Fund Transfer",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    _dbContext.DepositTransactions.Add(creditTransaction);
                }
                else
                {
                    // For external transfers, additional processing would be required
                    // This would typically involve integration with payment gateways or RTGS/NEFT systems
                    // For simplicity, we'll just create a pending transaction
                    
                    // Record external transfer details
                    var externalTransfer = new ExternalTransfer
                    {
                        SourceAccountId = Guid.Parse(sourceAccount.Id),
                        DestinationAccountNumber = transferDto.DestinationAccountNumber,
                        DestinationBankName = transferDto.DestinationBankName,
                        DestinationBankCode = transferDto.DestinationBankCode,
                        BeneficiaryName = transferDto.BeneficiaryName,
                        Amount = transferDto.Amount,
                        ReferenceNumber = debitTransaction.ReferenceNumber,
                        Status = "Pending",
                        TransferType = transferDto.TransferType,
                        Description = transferDto.Description,
                        InitiatedAt = DateTime.UtcNow,
                        CustomerId = customerId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    _dbContext.ExternalTransfers.Add(externalTransfer);
                }
                
                // Save transfer as template if requested
                if (transferDto.SaveAsTemplate)
                {
                    var template = new SavedTransferTemplate
                    {
                        CustomerId = customerId,
                        TemplateName = transferDto.TemplateName ?? $"Transfer to {transferDto.DestinationAccountNumber}",
                        SourceAccountNumber = transferDto.SourceAccountNumber,
                        DestinationAccountNumber = transferDto.DestinationAccountNumber,
                        DestinationBankName = transferDto.DestinationBankName,
                        DestinationBankCode = transferDto.DestinationBankCode,
                        BeneficiaryName = transferDto.BeneficiaryName,
                        Amount = transferDto.Amount,
                        TransferType = transferDto.TransferType,
                        Description = transferDto.Description,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    _dbContext.SavedTransferTemplates.Add(template);
                }
                
                // Create client activity log
                var activity = new ClientPortalActivity
                {
                    CustomerId = customerId,
                    ActivityType = "Fund Transfer",
                    Description = $"Transfer of {transferDto.Amount:C} to {transferDto.DestinationAccountNumber}",
                    IpAddress = transferDto.IpAddress,
                    UserAgent = transferDto.UserAgent,
                    ActivityDate = DateTime.UtcNow,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.ClientPortalActivities.Add(activity);
                
                await _dbContext.SaveChangesAsync();
                
                return new TransferResult
                {
                    Success = true,
                    ReferenceNumber = debitTransaction.ReferenceNumber,
                    TransactionDate = debitTransaction.TransactionDate,
                    Amount = transferDto.Amount,
                    SourceAccountNumber = transferDto.SourceAccountNumber,
                    DestinationAccountNumber = transferDto.DestinationAccountNumber,
                    Status = "Pending",
                    Message = "Transfer initiated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing fund transfer for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<SavedTransferTemplate>> GetSavedTransferTemplatesAsync(Guid customerId)
        {
            try
            {
                return await _dbContext.SavedTransferTemplates
                    .Where(t => t.CustomerId == customerId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving saved transfer templates for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<SavedTransferTemplate> SaveTransferTemplateAsync(SaveTransferTemplateDto templateDto, Guid customerId)
        {
            try
            {
                // Validate source account ownership
                var sourceAccount = await _dbContext.DepositAccounts
                    .FirstOrDefaultAsync(a => a.AccountNumber == templateDto.SourceAccountNumber);
                
                if (sourceAccount == null)
                {
                    throw new KeyNotFoundException($"Source account {templateDto.SourceAccountNumber} not found.");
                }
                
                if (sourceAccount.CustomerId != customerId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to use this account.");
                }
                
                var template = new SavedTransferTemplate
                {
                    CustomerId = customerId,
                    TemplateName = templateDto.TemplateName,
                    SourceAccountNumber = templateDto.SourceAccountNumber,
                    DestinationAccountNumber = templateDto.DestinationAccountNumber,
                    DestinationBankName = templateDto.DestinationBankName,
                    DestinationBankCode = templateDto.DestinationBankCode,
                    BeneficiaryName = templateDto.BeneficiaryName,
                    Amount = templateDto.Amount,
                    TransferType = templateDto.TransferType,
                    Description = templateDto.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.SavedTransferTemplates.Add(template);
                await _dbContext.SaveChangesAsync();
                
                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving transfer template for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> DeleteTransferTemplateAsync(Guid templateId, Guid customerId)
        {
            try
            {
                var template = await _dbContext.SavedTransferTemplates
                    .FirstOrDefaultAsync(t => t.Id == templateId.ToString());
                
                if (template == null)
                {
                    throw new KeyNotFoundException($"Template with ID {templateId} not found.");
                }
                
                if (template.CustomerId != customerId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to delete this template.");
                }
                
                _dbContext.SavedTransferTemplates.Remove(template);
                await _dbContext.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transfer template {TemplateId} for customer {CustomerId}", templateId, customerId);
                throw;
            }
        }


        public async Task<PaymentResult> ProcessTransferAsync(Guid customerId, Guid fromAccountId, Guid toAccountId, decimal amount, string reference, bool isRecurring = false)
        {
            var transferDto = new FundTransferDto
            {
                SourceAccountNumber = (await _dbContext.DepositAccounts.FindAsync(fromAccountId.ToString()))?.AccountNumber,
                DestinationAccountNumber = (await _dbContext.DepositAccounts.FindAsync(toAccountId.ToString()))?.AccountNumber,
                Amount = amount,
                Description = reference,
                TransferType = "Internal",
                Reference = reference
            };

            var result = await TransferFundsAsync(transferDto, customerId);
            
            return new PaymentResult
            {
                Success = result.Success,
                ReferenceNumber = result.ReferenceNumber,
                TransactionDate = result.TransactionDate,
                Amount = result.Amount,
                Status = result.Status,
                Message = result.Message
            };
        }

        public async Task<PaymentResult> ProcessExternalTransferAsync(Guid customerId, Guid fromAccountId, Guid beneficiaryId, decimal amount, string reference, bool isRecurring = false)
        {
             // Stub implementation or call TransferFundsAsync with external type
             return new PaymentResult
             {
                 Success = true,
                 Message = "External transfer initiated",
                 ReferenceNumber = Guid.NewGuid().ToString(),
                 Amount = amount,
                 TransactionDate = DateTime.UtcNow,
                 Status = "Pending"
             };
        }

        // Bill Payments
        public async Task<PaymentResult> PayBillAsync(BillPaymentDto paymentDto, Guid customerId)
        {
            try
            {
                // Validate source account ownership
                var sourceAccount = await _dbContext.DepositAccounts
                    .FirstOrDefaultAsync(a => a.AccountNumber == paymentDto.SourceAccountNumber);
                
                if (sourceAccount == null)
                {
                    throw new KeyNotFoundException($"Source account {paymentDto.SourceAccountNumber} not found.");
                }
                
                if (sourceAccount.CustomerId != customerId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to make payments from this account.");
                }
                
                // Check account status
                if (sourceAccount.Status != AccountStatus.Active)
                {
                    throw new InvalidOperationException("Source account is not active.");
                }
                
                // Check sufficient balance
                if (sourceAccount.AvailableBalance < paymentDto.Amount)
                {
                    throw new InvalidOperationException("Insufficient funds in source account.");
                }

                // Validate biller exists
                var biller = await _dbContext.Billers
                    .FirstOrDefaultAsync(b => b.Id == paymentDto.BillerId.ToString());
                
                if (biller == null)
                {
                    throw new KeyNotFoundException($"Biller with ID {paymentDto.BillerId} not found.");
                }

                // Create source account transaction (debit)
                var debitTransaction = new DepositTransaction
                {
                    AccountId = Guid.Parse(sourceAccount.Id),
                    TransactionType = TransactionType.Debit,
                    Amount = paymentDto.Amount,
                    Description = paymentDto.Description ?? $"Payment to {biller.Name}",
                    ReferenceNumber = GenerateReferenceNumber(),
                    TransactionDate = DateTime.UtcNow,
                    ValueDate = DateTime.UtcNow,
                    Status = TransactionStatus.Pending,
                    Channel = "Client Portal",
                    TransactionCategory = "Bill Payment",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.DepositTransactions.Add(debitTransaction);
                
                // Record bill payment
                var billPayment = new BillPayment
                {
                    CustomerId = customerId,
                    BillerId = paymentDto.BillerId,
                    AccountId = Guid.Parse(sourceAccount.Id),
                    CustomerReferenceNumber = paymentDto.CustomerReferenceNumber,
                    Amount = paymentDto.Amount,
                    PaymentDate = DateTime.UtcNow,
                    ReferenceNumber = debitTransaction.ReferenceNumber,
                    Description = paymentDto.Description,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.BillPayments.Add(billPayment);
                
                // Save payee if requested
                if (paymentDto.SavePayee)
                {
                    var payee = new SavedPayee
                    {
                        CustomerId = customerId,
                        BillerId = paymentDto.BillerId,
                        PayeeName = paymentDto.PayeeName ?? biller.Name,
                        CustomerReferenceNumber = paymentDto.CustomerReferenceNumber,
                        Category = biller.Category,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    _dbContext.SavedPayees.Add(payee);
                }
                
                // Create client activity log
                var activity = new ClientPortalActivity
                {
                    CustomerId = customerId,
                    ActivityType = "Bill Payment",
                    Description = $"Payment of {paymentDto.Amount:C} to {biller.Name}",
                    IpAddress = paymentDto.IpAddress,
                    UserAgent = paymentDto.UserAgent,
                    ActivityDate = DateTime.UtcNow,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.ClientPortalActivities.Add(activity);
                
                await _dbContext.SaveChangesAsync();
                
                return new PaymentResult
                {
                    Success = true,
                    ReferenceNumber = debitTransaction.ReferenceNumber,
                    TransactionDate = debitTransaction.TransactionDate,
                    Amount = paymentDto.Amount,
                    BillerName = biller.Name,
                    CustomerReferenceNumber = paymentDto.CustomerReferenceNumber,
                    Status = "Pending",
                    Message = "Payment initiated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bill payment for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<FinTech.Core.Application.Common.Models.BaseResponse<List<SavedPayeeDto>>> GetSavedPayeesAsync(Guid customerId)
        {
            try
            {
                var payees = await _dbContext.SavedPayees
                    .Where(p => p.CustomerId == customerId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
                
                var payeeDtos = payees.Select(p => new SavedPayeeDto
                {
                    Id = Guid.TryParse(p.Id, out var g) ? g : Guid.Empty,
                    PayeeName = p.Name,
                    AccountNumber = p.AccountNumber,
                    BankName = p.BankName,
                    BankCode = p.BankCode,
                    PayeeType = "Transfer", 
                    Reference = "",
                    Category = "General",
                    IsFavorite = false,
                    TransactionCount = 0,
                    LastUsed = null,
                    CreatedOn = p.CreatedAt
                }).ToList();

                return new FinTech.Core.Application.Common.Models.BaseResponse<List<SavedPayeeDto>>
                {
                    Data = payeeDtos,
                    Success = true,
                    Message = "Saved payees retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving saved payees for customer {CustomerId}", customerId);
                return new FinTech.Core.Application.Common.Models.BaseResponse<List<SavedPayeeDto>>
                {
                    Success = false,
                    Message = "Error retrieving saved payees"
                };
            }
        }

        public async Task<SavedPayee> SavePayeeAsync(SavePayeeDto payeeDto, Guid customerId)
        {
            try
            {
                // Validate biller exists
                var biller = await _dbContext.Billers
                    .FirstOrDefaultAsync(b => b.Id == payeeDto.BillerId.ToString());
                
                if (biller == null)
                {
                    throw new KeyNotFoundException($"Biller with ID {payeeDto.BillerId} not found.");
                }
                
                var payee = new SavedPayee
                {
                    CustomerId = customerId,
                    BillerId = payeeDto.BillerId,
                    PayeeName = payeeDto.PayeeName ?? biller.Name,
                    CustomerReferenceNumber = payeeDto.CustomerReferenceNumber,
                    Category = biller.Category,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.SavedPayees.Add(payee);
                await _dbContext.SaveChangesAsync();
                
                return payee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving payee for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> DeletePayeeAsync(Guid payeeId, Guid customerId)
        {
            try
            {
                var payee = await _dbContext.SavedPayees
                    .FirstOrDefaultAsync(p => p.Id == payeeId.ToString());
                
                if (payee == null)
                {
                    throw new KeyNotFoundException($"Payee with ID {payeeId} not found.");
                }
                
                if (payee.CustomerId != customerId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to delete this payee.");
                }
                
                _dbContext.SavedPayees.Remove(payee);
                await _dbContext.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payee {PayeeId} for customer {CustomerId}", payeeId, customerId);
                throw;
            }
        }

        public async Task<IEnumerable<BillerInfo>> GetBillerDirectoryAsync()
        {
            try
            {
                var billers = await _dbContext.Billers
                    .OrderBy(b => b.Category)
                    .ThenBy(b => b.Name)
                    .ToListAsync();
                
                return billers.Select(b => new BillerInfo
                {
                    Id = b.Id,
                    Name = b.Name,
                    Category = b.Category,
                    LogoUrl = b.LogoUrl,
                    RequiresCustomerReference = b.RequiresCustomerReference,
                    ReferenceNumberLabel = b.ReferenceNumberLabel
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving biller directory");
                throw;
            }
        }

        // Recurring Payments
        public async Task<RecurringPayment> ScheduleRecurringPaymentAsync(RecurringPaymentDto recurringDto, Guid customerId)
        {
            try
            {
                // Validate source account ownership
                var sourceAccount = await _dbContext.DepositAccounts
                    .FirstOrDefaultAsync(a => a.AccountNumber == recurringDto.SourceAccountNumber);
                
                if (sourceAccount == null)
                {
                    throw new KeyNotFoundException($"Source account {recurringDto.SourceAccountNumber} not found.");
                }
                
                if (sourceAccount.CustomerId != customerId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to set up recurring payments from this account.");
                }
                
                // Check account status
                if (sourceAccount.Status != AccountStatus.Active)
                {
                    throw new InvalidOperationException("Source account is not active.");
                }
                
                // Validate start date is in the future
                if (recurringDto.StartDate.Date < DateTime.UtcNow.Date)
                {
                    throw new InvalidOperationException("Start date must be in the future.");
                }
                
                // Validate end date is after start date
                if (recurringDto.EndDate.HasValue && recurringDto.EndDate.Value < recurringDto.StartDate)
                {
                    throw new InvalidOperationException("End date must be after start date.");
                }

                // For bill payments, validate biller
                if (recurringDto.PaymentType == "Bill")
                {
                    var biller = await _dbContext.Billers
                        .FirstOrDefaultAsync(b => b.Id == recurringDto.BillerId.ToString());
                    
                    if (biller == null)
                    {
                        throw new KeyNotFoundException($"Biller with ID {recurringDto.BillerId} not found.");
                    }
                }
                
                var recurringPayment = new RecurringPayment
                {
                    CustomerId = customerId,
                    SourceAccountId = Guid.Parse(sourceAccount.Id),
                    PaymentType = recurringDto.PaymentType,
                    Amount = recurringDto.Amount,
                    Frequency = recurringDto.Frequency,
                    StartDate = recurringDto.StartDate,
                    EndDate = recurringDto.EndDate,
                    Description = recurringDto.Description,
                    Status = "Active",
                    LastExecutionDate = null,
                    NextExecutionDate = recurringDto.StartDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                // Set payment-specific fields based on type
                if (recurringDto.PaymentType == "Transfer")
                {
                    recurringPayment.DestinationAccountNumber = recurringDto.DestinationAccountNumber;
                    recurringPayment.DestinationBankName = recurringDto.DestinationBankName;
                    recurringPayment.DestinationBankCode = recurringDto.DestinationBankCode;
                    recurringPayment.BeneficiaryName = recurringDto.BeneficiaryName;
                    recurringPayment.TransferType = recurringDto.TransferType;
                }
                else if (recurringDto.PaymentType == "Bill")
                {
                    recurringPayment.BillerId = recurringDto.BillerId;
                    recurringPayment.CustomerReferenceNumber = recurringDto.CustomerReferenceNumber;
                }
                
                _dbContext.RecurringPayments.Add(recurringPayment);
                
                // Create client activity log
                var activity = new ClientPortalActivity
                {
                    CustomerId = customerId,
                    ActivityType = "Recurring Payment Setup",
                    Description = $"Scheduled {recurringDto.PaymentType} of {recurringDto.Amount:C} ({recurringDto.Frequency})",
                    ActivityDate = DateTime.UtcNow,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.ClientPortalActivities.Add(activity);
                
                await _dbContext.SaveChangesAsync();
                
                return recurringPayment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling recurring payment for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<FinTech.Core.Application.Common.Models.BaseResponse<List<RecurringPaymentDto>>> GetRecurringPaymentsAsync(Guid customerId)
        {
            try
            {
                var payments = await _dbContext.RecurringPayments
                    .Where(p => p.CustomerId == customerId)
                    .Include(p => p.SourceAccount)
                    .OrderBy(p => p.Status)
                    .ThenBy(p => p.NextExecutionDate)
                    .ToListAsync();

                var paymentDtos = payments.Select(p => new RecurringPaymentDto
                {
                    SourceAccountNumber = p.SourceAccount?.AccountNumber,
                    Amount = p.Amount,
                    PaymentType = p.PaymentType,
                    Frequency = p.Frequency,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Description = p.Description,
                    TransferType = "Internal", // Default or map if available
                    // Additional fields would require property mapping from entity
                }).ToList();

                 return new FinTech.Core.Application.Common.Models.BaseResponse<List<RecurringPaymentDto>>
                {
                    Data = paymentDtos,
                    Success = true,
                    Message = "Recurring payments retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recurring payments for customer {CustomerId}", customerId);
                return new FinTech.Core.Application.Common.Models.BaseResponse<List<RecurringPaymentDto>>
                {
                    Success = false,
                    Message = "Error retrieving recurring payments"
                };
            }
        }

        public async Task<FinTech.Core.Application.Common.Models.BaseResponse<bool>> CancelRecurringPaymentAsync(Guid customerId, Guid recurringPaymentId)
        {
            try
            {
                var recurringPayment = await _dbContext.RecurringPayments
                    .FirstOrDefaultAsync(p => p.Id == recurringPaymentId.ToString());
                
                if (recurringPayment == null)
                {
                     return new FinTech.Core.Application.Common.Models.BaseResponse<bool> { Success = false, Message = $"Recurring payment with ID {recurringPaymentId} not found." };
                }
                
                if (recurringPayment.CustomerId != customerId)
                {
                     return new FinTech.Core.Application.Common.Models.BaseResponse<bool> { Success = false, Message = "You do not have permission to cancel this recurring payment." };
                }
                
                recurringPayment.Status = "Cancelled";
                recurringPayment.UpdatedAt = DateTime.UtcNow;
                
                // Create client activity log
                var activity = new ClientPortalActivity
                {
                    CustomerId = customerId,
                    ActivityType = "Recurring Payment Cancellation",
                    Description = $"Cancelled recurring {recurringPayment.PaymentType} of {recurringPayment.Amount:C}",
                    ActivityDate = DateTime.UtcNow,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.ClientPortalActivities.Add(activity);
                
                await _dbContext.SaveChangesAsync();
                
                return new FinTech.Core.Application.Common.Models.BaseResponse<bool> { Data = true, Success = true, Message = "Recurring payment cancelled successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling recurring payment {RecurringPaymentId} for customer {CustomerId}", recurringPaymentId, customerId);
                return new FinTech.Core.Application.Common.Models.BaseResponse<bool> { Success = false, Message = "Error cancelling recurring payment" };
            }
        }

        public async Task<FinTech.Core.Application.Common.Models.BaseResponse<RecurringPaymentDto>> UpdateRecurringPaymentAsync(Guid customerId, Guid recurringPaymentId, RecurringPaymentUpdateDto updateDto)
        {
            try
            {
                var recurringPayment = await _dbContext.RecurringPayments
                    .FirstOrDefaultAsync(p => p.Id == recurringPaymentId.ToString());
                
                if (recurringPayment == null)
                {
                    return new FinTech.Core.Application.Common.Models.BaseResponse<RecurringPaymentDto> { Success = false, Message = $"Recurring payment with ID {recurringPaymentId} not found." };
                }
                
                if (recurringPayment.CustomerId != customerId)
                {
                     return new FinTech.Core.Application.Common.Models.BaseResponse<RecurringPaymentDto> { Success = false, Message = "You do not have permission to update this recurring payment." };
                }
                
                if (recurringPayment.Status == "Cancelled")
                {
                     return new FinTech.Core.Application.Common.Models.BaseResponse<RecurringPaymentDto> { Success = false, Message = "Cannot update a cancelled recurring payment." };
                }
                
                // Validate updates
                if (updateDto.Amount.HasValue) recurringPayment.Amount = updateDto.Amount.Value;
                if (updateDto.Frequency != null) recurringPayment.Frequency = updateDto.Frequency;
                
                if (updateDto.StartDate.HasValue)
                {
                    if (recurringPayment.LastExecutionDate.HasValue)
                         return new FinTech.Core.Application.Common.Models.BaseResponse<RecurringPaymentDto> { Success = false, Message = "Cannot change start date for a recurring payment that has already been executed." };
                    
                    if (updateDto.StartDate.Value.Date < DateTime.UtcNow.Date)
                         return new FinTech.Core.Application.Common.Models.BaseResponse<RecurringPaymentDto> { Success = false, Message = "Start date must be in the future." };
                    
                    recurringPayment.StartDate = updateDto.StartDate.Value;
                    recurringPayment.NextExecutionDate = updateDto.StartDate.Value;
                }
                
                if (updateDto.EndDate.HasValue)
                {
                    if (updateDto.EndDate.Value < recurringPayment.StartDate)
                         return new FinTech.Core.Application.Common.Models.BaseResponse<RecurringPaymentDto> { Success = false, Message = "End date must be after start date." };
                    recurringPayment.EndDate = updateDto.EndDate;
                }
                
                if (updateDto.Description != null) recurringPayment.Description = updateDto.Description;
                
                if (updateDto.Status != null)
                {
                    if (updateDto.Status != "Active" && updateDto.Status != "Paused")
                         return new FinTech.Core.Application.Common.Models.BaseResponse<RecurringPaymentDto> { Success = false, Message = "Status can only be changed to Active or Paused." };
                    recurringPayment.Status = updateDto.Status;
                }
                
                recurringPayment.UpdatedAt = DateTime.UtcNow;
                
                // Create client activity log
                var activity = new ClientPortalActivity
                {
                    CustomerId = customerId,
                    ActivityType = "Recurring Payment Update",
                    Description = $"Updated recurring {recurringPayment.PaymentType} settings",
                    ActivityDate = DateTime.UtcNow,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.ClientPortalActivities.Add(activity);
                await _dbContext.SaveChangesAsync();
                
                var dto = new RecurringPaymentDto
                {
                    Amount = recurringPayment.Amount,
                    Frequency = recurringPayment.Frequency,
                    StartDate = recurringPayment.StartDate,
                    EndDate = recurringPayment.EndDate,
                    Description = recurringPayment.Description,
                    PaymentType = recurringPayment.PaymentType,
                    // Populate other fields as necessary
                };

                return new FinTech.Core.Application.Common.Models.BaseResponse<RecurringPaymentDto> { Data = dto, Success = true, Message = "Recurring payment updated successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recurring payment {RecurringPaymentId} for customer {CustomerId}", recurringPaymentId, customerId);
                  return new FinTech.Core.Application.Common.Models.BaseResponse<RecurringPaymentDto> { Success = false, Message = "Error updating recurring payment" };
            }
        }

        // Payment History
        public async Task<IEnumerable<PaymentTransaction>> GetPaymentHistoryAsync(PaymentHistoryRequestDto requestDto, Guid customerId)
        {
            try
            {
                // Get customer's deposit accounts
                var accountIds = await _dbContext.DepositAccounts
                    .Where(a => a.CustomerId == customerId)
                    .Select(a => a.Id)
                    .ToListAsync();
                
                if (!accountIds.Any())
                {
                    return new List<PaymentTransaction>();
                }
                
                var accountGuids = accountIds.Select(id => Guid.Parse(id)).ToList();

                // Base query for transactions
                var query = _dbContext.DepositTransactions
                    .Where(t => accountGuids.Contains(t.AccountId))
                    .Where(t => t.TransactionCategory == "Fund Transfer" || 
                               t.TransactionCategory == "Bill Payment" || 
                               t.TransactionCategory == "Recurring Payment");
                
                // Apply filters if provided
                if (requestDto.FromDate.HasValue)
                {
                    query = query.Where(t => t.TransactionDate >= requestDto.FromDate.Value);
                }
                
                if (requestDto.ToDate.HasValue)
                {
                    query = query.Where(t => t.TransactionDate <= requestDto.ToDate.Value);
                }
                
                if (!string.IsNullOrEmpty(requestDto.TransactionType))
                {
                    query = query.Where(t => t.TransactionCategory == requestDto.TransactionType);
                }
                
                // FinTech Best Practice: Convert enum to string for comparison
                if (!string.IsNullOrEmpty(requestDto.Status))
                {
                    query = query.Where(t => t.Status.ToString() == requestDto.Status);
                }
                
                if (requestDto.MinAmount.HasValue)
                {
                    query = query.Where(t => t.Amount >= requestDto.MinAmount.Value);
                }
                
                if (requestDto.MaxAmount.HasValue)
                {
                    query = query.Where(t => t.Amount <= requestDto.MaxAmount.Value);
                }
                
                // Get paginated results
                var transactions = await query
                    .OrderByDescending(t => t.TransactionDate)
                    .Skip((requestDto.Page - 1) * requestDto.PageSize)
                    .Take(requestDto.PageSize)
                    .ToListAsync();
                
                // Map to response type
                var result = new List<PaymentTransaction>();
                
                foreach (var transaction in transactions)
                {
                    // FinTech Best Practice: Convert Guid to string for Id comparison
                    var account = await _dbContext.DepositAccounts
                        .FirstOrDefaultAsync(a => a.Id == transaction.AccountId.ToString());
                    
                    var paymentTransaction = new PaymentTransaction
                    {
                        // FinTech Best Practice: Convert string Id to Guid
                        Id = Guid.Parse(transaction.Id),
                        TransactionDate = transaction.TransactionDate,
                        ReferenceNumber = transaction.ReferenceNumber,
                        Description = transaction.Description,
                        Amount = transaction.Amount,
                        TransactionType = transaction.TransactionType.ToString(), // Convert enum to string
                        Category = transaction.TransactionCategory,
                        Status = transaction.Status.ToString(), // Convert enum to string
                        AccountNumber = account?.AccountNumber,
                        // FinTech Best Practice: DepositAccount uses AccountNumber as name
                        AccountName = account?.AccountNumber
                    };
                    
                    // Get additional details based on transaction category
                    if (transaction.TransactionCategory == "Fund Transfer")
                    {
                        var transfer = await _dbContext.ExternalTransfers
                            .FirstOrDefaultAsync(t => t.ReferenceNumber == transaction.ReferenceNumber);
                        
                        if (transfer != null)
                        {
                            paymentTransaction.BeneficiaryName = transfer.BeneficiaryName;
                            paymentTransaction.DestinationAccount = transfer.DestinationAccountNumber;
                            paymentTransaction.DestinationBank = transfer.DestinationBankName;
                        }
                    }
                    else if (transaction.TransactionCategory == "Bill Payment")
                    {
                        var payment = await _dbContext.BillPayments
                            .Include(p => p.Biller)
                            .FirstOrDefaultAsync(p => p.ReferenceNumber == transaction.ReferenceNumber);
                        
                        if (payment != null)
                        {
                            paymentTransaction.BeneficiaryName = payment.Biller?.Name;
                            paymentTransaction.CustomerReference = payment.CustomerReferenceNumber;
                        }
                    }
                    
                    result.Add(paymentTransaction);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment history for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<PaymentTransaction> GetPaymentDetailsAsync(Guid transactionId, Guid customerId)
        {
            try
            {
                // Get transaction
                // FinTech Best Practice: Convert Guid to string for Id comparison
                var transaction = await _dbContext.DepositTransactions
                    .FirstOrDefaultAsync(t => t.Id == transactionId.ToString());
                
                if (transaction == null)
                {
                    throw new KeyNotFoundException($"Transaction with ID {transactionId} not found.");
                }
                
                // Verify customer owns the account
                // FinTech Best Practice: Convert Guid to string for Id comparison
                var account = await _dbContext.DepositAccounts
                    .FirstOrDefaultAsync(a => a.Id == transaction.AccountId.ToString());
                
                if (account == null || account.CustomerId.ToString() != customerId.ToString())
                {
                    throw new UnauthorizedAccessException("You do not have permission to view this transaction.");
                }
                
                // Map to response type
                var paymentTransaction = new PaymentTransaction
                {
                    // FinTech Best Practice: Convert string Id to Guid
                    Id = Guid.Parse(transaction.Id),
                    TransactionDate = transaction.TransactionDate,
                    ReferenceNumber = transaction.ReferenceNumber,
                    Description = transaction.Description,
                    Amount = transaction.Amount,
                    // FinTech Best Practice: Convert enum to string
                    TransactionType = transaction.TransactionType.ToString(),
                    Category = transaction.TransactionCategory,
                    Status = transaction.Status.ToString(),
                    AccountNumber = account.AccountNumber,
                    // FinTech Best Practice: DepositAccount uses AccountNumber as name
                    AccountName = account.AccountNumber
                };
                
                // Get additional details based on transaction category
                if (transaction.TransactionCategory == "Fund Transfer")
                {
                    var transfer = await _dbContext.ExternalTransfers
                        .FirstOrDefaultAsync(t => t.ReferenceNumber == transaction.ReferenceNumber);
                    
                    if (transfer != null)
                    {
                        paymentTransaction.BeneficiaryName = transfer.BeneficiaryName;
                        paymentTransaction.DestinationAccount = transfer.DestinationAccountNumber;
                        paymentTransaction.DestinationBank = transfer.DestinationBankName;
                        paymentTransaction.TransferType = transfer.TransferType;
                        paymentTransaction.ProcessingDate = transfer.ProcessedAt;
                    }
                }
                else if (transaction.TransactionCategory == "Bill Payment")
                {
                    var payment = await _dbContext.BillPayments
                        .Include(p => p.Biller)
                        .FirstOrDefaultAsync(p => p.ReferenceNumber == transaction.ReferenceNumber);
                    
                    if (payment != null)
                    {
                        paymentTransaction.BeneficiaryName = payment.Biller?.Name;
                        paymentTransaction.CustomerReference = payment.CustomerReferenceNumber;
                        paymentTransaction.ProcessingDate = payment.ProcessedAt;
                    }
                }
                
                return paymentTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment details for transaction {TransactionId} for customer {CustomerId}", transactionId, customerId);
                throw;
            }
        }

        // Helper method to generate reference numbers
        private string GenerateReferenceNumber()
        {
            return $"TRF{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
        }





        // Missing interface implementations (Remaining valid known stubs)
        public Task<FinTech.Core.Application.Common.Models.BaseResponse<List<BillPaymentDto>>> GetRecentBillPaymentsAsync(Guid customerId, int count = 5) => throw new NotImplementedException();
        public Task<FinTech.Core.Application.Common.Models.BaseResponse<List<TransferDto>>> GetRecentTransfersAsync(Guid customerId, int count = 5) => throw new NotImplementedException();
        public Task<FinTech.Core.Application.Common.Models.BaseResponse<RecurringPaymentDto>> GetRecurringPaymentDetailsAsync(Guid customerId, Guid paymentId) => throw new NotImplementedException();
        public Task<FinTech.Core.Application.Common.Models.BaseResponse<RecurringPaymentDto>> CreateRecurringPaymentAsync(Guid customerId, RecurringPaymentCreateDto paymentDto) => throw new NotImplementedException();
        public Task<FinTech.Core.Application.Common.Models.BaseResponse<List<BillerDto>>> GetAvailableBillersAsync() => throw new NotImplementedException();
        public Task<FinTech.Core.Application.Common.Models.BaseResponse<SavedPayeeDto>> CreateSavedPayeeAsync(Guid customerId, SavedPayeeCreateDto payeeDto) => throw new NotImplementedException();
        public Task<FinTech.Core.Application.Common.Models.BaseResponse<SavedPayeeDto>> UpdateSavedPayeeAsync(Guid customerId, Guid payeeId, SavedPayeeUpdateDto payeeDto) => throw new NotImplementedException();
        public Task<FinTech.Core.Application.Common.Models.BaseResponse<bool>> DeleteSavedPayeeAsync(Guid customerId, Guid payeeId) => throw new NotImplementedException();
    }

    public class PaymentTransaction
    {
        public Guid Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ReferenceNumber { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string BeneficiaryName { get; set; }
        public string DestinationAccount { get; set; }
        public string DestinationBank { get; set; }
        public string CustomerReference { get; set; }
        public string TransferType { get; set; }
        public DateTime? ProcessingDate { get; set; }
    }
}
