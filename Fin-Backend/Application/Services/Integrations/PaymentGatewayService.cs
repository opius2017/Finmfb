using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FinTech.Core.Application.Common.Settings;

namespace FinTech.Core.Application.Services.Integrations;

/// <summary>
/// Integration with payment gateways for accepting online payments
/// </summary>
public interface IPaymentGatewayService
{
    Task<PaymentInitiationResponse> InitiatePaymentAsync(PaymentInitiationRequest request);
    Task<PaymentVerificationResponse> VerifyPaymentAsync(string reference);
    Task<PaymentRefundResponse> InitiateRefundAsync(PaymentRefundRequest request);
    Task<RecurringBillingResponse> SetupRecurringBillingAsync(RecurringBillingRequest request);
    Task<bool> CancelRecurringBillingAsync(string recurringId);
}

public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentGatewayService> _logger;
    private readonly PaymentGatewaySettings _settings;

    public PaymentGatewayService(
        HttpClient httpClient,
        ILogger<PaymentGatewayService> logger,
        IOptions<PaymentGatewaySettings> settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;
        
        // Configure HTTP client for Payment Gateway
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.SecretKey}");
    }

    public async Task<PaymentInitiationResponse> InitiatePaymentAsync(PaymentInitiationRequest request)
    {
        try
        {
            _logger.LogInformation("Initiating payment for {Amount} {Currency} from {Email}", 
                request.Amount, request.Currency, MaskEmail(request.CustomerEmail));

            var payload = new
            {
                amount = request.Amount * 100, // Convert to lowest currency unit (kobo)
                currency = request.Currency,
                email = request.CustomerEmail,
                reference = request.Reference,
                callback_url = request.CallbackUrl,
                metadata = request.Metadata,
                channels = request.PaymentChannels
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/transaction/initialize", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PaymentInitiationResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                _logger.LogInformation("Payment initiation successful: {Reference}", request.Reference);
                return result;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Payment initiation failed: {ErrorMessage}", errorContent);
            
            return new PaymentInitiationResponse
            {
                Status = false,
                Message = $"Payment initiation failed: {errorContent}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during payment initiation for: {Reference}", 
                request.Reference);
            
            return new PaymentInitiationResponse
            {
                Status = false,
                Message = $"Exception: {ex.Message}"
            };
        }
    }

    public async Task<PaymentVerificationResponse> VerifyPaymentAsync(string reference)
    {
        try
        {
            _logger.LogInformation("Verifying payment: {Reference}", reference);

            var response = await _httpClient.GetAsync($"/transaction/verify/{reference}");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PaymentVerificationResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                _logger.LogInformation("Payment verification result for {Reference}: {Status}", 
                    reference, result?.Data?.Status);
                return result;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Payment verification failed: {ErrorMessage}", errorContent);
            
            return new PaymentVerificationResponse
            {
                Status = false,
                Message = $"Payment verification failed: {errorContent}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during payment verification for: {Reference}", reference);
            
            return new PaymentVerificationResponse
            {
                Status = false,
                Message = $"Exception: {ex.Message}"
            };
        }
    }

    public async Task<PaymentRefundResponse> InitiateRefundAsync(PaymentRefundRequest request)
    {
        try
        {
            _logger.LogInformation("Initiating refund for transaction: {TransactionId}, Amount: {Amount}", 
                request.TransactionId, request.Amount);

            var payload = new
            {
                transaction = request.TransactionId,
                amount = request.Amount * 100, // Convert to kobo
                currency = request.Currency,
                customer_note = request.CustomerNote,
                merchant_note = request.MerchantNote
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/refund", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PaymentRefundResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                _logger.LogInformation("Refund initiation successful for transaction: {TransactionId}", 
                    request.TransactionId);
                return result;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Refund initiation failed: {ErrorMessage}", errorContent);
            
            return new PaymentRefundResponse
            {
                Status = false,
                Message = $"Refund initiation failed: {errorContent}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during refund initiation for transaction: {TransactionId}", 
                request.TransactionId);
            
            return new PaymentRefundResponse
            {
                Status = false,
                Message = $"Exception: {ex.Message}"
            };
        }
    }

    public async Task<RecurringBillingResponse> SetupRecurringBillingAsync(RecurringBillingRequest request)
    {
        try
        {
            _logger.LogInformation("Setting up recurring billing for {Email}, Amount: {Amount} {Currency}", 
                MaskEmail(request.CustomerEmail), request.Amount, request.Currency);

            var payload = new
            {
                customer = request.CustomerId,
                amount = request.Amount * 100, // Convert to kobo
                currency = request.Currency,
                interval = request.Interval,
                start_date = request.StartDate.ToString("yyyy-MM-dd"),
                plan = request.PlanCode,
                reference = request.Reference
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/subscription", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<RecurringBillingResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                _logger.LogInformation("Recurring billing setup successful: {Reference}", request.Reference);
                return result;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Recurring billing setup failed: {ErrorMessage}", errorContent);
            
            return new RecurringBillingResponse
            {
                Status = false,
                Message = $"Recurring billing setup failed: {errorContent}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during recurring billing setup for: {Reference}", 
                request.Reference);
            
            return new RecurringBillingResponse
            {
                Status = false,
                Message = $"Exception: {ex.Message}"
            };
        }
    }

    public async Task<bool> CancelRecurringBillingAsync(string recurringId)
    {
        try
        {
            _logger.LogInformation("Cancelling recurring billing: {SubscriptionId}", recurringId);

            var payload = new
            {
                code = recurringId
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/subscription/disable", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Recurring billing cancelled successfully: {SubscriptionId}", recurringId);
                return true;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Recurring billing cancellation failed: {ErrorMessage}", errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while cancelling recurring billing: {SubscriptionId}", 
                recurringId);
            return false;
        }
    }

    private string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            return email;
        
        var parts = email.Split('@');
        
        if (parts[0].Length <= 2)
            return email;
        
        var maskedUsername = parts[0].Substring(0, 2) + new string('*', parts[0].Length - 2);
        
        return $"{maskedUsername}@{parts[1]}";
    }
}

