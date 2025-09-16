using System;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Application.Services.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTech.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BudgetingController : ControllerBase
    {
        private readonly IBudgetingService _budgetingService;
        
        public BudgetingController(IBudgetingService budgetingService)
        {
            _budgetingService = budgetingService ?? throw new ArgumentNullException(nameof(budgetingService));
        }
        
        /// <summary>
        /// Creates a new budget
        /// </summary>
        /// <param name="budgetDto">Budget creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created budget</returns>
        [HttpPost]
        public async Task<IActionResult> CreateBudget(
            [FromBody] CreateBudgetDto budgetDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _budgetingService.CreateBudgetAsync(budgetDto, cancellationToken);
                return CreatedAtAction(nameof(GetBudget), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Gets a budget by ID
        /// </summary>
        /// <param name="id">Budget ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Budget</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBudget(
            string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _budgetingService.GetBudgetAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Updates an existing budget
        /// </summary>
        /// <param name="id">Budget ID</param>
        /// <param name="budgetDto">Budget update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated budget</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBudget(
            string id,
            [FromBody] UpdateBudgetDto budgetDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _budgetingService.UpdateBudgetAsync(id, budgetDto, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Deletes a budget
        /// </summary>
        /// <param name="id">Budget ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success indicator</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBudget(
            string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _budgetingService.DeleteBudgetAsync(id, cancellationToken);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Generates a variance analysis comparing budget to actuals
        /// </summary>
        /// <param name="budgetId">Budget ID</param>
        /// <param name="financialPeriodId">Financial period ID</param>
        /// <param name="includeDetails">Whether to include account-level details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Budget variance analysis</returns>
        [HttpGet("{budgetId}/variance/{financialPeriodId}")]
        public async Task<IActionResult> GetVarianceAnalysis(
            string budgetId,
            string financialPeriodId,
            [FromQuery] bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _budgetingService.GenerateVarianceAnalysisAsync(
                    budgetId,
                    financialPeriodId,
                    includeDetails,
                    cancellationToken);
                    
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Gets all budgets for a specific financial period
        /// </summary>
        /// <param name="financialPeriodId">Financial period ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of budgets</returns>
        [HttpGet("period/{financialPeriodId}")]
        public async Task<IActionResult> GetBudgetsForPeriod(
            string financialPeriodId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _budgetingService.GetBudgetsForPeriodAsync(financialPeriodId, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Gets all budgets for a specific account
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of budgets</returns>
        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetBudgetsForAccount(
            string accountId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _budgetingService.GetBudgetsForAccountAsync(accountId, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Calculates budget performance metrics
        /// </summary>
        /// <param name="budgetId">Budget ID</param>
        /// <param name="financialPeriodId">Financial period ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Budget performance metrics</returns>
        [HttpGet("{budgetId}/performance/{financialPeriodId}")]
        public async Task<IActionResult> GetBudgetPerformance(
            string budgetId,
            string financialPeriodId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _budgetingService.CalculateBudgetPerformanceAsync(
                    budgetId,
                    financialPeriodId,
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