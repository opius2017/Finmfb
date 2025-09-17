using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FinTech.Application.DTOs.Loans;
using FinTech.Application.Interfaces.Loans;
using FinTech.Domain.Entities.Loans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinTech.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LoanProductsController : ControllerBase
    {
        private readonly ILoanProductService _loanProductService;
        private readonly IMapper _mapper;
        private readonly ILogger<LoanProductsController> _logger;

        public LoanProductsController(
            ILoanProductService loanProductService,
            IMapper mapper,
            ILogger<LoanProductsController> logger)
        {
            _loanProductService = loanProductService ?? throw new ArgumentNullException(nameof(loanProductService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all loan products
        /// </summary>
        /// <returns>List of loan products</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LoanProductDto>>> GetLoanProducts()
        {
            var loanProducts = await _loanProductService.GetAllLoanProductsAsync();
            return Ok(_mapper.Map<IEnumerable<LoanProductDto>>(loanProducts));
        }

        /// <summary>
        /// Gets a loan product by ID
        /// </summary>
        /// <param name="id">Loan product ID</param>
        /// <returns>Loan product details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanProductDto>> GetLoanProduct(string id)
        {
            var loanProduct = await _loanProductService.GetLoanProductByIdAsync(id);
            if (loanProduct == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<LoanProductDto>(loanProduct));
        }

        /// <summary>
        /// Creates a new loan product
        /// </summary>
        /// <param name="createLoanProductDto">Loan product data</param>
        /// <returns>Created loan product</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoanProductDto>> CreateLoanProduct(CreateLoanProductDto createLoanProductDto)
        {
            try
            {
                var loanProduct = _mapper.Map<LoanProduct>(createLoanProductDto);
                var createdProduct = await _loanProductService.CreateLoanProductAsync(loanProduct);
                var result = _mapper.Map<LoanProductDto>(createdProduct);

                return CreatedAtAction(nameof(GetLoanProduct), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates a loan product
        /// </summary>
        /// <param name="id">Loan product ID</param>
        /// <param name="loanProductDto">Updated loan product data</param>
        /// <returns>No content</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateLoanProduct(string id, LoanProductDto loanProductDto)
        {
            if (id != loanProductDto.Id)
            {
                return BadRequest("ID in URL must match ID in request body");
            }

            try
            {
                var loanProduct = _mapper.Map<LoanProduct>(loanProductDto);
                await _loanProductService.UpdateLoanProductAsync(loanProduct);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(ex.Message);
                }
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Activates a loan product
        /// </summary>
        /// <param name="id">Loan product ID</param>
        /// <returns>No content</returns>
        [HttpPost("{id}/activate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivateLoanProduct(string id)
        {
            try
            {
                var result = await _loanProductService.ActivateLoanProductAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Deactivates a loan product
        /// </summary>
        /// <param name="id">Loan product ID</param>
        /// <returns>No content</returns>
        [HttpPost("{id}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateLoanProduct(string id)
        {
            try
            {
                var result = await _loanProductService.DeactivateLoanProductAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a loan product
        /// </summary>
        /// <param name="id">Loan product ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteLoanProduct(string id)
        {
            try
            {
                var result = await _loanProductService.DeleteLoanProductAsync(id);
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
        }
    }
}