using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Security
{
    /// <summary>
    /// Security dashboard DTO
    /// </summary>
    public class SecurityDashboardDto
    {
        /// <summary>
        /// Gets or sets whether MFA is enabled
        /// </summary>
        public bool MfaEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets the last password change date
        /// </summary>
        public DateTime? LastPasswordChange { get; set; }
        
        /// <summary>
        /// Gets or sets the number of active sessions
        /// </summary>
        public int ActiveSessionCount { get; set; }
        
        /// <summary>
        /// Gets or sets the number of trusted devices
        /// </summary>
        public int TrustedDeviceCount { get; set; }
        
        /// <summary>
        /// Gets or sets the number of linked social accounts
        /// </summary>
        public int LinkedSocialAccountCount { get; set; }
        
        /// <summary>
        /// Gets or sets the number of recent suspicious activities
        /// </summary>
        public int RecentSuspiciousActivityCount { get; set; }
        
        /// <summary>
        /// Gets or sets the most recent login date
        /// </summary>
        public DateTime? MostRecentLogin { get; set; }
        
        /// <summary>
        /// Gets or sets the security score (0-100)
        /// </summary>
        public int SecurityScore { get; set; }
        
        /// <summary>
        /// Gets or sets security recommendations
        /// </summary>
        public List<SecurityRecommendationDto> Recommendations { get; set; }
    }
}
