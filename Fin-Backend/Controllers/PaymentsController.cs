using Fin_Backend.Infrastructure.Documentation;
using Fin_Backend.Infrastructure.Messaging;
using Fin_Backend.Infrastructure.Messaging.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

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

    /// <summary>
    /// Payment request data transfer object
    /// </summary>
    public class PaymentRequestDto
    {
        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        /// <example>a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6</example>
        [Required]
        public string AccountId { get; set; }

        /// <summary>
        /// Gets or sets the payment amount
        /// </summary>
        /// <example>1000.00</example>
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the currency
        /// </summary>
        /// <example>NGN</example>
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the payment method
        /// </summary>
        /// <example>CreditCard</example>
        [Required]
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the payment description
        /// </summary>
        /// <example>Monthly bill payment</example>
        public string Description { get; set; }
    }

    /// <summary>
    /// Payment result data transfer object
    /// </summary>
    public class PaymentResultDto
    {
        /// <summary>
        /// Gets or sets the payment ID
        /// </summary>
        /// <example>a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6</example>
        public string PaymentId { get; set; }

        /// <summary>
        /// Gets or sets the payment status
        /// </summary>
        /// <example>Successful</example>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the transaction reference
        /// </summary>
        /// <example>PMT-20250916-123456</example>
        public string TransactionReference { get; set; }

        /// <summary>
        /// Gets or sets the processed date and time
        /// </summary>
        /// <example>2025-09-16T10:30:00Z</example>
        public DateTime ProcessedAt { get; set; }

        /// <summary>
        /// Gets or sets the payment amount
        /// </summary>
        /// <example>1000.00</example>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the currency
        /// </summary>
        /// <example>NGN</example>
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the result message
        /// </summary>
        /// <example>Payment processed successfully</example>
        public string Message { get; set; }
    }

    /// <summary>
    /// Account creation request data transfer object
    /// </summary>
    public class AccountCreationRequestDto
    {
        /// <summary>
        /// Gets or sets the customer ID
        /// </summary>
        /// <example>c1d2e3f4-g5h6-7i8j-9k0l-m1n2o3p4q5r6</example>
        [Required]
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the account name
        /// </summary>
        /// <example>My Savings Account</example>
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string AccountName { get; set; }

        /// <summary>
        /// Gets or sets the account type
        /// </summary>
        /// <example>Savings</example>
        [Required]
        public string AccountType { get; set; }

        /// <summary>
        /// Gets or sets the currency
        /// </summary>
        /// <example>NGN</example>
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; }
    }

    /// <summary>
    /// Account creation result data transfer object
    /// </summary>
    public class AccountCreationResultDto
    {
        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        /// <example>a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6</example>
        public string AccountId { get; set; }

        /// <summary>
        /// Gets or sets the account number
        /// </summary>
        /// <example>12345678</example>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Gets or sets the account name
        /// </summary>
        /// <example>My Savings Account</example>
        public string AccountName { get; set; }

        /// <summary>
        /// Gets or sets the account type
        /// </summary>
        /// <example>Savings</example>
        public string AccountType { get; set; }

        /// <summary>
        /// Gets or sets the currency
        /// </summary>
        /// <example>NGN</example>
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the account status
        /// </summary>
        /// <example>Active</example>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the open date
        /// </summary>
        /// <example>2025-09-16T10:30:00Z</example>
        public DateTime OpenDate { get; set; }

        /// <summary>
        /// Gets or sets the result message
        /// </summary>
        /// <example>Account created successfully</example>
        public string Message { get; set; }
    }
}