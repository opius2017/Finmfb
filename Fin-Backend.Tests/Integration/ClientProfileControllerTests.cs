using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Fin_Backend.Tests.Common;
using FinTech.WebAPI.Application.DTOs.ClientPortal;
using FinTech.WebAPI.Application.DTOs.Common;
using FluentAssertions;
using Xunit;

namespace Fin_Backend.Tests.Integration
{
    public class ClientProfileControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
    {
        private readonly TestWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ClientProfileControllerTests(TestWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetProfile_UnauthenticatedUser_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/ClientProfile");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetProfile_AuthenticatedUser_ReturnsProfile()
        {
            // Arrange
            var client = await TestAuthHandler.GetAuthenticatedClientAsync(_factory);

            // Act
            var response = await client.GetAsync("/api/ClientProfile");

            // Assert
            response.EnsureSuccessStatusCode();
            var profileResponse = await response.Content.ReadFromJsonAsync<BaseResponse<ClientProfileDto>>();
            
            profileResponse.Should().NotBeNull();
            profileResponse.Success.Should().BeTrue();
            profileResponse.Data.Should().NotBeNull();
            profileResponse.Data.Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task UpdateProfile_ValidData_UpdatesProfile()
        {
            // Arrange
            var client = await TestAuthHandler.GetAuthenticatedClientAsync(_factory);
            
            // First get current profile
            var currentProfileResponse = await client.GetAsync("/api/ClientProfile");
            currentProfileResponse.EnsureSuccessStatusCode();
            var currentProfile = await currentProfileResponse.Content.ReadFromJsonAsync<BaseResponse<ClientProfileDto>>();
            
            // Update profile with modified data
            var updateProfileRequest = new UpdateProfileRequestDto
            {
                FirstName = "Updated",
                LastName = "User",
                PhoneNumber = currentProfile.Data.PhoneNumber,
                Address = new AddressDto
                {
                    AddressLine1 = "123 Updated St",
                    City = "New City",
                    State = "NS",
                    ZipCode = "12345",
                    Country = "USA",
                    AddressType = "Residential"
                }
            };

            // Act
            var response = await client.PutAsJsonAsync("/api/ClientProfile", updateProfileRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            var updateResponse = await response.Content.ReadFromJsonAsync<BaseResponse<bool>>();
            
            updateResponse.Should().NotBeNull();
            updateResponse.Success.Should().BeTrue();
            updateResponse.Data.Should().BeTrue();
            
            // Verify profile was updated
            var updatedProfileResponse = await client.GetAsync("/api/ClientProfile");
            updatedProfileResponse.EnsureSuccessStatusCode();
            var updatedProfile = await updatedProfileResponse.Content.ReadFromJsonAsync<BaseResponse<ClientProfileDto>>();
            
            updatedProfile.Data.FirstName.Should().Be("Updated");
            updatedProfile.Data.LastName.Should().Be("User");
            updatedProfile.Data.Address.AddressLine1.Should().Be("123 Updated St");
            updatedProfile.Data.Address.City.Should().Be("New City");
        }

        [Fact]
        public async Task ChangePassword_InvalidCurrentPassword_ReturnsBadRequest()
        {
            // Arrange
            var client = await TestAuthHandler.GetAuthenticatedClientAsync(_factory);
            
            var changePasswordRequest = new ChangePasswordRequestDto
            {
                CurrentPassword = "WrongPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/ClientProfile/change-password", changePasswordRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var errorResponse = await response.Content.ReadFromJsonAsync<BaseResponse<bool>>();
            
            errorResponse.Should().NotBeNull();
            errorResponse.Success.Should().BeFalse();
            errorResponse.Data.Should().BeFalse();
            errorResponse.Message.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ChangePassword_PasswordMismatch_ReturnsBadRequest()
        {
            // Arrange
            var client = await TestAuthHandler.GetAuthenticatedClientAsync(_factory);
            
            var changePasswordRequest = new ChangePasswordRequestDto
            {
                CurrentPassword = "Test@123",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "DifferentPassword123!"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/ClientProfile/change-password", changePasswordRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var errorResponse = await response.Content.ReadFromJsonAsync<BaseResponse<bool>>();
            
            errorResponse.Should().NotBeNull();
            errorResponse.Success.Should().BeFalse();
            errorResponse.Message.Should().Contain("match");
        }

        [Fact]
        public async Task GetSecurityActivity_AuthenticatedUser_ReturnsActivityList()
        {
            // Arrange
            var client = await TestAuthHandler.GetAuthenticatedClientAsync(_factory);

            // Act
            var response = await client.GetAsync("/api/ClientProfile/security-activity");

            // Assert
            response.EnsureSuccessStatusCode();
            var activityResponse = await response.Content.ReadFromJsonAsync<BaseResponse<PagedResponse<SecurityActivityDto>>>();
            
            activityResponse.Should().NotBeNull();
            activityResponse.Success.Should().BeTrue();
            activityResponse.Data.Should().NotBeNull();
            activityResponse.Data.Items.Should().NotBeNull();
            // Database initializer seeds at least 3 security activities
            activityResponse.Data.Items.Count.Should().BeGreaterOrEqualTo(3);
        }
    }
}