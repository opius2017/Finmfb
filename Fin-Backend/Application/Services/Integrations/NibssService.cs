using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FinTech.Core.Application.Common.Settings;
using FinTech.Application.Settings;

namespace FinTech.Core.Application.Services.Integrations;

/// <summary>
/// Integration with Nigeria Inter-Bank Settlement System (NIBSS) for BVN verification, NIP transfers, and NACH direct debits
/// </summary>
public interface INibssService
{
    Task<BvnVerificationResponse> VerifyBvnAsync(string bvn);
    Task<NipTransferResponse> ProcessNipTransferAsync(NipTransferRequest request);
    Task<NachDirectDebitResponse> CreateDirectDebitMandateAsync(NachDirectDebitRequest request);
    Task<bool> CancelDirectDebitMandateAsync(string mandateReference);
    Task<List<DirectDebitMandate>> GetActiveDirectDebitMandatesAsync(string accountNumber);
}

public class NibssService : INibssService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NibssService> _logger;
    private readonly NibssSettings _settings;

    public NibssService(
        HttpClient httpClient,
        ILogger<NibssService> logger,
        IOptions<NibssSettings> settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;
        
        // Configure HTTP client for NIBSS
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("InstitutionCode", _settings.InstitutionCode);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
    }

    public async Task<BvnVerificationResponse> VerifyBvnAsync(string bvn)
    {
        try
        {
            _logger.LogInformation("Verifying BVN: {BVN}", bvn);

            var requestData = new
            {
                BVN = bvn,
                InstitutionCode = _settings.InstitutionCode,
                Hash = GenerateHash(bvn + _settings.InstitutionCode)
            };

            var response = await _httpClient.PostAsJsonAsync("/api/v1/bvnr/verify", requestData);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BvnVerificationResponse>();
                _logger.LogInformation("BVN verification successful for: {BVN}", bvn);
                return result;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("BVN verification failed: {ErrorMessage}", errorContent);
            
            return new BvnVerificationResponse
            {
                IsSuccessful = false,
                ResponseCode = "99",
                ResponseMessage = $"Failed to verify BVN: {errorContent}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while verifying BVN: {BVN}", bvn);
            return new BvnVerificationResponse
            {
                IsSuccessful = false,
                ResponseCode = "99",
                ResponseMessage = $"Exception: {ex.Message}"
            };
        }
    }

    public async Task<NipTransferResponse> ProcessNipTransferAsync(NipTransferRequest request)
    {
        try
        {
            _logger.LogInformation("Processing NIP transfer: {Amount} to {DestinationBank}/{DestinationAccount}", 
                request.Amount, request.DestinationBankCode, request.DestinationAccountNumber);

            var hash = GenerateHash(
                request.SourceAccountNumber + 
                request.DestinationAccountNumber + 
                request.Amount.ToString("F2") + 
                _settings.InstitutionCode);

            var requestData = new
            {
                SourceAccountNumber = request.SourceAccountNumber,
                SourceAccountName = request.SourceAccountName,
                DestinationBankCode = request.DestinationBankCode,
                DestinationAccountNumber = request.DestinationAccountNumber,
                DestinationAccountName = request.DestinationAccountName,
                Amount = request.Amount,
                Narration = request.Narration,
                TransactionReference = request.TransactionReference,
                InstitutionCode = _settings.InstitutionCode,
                Hash = hash
            };

            var response = await _httpClient.PostAsJsonAsync("/api/v1/nip/transfer", requestData);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<NipTransferResponse>();
                _logger.LogInformation("NIP transfer successful: {Reference}", request.TransactionReference);
                return result;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("NIP transfer failed: {ErrorMessage}", errorContent);
            
            return new NipTransferResponse
            {
                IsSuccessful = false,
                ResponseCode = "99",
                ResponseMessage = $"Failed to process NIP transfer: {errorContent}",
                SessionId = request.TransactionReference
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while processing NIP transfer: {Reference}", 
                request.TransactionReference);
            
            return new NipTransferResponse
            {
                IsSuccessful = false,
                ResponseCode = "99",
                ResponseMessage = $"Exception: {ex.Message}",
                SessionId = request.TransactionReference
            };
        }
    }

    public async Task<NachDirectDebitResponse> CreateDirectDebitMandateAsync(NachDirectDebitRequest request)
    {
        try
        {
            _logger.LogInformation("Creating direct debit mandate for account: {AccountNumber}", 
                request.AccountNumber);

            var hash = GenerateHash(
                request.AccountNumber + 
                request.MaximumAmount.ToString("F2") + 
                _settings.InstitutionCode);

            var requestData = new
            {
                AccountNumber = request.AccountNumber,
                AccountName = request.AccountName,
                BankCode = request.BankCode,
                MaximumAmount = request.MaximumAmount,
                StartDate = request.StartDate.ToString("yyyy-MM-dd"),
                EndDate = request.EndDate.ToString("yyyy-MM-dd"),
                MandateType = request.MandateType,
                FrequencyType = request.FrequencyType,
                MandateReference = request.MandateReference,
                InstitutionCode = _settings.InstitutionCode,
                Hash = hash
            };

            var response = await _httpClient.PostAsJsonAsync("/api/v1/nach/mandate/create", requestData);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<NachDirectDebitResponse>();
                _logger.LogInformation("Direct debit mandate created successfully: {Reference}", 
                    request.MandateReference);
                return result;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Direct debit mandate creation failed: {ErrorMessage}", errorContent);
            
            return new NachDirectDebitResponse
            {
                IsSuccessful = false,
                ResponseCode = "99",
                ResponseMessage = $"Failed to create direct debit mandate: {errorContent}",
                MandateReference = request.MandateReference
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while creating direct debit mandate: {Reference}", 
                request.MandateReference);
            
            return new NachDirectDebitResponse
            {
                IsSuccessful = false,
                ResponseCode = "99",
                ResponseMessage = $"Exception: {ex.Message}",
                MandateReference = request.MandateReference
            };
        }
    }

    public async Task<bool> CancelDirectDebitMandateAsync(string mandateReference)
    {
        try
        {
            _logger.LogInformation("Cancelling direct debit mandate: {Reference}", mandateReference);

            var hash = GenerateHash(mandateReference + _settings.InstitutionCode);

            var requestData = new
            {
                MandateReference = mandateReference,
                InstitutionCode = _settings.InstitutionCode,
                Hash = hash
            };

            var response = await _httpClient.PostAsJsonAsync("/api/v1/nach/mandate/cancel", requestData);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<dynamic>();
                _logger.LogInformation("Direct debit mandate cancelled successfully: {Reference}", mandateReference);
                return true;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Direct debit mandate cancellation failed: {ErrorMessage}", errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while cancelling direct debit mandate: {Reference}", 
                mandateReference);
            return false;
        }
    }

    public async Task<List<DirectDebitMandate>> GetActiveDirectDebitMandatesAsync(string accountNumber)
    {
        try
        {
            _logger.LogInformation("Getting active direct debit mandates for account: {AccountNumber}", 
                accountNumber);

            var hash = GenerateHash(accountNumber + _settings.InstitutionCode);

            var response = await _httpClient.GetAsync(
                $"/api/v1/nach/mandate/list?accountNumber={accountNumber}&institutionCode={_settings.InstitutionCode}&hash={hash}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ActiveMandatesResponse>();
                _logger.LogInformation("Retrieved {Count} active mandates for account: {AccountNumber}", 
                    result?.Mandates?.Count ?? 0, accountNumber);
                return result?.Mandates ?? new List<DirectDebitMandate>();
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to get active mandates: {ErrorMessage}", errorContent);
            return new List<DirectDebitMandate>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting active mandates for account: {AccountNumber}", 
                accountNumber);
            return new List<DirectDebitMandate>();
        }
    }

    private string GenerateHash(string data)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data + _settings.SecretKey));
        return Convert.ToBase64String(hashBytes);
    }
}

