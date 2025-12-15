using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using FinTech.Core.Application.Interfaces.Loans;

namespace FinTech.Core.Application.Services.Loans
{
    public class AssetLienService : IAssetLienService
    {
        private readonly IRepository<AssetLien> _lienRepository;
        private readonly IRepository<Loan> _loanRepository;
        private readonly IRepository<Member> _memberRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AssetLienService> _logger;

        public AssetLienService(
            IRepository<AssetLien> lienRepository,
            IRepository<Loan> loanRepository,
            IRepository<Member> memberRepository,
            IUnitOfWork unitOfWork,
            ILogger<AssetLienService> logger)
        {
            _lienRepository = lienRepository;
            _loanRepository = loanRepository;
            _memberRepository = memberRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<AssetLienDto> CreateAssetLienAsync(CreateAssetLienRequest request)
        {
            try
            {
                _logger.LogInformation("Creating asset lien for loan {LoanId}", request.LoanId);

                var loan = await _loanRepository.GetByIdAsync(request.LoanId);
                if (loan == null)
                    throw new InvalidOperationException("Loan not found");

                if (loan.LoanType != "COMMODITY")
                    throw new InvalidOperationException("Asset liens can only be created for commodity loans");

                var member = await _memberRepository.GetByIdAsync(loan.MemberId);
                if (member == null)
                    throw new InvalidOperationException("Member not found");

                // Generate lien number
                var lienNumber = await GenerateLienNumberAsync();

                var lien = new AssetLien
                {
                    LienNumber = lienNumber,
                    LoanId = request.LoanId,
                    MemberId = member.Id,
                    AssetDescription = request.AssetDescription,
                    AssetSerialNumber = request.AssetSerialNumber,
                    AssetModel = request.AssetModel,
                    AssetValue = request.AssetValue,
                    PurchaseDate = DateTime.UtcNow,
                    Status = "ACTIVE",
                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                };

                await _lienRepository.AddAsync(lien);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Asset lien {LienNumber} created for loan {LoanNumber}",
                    lienNumber, loan.LoanNumber);

                return MapToDto(lien);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating asset lien for loan {LoanId}", request.LoanId);
                throw;
            }
        }

        public async Task<AssetLienDto> ReleaseAssetLienAsync(ReleaseAssetLienRequest request)
        {
            try
            {
                _logger.LogInformation("Releasing asset lien {LienId}", request.LienId);

                var lien = await _lienRepository.GetByIdAsync(request.LienId);
                if (lien == null)
                    throw new InvalidOperationException("Asset lien not found");

                if (lien.Status != "ACTIVE")
                    throw new InvalidOperationException($"Cannot release lien with status {lien.Status}");

                // Verify loan is fully repaid
                var loan = await _loanRepository.GetByIdAsync(lien.LoanId);
                if (loan != null && loan.OutstandingBalance > 0)
                {
                    throw new InvalidOperationException("Cannot release lien while loan has outstanding balance");
                }

                lien.Status = "RELEASED";
                lien.ReleasedDate = DateTime.UtcNow;
                lien.ReleasedBy = request.ReleasedBy;
                lien.ReleaseNotes = request.ReleaseNotes;
                lien.UpdatedAt = DateTime.UtcNow;
                lien.LastModifiedBy = request.ReleasedBy;

                await _lienRepository.UpdateAsync(lien);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Asset lien {LienNumber} released by {ReleasedBy}",
                    lien.LienNumber, request.ReleasedBy);

                return MapToDto(lien);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing asset lien {LienId}", request.LienId);
                throw;
            }
        }

        public async Task<AssetLienDto?> GetAssetLienByIdAsync(string lienId)
        {
            var lien = await _lienRepository.GetByIdAsync(lienId);
            return lien != null ? MapToDto(lien) : null;
        }

        public async Task<List<AssetLienDto>> GetLoanAssetLiensAsync(string loanId)
        {
            var liens = await _lienRepository.GetAll()
                        .Where(l => l.LoanId == loanId)
                        .ToListAsync();

            return liens.OrderByDescending(l => l.CreatedAt)
                       .Select(l => MapToDto(l))
                       .ToList();
        }

        public async Task<List<AssetLienDto>> GetMemberAssetLiensAsync(string memberId, string? status = null)
        {
            var query = _lienRepository.GetAll().Where(l => l.MemberId == memberId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(l => l.Status == status);

            var result = await query.ToListAsync();

            return result.OrderByDescending(l => l.CreatedAt)
                       .Select(l => MapToDto(l))
                       .ToList();
        }

        public async Task<List<AssetLienDto>> GetActiveAssetLiensAsync()
        {
            var liens = await _lienRepository.GetAll()
                        .Where(l => l.Status == "ACTIVE")
                        .ToListAsync();

            return liens.OrderByDescending(l => l.CreatedAt)
                       .Select(l => MapToDto(l))
                       .ToList();
        }

        public async Task<bool> HasActiveLiensAsync(string loanId)
        {
            var liens = await _lienRepository.GetAll()
                .Where(l => l.LoanId == loanId && l.Status == "ACTIVE")
                .ToListAsync();
            
            return liens.Any();
        }

        public async Task<decimal> GetMemberTotalLienValueAsync(string memberId)
        {
            var activeLiens = await _lienRepository.GetAll()
                .Where(l => l.MemberId == memberId && l.Status == "ACTIVE")
                .ToListAsync();
            
            return activeLiens.Sum(l => l.AssetValue);
        }

        #region Helper Methods

        private async Task<string> GenerateLienNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var allLiens = await _lienRepository.GetAll().ToListAsync();
            var count = allLiens.Count(l => l.LienNumber.StartsWith($"LIEN/{year}")) + 1;
            return $"LIEN/{year}/{count:D6}";
        }

        private AssetLienDto MapToDto(AssetLien lien)
        {
            return new AssetLienDto
            {
                Id = lien.Id,
                LienNumber = lien.LienNumber,
                LoanId = lien.LoanId,
                MemberId = lien.MemberId,
                AssetDescription = lien.AssetDescription,
                AssetSerialNumber = lien.AssetSerialNumber,
                AssetModel = lien.AssetModel,
                AssetValue = lien.AssetValue,
                PurchaseDate = lien.PurchaseDate,
                Status = lien.Status,
                ReleasedDate = lien.ReleasedDate
            };
        }

        #endregion
    }
}
