using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using FinTech.WebAPI.Application.DTOs.Auth;
using FinTech.WebAPI.Application.Interfaces;
using FinTech.WebAPI.Application.Interfaces.Repositories;
using FinTech.WebAPI.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FinTech.Tests.Unit
{
    public class MfaServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IMfaSettingsRepository> _mockMfaSettingsRepository;
        private readonly Mock<IBackupCodeRepository> _mockBackupCodeRepository;
        private readonly Mock<IUserDeviceRepository> _mockUserDeviceRepository;
        private readonly Mock<ISecurityActivityRepository> _mockSecurityActivityRepository;
        private readonly Mock<IMfaProviderFactory> _mockMfaProviderFactory;
        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
        private readonly Mock<ILogger<MfaService>> _mockLogger;
        private readonly MfaService _sut;

        public MfaServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            
            _mockMfaSettingsRepository = _fixture.Freeze<Mock<IMfaSettingsRepository>>();
            _mockBackupCodeRepository = _fixture.Freeze<Mock<IBackupCodeRepository>>();
            _mockUserDeviceRepository = _fixture.Freeze<Mock<IUserDeviceRepository>>();
            _mockSecurityActivityRepository = _fixture.Freeze<Mock<ISecurityActivityRepository>>();
            _mockMfaProviderFactory = _fixture.Freeze<Mock<IMfaProviderFactory>>();
            
            // Setup UserManager mock
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            
            _mockLogger = _fixture.Freeze<Mock<ILogger<MfaService>>>();
            
            // Mock MFA providers
            var mockTotpProvider = new Mock<ITotpProvider>();
            mockTotpProvider
                .Setup(m => m.GenerateSetupInfo(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MfaSetupInfo
                {
                    Secret = "TESTSECRET",
                    QrCodeUrl = "https://example.com/qrcode",
                    ManualEntryKey = "TESTSECRET"
                });
            
            mockTotpProvider
                .Setup(m => m.VerifyCode(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((secret, code) => Task.FromResult(code == "123456"));
            
            _mockMfaProviderFactory
                .Setup(m => m.GetProvider("App"))
                .Returns(mockTotpProvider.Object);
            
            // Create system under test
            _sut = new MfaService(
                _mockMfaSettingsRepository.Object,
                _mockBackupCodeRepository.Object,
                _mockUserDeviceRepository.Object,
                _mockSecurityActivityRepository.Object,
                _mockMfaProviderFactory.Object,
                _mockUserManager.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task GenerateMfaSetupAsync_ValidUserId_ReturnsMfaSetupResponse()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var userName = "testuser";
            
            _mockUserManager
                .Setup(m => m.FindByIdAsync(userId))
                .ReturnsAsync(new IdentityUser { Id = userId, UserName = userName });
            
            // Act
            var result = await _sut.GenerateMfaSetupAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Secret.Should().Be("TESTSECRET");
            result.QrCodeUrl.Should().Be("https://example.com/qrcode");
            result.ManualEntryKey.Should().Be("TESTSECRET");
            result.BackupCodes.Should().NotBeNull();
            result.BackupCodes.Should().HaveCount(10);
        }

        [Fact]
        public async Task VerifyMfaSetupAsync_ValidCode_ReturnsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var secret = "TESTSECRET";
            var code = "123456";
            
            _mockMfaSettingsRepository
                .Setup(m => m.GetByUserIdAsync(userId))
                .ReturnsAsync(new Domain.Entities.Authentication.MfaSettings
                {
                    UserId = userId,
                    Secret = secret,
                    IsEnabled = false,
                    PreferredMethod = "App"
                });
            
            // Act
            var result = await _sut.VerifyMfaSetupAsync(userId, code);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task VerifyMfaSetupAsync_InvalidCode_ReturnsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var secret = "TESTSECRET";
            var code = "999999"; // Invalid code
            
            _mockMfaSettingsRepository
                .Setup(m => m.GetByUserIdAsync(userId))
                .ReturnsAsync(new Domain.Entities.Authentication.MfaSettings
                {
                    UserId = userId,
                    Secret = secret,
                    IsEnabled = false,
                    PreferredMethod = "App"
                });
            
            // Act
            var result = await _sut.VerifyMfaSetupAsync(userId, code);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task EnableMfaAsync_ValidParameters_EnablesMfa()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var secret = "TESTSECRET";
            
            _mockMfaSettingsRepository
                .Setup(m => m.GetByUserIdAsync(userId))
                .ReturnsAsync(new Domain.Entities.Authentication.MfaSettings
                {
                    UserId = userId,
                    Secret = secret,
                    IsEnabled = false,
                    PreferredMethod = "App"
                });
            
            // Act
            await _sut.EnableMfaAsync(userId, secret);

            // Assert
            _mockMfaSettingsRepository.Verify(
                m => m.UpdateAsync(It.Is<Domain.Entities.Authentication.MfaSettings>(
                    s => s.UserId == userId && s.IsEnabled == true && s.Secret == secret)),
                Times.Once);
            
            _mockSecurityActivityRepository.Verify(
                m => m.AddAsync(It.Is<Domain.Entities.Authentication.SecurityActivity>(
                    a => a.UserId == userId && a.EventType == "mfa_enabled")),
                Times.Once);
        }

        [Fact]
        public async Task ValidateMfaCodeAsync_ValidCode_ReturnsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var secret = "TESTSECRET";
            var code = "123456";
            
            _mockMfaSettingsRepository
                .Setup(m => m.GetByUserIdAsync(userId))
                .ReturnsAsync(new Domain.Entities.Authentication.MfaSettings
                {
                    UserId = userId,
                    Secret = secret,
                    IsEnabled = true,
                    PreferredMethod = "App"
                });
            
            // Act
            var result = await _sut.ValidateMfaCodeAsync(userId, code);

            // Assert
            result.Should().BeTrue();
            
            _mockMfaSettingsRepository.Verify(
                m => m.UpdateAsync(It.Is<Domain.Entities.Authentication.MfaSettings>(
                    s => s.UserId == userId && s.LastVerifiedAt != null)),
                Times.Once);
        }

        [Fact]
        public async Task ValidateBackupCodeAsync_ValidUnusedCode_ReturnsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var backupCode = "123456";
            
            _mockBackupCodeRepository
                .Setup(m => m.GetByUserIdAndCodeAsync(userId, backupCode))
                .ReturnsAsync(new Domain.Entities.Authentication.BackupCode
                {
                    UserId = userId,
                    Code = backupCode,
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                });
            
            // Act
            var result = await _sut.ValidateBackupCodeAsync(userId, backupCode);

            // Assert
            result.Should().BeTrue();
            
            _mockBackupCodeRepository.Verify(
                m => m.UpdateAsync(It.Is<Domain.Entities.Authentication.BackupCode>(
                    c => c.UserId == userId && c.Code == backupCode && c.IsUsed == true)),
                Times.Once);
            
            _mockSecurityActivityRepository.Verify(
                m => m.AddAsync(It.Is<Domain.Entities.Authentication.SecurityActivity>(
                    a => a.UserId == userId && a.EventType == "backup_code_used")),
                Times.Once);
        }

        [Fact]
        public async Task ValidateBackupCodeAsync_UsedCode_ReturnsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var backupCode = "123456";
            
            _mockBackupCodeRepository
                .Setup(m => m.GetByUserIdAndCodeAsync(userId, backupCode))
                .ReturnsAsync(new Domain.Entities.Authentication.BackupCode
                {
                    UserId = userId,
                    Code = backupCode,
                    IsUsed = true, // Already used
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                });
            
            // Act
            var result = await _sut.ValidateBackupCodeAsync(userId, backupCode);

            // Assert
            result.Should().BeFalse();
        }
    }
}