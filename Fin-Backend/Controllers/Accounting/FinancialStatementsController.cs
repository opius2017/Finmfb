using System;
using System.Threading.Tasks;
using FinTech.Core.Application.Services.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTech.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialStatementsController : ControllerBase
    {
        private readonly IFinancialStatementService _financialStatementService;

        public FinancialStatementsController(IFinancialStatementService financialStatementService)
        {
            _financialStatementService = financialStatementService ?? throw new ArgumentNullException(nameof(financialStatementService));
        }

        /// <summary>
        /// Generates an income statement (profit and loss) for a specific financial period
        /// </summary>
        /// <param name="financialPeriodId">The ID of the financial period</param>
        /// <param name="includeZeroBalances">Whether to include accounts with zero balances</param>
        /// <param name="includeComparativePeriod">Whether to include data from the previous period for comparison</param>
        /// <param name="currencyCode">The currency code for the report (defaults to NGN)</param>
        /// <returns>An income statement DTO</returns>
        [HttpGet("income-statement")]
        public async Task<IActionResult> GetIncomeStatement(
            string financialPeriodId, 
            bool includeZeroBalances = false, 
            bool includeComparativePeriod = false,
            string currencyCode = null)
        {
            try
            {
                var result = await _financialStatementService.GenerateIncomeStatementAsync(
                    financialPeriodId, 
                    includeZeroBalances, 
                    includeComparativePeriod,
                    currencyCode);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Generates a balance sheet for a specific financial period
        /// </summary>
        /// <param name="financialPeriodId">The ID of the financial period</param>
        /// <param name="includeZeroBalances">Whether to include accounts with zero balances</param>
        /// <param name="includeComparativePeriod">Whether to include data from the previous period for comparison</param>
        /// <param name="currencyCode">The currency code for the report (defaults to NGN)</param>
        /// <returns>A balance sheet DTO</returns>
        [HttpGet("balance-sheet")]
        public async Task<IActionResult> GetBalanceSheet(
            string financialPeriodId, 
            bool includeZeroBalances = false, 
            bool includeComparativePeriod = false,
            string currencyCode = null)
        {
            try
            {
                var result = await _financialStatementService.GenerateBalanceSheetAsync(
                    financialPeriodId, 
                    includeZeroBalances, 
                    includeComparativePeriod,
                    currencyCode);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Generates a cash flow statement for a specific financial period
        /// </summary>
        /// <param name="financialPeriodId">The ID of the financial period</param>
        /// <param name="includeZeroBalances">Whether to include items with zero balances</param>
        /// <param name="includeComparativePeriod">Whether to include data from the previous period for comparison</param>
        /// <param name="currencyCode">The currency code for the report (defaults to NGN)</param>
        /// <returns>A cash flow statement DTO</returns>
        [HttpGet("cash-flow-statement")]
        public async Task<IActionResult> GetCashFlowStatement(
            string financialPeriodId, 
            bool includeZeroBalances = false, 
            bool includeComparativePeriod = false,
            string currencyCode = null)
        {
            try
            {
                var result = await _financialStatementService.GenerateCashFlowStatementAsync(
                    financialPeriodId, 
                    includeZeroBalances, 
                    includeComparativePeriod,
                    currencyCode);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Generates a statement of changes in equity for a specific financial period
        /// </summary>
        /// <param name="financialPeriodId">The ID of the financial period</param>
        /// <param name="includeZeroBalances">Whether to include items with zero balances</param>
        /// <param name="includeComparativePeriod">Whether to include data from the previous period for comparison</param>
        /// <param name="currencyCode">The currency code for the report (defaults to NGN)</param>
        /// <returns>An equity change statement DTO</returns>
        [HttpGet("equity-change-statement")]
        public async Task<IActionResult> GetEquityChangeStatement(
            string financialPeriodId, 
            bool includeZeroBalances = false, 
            bool includeComparativePeriod = false,
            string currencyCode = null)
        {
            try
            {
                var result = await _financialStatementService.GenerateEquityChangeStatementAsync(
                    financialPeriodId, 
                    includeZeroBalances, 
                    includeComparativePeriod,
                    currencyCode);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
