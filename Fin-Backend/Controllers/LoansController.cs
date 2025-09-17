using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FinTech.Application.DTOs.Loans;
using FinTech.Application.Interfaces.Loans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinTech.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;
        private readonly IMapper _mapper;
        private readonly ILogger<LoansController> _logger;

        public LoansController(
            ILoanService loanService,
            IMapper mapper,
            ILogger<LoansController> logger)
        {
            _loanService = loanService ?? throw new ArgumentNullException(nameof(loanService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all loans
        /// </summary>
        /// <returns>List of loans</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetLoans()
        {
            var loans = await _loanService.GetAllLoansAsync();
            return Ok(_mapper.Map<IEnumerable<LoanDto>>(loans));
        }

        /// <summary>
        /// Gets a loan by ID
        /// </summary>
        /// <param name="id">Loan ID</param>
        /// <returns>Loan details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanDto>> GetLoan(string id)
        {
            var loan = await _loanService.GetLoanByIdAsync(id);
            if (loan == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<LoanDto>(loan));
        }

        /// <summary>
        /// Gets loans for a customer
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>List of loans for the customer</returns>
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetLoansByCustomer(string customerId)
        {
            var loans = await _loanService.GetLoansByCustomerIdAsync(customerId);
            return Ok(_mapper.Map<IEnumerable<LoanDto>>(loans));
        }

        /// <summary>
        /// Disburses a loan
        /// </summary>
        /// <param name="id">Loan ID</param>
        /// <param name="disbursementDto">Disbursement details</param>
        /// <returns>Updated loan details</returns>
        [HttpPost("{id}/disburse")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanDto>> DisburseLoan(string id, [FromBody] LoanDisbursementDto disbursementDto)
        {
            try
            {
                var loan = await _loanService.DisburseLoanAsync(
                    id,
                    disbursementDto.Amount,
                    disbursementDto.DisbursedTo,
                    disbursementDto.Reference,
                    disbursementDto.Description);

                return Ok(_mapper.Map<LoanDto>(loan));
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(ex.Message);
                }
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Records a loan repayment
        /// </summary>
        /// <param name="id">Loan ID</param>
        /// <param name="repaymentDto">Repayment details</param>
        /// <returns>Transaction details</returns>
        [HttpPost("{id}/repayment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanTransactionDto>> RecordRepayment(string id, [FromBody] LoanRepaymentDto repaymentDto)
        {
            try
            {
                var transaction = await _loanService.RecordRepaymentAsync(
                    id,
                    repaymentDto.TotalAmount,
                    repaymentDto.PrincipalAmount,
                    repaymentDto.InterestAmount,
                    repaymentDto.FeesAmount,
                    repaymentDto.PenaltyAmount,
                    repaymentDto.Reference,
                    repaymentDto.Description);

                return Ok(_mapper.Map<LoanTransactionDto>(transaction));
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(ex.Message);
                }
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Writes off a loan
        /// </summary>
        /// <param name="id">Loan ID</param>
        /// <param name="writeOffDto">Write-off details</param>
        /// <returns>Transaction details</returns>
        [HttpPost("{id}/write-off")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanTransactionDto>> WriteOffLoan(string id, [FromBody] LoanWriteOffDto writeOffDto)
        {
            try
            {
                var transaction = await _loanService.WriteOffLoanAsync(
                    id,
                    writeOffDto.Reason,
                    writeOffDto.ApprovedBy);

                return Ok(_mapper.Map<LoanTransactionDto>(transaction));
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(ex.Message);
                }
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Reschedules a loan
        /// </summary>
        /// <param name="id">Loan ID</param>
        /// <param name="rescheduleDto">Reschedule details</param>
        /// <returns>Updated loan details</returns>
        [HttpPost("{id}/reschedule")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanDto>> RescheduleLoan(string id, [FromBody] LoanRescheduleDto rescheduleDto)
        {
            try
            {
                var loan = await _loanService.RescheduleLoanAsync(
                    id,
                    rescheduleDto.NewEndDate,
                    rescheduleDto.Reason,
                    rescheduleDto.ApprovedBy);

                return Ok(_mapper.Map<LoanDto>(loan));
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(ex.Message);
                }
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets all transactions for a loan
        /// </summary>
        /// <param name="id">Loan ID</param>
        /// <returns>List of transactions</returns>
        [HttpGet("{id}/transactions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<LoanTransactionDto>>> GetLoanTransactions(string id)
        {
            try
            {
                var transactions = await _loanService.GetLoanTransactionsAsync(id);
                return Ok(_mapper.Map<IEnumerable<LoanTransactionDto>>(transactions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions for loan with ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Gets the repayment schedule for a loan
        /// </summary>
        /// <param name="id">Loan ID</param>
        /// <returns>List of repayment schedule items</returns>
        [HttpGet("{id}/repayment-schedule")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<LoanRepaymentScheduleDto>>> GetLoanRepaymentSchedule(string id)
        {
            try
            {
                var schedule = await _loanService.GetLoanRepaymentScheduleAsync(id);
                return Ok(_mapper.Map<IEnumerable<LoanRepaymentScheduleDto>>(schedule));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting repayment schedule for loan with ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Generates a loan statement
        /// </summary>
        /// <param name="id">Loan ID</param>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <returns>Loan statement</returns>
        [HttpGet("{id}/statement")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanStatement>> GenerateLoanStatement(
            string id, 
            [FromQuery] DateTime fromDate, 
            [FromQuery] DateTime toDate)
        {
            try
            {
                if (toDate < fromDate)
                {
                    return BadRequest("End date must be greater than or equal to start date");
                }

                var statement = await _loanService.GenerateLoanStatementAsync(id, fromDate, toDate);
                return Ok(statement);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating loan statement for loan with ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }
    }

    public class LoanDisbursementDto
    {
        public decimal Amount { get; set; }
        public string DisbursedTo { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
    }

    public class LoanRepaymentDto
    {
        public decimal TotalAmount { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal FeesAmount { get; set; }
        public decimal PenaltyAmount { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
    }

    public class LoanWriteOffDto
    {
        public string Reason { get; set; }
        public string ApprovedBy { get; set; }
    }

    public class LoanRescheduleDto
    {
        public DateTime NewEndDate { get; set; }
        public string Reason { get; set; }
        public string ApprovedBy { get; set; }
    }
}