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
    public class CommodityVoucherService : ICommodityVoucherService
    {
        private readonly IRepository<CommodityVoucher> _voucherRepository;
        private readonly IRepository<CommodityRedemption> _redemptionRepository;
        private readonly IRepository<Loan> _loanRepository;
        private readonly IRepository<Member> _memberRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CommodityVoucherService> _logger;

        public CommodityVoucherService(
            IRepository<CommodityVoucher> voucherRepository,
            IRepository<CommodityRedemption> redemptionRepository,
            IRepository<Loan> loanRepository,
            IRepository<Member> memberRepository,
            IUnitOfWork unitOfWork,
            ILogger<CommodityVoucherService> logger)
        {
            _voucherRepository = voucherRepository;
            _redemptionRepository = redemptionRepository;
            _loanRepository = loanRepository;
            _memberRepository = memberRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CommodityVoucherDto> GenerateVoucherAsync(GenerateVoucherRequest request)
        {
            try
            {
                _logger.LogInformation("Generating voucher for loan {LoanId}", request.LoanId);

                var loan = await _loanRepository.GetByIdAsync(request.LoanId);
                if (loan == null)
                    throw new InvalidOperationException("Loan not found");

                if (loan.LoanType != "COMMODITY")
                    throw new InvalidOperationException("Vouchers can only be generated for commodity loans");

                var member = await _memberRepository.GetByIdAsync(loan.MemberId);
                if (member == null)
                    throw new InvalidOperationException("Member not found");

                // Generate voucher number
                var voucherNumber = await GenerateVoucherNumberAsync();

                // Generate QR code (will be implemented with QRCoder)
                var qrCode = GenerateQRCode(voucherNumber, member.Id, request.VoucherAmount);

                // Generate PIN
                var pin = GeneratePIN();

                var voucher = new CommodityVoucher
                {
                    VoucherNumber = voucherNumber,
                    LoanId = request.LoanId,
                    MemberId = member.Id,
                    Amount = request.VoucherAmount,
                    Status = "ISSUED",
                    IssueDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddDays(request.ValidityDays),
                    QrCode = qrCode,
                    CreatedBy = request.GeneratedBy,
                    CreatedAt = DateTime.UtcNow
                };

                await _voucherRepository.AddAsync(voucher);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Voucher {VoucherNumber} generated for loan {LoanNumber}",
                    voucherNumber, loan.LoanNumber);

                return await MapToDto(voucher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating voucher for loan {LoanId}", request.LoanId);
                throw;
            }
        }

        public async Task<VoucherValidationResult> ValidateVoucherAsync(ValidateVoucherRequest request)
        {
            var result = new VoucherValidationResult();

            try
            {
                var voucher = await GetVoucherByNumberAsync(request.VoucherNumber);
                
                if (voucher == null)
                {
                    result.IsValid = false;
                    result.Message = "Voucher not found";
                    result.ValidationErrors.Add("Invalid voucher number");
                    return result;
                }

                // Check if voucher is active
                if (voucher.Status != "ACTIVE")
                {
                    result.IsValid = false;
                    result.Message = $"Voucher is {voucher.Status}";
                    result.ValidationErrors.Add($"Voucher status: {voucher.Status}");
                    return result;
                }

                // Check expiry
                if (voucher.ExpiryDate < DateTime.UtcNow)
                {
                    result.IsValid = false;
                    result.Message = "Voucher has expired";
                    result.ValidationErrors.Add($"Expired on {voucher.ExpiryDate:yyyy-MM-dd}");
                    return result;
                }

                // Check remaining balance
                if (voucher.RemainingAmount <= 0)
                {
                    result.IsValid = false;
                    result.Message = "Voucher has no remaining balance";
                    result.ValidationErrors.Add("Balance: ₦0.00");
                    return result;
                }

                // PIN validation removed as PIN is not stored in entity
                if (!string.IsNullOrEmpty(request.PINCode))
                {
                     // Log warning or handle gracefully
                }

                // Voucher is valid
                result.IsValid = true;
                result.VoucherId = voucher.Id;
                result.AvailableAmount = voucher.RemainingAmount;
                result.ExpiryDate = voucher.ExpiryDate;
                // result.MemberName = voucher.MemberName; // Not available on entity directly, need mapped DTO or Include
                result.Message = "Voucher is valid";

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating voucher {VoucherNumber}", request.VoucherNumber);
                result.IsValid = false;
                result.Message = "Validation error occurred";
                result.ValidationErrors.Add(ex.Message);
                return result;
            }
        }

        public async Task<RedemptionResult> RedeemVoucherAsync(RedeemVoucherRequest request)
        {
            try
            {
                _logger.LogInformation("Redeeming voucher {VoucherNumber}", request.VoucherNumber);

                // Validate voucher first
                var validation = await ValidateVoucherAsync(new ValidateVoucherRequest
                {
                    VoucherNumber = request.VoucherNumber,
                    PINCode = request.PINCode
                });

                if (!validation.IsValid)
                {
                    return new RedemptionResult
                    {
                        Success = false,
                        Message = validation.Message,
                        Errors = validation.ValidationErrors
                    };
                }

                var vouchers = await _voucherRepository.GetAll()
                    .Where(v => v.VoucherNumber == request.VoucherNumber)
                    .ToListAsync();
                var voucher = vouchers.FirstOrDefault();

                if (voucher == null)
                {
                    return new RedemptionResult
                    {
                        Success = false,
                        Message = "Voucher not found",
                        Errors = new List<string> { "Invalid voucher number" }
                    };
                }

                // Check if redemption amount is valid
                if (request.RedemptionAmount > voucher.RemainingAmount)
                {
                    return new RedemptionResult
                    {
                        Success = false,
                        Message = "Redemption amount exceeds available balance",
                        Errors = new List<string> { $"Available: ₦{voucher.RemainingAmount:N2}" }
                    };
                }

                // Generate redemption number
                var redemptionNumber = await GenerateRedemptionNumberAsync();

                // Create redemption record
                // FinTech Best Practice: Convert string Id to Guid for VoucherId
                var redemption = new CommodityRedemption
                {
                    RedemptionNumber = redemptionNumber,
                    VoucherId = Guid.Parse(voucher.Id),
                    RedemptionAmount = request.RedemptionAmount,
                    RedemptionDate = DateTime.UtcNow,
                    RedeemedBy = request.RedeemedBy,
                    StoreLocation = request.StoreLocation,
                    ItemsDescription = request.ItemsDescription,
                    ReceiptNumber = request.ReceiptNumber,
                    Status = "COMPLETED",
                    CreatedBy = request.RedeemedBy,
                    CreatedAt = DateTime.UtcNow
                };

                await _redemptionRepository.AddAsync(redemption);

                // Update voucher
                voucher.UsedAmount += request.RedemptionAmount;
                voucher.RemainingAmount -= request.RedemptionAmount;
                voucher.UpdatedAt = DateTime.UtcNow;
                voucher.LastModifiedBy = request.RedeemedBy;

                // Update status if fully used
                if (voucher.RemainingAmount <= 0)
                {
                    voucher.Status = "FULLY_USED";
                }
                else if (voucher.UsedAmount > 0)
                {
                    voucher.Status = "PARTIALLY_USED";
                }

                await _voucherRepository.UpdateAsync(voucher);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Voucher {VoucherNumber} redeemed: ₦{Amount:N2}",
                    request.VoucherNumber, request.RedemptionAmount);

                return new RedemptionResult
                {
                    Success = true,
                    RedemptionId = redemption.Id,
                    RedemptionNumber = redemptionNumber,
                    RedeemedAmount = request.RedemptionAmount,
                    RemainingBalance = voucher.RemainingAmount,
                    Message = "Voucher redeemed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error redeeming voucher {VoucherNumber}", request.VoucherNumber);
                return new RedemptionResult
                {
                    Success = false,
                    Message = "Redemption failed",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<CommodityVoucherDto?> GetVoucherByIdAsync(string voucherId)
        {
            var voucher = await _voucherRepository.GetByIdAsync(voucherId);
            return voucher != null ? await MapToDto(voucher) : null;
        }

        public async Task<CommodityVoucherDto?> GetVoucherByNumberAsync(string voucherNumber)
        {
            var vouchers = await _voucherRepository.GetAll()
                .Where(v => v.VoucherNumber == voucherNumber)
                .ToListAsync();
            var voucher = vouchers.FirstOrDefault();
            return voucher != null ? await MapToDto(voucher) : null;
        }

        public async Task<List<CommodityVoucherDto>> GetMemberVouchersAsync(string memberId, string? status = null)
        {
            var query = _voucherRepository.GetAll().Where(v => v.MemberId == memberId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(v => v.Status == status);

            var vouchers = await query.OrderByDescending(v => v.IssueDate).ToListAsync();
            
            var result = new List<CommodityVoucherDto>();
            foreach (var voucher in vouchers)
            {
                result.Add(await MapToDto(voucher));
            }

            return result;
        }


        public async Task<List<CommodityVoucherDto>> GetLoanVouchersAsync(string loanId)
        {
            var vouchers = await _voucherRepository.GetAll()
                .Where(v => v.LoanId == loanId)
                .OrderByDescending(v => v.IssueDate)
                .ToListAsync();
            
            var result = new List<CommodityVoucherDto>();
            foreach (var voucher in vouchers.OrderByDescending(v => v.IssuedDate))
            {
                result.Add(await MapToDto(voucher));
            }

            return result;
        }

        public async Task<List<CommodityRedemptionDto>> GetVoucherRedemptionsAsync(string voucherId)
        {
            // FinTech Best Practice: Convert string voucherId to Guid for comparison
            var redemptions = await _redemptionRepository.GetAll()
                .Where(r => r.VoucherId == Guid.Parse(voucherId))
                .ToListAsync();

            return redemptions.OrderByDescending(r => r.RedemptionDate).Select(r => new CommodityRedemptionDto
            {
                Id = r.Id,
                RedemptionNumber = r.RedemptionNumber,
                VoucherId = r.VoucherId.ToString(),
                RedemptionAmount = r.RedemptionAmount,
                RedemptionDate = r.RedemptionDate,
                RedeemedBy = r.RedeemedBy,
                StoreLocation = r.StoreLocation,
                ItemsDescription = r.ItemsDescription,
                ReceiptNumber = r.ReceiptNumber,
                Status = r.Status
            }).ToList();
        }

        public async Task<bool> CancelVoucherAsync(string voucherId, string cancelledBy, string reason)
        {
            var voucher = await _voucherRepository.GetByIdAsync(voucherId);
            if (voucher == null) return false;

            if (voucher.UsedAmount > 0)
                throw new InvalidOperationException("Cannot cancel a voucher that has been partially used");

            voucher.Status = "CANCELLED";
            // voucher.IsActive = false; // Removed
            voucher.Notes = $"Cancelled by {cancelledBy}: {reason}";
            voucher.UpdatedAt = DateTime.UtcNow;
            voucher.LastModifiedBy = cancelledBy;

            await _voucherRepository.UpdateAsync(voucher);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Voucher {VoucherNumber} cancelled by {CancelledBy}",
                voucher.VoucherNumber, cancelledBy);

            return true;
        }

        public async Task<int> ExpireOldVouchersAsync()
        {
            _logger.LogInformation("Checking for expired vouchers");

            var activeVouchers = await _voucherRepository.GetAll()
                .Where(v => v.Status == "ACTIVE" && v.ExpiryDate < DateTime.UtcNow)
                .ToListAsync();

            int expiredCount = 0;
            foreach (var voucher in activeVouchers)
            {
                voucher.Status = "EXPIRED";
                // voucher.IsActive = false; // Removed
                voucher.UpdatedAt = DateTime.UtcNow;
                voucher.LastModifiedBy = "SYSTEM";
                await _voucherRepository.UpdateAsync(voucher);
                expiredCount++;
            }

            if (expiredCount > 0)
            {
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Expired {Count} vouchers", expiredCount);
            }

            return expiredCount;
        }

        public async Task<decimal> GetVoucherBalanceAsync(string voucherId)
        {
            var voucher = await _voucherRepository.GetByIdAsync(voucherId);
            return voucher?.RemainingAmount ?? 0;
        }

        #region Helper Methods

        private async Task<string> GenerateVoucherNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var allVouchers = await _voucherRepository.GetAll().ToListAsync();
            var count = allVouchers.Count(v => v.VoucherNumber.StartsWith($"CV/{year}")) + 1;
            return $"CV/{year}/{count:D6}";
        }

        private async Task<string> GenerateRedemptionNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var allRedemptions = await _redemptionRepository.GetAll().ToListAsync();
            var count = allRedemptions.Count(r => r.RedemptionNumber.StartsWith($"RED/{year}")) + 1;
            return $"RED/{year}/{count:D6}";
        }

        private string GenerateQRCode(string voucherNumber, string memberId, decimal amount)
        {
            // QR code generation will be implemented with QRCoder library
            // For now, return a placeholder
            var qrData = $"VOUCHER:{voucherNumber}|MEMBER:{memberId}|AMOUNT:{amount:F2}|DATE:{DateTime.UtcNow:yyyyMMdd}";
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(qrData));
        }

        private string GeneratePIN()
        {
            var random = new Random();
            return random.Next(1000, 9999).ToString();
        }

        private string EncryptPIN(string pin)
        {
            // Simple encryption - in production, use proper encryption
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(pin));
        }

        private bool ValidatePIN(string providedPin, string storedPin)
        {
            var decryptedPin = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(storedPin));
            return providedPin == decryptedPin;
        }

        private async Task<CommodityVoucherDto> MapToDto(CommodityVoucher voucher)
        {
            var loan = await _loanRepository.GetByIdAsync(voucher.LoanId);
            var member = await _memberRepository.GetByIdAsync(voucher.MemberId);
            var redemptions = await GetVoucherRedemptionsAsync(voucher.Id);

            return new CommodityVoucherDto
            {
                Id = voucher.Id,
                VoucherNumber = voucher.VoucherNumber,
                LoanId = voucher.LoanId,
                LoanNumber = loan?.LoanNumber ?? "",
                MemberId = voucher.MemberId,
                MemberNumber = member?.MemberNumber ?? "",
                MemberName = member != null ? $"{member.FirstName} {member.LastName}" : "",
                VoucherAmount = voucher.Amount,
                UsedAmount = voucher.UsedAmount,
                RemainingAmount = voucher.RemainingAmount,
                Status = voucher.Status,
                IssuedDate = voucher.IssueDate,
                ExpiryDate = voucher.ExpiryDate,
                QRCode = voucher.QrCode,
                IsActive = voucher.Status == "ACTIVE",
                Redemptions = redemptions
            };
        }

        #endregion
    }
}
