namespace FinTech.Core.Application.DTOs.Security
{
    /// <summary>
    /// Security preferences DTO
    /// </summary>
    public class SecurityPreferencesDto
    {
        /// <summary>
        /// Gets or sets whether MFA is enabled
        /// </summary>
        public bool MfaEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets whether to require MFA for new devices
        /// </summary>
        public bool RequireMfaForNewDevices { get; set; }
        
        /// <summary>
        /// Gets or sets whether to notify on new logins
        /// </summary>
        public bool NotifyOnNewLogins { get; set; }
        
        /// <summary>
        /// Gets or sets whether to notify on suspicious activities
        /// </summary>
        public bool NotifyOnSuspiciousActivity { get; set; }
        
        /// <summary>
        /// Gets or sets whether to use biometric authentication (if available)
        /// </summary>
        public bool UseBiometricIfAvailable { get; set; }
        
        /// <summary>
        /// Gets or sets the preferred MFA method
        /// </summary>
        public string PreferredMfaMethod { get; set; }
    }
}
