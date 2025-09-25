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
    public class LoanApplicationService : ILoanApplicationService
    {
        private readonly ILoanApplicationRepository _loanApplicationRepository;
        private readonly ILoanProductRepository _loanProductRepository;
        private readonly ILogger<LoanApplicationService> _logger;
        private readonly IMapper _mapper;

        public LoanApplicationService(
            ILoanApplicationRepository loanApplicationRepository,
            ILoanProductRepository loanProductRepository,
            ILogger<LoanApplicationService> logger,
            IMapper mapper)
        {
            _loanApplicationRepository = loanApplicationRepository ?? throw new ArgumentNullException(nameof(loanApplicationRepository));
            _loanProductRepository = loanProductRepository ?? throw new ArgumentNullException(nameof(loanProductRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<LoanApplication>> GetAllLoanApplicationsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all loan applications");
                return await _loanApplicationRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all loan applications");
                throw;
            }
        }

        public async Task<LoanApplication> GetLoanApplicationByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("Getting loan application with ID: {Id}", id);
                var loanApplication = await _loanApplicationRepository.GetByIdAsync(id);
                
                if (loanApplication == null)
                {
                    _logger.LogWarning("Loan application with ID: {Id} not found", id);
                }
                
                return loanApplication;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan application with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<LoanApplication>> GetLoanApplicationsByCustomerIdAsync(string customerId)
        {
            try
            {
                _logger.LogInformation("Getting loan applications for customer ID: {CustomerId}", customerId);
                return await _loanApplicationRepository.GetByCustomerIdAsync(customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan applications for customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<LoanApplication> CreateLoanApplicationAsync(LoanApplication loanApplication)
        {
            try
            {
                _logger.LogInformation("Creating new loan application for customer: {CustomerId}", loanApplication.CustomerId);
                
                // Validate loan product exists
                var loanProduct = await _loanProductRepository.GetByIdAsync(loanApplication.LoanProductId);
                if (loanProduct == null)
                {
                    throw new ArgumentException($"Loan product with ID: {loanApplication.LoanProductId} not found");
                }
                
                // Validate loan amount is within product limits
                if (loanApplication.RequestedAmount < loanProduct.MinAmount || loanApplication.RequestedAmount > loanProduct.MaxAmount)
                {
                    throw new ArgumentException($"Requested loan amount must be between {loanProduct.MinAmount} and {loanProduct.MaxAmount}");
                }
                
                // Validate loan term is within product limits
                if (loanApplication.RequestedTerm < loanProduct.MinTerm || loanApplication.RequestedTerm > loanProduct.MaxTerm)
                {
                    throw new ArgumentException($"Requested loan term must be between {loanProduct.MinTerm} and {loanProduct.MaxTerm} months");
                }
                
                // Set default values
                loanApplication.ApplicationDate = DateTime.UtcNow;
                loanApplication.Status = LoanApplicationStatus.Draft;
                loanApplication.InterestRate = loanProduct.InterestRate;
                loanApplication.ApplicationNumber = GenerateApplicationNumber();
                
                // Create loan application
                var createdApplication = await _loanApplicationRepository.AddAsync(loanApplication);
                _logger.LogInformation("Loan application created with ID: {Id}", createdApplication.Id);
                
                return createdApplication;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating loan application for customer: {CustomerId}", loanApplication.CustomerId);
                throw;
            }
        }

        public async Task<LoanApplication> UpdateLoanApplicationAsync(LoanApplication loanApplication)
        {
            try
            {
                _logger.LogInformation("Updating loan application with ID: {Id}", loanApplication.Id);
                
                // Get existing application
                var existingApplication = await _loanApplicationRepository.GetByIdAsync(loanApplication.Id);
                if (existingApplication == null)
                {
                    throw new ArgumentException($"Loan application with ID: {loanApplication.Id} not found");
                }
                
                // Validate application status for updates
                if (existingApplication.Status != LoanApplicationStatus.Draft)
                {
                    throw new InvalidOperationException("Only draft applications can be updated");
                }
                
                // Update loan application
                var updatedApplication = await _loanApplicationRepository.UpdateAsync(loanApplication);
                _logger.LogInformation("Loan application updated with ID: {Id}", updatedApplication.Id);
                
                return updatedApplication;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loan application with ID: {Id}", loanApplication.Id);
                throw;
            }
        }

        public async Task<LoanApplication> SubmitLoanApplicationAsync(string id)
        {
            try
            {
                _logger.LogInformation("Submitting loan application with ID: {Id}", id);
                
                // Get existing application
                var loanApplication = await _loanApplicationRepository.GetByIdAsync(id);
                if (loanApplication == null)
                {
                    throw new ArgumentException($"Loan application with ID: {id} not found");
                }
                
                // Submit application
                loanApplication.Submit();
                
                // Update loan application
                var updatedApplication = await _loanApplicationRepository.UpdateAsync(loanApplication);
                _logger.LogInformation("Loan application submitted with ID: {Id}", updatedApplication.Id);
                
                return updatedApplication;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting loan application with ID: {Id}", id);
                throw;
            }
        }

        public async Task<LoanApplication> ApproveLoanApplicationAsync(string id, string approvedBy, decimal? approvedAmount = null, int? approvedTerm = null)
        {
            try
            {
                _logger.LogInformation("Approving loan application with ID: {Id}", id);
                
                // Get existing application
                var loanApplication = await _loanApplicationRepository.GetByIdAsync(id);
                if (loanApplication == null)
                {
                    throw new ArgumentException($"Loan application with ID: {id} not found");
                }
                
                // Update status to in review first if it's in submitted status
                if (loanApplication.Status == LoanApplicationStatus.Submitted)
                {
                    loanApplication.Status = LoanApplicationStatus.InReview;
                    await _loanApplicationRepository.UpdateAsync(loanApplication);
                }
                
                // Approve application
                loanApplication.Approve(approvedBy, approvedAmount, approvedTerm);
                
                // Update loan application
                var updatedApplication = await _loanApplicationRepository.UpdateAsync(loanApplication);
                _logger.LogInformation("Loan application approved with ID: {Id}", updatedApplication.Id);
                
                return updatedApplication;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving loan application with ID: {Id}", id);
                throw;
            }
        }

        public async Task<LoanApplication> RejectLoanApplicationAsync(string id, string rejectionReason)
        {
            try
            {
                _logger.LogInformation("Rejecting loan application with ID: {Id}", id);
                
                // Get existing application
                var loanApplication = await _loanApplicationRepository.GetByIdAsync(id);
                if (loanApplication == null)
                {
                    throw new ArgumentException($"Loan application with ID: {id} not found");
                }
                
                // Reject application
                loanApplication.Reject(rejectionReason);
                
                // Update loan application
                var updatedApplication = await _loanApplicationRepository.UpdateAsync(loanApplication);
                _logger.LogInformation("Loan application rejected with ID: {Id}", updatedApplication.Id);
                
                return updatedApplication;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting loan application with ID: {Id}", id);
                throw;
            }
        }

        public async Task<Loan> CreateLoanFromApplicationAsync(string applicationId)
        {
            try
            {
                _logger.LogInformation("Creating loan from application with ID: {Id}", applicationId);
                
                // Get existing application
                var loanApplication = await _loanApplicationRepository.GetByIdAsync(applicationId);
                if (loanApplication == null)
                {
                    throw new ArgumentException($"Loan application with ID: {applicationId} not found");
                }
                
                // Create loan from application
                var loan = loanApplication.CreateLoan();
                
                // Update loan application with loan reference
                await _loanApplicationRepository.UpdateAsync(loanApplication);
                
                _logger.LogInformation("Loan created from application with ID: {ApplicationId}, Loan ID: {LoanId}", 
                    applicationId, loan.Id);
                
                return loan;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating loan from application with ID: {Id}", applicationId);
                throw;
            }
        }

        public async Task<bool> DeleteLoanApplicationAsync(string id)
        {
            try
            {
                _logger.LogInformation("Deleting loan application with ID: {Id}", id);
                
                // Get existing application
                var loanApplication = await _loanApplicationRepository.GetByIdAsync(id);
                if (loanApplication == null)
                {
                    throw new ArgumentException($"Loan application with ID: {id} not found");
                }
                
                // Validate application status for deletion
                if (loanApplication.Status != LoanApplicationStatus.Draft)
                {
                    throw new InvalidOperationException("Only draft applications can be deleted");
                }
                
                // Delete loan application
                var result = await _loanApplicationRepository.DeleteAsync(id);
                _logger.LogInformation("Loan application deleted with ID: {Id}, Result: {Result}", id, result);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting loan application with ID: {Id}", id);
                throw;
            }
        }

        private string GenerateApplicationNumber()
        {
            // Generate a unique application number format: LA-{year}{month}{day}-{random 6 digits}
            return $"LA-{DateTime.Now:yyyyMMdd}-{new Random().Next(100000, 999999)}";
        }
    }
}
