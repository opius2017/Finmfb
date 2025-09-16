using Fin_Backend.Infrastructure.Documentation;
using Fin_Backend.Infrastructure.Messaging;
using Fin_Backend.Infrastructure.Messaging.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Payment;
using FinTech.Core.Application.DTOs.Account;

namespace Fin_Backend.Controllers
{
    /// <summary>
    /// Controller for handling payment operations
    /// </summary>
    [ApiVersion("1.0")]
    [SwaggerTag("Payment Operations")]
    public class PaymentsController : ApiControllerBase
    {
        private readonly IIntegrationEventOutboxService _outboxService;
        private readonly ILogger<PaymentsController> _logger;

        /// <summary>
        /// Creates a new payments controller
        /// </summary>
        /// <param name="outboxService">The integration event outbox service</param>
        /// <param name="logger">The logger</param>
        public PaymentsController(
            IIntegrationEventOutboxService outboxService,
            ILogger<PaymentsController> logger)
        {
            _outboxService = outboxService ?? throw new ArgumentNullException(nameof(outboxService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Processes a payment
        /// </summary>
        /// <param name="request">The payment request</param>
        /// <returns>The payment result</returns>
        [HttpPost]
        [Authorize]
        [SwaggerOperation(
            Summary = "Processes a payment",
            Description = "Processes a payment and publishes a payment processed event",
            OperationId = "Payments_Process",
            Tags = new[] { "Payments" })]
        [SwaggerResponse(StatusCodes.Status200OK, "The payment was processed successfully", typeof(ApiResponse<PaymentResultDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The payment request is invalid")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "The user is not authenticated")]
        public async Task<ActionResult<ApiResponse<PaymentResultDto>>> ProcessPayment([FromBody] PaymentRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Simulate payment processing
            var paymentId = Guid.NewGuid().ToString();
            var transactionReference = $"PMT-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(100000, 999999)}";
            var paymentDate = DateTime.UtcNow;

            // Create payment processed event
            var paymentProcessedEvent = new PaymentProcessedIntegrationEvent(
                paymentId,
                request.AccountId,
                request.Amount,
                request.Currency,
                request.PaymentMethod,
                "Successful",
                transactionReference,
                paymentDate);

            // Save event to outbox
            await _outboxService.SaveEventAsync(paymentProcessedEvent);

            _logger.LogInformation("Payment processed: {PaymentId}, Amount: {Amount} {Currency}, Reference: {Reference}",
                paymentId, request.Amount, request.Currency, transactionReference);

            // Return response
            var result = new PaymentResultDto
            {
                PaymentId = paymentId,
                Status = "Successful",
                TransactionReference = transactionReference,
                ProcessedAt = paymentDate,
                Amount = request.Amount,
                Currency = request.Currency,
                Message = "Payment processed successfully"
            };

            return Success(result);
        }

        /// <summary>
        /// Creates a new account
        /// </summary>
        /// <param name="request">The account creation request</param>
        /// <returns>The account creation result</returns>
        [HttpPost("accounts")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Creates a new account",
            Description = "Creates a new account and publishes an account created event",
            OperationId = "Payments_CreateAccount",
            Tags = new[] { "Accounts" })]
        [SwaggerResponse(StatusCodes.Status200OK, "The account was created successfully", typeof(ApiResponse<AccountCreationResultDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The account creation request is invalid")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "The user is not authenticated")]
        public async Task<ActionResult<ApiResponse<AccountCreationResultDto>>> CreateAccount([FromBody] AccountCreationRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Simulate account creation
            var accountId = Guid.NewGuid().ToString();
            var accountNumber = $"{new Random().Next(10000000, 99999999)}";
            var openDate = DateTime.UtcNow;

            // Create account created event
            var accountCreatedEvent = new AccountCreatedIntegrationEvent(
                accountId,
                request.CustomerId,
                accountNumber,
                request.AccountName,
                request.AccountType,
                request.Currency,
                "Active",
                openDate);

            // Save event to outbox
            await _outboxService.SaveEventAsync(accountCreatedEvent);

            _logger.LogInformation("Account created: {AccountId}, Account Number: {AccountNumber}, Type: {AccountType}",
                accountId, accountNumber, request.AccountType);

            // Return response
            var result = new AccountCreationResultDto
            {
                AccountId = accountId,
                AccountNumber = accountNumber,
                AccountName = request.AccountName,
                AccountType = request.AccountType,
                Currency = request.Currency,
                Status = "Active",
                OpenDate = openDate,
                Message = "Account created successfully"
            };

            return Success(result);
        }
    }
}