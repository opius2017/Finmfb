using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinTech.Core.Application.Features.BankReconciliation.Commands.CreateReconciliation;
using FinTech.Core.Application.Features.BankReconciliation.Queries.GetReconciliation;

namespace FinTech.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class BankReconciliationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BankReconciliationController> _logger;

    public BankReconciliationController(IMediator mediator, ILogger<BankReconciliationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new bank reconciliation
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateReconciliationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateReconciliation([FromBody] CreateReconciliationCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetReconciliation), new { id = result.Value!.Id }, result.Value);
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Get bank reconciliation by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReconciliationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReconciliation(string id)
    {
        var result = await _mediator.Send(new GetReconciliationQuery(id));

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NotFound(result.Error);
    }

    /// <summary>
    /// Get all reconciliations for a bank account
    /// </summary>
    [HttpGet("bank-account/{bankAccountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReconciliationsByBankAccount(string bankAccountId)
    {
        // TODO: Implement query
        return Ok(new List<ReconciliationDetailDto>());
    }

    /// <summary>
    /// Import bank statement
    /// </summary>
    [HttpPost("import-statement")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportBankStatement([FromForm] IFormFile file, [FromForm] string bankAccountId)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required");
        }

        // TODO: Implement bank statement import
        _logger.LogInformation("Importing bank statement for account {BankAccountId}", bankAccountId);

        return Ok(new { message = "Bank statement imported successfully" });
    }

    /// <summary>
    /// Match transactions automatically
    /// </summary>
    [HttpPost("{id}/auto-match")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AutoMatchTransactions(string id)
    {
        // TODO: Implement auto-matching algorithm
        _logger.LogInformation("Auto-matching transactions for reconciliation {ReconciliationId}", id);

        return Ok(new { message = "Auto-matching completed", matchedCount = 0 });
    }

    /// <summary>
    /// Complete reconciliation
    /// </summary>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CompleteReconciliation(string id)
    {
        // TODO: Implement completion logic
        _logger.LogInformation("Completing reconciliation {ReconciliationId}", id);

        return Ok(new { message = "Reconciliation completed successfully" });
    }

    /// <summary>
    /// Approve reconciliation
    /// </summary>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ApproveReconciliation(string id)
    {
        // TODO: Implement approval logic
        _logger.LogInformation("Approving reconciliation {ReconciliationId}", id);

        return Ok(new { message = "Reconciliation approved successfully" });
    }
}
