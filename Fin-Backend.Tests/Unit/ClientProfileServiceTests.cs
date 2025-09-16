using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using FinTech.WebAPI.Application.DTOs.ClientPortal;
using FinTech.WebAPI.Application.Interfaces.Repositories;
using FinTech.WebAPI.Application.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Fin_Backend.Tests.Unit
{
    public class ClientProfileServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<ICustomerRepository> _mockCustomerRepository;
        private readonly Mock<IAddressRepository> _mockAddressRepository;
        private readonly Mock<ISecurityActivityRepository> _mockSecurityActivityRepository;
        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
        private readonly Mock<ILogger<ClientProfileService>> _mockLogger;
        private readonly ClientProfileService _sut;

        public ClientProfileServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            
            _mockCustomerRepository = _fixture.Freeze<Mock<ICustomerRepository>>();
            _mockAddressRepository = _fixture.Freeze<Mock<IAddressRepository>>();
            _mockSecurityActivityRepository = _fixture.Freeze<Mock<ISecurityActivityRepository>>();
            
            // Setup UserManager mock
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            
            _mockLogger = _fixture.Freeze<Mock<ILogger<ClientProfileService>>>();
            
            _sut = new ClientProfileService(
                _mockCustomerRepository.Object,
                _mockAddressRepository.Object,
                _mockSecurityActivityRepository.Object,
                _mockUserManager.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task GetProfileAsync_ValidUserId_ReturnsClientProfile()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var customerId = Guid.NewGuid();
            
            var user = new IdentityUser
            {
                Id = userId,
                Email = "test@example.com",
                PhoneNumber = "1234567890"
            };
            
            _mockUserManager
                .Setup(m => m.FindByIdAsync(userId))
                .ReturnsAsync(user);
            
            _mockCustomerRepository
                .Setup(m => m.GetCustomerIdByUserIdAsync(userId))
                .ReturnsAsync(customerId);
            
            var customer = new Domain.Entities.Customer
            {
                Id = customerId,
                UserId = userId,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1980, 1, 1),
                Gender = "Male",
                IdentificationType = "National ID",
                IdentificationNumber = "123456789",
                CustomerType = "Individual",
                CreatedAt = DateTime.UtcNow.AddYears(-1),
                UpdatedAt = DateTime.UtcNow.AddMonths(-1)
            };
            
            _mockCustomerRepository
                .Setup(m => m.GetByIdAsync(customerId))
                .ReturnsAsync(customer);
            
            var address = new Domain.Entities.Address
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                AddressLine1 = "123 Main St",
                AddressLine2 = "Apt 4B",
                City = "New York",
                State = "NY",
                ZipCode = "10001",
                Country = "USA",
                IsDefault = true,
                AddressType = "Residential"
            };
            
            _mockAddressRepository
                .Setup(m => m.GetDefaultAddressByCustomerIdAsync(customerId))
                .ReturnsAsync(address);
            
            // Act
            var result = await _sut.GetProfileAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
            result.Email.Should().Be("test@example.com");
            result.PhoneNumber.Should().Be("1234567890");
            result.IdentificationType.Should().Be("National ID");
            result.IdentificationNumber.Should().Be("123456789");
            
            result.Address.Should().NotBeNull();
            result.Address.AddressLine1.Should().Be("123 Main St");
            result.Address.City.Should().Be("New York");
            result.Address.Country.Should().Be("USA");
        }

        [Fact]
        public async Task UpdateProfileAsync_ValidData_UpdatesProfile()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var customerId = Guid.NewGuid();
            
            var user = new IdentityUser
            {
                Id = userId,
                Email = "test@example.com",
                PhoneNumber = "1234567890"
            };
            
            _mockUserManager
                .Setup(m => m.FindByIdAsync(userId))
                .ReturnsAsync(user);
            
            _mockCustomerRepository
                .Setup(m => m.GetCustomerIdByUserIdAsync(userId))
                .ReturnsAsync(customerId);
            
            var customer = new Domain.Entities.Customer
            {
                Id = customerId,
                UserId = userId,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1980, 1, 1),
                Gender = "Male",
                IdentificationType = "National ID",
                IdentificationNumber = "123456789",
                CustomerType = "Individual",
                CreatedAt = DateTime.UtcNow.AddYears(-1),
                UpdatedAt = DateTime.UtcNow.AddMonths(-1)
            };
            
            _mockCustomerRepository
                .Setup(m => m.GetByIdAsync(customerId))
                .ReturnsAsync(customer);
            
            var updateRequest = new UpdateProfileRequestDto
            {
                FirstName = "Jane",
                LastName = "Doe",
                PhoneNumber = "9876543210",
                Address = new AddressDto
                {
                    AddressLine1 = "456 Park Ave",
                    AddressLine2 = "Suite 10",
                    City = "Boston",
                    State = "MA",
                    ZipCode = "02108",
                    Country = "USA",
                    AddressType = "Residential"
                }
            };
            
            _mockUserManager
                .Setup(m => m.UpdateAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync(IdentityResult.Success);
            
            _mockCustomerRepository
                .Setup(m => m.UpdateAsync(It.IsAny<Domain.Entities.Customer>()))
                .Returns(Task.CompletedTask);
            
            var existingAddress = new Domain.Entities.Address
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                AddressLine1 = "123 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "10001",
                Country = "USA",
                IsDefault = true,
                AddressType = "Residential"
            };
            
            _mockAddressRepository
                .Setup(m => m.GetDefaultAddressByCustomerIdAsync(customerId))
                .ReturnsAsync(existingAddress);
            
            _mockAddressRepository
                .Setup(m => m.UpdateAsync(It.IsAny<Domain.Entities.Address>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var result = await _sut.UpdateProfileAsync(updateRequest, userId);

            // Assert
            result.Should().BeTrue();
            
            _mockUserManager.Verify(
                m => m.UpdateAsync(It.Is<IdentityUser>(
                    u => u.Id == userId && u.PhoneNumber == "9876543210")),
                Times.Once);
            
            _mockCustomerRepository.Verify(
                m => m.UpdateAsync(It.Is<Domain.Entities.Customer>(
                    c => c.Id == customerId && 
                         c.FirstName == "Jane" &&
                         c.LastName == "Doe")),
                Times.Once);
            
            _mockAddressRepository.Verify(
                m => m.UpdateAsync(It.Is<Domain.Entities.Address>(
                    a => a.Id == existingAddress.Id && 
                         a.AddressLine1 == "456 Park Ave" &&
                         a.City == "Boston")),
                Times.Once);
            
            _mockSecurityActivityRepository.Verify(
                m => m.AddAsync(It.Is<Domain.Entities.Authentication.SecurityActivity>(
                    a => a.UserId == userId && a.EventType == "profile_updated")),
                Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_ValidData_ChangesPassword()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new IdentityUser { Id = userId };
            
            _mockUserManager
                .Setup(m => m.FindByIdAsync(userId))
                .ReturnsAsync(user);
            
            _mockUserManager
                .Setup(m => m.ChangePasswordAsync(user, "OldPassword123!", "NewPassword123!"))
                .ReturnsAsync(IdentityResult.Success);
            
            var changePasswordRequest = new ChangePasswordRequestDto
            {
                CurrentPassword = "OldPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };
            
            // Act
            var result = await _sut.ChangePasswordAsync(changePasswordRequest, userId);

            // Assert
            result.Should().BeTrue();
            
            _mockSecurityActivityRepository.Verify(
                m => m.AddAsync(It.Is<Domain.Entities.Authentication.SecurityActivity>(
                    a => a.UserId == userId && a.EventType == "password_changed")),
                Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_InvalidCurrentPassword_ReturnsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new IdentityUser { Id = userId };
            
            _mockUserManager
                .Setup(m => m.FindByIdAsync(userId))
                .ReturnsAsync(user);
            
            _mockUserManager
                .Setup(m => m.ChangePasswordAsync(user, "WrongPassword123!", "NewPassword123!"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError 
                { 
                    Code = "PasswordMismatch", 
                    Description = "Incorrect password." 
                }));
            
            var changePasswordRequest = new ChangePasswordRequestDto
            {
                CurrentPassword = "WrongPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };
            
            // Act
            var result = await _sut.ChangePasswordAsync(changePasswordRequest, userId);

            // Assert
            result.Should().BeFalse();
            
            _mockSecurityActivityRepository.Verify(
                m => m.AddAsync(It.IsAny<Domain.Entities.Authentication.SecurityActivity>()),
                Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_PasswordsDontMatch_ThrowsArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            
            var changePasswordRequest = new ChangePasswordRequestDto
            {
                CurrentPassword = "OldPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "DifferentPassword123!"
            };
            
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.ChangePasswordAsync(changePasswordRequest, userId));
        }
    }
}