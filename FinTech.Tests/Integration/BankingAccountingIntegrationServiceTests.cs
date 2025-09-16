using FinTech.Application.Services;
using FinTech.Application.Services.Integration;
using FinTech.Infrastructure.Services.Integration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FinTech.Tests.Integration
{
    public class BankingAccountingIntegrationServiceTests : IntegrationServiceTestBase
    {
        private readonly BankingAccountingIntegrationService _service;
        private readonly Mock<ILogger<BankingAccountingIntegrationService>> _mockLogger;

        public BankingAccountingIntegrationServiceTests()
        {
            _mockLogger = new Mock<ILogger<BankingAccountingIntegrationService>>();
            _service = new BankingAccountingIntegrationService(
                _mockLogger.Object,
                MockJournalEntryService.Object,
                MockChartOfAccountService.Object);
        }

        [Fact]
        public async Task ProcessDepositAsync_ShouldCreateBalancedJournalEntry()
        {
            // Arrange
            int accountId = 123;
            decimal amount = 1000;
            string reference = "DEP123456";
            string description = "Customer deposit";
            
            JournalEntryDto capturedJournalEntry = null;
            
            MockJournalEntryService
                .Setup(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()))
                .Callback<JournalEntryDto>(je => capturedJournalEntry = je)
                .ReturnsAsync(new JournalEntryDto { Id = 1 });
                
            // Act
            await _service.ProcessDepositAsync(accountId, amount, reference, description);
            
            // Assert
            MockJournalEntryService.Verify(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()), Times.Once);
            
            Assert.NotNull(capturedJournalEntry);
            Assert.Equal("Banking", capturedJournalEntry.Source);
            Assert.Equal(reference, capturedJournalEntry.Reference);
            Assert.Equal(description, capturedJournalEntry.Description);
            
            // Verify lines
            Assert.Equal(2, capturedJournalEntry.Lines.Count);
            
            // Verify that the entry is balanced
            var totalDebits = capturedJournalEntry.Lines.Sum(l => l.DebitAmount);
            var totalCredits = capturedJournalEntry.Lines.Sum(l => l.CreditAmount);
            Assert.Equal(totalDebits, totalCredits);
            
            // Verify that the bank account is debited
            var bankLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 2001);
            Assert.NotNull(bankLine);
            Assert.Equal(amount, bankLine.DebitAmount);
            Assert.Equal(0, bankLine.CreditAmount);
            
            // Verify that the cash account is credited
            var cashLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 1001);
            Assert.NotNull(cashLine);
            Assert.Equal(0, cashLine.DebitAmount);
            Assert.Equal(amount, cashLine.CreditAmount);
        }

        [Fact]
        public async Task ProcessWithdrawalAsync_ShouldCreateBalancedJournalEntry()
        {
            // Arrange
            int accountId = 123;
            decimal amount = 500;
            string reference = "WTH123456";
            string description = "Customer withdrawal";
            
            JournalEntryDto capturedJournalEntry = null;
            
            MockJournalEntryService
                .Setup(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()))
                .Callback<JournalEntryDto>(je => capturedJournalEntry = je)
                .ReturnsAsync(new JournalEntryDto { Id = 1 });
                
            // Act
            await _service.ProcessWithdrawalAsync(accountId, amount, reference, description);
            
            // Assert
            MockJournalEntryService.Verify(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()), Times.Once);
            
            Assert.NotNull(capturedJournalEntry);
            Assert.Equal("Banking", capturedJournalEntry.Source);
            Assert.Equal(reference, capturedJournalEntry.Reference);
            Assert.Equal(description, capturedJournalEntry.Description);
            
            // Verify lines
            Assert.Equal(2, capturedJournalEntry.Lines.Count);
            
            // Verify that the entry is balanced
            var totalDebits = capturedJournalEntry.Lines.Sum(l => l.DebitAmount);
            var totalCredits = capturedJournalEntry.Lines.Sum(l => l.CreditAmount);
            Assert.Equal(totalDebits, totalCredits);
            
            // Verify that the cash account is debited
            var cashLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 1001);
            Assert.NotNull(cashLine);
            Assert.Equal(amount, cashLine.DebitAmount);
            Assert.Equal(0, cashLine.CreditAmount);
            
            // Verify that the bank account is credited
            var bankLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 2001);
            Assert.NotNull(bankLine);
            Assert.Equal(0, bankLine.DebitAmount);
            Assert.Equal(amount, bankLine.CreditAmount);
        }

        [Fact]
        public async Task ProcessTransferAsync_ShouldCreateBalancedJournalEntry()
        {
            // Arrange
            int fromAccountId = 123;
            int toAccountId = 456;
            decimal amount = 750;
            string reference = "TRF123456";
            string description = "Transfer between accounts";
            
            JournalEntryDto capturedJournalEntry = null;
            
            MockJournalEntryService
                .Setup(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()))
                .Callback<JournalEntryDto>(je => capturedJournalEntry = je)
                .ReturnsAsync(new JournalEntryDto { Id = 1 });
                
            // Set up different account IDs for from and to accounts
            MockChartOfAccountService.Setup(s => s.GetBankAccountIdForCustomerAsync(fromAccountId))
                .ReturnsAsync(2001);
                
            MockChartOfAccountService.Setup(s => s.GetBankAccountIdForCustomerAsync(toAccountId))
                .ReturnsAsync(2002);
                
            // Act
            await _service.ProcessTransferAsync(fromAccountId, toAccountId, amount, reference, description);
            
            // Assert
            MockJournalEntryService.Verify(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()), Times.Once);
            
            Assert.NotNull(capturedJournalEntry);
            Assert.Equal("Banking", capturedJournalEntry.Source);
            Assert.Equal(reference, capturedJournalEntry.Reference);
            Assert.Equal(description, capturedJournalEntry.Description);
            
            // Verify lines
            Assert.Equal(2, capturedJournalEntry.Lines.Count);
            
            // Verify that the entry is balanced
            var totalDebits = capturedJournalEntry.Lines.Sum(l => l.DebitAmount);
            var totalCredits = capturedJournalEntry.Lines.Sum(l => l.CreditAmount);
            Assert.Equal(totalDebits, totalCredits);
            
            // Verify that the to account is debited
            var toLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 2002);
            Assert.NotNull(toLine);
            Assert.Equal(amount, toLine.DebitAmount);
            Assert.Equal(0, toLine.CreditAmount);
            
            // Verify that the from account is credited
            var fromLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 2001);
            Assert.NotNull(fromLine);
            Assert.Equal(0, fromLine.DebitAmount);
            Assert.Equal(amount, fromLine.CreditAmount);
        }

        [Fact]
        public async Task ProcessFeeChargeAsync_ShouldCreateBalancedJournalEntry()
        {
            // Arrange
            int accountId = 123;
            decimal amount = 25;
            string feeType = "Monthly";
            string reference = "FEE123456";
            string description = "Monthly maintenance fee";
            
            JournalEntryDto capturedJournalEntry = null;
            
            MockJournalEntryService
                .Setup(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()))
                .Callback<JournalEntryDto>(je => capturedJournalEntry = je)
                .ReturnsAsync(new JournalEntryDto { Id = 1 });
                
            // Act
            await _service.ProcessFeeChargeAsync(accountId, amount, feeType, reference, description);
            
            // Assert
            MockJournalEntryService.Verify(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()), Times.Once);
            
            Assert.NotNull(capturedJournalEntry);
            Assert.Equal("Banking", capturedJournalEntry.Source);
            Assert.Equal(reference, capturedJournalEntry.Reference);
            Assert.Equal(description, capturedJournalEntry.Description);
            
            // Verify lines
            Assert.Equal(2, capturedJournalEntry.Lines.Count);
            
            // Verify that the entry is balanced
            var totalDebits = capturedJournalEntry.Lines.Sum(l => l.DebitAmount);
            var totalCredits = capturedJournalEntry.Lines.Sum(l => l.CreditAmount);
            Assert.Equal(totalDebits, totalCredits);
            
            // Verify that the bank account is debited
            var bankLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 2001);
            Assert.NotNull(bankLine);
            Assert.Equal(amount, bankLine.DebitAmount);
            Assert.Equal(0, bankLine.CreditAmount);
            
            // Verify that the fee income account is credited
            var feeLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 3001);
            Assert.NotNull(feeLine);
            Assert.Equal(0, feeLine.DebitAmount);
            Assert.Equal(amount, feeLine.CreditAmount);
        }
    }
}