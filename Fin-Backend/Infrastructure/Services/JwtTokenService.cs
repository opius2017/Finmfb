using Fin_Backend.Application.Common.Settings;
using Fin_Backend.Domain.Entities.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Fin_Backend.Infrastructure.Services
{
    /// <summary>
    /// JWT token service
    /// </summary>
    public class JwtTokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenService"/> class
        /// </summary>
        /// <param name="jwtSettings">The JWT settings</param>
        /// <param name="userManager">The user manager</param>
        public JwtTokenService(IOptions<JwtSettings> jwtSettings, UserManager<ApplicationUser> userManager)
        {
            _jwtSettings = jwtSettings.Value;
            _userManager = userManager;
        }

        /// <summary>
        /// Generates a JWT token for a user
        /// </summary>
        /// <param name="user">The user</param>
        /// <returns>The JWT token</returns>
        public async Task<JwtSecurityToken> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id),
                new Claim("username", user.UserName)
            };

            claims.AddRange(userClaims);
            claims.AddRange(roleClaims);

            // Add custom claims
            if (user.IsMfaEnabled)
            {
                claims.Add(new Claim("mfa_enabled", "true"));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return token;
        }

        /// <summary>
        /// Generates a refresh token
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="jwtId">The JWT ID</param>
        /// <param name="ipAddress">The IP address</param>
        /// <param name="deviceId">The device ID</param>
        /// <param name="expiryTimeInDays">The expiry time in days</param>
        /// <returns>The refresh token</returns>
        public RefreshToken GenerateRefreshToken(string userId, string jwtId, string ipAddress, string deviceId, int expiryTimeInDays = 7)
        {
            return new RefreshToken
            {
                UserId = userId,
                Token = GenerateRefreshTokenString(),
                JwtId = jwtId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(expiryTimeInDays),
                IsUsed = false,
                IsRevoked = false,
                IpAddress = ipAddress,
                DeviceId = deviceId
            };
        }

        /// <summary>
        /// Validates a JWT token
        /// </summary>
        /// <param name="token">The token</param>
        /// <returns>The principal</returns>
        public ClaimsPrincipal ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the expiration date from a JWT token
        /// </summary>
        /// <param name="token">The token</param>
        /// <returns>The expiration date</returns>
        public DateTime GetJwtTokenExpirationDate(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            return jwtToken.ValidTo;
        }

        /// <summary>
        /// Gets the JWT ID from a JWT token
        /// </summary>
        /// <param name="token">The token</param>
        /// <returns>The JWT ID</returns>
        public string GetJwtIdFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            return jwtToken.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Jti).Value;
        }

        /// <summary>
        /// Gets the user ID from a JWT token
        /// </summary>
        /// <param name="token">The token</param>
        /// <returns>The user ID</returns>
        public string GetUserIdFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            return jwtToken.Claims.First(claim => claim.Type == "uid").Value;
        }

        /// <summary>
        /// Generates a refresh token string
        /// </summary>
        /// <returns>The refresh token string</returns>
        private string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
