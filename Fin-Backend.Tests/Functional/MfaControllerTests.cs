using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FinTech.Tests.Common;
using FinTech.WebAPI.Application.DTOs.Auth;
using FinTech.WebAPI.Application.DTOs.Common;
using FluentAssertions;
using Xunit;

namespace FinTech.Tests.Functional
{
    public class MfaControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
    {
        private readonly TestWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public MfaControllerTests(TestWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Setup_AuthenticatedUser_GeneratesMfaSetup()
        {
            // Arrange
            var client = await TestAuthHandler.GetAuthenticatedClientAsync(_factory);

            // Act
            var response = await client.GetAsync("/api/Mfa/setup");

            // Assert
            response.EnsureSuccessStatusCode();
            var setupResponse = await response.Content.ReadFromJsonAsync<BaseResponse<MfaSetupResponseDto>>();
            
            setupResponse.Should().NotBeNull();
            setupResponse.Success.Should().BeTrue();
            setupResponse.Data.Should().NotBeNull();
            setupResponse.Data.Secret.Should().NotBeNullOrEmpty();
            setupResponse.Data.QrCodeUrl.Should().NotBeNullOrEmpty();
            setupResponse.Data.ManualEntryKey.Should().NotBeNullOrEmpty();
            setupResponse.Data.BackupCodes.Should().NotBeNull();
            setupResponse.Data.BackupCodes.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public async Task Setup_UnauthenticatedUser_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Mfa/setup");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task VerifySetup_InvalidCode_ReturnsBadRequest()
        {
            // Arrange
            var client = await TestAuthHandler.GetAuthenticatedClientAsync(_factory);
            
            // First get MFA setup
            var setupResponse = await client.GetAsync("/api/Mfa/setup");
            setupResponse.EnsureSuccessStatusCode();
            var setup = await setupResponse.Content.ReadFromJsonAsync<BaseResponse<MfaSetupResponseDto>>();
            
            var verifyRequest = new MfaVerifyRequestDto
            {
                Code = "000000", // Invalid code
                Secret = setup.Data.Secret
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/Mfa/verify-setup", verifyRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            var verifyResponse = await response.Content.ReadFromJsonAsync<BaseResponse<bool>>();
            
            verifyResponse.Should().NotBeNull();
            verifyResponse.Success.Should().BeFalse();
            verifyResponse.Data.Should().BeFalse();
        }

        [Fact]
        public async Task GetSecurityActivity_AuthenticatedUser_ReturnsActivityHistory()
        {
            // Arrange
            var client = await TestAuthHandler.GetAuthenticatedClientAsync(_factory);

            // Act
            var response = await client.GetAsync("/api/Mfa/security-activity");

            // Assert
            response.EnsureSuccessStatusCode();
            var activityResponse = await response.Content.ReadFromJsonAsync<BaseResponse<List<SecurityActivityDto>>>();
            
            activityResponse.Should().NotBeNull();
            activityResponse.Success.Should().BeTrue();
            activityResponse.Data.Should().NotBeNull();
            // We seeded 3 security activities in the test database
            activityResponse.Data.Should().HaveCountGreaterOrEqualTo(3);
        }

        [Fact]
        public async Task GetSecurityPreferences_AuthenticatedUser_ReturnsPreferences()
        {
            // Arrange
            var client = await TestAuthHandler.GetAuthenticatedClientAsync(_factory);

            // Act
            var response = await client.GetAsync("/api/Mfa/security-preferences");

            // Assert
            response.EnsureSuccessStatusCode();
            var preferencesResponse = await response.Content.ReadFromJsonAsync<BaseResponse<SecurityPreferencesDto>>();
            
            preferencesResponse.Should().NotBeNull();
            preferencesResponse.Success.Should().BeTrue();
            preferencesResponse.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateSecurityPreferences_ValidPreferences_UpdatesPreferences()
        {
            // Arrange
            var client = await TestAuthHandler.GetAuthenticatedClientAsync(_factory);
            
            // First get current preferences
            var currentPreferencesResponse = await client.GetAsync("/api/Mfa/security-preferences");
            currentPreferencesResponse.EnsureSuccessStatusCode();
            var currentPreferences = await currentPreferencesResponse.Content.ReadFromJsonAsync<BaseResponse<SecurityPreferencesDto>>();
            
            // Create update request with modified preferences
            var updatedPreferences = currentPreferences.Data;
            updatedPreferences.NotifyOnLogin = !updatedPreferences.NotifyOnLogin;
            updatedPreferences.EnableSuspiciousActivityAlerts = !updatedPreferences.EnableSuspiciousActivityAlerts;

            // Act
            var response = await client.PutAsJsonAsync("/api/Mfa/security-preferences", updatedPreferences);

            // Assert
            response.EnsureSuccessStatusCode();
            var updateResponse = await response.Content.ReadFromJsonAsync<BaseResponse<bool>>();
            
            updateResponse.Should().NotBeNull();
            updateResponse.Success.Should().BeTrue();
            updateResponse.Data.Should().BeTrue();
            
            // Verify preferences were updated
            var newPreferencesResponse = await client.GetAsync("/api/Mfa/security-preferences");
            newPreferencesResponse.EnsureSuccessStatusCode();
            var newPreferences = await newPreferencesResponse.Content.ReadFromJsonAsync<BaseResponse<SecurityPreferencesDto>>();
            
            newPreferences.Data.NotifyOnLogin.Should().Be(!currentPreferences.Data.NotifyOnLogin);
            newPreferences.Data.EnableSuspiciousActivityAlerts.Should().Be(!currentPreferences.Data.EnableSuspiciousActivityAlerts);
        }
    }
}