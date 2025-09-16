using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FinTech.Core.Application.Common.Settings;

namespace FinTech.Core.Application.Services.Integrations;

/// <summary>
/// Integration with SMS Gateway providers for automated SMS notifications
/// </summary>
public interface ISmsService
{
    Task<SmsResponse> SendSmsAsync(SmsRequest request);
    Task<SmsResponse> SendBulkSmsAsync(BulkSmsRequest request);
    Task<DeliveryStatusResponse> CheckDeliveryStatusAsync(string messageId);
}

public class SmsService : ISmsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SmsService> _logger;
    private readonly SmsSettings _settings;

    public SmsService(
        HttpClient httpClient,
        ILogger<SmsService> logger,
        IOptions<SmsSettings> settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;
        
        // Configure HTTP client for SMS Gateway
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("ApiKey", _settings.ApiKey);
    }

    public async Task<SmsResponse> SendSmsAsync(SmsRequest request)
    {
        try
        {
            _logger.LogInformation("Sending SMS to: {PhoneNumber}", MaskPhoneNumber(request.PhoneNumber));

            var requestData = new
            {
                ApiKey = _settings.ApiKey,
                From = _settings.SenderId,
                To = request.PhoneNumber,
                Message = request.Message,
                Channel = "dnd" // Bypass DND
            };

            var response = await _httpClient.PostAsJsonAsync("/api/v2/sms/send", requestData);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SmsResponse>();
                _logger.LogInformation("SMS successfully sent to: {PhoneNumber}, MessageId: {MessageId}", 
                    MaskPhoneNumber(request.PhoneNumber), result?.MessageId);
                return result;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to send SMS: {ErrorMessage}", errorContent);
            
            return new SmsResponse
            {
                Success = false,
                Message = $"Failed to send SMS: {errorContent}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending SMS to: {PhoneNumber}", 
                MaskPhoneNumber(request.PhoneNumber));
            
            return new SmsResponse
            {
                Success = false,
                Message = $"Exception: {ex.Message}"
            };
        }
    }

    public async Task<SmsResponse> SendBulkSmsAsync(BulkSmsRequest request)
    {
        try
        {
            _logger.LogInformation("Sending bulk SMS to {Count} recipients", request.PhoneNumbers.Count);

            var requestData = new
            {
                ApiKey = _settings.ApiKey,
                From = _settings.SenderId,
                To = request.PhoneNumbers,
                Message = request.Message,
                Channel = "dnd" // Bypass DND
            };

            var response = await _httpClient.PostAsJsonAsync("/api/v2/sms/bulk", requestData);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SmsResponse>();
                _logger.LogInformation("Bulk SMS successfully sent to {Count} recipients, BatchId: {BatchId}", 
                    request.PhoneNumbers.Count, result?.BatchId);
                return result;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to send bulk SMS: {ErrorMessage}", errorContent);
            
            return new SmsResponse
            {
                Success = false,
                Message = $"Failed to send bulk SMS: {errorContent}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending bulk SMS to {Count} recipients", 
                request.PhoneNumbers.Count);
            
            return new SmsResponse
            {
                Success = false,
                Message = $"Exception: {ex.Message}"
            };
        }
    }

    public async Task<DeliveryStatusResponse> CheckDeliveryStatusAsync(string messageId)
    {
        try
        {
            _logger.LogInformation("Checking delivery status for message: {MessageId}", messageId);

            var response = await _httpClient.GetAsync($"/api/v2/sms/status?apiKey={_settings.ApiKey}&messageId={messageId}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DeliveryStatusResponse>();
                _logger.LogInformation("Delivery status for message {MessageId}: {Status}", 
                    messageId, result?.Status);
                return result;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to check delivery status: {ErrorMessage}", errorContent);
            
            return new DeliveryStatusResponse
            {
                Success = false,
                Message = $"Failed to check delivery status: {errorContent}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while checking delivery status for message: {MessageId}", 
                messageId);
            
            return new DeliveryStatusResponse
            {
                Success = false,
                Message = $"Exception: {ex.Message}"
            };
        }
    }

    private string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 8)
            return phoneNumber;
            
        return phoneNumber.Substring(0, 4) + "XXXX" + phoneNumber.Substring(phoneNumber.Length - 3);
    }
}

public class SmsRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Reference { get; set; }
}

public class BulkSmsRequest
{
    public List<string> PhoneNumbers { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    public string? Reference { get; set; }
}

public class SmsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? MessageId { get; set; }
    public string? BatchId { get; set; }
    public decimal Cost { get; set; }
    public int Units { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
}

public class DeliveryStatusResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Status { get; set; } // DELIVERED, PENDING, FAILED
    public string? DeliveryTime { get; set; }
    public string? FailureReason { get; set; }
}