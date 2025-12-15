using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.Services.Accounting;
using FinTech.Core.Domain.Entities.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinTech.Controllers.Accounting.Accounting
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChartOfAccountsController : ControllerBase
    {
        private readonly IChartOfAccountService _chartOfAccountService;

        public ChartOfAccountsController(IChartOfAccountService chartOfAccountService)
        {
            _chartOfAccountService = chartOfAccountService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ChartOfAccount>>> GetAllAccounts()
        {
            var accounts = await _chartOfAccountService.GetAllAsync();
            return Ok(accounts);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ChartOfAccount>> GetAccountById(string id)
        {
            var account = await _chartOfAccountService.GetByIdAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            return Ok(account);
        }

        [HttpGet("number/{accountNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ChartOfAccount>> GetAccountByNumber(string accountNumber)
        {
            var account = await _chartOfAccountService.GetByAccountNumberAsync(accountNumber);
            if (account == null)
            {
                return NotFound();
            }

            return Ok(account);
        }

        [HttpGet("type/{accountType}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ChartOfAccount>>> GetAccountsByType(AccountType accountType)
        {
            var accounts = await _chartOfAccountService.GetByTypeAsync(accountType);
            return Ok(accounts);
        }

        [HttpGet("classification/{classification}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ChartOfAccount>>> GetAccountsByClassification(AccountClassification classification)
        {
            var accounts = await _chartOfAccountService.GetByClassificationAsync(classification);
            return Ok(accounts);
        }

        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ChartOfAccount>>> GetActiveAccounts()
        {
            var accounts = await _chartOfAccountService.GetActiveAccountsAsync();
            return Ok(accounts);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> CreateAccount(ChartOfAccount account)
        {
            try
            {
                // Set the creator to the current user, fallback to "system" if null
                account.CreatedBy = User?.Identity?.Name ?? "system";
                var accountId = await _chartOfAccountService.CreateAccountAsync(account);
                return CreatedAtAction(nameof(GetAccountById), new { id = accountId }, accountId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAccount(string id, ChartOfAccount account)
        {
            // Use string comparison for Id, fallback to ToString if ambiguous
            if (id != account.Id?.ToString())
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                // Set the modifier to the current user, fallback to "system" if null
                account.LastModifiedBy = User?.Identity?.Name ?? "system";
                await _chartOfAccountService.UpdateAccountAsync(account);
                return NoContent();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/activate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivateAccount(string id)
        {
            try
            {
                await _chartOfAccountService.ActivateAccountAsync(id, User?.Identity?.Name ?? "system");
                return NoContent();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateAccount(string id)
        {
            try
            {
                await _chartOfAccountService.DeactivateAccountAsync(id, User?.Identity?.Name ?? "system");
                return NoContent();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("generate-number")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> GenerateAccountNumber(
            [FromQuery] AccountType accountType, 
            [FromQuery] AccountClassification classification)
        {
            try
            {
                var accountNumber = await _chartOfAccountService.GenerateAccountNumberAsync(accountType, classification);
                return Ok(accountNumber);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("check-number-exists/{accountNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> CheckAccountNumberExists(string accountNumber)
        {
            var exists = await _chartOfAccountService.AccountNumberExistsAsync(accountNumber);
            return Ok(exists);
        }

        [HttpPost("template/cbn")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> InitializeCbnTemplate([FromQuery] string tenantId)
        {
            try
            {
                var templateService = new ChartOfAccountTemplateService();
                var templateAccounts = templateService.GetCbnTemplate(tenantId);
                var creator = User?.Identity?.Name ?? "system";
                foreach (var account in templateAccounts)
                {
                    account.CreatedBy = creator;
                    await _chartOfAccountService.CreateAccountAsync(account);
                }
                return Created($"/api/chartofaccounts/template/cbn?tenantId={tenantId}", templateAccounts.Count);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("template/ndic")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> InitializeNdicTemplate([FromQuery] string tenantId)
        {
            try
            {
                var templateService = new ChartOfAccountTemplateService();
                var templateAccounts = templateService.GetNdicTemplate(tenantId);
                var creator = User?.Identity?.Name ?? "system";
                foreach (var account in templateAccounts)
                {
                    account.CreatedBy = creator;
                    await _chartOfAccountService.CreateAccountAsync(account);
                }
                return Created($"/api/chartofaccounts/template/ndic?tenantId={tenantId}", templateAccounts.Count);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("template/ifrs")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> InitializeIfrsTemplate([FromQuery] string tenantId)
        {
            try
            {
                var templateService = new ChartOfAccountTemplateService();
                var templateAccounts = templateService.GetIfrsTemplate(tenantId);
                var creator = User?.Identity?.Name ?? "system";
                foreach (var account in templateAccounts)
                {
                    account.CreatedBy = creator;
                    await _chartOfAccountService.CreateAccountAsync(account);
                }
                return Created($"/api/chartofaccounts/template/ifrs?tenantId={tenantId}", templateAccounts.Count);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

