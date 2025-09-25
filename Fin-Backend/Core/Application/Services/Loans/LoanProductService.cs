using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FinTech.Core.Application.Interfaces.Loans;
using FinTech.Core.Application.Interfaces.Repositories.Loans;
using FinTech.Core.Domain.Entities.Loans;
using Microsoft.Extensions.Logging;

namespace FinTech.Core.Application.Services.Loans
{
    public class LoanProductService : ILoanProductService
    {
        private readonly ILoanProductRepository _loanProductRepository;
        private readonly ILogger<LoanProductService> _logger;
        private readonly IMapper _mapper;

        public LoanProductService(
            ILoanProductRepository loanProductRepository,
            ILogger<LoanProductService> logger,
            IMapper mapper)
        {
            _loanProductRepository = loanProductRepository ?? throw new ArgumentNullException(nameof(loanProductRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<LoanProduct>> GetAllLoanProductsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all loan products");
                return await _loanProductRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all loan products");
                throw;
            }
        }

        public async Task<LoanProduct> GetLoanProductByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("Getting loan product with ID: {Id}", id);
                var loanProduct = await _loanProductRepository.GetByIdAsync(id);
                
                if (loanProduct == null)
                {
                    _logger.LogWarning("Loan product with ID: {Id} not found", id);
                }
                
                return loanProduct;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan product with ID: {Id}", id);
                throw;
            }
        }

        public async Task<LoanProduct> CreateLoanProductAsync(LoanProduct loanProduct)
        {
            try
            {
                _logger.LogInformation("Creating new loan product: {Name}", loanProduct.Name);
                
                // Validate loan product
                ValidateLoanProduct(loanProduct);
                
                // Set default values
                loanProduct.IsActive = true;
                
                // Create loan product
                var createdProduct = await _loanProductRepository.AddAsync(loanProduct);
                _logger.LogInformation("Loan product created with ID: {Id}", createdProduct.Id);
                
                return createdProduct;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating loan product: {Name}", loanProduct.Name);
                throw;
            }
        }

        public async Task<LoanProduct> UpdateLoanProductAsync(LoanProduct loanProduct)
        {
            try
            {
                _logger.LogInformation("Updating loan product with ID: {Id}", loanProduct.Id);
                
                // Get existing product
                var existingProduct = await _loanProductRepository.GetByIdAsync(loanProduct.Id);
                if (existingProduct == null)
                {
                    throw new ArgumentException($"Loan product with ID: {loanProduct.Id} not found");
                }
                
                // Validate loan product
                ValidateLoanProduct(loanProduct);
                
                // Update loan product
                var updatedProduct = await _loanProductRepository.UpdateAsync(loanProduct);
                _logger.LogInformation("Loan product updated with ID: {Id}", updatedProduct.Id);
                
                return updatedProduct;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loan product with ID: {Id}", loanProduct.Id);
                throw;
            }
        }

        public async Task<bool> ActivateLoanProductAsync(string id)
        {
            try
            {
                _logger.LogInformation("Activating loan product with ID: {Id}", id);
                
                // Get existing product
                var loanProduct = await _loanProductRepository.GetByIdAsync(id);
                if (loanProduct == null)
                {
                    throw new ArgumentException($"Loan product with ID: {id} not found");
                }
                
                // Activate product
                loanProduct.IsActive = true;
                
                // Update loan product
                await _loanProductRepository.UpdateAsync(loanProduct);
                _logger.LogInformation("Loan product activated with ID: {Id}", id);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating loan product with ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeactivateLoanProductAsync(string id)
        {
            try
            {
                _logger.LogInformation("Deactivating loan product with ID: {Id}", id);
                
                // Get existing product
                var loanProduct = await _loanProductRepository.GetByIdAsync(id);
                if (loanProduct == null)
                {
                    throw new ArgumentException($"Loan product with ID: {id} not found");
                }
                
                // Deactivate product
                loanProduct.IsActive = false;
                
                // Update loan product
                await _loanProductRepository.UpdateAsync(loanProduct);
                _logger.LogInformation("Loan product deactivated with ID: {Id}", id);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating loan product with ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteLoanProductAsync(string id)
        {
            try
            {
                _logger.LogInformation("Deleting loan product with ID: {Id}", id);
                
                // Get existing product
                var loanProduct = await _loanProductRepository.GetByIdAsync(id);
                if (loanProduct == null)
                {
                    throw new ArgumentException($"Loan product with ID: {id} not found");
                }
                
                // Delete loan product
                var result = await _loanProductRepository.DeleteAsync(id);
                _logger.LogInformation("Loan product deleted with ID: {Id}, Result: {Result}", id, result);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting loan product with ID: {Id}", id);
                throw;
            }
        }

        private void ValidateLoanProduct(LoanProduct loanProduct)
        {
            // Basic validation for loan product properties
            if (string.IsNullOrWhiteSpace(loanProduct.Name))
            {
                throw new ArgumentException("Loan product name is required");
            }
            
            if (string.IsNullOrWhiteSpace(loanProduct.Code))
            {
                throw new ArgumentException("Loan product code is required");
            }
            
            if (loanProduct.MinAmount <= 0)
            {
                throw new ArgumentException("Minimum loan amount must be greater than zero");
            }
            
            if (loanProduct.MaxAmount <= 0)
            {
                throw new ArgumentException("Maximum loan amount must be greater than zero");
            }
            
            if (loanProduct.MaxAmount < loanProduct.MinAmount)
            {
                throw new ArgumentException("Maximum loan amount must be greater than or equal to minimum loan amount");
            }
            
            if (loanProduct.MinTerm <= 0)
            {
                throw new ArgumentException("Minimum loan term must be greater than zero");
            }
            
            if (loanProduct.MaxTerm <= 0)
            {
                throw new ArgumentException("Maximum loan term must be greater than zero");
            }
            
            if (loanProduct.MaxTerm < loanProduct.MinTerm)
            {
                throw new ArgumentException("Maximum loan term must be greater than or equal to minimum loan term");
            }
            
            if (loanProduct.InterestRate < 0)
            {
                throw new ArgumentException("Interest rate cannot be negative");
            }
        }
    }
}