public class PaymentInitiationRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "NGN";
    public string CustomerEmail { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
    public object? Metadata { get; set; }
    public string[]? PaymentChannels { get; set; }
}

public class PaymentInitiationResponse
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public PaymentInitiationData? Data { get; set; }
}

public class PaymentInitiationData
{
    public string AuthorizationUrl { get; set; } = string.Empty;
    public string AccessCode { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
}

public class PaymentVerificationResponse
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public PaymentVerificationData? Data { get; set; }
}

public class PaymentVerificationData
{
    public string Id { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PaymentChannel { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public PaymentCustomer? Customer { get; set; }
    public PaymentAuthorization? Authorization { get; set; }
}

public class PaymentCustomer
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class PaymentAuthorization
{
    public string AuthorizationCode { get; set; } = string.Empty;
    public string Bin { get; set; } = string.Empty;
    public string Last4 { get; set; } = string.Empty;
    public string ExpMonth { get; set; } = string.Empty;
    public string ExpYear { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public string Bank { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public bool Reusable { get; set; }
}

public class PaymentRefundRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "NGN";
    public string CustomerNote { get; set; } = string.Empty;
    public string MerchantNote { get; set; } = string.Empty;
}

public class PaymentRefundResponse
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public PaymentRefundData? Data { get; set; }
}

public class PaymentRefundData
{
    public string Id { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string TransactionId { get; set; } = string.Empty;
}

public class RecurringBillingRequest
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "NGN";
    public string Interval { get; set; } = "monthly"; // daily, weekly, monthly, quarterly, annually
    public DateTime StartDate { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
}

public class RecurringBillingResponse
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public RecurringBillingData? Data { get; set; }
}

public class RecurringBillingData
{
    public string Id { get; set; } = string.Empty;
    public string SubscriptionCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Interval { get; set; } = string.Empty;
    public DateTime NextPaymentDate { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
}