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
    public class JournalEntriesController : ControllerBase
    {
        private readonly IJournalEntryService _journalEntryService;
        private readonly IFinancialPeriodService _financialPeriodService;

        public JournalEntriesController(
            IJournalEntryService journalEntryService,
            IFinancialPeriodService financialPeriodService)
        {
            _journalEntryService = journalEntryService;
            _financialPeriodService = financialPeriodService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<JournalEntry>> GetJournalEntryById(string id)
        {
            var journalEntry = await _journalEntryService.GetByIdAsync(id);
            if (journalEntry == null)
            {
                return NotFound();
            }

            return Ok(journalEntry);
        }

        [HttpGet("number/{journalNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<JournalEntry>> GetJournalEntryByNumber(string journalNumber)
        {
            var journalEntry = await _journalEntryService.GetByJournalNumberAsync(journalNumber);
            if (journalEntry == null)
            {
                return NotFound();
            }

            return Ok(journalEntry);
        }

        [HttpGet("status/{status}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<JournalEntry>>> GetJournalEntriesByStatus(JournalEntryStatus status)
        {
            var journalEntries = await _journalEntryService.GetByStatusAsync(status);
            return Ok(journalEntries);
        }

        [HttpGet("date-range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<JournalEntry>>> GetJournalEntriesByDateRange(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest("Start date must be before end date");
            }

            var journalEntries = await _journalEntryService.GetByDateRangeAsync(startDate, endDate);
            return Ok(journalEntries);
        }

        [HttpGet("period/{periodId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<JournalEntry>>> GetJournalEntriesByPeriod(string periodId)
        {
            var journalEntries = await _journalEntryService.GetByFinancialPeriodAsync(periodId);
            return Ok(journalEntries);
        }

        [HttpGet("account/{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<JournalEntry>>> GetJournalEntriesByAccount(string accountId)
        {
            var journalEntries = await _journalEntryService.GetByAccountIdAsync(accountId);
            return Ok(journalEntries);
        }

        [HttpGet("pending-approvals")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<JournalEntry>>> GetPendingApprovals()
        {
            var journalEntries = await _journalEntryService.GetPendingApprovalsAsync();
            return Ok(journalEntries);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> CreateJournalEntry(JournalEntry journalEntry)
        {
            try
            {
                // Set the financial period if not provided
                if (string.IsNullOrEmpty(journalEntry.FinancialPeriodId))
                {
                    var period = await _financialPeriodService.GetByDateAsync(journalEntry.EntryDate);
                    if (period == null)
                    {
                        return BadRequest($"No financial period found for the date {journalEntry.EntryDate:yyyy-MM-dd}");
                    }
                    journalEntry.FinancialPeriodId = period.Id;
                }

                // Set the creator to the current user
                journalEntry.CreatedBy = User?.Identity?.Name ?? "system";
                
                // Set the creation date to all lines
                if (journalEntry.JournalEntryLines != null)
                {
                    foreach (var line in journalEntry.JournalEntryLines)
                    {
                        line.CreatedBy = User?.Identity?.Name ?? "system";
                    }
                }

                var journalEntryId = await _journalEntryService.CreateJournalEntryAsync(journalEntry);
                return CreatedAtAction(nameof(GetJournalEntryById), new { id = journalEntryId }, journalEntryId);
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
        public async Task<IActionResult> UpdateJournalEntry(string id, JournalEntry journalEntry)
        {
            if (id != journalEntry.Id?.ToString())
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                // Set the modifier to the current user
                journalEntry.LastModifiedBy = User?.Identity?.Name ?? "system";
                
                // Set the modifier to all lines
                if (journalEntry.JournalEntryLines != null)
                {
                    foreach (var line in journalEntry.JournalEntryLines)
                    {
                        if (string.IsNullOrEmpty(line.CreatedBy))
                        {
                            line.CreatedBy = User?.Identity?.Name ?? "system";
                        }
                        else
                        {
                            line.LastModifiedBy = User?.Identity?.Name ?? "system";
                        }
                    }
                }

                await _journalEntryService.UpdateJournalEntryAsync(journalEntry);
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

        [HttpPatch("{id}/submit")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SubmitForApproval(string id)
        {
            try
            {
                await _journalEntryService.SubmitForApprovalAsync(id, User?.Identity?.Name ?? "system");
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

        [HttpPatch("{id}/approve")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Approver,Admin")]
        public async Task<IActionResult> ApproveJournalEntry(string id)
        {
            try
            {
                await _journalEntryService.ApproveJournalEntryAsync(id, User?.Identity?.Name ?? "system");
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

        [HttpPatch("{id}/reject")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Approver,Admin")]
        public async Task<IActionResult> RejectJournalEntry(string id, [FromBody] string rejectionReason)
        {
            try
            {
                await _journalEntryService.RejectJournalEntryAsync(id, User?.Identity?.Name ?? "system", rejectionReason);
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

        [HttpPatch("{id}/post")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Accountant,Admin")]
        public async Task<IActionResult> PostJournalEntry(string id)
        {
            try
            {
                await _journalEntryService.PostJournalEntryAsync(id, User?.Identity?.Name ?? "system");
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

        [HttpPatch("{id}/reverse")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Accountant,Admin")]
        public async Task<ActionResult<string>> ReverseJournalEntry(string id, [FromBody] string reversalReason)
        {
            try
            {
                var reversalId = await _journalEntryService.ReverseJournalEntryAsync(id, User?.Identity?.Name ?? "system", reversalReason);
                return Ok(reversalId);
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
        public async Task<ActionResult<string>> GenerateJournalNumber([FromQuery] JournalEntryType entryType)
        {
            try
            {
                var journalNumber = await _journalEntryService.GenerateJournalNumberAsync(entryType);
                return Ok(journalNumber);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/upload-document")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadDocument(string id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            try
            {
                // Save file to storage (implement IFileStorageService as needed)
                var fileStorageService = HttpContext.RequestServices.GetService(typeof(IFileStorageService)) as IFileStorageService;
                var filePath = await fileStorageService.SaveFileAsync(file);

                // Link document to journal entry
                await _journalEntryService.AttachDocumentAsync(id, filePath, User?.Identity?.Name ?? "system");
                return Ok("Document uploaded and attached");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

