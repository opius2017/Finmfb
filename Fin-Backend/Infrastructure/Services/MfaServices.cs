using Fin_Backend.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Fin_Backend.Infrastructure.Services
{
    /// <summary>
    /// App-based MFA service
    /// </summary>
    public class AppBasedMfaService : IMfaService
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
        public string GenerateSecretKey()
        {
            var bytes = new byte[20]; // 160 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            return Convert.ToBase64String(bytes);
        }

        /// <inheritdoc/>
        public string GetMfaSetupQrCodeUri(string secretKey, string email, string issuer = "FinTech")
        {
            var keyBase32 = ConvertToBase32(secretKey);
            
            // Format: otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}
            var uri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={keyBase32}&issuer={Uri.EscapeDataString(issuer)}&digits={TotpCodeLength}&period={TotpPeriod}";
            
            return uri;
        }

        /// <inheritdoc/>
        public bool ValidateCode(string secretKey, string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != TotpCodeLength || !code.All(char.IsDigit))
            {
                return false;
            }

            var currentTimeStep = GetCurrentTimeStep();
            var secretKeyBytes = Convert.FromBase64String(secretKey);

            // Check for time drift (one step before and after)
            for (int i = -MaxTimeDrift; i <= MaxTimeDrift; i++)
            {
                var timeStep = currentTimeStep + i;
                var expectedCode = GenerateTotp(secretKeyBytes, timeStep);
                
                if (string.Equals(expectedCode, code, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

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
    public class EmailMfaService : IMfaService
    {
        private readonly IMfaNotificationService _notificationService;
        private readonly Random _random = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailMfaService"/> class
        /// </summary>
        /// <param name="notificationService">The notification service</param>
        public EmailMfaService(IMfaNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <inheritdoc/>
        public string GenerateSecretKey()
        {
            return GenerateCode();
        }

        /// <inheritdoc/>
        public string GetMfaSetupQrCodeUri(string secretKey, string email, string issuer = "FinTech")
        {
            // Email-based MFA doesn't use QR codes
            return null;
        }

        /// <inheritdoc/>
        public bool ValidateCode(string secretKey, string code)
        {
            return string.Equals(secretKey, code, StringComparison.Ordinal);
        }

        /// <summary>
        /// Sends an MFA code to an email address
        /// </summary>
        /// <param name="email">The email address</param>
        /// <returns>The MFA code</returns>
        public async Task<string> SendMfaCodeAsync(string email)
        {
            var code = GenerateCode();
            await _notificationService.SendMfaCodeEmailAsync(email, code);
            return code;
        }

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
    public class SmsMfaService : IMfaService
    {
        private readonly IMfaNotificationService _notificationService;
        private readonly Random _random = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsMfaService"/> class
        /// </summary>
        /// <param name="notificationService">The notification service</param>
        public SmsMfaService(IMfaNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <inheritdoc/>
        public string GenerateSecretKey()
        {
            return GenerateCode();
        }

        /// <inheritdoc/>
        public string GetMfaSetupQrCodeUri(string secretKey, string email, string issuer = "FinTech")
        {
            // SMS-based MFA doesn't use QR codes
            return null;
        }

        /// <inheritdoc/>
        public bool ValidateCode(string secretKey, string code)
        {
            return string.Equals(secretKey, code, StringComparison.Ordinal);
        }

        /// <summary>
        /// Sends an MFA code to a phone number
        /// </summary>
        /// <param name="phoneNumber">The phone number</param>
        /// <returns>The MFA code</returns>
        public async Task<string> SendMfaCodeAsync(string phoneNumber)
        {
            var code = GenerateCode();
            await _notificationService.SendMfaCodeSmsAsync(phoneNumber, code);
            return code;
        }

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
    /// MFA service factory
    /// </summary>
    public class MfaServiceFactory
    {
        private readonly AppBasedMfaService _appBasedMfaService;
        private readonly EmailMfaService _emailMfaService;
        private readonly SmsMfaService _smsMfaService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MfaServiceFactory"/> class
        /// </summary>
        /// <param name="appBasedMfaService">The app-based MFA service</param>
        /// <param name="emailMfaService">The email-based MFA service</param>
        /// <param name="smsMfaService">The SMS-based MFA service</param>
        public MfaServiceFactory(
            AppBasedMfaService appBasedMfaService,
            EmailMfaService emailMfaService,
            SmsMfaService smsMfaService)
        {
            _appBasedMfaService = appBasedMfaService;
            _emailMfaService = emailMfaService;
            _smsMfaService = smsMfaService;
        }

        /// <summary>
        /// Gets an MFA service by method
        /// </summary>
        /// <param name="method">The method</param>
        /// <returns>The MFA service</returns>
        public IMfaService GetMfaService(string method)
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