using Fin_Backend.Infrastructure.Documentation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Fin_Backend.Controllers.V2
{
    /// <summary>
    /// API controller for managing customer accounts (V2)
    /// </summary>
    [ApiVersion("2.0")]
    [SwaggerTag("Customer Account Management (V2)")]
    public class AccountsController : ApiControllerBase
    {
        /// <summary>
        /// Gets a list of all accounts for the authenticated customer (V2)
        /// </summary>
        /// <remarks>
        /// This V2 endpoint includes additional account details and features compared to V1.
        /// 
        /// Sample request:
        ///
        ///     GET /api/v2/accounts
        ///     
        /// </remarks>
        /// <returns>A list of customer accounts with enhanced details</returns>
        /// <response code="200">Returns the list of accounts</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpGet]
        [Authorize]
        [SwaggerOperation(
            Summary = "Gets all customer accounts (V2)",
            Description = "Retrieves all accounts owned by the authenticated customer with enhanced details",
            OperationId = "Accounts_GetAll_V2",
            Tags = new[] { "Accounts" })]
        [SwaggerResponse(StatusCodes.Status200OK, "The list of accounts", typeof(ApiResponse<List<AccountDtoV2>>))]
        public async Task<ActionResult<ApiResponse<List<AccountDtoV2>>>> GetAllAccounts()
        {
            // Simulated data for documentation purposes
            var accounts = new List<AccountDtoV2>
            {
                new AccountDtoV2
                {
                    AccountId = Guid.NewGuid().ToString(),
                    AccountNumber = "1234567890",
                    AccountName = "Savings Account",
                    AccountType = "Savings",
                    Balance = 1000.00m,
                    AvailableBalance = 1000.00m,
                    Currency = "NGN",
                    Status = "Active",
                    OpenDate = DateTime.UtcNow.AddYears(-1),
                    LastActivityDate = DateTime.UtcNow.AddDays(-2),
                    InterestRate = 3.5m,
                    OwnershipType = "Individual",
                    Features = new List<string> { "Mobile Banking", "Internet Banking", "Debit Card" },
                    IsOverdraftEnabled = false,
                    OverdraftLimit = 0m
                }
            };

            return Success(accounts);
        }

        /// <summary>
        /// Gets an account by its ID (V2)
        /// </summary>
        /// <param name="id">The account ID</param>
        /// <returns>The account details with enhanced information</returns>
        /// <response code="200">Returns the account</response>
        /// <response code="404">If the account is not found</response>
        [HttpGet("{id}")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Gets an account by ID (V2)",
            Description = "Retrieves a specific account by its unique identifier with enhanced details",
            OperationId = "Accounts_GetById_V2",
            Tags = new[] { "Accounts" })]
        [SwaggerResponse(StatusCodes.Status200OK, "The account", typeof(ApiResponse<AccountDtoV2>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Account not found")]
        public async Task<ActionResult<ApiResponse<AccountDtoV2>>> GetAccountById(string id)
        {
            // Simulated data for documentation purposes
            var account = new AccountDtoV2
            {
                AccountId = id,
                AccountNumber = "1234567890",
                AccountName = "Savings Account",
                AccountType = "Savings",
                Balance = 1000.00m,
                AvailableBalance = 1000.00m,
                Currency = "NGN",
                Status = "Active",
                OpenDate = DateTime.UtcNow.AddYears(-1),
                LastActivityDate = DateTime.UtcNow.AddDays(-2),
                InterestRate = 3.5m,
                OwnershipType = "Individual",
                Features = new List<string> { "Mobile Banking", "Internet Banking", "Debit Card" },
                IsOverdraftEnabled = false,
                OverdraftLimit = 0m
            };

            return Success(account);
        }

        /// <summary>
        /// Creates a new account (V2)
        /// </summary>
        /// <param name="request">The account creation request</param>
        /// <returns>The newly created account</returns>
        /// <response code="201">Returns the newly created account</response>
        /// <response code="400">If the request is invalid</response>
        [HttpPost]
        [Authorize]
        [SwaggerOperation(
            Summary = "Creates a new account (V2)",
            Description = "Creates a new account for the authenticated customer with enhanced options",
            OperationId = "Accounts_Create_V2",
            Tags = new[] { "Accounts" })]
        [SwaggerResponse(StatusCodes.Status201Created, "The newly created account", typeof(ApiResponse<AccountDtoV2>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        public async Task<ActionResult<ApiResponse<AccountDtoV2>>> CreateAccount([FromBody] CreateAccountRequestV2 request)
        {
            // Simulated data for documentation purposes
            var account = new AccountDtoV2
            {
                AccountId = Guid.NewGuid().ToString(),
                AccountNumber = "9876543210",
                AccountName = request.AccountName,
                AccountType = request.AccountType,
                Balance = 0.00m,
                AvailableBalance = 0.00m,
                Currency = request.Currency,
                Status = "Active",
                OpenDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow,
                InterestRate = request.InterestRate,
                OwnershipType = request.OwnershipType,
                Features = request.Features,
                IsOverdraftEnabled = request.IsOverdraftEnabled,
                OverdraftLimit = request.OverdraftLimit
            };

            return CreatedAtAction(nameof(GetAccountById), new { id = account.AccountId }, Success(account));
        }

        /// <summary>
        /// Gets the account statement with filters (V2)
        /// </summary>
        /// <param name="id">The account ID</param>
        /// <param name="startDate">Start date for the statement</param>
        /// <param name="endDate">End date for the statement</param>
        /// <param name="format">Statement format (PDF, CSV, JSON)</param>
        /// <returns>The account statement in the requested format</returns>
        /// <response code="200">Returns the account statement</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="404">If the account is not found</response>
        [HttpGet("{id}/statement")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Gets account statement (V2)",
            Description = "Retrieves the account statement for a specific period with format options",
            OperationId = "Accounts_GetStatement_V2",
            Tags = new[] { "Accounts", "Statements" })]
        [SwaggerResponse(StatusCodes.Status200OK, "The account statement", typeof(ApiResponse<StatementDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Account not found")]
        public async Task<ActionResult<ApiResponse<StatementDto>>> GetAccountStatement(
            string id,
            [FromQuery, Required] DateTime startDate,
            [FromQuery, Required] DateTime endDate,
            [FromQuery] string format = "JSON")
        {
            // Validate date range
            if (startDate > endDate)
            {
                return Error("Start date cannot be after end date");
            }

            if ((endDate - startDate).TotalDays > 90)
            {
                return Error("Statement period cannot exceed 90 days");
            }

            // Simulated data for documentation purposes
            var statement = new StatementDto
            {
                AccountId = id,
                AccountNumber = "1234567890",
                AccountName = "Savings Account",
                StartDate = startDate,
                EndDate = endDate,
                OpeningBalance = 500.00m,
                ClosingBalance = 1000.00m,
                Currency = "NGN",
                GeneratedDate = DateTime.UtcNow,
                Format = format,
                Transactions = new List<TransactionDtoV2>
                {
                    new TransactionDtoV2
                    {
                        TransactionId = Guid.NewGuid().ToString(),
                        TransactionType = "Credit",
                        Amount = 500.00m,
                        Currency = "NGN",
                        Description = "Salary payment",
                        TransactionDate = DateTime.UtcNow.AddDays(-10),
                        ValueDate = DateTime.UtcNow.AddDays(-10),
                        Reference = "TRF/2025/09/10/001",
                        BalanceAfter = 1000.00m,
                        Category = "Income",
                        Status = "Completed"
                    }
                }
            };

            return Success(statement);
        }
    }

    /// <summary>
    /// Enhanced account information (V2)
    /// </summary>
    public class AccountDtoV2
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
        /// Available balance (may differ from current balance due to holds)
        /// </summary>
        /// <example>950.00</example>
        public decimal AvailableBalance { get; set; }

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

        /// <summary>
        /// Date when the account was opened
        /// </summary>
        /// <example>2024-01-15T10:30:00Z</example>
        public DateTime OpenDate { get; set; }

        /// <summary>
        /// Date of the last activity on the account
        /// </summary>
        /// <example>2025-09-14T14:25:30Z</example>
        public DateTime LastActivityDate { get; set; }

        /// <summary>
        /// Interest rate for the account
        /// </summary>
        /// <example>3.5</example>
        public decimal InterestRate { get; set; }

        /// <summary>
        /// Type of ownership (Individual, Joint, Corporate)
        /// </summary>
        /// <example>Individual</example>
        public string OwnershipType { get; set; }

        /// <summary>
        /// Features enabled for this account
        /// </summary>
        /// <example>["Mobile Banking", "Internet Banking", "Debit Card"]</example>
        public List<string> Features { get; set; }

        /// <summary>
        /// Whether overdraft is enabled for this account
        /// </summary>
        /// <example>false</example>
        public bool IsOverdraftEnabled { get; set; }

        /// <summary>
        /// Overdraft limit if enabled
        /// </summary>
        /// <example>0</example>
        public decimal OverdraftLimit { get; set; }
    }

    /// <summary>
    /// Enhanced request to create a new account (V2)
    /// </summary>
    public class CreateAccountRequestV2
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

        /// <summary>
        /// Interest rate for the account
        /// </summary>
        /// <example>3.5</example>
        public decimal InterestRate { get; set; }

        /// <summary>
        /// Type of ownership (Individual, Joint, Corporate)
        /// </summary>
        /// <example>Individual</example>
        [Required]
        public string OwnershipType { get; set; }

        /// <summary>
        /// Features to enable for this account
        /// </summary>
        /// <example>["Mobile Banking", "Internet Banking", "Debit Card"]</example>
        public List<string> Features { get; set; } = new List<string>();

        /// <summary>
        /// Whether overdraft should be enabled for this account
        /// </summary>
        /// <example>false</example>
        public bool IsOverdraftEnabled { get; set; }

        /// <summary>
        /// Overdraft limit if enabled
        /// </summary>
        /// <example>0</example>
        public decimal OverdraftLimit { get; set; }
    }

    /// <summary>
    /// Enhanced transaction information (V2)
    /// </summary>
    public class TransactionDtoV2
    {
        /// <summary>
        /// Unique identifier for the transaction
        /// </summary>
        /// <example>t1u2v3w4-x5y6-7z8a-9b0c-d1e2f3g4h5i6</example>
        public string TransactionId { get; set; }

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
        /// <example>2025-09-05T14:30:00Z</example>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Value date of the transaction
        /// </summary>
        /// <example>2025-09-05T14:30:00Z</example>
        public DateTime ValueDate { get; set; }

        /// <summary>
        /// Transaction reference number
        /// </summary>
        /// <example>TRF/2025/09/05/001</example>
        public string Reference { get; set; }

        /// <summary>
        /// Balance after the transaction
        /// </summary>
        /// <example>1000.00</example>
        public decimal BalanceAfter { get; set; }

        /// <summary>
        /// Transaction category
        /// </summary>
        /// <example>Income</example>
        public string Category { get; set; }

        /// <summary>
        /// Transaction status
        /// </summary>
        /// <example>Completed</example>
        public string Status { get; set; }
    }

    /// <summary>
    /// Account statement information
    /// </summary>
    public class StatementDto
    {
        /// <summary>
        /// Account ID
        /// </summary>
        /// <example>a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6</example>
        public string AccountId { get; set; }

        /// <summary>
        /// Account number
        /// </summary>
        /// <example>1234567890</example>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Account name
        /// </summary>
        /// <example>Savings Account</example>
        public string AccountName { get; set; }

        /// <summary>
        /// Start date of the statement period
        /// </summary>
        /// <example>2025-07-01T00:00:00Z</example>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date of the statement period
        /// </summary>
        /// <example>2025-09-15T23:59:59Z</example>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Opening balance at the start of the period
        /// </summary>
        /// <example>500.00</example>
        public decimal OpeningBalance { get; set; }

        /// <summary>
        /// Closing balance at the end of the period
        /// </summary>
        /// <example>1000.00</example>
        public decimal ClosingBalance { get; set; }

        /// <summary>
        /// Currency code
        /// </summary>
        /// <example>NGN</example>
        public string Currency { get; set; }

        /// <summary>
        /// Date and time when the statement was generated
        /// </summary>
        /// <example>2025-09-16T10:30:00Z</example>
        public DateTime GeneratedDate { get; set; }

        /// <summary>
        /// Format of the statement (PDF, CSV, JSON)
        /// </summary>
        /// <example>PDF</example>
        public string Format { get; set; }

        /// <summary>
        /// List of transactions in the statement period
        /// </summary>
        public List<TransactionDtoV2> Transactions { get; set; }
    }
}