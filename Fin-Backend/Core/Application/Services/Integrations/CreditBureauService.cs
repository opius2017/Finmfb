using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FinTech.Core.Application.Common.Settings;

namespace FinTech.Core.Application.Services.Integrations;

/// <summary>
/// Integration with Credit Bureaus (CRC, XDS) for credit checks and reporting
/// </summary>
public interface ICreditBureauService
{
    Task<CreditReportResponse> GetCreditReportAsync(CreditReportRequest request);
    Task<ScoreReportResponse> GetCreditScoreAsync(string bvn);
    Task<bool> ReportLoanRepaymentAsync(LoanRepaymentReportRequest request);
}

public class CreditBureauService : ICreditBureauService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CreditBureauService> _logger;
    private readonly CreditBureauSettings _settings;

    public CreditBureauService(
        HttpClient httpClient,
        ILogger<CreditBureauService> logger,
        IOptions<CreditBureauSettings> settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;
        
        // Configure HTTP client for Credit Bureau
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("ApiKey", _settings.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("SubscriberId", _settings.SubscriberId);
    }

    public async Task<CreditReportResponse> GetCreditReportAsync(CreditReportRequest request)
    {
        try
        {
            _logger.LogInformation("Requesting credit report for BVN: {BVN}", request.BVN);

            var response = await _httpClient.PostAsJsonAsync("/api/v2/creditreport", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CreditReportResponse>();
                _logger.LogInformation("Credit report successfully retrieved for BVN: {BVN}", request.BVN);
                return result;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve credit report: {ErrorMessage}", errorContent);
            
            return new CreditReportResponse
            {
                Success = false,
                Message = $"Failed to retrieve credit report: {errorContent}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while retrieving credit report for BVN: {BVN}", request.BVN);
            
            return new CreditReportResponse
            {
                Success = false,
                Message = $"Exception: {ex.Message}"
            };
        }
    }

    public async Task<ScoreReportResponse> GetCreditScoreAsync(string bvn)
    {
        try
        {
            _logger.LogInformation("Requesting credit score for BVN: {BVN}", bvn);

            var response = await _httpClient.GetAsync($"/api/v2/creditscore?bvn={bvn}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ScoreReportResponse>();
                _logger.LogInformation("Credit score successfully retrieved for BVN: {BVN}", bvn);
                return result;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to retrieve credit score: {ErrorMessage}", errorContent);
            
            return new ScoreReportResponse
            {
                Success = false,
                Message = $"Failed to retrieve credit score: {errorContent}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while retrieving credit score for BVN: {BVN}", bvn);
            
            return new ScoreReportResponse
            {
                Success = false,
                Message = $"Exception: {ex.Message}"
            };
        }
    }

    public async Task<bool> ReportLoanRepaymentAsync(LoanRepaymentReportRequest request)
    {
        try
        {
            _logger.LogInformation("Reporting loan repayment for BVN: {BVN}", request.BVN);

            var response = await _httpClient.PostAsJsonAsync("/api/v2/report/loan", request);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Loan repayment successfully reported for BVN: {BVN}", request.BVN);
                return true;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to report loan repayment: {ErrorMessage}", errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while reporting loan repayment for BVN: {BVN}", request.BVN);
            return false;
        }
    }
}

public class CreditReportRequest
{
    public string BVN { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty; // Format: YYYY-MM-DD
    public string PhoneNumber { get; set; } = string.Empty;
    public string ReportType { get; set; } = "FULL"; // FULL, SUMMARY, BASIC
    public string RequestId { get; set; } = string.Empty;
}

public class CreditReportResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ReportId { get; set; } = string.Empty;
    public decimal CreditScore { get; set; }
    public string ScoreRating { get; set; } = string.Empty; // EXCELLENT, GOOD, FAIR, POOR, VERY_POOR
    public DateTime ReportDate { get; set; }
    public decimal TotalLoanBalance { get; set; }
    public int ActiveLoans { get; set; }
    public int PaidLoans { get; set; }
    public int DefaultedLoans { get; set; }
    public LoanHistory[] LoanHistory { get; set; } = Array.Empty<LoanHistory>();
    public CreditInquiry[] RecentInquiries { get; set; } = Array.Empty<CreditInquiry>();
}

public class ScoreReportResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ReportId { get; set; } = string.Empty;
    public decimal CreditScore { get; set; }
    public string ScoreRating { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public string RiskCategory { get; set; } = string.Empty;
    public decimal EstimatedDefaultRate { get; set; }
    public ScoreAnalysis ScoreAnalysis { get; set; } = new();
}

public class LoanHistory
{
    public string LenderName { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public decimal OutstandingBalance { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty; // ACTIVE, PAID, DEFAULTED, RESTRUCTURED
    public int DaysInArrears { get; set; }
    public int TimesLate30Plus { get; set; }
    public int TimesLate60Plus { get; set; }
    public int TimesLate90Plus { get; set; }
}

public class CreditInquiry
{
    public string InquiringInstitution { get; set; } = string.Empty;
    public DateTime InquiryDate { get; set; }
    public string InquiryPurpose { get; set; } = string.Empty;
}

public class ScoreAnalysis
{
    public decimal PaymentHistoryImpact { get; set; }
    public decimal OutstandingDebtImpact { get; set; }
    public decimal CreditMixImpact { get; set; }
    public decimal CreditHistoryLengthImpact { get; set; }
    public decimal NewCreditImpact { get; set; }
}

public class LoanRepaymentReportRequest
{
    public string BVN { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string LoanAccountNumber { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty; // ACTIVE, PAID, DEFAULTED, RESTRUCTURED
    public int DaysInArrears { get; set; }
    public DateTime ReportingDate { get; set; }
    public string ReportId { get; set; } = string.Empty;
}
