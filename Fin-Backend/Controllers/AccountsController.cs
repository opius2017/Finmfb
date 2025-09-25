using Fin_Backend.Infrastructure.Documentation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Account;
using FinTech.Core.Application.DTOs.Transaction;
using FinTech.Core.Application.DTOs.Common;

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
}
