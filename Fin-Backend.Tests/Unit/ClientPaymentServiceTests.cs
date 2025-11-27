using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using FinTech.WebAPI.Application.DTOs.ClientPortal;
using FinTech.WebAPI.Application.Interfaces.Repositories;
using FinTech.WebAPI.Application.Interfaces.Services;
using FinTech.WebAPI.Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinTech.Tests.Unit
{
    public class ClientPaymentServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IAccountRepository> _mockAccountRepository;
        private readonly Mock<ICustomerRepository> _mockCustomerRepository;
        private readonly Mock<ITransactionRepository> _mockTransactionRepository;
        private readonly Mock<IBeneficiaryRepository> _mockBeneficiaryRepository;
        private readonly Mock<IPaymentGatewayService> _mockPaymentGatewayService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<ILogger<ClientPaymentService>> _mockLogger;
        private readonly ClientPaymentService _sut;

        public ClientPaymentServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            
            _mockAccountRepository = _fixture.Freeze<Mock<IAccountRepository>>();
            _mockCustomerRepository = _fixture.Freeze<Mock<ICustomerRepository>>();
            _mockTransactionRepository = _fixture.Freeze<Mock<ITransactionRepository>>();
            _mockBeneficiaryRepository = _fixture.Freeze<Mock<IBeneficiaryRepository>>();
            _mockPaymentGatewayService = _fixture.Freeze<Mock<IPaymentGatewayService>>();
            _mockNotificationService = _fixture.Freeze<Mock<INotificationService>>();
            _mockLogger = _fixture.Freeze<Mock<ILogger<ClientPaymentService>>>();
            
            _sut = new ClientPaymentService(
                _mockAccountRepository.Object,
                _mockCustomerRepository.Object,
                _mockTransactionRepository.Object,
                _mockBeneficiaryRepository.Object,
                _mockPaymentGatewayService.Object,
                _mockNotificationService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task GetBeneficiariesAsync_ValidUserId_ReturnsBeneficiaries()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var customerId = Guid.NewGuid();
            
            _mockCustomerRepository
                .Setup(m => m.GetCustomerIdByUserIdAsync(userId))
                .ReturnsAsync(customerId);
            
            var beneficiaries = new[]
            {
                new Domain.Entities.Beneficiary
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    Name = "John Doe",
                    AccountNumber = "1234567890",
                    BankName = "Test Bank",
                    BankCode = "TB",
                    BeneficiaryType = "Individual",
                    IsVerified = true
                },
                new Domain.Entities.Beneficiary
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    Name = "Jane Smith",
                    AccountNumber = "0987654321",
                    BankName = "Another Bank",
                    BankCode = "AB",
                    BeneficiaryType = "Individual",
                    IsVerified = true
                }
            };
            
            _mockBeneficiaryRepository
                .Setup(m => m.GetBeneficiariesByCustomerIdAsync(customerId))
                .ReturnsAsync(beneficiaries);
            
            // Act
            var result = await _sut.GetBeneficiariesAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Name.Should().Be("John Doe");
            result[1].Name.Should().Be("Jane Smith");
        }

        [Fact]
        public async Task AddBeneficiaryAsync_ValidData_AddsBeneficiary()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var customerId = Guid.NewGuid();
            
            _mockCustomerRepository
                .Setup(m => m.GetCustomerIdByUserIdAsync(userId))
                .ReturnsAsync(customerId);
            
            var request = new BeneficiaryRequestDto
            {
                Name = "John Doe",
                AccountNumber = "1234567890",
                BankName = "Test Bank",
                BankCode = "TB",
                BeneficiaryType = "Individual"
            };
            
            // Act
            var result = await _sut.AddBeneficiaryAsync(request, userId);

            // Assert
            result.Should().BeTrue();
            
            _mockBeneficiaryRepository.Verify(
                m => m.AddAsync(It.Is<Domain.Entities.Beneficiary>(
                    b => b.CustomerId == customerId && 
                         b.Name == "John Doe" && 
                         b.AccountNumber == "1234567890")),
                Times.Once);
        }

        [Fact]
        public async Task MakeTransferAsync_ValidTransfer_ProcessesTransfer()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var customerId = Guid.NewGuid();
            var sourceAccountId = Guid.NewGuid();
            var beneficiaryId = Guid.NewGuid();
            
            _mockCustomerRepository
                .Setup(m => m.GetCustomerIdByUserIdAsync(userId))
                .ReturnsAsync(customerId);
            
            var sourceAccount = new Domain.Entities.Account
            {
                Id = sourceAccountId,
                CustomerId = customerId,
                AccountNumber = "1234567890",
                AccountType = "Savings",
                Balance = 5000.00m,
                Currency = "USD",
                IsActive = true
            };
            
            _mockAccountRepository
                .Setup(m => m.GetByIdAsync(sourceAccountId))
                .ReturnsAsync(sourceAccount);
            
            _mockAccountRepository
                .Setup(m => m.IsAccountOwnedByCustomerAsync(sourceAccountId, customerId))
                .ReturnsAsync(true);
            
            var beneficiary = new Domain.Entities.Beneficiary
            {
                Id = beneficiaryId,
                CustomerId = customerId,
                Name = "John Doe",
                AccountNumber = "0987654321",
                BankName = "Test Bank",
                BankCode = "TB",
                BeneficiaryType = "Individual",
                IsVerified = true
            };
            
            _mockBeneficiaryRepository
                .Setup(m => m.GetByIdAsync(beneficiaryId))
                .ReturnsAsync(beneficiary);
            
            _mockBeneficiaryRepository
                .Setup(m => m.IsBeneficiaryOwnedByCustomerAsync(beneficiaryId, customerId))
                .ReturnsAsync(true);
            
            var paymentReference = Guid.NewGuid().ToString();
            
            _mockPaymentGatewayService
                .Setup(m => m.ProcessTransferAsync(It.IsAny<TransferRequestDto>()))
                .ReturnsAsync(new PaymentResponseDto
                {
                    IsSuccessful = true,
                    Reference = paymentReference,
                    Message = "Transfer successful"
                });
            
            var transferRequest = new TransferRequestDto
            {
                SourceAccountId = sourceAccountId,
                BeneficiaryId = beneficiaryId,
                Amount = 1000.00m,
                Narration = "Test transfer"
            };
            
            // Act
            var result = await _sut.MakeTransferAsync(transferRequest, userId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeTrue();
            result.Reference.Should().Be(paymentReference);
            
            _mockAccountRepository.Verify(
                m => m.UpdateBalanceAsync(sourceAccountId, 4000.00m), // 5000 - 1000
                Times.Once);
            
            _mockTransactionRepository.Verify(
                m => m.AddAsync(It.Is<Domain.Entities.Transaction>(
                    t => t.AccountId == sourceAccountId &&
                         t.Amount == 1000.00m &&
                         t.Type == "Debit" &&
                         t.Status == "Completed")),
                Times.Once);
            
            _mockNotificationService.Verify(
                m => m.SendTransactionAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task MakeTransferAsync_InsufficientBalance_ReturnsFailure()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var customerId = Guid.NewGuid();
            var sourceAccountId = Guid.NewGuid();
            var beneficiaryId = Guid.NewGuid();
            
            _mockCustomerRepository
                .Setup(m => m.GetCustomerIdByUserIdAsync(userId))
                .ReturnsAsync(customerId);
            
            var sourceAccount = new Domain.Entities.Account
            {
                Id = sourceAccountId,
                CustomerId = customerId,
                AccountNumber = "1234567890",
                AccountType = "Savings",
                Balance = 500.00m, // Less than transfer amount
                Currency = "USD",
                IsActive = true
            };
            
            _mockAccountRepository
                .Setup(m => m.GetByIdAsync(sourceAccountId))
                .ReturnsAsync(sourceAccount);
            
            _mockAccountRepository
                .Setup(m => m.IsAccountOwnedByCustomerAsync(sourceAccountId, customerId))
                .ReturnsAsync(true);
            
            var beneficiary = new Domain.Entities.Beneficiary
            {
                Id = beneficiaryId,
                CustomerId = customerId,
                Name = "John Doe",
                AccountNumber = "0987654321",
                BankName = "Test Bank",
                BankCode = "TB",
                BeneficiaryType = "Individual",
                IsVerified = true
            };
            
            _mockBeneficiaryRepository
                .Setup(m => m.GetByIdAsync(beneficiaryId))
                .ReturnsAsync(beneficiary);
            
            _mockBeneficiaryRepository
                .Setup(m => m.IsBeneficiaryOwnedByCustomerAsync(beneficiaryId, customerId))
                .ReturnsAsync(true);
            
            var transferRequest = new TransferRequestDto
            {
                SourceAccountId = sourceAccountId,
                BeneficiaryId = beneficiaryId,
                Amount = 1000.00m, // More than available balance
                Narration = "Test transfer"
            };
            
            // Act
            var result = await _sut.MakeTransferAsync(transferRequest, userId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeFalse();
            result.Message.Should().Contain("insufficient");
            
            _mockAccountRepository.Verify(
                m => m.UpdateBalanceAsync(It.IsAny<Guid>(), It.IsAny<decimal>()),
                Times.Never);
            
            _mockTransactionRepository.Verify(
                m => m.AddAsync(It.IsAny<Domain.Entities.Transaction>()),
                Times.Never);
            
            _mockPaymentGatewayService.Verify(
                m => m.ProcessTransferAsync(It.IsAny<TransferRequestDto>()),
                Times.Never);
        }

        [Fact]
        public async Task MakeTransferAsync_AccountNotOwnedByCustomer_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var customerId = Guid.NewGuid();
            var sourceAccountId = Guid.NewGuid();
            var beneficiaryId = Guid.NewGuid();
            
            _mockCustomerRepository
                .Setup(m => m.GetCustomerIdByUserIdAsync(userId))
                .ReturnsAsync(customerId);
            
            _mockAccountRepository
                .Setup(m => m.IsAccountOwnedByCustomerAsync(sourceAccountId, customerId))
                .ReturnsAsync(false); // Account not owned by customer
            
            var transferRequest = new TransferRequestDto
            {
                SourceAccountId = sourceAccountId,
                BeneficiaryId = beneficiaryId,
                Amount = 1000.00m,
                Narration = "Test transfer"
            };
            
            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _sut.MakeTransferAsync(transferRequest, userId));
        }
    }
}