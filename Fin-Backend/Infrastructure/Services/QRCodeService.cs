using System;
using System.IO;
using Microsoft.Extensions.Logging;
using QRCoder;

namespace FinTech.Infrastructure.Services
{
    /// <summary>
    /// Service for generating QR codes for commodity vouchers
    /// </summary>
    public class QRCodeService
    {
        private readonly ILogger<QRCodeService> _logger;

        public QRCodeService(ILogger<QRCodeService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Generate QR code for a voucher
        /// </summary>
        /// <param name="voucherCode">The voucher code to encode</param>
        /// <param name="pixelsPerModule">Size of each module (default: 20)</param>
        /// <returns>QR code as PNG byte array</returns>
        public byte[] GenerateQRCode(string voucherCode, int pixelsPerModule = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(voucherCode))
                {
                    throw new ArgumentException("Voucher code cannot be empty", nameof(voucherCode));
                }

                _logger.LogInformation("Generating QR code for voucher: {VoucherCode}", voucherCode);

                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(voucherCode, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                
                var qrCodeImage = qrCode.GetGraphic(pixelsPerModule);

                _logger.LogInformation("Successfully generated QR code for voucher: {VoucherCode}", voucherCode);

                return qrCodeImage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for voucher: {VoucherCode}", voucherCode);
                throw;
            }
        }

        /// <summary>
        /// Generate QR code with custom colors
        /// </summary>
        /// <param name="voucherCode">The voucher code to encode</param>
        /// <param name="darkColorHex">Hex color for dark modules (e.g., "#000000")</param>
        /// <param name="lightColorHex">Hex color for light modules (e.g., "#FFFFFF")</param>
        /// <param name="pixelsPerModule">Size of each module (default: 20)</param>
        /// <returns>QR code as PNG byte array</returns>
        public byte[] GenerateQRCodeWithColors(
            string voucherCode, 
            string darkColorHex = "#000000", 
            string lightColorHex = "#FFFFFF",
            int pixelsPerModule = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(voucherCode))
                {
                    throw new ArgumentException("Voucher code cannot be empty", nameof(voucherCode));
                }

                _logger.LogInformation(
                    "Generating colored QR code for voucher: {VoucherCode} with colors {Dark}/{Light}", 
                    voucherCode, darkColorHex, lightColorHex);

                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(voucherCode, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                
                var qrCodeImage = qrCode.GetGraphic(
                    pixelsPerModule,
                    darkColorHex,
                    lightColorHex);

                _logger.LogInformation("Successfully generated colored QR code for voucher: {VoucherCode}", voucherCode);

                return qrCodeImage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating colored QR code for voucher: {VoucherCode}", voucherCode);
                throw;
            }
        }

        /// <summary>
        /// Generate QR code and save to file
        /// </summary>
        /// <param name="voucherCode">The voucher code to encode</param>
        /// <param name="filePath">Path where to save the QR code image</param>
        /// <param name="pixelsPerModule">Size of each module (default: 20)</param>
        public void GenerateQRCodeToFile(string voucherCode, string filePath, int pixelsPerModule = 20)
        {
            try
            {
                var qrCodeBytes = GenerateQRCode(voucherCode, pixelsPerModule);
                File.WriteAllBytes(filePath, qrCodeBytes);

                _logger.LogInformation("QR code saved to file: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving QR code to file: {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// Generate QR code as Base64 string (for embedding in HTML/JSON)
        /// </summary>
        /// <param name="voucherCode">The voucher code to encode</param>
        /// <param name="pixelsPerModule">Size of each module (default: 20)</param>
        /// <returns>Base64 encoded QR code image</returns>
        public string GenerateQRCodeAsBase64(string voucherCode, int pixelsPerModule = 20)
        {
            try
            {
                var qrCodeBytes = GenerateQRCode(voucherCode, pixelsPerModule);
                var base64String = Convert.ToBase64String(qrCodeBytes);

                _logger.LogInformation("Generated QR code as Base64 for voucher: {VoucherCode}", voucherCode);

                return base64String;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code as Base64 for voucher: {VoucherCode}", voucherCode);
                throw;
            }
        }

        /// <summary>
        /// Generate QR code as data URI (for direct use in img src)
        /// </summary>
        /// <param name="voucherCode">The voucher code to encode</param>
        /// <param name="pixelsPerModule">Size of each module (default: 20)</param>
        /// <returns>Data URI string</returns>
        public string GenerateQRCodeAsDataUri(string voucherCode, int pixelsPerModule = 20)
        {
            try
            {
                var base64String = GenerateQRCodeAsBase64(voucherCode, pixelsPerModule);
                var dataUri = $"data:image/png;base64,{base64String}";

                _logger.LogInformation("Generated QR code as Data URI for voucher: {VoucherCode}", voucherCode);

                return dataUri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code as Data URI for voucher: {VoucherCode}", voucherCode);
                throw;
            }
        }

        /// <summary>
        /// Validate voucher code format before generating QR code
        /// </summary>
        /// <param name="voucherCode">The voucher code to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateVoucherCode(string voucherCode)
        {
            if (string.IsNullOrWhiteSpace(voucherCode))
            {
                return false;
            }

            // Voucher code should be alphanumeric and between 8-20 characters
            if (voucherCode.Length < 8 || voucherCode.Length > 20)
            {
                return false;
            }

            // Check if alphanumeric with hyphens allowed
            foreach (char c in voucherCode)
            {
                if (!char.IsLetterOrDigit(c) && c != '-')
                {
                    return false;
                }
            }

            return true;
        }
    }
}
