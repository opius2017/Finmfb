using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using LoanCollateralRepo = FinTech.Core.Application.Interfaces.Repositories.Loans.ILoanCollateralRepository;
using FinTech.Core.Application.Interfaces.Loans;
using FinTech.Core.Application.Interfaces.Repositories.Loans;
using FinTech.Core.Domain.Entities.Loans;
using Microsoft.Extensions.Logging;

namespace FinTech.Core.Application.Services.Loans
{
    public class LoanCollateralService : ILoanCollateralService
    {
        private readonly LoanCollateralRepo _collateralRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly ILogger<LoanCollateralService> _logger;

        public LoanCollateralService(
            LoanCollateralRepo collateralRepository,
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
                    CollateralType = collateralDto.CollateralType,
                    Description = collateralDto.Description,
                    // FinTech Best Practice: Value is nullable decimal, use null-coalescing
                    EstimatedValue = collateralDto.Value ?? 0m,
                    ValuationDate = collateralDto.ValuationDate,
                    ValuedBy = collateralDto.ValuedBy,
                    Location = collateralDto.Location,
                    Notes = collateralDto.Notes ?? string.Empty,
                    // Set other properties as needed, e.g. TenantId, Status, etc.
                };

                var result = await _collateralRepository.AddAsync(collateral);
                // Map domain entity to DTO
                return new LoanCollateralDto
                {
                    Id = collateral.Id.ToString(),
                    CollateralType = result.CollateralType,
                    Description = result.Description,
                    Value = result.EstimatedValue,
                    ValuationDate = result.ValuationDate ?? DateTime.MinValue,
                    ValuedBy = result.ValuedBy,
                    Location = result.Location,
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
                existingCollateral.EstimatedValue = collateralDto.Value;
                existingCollateral.ValuationDate = collateralDto.ValuationDate;
                existingCollateral.ValuedBy = collateralDto.ValuedBy;
                existingCollateral.Location = collateralDto.Location;
                existingCollateral.Notes = collateralDto.Notes;
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

                collateral.Status = (FinTech.Core.Domain.Features.Loans.Enums.CollateralStatus)FinTech.Core.Domain.Enums.CollateralStatus.Active;
                collateral.Notes = $"Approved by {approverId}: {comments}";
                collateral.UpdatedAt = DateTime.UtcNow;

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

                collateral.Status = (FinTech.Core.Domain.Features.Loans.Enums.CollateralStatus)FinTech.Core.Domain.Enums.CollateralStatus.Impaired;
                collateral.Notes = $"Rejected by {rejecterId}: {reason}";
                collateral.UpdatedAt = DateTime.UtcNow;

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
                return collaterals.Sum(c => c.EstimatedValue);
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
                Id = collateral.Id.ToString(),
                CollateralType = collateral.CollateralType,
                Description = collateral.Description,
                Value = collateral.EstimatedValue,
                ValuationDate = collateral.ValuationDate ?? DateTime.MinValue,
                ValuedBy = collateral.ValuedBy ?? string.Empty,
                Location = collateral.Location ?? string.Empty,
                Notes = collateral.Notes ?? string.Empty
            };
        }
    }
}
