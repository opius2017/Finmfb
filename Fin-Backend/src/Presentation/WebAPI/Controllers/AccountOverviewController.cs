using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinTech.Application.DTOs.Common;
using FinTech.Application.DTOs.ClientPortal;
using FinTech.Application.Services;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Security.Claims;
using System.Linq;

namespace FinTech.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountOverviewController : ControllerBase
    {
        private readonly IAccountOverviewService _accountOverviewService;
        private readonly IClientPortalService _clientPortalService;
        private readonly ILogger<AccountOverviewController> _logger;
        private readonly IMapper _mapper;

        public AccountOverviewController(
            IAccountOverviewService accountOverviewService,
            IClientPortalService clientPortalService,
            ILogger<AccountOverviewController> logger,
            IMapper mapper)
        {
            _accountOverviewService = accountOverviewService;
            _clientPortalService = clientPortalService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("accounts")]
        public async Task<ActionResult<BaseResponse<List<AccountDto>>>> GetAccounts()
        {
            try
            {
                var customerId = await GetCustomerIdAsync();
                var accounts = await _accountOverviewService.GetClientAccountsAsync(customerId);

                var accountDtos = _mapper.Map<List<AccountDto>>(accounts);

                return Ok(new BaseResponse<List<AccountDto>>
                {
                    Success = true,
                    Data = accountDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer accounts");
                return StatusCode(500, new BaseResponse<List<AccountDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving accounts"
                });
            }
        }

        [HttpGet("accounts/{accountNumber}")]
        public async Task<ActionResult<BaseResponse<AccountDetailDto>>> GetAccountDetails(string accountNumber)
        {
            try
            {
                var customerId = await GetCustomerIdAsync();
                var accounts = await _accountOverviewService.GetClientAccountsAsync(customerId);
                
                // Verify that the account belongs to the customer
                if (!accounts.Any(a => a.AccountNumber == accountNumber))
                {
                    return NotFound(new BaseResponse<AccountDetailDto>
                    {
                        Success = false,
                        Message = "Account not found or does not belong to the customer"
                    });
                }

                var account = await _accountOverviewService.GetAccountDetailsAsync(accountNumber);
                var accountDto = _mapper.Map<AccountDetailDto>(account);

                return Ok(new BaseResponse<AccountDetailDto>
                {
                    Success = true,
                    Data = accountDto
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new BaseResponse<AccountDetailDto>
                {
                    Success = false,
                    Message = "Account not found"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account details for account {AccountNumber}", accountNumber);
                return StatusCode(500, new BaseResponse<AccountDetailDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving account details"
                });
            }
        }

        [HttpGet("accounts/{accountNumber}/balance")]
        public async Task<ActionResult<BaseResponse<AccountBalanceDto>>> GetAccountBalance(string accountNumber)
        {
            try
            {
                var customerId = await GetCustomerIdAsync();
                var accounts = await _accountOverviewService.GetClientAccountsAsync(customerId);
                
                // Verify that the account belongs to the customer
                if (!accounts.Any(a => a.AccountNumber == accountNumber))
                {
                    return NotFound(new BaseResponse<AccountBalanceDto>
                    {
                        Success = false,
                        Message = "Account not found or does not belong to the customer"
                    });
                }

                var balance = await _accountOverviewService.GetAccountBalanceAsync(accountNumber);
                var account = accounts.First(a => a.AccountNumber == accountNumber);

                var balanceDto = new AccountBalanceDto
                {
                    AccountNumber = accountNumber,
                    AccountName = account.AccountName,
                    CurrentBalance = balance,
                    AvailableBalance = balance, // This could be different based on holds, etc.
                    Currency = "NGN", // Assuming Nigerian Naira
                    AsOfDate = DateTime.UtcNow
                };

                return Ok(new BaseResponse<AccountBalanceDto>
                {
                    Success = true,
                    Data = balanceDto
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new BaseResponse<AccountBalanceDto>
                {
                    Success = false,
                    Message = "Account not found"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving balance for account {AccountNumber}", accountNumber);
                return StatusCode(500, new BaseResponse<AccountBalanceDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving account balance"
                });
            }
        }

        [HttpGet("accounts/{accountNumber}/transactions")]
        public async Task<ActionResult<BaseResponse<List<TransactionDto>>>> GetAccountTransactions(
            string accountNumber,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var customerId = await GetCustomerIdAsync();
                var accounts = await _accountOverviewService.GetClientAccountsAsync(customerId);
                
                // Verify that the account belongs to the customer
                if (!accounts.Any(a => a.AccountNumber == accountNumber))
                {
                    return NotFound(new BaseResponse<List<TransactionDto>>
                    {
                        Success = false,
                        Message = "Account not found or does not belong to the customer"
                    });
                }

                // Default to last 30 days if dates not provided
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var transactions = await _accountOverviewService.GetAccountTransactionsAsync(
                    accountNumber, start, end, page, pageSize);

                var transactionDtos = _mapper.Map<List<TransactionDto>>(transactions);

                return Ok(new BaseResponse<List<TransactionDto>>
                {
                    Success = true,
                    Data = transactionDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions for account {AccountNumber}", accountNumber);
                return StatusCode(500, new BaseResponse<List<TransactionDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving transactions"
                });
            }
        }

        [HttpGet("accounts/{accountNumber}/statement")]
        public async Task<ActionResult> GetAccountStatement(
            string accountNumber,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string format = "pdf")
        {
            try
            {
                var customerId = await GetCustomerIdAsync();
                var accounts = await _accountOverviewService.GetClientAccountsAsync(customerId);
                
                // Verify that the account belongs to the customer
                if (!accounts.Any(a => a.AccountNumber == accountNumber))
                {
                    return NotFound(new BaseResponse<string>
                    {
                        Success = false,
                        Message = "Account not found or does not belong to the customer"
                    });
                }

                // Default to last 30 days if dates not provided
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                // Validate format
                if (format.ToLower() != "pdf" && format.ToLower() != "excel")
                {
                    return BadRequest(new BaseResponse<string>
                    {
                        Success = false,
                        Message = "Invalid format. Supported formats are 'pdf' and 'excel'."
                    });
                }

                var statementData = await _accountOverviewService.GenerateAccountStatementAsync(
                    accountNumber, start, end, format.ToLower());

                var contentType = format.ToLower() == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = $"Statement_{accountNumber}_{start:yyyyMMdd}_to_{end:yyyyMMdd}.{format.ToLower()}";

                return File(statementData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating statement for account {AccountNumber}", accountNumber);
                return StatusCode(500, new BaseResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while generating account statement"
                });
            }
        }

        [HttpGet("summary")]
        public async Task<ActionResult<BaseResponse<AccountSummaryDto>>> GetAccountSummary()
        {
            try
            {
                var customerId = await GetCustomerIdAsync();
                var summary = await _accountOverviewService.GetAccountSummaryAsync(customerId);

                var summaryDto = _mapper.Map<AccountSummaryDto>(summary);

                return Ok(new BaseResponse<AccountSummaryDto>
                {
                    Success = true,
                    Data = summaryDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account summary");
                return StatusCode(500, new BaseResponse<AccountSummaryDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving account summary"
                });
            }
        }

        [HttpGet("recent-transactions")]
        public async Task<ActionResult<BaseResponse<List<TransactionDto>>>> GetRecentTransactions([FromQuery] int count = 5)
        {
            try
            {
                var customerId = await GetCustomerIdAsync();
                var transactions = await _accountOverviewService.GetRecentTransactionsAsync(customerId, count);

                var transactionDtos = _mapper.Map<List<TransactionDto>>(transactions);

                return Ok(new BaseResponse<List<TransactionDto>>
                {
                    Success = true,
                    Data = transactionDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent transactions");
                return StatusCode(500, new BaseResponse<List<TransactionDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving recent transactions"
                });
            }
        }

        [HttpGet("accounts/{accountNumber}/activity")]
        public async Task<ActionResult<BaseResponse<List<AccountActivityDto>>>> GetAccountActivity(
            string accountNumber,
            [FromQuery] int days = 30)
        {
            try
            {
                var customerId = await GetCustomerIdAsync();
                var accounts = await _accountOverviewService.GetClientAccountsAsync(customerId);
                
                // Verify that the account belongs to the customer
                if (!accounts.Any(a => a.AccountNumber == accountNumber))
                {
                    return NotFound(new BaseResponse<List<AccountActivityDto>>
                    {
                        Success = false,
                        Message = "Account not found or does not belong to the customer"
                    });
                }

                var activity = await _accountOverviewService.GetAccountActivityAsync(accountNumber, days);
                var activityDtos = _mapper.Map<List<AccountActivityDto>>(activity);

                return Ok(new BaseResponse<List<AccountActivityDto>>
                {
                    Success = true,
                    Data = activityDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity for account {AccountNumber}", accountNumber);
                return StatusCode(500, new BaseResponse<List<AccountActivityDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving account activity"
                });
            }
        }

        #region Helper Methods

        private async Task<Guid> GetCustomerIdAsync()
        {
            var userId = GetCurrentUserId();
            var profile = await _clientPortalService.GetClientProfileAsync(userId);
            
            if (profile == null)
            {
                throw new UnauthorizedAccessException("Client profile not found");
            }
            
            return profile.CustomerId;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            
            return Guid.Parse(userIdClaim);
        }

        #endregion
    }
}