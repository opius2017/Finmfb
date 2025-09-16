using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using FinTech.Application.Services;
using FinTech.Application.DTOs.ClientPortal;
using FinTech.Domain.Entities.ClientPortal;
using System.Security.Claims;
using System.Linq;

namespace FinTech.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/client/payments")]
    public class ClientPaymentController : ControllerBase
    {
        private readonly IClientPaymentService _paymentService;
        private readonly ILogger<ClientPaymentController> _logger;

        public ClientPaymentController(IClientPaymentService paymentService, ILogger<ClientPaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        // Fund Transfers
        [HttpPost("transfers")]
        public async Task<ActionResult<TransferResult>> TransferFunds([FromBody] FundTransferDto transferDto)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                
                // Add IP and user agent for activity logging
                transferDto.IpAddress = GetClientIpAddress();
                transferDto.UserAgent = GetUserAgent();
                
                var result = await _paymentService.TransferFundsAsync(transferDto, customerId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex.Message);
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing fund transfer");
                return StatusCode(500, "An error occurred while processing the fund transfer");
            }
        }

        [HttpGet("transfers/templates")]
        public async Task<ActionResult<IEnumerable<SavedTransferTemplate>>> GetSavedTransferTemplates()
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var templates = await _paymentService.GetSavedTransferTemplatesAsync(customerId);
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving saved transfer templates");
                return StatusCode(500, "An error occurred while retrieving saved transfer templates");
            }
        }

        [HttpPost("transfers/templates")]
        public async Task<ActionResult<SavedTransferTemplate>> SaveTransferTemplate([FromBody] SaveTransferTemplateDto templateDto)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var template = await _paymentService.SaveTransferTemplateAsync(templateDto, customerId);
                return CreatedAtAction(nameof(GetSavedTransferTemplates), template);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex.Message);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving transfer template");
                return StatusCode(500, "An error occurred while saving the transfer template");
            }
        }

        [HttpDelete("transfers/templates/{templateId}")]
        public async Task<ActionResult> DeleteTransferTemplate(Guid templateId)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var result = await _paymentService.DeleteTransferTemplateAsync(templateId, customerId);
                return Ok(new { success = result });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex.Message);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transfer template");
                return StatusCode(500, "An error occurred while deleting the transfer template");
            }
        }

        // Bill Payments
        [HttpPost("bills")]
        public async Task<ActionResult<PaymentResult>> PayBill([FromBody] BillPaymentDto paymentDto)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                
                // Add IP and user agent for activity logging
                paymentDto.IpAddress = GetClientIpAddress();
                paymentDto.UserAgent = GetUserAgent();
                
                var result = await _paymentService.PayBillAsync(paymentDto, customerId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex.Message);
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bill payment");
                return StatusCode(500, "An error occurred while processing the bill payment");
            }
        }

        [HttpGet("bills/payees")]
        public async Task<ActionResult<IEnumerable<SavedPayee>>> GetSavedPayees()
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var payees = await _paymentService.GetSavedPayeesAsync(customerId);
                return Ok(payees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving saved payees");
                return StatusCode(500, "An error occurred while retrieving saved payees");
            }
        }

        [HttpPost("bills/payees")]
        public async Task<ActionResult<SavedPayee>> SavePayee([FromBody] SavePayeeDto payeeDto)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var payee = await _paymentService.SavePayeeAsync(payeeDto, customerId);
                return CreatedAtAction(nameof(GetSavedPayees), payee);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving payee");
                return StatusCode(500, "An error occurred while saving the payee");
            }
        }

        [HttpDelete("bills/payees/{payeeId}")]
        public async Task<ActionResult> DeletePayee(Guid payeeId)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var result = await _paymentService.DeletePayeeAsync(payeeId, customerId);
                return Ok(new { success = result });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex.Message);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payee");
                return StatusCode(500, "An error occurred while deleting the payee");
            }
        }

        [HttpGet("bills/billers")]
        public async Task<ActionResult<IEnumerable<BillerInfo>>> GetBillerDirectory()
        {
            try
            {
                var billers = await _paymentService.GetBillerDirectoryAsync();
                return Ok(billers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving biller directory");
                return StatusCode(500, "An error occurred while retrieving the biller directory");
            }
        }

        // Recurring Payments
        [HttpPost("recurring")]
        public async Task<ActionResult<RecurringPayment>> ScheduleRecurringPayment([FromBody] RecurringPaymentDto recurringDto)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var result = await _paymentService.ScheduleRecurringPaymentAsync(recurringDto, customerId);
                return CreatedAtAction(nameof(GetRecurringPayments), result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex.Message);
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling recurring payment");
                return StatusCode(500, "An error occurred while scheduling the recurring payment");
            }
        }

        [HttpGet("recurring")]
        public async Task<ActionResult<IEnumerable<RecurringPayment>>> GetRecurringPayments()
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var payments = await _paymentService.GetRecurringPaymentsAsync(customerId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recurring payments");
                return StatusCode(500, "An error occurred while retrieving recurring payments");
            }
        }

        [HttpDelete("recurring/{recurringPaymentId}")]
        public async Task<ActionResult> CancelRecurringPayment(Guid recurringPaymentId)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var result = await _paymentService.CancelRecurringPaymentAsync(recurringPaymentId, customerId);
                return Ok(new { success = result });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex.Message);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling recurring payment");
                return StatusCode(500, "An error occurred while cancelling the recurring payment");
            }
        }

        [HttpPut("recurring/{recurringPaymentId}")]
        public async Task<ActionResult> UpdateRecurringPayment(Guid recurringPaymentId, [FromBody] RecurringPaymentUpdateDto updateDto)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var result = await _paymentService.UpdateRecurringPaymentAsync(recurringPaymentId, updateDto, customerId);
                return Ok(new { success = result });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex.Message);
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recurring payment");
                return StatusCode(500, "An error occurred while updating the recurring payment");
            }
        }

        // Payment History
        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<PaymentTransaction>>> GetPaymentHistory([FromQuery] PaymentHistoryRequestDto requestDto)
        {
            try
            {
                // Set default values if not provided
                if (requestDto.Page <= 0) requestDto.Page = 1;
                if (requestDto.PageSize <= 0) requestDto.PageSize = 20;
                
                var customerId = GetCustomerIdFromClaims();
                var history = await _paymentService.GetPaymentHistoryAsync(requestDto, customerId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment history");
                return StatusCode(500, "An error occurred while retrieving payment history");
            }
        }

        [HttpGet("history/{transactionId}")]
        public async Task<ActionResult<PaymentTransaction>> GetPaymentDetails(Guid transactionId)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var transaction = await _paymentService.GetPaymentDetailsAsync(transactionId, customerId);
                return Ok(transaction);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex.Message);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment details");
                return StatusCode(500, "An error occurred while retrieving payment details");
            }
        }

        // Helper methods
        private Guid GetCustomerIdFromClaims()
        {
            var customerId = User.Claims.FirstOrDefault(c => c.Type == "CustomerId")?.Value;
            if (string.IsNullOrEmpty(customerId))
            {
                throw new UnauthorizedAccessException("Customer ID not found in claims");
            }
            return Guid.Parse(customerId);
        }

        private string GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private string GetUserAgent()
        {
            return HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown";
        }
    }
}