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
    public class TransactionManagementController : ControllerBase
    {
        private readonly ITransactionManagementService _transactionService;
        private readonly IClientPortalService _clientPortalService;
        private readonly IAccountOverviewService _accountService;
        private readonly ILogger<TransactionManagementController> _logger;
        private readonly IMapper _mapper;

        public TransactionManagementController(
            ITransactionManagementService transactionService,
            IClientPortalService clientPortalService,
            IAccountOverviewService accountService,
            ILogger<TransactionManagementController> logger,
            IMapper mapper)
        {
            _transactionService = transactionService;
            _clientPortalService = clientPortalService;
            _accountService = accountService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpPost("search")]
        public async Task<ActionResult<BaseResponse<TransactionSearchResultDto>>> SearchTransactions(TransactionSearchDto searchDto)
        {
            try
            {
                var customerId = await GetCustomerIdAsync();
                
                // Validate account ownership if account number is provided
                if (!string.IsNullOrEmpty(searchDto.AccountNumber))
                {
                    var accounts = await _accountService.GetClientAccountsAsync(customerId);
                    if (!accounts.Any(a => a.AccountNumber == searchDto.AccountNumber))
                    {
                        return BadRequest(new BaseResponse<TransactionSearchResultDto>
                        {
                            Success = false,
                            Message = "Account not found or does not belong to the customer"
                        });
                    }
                }

                var (transactions, totalCount) = await _transactionService.SearchTransactionsAsync(searchDto, customerId);
                var transactionDtos = _mapper.Map<List<TransactionDto>>(transactions);

                var result = new TransactionSearchResultDto
                {
                    Transactions = transactionDtos,
                    TotalCount = totalCount,
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)searchDto.PageSize)
                };

                return Ok(new BaseResponse<TransactionSearchResultDto>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (UnauthorizedAccessException)
            {
                return BadRequest(new BaseResponse<TransactionSearchResultDto>
                {
                    Success = false,
                    Message = "Account not found or does not belong to the customer"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching transactions");
                return StatusCode(500, new BaseResponse<TransactionSearchResultDto>
                {
                    Success = false,
                    Message = "An error occurred while searching transactions"
                });
            }
        }

        [HttpGet("categories/{accountNumber}")]
        public async Task<ActionResult<BaseResponse<List<string>>>> GetTransactionCategories(string accountNumber)
        {
            try
            {
                var customerId = await GetCustomerIdAsync();
                
                // Validate account ownership
                var accounts = await _accountService.GetClientAccountsAsync(customerId);
                if (!accounts.Any(a => a.AccountNumber == accountNumber))
                {
                    return BadRequest(new BaseResponse<List<string>>
                    {
                        Success = false,
                        Message = "Account not found or does not belong to the customer"
                    });
                }

                var categories = await _transactionService.GetTransactionCategoriesAsync(accountNumber);

                return Ok(new BaseResponse<List<string>>
                {
                    Success = true,
                    Data = categories.ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction categories for account {AccountNumber}", accountNumber);
                return StatusCode(500, new BaseResponse<List<string>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving transaction categories"
                });
            }
        }

        [HttpGet("channels/{accountNumber}")]
        public async Task<ActionResult<BaseResponse<List<string>>>> GetTransactionChannels(string accountNumber)
        {
            try
            {
                var customerId = await GetCustomerIdAsync();
                
                // Validate account ownership
                var accounts = await _accountService.GetClientAccountsAsync(customerId);
                if (!accounts.Any(a => a.AccountNumber == accountNumber))
                {
                    return BadRequest(new BaseResponse<List<string>>
                    {
                        Success = false,
                        Message = "Account not found or does not belong to the customer"
                    });
                }

                var channels = await _transactionService.GetTransactionChannelsAsync(accountNumber);

                return Ok(new BaseResponse<List<string>>
                {
                    Success = true,
                    Data = channels.ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction channels for account {AccountNumber}", accountNumber);
                return StatusCode(500, new BaseResponse<List<string>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving transaction channels"
                });
            }
        }

        [HttpPost("export")]
        public async Task<ActionResult> ExportTransactions(TransactionExportDto exportDto)
        {
            try
            {
                var customerId = await GetCustomerIdAsync();
                
                // Validate account ownership
                var accounts = await _accountService.GetClientAccountsAsync(customerId);
                if (!accounts.Any(a => a.AccountNumber == exportDto.AccountNumber))
                {
                    return BadRequest(new BaseResponse<string>
                    {
                        Success = false,
                        Message = "Account not found or does not belong to the customer"
                    });
                }

                // Validate format
                if (exportDto.Format.ToLower() != "csv" && 
                    exportDto.Format.ToLower() != "excel" && 
                    exportDto.Format.ToLower() != "pdf")
                {
                    return BadRequest(new BaseResponse<string>
                    {
                        Success = false,
                        Message = "Invalid format. Supported formats are 'csv', 'excel', and 'pdf'."
                    });
                }

                var fileData = await _transactionService.ExportTransactionsAsync(exportDto, customerId);

                string contentType;
                string fileExtension;
                
                switch (exportDto.Format.ToLower())
                {
                    case "pdf":
                        contentType = "application/pdf";
                        fileExtension = "pdf";
                        break;
                    case "excel":
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        fileExtension = "xlsx";
                        break;
                    default: // csv
                        contentType = "text/csv";
                        fileExtension = "csv";
                        break;
                }

                var fileName = $"Transactions_{exportDto.AccountNumber}_{exportDto.StartDate:yyyyMMdd}_to_{exportDto.EndDate:yyyyMMdd}.{fileExtension}";

                return File(fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting transactions for account {AccountNumber}", exportDto.AccountNumber);
                return StatusCode(500, new BaseResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while exporting transactions"
                });
            }
        }

        [HttpGet("category-breakdown/{accountNumber}")]
        public async Task<ActionResult<BaseResponse<Dictionary<string, decimal>>>> GetCategoryBreakdown(
            string accountNumber,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var customerId = await GetCustomerIdAsync();
                
                // Validate account ownership
                var accounts = await _accountService.GetClientAccountsAsync(customerId);
                if (!accounts.Any(a => a.AccountNumber == accountNumber))
                {
                    return BadRequest(new BaseResponse<Dictionary<string, decimal>>
                    {
                        Success = false,
                        Message = "Account not found or does not belong to the customer"
                    });
                }

                // Default to last 30 days if dates not provided
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var breakdown = await _transactionService.GetCategoryBreakdownAsync(accountNumber, start, end);

                return Ok(new BaseResponse<Dictionary<string, decimal>>
                {
                    Success = true,
                    Data = breakdown
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category breakdown for account {AccountNumber}", accountNumber);
                return StatusCode(500, new BaseResponse<Dictionary<string, decimal>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving category breakdown"
                });
            }
        }

        [HttpGet("largest-transactions")]
        public async Task<ActionResult<BaseResponse<List<TransactionDto>>>> GetLargestTransactions([FromQuery] int count = 5)
        {
            try
            {
                var customerId = await GetCustomerIdAsync();
                var transactions = await _transactionService.GetLargestTransactionsAsync(customerId, count);
                var transactionDtos = _mapper.Map<List<TransactionDto>>(transactions);

                return Ok(new BaseResponse<List<TransactionDto>>
                {
                    Success = true,
                    Data = transactionDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving largest transactions");
                return StatusCode(500, new BaseResponse<List<TransactionDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving largest transactions"
                });
            }
        }

        [HttpGet("monthly-spending/{accountNumber}")]
        public async Task<ActionResult<BaseResponse<Dictionary<string, decimal>>>> GetMonthlySpending(
            string accountNumber,
            [FromQuery] int months = 6)
        {
            try
            {
                var customerId = await GetCustomerIdAsync();
                
                // Validate account ownership
                var accounts = await _accountService.GetClientAccountsAsync(customerId);
                if (!accounts.Any(a => a.AccountNumber == accountNumber))
                {
                    return BadRequest(new BaseResponse<Dictionary<string, decimal>>
                    {
                        Success = false,
                        Message = "Account not found or does not belong to the customer"
                    });
                }

                var spending = await _transactionService.GetMonthlySpendingAsync(accountNumber, months);

                return Ok(new BaseResponse<Dictionary<string, decimal>>
                {
                    Success = true,
                    Data = spending
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving monthly spending for account {AccountNumber}", accountNumber);
                return StatusCode(500, new BaseResponse<Dictionary<string, decimal>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving monthly spending"
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

    public class TransactionSearchResultDto
    {
        public List<TransactionDto> Transactions { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}