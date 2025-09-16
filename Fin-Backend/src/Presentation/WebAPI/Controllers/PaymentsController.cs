using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FinTech.Core.Application.Services.Integrations;

namespace FinTech.Presentation.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentGatewayService paymentGatewayService,
        ILogger<PaymentsController> logger)
    {
        _paymentGatewayService = paymentGatewayService;
        _logger = logger;
    }

    [HttpPost("initiate")]
    public async Task<IActionResult> InitiatePayment([FromBody] PaymentInitiationRequest request)
    {
        try
        {
            // Generate reference if not provided
            if (string.IsNullOrEmpty(request.Reference))
            {
                request.Reference = $"TX-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }

            var result = await _paymentGatewayService.InitiatePaymentAsync(request);
            
            if (result.Status)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while initiating payment");
            return StatusCode(500, new { Status = false, Message = "An error occurred while initiating payment" });
        }
    }

    [HttpGet("verify/{reference}")]
    public async Task<IActionResult> VerifyPayment(string reference)
    {
        try
        {
            var result = await _paymentGatewayService.VerifyPaymentAsync(reference);
            
            if (result.Status)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while verifying payment");
            return StatusCode(500, new { Status = false, Message = "An error occurred while verifying payment" });
        }
    }

    [HttpPost("refund")]
    public async Task<IActionResult> InitiateRefund([FromBody] PaymentRefundRequest request)
    {
        try
        {
            var result = await _paymentGatewayService.InitiateRefundAsync(request);
            
            if (result.Status)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while initiating refund");
            return StatusCode(500, new { Status = false, Message = "An error occurred while initiating refund" });
        }
    }

    [HttpPost("recurring/setup")]
    public async Task<IActionResult> SetupRecurringBilling([FromBody] RecurringBillingRequest request)
    {
        try
        {
            // Generate reference if not provided
            if (string.IsNullOrEmpty(request.Reference))
            {
                request.Reference = $"RECUR-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }

            var result = await _paymentGatewayService.SetupRecurringBillingAsync(request);
            
            if (result.Status)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while setting up recurring billing");
            return StatusCode(500, new { Status = false, Message = "An error occurred while setting up recurring billing" });
        }
    }

    [HttpPost("recurring/cancel/{recurringId}")]
    public async Task<IActionResult> CancelRecurringBilling(string recurringId)
    {
        try
        {
            var result = await _paymentGatewayService.CancelRecurringBillingAsync(recurringId);
            
            if (result)
            {
                return Ok(new { Status = true, Message = "Recurring billing cancelled successfully" });
            }
            
            return BadRequest(new { Status = false, Message = "Failed to cancel recurring billing" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cancelling recurring billing");
            return StatusCode(500, new { Status = false, Message = "An error occurred while cancelling recurring billing" });
        }
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook()
    {
        try
        {
            // Read and parse the request body
            string requestBody;
            using (var reader = new System.IO.StreamReader(Request.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            _logger.LogInformation("Received payment webhook: {RequestBody}", requestBody);

            // TODO: Validate webhook signature using the webhook secret
            // For now, just acknowledge receipt
            
            return Ok(new { Status = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing payment webhook");
            return StatusCode(500, new { Status = false, Message = "An error occurred while processing payment webhook" });
        }
    }
}