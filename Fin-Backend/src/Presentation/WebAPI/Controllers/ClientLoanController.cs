using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using FinTech.Application.Services;
using FinTech.Application.DTOs.ClientPortal;
using FinTech.Domain.Entities.Loans;
using System.Security.Claims;
using System.Linq;

namespace FinTech.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/client/loans")]
    public class ClientLoanController : ControllerBase
    {
        private readonly IClientLoanService _loanService;
        private readonly ILogger<ClientLoanController> _logger;

        public ClientLoanController(IClientLoanService loanService, ILogger<ClientLoanController> logger)
        {
            _loanService = loanService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoanAccount>>> GetClientLoans()
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var loans = await _loanService.GetClientLoansAsync(customerId);
                return Ok(loans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client loans");
                return StatusCode(500, "An error occurred while retrieving loan information");
            }
        }

        [HttpGet("{loanAccountNumber}")]
        public async Task<ActionResult<LoanAccount>> GetLoanDetails(string loanAccountNumber)
        {
            try
            {
                var loan = await _loanService.GetLoanDetailsAsync(loanAccountNumber);
                
                // Verify customer owns the loan
                var customerId = GetCustomerIdFromClaims();
                if (loan.CustomerId != customerId)
                {
                    return Forbid();
                }
                
                return Ok(loan);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving loan details for {LoanAccountNumber}", loanAccountNumber);
                return StatusCode(500, "An error occurred while retrieving loan details");
            }
        }

        [HttpGet("{loanAccountNumber}/schedule")]
        public async Task<ActionResult<IEnumerable<LoanRepaymentSchedule>>> GetLoanRepaymentSchedule(string loanAccountNumber)
        {
            try
            {
                // First verify loan exists and belongs to customer
                var loan = await _loanService.GetLoanDetailsAsync(loanAccountNumber);
                var customerId = GetCustomerIdFromClaims();
                if (loan.CustomerId != customerId)
                {
                    return Forbid();
                }
                
                var schedule = await _loanService.GetLoanRepaymentScheduleAsync(loanAccountNumber);
                return Ok(schedule);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving loan repayment schedule for {LoanAccountNumber}", loanAccountNumber);
                return StatusCode(500, "An error occurred while retrieving loan repayment schedule");
            }
        }

        [HttpGet("{loanAccountNumber}/transactions")]
        public async Task<ActionResult<IEnumerable<LoanTransaction>>> GetLoanTransactions(string loanAccountNumber)
        {
            try
            {
                // First verify loan exists and belongs to customer
                var loan = await _loanService.GetLoanDetailsAsync(loanAccountNumber);
                var customerId = GetCustomerIdFromClaims();
                if (loan.CustomerId != customerId)
                {
                    return Forbid();
                }
                
                var transactions = await _loanService.GetLoanTransactionsAsync(loanAccountNumber);
                return Ok(transactions);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving loan transactions for {LoanAccountNumber}", loanAccountNumber);
                return StatusCode(500, "An error occurred while retrieving loan transactions");
            }
        }

        [HttpPost("apply")]
        public async Task<ActionResult<LoanApplicationRequest>> SubmitLoanApplication([FromBody] LoanApplicationDto applicationDto)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var application = await _loanService.SubmitLoanApplicationAsync(applicationDto, customerId);
                return CreatedAtAction(nameof(GetLoanApplicationStatus), new { applicationId = application.Id }, application);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting loan application");
                return StatusCode(500, "An error occurred while submitting the loan application");
            }
        }

        [HttpGet("applications/{applicationId}")]
        public async Task<ActionResult<LoanApplicationRequest>> GetLoanApplicationStatus(Guid applicationId)
        {
            try
            {
                var application = await _loanService.GetLoanApplicationStatusAsync(applicationId);
                
                // Verify customer owns the application
                var customerId = GetCustomerIdFromClaims();
                if (application.CustomerId != customerId)
                {
                    return Forbid();
                }
                
                return Ok(application);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving loan application status for {ApplicationId}", applicationId);
                return StatusCode(500, "An error occurred while retrieving loan application status");
            }
        }

        [HttpGet("applications")]
        public async Task<ActionResult<IEnumerable<LoanApplicationRequest>>> GetClientLoanApplications()
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var applications = await _loanService.GetClientLoanApplicationsAsync(customerId);
                return Ok(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client loan applications");
                return StatusCode(500, "An error occurred while retrieving loan applications");
            }
        }

        [HttpPost("eligibility")]
        public async Task<ActionResult<LoanEligibility>> CheckLoanEligibility([FromBody] LoanEligibilityCheckDto checkDto)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var eligibility = await _loanService.CheckLoanEligibilityAsync(checkDto, customerId);
                return Ok(eligibility);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking loan eligibility");
                return StatusCode(500, "An error occurred while checking loan eligibility");
            }
        }

        [HttpPost("simulate")]
        public async Task<ActionResult<LoanSimulation>> SimulateLoan([FromBody] LoanSimulationDto simulationDto)
        {
            try
            {
                var simulation = await _loanService.SimulateLoanAsync(simulationDto);
                return Ok(simulation);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error simulating loan");
                return StatusCode(500, "An error occurred while simulating the loan");
            }
        }

        [HttpPost("repay")]
        public async Task<ActionResult> MakeLoanRepayment([FromBody] LoanRepaymentDto repaymentDto)
        {
            try
            {
                var customerId = GetCustomerIdFromClaims();
                var result = await _loanService.MakeLoanRepaymentAsync(repaymentDto, customerId);
                
                if (result)
                {
                    return Ok(new { message = "Loan repayment initiated successfully" });
                }
                
                return BadRequest("Loan repayment failed");
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
                _logger.LogError(ex, "Error making loan repayment");
                return StatusCode(500, "An error occurred while processing the loan repayment");
            }
        }

        private Guid GetCustomerIdFromClaims()
        {
            var customerId = User.Claims.FirstOrDefault(c => c.Type == "CustomerId")?.Value;
            if (string.IsNullOrEmpty(customerId))
            {
                throw new UnauthorizedAccessException("Customer ID not found in claims");
            }
            return Guid.Parse(customerId);
        }
    }
}