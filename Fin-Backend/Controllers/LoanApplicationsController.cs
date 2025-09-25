using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Application.Interfaces.Loans;
using FinTech.Core.Domain.Entities.Loans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinTech.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LoanApplicationsController : ControllerBase
    {
        private readonly ILoanApplicationService _loanApplicationService;
        private readonly IMapper _mapper;
        private readonly ILogger<LoanApplicationsController> _logger;

        public LoanApplicationsController(
            ILoanApplicationService loanApplicationService,
            IMapper mapper,
            ILogger<LoanApplicationsController> logger)
        {
            _loanApplicationService = loanApplicationService ?? throw new ArgumentNullException(nameof(loanApplicationService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all loan applications
        /// </summary>
        /// <returns>List of loan applications</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LoanApplicationDto>>> GetLoanApplications()
        {
            var loanApplications = await _loanApplicationService.GetAllLoanApplicationsAsync();
            return Ok(_mapper.Map<IEnumerable<LoanApplicationDto>>(loanApplications));
        }

        /// <summary>
        /// Gets a loan application by ID
        /// </summary>
        /// <param name="id">Loan application ID</param>
        /// <returns>Loan application details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanApplicationDto>> GetLoanApplication(string id)
        {
            var loanApplication = await _loanApplicationService.GetLoanApplicationByIdAsync(id);
            if (loanApplication == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<LoanApplicationDto>(loanApplication));
        }

        /// <summary>
        /// Gets loan applications for a customer
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>List of loan applications for the customer</returns>
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LoanApplicationDto>>> GetLoanApplicationsByCustomer(string customerId)
        {
            var loanApplications = await _loanApplicationService.GetLoanApplicationsByCustomerIdAsync(customerId);
            return Ok(_mapper.Map<IEnumerable<LoanApplicationDto>>(loanApplications));
        }

        /// <summary>
        /// Creates a new loan application
        /// </summary>
        /// <param name="createLoanApplicationDto">Loan application data</param>
        /// <returns>Created loan application</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoanApplicationDto>> CreateLoanApplication(CreateLoanApplicationDto createLoanApplicationDto)
        {
            try
            {
                var loanApplication = _mapper.Map<LoanApplication>(createLoanApplicationDto);
                var createdApplication = await _loanApplicationService.CreateLoanApplicationAsync(loanApplication);
                var result = _mapper.Map<LoanApplicationDto>(createdApplication);

                return CreatedAtAction(nameof(GetLoanApplication), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates a loan application
        /// </summary>
        /// <param name="id">Loan application ID</param>
        /// <param name="loanApplicationDto">Updated loan application data</param>
        /// <returns>No content</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateLoanApplication(string id, LoanApplicationDto loanApplicationDto)
        {
            if (id != loanApplicationDto.Id)
            {
                return BadRequest("ID in URL must match ID in request body");
            }

            try
            {
                var loanApplication = _mapper.Map<LoanApplication>(loanApplicationDto);
                await _loanApplicationService.UpdateLoanApplicationAsync(loanApplication);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Submits a loan application for review
        /// </summary>
        /// <param name="id">Loan application ID</param>
        /// <returns>Updated loan application</returns>
        [HttpPost("{id}/submit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanApplicationDto>> SubmitLoanApplication(string id)
        {
            try
            {
                var loanApplication = await _loanApplicationService.SubmitLoanApplicationAsync(id);
                return Ok(_mapper.Map<LoanApplicationDto>(loanApplication));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Approves a loan application
        /// </summary>
        /// <param name="id">Loan application ID</param>
        /// <param name="approvalDto">Approval details</param>
        /// <returns>Updated loan application</returns>
        [HttpPost("{id}/approve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanApplicationDto>> ApproveLoanApplication(string id, [FromBody] LoanApprovalDto approvalDto)
        {
            try
            {
                var loanApplication = await _loanApplicationService.ApproveLoanApplicationAsync(
                    id, 
                    approvalDto.ApprovedBy, 
                    approvalDto.ApprovedAmount, 
                    approvalDto.ApprovedTerm);
                    
                return Ok(_mapper.Map<LoanApplicationDto>(loanApplication));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Rejects a loan application
        /// </summary>
        /// <param name="id">Loan application ID</param>
        /// <param name="rejectionDto">Rejection details</param>
        /// <returns>Updated loan application</returns>
        [HttpPost("{id}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanApplicationDto>> RejectLoanApplication(string id, [FromBody] LoanRejectionDto rejectionDto)
        {
            try
            {
                var loanApplication = await _loanApplicationService.RejectLoanApplicationAsync(id, rejectionDto.RejectionReason);
                return Ok(_mapper.Map<LoanApplicationDto>(loanApplication));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Creates a loan from an approved application
        /// </summary>
        /// <param name="id">Loan application ID</param>
        /// <returns>Created loan details</returns>
        [HttpPost("{id}/create-loan")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanDto>> CreateLoanFromApplication(string id)
        {
            try
            {
                var loan = await _loanApplicationService.CreateLoanFromApplicationAsync(id);
                return Ok(_mapper.Map<LoanDto>(loan));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a loan application
        /// </summary>
        /// <param name="id">Loan application ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteLoanApplication(string id)
        {
            try
            {
                var result = await _loanApplicationService.DeleteLoanApplicationAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class LoanApprovalDto
    {
        public string ApprovedBy { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public int? ApprovedTerm { get; set; }
    }

    public class LoanRejectionDto
    {
        public string RejectionReason { get; set; }
    }
}
