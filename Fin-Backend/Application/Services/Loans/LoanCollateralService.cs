using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FinTech.Application.DTOs.Loans;
using FinTech.Application.Interfaces.Repositories.Loans;
using FinTech.Application.Interfaces.Services.Loans;
using FinTech.Domain.Entities.Loans;
using Microsoft.Extensions.Logging;

namespace FinTech.Application.Services.Loans
{
    public class LoanCollateralService : ILoanCollateralService
    {
        private readonly ILoanCollateralRepository _collateralRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly ILogger<LoanCollateralService> _logger;

        public LoanCollateralService(
            ILoanCollateralRepository collateralRepository,
            ILoanRepository loanRepository,
            ILogger<LoanCollateralService> logger)
        {
            _collateralRepository = collateralRepository ?? throw new ArgumentNullException(nameof(collateralRepository));
            _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<LoanCollateralDto>> GetAllCollateralsAsync()
        {
            try
            {
                var collaterals = await _collateralRepository.GetAllAsync();
                return collaterals.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all loan collaterals");
                throw;
            }
        }

        public async Task<LoanCollateralDto> GetCollateralByIdAsync(string id)
        {
            try
            {
                var collateral = await _collateralRepository.GetByIdAsync(id);
                return collateral != null ? MapToDto(collateral) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan collateral with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<LoanCollateralDto>> GetCollateralsByLoanIdAsync(string loanId)
        {
            try
            {
                var loan = await _loanRepository.GetByIdAsync(loanId);
                if (loan == null)
                {
                    throw new ApplicationException($"Loan with ID {loanId} not found");
                }

                var collaterals = await _collateralRepository.GetByLoanIdAsync(loanId);
                return collaterals.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collaterals for loan ID: {LoanId}", loanId);
                throw;
            }
        }

    public async Task<LoanCollateralDto> AddCollateralAsync(string loanId, CreateLoanCollateralDto collateralDto)
        {
            try
            {
                var loan = await _loanRepository.GetByIdAsync(loanId);
                if (loan == null)
                {
                    throw new ApplicationException($"Loan with ID {loanId} not found");
                }

                var collateral = new LoanCollateral
                {
                    LoanId = loanId,
                    CollateralType = collateralDto.CollateralType,
                    Description = collateralDto.Description,
                    Value = collateralDto.Value,
                    ValuationDate = collateralDto.ValuationDate,
                    ValuationMethod = collateralDto.ValuationMethod,
                    ValuedBy = collateralDto.ValuedBy,
                    OwnerName = collateralDto.OwnerName,
                    OwnerRelationship = collateralDto.OwnerRelationship,
                    RegistrationNumber = collateralDto.RegistrationNumber,
                    Location = collateralDto.Location,
                    IsInsured = collateralDto.IsInsured,
                    InsuranceCompany = collateralDto.InsuranceCompany,
                    InsurancePolicyNumber = collateralDto.InsurancePolicyNumber,
                    InsuranceExpiryDate = collateralDto.InsuranceExpiryDate,
                    VerificationStatus = collateralDto.VerificationStatus,
                    VerificationDate = collateralDto.VerificationDate,
                    VerifiedBy = collateralDto.VerifiedBy,
                    Notes = collateralDto.Notes
                };

                var result = await _collateralRepository.AddAsync(collateral);
                // Map domain entity to DTO
                return new LoanCollateralDto
                {
                    Id = result.Id.ToString(),
                    LoanId = result.LoanId,
                    CollateralType = result.CollateralType,
                    Description = result.Description,
                    Value = result.Value,
                    ValuationDate = result.ValuationDate,
                    ValuationMethod = result.ValuationMethod,
                    ValuedBy = result.ValuedBy,
                    OwnerName = result.OwnerName,
                    OwnerRelationship = result.OwnerRelationship,
                    RegistrationNumber = result.RegistrationNumber,
                    Location = result.Location,
                    IsInsured = result.IsInsured,
                    InsuranceCompany = result.InsuranceCompany,
                    InsurancePolicyNumber = result.InsurancePolicyNumber,
                    InsuranceExpiryDate = result.InsuranceExpiryDate,
                    VerificationStatus = result.VerificationStatus,
                    VerificationDate = result.VerificationDate,
                    VerifiedBy = result.VerifiedBy,
                    Notes = result.Notes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding loan collateral");
                throw;
            }
        }

        public async Task<LoanCollateralDto> UpdateCollateralAsync(LoanCollateralDto collateralDto)
        {
            try
            {
                var existingCollateral = await _collateralRepository.GetByIdAsync(collateralDto.Id);
                if (existingCollateral == null)
                {
                    throw new ApplicationException($"Collateral with ID {collateralDto.Id} not found");
                }

                existingCollateral.CollateralType = collateralDto.CollateralType;
                existingCollateral.Description = collateralDto.Description;
                existingCollateral.Value = collateralDto.Value;
                existingCollateral.OwnerName = collateralDto.OwnerName;
                existingCollateral.RegistrationNumber = collateralDto.RegistrationNumber;
                existingCollateral.RegistrationDate = collateralDto.RegistrationDate;
                existingCollateral.Location = collateralDto.Location;
                existingCollateral.Status = collateralDto.Status;
                existingCollateral.LastModifiedDate = DateTime.UtcNow;

                var result = await _collateralRepository.UpdateAsync(existingCollateral);
                return MapToDto(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loan collateral with ID: {Id}", collateralDto.Id);
                throw;
            }
        }

        public async Task<bool> DeleteCollateralAsync(string id)
        {
            try
            {
                return await _collateralRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting loan collateral with ID: {Id}", id);
                throw;
            }
        }

        public async Task<LoanCollateralDto> ApproveCollateralAsync(string id, string approverId, string comments)
        {
            try
            {
                var collateral = await _collateralRepository.GetByIdAsync(id);
                if (collateral == null)
                {
                    throw new ApplicationException($"Collateral with ID {id} not found");
                }

                collateral.Status = CollateralStatus.Approved;
                collateral.ApprovedBy = approverId;
                collateral.ApprovalDate = DateTime.UtcNow;
                collateral.ApprovalComments = comments;
                collateral.LastModifiedDate = DateTime.UtcNow;

                var result = await _collateralRepository.UpdateAsync(collateral);
                return MapToDto(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving loan collateral with ID: {Id}", id);
                throw;
            }
        }

        public async Task<LoanCollateralDto> RejectCollateralAsync(string id, string rejecterId, string reason)
        {
            try
            {
                var collateral = await _collateralRepository.GetByIdAsync(id);
                if (collateral == null)
                {
                    throw new ApplicationException($"Collateral with ID {id} not found");
                }

                collateral.Status = CollateralStatus.Rejected;
                collateral.RejectedBy = rejecterId;
                collateral.RejectionDate = DateTime.UtcNow;
                collateral.RejectionReason = reason;
                collateral.LastModifiedDate = DateTime.UtcNow;

                var result = await _collateralRepository.UpdateAsync(collateral);
                return MapToDto(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting loan collateral with ID: {Id}", id);
                throw;
            }
        }

        public async Task<decimal> GetTotalCollateralValueForLoanAsync(string loanId)
        {
            try
            {
                var collaterals = await _collateralRepository.GetByLoanIdAsync(loanId);
                return collaterals.Sum(c => c.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total collateral value for loan ID: {LoanId}", loanId);
                throw;
            }
        }

        private LoanCollateralDto MapToDto(LoanCollateral collateral)
        {
            return new LoanCollateralDto
            {
                Id = collateral.Id,
                LoanId = collateral.LoanId,
                CollateralType = collateral.CollateralType,
                Description = collateral.Description,
                Value = collateral.Value,
                OwnerName = collateral.OwnerName,
                RegistrationNumber = collateral.RegistrationNumber,
                RegistrationDate = collateral.RegistrationDate,
                Location = collateral.Location,
                Status = collateral.Status,
                ApprovedBy = collateral.ApprovedBy,
                ApprovalDate = collateral.ApprovalDate,
                ApprovalComments = collateral.ApprovalComments,
                RejectedBy = collateral.RejectedBy,
                RejectionDate = collateral.RejectionDate,
                RejectionReason = collateral.RejectionReason,
                CreatedDate = collateral.CreatedDate,
                LastModifiedDate = collateral.LastModifiedDate
            };
        }
    }
}