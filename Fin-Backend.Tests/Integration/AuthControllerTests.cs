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

namespace FinTech.Tests.Integration
{
    public class AuthControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
    {
        private readonly TestWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AuthControllerTests(TestWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Test@123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            var loginResponse = await response.Content.ReadFromJsonAsync<BaseResponse<LoginResponse>>();
            
            loginResponse.Should().NotBeNull();
            loginResponse.Success.Should().BeTrue();
            loginResponse.Data.Should().NotBeNull();
            loginResponse.Data.Token.Should().NotBeNullOrEmpty();
            loginResponse.Data.UserId.Should().NotBeNullOrEmpty();
            loginResponse.Data.Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "WrongPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
            var errorResponse = await response.Content.ReadFromJsonAsync<BaseResponse<LoginResponse>>();
            
            errorResponse.Should().NotBeNull();
            errorResponse.Success.Should().BeFalse();
            errorResponse.Message.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Register_ValidData_CreatesUser()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = $"test-{Guid.NewGuid()}@example.com",
                Password = "Test@123",
                ConfirmPassword = "Test@123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            var registerResponse = await response.Content.ReadFromJsonAsync<BaseResponse<RegisterResponse>>();
            
            registerResponse.Should().NotBeNull();
            registerResponse.Success.Should().BeTrue();
            registerResponse.Data.Should().NotBeNull();
            registerResponse.Data.UserId.Should().NotBeNullOrEmpty();
            registerResponse.Data.Email.Should().Be(registerRequest.Email);
        }

        [Fact]
        public async Task GetMfaStatus_AuthenticatedUser_ReturnsMfaStatus()
        {
            // Arrange
            var client = await TestAuthHandler.GetAuthenticatedClientAsync(_factory);

            // Act
            var response = await client.GetAsync("/api/Auth/mfa-status");

            // Assert
            response.EnsureSuccessStatusCode();
            var mfaStatus = await response.Content.ReadFromJsonAsync<BaseResponse<MfaStatusResponseDto>>();
            
            mfaStatus.Should().NotBeNull();
            mfaStatus.Success.Should().BeTrue();
            mfaStatus.Data.Should().NotBeNull();
            // Initial MFA setting is disabled based on our test data
            mfaStatus.Data.IsEnabled.Should().BeFalse();
        }

        [Fact]
        public async Task GetMfaStatus_UnauthenticatedUser_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/Auth/mfa-status");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }
    }
}