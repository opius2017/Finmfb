using FinTech.Core.Application.Interfaces.Caching;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FinTech.Core.Application.Services
{
    public class BankingIntegrationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDistributedCacheService _cacheService;
        private readonly ILogger<BankingIntegrationService> _logger;

        public BankingIntegrationService(
            IHttpClientFactory httpClientFactory,
            IDistributedCacheService cacheService,
            ILogger<BankingIntegrationService> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get account details with resilience and caching
        /// </summary>
        public async Task<AccountDetails> GetAccountDetailsAsync(string accountNumber)
        {
            string cacheKey = $"account:{accountNumber}";
            
            // Try to get from cache first
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () =>
                {
                    // If not in cache, get from the banking API with circuit breaker and retry policies
                    try
                    {
                        var client = _httpClientFactory.CreateClient("banking-integration");
                        var response = await client.GetAsync($"api/accounts/{accountNumber}");
                        
                        response.EnsureSuccessStatusCode();
                        
                        var accountDetails = await response.Content.ReadFromJsonAsync<AccountDetails>();
                        return accountDetails;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error fetching account details for {AccountNumber}", accountNumber);
                        throw;
                    }
                },
                // Cache for 15 minutes
                absoluteExpiration: TimeSpan.FromMinutes(15)
            );
        }

        /// <summary>
        /// Process a transaction with resilience
        /// </summary>
        public async Task<TransactionResult> ProcessTransactionAsync(TransactionRequest transaction)
        {
            // Don't cache transactions, but use resilience policies
            try
            {
                var client = _httpClientFactory.CreateClient("banking-integration");
                var response = await client.PostAsJsonAsync("api/transactions", transaction);
                
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<TransactionResult>();
                
                // Invalidate account cache after successful transaction
                await _cacheService.RemoveAsync($"account:{transaction.FromAccount}");
                if (!string.IsNullOrEmpty(transaction.ToAccount))
                {
                    await _cacheService.RemoveAsync($"account:{transaction.ToAccount}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing transaction {TransactionId}", transaction.TransactionId);
                throw;
            }
        }
    }

    public class AccountDetails
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class TransactionRequest
    {
        public Guid TransactionId { get; set; }
        public string FromAccount { get; set; }
        public string ToAccount { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
    }

    public class TransactionResult
    {
        public Guid TransactionId { get; set; }
        public string Status { get; set; }
        public string ReferenceNumber { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
    }
}
