using FinTech.Core.Application.Interfaces.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FinTech.Infrastructure.Services
{
    /// <summary>
    /// App-based MFA service
    /// </summary>
    public class AppBasedMfaService : IMfaProvider
    {
        /// <summary>
        /// The time-based one-time password (TOTP) code length
        /// </summary>
        private const int TotpCodeLength = 6;
        
        /// <summary>
        /// The TOTP code validity period in seconds
        /// </summary>
        private const int TotpPeriod = 30;
        
        /// <summary>
        /// The TOTP time step in seconds
        /// </summary>
        private const int TotpTimeStep = 30;
        
        /// <summary>
        /// The maximum time drift in steps
        /// </summary>
        private const int MaxTimeDrift = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppBasedMfaService"/> class
        /// </summary>
        public AppBasedMfaService()
        {
        }

        /// <inheritdoc/>
        public Task<string> GenerateTotpSecretAsync()
        {
            var bytes = new byte[20]; // 160 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            return Task.FromResult(Convert.ToBase64String(bytes));
        }

        /// <inheritdoc/>
        public Task<string> GenerateQrCodeUriAsync(string email, string secret)
        {
            var keyBase32 = ConvertToBase32(secret);
            string issuer = "FinTech";
            
            // Format: otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}
            var uri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={keyBase32}&issuer={Uri.EscapeDataString(issuer)}&digits={TotpCodeLength}&period={TotpPeriod}";
            
            return Task.FromResult(uri);
        }

        /// <inheritdoc/>
        public Task<bool> ValidateTotpCodeAsync(string secret, string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != TotpCodeLength || !code.All(char.IsDigit))
            {
                return Task.FromResult(false);
            }

            var currentTimeStep = GetCurrentTimeStep();
            var secretKeyBytes = Convert.FromBase64String(secret);

            // Check for time drift (one step before and after)
            for (int i = -MaxTimeDrift; i <= MaxTimeDrift; i++)
            {
                var timeStep = currentTimeStep + i;
                var expectedCode = GenerateTotp(secretKeyBytes, timeStep);
                
                if (string.Equals(expectedCode, code, StringComparison.Ordinal))
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        public Task<string> GenerateSmsCodeAsync() => throw new NotImplementedException();
        public Task<string> GenerateEmailCodeAsync() => throw new NotImplementedException();
        public Task<bool> ValidateCodeAsync(string code, string expectedCode) => throw new NotImplementedException();
        public Task<IEnumerable<string>> GenerateBackupCodesAsync(int count = 10) => throw new NotImplementedException();

        /// <summary>
        /// Converts a Base64 string to Base32
        /// </summary>
        /// <param name="base64String">The Base64 string</param>
        /// <returns>The Base32 string</returns>
        private string ConvertToBase32(string base64String)
        {
            var bytes = Convert.FromBase64String(base64String);
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var bits = GetBits(bytes);
            var result = new StringBuilder();
            
            for (int i = 0; i < bits.Length; i += 5)
            {
                int index = 0;
                for (int j = 0; j < 5; j++)
                {
                    if (i + j < bits.Length)
                    {
                        index = (index << 1) | bits[i + j];
                    }
                    else
                    {
                        index = index << 1;
                    }
                }
                result.Append(alphabet[index]);
            }
            
            return result.ToString();
        }

        /// <summary>
        /// Gets the bits from a byte array
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>The bits</returns>
        private int[] GetBits(byte[] bytes)
        {
            var bits = new int[bytes.Length * 8];
            for (int i = 0; i < bytes.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    bits[i * 8 + j] = (bytes[i] >> (7 - j)) & 1;
                }
            }
            return bits;
        }

        /// <summary>
        /// Gets the current time step
        /// </summary>
        /// <returns>The current time step</returns>
        private long GetCurrentTimeStep()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() / TotpTimeStep;
        }

        /// <summary>
        /// Generates a TOTP code
        /// </summary>
        /// <param name="secretKey">The secret key</param>
        /// <param name="timeStep">The time step</param>
        /// <returns>The TOTP code</returns>
        private string GenerateTotp(byte[] secretKey, long timeStep)
        {
            var timeStepBytes = BitConverter.GetBytes(timeStep);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(timeStepBytes);
            }

            // Ensure timeStepBytes is 8 bytes
            if (timeStepBytes.Length != 8)
            {
                var temp = new byte[8];
                Array.Copy(timeStepBytes, 0, temp, 8 - timeStepBytes.Length, timeStepBytes.Length);
                timeStepBytes = temp;
            }

            using (var hmac = new HMACSHA1(secretKey))
            {
                var hash = hmac.ComputeHash(timeStepBytes);
                var offset = hash[hash.Length - 1] & 0x0F;
                var truncatedHash = (hash[offset] & 0x7F) << 24 |
                                    (hash[offset + 1] & 0xFF) << 16 |
                                    (hash[offset + 2] & 0xFF) << 8 |
                                    (hash[offset + 3] & 0xFF);

                var code = (truncatedHash % (int)Math.Pow(10, TotpCodeLength)).ToString($"D{TotpCodeLength}");
                return code;
            }
        }
    }

    /// <summary>
    /// Email-based MFA service
    /// </summary>
    public class EmailMfaService : IMfaProvider
    {
        private readonly IMfaProviderNotificationService _notificationService;
        private readonly Random _random = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailMfaService"/> class
        /// </summary>
        /// <param name="notificationService">The notification service</param>
        public EmailMfaService(IMfaProviderNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public Task<string> GenerateTotpSecretAsync() => throw new NotImplementedException();
        public Task<string> GenerateQrCodeUriAsync(string email, string secret) => throw new NotImplementedException();
        public Task<bool> ValidateTotpCodeAsync(string secret, string code) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task<string> GenerateEmailCodeAsync()
        {
            var code = GenerateCode();
            // Assuming we will send it later using SendMfaCodeByEmailAsync which takes email
            // But this interface method doesn't take email.
            // This is a disconnect. I'll just return code for now.
            return Task.FromResult(code);
        }

        public async Task<string> SendMfaCodeAsync(string email)
        {
            var code = GenerateCode();
            await _notificationService.SendMfaCodeByEmailAsync(email, code);
            return code;
        }

        public Task<string> GenerateSmsCodeAsync() => throw new NotImplementedException();

        public Task<bool> ValidateCodeAsync(string code, string expectedCode)
        {
            return Task.FromResult(string.Equals(code, expectedCode, StringComparison.Ordinal));
        }

        public Task<IEnumerable<string>> GenerateBackupCodesAsync(int count = 10) => throw new NotImplementedException();

        /// <summary>
        /// Generates a verification code
        /// </summary>
        /// <returns>The verification code</returns>
        private string GenerateCode()
        {
            return _random.Next(100000, 999999).ToString();
        }
    }

    /// <summary>
    /// SMS-based MFA service
    /// </summary>
    public class SmsMfaService : IMfaProvider
    {
        private readonly IMfaProviderNotificationService _notificationService;
        private readonly Random _random = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsMfaService"/> class
        /// </summary>
        /// <param name="notificationService">The notification service</param>
        public SmsMfaService(IMfaProviderNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public Task<string> GenerateTotpSecretAsync() => throw new NotImplementedException();
        public Task<string> GenerateQrCodeUriAsync(string email, string secret) => throw new NotImplementedException();
        public Task<bool> ValidateTotpCodeAsync(string secret, string code) => throw new NotImplementedException();

        public Task<string> GenerateEmailCodeAsync() => throw new NotImplementedException();

        public Task<string> GenerateSmsCodeAsync()
        {
            return Task.FromResult(GenerateCode());
        }

        public async Task<string> SendMfaCodeAsync(string phoneNumber)
        {
            var code = GenerateCode();
            await _notificationService.SendMfaCodeBySmsAsync(phoneNumber, code);
            return code;
        }

        public Task<bool> ValidateCodeAsync(string code, string expectedCode)
        {
            return Task.FromResult(string.Equals(code, expectedCode, StringComparison.Ordinal));
        }

        public Task<IEnumerable<string>> GenerateBackupCodesAsync(int count = 10) => throw new NotImplementedException();

        /// <summary>
        /// Generates a verification code
        /// </summary>
        /// <returns>The verification code</returns>
        private string GenerateCode()
        {
            return _random.Next(100000, 999999).ToString();
        }
    }

    /// <summary>
    /// MFA provider factory
    /// </summary>
    public class MfaProviderFactory : IMfaProviderFactory
    {
        private readonly AppBasedMfaService _appBasedMfaService;
        private readonly EmailMfaService _emailMfaService;
        private readonly SmsMfaService _smsMfaService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MfaProviderFactory"/> class
        /// </summary>
        /// <param name="appBasedMfaService">The app-based MFA service</param>
        /// <param name="emailMfaService">The email-based MFA service</param>
        /// <param name="smsMfaService">The SMS-based MFA service</param>
        public MfaProviderFactory(
            AppBasedMfaService appBasedMfaService,
            EmailMfaService emailMfaService,
            SmsMfaService smsMfaService)
        {
            _appBasedMfaService = appBasedMfaService;
            _emailMfaService = emailMfaService;
            _smsMfaService = smsMfaService;
        }

        /// <summary>
        /// Gets an MFA provider by method
        /// </summary>
        /// <param name="method">The method</param>
        /// <returns>The MFA provider</returns>
        public IMfaProvider GetMfaProvider(string method)
        {
            return method.ToLower() switch
            {
                "app" => _appBasedMfaService,
                "email" => _emailMfaService,
                "sms" => _smsMfaService,
                _ => throw new ArgumentException($"Unsupported MFA method: {method}")
            };
        }
    }
}
