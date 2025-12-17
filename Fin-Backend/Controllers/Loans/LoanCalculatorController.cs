using System;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Application.Interfaces.Loans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinTech.Controllers.Loans
{
    /// <summary>
    /// Controller for loan calculations
    /// </summary>
    [ApiController]
    [Route("api/loan-calculator")]
    [Authorize]
    public class LoanCalculatorController : ControllerBase
    {
        private readonly ILoanCalculatorService _calculatorService;
        private readonly ILogger<LoanCalculatorController> _logger;

        public LoanCalculatorController(
            ILoanCalculatorService calculatorService,
            ILogger<LoanCalculatorController> logger)
        {
            _calculatorService = calculatorService;
            _logger = logger;
        }

        /// <summary>
        /// Calculate monthly EMI
        /// </summary>
        [HttpPost("calculate-emi")]
        [ProducesResponseType(typeof(EMICalculationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<EMICalculationResponse> CalculateEMI([FromBody] EMICalculationRequest request)
        {
            try
            {
                // Validate parameters
                var validation = _calculatorService.ValidateLoanParameters(
                    request.Principal,
                    request.AnnualInterestRate,
                    request.TenureMonths);

                if (!validation.IsValid)
                {
                    return BadRequest(new { errors = validation.Errors });
                }

                var emi = _calculatorService.CalculateEMI(
                    request.Principal,
                    request.AnnualInterestRate,
                    request.TenureMonths);

                var totalInterest = _calculatorService.CalculateTotalInterest(
                    request.Principal,
                    request.AnnualInterestRate,
                    request.TenureMonths);

                var response = new EMICalculationResponse
                {
                    Principal = request.Principal,
                    AnnualInterestRate = request.AnnualInterestRate,
                    TenureMonths = request.TenureMonths,
                    MonthlyEMI = emi,
                    TotalInterest = totalInterest,
                    TotalPayment = request.Principal + totalInterest
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating EMI");
                return StatusCode(500, new { message = "An error occurred while calculating EMI" });
            }
        }

        /// <summary>
        /// Generate amortization schedule
        /// </summary>
        [HttpPost("amortization-schedule")]
        [ProducesResponseType(typeof(AmortizationScheduleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<AmortizationScheduleDto> GenerateAmortizationSchedule(
            [FromBody] AmortizationScheduleRequest request)
        {
            try
            {
                // Validate parameters
                var validation = _calculatorService.ValidateLoanParameters(
                    request.Principal,
                    request.AnnualInterestRate,
                    request.TenureMonths);

                if (!validation.IsValid)
                {
                    return BadRequest(new { errors = validation.Errors });
                }

                var schedule = _calculatorService.GenerateAmortizationSchedule(request);
                return Ok(schedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating amortization schedule");
                return StatusCode(500, new { message = "An error occurred while generating schedule" });
            }
        }

        /// <summary>
        /// Calculate penalty for overdue payment
        /// </summary>
        [HttpPost("calculate-penalty")]
        [ProducesResponseType(typeof(PenaltyCalculationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<PenaltyCalculationResponse> CalculatePenalty([FromBody] PenaltyCalculationRequest request)
        {
            try
            {
                if (request.OverdueAmount <= 0)
                {
                    return BadRequest(new { message = "Overdue amount must be greater than zero" });
                }

                if (request.DaysOverdue <= 0)
                {
                    return BadRequest(new { message = "Days overdue must be greater than zero" });
                }

                var penalty = _calculatorService.CalculatePenalty(
                    request.OverdueAmount,
                    request.DaysOverdue,
                    request.PenaltyRate);

                var response = new PenaltyCalculationResponse
                {
                    OverdueAmount = request.OverdueAmount,
                    DaysOverdue = request.DaysOverdue,
                    PenaltyRate = request.PenaltyRate,
                    PenaltyAmount = penalty,
                    TotalAmountDue = request.OverdueAmount + penalty
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating penalty");
                return StatusCode(500, new { message = "An error occurred while calculating penalty" });
            }
        }

        /// <summary>
        /// Calculate early repayment details
        /// </summary>
        [HttpPost("early-repayment")]
        [ProducesResponseType(typeof(EarlyRepaymentCalculationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<EarlyRepaymentCalculationDto> CalculateEarlyRepayment(
            [FromBody] EarlyRepaymentRequest request)
        {
            try
            {
                if (request.OutstandingPrincipal <= 0)
                {
                    return BadRequest(new { message = "Outstanding principal must be greater than zero" });
                }

                if (request.EarlyRepaymentAmount <= 0)
                {
                    return BadRequest(new { message = "Early repayment amount must be greater than zero" });
                }

                if (request.EarlyRepaymentAmount > request.OutstandingPrincipal)
                {
                    return BadRequest(new { message = "Early repayment amount cannot exceed outstanding principal" });
                }

                var result = _calculatorService.CalculateEarlyRepayment(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating early repayment");
                return StatusCode(500, new { message = "An error occurred while calculating early repayment" });
            }
        }

        /// <summary>
        /// Calculate outstanding balance
        /// </summary>
        [HttpPost("outstanding-balance")]
        [ProducesResponseType(typeof(OutstandingBalanceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<OutstandingBalanceResponse> CalculateOutstandingBalance(
            [FromBody] OutstandingBalanceRequest request)
        {
            try
            {
                var validation = _calculatorService.ValidateLoanParameters(
                    request.Principal,
                    request.AnnualInterestRate,
                    request.TenureMonths);

                if (!validation.IsValid)
                {
                    return BadRequest(new { errors = validation.Errors });
                }

                if (request.PaymentsMade < 0 || request.PaymentsMade > request.TenureMonths)
                {
                    return BadRequest(new { message = "Invalid number of payments made" });
                }

                var outstandingBalance = _calculatorService.CalculateOutstandingBalance(
                    request.Principal,
                    request.AnnualInterestRate,
                    request.TenureMonths,
                    request.PaymentsMade);

                var response = new OutstandingBalanceResponse
                {
                    Principal = request.Principal,
                    PaymentsMade = request.PaymentsMade,
                    RemainingPayments = request.TenureMonths - request.PaymentsMade,
                    OutstandingBalance = outstandingBalance,
                    PrincipalPaid = request.Principal - outstandingBalance
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating outstanding balance");
                return StatusCode(500, new { message = "An error occurred while calculating outstanding balance" });
            }
        }

        /// <summary>
        /// Validate loan parameters
        /// </summary>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status200OK)]
        public ActionResult<ValidationResult> ValidateLoanParameters([FromBody] LoanValidationRequest request)
        {
            try
            {
                var result = _calculatorService.ValidateLoanParameters(
                    request.Principal,
                    request.AnnualInterestRate,
                    request.TenureMonths);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating loan parameters");
                return StatusCode(500, new { message = "An error occurred during validation" });
            }
        }
    }

    #region Request/Response Models

    public class EMICalculationRequest
    {
        public decimal Principal { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public int TenureMonths { get; set; }
    }

    public class EMICalculationResponse
    {
        public decimal Principal { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public int TenureMonths { get; set; }
        public decimal MonthlyEMI { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalPayment { get; set; }
    }

    public class PenaltyCalculationRequest
    {
        public decimal OverdueAmount { get; set; }
        public int DaysOverdue { get; set; }
        public decimal PenaltyRate { get; set; }
    }

    public class PenaltyCalculationResponse
    {
        public decimal OverdueAmount { get; set; }
        public int DaysOverdue { get; set; }
        public decimal PenaltyRate { get; set; }
        public decimal PenaltyAmount { get; set; }
        public decimal TotalAmountDue { get; set; }
    }

    public class OutstandingBalanceRequest
    {
        public decimal Principal { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public int TenureMonths { get; set; }
        public int PaymentsMade { get; set; }
    }

    public class OutstandingBalanceResponse
    {
        public decimal Principal { get; set; }
        public int PaymentsMade { get; set; }
        public int RemainingPayments { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal PrincipalPaid { get; set; }
    }

    public class LoanValidationRequest
    {
        public decimal Principal { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public int TenureMonths { get; set; }
    }

    #endregion
}
