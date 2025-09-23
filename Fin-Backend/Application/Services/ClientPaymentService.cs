using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using FinTech.Domain.Entities.Deposits;
using FinTech.Domain.Entities.ClientPortal;
using FinTech.Application.DTOs.ClientPortal;
using FinTech.Core.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTech.Application.Services
{
    public interface IClientPaymentService
    {
        // Fund Transfers
        Task<TransferResult> TransferFundsAsync(FundTransferDto transferDto, Guid customerId);
        Task<IEnumerable<SavedTransferTemplate>> GetSavedTransferTemplatesAsync(Guid customerId);
        Task<SavedTransferTemplate> SaveTransferTemplateAsync(SaveTransferTemplateDto templateDto, Guid customerId);
        Task<bool> DeleteTransferTemplateAsync(Guid templateId, Guid customerId);
        
        // Bill Payments
        Task<PaymentResult> PayBillAsync(BillPaymentDto paymentDto, Guid customerId);
        Task<IEnumerable<SavedPayee>> GetSavedPayeesAsync(Guid customerId);
        Task<SavedPayee> SavePayeeAsync(SavePayeeDto payeeDto, Guid customerId);
        Task<bool> DeletePayeeAsync(Guid payeeId, Guid customerId);
        Task<IEnumerable<BillerInfo>> GetBillerDirectoryAsync();
        
        // Recurring Payments
        Task<RecurringPayment> ScheduleRecurringPaymentAsync(RecurringPaymentDto recurringDto, Guid customerId);
        Task<IEnumerable<RecurringPayment>> GetRecurringPaymentsAsync(Guid customerId);
        Task<bool> CancelRecurringPaymentAsync(Guid recurringPaymentId, Guid customerId);
        Task<bool> UpdateRecurringPaymentAsync(Guid recurringPaymentId, RecurringPaymentUpdateDto updateDto, Guid customerId);
        
        // Payment History
        Task<IEnumerable<PaymentTransaction>> GetPaymentHistoryAsync(PaymentHistoryRequestDto requestDto, Guid customerId);
        Task<PaymentTransaction> GetPaymentDetailsAsync(Guid transactionId, Guid customerId);
    }

    public class ClientPaymentService : IClientPaymentService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ILogger<ClientPaymentService> _logger;

        public ClientPaymentService(IApplicationDbContext dbContext, ILogger<ClientPaymentService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
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
                if (sourceAccount.Status != "Active")
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
                    
                    if (destinationAccount.Status != "Active")
                    {
                        throw new InvalidOperationException("Destination account is not active.");
                    }
                }

                // Create source account transaction (debit)
                var debitTransaction = new DepositTransaction
                {
                    AccountId = sourceAccount.Id,
                    TransactionType = "Debit",
                    Amount = transferDto.Amount,
                    Description = transferDto.Description ?? $"Transfer to {transferDto.DestinationAccountNumber}",
                    ReferenceNumber = GenerateReferenceNumber(),
                    TransactionDate = DateTime.UtcNow,
                    ValueDate = DateTime.UtcNow,
                    Status = "Pending",
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
                        AccountId = destinationAccount.Id,
                        TransactionType = "Credit",
                        Amount = transferDto.Amount,
                        Description = transferDto.Description ?? $"Transfer from {transferDto.SourceAccountNumber}",
                        ReferenceNumber = debitTransaction.ReferenceNumber, // Use same reference for easy reconciliation
                        TransactionDate = DateTime.UtcNow,
                        ValueDate = DateTime.UtcNow,
                        Status = "Pending",
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
                        SourceAccountId = sourceAccount.Id,
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
                    .FirstOrDefaultAsync(t => t.Id == templateId);
                
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
                if (sourceAccount.Status != "Active")
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
                    .FirstOrDefaultAsync(b => b.Id == paymentDto.BillerId);
                
                if (biller == null)
                {
                    throw new KeyNotFoundException($"Biller with ID {paymentDto.BillerId} not found.");
                }

                // Create source account transaction (debit)
                var debitTransaction = new DepositTransaction
                {
                    AccountId = sourceAccount.Id,
                    TransactionType = "Debit",
                    Amount = paymentDto.Amount,
                    Description = paymentDto.Description ?? $"Payment to {biller.Name}",
                    ReferenceNumber = GenerateReferenceNumber(),
                    TransactionDate = DateTime.UtcNow,
                    ValueDate = DateTime.UtcNow,
                    Status = "Pending",
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
                    AccountId = sourceAccount.Id,
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

        public async Task<IEnumerable<SavedPayee>> GetSavedPayeesAsync(Guid customerId)
        {
            try
            {
                return await _dbContext.SavedPayees
                    .Where(p => p.CustomerId == customerId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving saved payees for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<SavedPayee> SavePayeeAsync(SavePayeeDto payeeDto, Guid customerId)
        {
            try
            {
                // Validate biller exists
                var biller = await _dbContext.Billers
                    .FirstOrDefaultAsync(b => b.Id == payeeDto.BillerId);
                
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
                    .FirstOrDefaultAsync(p => p.Id == payeeId);
                
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
                if (sourceAccount.Status != "Active")
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
                        .FirstOrDefaultAsync(b => b.Id == recurringDto.BillerId);
                    
                    if (biller == null)
                    {
                        throw new KeyNotFoundException($"Biller with ID {recurringDto.BillerId} not found.");
                    }
                }
                
                var recurringPayment = new RecurringPayment
                {
                    CustomerId = customerId,
                    SourceAccountId = sourceAccount.Id,
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

        public async Task<IEnumerable<RecurringPayment>> GetRecurringPaymentsAsync(Guid customerId)
        {
            try
            {
                return await _dbContext.RecurringPayments
                    .Where(p => p.CustomerId == customerId)
                    .Include(p => p.SourceAccount)
                    .OrderBy(p => p.Status)
                    .ThenBy(p => p.NextExecutionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recurring payments for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> CancelRecurringPaymentAsync(Guid recurringPaymentId, Guid customerId)
        {
            try
            {
                var recurringPayment = await _dbContext.RecurringPayments
                    .FirstOrDefaultAsync(p => p.Id == recurringPaymentId);
                
                if (recurringPayment == null)
                {
                    throw new KeyNotFoundException($"Recurring payment with ID {recurringPaymentId} not found.");
                }
                
                if (recurringPayment.CustomerId != customerId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to cancel this recurring payment.");
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
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling recurring payment {RecurringPaymentId} for customer {CustomerId}", recurringPaymentId, customerId);
                throw;
            }
        }

        public async Task<bool> UpdateRecurringPaymentAsync(Guid recurringPaymentId, RecurringPaymentUpdateDto updateDto, Guid customerId)
        {
            try
            {
                var recurringPayment = await _dbContext.RecurringPayments
                    .FirstOrDefaultAsync(p => p.Id == recurringPaymentId);
                
                if (recurringPayment == null)
                {
                    throw new KeyNotFoundException($"Recurring payment with ID {recurringPaymentId} not found.");
                }
                
                if (recurringPayment.CustomerId != customerId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to update this recurring payment.");
                }
                
                // Cannot update cancelled payments
                if (recurringPayment.Status == "Cancelled")
                {
                    throw new InvalidOperationException("Cannot update a cancelled recurring payment.");
                }
                
                // Validate updates
                if (updateDto.Amount.HasValue)
                {
                    recurringPayment.Amount = updateDto.Amount.Value;
                }
                
                if (updateDto.Frequency != null)
                {
                    recurringPayment.Frequency = updateDto.Frequency;
                }
                
                if (updateDto.StartDate.HasValue)
                {
                    // Cannot change start date if already executed
                    if (recurringPayment.LastExecutionDate.HasValue)
                    {
                        throw new InvalidOperationException("Cannot change start date for a recurring payment that has already been executed.");
                    }
                    
                    // Validate start date is in the future
                    if (updateDto.StartDate.Value.Date < DateTime.UtcNow.Date)
                    {
                        throw new InvalidOperationException("Start date must be in the future.");
                    }
                    
                    recurringPayment.StartDate = updateDto.StartDate.Value;
                    recurringPayment.NextExecutionDate = updateDto.StartDate.Value;
                }
                
                if (updateDto.EndDate.HasValue)
                {
                    // Validate end date is after start date
                    if (updateDto.EndDate.Value < recurringPayment.StartDate)
                    {
                        throw new InvalidOperationException("End date must be after start date.");
                    }
                    
                    recurringPayment.EndDate = updateDto.EndDate;
                }
                
                if (updateDto.Description != null)
                {
                    recurringPayment.Description = updateDto.Description;
                }
                
                if (updateDto.Status != null)
                {
                    // Only allow changing between Active and Paused
                    if (updateDto.Status != "Active" && updateDto.Status != "Paused")
                    {
                        throw new InvalidOperationException("Status can only be changed to Active or Paused.");
                    }
                    
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
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recurring payment {RecurringPaymentId} for customer {CustomerId}", recurringPaymentId, customerId);
                throw;
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
                
                // Base query for transactions
                var query = _dbContext.DepositTransactions
                    .Where(t => accountIds.Contains(t.AccountId))
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
                
                if (!string.IsNullOrEmpty(requestDto.Status))
                {
                    query = query.Where(t => t.Status == requestDto.Status);
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
                    var account = await _dbContext.DepositAccounts
                        .FirstOrDefaultAsync(a => a.Id == transaction.AccountId);
                    
                    var paymentTransaction = new PaymentTransaction
                    {
                        Id = transaction.Id,
                        TransactionDate = transaction.TransactionDate,
                        ReferenceNumber = transaction.ReferenceNumber,
                        Description = transaction.Description,
                        Amount = transaction.Amount,
                        TransactionType = transaction.TransactionType,
                        Category = transaction.TransactionCategory,
                        Status = transaction.Status,
                        AccountNumber = account?.AccountNumber,
                        AccountName = account?.AccountName
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
                var transaction = await _dbContext.DepositTransactions
                    .FirstOrDefaultAsync(t => t.Id == transactionId);
                
                if (transaction == null)
                {
                    throw new KeyNotFoundException($"Transaction with ID {transactionId} not found.");
                }
                
                // Verify customer owns the account
                var account = await _dbContext.DepositAccounts
                    .FirstOrDefaultAsync(a => a.Id == transaction.AccountId);
                
                if (account == null || account.CustomerId != customerId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to view this transaction.");
                }
                
                // Map to response type
                var paymentTransaction = new PaymentTransaction
                {
                    Id = transaction.Id,
                    TransactionDate = transaction.TransactionDate,
                    ReferenceNumber = transaction.ReferenceNumber,
                    Description = transaction.Description,
                    Amount = transaction.Amount,
                    TransactionType = transaction.TransactionType,
                    Category = transaction.TransactionCategory,
                    Status = transaction.Status,
                    AccountNumber = account.AccountNumber,
                    AccountName = account.AccountName
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
    }

    // Result classes
    public class TransferResult
    {
        public bool Success { get; set; }
        public string ReferenceNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string SourceAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string ReferenceNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string BillerName { get; set; }
        public string CustomerReferenceNumber { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }

    public class BillerInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string LogoUrl { get; set; }
        public bool RequiresCustomerReference { get; set; }
        public string ReferenceNumberLabel { get; set; }
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