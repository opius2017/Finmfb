using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FinTech.Core.Application.Common.Settings;
using FinTech.Core.Application.Settings;

namespace FinTech.Core.Application.Services.Integrations;

/// <summary>
/// Integration with biometric devices for enhanced KYC and transaction authorization
/// </summary>
public interface IBiometricService
{
    Task<BiometricVerificationResponse> VerifyFingerprintAsync(BiometricVerificationRequest request);
    Task<BiometricVerificationResponse> VerifyFacialAsync(BiometricVerificationRequest request);
    Task<BiometricEnrollmentResponse> EnrollFingerprintAsync(BiometricEnrollmentRequest request);
    Task<BiometricEnrollmentResponse> EnrollFacialAsync(BiometricEnrollmentRequest request);
}

public class BiometricService : IBiometricService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BiometricService> _logger;
    private readonly BiometricSettings _settings;

    public BiometricService(
        HttpClient httpClient,
        ILogger<BiometricService> logger,
        IOptions<BiometricSettings> settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;
        
        // Configure HTTP client for Biometric Service
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("ApiKey", _settings.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("TenantId", _settings.TenantId);
    }

    public async Task<BiometricVerificationResponse> VerifyFingerprintAsync(BiometricVerificationRequest request)
    {
        try
        {
            _logger.LogInformation("Verifying fingerprint for BVN: {BVN}", request.BVN);

            var response = await _httpClient.PostAsJsonAsync("/api/v1/biometrics/fingerprint/verify", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BiometricVerificationResponse>();
                _logger.LogInformation("Fingerprint verification result for BVN {BVN}: {Result}", 
                    request.BVN, result?.Matched);
                return result;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Fingerprint verification failed: {ErrorMessage}", errorContent);
            
            return new BiometricVerificationResponse
            {
                Success = false,
                Message = $"Fingerprint verification failed: {errorContent}",
                Matched = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during fingerprint verification for BVN: {BVN}", request.BVN);
            
            return new BiometricVerificationResponse
            {
                Success = false,
                Message = $"Exception: {ex.Message}",
                Matched = false
            };
        }
    }

    public async Task<BiometricVerificationResponse> VerifyFacialAsync(BiometricVerificationRequest request)
    {
        try
        {
            _logger.LogInformation("Verifying facial biometrics for BVN: {BVN}", request.BVN);

            var response = await _httpClient.PostAsJsonAsync("/api/v1/biometrics/facial/verify", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BiometricVerificationResponse>();
                _logger.LogInformation("Facial verification result for BVN {BVN}: {Result}", 
                    request.BVN, result?.Matched);
                return result;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Facial verification failed: {ErrorMessage}", errorContent);
            
            return new BiometricVerificationResponse
            {
                Success = false,
                Message = $"Facial verification failed: {errorContent}",
                Matched = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during facial verification for BVN: {BVN}", request.BVN);
            
            return new BiometricVerificationResponse
            {
                Success = false,
                Message = $"Exception: {ex.Message}",
                Matched = false
            };
        }
    }

    public async Task<BiometricEnrollmentResponse> EnrollFingerprintAsync(BiometricEnrollmentRequest request)
    {
        try
        {
            _logger.LogInformation("Enrolling fingerprint for BVN: {BVN}", request.BVN);

            var response = await _httpClient.PostAsJsonAsync("/api/v1/biometrics/fingerprint/enroll", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BiometricEnrollmentResponse>();
                _logger.LogInformation("Fingerprint enrollment successful for BVN: {BVN}", request.BVN);
                return result;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Fingerprint enrollment failed: {ErrorMessage}", errorContent);
            
            return new BiometricEnrollmentResponse
            {
                Success = false,
                Message = $"Fingerprint enrollment failed: {errorContent}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during fingerprint enrollment for BVN: {BVN}", request.BVN);
            
            return new BiometricEnrollmentResponse
            {
                Success = false,
                Message = $"Exception: {ex.Message}"
            };
        }
    }

    public async Task<BiometricEnrollmentResponse> EnrollFacialAsync(BiometricEnrollmentRequest request)
    {
        try
        {
            _logger.LogInformation("Enrolling facial biometrics for BVN: {BVN}", request.BVN);

            var response = await _httpClient.PostAsJsonAsync("/api/v1/biometrics/facial/enroll", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BiometricEnrollmentResponse>();
                _logger.LogInformation("Facial enrollment successful for BVN: {BVN}", request.BVN);
                return result;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Facial enrollment failed: {ErrorMessage}", errorContent);
            
            return new BiometricEnrollmentResponse
            {
                Success = false,
                Message = $"Facial enrollment failed: {errorContent}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during facial enrollment for BVN: {BVN}", request.BVN);
            
            return new BiometricEnrollmentResponse
            {
                Success = false,
                Message = $"Exception: {ex.Message}"
            };
        }
    }
}

public class BiometricVerificationRequest
{
    public string BVN { get; set; } = string.Empty;
    public string BiometricData { get; set; } = string.Empty; // Base64 encoded biometric data
    public string DeviceId { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string FingerPosition { get; set; } = string.Empty; // For fingerprint: LEFT_THUMB, RIGHT_INDEX, etc.
}

public class BiometricVerificationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool Matched { get; set; }
    public decimal MatchScore { get; set; } // 0-100 confidence score
    public string VerificationId { get; set; } = string.Empty;
    public DateTime VerificationTime { get; set; }
}

public class BiometricEnrollmentRequest
{
    public string BVN { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string BiometricData { get; set; } = string.Empty; // Base64 encoded biometric data
    public string DeviceId { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string FingerPosition { get; set; } = string.Empty; // For fingerprint: LEFT_THUMB, RIGHT_INDEX, etc.
}

public class BiometricEnrollmentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string EnrollmentId { get; set; } = string.Empty;
    public DateTime EnrollmentTime { get; set; }
    public int Quality { get; set; } // 0-100 quality score
}
