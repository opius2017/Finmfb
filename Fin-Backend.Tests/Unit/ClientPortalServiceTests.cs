using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using FinTech.WebAPI.Application.DTOs.ClientPortal;
using FinTech.WebAPI.Application.Interfaces.Repositories;
using FinTech.WebAPI.Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinTech.Tests.Unit
{
    public class ClientPortalServiceTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IAccountRepository> _mockAccountRepository;
        private readonly Mock<ICustomerRepository> _mockCustomerRepository;
        private readonly Mock<ITransactionRepository> _mockTransactionRepository;
        private readonly Mock<ILogger<ClientPortalService>> _mockLogger;
        private readonly ClientPortalService _sut;

        public ClientPortalServiceTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            
            _mockAccountRepository = _fixture.Freeze<Mock<IAccountRepository>>();
            _mockCustomerRepository = _fixture.Freeze<Mock<ICustomerRepository>>();
            _mockTransactionRepository = _fixture.Freeze<Mock<ITransactionRepository>>();
            _mockLogger = _fixture.Freeze<Mock<ILogger<ClientPortalService>>>();
            
            _sut = new ClientPortalService(
                _mockAccountRepository.Object,
                _mockCustomerRepository.Object,
                _mockTransactionRepository.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task GetClientDashboardDataAsync_ValidUserId_ReturnsDashboardData()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var customerId = Guid.NewGuid();
            
            _mockCustomerRepository
                .Setup(m => m.GetCustomerIdByUserIdAsync(userId))
                .ReturnsAsync(customerId);
            
            var accounts = new List<Domain.Entities.Account>
            {
                new Domain.Entities.Account 
                { 
                    Id = Guid.NewGuid(), 
                    CustomerId = customerId, 
                    AccountNumber = "1234567890",
                    AccountType = "Savings",
                    Balance = 5000.00m,
                    Currency = "USD",
                    IsActive = true,
                    DateOpened = DateTime.UtcNow.AddMonths(-6)
                },
                new Domain.Entities.Account 
                { 
                    Id = Guid.NewGuid(), 
                    CustomerId = customerId, 
                    AccountNumber = "0987654321",
                    AccountType = "Current",
                    Balance = 2500.00m,
                    Currency = "USD",
                    IsActive = true,
                    DateOpened = DateTime.UtcNow.AddMonths(-3)
                }
            };
            
            _mockAccountRepository
                .Setup(m => m.GetAccountsByCustomerIdAsync(customerId))
                .ReturnsAsync(accounts);
            
            var transactions = new List<Domain.Entities.Transaction>
            {
                new Domain.Entities.Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = accounts[0].Id,
                    Amount = 500.00m,
                    Type = "Credit",
                    Description = "Salary",
                    TransactionDate = DateTime.UtcNow.AddDays(-1),
                    Status = "Completed"
                },
                new Domain.Entities.Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = accounts[0].Id,
                    Amount = 100.00m,
                    Type = "Debit",
                    Description = "ATM Withdrawal",
                    TransactionDate = DateTime.UtcNow.AddDays(-2),
                    Status = "Completed"
                }
            };
            
            _mockTransactionRepository
                .Setup(m => m.GetRecentTransactionsByAccountIdsAsync(
                    It.IsAny<IEnumerable<Guid>>(), It.IsAny<int>()))
                .ReturnsAsync(transactions);
            
            // Act
            var result = await _sut.GetClientDashboardDataAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.TotalBalance.Should().Be(7500.00m);
            result.AccountsCount.Should().Be(2);
            result.Accounts.Should().HaveCount(2);
            result.RecentTransactions.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAccountDetailsAsync_ValidAccountId_ReturnsAccountDetails()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var customerId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            
            _mockCustomerRepository
                .Setup(m => m.GetCustomerIdByUserIdAsync(userId))
                .ReturnsAsync(customerId);
            
            var account = new Domain.Entities.Account 
            { 
                Id = accountId, 
                CustomerId = customerId, 
                AccountNumber = "1234567890",
                AccountType = "Savings",
                Balance = 5000.00m,
                Currency = "USD",
                IsActive = true,
                DateOpened = DateTime.UtcNow.AddMonths(-6)
            };
            
            _mockAccountRepository
                .Setup(m => m.GetByIdAsync(accountId))
                .ReturnsAsync(account);
            
            _mockAccountRepository
                .Setup(m => m.IsAccountOwnedByCustomerAsync(accountId, customerId))
                .ReturnsAsync(true);
            
            // Act
            var result = await _sut.GetAccountDetailsAsync(accountId, userId);

            // Assert
            result.Should().NotBeNull();
            result.AccountId.Should().Be(accountId);
            result.AccountNumber.Should().Be("1234567890");
            result.AccountType.Should().Be("Savings");
            result.Balance.Should().Be(5000.00m);
            result.Currency.Should().Be("USD");
            result.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetAccountDetailsAsync_AccountNotOwnedByCustomer_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var customerId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            
            _mockCustomerRepository
                .Setup(m => m.GetCustomerIdByUserIdAsync(userId))
                .ReturnsAsync(customerId);
            
            _mockAccountRepository
                .Setup(m => m.IsAccountOwnedByCustomerAsync(accountId, customerId))
                .ReturnsAsync(false);
            
            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _sut.GetAccountDetailsAsync(accountId, userId));
        }

        [Fact]
        public async Task GetAccountTransactionsAsync_ValidParameters_ReturnsTransactions()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var customerId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            
            _mockCustomerRepository
                .Setup(m => m.GetCustomerIdByUserIdAsync(userId))
                .ReturnsAsync(customerId);
            
            _mockAccountRepository
                .Setup(m => m.IsAccountOwnedByCustomerAsync(accountId, customerId))
                .ReturnsAsync(true);
            
            var transactions = new List<Domain.Entities.Transaction>
            {
                new Domain.Entities.Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = accountId,
                    Amount = 500.00m,
                    Type = "Credit",
                    Description = "Salary",
                    TransactionDate = DateTime.UtcNow.AddDays(-1),
                    Status = "Completed"
                },
                new Domain.Entities.Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = accountId,
                    Amount = 100.00m,
                    Type = "Debit",
                    Description = "ATM Withdrawal",
                    TransactionDate = DateTime.UtcNow.AddDays(-2),
                    Status = "Completed"
                }
            };
            
            _mockTransactionRepository
                .Setup(m => m.GetTransactionsByAccountIdAsync(
                    accountId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((transactions, transactions.Count));
            
            // Act
            var result = await _sut.GetAccountTransactionsAsync(
                accountId, 
                DateTime.UtcNow.AddDays(-30), 
                DateTime.UtcNow, 
                1, 
                10, 
                userId);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAccountTransactionsAsync_AccountNotOwnedByCustomer_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var customerId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            
            _mockCustomerRepository
                .Setup(m => m.GetCustomerIdByUserIdAsync(userId))
                .ReturnsAsync(customerId);
            
            _mockAccountRepository
                .Setup(m => m.IsAccountOwnedByCustomerAsync(accountId, customerId))
                .ReturnsAsync(false);
            
            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _sut.GetAccountTransactionsAsync(
                    accountId, 
                    DateTime.UtcNow.AddDays(-30), 
                    DateTime.UtcNow, 
                    1, 
                    10, 
                    userId));
        }
    }
}