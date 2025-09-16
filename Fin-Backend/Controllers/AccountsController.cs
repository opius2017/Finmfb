using Fin_Backend.Infrastructure.Documentation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace Fin_Backend.Controllers
{
    /// <summary>
    /// API controller for managing customer accounts
    /// </summary>
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [SwaggerTag("Customer Account Management")]
    public class AccountsController : ApiControllerBase
    {
        /// <summary>
        /// Gets a list of all accounts for the authenticated customer
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/accounts
        ///     
        /// </remarks>
        /// <returns>A list of customer accounts</returns>
        /// <response code="200">Returns the list of accounts</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet]
        [Authorize]
        [SwaggerOperation(
            Summary = "Gets all customer accounts",
            Description = "Retrieves all accounts owned by the authenticated customer",
            OperationId = "Accounts_GetAll",
            Tags = new[] { "Accounts" })]
        [SwaggerResponse(StatusCodes.Status200OK, "The list of accounts", typeof(ApiResponse<List<AccountDto>>))]
        public async Task<ActionResult<ApiResponse<List<AccountDto>>>> GetAllAccounts()
        {
            // Simulated data for documentation purposes
            var accounts = new List<AccountDto>
            {
                new AccountDto
                {
                    AccountId = Guid.NewGuid().ToString(),
                    AccountNumber = "1234567890",
                    AccountName = "Savings Account",
                    AccountType = "Savings",
                    Balance = 1000.00m,
                    Currency = "NGN",
                    Status = "Active"
                }
            };

            return Success(accounts);
        }

        /// <summary>
        /// Gets an account by its ID
        /// </summary>
        /// <param name="id">The account ID</param>
        /// <returns>The account details</returns>
        /// <response code="200">Returns the account</response>
        /// <response code="404">If the account is not found</response>
        [HttpGet("{id}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Gets an account by ID",
            Description = "Retrieves a specific account by its unique identifier",
            OperationId = "Accounts_GetById",
            Tags = new[] { "Accounts" })]
        [SwaggerResponse(StatusCodes.Status200OK, "The account", typeof(ApiResponse<AccountDto>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Account not found")]
        public async Task<ActionResult<ApiResponse<AccountDto>>> GetAccountById(string id)
        {
            // Simulated data for documentation purposes
            var account = new AccountDto
            {
                AccountId = id,
                AccountNumber = "1234567890",
                AccountName = "Savings Account",
                AccountType = "Savings",
                Balance = 1000.00m,
                Currency = "NGN",
                Status = "Active"
            };

            return Success(account);
        }

        /// <summary>
        /// Creates a new account
        /// </summary>
        /// <param name="request">The account creation request</param>
        /// <returns>The newly created account</returns>
        /// <response code="201">Returns the newly created account</response>
        /// <response code="400">If the request is invalid</response>
        [HttpPost]
        [Authorize]
        [SwaggerOperation(
            Summary = "Creates a new account",
            Description = "Creates a new account for the authenticated customer",
            OperationId = "Accounts_Create",
            Tags = new[] { "Accounts" })]
        [SwaggerResponse(StatusCodes.Status201Created, "The newly created account", typeof(ApiResponse<AccountDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        public async Task<ActionResult<ApiResponse<AccountDto>>> CreateAccount([FromBody] CreateAccountRequest request)
        {
            // Simulated data for documentation purposes
            var account = new AccountDto
            {
                AccountId = Guid.NewGuid().ToString(),
                AccountNumber = "9876543210",
                AccountName = request.AccountName,
                AccountType = request.AccountType,
                Balance = 0.00m,
                Currency = request.Currency,
                Status = "Active"
            };

            return CreatedAtAction(nameof(GetAccountById), new { id = account.AccountId }, Success(account));
        }

        /// <summary>
        /// Updates an existing account
        /// </summary>
        /// <param name="id">The account ID</param>
        /// <param name="request">The account update request</param>
        /// <returns>The updated account</returns>
        /// <response code="200">Returns the updated account</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="404">If the account is not found</response>
        [HttpPut("{id}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Updates an account",
            Description = "Updates an existing account's details",
            OperationId = "Accounts_Update",
            Tags = new[] { "Accounts" })]
        [SwaggerResponse(StatusCodes.Status200OK, "The updated account", typeof(ApiResponse<AccountDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Account not found")]
        public async Task<ActionResult<ApiResponse<AccountDto>>> UpdateAccount(string id, [FromBody] UpdateAccountRequest request)
        {
            // Simulated data for documentation purposes
            var account = new AccountDto
            {
                AccountId = id,
                AccountNumber = "1234567890",
                AccountName = request.AccountName,
                AccountType = "Savings",
                Balance = 1000.00m,
                Currency = "NGN",
                Status = "Active"
            };

            return Success(account);
        }

        /// <summary>
        /// Closes an account
        /// </summary>
        /// <param name="id">The account ID</param>
        /// <returns>A success message</returns>
        /// <response code="200">Returns a success message</response>
        /// <response code="404">If the account is not found</response>
        [HttpDelete("{id}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Closes an account",
            Description = "Closes an existing account",
            OperationId = "Accounts_Close",
            Tags = new[] { "Accounts" })]
        [SwaggerResponse(StatusCodes.Status200OK, "Account closed successfully", typeof(ApiResponse<object>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Account not found")]
        public async Task<ActionResult<ApiResponse<object>>> CloseAccount(string id)
        {
            return Success("Account closed successfully");
        }

        /// <summary>
        /// Gets the transaction history for an account
        /// </summary>
        /// <param name="id">The account ID</param>
        /// <param name="startDate">Optional start date for filtering</param>
        /// <param name="endDate">Optional end date for filtering</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <returns>A list of transactions</returns>
        /// <response code="200">Returns the transaction history</response>
        /// <response code="404">If the account is not found</response>
        [HttpGet("{id}/transactions")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Gets account transaction history",
            Description = "Retrieves the transaction history for a specific account",
            OperationId = "Accounts_GetTransactions",
            Tags = new[] { "Accounts", "Transactions" })]
        [SwaggerResponse(StatusCodes.Status200OK, "The transaction history", typeof(ApiResponse<PagedResult<TransactionDto>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Account not found")]
        public async Task<ActionResult<ApiResponse<PagedResult<TransactionDto>>>> GetTransactions(
            string id,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // Simulated data for documentation purposes
            var transactions = new List<TransactionDto>
            {
                new TransactionDto
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    AccountId = id,
                    TransactionType = "Credit",
                    Amount = 500.00m,
                    Currency = "NGN",
                    Description = "Salary payment",
                    TransactionDate = DateTime.UtcNow.AddDays(-1),
                    Status = "Completed"
                }
            };

            var result = new PagedResult<TransactionDto>
            {
                Items = transactions,
                TotalCount = 1,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = 1
            };

            return Success(result);
        }
    }

    /// <summary>
    /// Account information
    /// </summary>
    public class AccountDto
    {
        /// <summary>
        /// Unique identifier for the account
        /// </summary>
        /// <example>a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6</example>
        public string AccountId { get; set; }

        /// <summary>
        /// Account number
        /// </summary>
        /// <example>1234567890</example>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Name of the account
        /// </summary>
        /// <example>Savings Account</example>
        public string AccountName { get; set; }

        /// <summary>
        /// Type of account
        /// </summary>
        /// <example>Savings</example>
        public string AccountType { get; set; }

        /// <summary>
        /// Current balance
        /// </summary>
        /// <example>1000.00</example>
        public decimal Balance { get; set; }

        /// <summary>
        /// Currency code
        /// </summary>
        /// <example>NGN</example>
        public string Currency { get; set; }

        /// <summary>
        /// Account status
        /// </summary>
        /// <example>Active</example>
        public string Status { get; set; }
    }

    /// <summary>
    /// Request to create a new account
    /// </summary>
    public class CreateAccountRequest
    {
        /// <summary>
        /// Name of the account
        /// </summary>
        /// <example>My Savings Account</example>
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string AccountName { get; set; }

        /// <summary>
        /// Type of account
        /// </summary>
        /// <example>Savings</example>
        [Required]
        [StringLength(50)]
        public string AccountType { get; set; }

        /// <summary>
        /// Currency code
        /// </summary>
        /// <example>NGN</example>
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; }
    }

    /// <summary>
    /// Request to update an account
    /// </summary>
    public class UpdateAccountRequest
    {
        /// <summary>
        /// Name of the account
        /// </summary>
        /// <example>My Updated Savings Account</example>
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string AccountName { get; set; }
    }

    /// <summary>
    /// Transaction information
    /// </summary>
    public class TransactionDto
    {
        /// <summary>
        /// Unique identifier for the transaction
        /// </summary>
        /// <example>t1u2v3w4-x5y6-7z8a-9b0c-d1e2f3g4h5i6</example>
        public string TransactionId { get; set; }

        /// <summary>
        /// Account ID
        /// </summary>
        /// <example>a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6</example>
        public string AccountId { get; set; }

        /// <summary>
        /// Type of transaction
        /// </summary>
        /// <example>Credit</example>
        public string TransactionType { get; set; }

        /// <summary>
        /// Transaction amount
        /// </summary>
        /// <example>500.00</example>
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency code
        /// </summary>
        /// <example>NGN</example>
        public string Currency { get; set; }

        /// <summary>
        /// Transaction description
        /// </summary>
        /// <example>Salary payment</example>
        public string Description { get; set; }

        /// <summary>
        /// Date and time of the transaction
        /// </summary>
        /// <example>2025-09-15T14:30:00Z</example>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Transaction status
        /// </summary>
        /// <example>Completed</example>
        public string Status { get; set; }
    }

    /// <summary>
    /// A paged result of items
    /// </summary>
    /// <typeparam name="T">Type of items</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Items in the current page
        /// </summary>
        public List<T> Items { get; set; }

        /// <summary>
        /// Total number of items across all pages
        /// </summary>
        /// <example>100</example>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        /// <example>1</example>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        /// <example>20</example>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        /// <example>5</example>
        public int TotalPages { get; set; }
    }
}