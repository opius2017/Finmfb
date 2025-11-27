using FinTech.Infrastructure.Messaging.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FinTech.Infrastructure.Messaging.Handlers
{
    /// <summary>
    /// Handler for the payment processed integration event
    /// </summary>
    public class PaymentProcessedIntegrationEventHandler : IIntegrationEventHandler<PaymentProcessedIntegrationEvent>
    {
        private readonly ILogger<PaymentProcessedIntegrationEventHandler> _logger;
        private readonly ICustomerNotificationService _notificationService;
        private readonly IAccountService _accountService;

        /// <summary>
        /// Creates a new payment processed integration event handler
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="notificationService">The notification service</param>
        /// <param name="accountService">The account service</param>
        public PaymentProcessedIntegrationEventHandler(
            ILogger<PaymentProcessedIntegrationEventHandler> logger,
            ICustomerNotificationService notificationService,
            IAccountService accountService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        /// <summary>
        /// Handles the payment processed integration event
        /// </summary>
        /// <param name="event">The integration event</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task HandleAsync(PaymentProcessedIntegrationEvent @event)
        {
            _logger.LogInformation("Handling payment processed integration event: {PaymentId}", @event.PaymentId);

            try
            {
                // Update account balance
                await _accountService.UpdateAccountBalanceAsync(@event.AccountId, @event.Amount, @event.TransactionReference);

                // Send notification to customer
                var message = $"Your payment of {@event.Amount} {@event.Currency} has been processed successfully. Reference: {@event.TransactionReference}";
                await _notificationService.SendPaymentNotificationAsync(@event.AccountId, message, @event.PaymentId);

                _logger.LogInformation("Payment processed integration event handled successfully: {PaymentId}", @event.PaymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment processed integration event: {PaymentId}", @event.PaymentId);
                throw;
            }
        }
    }

    /// <summary>
    /// Handler for the account created integration event
    /// </summary>
    public class AccountCreatedIntegrationEventHandler : IIntegrationEventHandler<AccountCreatedIntegrationEvent>
    {
        private readonly ILogger<AccountCreatedIntegrationEventHandler> _logger;
        private readonly ICustomerNotificationService _notificationService;
        private readonly IReportingService _reportingService;

        /// <summary>
        /// Creates a new account created integration event handler
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="notificationService">The notification service</param>
        /// <param name="reportingService">The reporting service</param>
        public AccountCreatedIntegrationEventHandler(
            ILogger<AccountCreatedIntegrationEventHandler> logger,
            ICustomerNotificationService notificationService,
            IReportingService reportingService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _reportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        /// <summary>
        /// Handles the account created integration event
        /// </summary>
        /// <param name="event">The integration event</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task HandleAsync(AccountCreatedIntegrationEvent @event)
        {
            _logger.LogInformation("Handling account created integration event: {AccountId}", @event.AccountId);

            try
            {
                // Send welcome notification to customer
                var message = $"Welcome to FinTech! Your new {@event.AccountType} account ({@event.AccountNumber}) has been created successfully.";
                await _notificationService.SendAccountNotificationAsync(@event.CustomerId, message, @event.AccountId);

                // Update reporting system
                await _reportingService.RecordNewAccountAsync(
                    @event.AccountId,
                    @event.CustomerId,
                    @event.AccountType,
                    @event.Currency,
                    @event.OpenDate);

                _logger.LogInformation("Account created integration event handled successfully: {AccountId}", @event.AccountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling account created integration event: {AccountId}", @event.AccountId);
                throw;
            }
        }
    }

    /// <summary>
    /// Handler for the loan approved integration event
    /// </summary>
    public class LoanApprovedIntegrationEventHandler : IIntegrationEventHandler<LoanApprovedIntegrationEvent>
    {
        private readonly ILogger<LoanApprovedIntegrationEventHandler> _logger;
        private readonly ICustomerNotificationService _notificationService;
        private readonly ILoanDisbursementService _loanDisbursementService;
        private readonly IAccountService _accountService;

        /// <summary>
        /// Creates a new loan approved integration event handler
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="notificationService">The notification service</param>
        /// <param name="loanDisbursementService">The loan disbursement service</param>
        /// <param name="accountService">The account service</param>
        public LoanApprovedIntegrationEventHandler(
            ILogger<LoanApprovedIntegrationEventHandler> logger,
            ICustomerNotificationService notificationService,
            ILoanDisbursementService loanDisbursementService,
            IAccountService accountService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _loanDisbursementService = loanDisbursementService ?? throw new ArgumentNullException(nameof(loanDisbursementService));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        /// <summary>
        /// Handles the loan approved integration event
        /// </summary>
        /// <param name="event">The integration event</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task HandleAsync(LoanApprovedIntegrationEvent @event)
        {
            _logger.LogInformation("Handling loan approved integration event: {LoanId}", @event.LoanId);

            try
            {
                // Disburse the loan
                var disbursementId = await _loanDisbursementService.DisburseLoanAsync(
                    @event.LoanId,
                    @event.AccountId,
                    @event.Amount,
                    @event.Currency);

                // Update account balance
                await _accountService.UpdateAccountBalanceAsync(
                    @event.AccountId,
                    @event.Amount,
                    $"LOAN/{@event.LoanId}/DISBURSEMENT");

                // Send notification to customer
                var message = $"Congratulations! Your loan of {@event.Amount} {@event.Currency} has been approved and disbursed to your account. " +
                              $"Your first payment is due on {@event.FirstPaymentDate:yyyy-MM-dd}.";
                await _notificationService.SendLoanNotificationAsync(@event.CustomerId, message, @event.LoanId);

                _logger.LogInformation("Loan approved integration event handled successfully: {LoanId}", @event.LoanId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling loan approved integration event: {LoanId}", @event.LoanId);
                throw;
            }
        }
    }

    /// <summary>
    /// Interface for customer notification service
    /// </summary>
    public interface ICustomerNotificationService
    {
        /// <summary>
        /// Sends a payment notification to a customer
        /// </summary>
        /// <param name="accountId">The account ID</param>
        /// <param name="message">The notification message</param>
        /// <param name="paymentId">The payment ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SendPaymentNotificationAsync(string accountId, string message, string paymentId);
        
        /// <summary>
        /// Sends an account notification to a customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="message">The notification message</param>
        /// <param name="accountId">The account ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SendAccountNotificationAsync(string customerId, string message, string accountId);
        
        /// <summary>
        /// Sends a loan notification to a customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="message">The notification message</param>
        /// <param name="loanId">The loan ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SendLoanNotificationAsync(string customerId, string message, string loanId);
    }

    /// <summary>
    /// Interface for account service
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Updates an account balance
        /// </summary>
        /// <param name="accountId">The account ID</param>
        /// <param name="amount">The amount</param>
        /// <param name="reference">The reference</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task UpdateAccountBalanceAsync(string accountId, decimal amount, string reference);
    }

    /// <summary>
    /// Interface for reporting service
    /// </summary>
    public interface IReportingService
    {
        /// <summary>
        /// Records a new account in the reporting system
        /// </summary>
        /// <param name="accountId">The account ID</param>
        /// <param name="customerId">The customer ID</param>
        /// <param name="accountType">The account type</param>
        /// <param name="currency">The currency</param>
        /// <param name="openDate">The open date</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task RecordNewAccountAsync(string accountId, string customerId, string accountType, string currency, DateTime openDate);
    }

    /// <summary>
    /// Interface for loan disbursement service
    /// </summary>
    public interface ILoanDisbursementService
    {
        /// <summary>
        /// Disburses a loan
        /// </summary>
        /// <param name="loanId">The loan ID</param>
        /// <param name="accountId">The account ID</param>
        /// <param name="amount">The amount</param>
        /// <param name="currency">The currency</param>
        /// <returns>The disbursement ID</returns>
        Task<string> DisburseLoanAsync(string loanId, string accountId, decimal amount, string currency);
    }
}
