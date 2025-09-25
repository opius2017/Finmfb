using System;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Application.Services.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTech.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FinancialAnalyticsController : ControllerBase
    {
        private readonly IFinancialAnalyticsService _financialAnalyticsService;
        
        public FinancialAnalyticsController(IFinancialAnalyticsService financialAnalyticsService)
        {
            _financialAnalyticsService = financialAnalyticsService ?? throw new ArgumentNullException(nameof(financialAnalyticsService));
        }
        
        /// <summary>
        /// Calculates financial ratios for a specific period
        /// </summary>
        /// <param name="financialPeriodId">ID of the financial period</param>
        /// <param name="includeComparativePeriod">Whether to include comparative data from the previous period</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Financial ratios</returns>
        [HttpGet("ratios/{financialPeriodId}")]
        public async Task<IActionResult> GetFinancialRatios(
            string financialPeriodId,
            [FromQuery] bool includeComparativePeriod = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _financialAnalyticsService.CalculateFinancialRatiosAsync(
                    financialPeriodId,
                    includeComparativePeriod,
                    cancellationToken);
                    
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Generates trend analysis for specified accounts or financial metrics
        /// </summary>
        /// <param name="numberOfPeriods">Number of periods to include in the analysis</param>
        /// <param name="endPeriodId">ID of the end period (optional, uses current period if not specified)</param>
        /// <param name="accountNumbers">Account numbers to analyze (optional)</param>
        /// <param name="metrics">Financial metrics to analyze (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Trend analysis</returns>
        [HttpGet("trends")]
        public async Task<IActionResult> GetTrendAnalysis(
            [FromQuery] int numberOfPeriods,
            [FromQuery] string endPeriodId = null,
            [FromQuery] string[] accountNumbers = null,
            [FromQuery] string[] metrics = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _financialAnalyticsService.GenerateTrendAnalysisAsync(
                    numberOfPeriods,
                    endPeriodId,
                    accountNumbers,
                    metrics,
                    cancellationToken);
                    
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Calculates key financial KPIs for a specific period
        /// </summary>
        /// <param name="financialPeriodId">ID of the financial period</param>
        /// <param name="includeComparativePeriod">Whether to include comparative data from the previous period</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Financial KPIs</returns>
        [HttpGet("kpis/{financialPeriodId}")]
        public async Task<IActionResult> GetFinancialKpis(
            string financialPeriodId,
            [FromQuery] bool includeComparativePeriod = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _financialAnalyticsService.CalculateFinancialKpisAsync(
                    financialPeriodId,
                    includeComparativePeriod,
                    cancellationToken);
                    
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Generates a profitability summary by segment
        /// </summary>
        /// <param name="financialPeriodId">ID of the financial period</param>
        /// <param name="segmentType">Type of segment (Branch, Product, Customer, etc.)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Profitability summary</returns>
        [HttpGet("profitability/{financialPeriodId}")]
        public async Task<IActionResult> GetProfitabilitySummary(
            string financialPeriodId,
            [FromQuery] string segmentType = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _financialAnalyticsService.GenerateProfitabilitySummaryAsync(
                    financialPeriodId,
                    segmentType,
                    cancellationToken);
                    
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
