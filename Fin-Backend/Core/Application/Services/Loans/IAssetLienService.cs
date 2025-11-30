using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;

namespace FinTech.Core.Application.Services.Loans
{
    /// <summary>
    /// Service for managing asset liens on commodity loans
    /// </summary>
    public interface IAssetLienService
    {
        /// <summary>
        /// Create a new asset lien
        /// </summary>
        Task<AssetLienDto> CreateAssetLienAsync(CreateAssetLienRequest request);

        /// <summary>
        /// Release an asset lien (when loan is fully repaid)
        /// </summary>
        Task<AssetLienDto> ReleaseAssetLienAsync(ReleaseAssetLienRequest request);

        /// <summary>
        /// Get asset lien by ID
        /// </summary>
        Task<AssetLienDto?> GetAssetLienByIdAsync(string lienId);

        /// <summary>
        /// Get all asset liens for a loan
        /// </summary>
        Task<List<AssetLienDto>> GetLoanAssetLiensAsync(string loanId);

        /// <summary>
        /// Get all asset liens for a member
        /// </summary>
        Task<List<AssetLienDto>> GetMemberAssetLiensAsync(string memberId, string? status = null);

        /// <summary>
        /// Get all active asset liens
        /// </summary>
        Task<List<AssetLienDto>> GetActiveAssetLiensAsync();

        /// <summary>
        /// Check if loan has active liens
        /// </summary>
        Task<bool> HasActiveLiensAsync(string loanId);

        /// <summary>
        /// Get total value of assets under lien for a member
        /// </summary>
        Task<decimal> GetMemberTotalLienValueAsync(string memberId);
    }
}
