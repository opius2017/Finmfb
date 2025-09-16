using FinTech.Application.Services;
using FinTech.Application.Services.Integration;
using FinTech.Infrastructure.Services.Integration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FinTech.Tests.Integration
{
    public class LoanAccountingIntegrationServiceTests : IntegrationServiceTestBase
    {
        private readonly LoanAccountingIntegrationService _service;
        private readonly Mock<ILogger<LoanAccountingIntegrationService>> _mockLogger;

        public LoanAccountingIntegrationServiceTests()
        {
            _mockLogger = new Mock<ILogger<LoanAccountingIntegrationService>>();
            _service = new LoanAccountingIntegrationService(
                _mockLogger.Object,
                MockJournalEntryService.Object,
                MockChartOfAccountService.Object);
        }

        [Fact]
        public async Task ProcessLoanDisbursementAsync_ShouldCreateBalancedJournalEntry()
        {
            // Arrange
            int loanId = 789;
            decimal amount = 10000;
            string reference = "LOAN789";
            string description = "Loan disbursement";
            
            JournalEntryDto capturedJournalEntry = null;
            
            MockJournalEntryService
                .Setup(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()))
                .Callback<JournalEntryDto>(je => capturedJournalEntry = je)
                .ReturnsAsync(new JournalEntryDto { Id = 1 });
                
            // Act
            await _service.ProcessLoanDisbursementAsync(loanId, amount, reference, description);
            
            // Assert
            MockJournalEntryService.Verify(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()), Times.Once);
            
            Assert.NotNull(capturedJournalEntry);
            Assert.Equal("Loans", capturedJournalEntry.Source);
            Assert.Equal(reference, capturedJournalEntry.Reference);
            Assert.Equal(description, capturedJournalEntry.Description);
            
            // Verify lines
            Assert.Equal(2, capturedJournalEntry.Lines.Count);
            
            // Verify that the entry is balanced
            var totalDebits = capturedJournalEntry.Lines.Sum(l => l.DebitAmount);
            var totalCredits = capturedJournalEntry.Lines.Sum(l => l.CreditAmount);
            Assert.Equal(totalDebits, totalCredits);
            
            // Verify that loan receivable is debited
            var loanLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 5001);
            Assert.NotNull(loanLine);
            Assert.Equal(amount, loanLine.DebitAmount);
            Assert.Equal(0, loanLine.CreditAmount);
            
            // Verify that cash is credited
            var cashLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 1001);
            Assert.NotNull(cashLine);
            Assert.Equal(0, cashLine.DebitAmount);
            Assert.Equal(amount, cashLine.CreditAmount);
        }

        [Fact]
        public async Task ProcessLoanRepaymentAsync_ShouldCreateBalancedJournalEntry()
        {
            // Arrange
            int loanId = 789;
            decimal principalAmount = 500;
            decimal interestAmount = 100;
            decimal totalAmount = principalAmount + interestAmount;
            string reference = "REPAY789";
            string description = "Loan repayment";
            
            JournalEntryDto capturedJournalEntry = null;
            
            MockJournalEntryService
                .Setup(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()))
                .Callback<JournalEntryDto>(je => capturedJournalEntry = je)
                .ReturnsAsync(new JournalEntryDto { Id = 1 });
                
            // Act
            await _service.ProcessLoanRepaymentAsync(loanId, principalAmount, interestAmount, reference, description);
            
            // Assert
            MockJournalEntryService.Verify(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()), Times.Once);
            
            Assert.NotNull(capturedJournalEntry);
            Assert.Equal("Loans", capturedJournalEntry.Source);
            Assert.Equal(reference, capturedJournalEntry.Reference);
            Assert.Equal(description, capturedJournalEntry.Description);
            
            // Verify lines
            Assert.Equal(3, capturedJournalEntry.Lines.Count);
            
            // Verify that the entry is balanced
            var totalDebits = capturedJournalEntry.Lines.Sum(l => l.DebitAmount);
            var totalCredits = capturedJournalEntry.Lines.Sum(l => l.CreditAmount);
            Assert.Equal(totalDebits, totalCredits);
            
            // Verify that cash is debited for total amount
            var cashLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 1001);
            Assert.NotNull(cashLine);
            Assert.Equal(totalAmount, cashLine.DebitAmount);
            Assert.Equal(0, cashLine.CreditAmount);
            
            // Verify that loan receivable is credited for principal
            var loanLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 5001);
            Assert.NotNull(loanLine);
            Assert.Equal(0, loanLine.DebitAmount);
            Assert.Equal(principalAmount, loanLine.CreditAmount);
            
            // Verify that interest income is credited
            var interestLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 4002);
            Assert.NotNull(interestLine);
            Assert.Equal(0, interestLine.DebitAmount);
            Assert.Equal(interestAmount, interestLine.CreditAmount);
        }

        [Fact]
        public async Task ProcessLoanWriteOffAsync_ShouldCreateBalancedJournalEntry()
        {
            // Arrange
            int loanId = 789;
            decimal amount = 2000;
            string reference = "WRITE789";
            string description = "Loan write-off";
            
            JournalEntryDto capturedJournalEntry = null;
            
            MockJournalEntryService
                .Setup(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()))
                .Callback<JournalEntryDto>(je => capturedJournalEntry = je)
                .ReturnsAsync(new JournalEntryDto { Id = 1 });
                
            // Act
            await _service.ProcessLoanWriteOffAsync(loanId, amount, reference, description);
            
            // Assert
            MockJournalEntryService.Verify(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()), Times.Once);
            
            Assert.NotNull(capturedJournalEntry);
            Assert.Equal("Loans", capturedJournalEntry.Source);
            Assert.Equal(reference, capturedJournalEntry.Reference);
            Assert.Equal(description, capturedJournalEntry.Description);
            
            // Verify lines
            Assert.Equal(2, capturedJournalEntry.Lines.Count);
            
            // Verify that the entry is balanced
            var totalDebits = capturedJournalEntry.Lines.Sum(l => l.DebitAmount);
            var totalCredits = capturedJournalEntry.Lines.Sum(l => l.CreditAmount);
            Assert.Equal(totalDebits, totalCredits);
            
            // Verify that write-off expense is debited
            var writeOffLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 5002);
            Assert.NotNull(writeOffLine);
            Assert.Equal(amount, writeOffLine.DebitAmount);
            Assert.Equal(0, writeOffLine.CreditAmount);
            
            // Verify that loan receivable is credited
            var loanLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 5001);
            Assert.NotNull(loanLine);
            Assert.Equal(0, loanLine.DebitAmount);
            Assert.Equal(amount, loanLine.CreditAmount);
        }
    }
}