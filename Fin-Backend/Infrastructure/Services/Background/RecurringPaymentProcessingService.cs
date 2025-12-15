using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.Services;
using FinTech.Core.Domain.Entities.ClientPortal;
using FinTech.Core.Application.Interfaces;
using FinTech.Infrastructure.Data;

namespace FinTech.Infrastructure.Services.Background
{
    public class RecurringPaymentProcessingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RecurringPaymentProcessingService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1);

        public RecurringPaymentProcessingService(
            IServiceProvider serviceProvider,
            ILogger<RecurringPaymentProcessingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Recurring Payment Processing Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRecurringPaymentsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during recurring payment processing");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Recurring Payment Processing Service stopped");
        }

        private async Task ProcessRecurringPaymentsAsync()
        {
            _logger.LogInformation("Starting recurring payment processing");

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                var paymentService = scope.ServiceProvider.GetRequiredService<IClientPaymentService>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var now = DateTime.UtcNow.Date;

                // Find due recurring payments
                var duePayments = await dbContext.RecurringPayments
                    .Where(p => p.Status == "Active" && p.NextPaymentDate.Date <= now)
                    .ToListAsync();

                if (duePayments.Any())
                {
                    _logger.LogInformation("Found {Count} recurring payments due for processing", duePayments.Count);

                    foreach (var payment in duePayments)
                    {
                        try
                        {
                            // Process the payment
                            var result = await ProcessPaymentAsync(payment, paymentService);

                            // Create history record
                            var historyRecord = new RecurringPaymentHistory
                            {
                                RecurringPaymentId = payment.Id,
                                ProcessedDate = DateTime.UtcNow,
                                Amount = payment.Amount,
                                Status = result.Success ? "Success" : "Failed",
                                ReferenceNumber = result.TransactionReference,
                                ErrorMessage = result.Success ? null : result.ErrorMessage
                            };

                            dbContext.RecurringPaymentHistory.Add(historyRecord);

                            // Update next payment date based on frequency
                            payment.LastPaymentDate = DateTime.UtcNow;
                            payment.NextPaymentDate = CalculateNextPaymentDate(payment);
                            
                            // If payment is one-time, deactivate it
                            if (payment.Frequency == "OneTime")
                            {
                                payment.Status = "Completed";
                            }

                            // Send notification to customer
                            await SendPaymentNotificationAsync(
                                notificationService, 
                                payment.CustomerId, 
                                payment, 
                                result.Success, 
                                result.ErrorMessage
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing recurring payment {PaymentId}", payment.Id);
                        }
                    }

                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    _logger.LogInformation("No recurring payments due for processing");
                }
            }
        }

        private async Task<PaymentResult> ProcessPaymentAsync(RecurringPayment payment, IClientPaymentService paymentService)
        {
            try
            {
                // This is a simplified example - actual implementation would depend on payment type
                switch (payment.PaymentType)
                {
                    case "BillPayment":
                        return await paymentService.ProcessBillPaymentAsync(
                            payment.CustomerId,
                            payment.FromAccountId,
                            payment.BillerId,
                            payment.Amount,
                            payment.Reference,
                            true);
                    
                    case "Transfer":
                        return await paymentService.ProcessTransferAsync(
                            payment.CustomerId,
                            payment.FromAccountId,
                            payment.ToAccountId,
                            payment.Amount,
                            payment.Reference,
                            true);
                    
                    case "ExternalTransfer":
                        return await paymentService.ProcessExternalTransferAsync(
                            payment.CustomerId,
                            payment.FromAccountId,
                            payment.BeneficiaryId,
                            payment.Amount,
                            payment.Reference,
                            true);
                    
                    default:
                        return new PaymentResult
                        {
                            Success = false,
                            ErrorMessage = "Unsupported payment type"
                        };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = "Payment processing failed: " + ex.Message
                };
            }
        }

        private DateTime CalculateNextPaymentDate(RecurringPayment payment)
        {
            var baseDate = payment.NextPaymentDate;
            
            switch (payment.Frequency)
            {
                case "Daily":
                    return baseDate.AddDays(1);
                
                case "Weekly":
                    return baseDate.AddDays(7);
                
                case "Biweekly":
                    return baseDate.AddDays(14);
                
                case "Monthly":
                    return baseDate.AddMonths(1);
                
                case "Quarterly":
                    return baseDate.AddMonths(3);
                
                case "SemiAnnually":
                    return baseDate.AddMonths(6);
                
                case "Annually":
                    return baseDate.AddYears(1);
                
                case "OneTime":
                default:
                    return DateTime.MaxValue; // No next date for one-time payments
            }
        }

        private async Task SendPaymentNotificationAsync(
            INotificationService notificationService,
            Guid customerId,
            RecurringPayment payment,
            bool success,
            string errorMessage)
        {
            try
            {
                string title = success 
                    ? "Recurring Payment Processed" 
                    : "Recurring Payment Failed";
                
                string message = success
                    ? $"Your recurring payment of {payment.Amount} {payment.Currency} for {payment.PaymentName} has been processed successfully."
                    : $"Your recurring payment of {payment.Amount} {payment.Currency} for {payment.PaymentName} failed. Reason: {errorMessage}";
                
                string type = success ? "payment" : "payment_failed";
                
                var metadata = new Dictionary<string, string>
                {
                    { "paymentId", payment.Id.ToString() },
                    { "amount", payment.Amount.ToString() },
                    { "currency", payment.Currency },
                    { "paymentName", payment.PaymentName },
                    { "status", success ? "Success" : "Failed" }
                };
                
                await notificationService.SendNotificationAsync(
                    customerId,
                    title,
                    message,
                    type,
                    metadata
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment notification");
            }
        }
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string TransactionReference { get; set; }
        public string ErrorMessage { get; set; }
    }
}
