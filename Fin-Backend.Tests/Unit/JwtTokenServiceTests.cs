using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using FinTech.WebAPI.Application.Common.Settings;
using FinTech.WebAPI.Application.DTOs.Auth;
using FinTech.WebAPI.Application.Interfaces.Repositories;
using FinTech.WebAPI.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FinTech.Tests.Unit
{
    public class JwtTokenServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IOptions<JwtSettings>> _mockJwtSettings;
        private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
        private readonly JwtTokenService _sut;

        public JwtTokenServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            
            _mockJwtSettings = _fixture.Freeze<Mock<IOptions<JwtSettings>>>();
            _mockRefreshTokenRepository = _fixture.Freeze<Mock<IRefreshTokenRepository>>();
            
            // Setup UserManager mock
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            
            // Configure JWT Settings
            _mockJwtSettings
                .Setup(s => s.Value)
                .Returns(new JwtSettings
                {
                    Key = "TestSecretKeyForDevelopmentPurposesOnlyWithSufficientLength",
                    Issuer = "FinTechAPI.Test",
                    Audience = "FinTechClient.Test",
                    DurationInMinutes = 5,
                    RefreshTokenExpiryInDays = 7
                });
            
            // Create system under test
            _sut = new JwtTokenService(
                _mockJwtSettings.Object,
                _mockRefreshTokenRepository.Object,
                _mockUserManager.Object
            );
        }

        [Fact]
        public async Task GenerateJwtTokenAsync_ValidUser_ReturnsTokenWithClaims()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new IdentityUser
            {
                Id = userId,
                UserName = "test@example.com",
                Email = "test@example.com"
            };
            
            var roles = new List<string> { "User" };
            
            _mockUserManager
                .Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(roles);
            
            // Act
            var result = await _sut.GenerateJwtTokenAsync(user);

            // Assert
            result.Should().NotBeNullOrEmpty();
            
            // Decode token to verify claims
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(result) as JwtSecurityToken;
            
            jsonToken.Should().NotBeNull();
            jsonToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId);
            jsonToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "test@example.com");
            jsonToken.Claims.Should().Contain(c => c.Type == "role" && c.Value == "User");
            jsonToken.ValidTo.Should().BeAfter(DateTime.UtcNow);
            jsonToken.ValidTo.Should().BeBefore(DateTime.UtcNow.AddMinutes(6));
        }

        [Fact]
        public async Task GenerateRefreshTokenAsync_ValidUser_CreatesAndStoresRefreshToken()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var deviceInfo = new DeviceInfo
            {
                ClientIp = "192.168.1.1",
                Browser = "Chrome",
                DeviceId = "device123",
                DeviceName = "Test Device",
                DeviceType = "Desktop",
                OperatingSystem = "Windows",
                BrowserVersion = "98.0"
            };
            
            // Act
            var result = await _sut.GenerateRefreshTokenAsync(userId, deviceInfo);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.ExpiryDate.Should().BeAfter(DateTime.UtcNow.AddDays(6));
            result.ExpiryDate.Should().BeBefore(DateTime.UtcNow.AddDays(8));
            
            _mockRefreshTokenRepository.Verify(
                m => m.AddAsync(It.Is<Domain.Entities.Authentication.RefreshToken>(
                    t => t.UserId == userId && 
                         t.Token == result.Token &&
                         !t.IsRevoked &&
                         t.DeviceInfo.Contains(deviceInfo.DeviceId))),
                Times.Once);
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_ValidToken_ReturnsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var refreshToken = Guid.NewGuid().ToString();
            
            _mockRefreshTokenRepository
                .Setup(m => m.GetByTokenAsync(refreshToken))
                .ReturnsAsync(new Domain.Entities.Authentication.RefreshToken
                {
                    UserId = userId,
                    Token = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(5),
                    IsRevoked = false,
                    CreatedByIp = "192.168.1.1",
                    DeviceInfo = "Chrome on Windows"
                });
            
            // Act
            var result = await _sut.ValidateRefreshTokenAsync(refreshToken, userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_ExpiredToken_ReturnsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var refreshToken = Guid.NewGuid().ToString();
            
            _mockRefreshTokenRepository
                .Setup(m => m.GetByTokenAsync(refreshToken))
                .ReturnsAsync(new Domain.Entities.Authentication.RefreshToken
                {
                    UserId = userId,
                    Token = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(-1), // Expired
                    IsRevoked = false,
                    CreatedByIp = "192.168.1.1",
                    DeviceInfo = "Chrome on Windows"
                });
            
            // Act
            var result = await _sut.ValidateRefreshTokenAsync(refreshToken, userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateRefreshTokenAsync_RevokedToken_ReturnsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var refreshToken = Guid.NewGuid().ToString();
            
            _mockRefreshTokenRepository
                .Setup(m => m.GetByTokenAsync(refreshToken))
                .ReturnsAsync(new Domain.Entities.Authentication.RefreshToken
                {
                    UserId = userId,
                    Token = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(5),
                    IsRevoked = true, // Revoked
                    CreatedByIp = "192.168.1.1",
                    DeviceInfo = "Chrome on Windows"
                });
            
            // Act
            var result = await _sut.ValidateRefreshTokenAsync(refreshToken, userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RevokeRefreshTokenAsync_ValidToken_RevokesToken()
        {
            // Arrange
            var refreshToken = Guid.NewGuid().ToString();
            
            _mockRefreshTokenRepository
                .Setup(m => m.GetByTokenAsync(refreshToken))
                .ReturnsAsync(new Domain.Entities.Authentication.RefreshToken
                {
                    Token = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(5),
                    IsRevoked = false,
                    CreatedByIp = "192.168.1.1",
                    DeviceInfo = "Chrome on Windows"
                });
            
            // Act
            var result = await _sut.RevokeRefreshTokenAsync(refreshToken);

            // Assert
            result.Should().BeTrue();
            
            _mockRefreshTokenRepository.Verify(
                m => m.UpdateAsync(It.Is<Domain.Entities.Authentication.RefreshToken>(
                    t => t.Token == refreshToken && t.IsRevoked == true)),
                Times.Once);
        }

        [Fact]
        public void ValidateToken_ValidToken_ReturnsClaimsPrincipal()
        {
            // Arrange
            var user = new IdentityUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "test@example.com",
                Email = "test@example.com"
            };
            
            var roles = new List<string> { "User" };
            
            _mockUserManager
                .Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(roles);
            
            var token = _sut.GenerateJwtTokenAsync(user).Result;
            
            // Act
            var result = _sut.ValidateToken(token);

            // Assert
            result.Should().NotBeNull();
            result.Identity.Should().NotBeNull();
            result.Identity.IsAuthenticated.Should().BeTrue();
            result.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "test@example.com");
        }
    }
}