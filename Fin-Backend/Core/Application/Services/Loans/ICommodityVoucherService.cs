using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;

namespace FinTech.Core.Application.Services.Loans
{
    /// <summary>
    /// Service for managing commodity loan vouchers and redemptions
    /// </summary>
    public interface ICommodityVoucherService
    {
        /// <summary>
        /// Generate a new commodity voucher for a loan
        /// </summary>
        Task<CommodityVoucherDto> GenerateVoucherAsync(GenerateVoucherRequest request);

        /// <summary>
        /// Validate a voucher before redemption
        /// </summary>
        Task<VoucherValidationResult> ValidateVoucherAsync(ValidateVoucherRequest request);

        /// <summary>
        /// Redeem a voucher for commodity purchase
        /// </summary>
        Task<RedemptionResult> RedeemVoucherAsync(RedeemVoucherRequest request);

        /// <summary>
        /// Get voucher by ID
        /// </summary>
        Task<CommodityVoucherDto?> GetVoucherByIdAsync(string voucherId);

        /// <summary>
        /// Get voucher by voucher number
        /// </summary>
        Task<CommodityVoucherDto?> GetVoucherByNumberAsync(string voucherNumber);

        /// <summary>
        /// Get all vouchers for a member
        /// </summary>
        Task<List<CommodityVoucherDto>> GetMemberVouchersAsync(string memberId, string? status = null);

        /// <summary>
        /// Get all vouchers for a loan
        /// </summary>
        Task<List<CommodityVoucherDto>> GetLoanVouchersAsync(string loanId);

        /// <summary>
        /// Get redemption history for a voucher
        /// </summary>
        Task<List<CommodityRedemptionDto>> GetVoucherRedemptionsAsync(string voucherId);

        /// <summary>
        /// Cancel a voucher
        /// </summary>
        Task<bool> CancelVoucherAsync(string voucherId, string cancelledBy, string reason);

        /// <summary>
        /// Check and expire old vouchers
        /// </summary>
        Task<int> ExpireOldVouchersAsync();

        /// <summary>
        /// Get voucher balance
        /// </summary>
        Task<decimal> GetVoucherBalanceAsync(string voucherId);
    }
}