public class BvnVerificationResponse
{
    public bool IsSuccessful { get; set; }
    public string ResponseCode { get; set; } = string.Empty;
    public string ResponseMessage { get; set; } = string.Empty;
    public string BVN { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string EnrollmentBank { get; set; } = string.Empty;
    public string EnrollmentBranch { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LevelOfAccount { get; set; } = string.Empty;
    public string LGA { get; set; } = string.Empty;
    public string WatchListed { get; set; } = string.Empty;
    public string Base64Image { get; set; } = string.Empty;
}

public class NipTransferRequest
{
    public string SourceAccountNumber { get; set; } = string.Empty;
    public string SourceAccountName { get; set; } = string.Empty;
    public string DestinationBankCode { get; set; } = string.Empty;
    public string DestinationAccountNumber { get; set; } = string.Empty;
    public string DestinationAccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Narration { get; set; } = string.Empty;
    public string TransactionReference { get; set; } = string.Empty;
}

public class NipTransferResponse
{
    public bool IsSuccessful { get; set; }
    public string ResponseCode { get; set; } = string.Empty;
    public string ResponseMessage { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string DestinationAccountName { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
}

public class NachDirectDebitRequest
{
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public decimal MaximumAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string MandateType { get; set; } = "FIXED"; // FIXED or VARIABLE
    public string FrequencyType { get; set; } = "MONTHLY"; // DAILY, WEEKLY, MONTHLY
    public string MandateReference { get; set; } = string.Empty;
}

public class NachDirectDebitResponse
{
    public bool IsSuccessful { get; set; }
    public string ResponseCode { get; set; } = string.Empty;
    public string ResponseMessage { get; set; } = string.Empty;
    public string MandateReference { get; set; } = string.Empty;
    public string MandateId { get; set; } = string.Empty;
}

public class DirectDebitMandate
{
    public string MandateReference { get; set; } = string.Empty;
    public string MandateId { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public decimal MaximumAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string MandateType { get; set; } = string.Empty;
    public string FrequencyType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class ActiveMandatesResponse
{
    public bool IsSuccessful { get; set; }
    public string ResponseCode { get; set; } = string.Empty;
    public string ResponseMessage { get; set; } = string.Empty;
    public List<DirectDebitMandate> Mandates { get; set; } = new();
}