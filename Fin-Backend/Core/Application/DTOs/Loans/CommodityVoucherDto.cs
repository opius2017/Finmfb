using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class CommodityVoucherDto
    {
        public string Id { get; set; } = string.Empty;
        public string VoucherNumber { get; set; } = string.Empty;
        public string LoanId { get; set; } = string.Empty;
        public string LoanNumber { get; set; } = string.Empty;
        public string MemberId { get; set; } = string.Empty;
        public string MemberNumber { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public decimal VoucherAmount { get; set; }
        public decimal UsedAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime IssuedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? QRCode { get; set; }
        public bool IsActive { get; set; }
        public List<CommodityRedemptionDto> Redemptions { get; set; } = new();
    }

    public class CommodityRedemptionDto
    {
        public string Id { get; set; } = string.Empty;
        public string RedemptionNumber { get; set; } = string.Empty;
        public string VoucherId { get; set; } = string.Empty;
        public decimal RedemptionAmount { get; set; }
        public DateTime RedemptionDate { get; set; }
        public string RedeemedBy { get; set; } = string.Empty;
        public string? StoreLocation { get; set; }
        public string? ItemsDescription { get; set; }
        public string? ReceiptNumber { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class AssetLienDto
    {
        public string Id { get; set; } = string.Empty;
        public string LienNumber { get; set; } = string.Empty;
        public string LoanId { get; set; } = string.Empty;
        public string MemberId { get; set; } = string.Empty;
        public string AssetDescription { get; set; } = string.Empty;
        public string? AssetSerialNumber { get; set; }
        public string? AssetModel { get; set; }
        public decimal AssetValue { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ReleasedDate { get; set; }
    }

    public class GenerateVoucherRequest
    {
        public string LoanId { get; set; } = string.Empty;
        public decimal VoucherAmount { get; set; }
        public int ValidityDays { get; set; } = 90; // Default 90 days
        public string GeneratedBy { get; set; } = string.Empty;
    }

    public class ValidateVoucherRequest
    {
        public string VoucherNumber { get; set; } = string.Empty;
        public string? PINCode { get; set; }
    }

    public class VoucherValidationResult
    {
        public bool IsValid { get; set; }
        public string? VoucherId { get; set; }
        public decimal? AvailableAmount { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? MemberName { get; set; }
        public string? Message { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
    }

    public class RedeemVoucherRequest
    {
        public string VoucherNumber { get; set; } = string.Empty;
        public string? PINCode { get; set; }
        public decimal RedemptionAmount { get; set; }
        public string ItemsDescription { get; set; } = string.Empty;
        public string RedeemedBy { get; set; } = string.Empty;
        public string? StoreLocation { get; set; }
        public string? ReceiptNumber { get; set; }
    }

    public class RedemptionResult
    {
        public bool Success { get; set; }
        public string? RedemptionId { get; set; }
        public string? RedemptionNumber { get; set; }
        public decimal RedeemedAmount { get; set; }
        public decimal RemainingBalance { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class CreateAssetLienRequest
    {
        public string LoanId { get; set; } = string.Empty;
        public string AssetDescription { get; set; } = string.Empty;
        public string? AssetSerialNumber { get; set; }
        public string? AssetModel { get; set; }
        public decimal AssetValue { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class ReleaseAssetLienRequest
    {
        public string LienId { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
        public string ReleasedBy { get; set; } = string.Empty;
    }
}
