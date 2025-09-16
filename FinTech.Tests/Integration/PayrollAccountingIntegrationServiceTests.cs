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
    public class PayrollAccountingIntegrationServiceTests : IntegrationServiceTestBase
    {
        private readonly PayrollAccountingIntegrationService _service;
        private readonly Mock<ILogger<PayrollAccountingIntegrationService>> _mockLogger;

        public PayrollAccountingIntegrationServiceTests()
        {
            _mockLogger = new Mock<ILogger<PayrollAccountingIntegrationService>>();
            _service = new PayrollAccountingIntegrationService(
                _mockLogger.Object,
                MockJournalEntryService.Object,
                MockChartOfAccountService.Object);
        }

        [Fact]
        public async Task ProcessSalaryPaymentAsync_ShouldCreateBalancedJournalEntry()
        {
            // Arrange
            int employeeId = 123;
            decimal grossAmount = 5000;
            decimal taxAmount = 1000;
            decimal pensionAmount = 500;
            decimal otherDeductions = 250;
            string payPeriod = "JUL-2025";
            string reference = "SAL123456";
            string description = "July 2025 salary payment";
            
            JournalEntryDto capturedJournalEntry = null;
            
            MockJournalEntryService
                .Setup(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()))
                .Callback<JournalEntryDto>(je => capturedJournalEntry = je)
                .ReturnsAsync(new JournalEntryDto { Id = 1 });
                
            // Act
            await _service.ProcessSalaryPaymentAsync(employeeId, grossAmount, taxAmount, 
                pensionAmount, otherDeductions, payPeriod, reference, description);
            
            // Assert
            MockJournalEntryService.Verify(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()), Times.Once);
            
            Assert.NotNull(capturedJournalEntry);
            Assert.Equal("Payroll", capturedJournalEntry.Source);
            Assert.Equal(reference, capturedJournalEntry.Reference);
            Assert.Equal(description, capturedJournalEntry.Description);
            
            // Verify lines
            Assert.Equal(4, capturedJournalEntry.Lines.Count);
            
            // Verify that the entry is balanced
            var totalDebits = capturedJournalEntry.Lines.Sum(l => l.DebitAmount);
            var totalCredits = capturedJournalEntry.Lines.Sum(l => l.CreditAmount);
            Assert.Equal(totalDebits, totalCredits);
            
            // Verify that the payroll expense account is debited
            var expenseLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 6001);
            Assert.NotNull(expenseLine);
            Assert.Equal(grossAmount, expenseLine.DebitAmount);
            Assert.Equal(0, expenseLine.CreditAmount);
            
            // Verify that the tax payable account is credited
            var taxLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 6003);
            Assert.NotNull(taxLine);
            Assert.Equal(0, taxLine.DebitAmount);
            Assert.Equal(taxAmount, taxLine.CreditAmount);
            
            // Verify that the pension payable account is credited
            var pensionLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 6004);
            Assert.NotNull(pensionLine);
            Assert.Equal(0, pensionLine.DebitAmount);
            Assert.Equal(pensionAmount, pensionLine.CreditAmount);
            
            // Verify that the cash/bank account is credited with net salary
            var netAmount = grossAmount - taxAmount - pensionAmount - otherDeductions;
            var cashLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 1001);
            Assert.NotNull(cashLine);
            Assert.Equal(0, cashLine.DebitAmount);
            Assert.Equal(netAmount, cashLine.CreditAmount);
        }

        [Fact]
        public async Task ProcessTaxRemittanceAsync_ShouldCreateBalancedJournalEntry()
        {
            // Arrange
            decimal amount = 15000;
            string taxType = "PAYE";
            string taxPeriod = "Q2-2025";
            string reference = "TAX123456";
            string description = "Q2 2025 PAYE tax remittance";
            
            JournalEntryDto capturedJournalEntry = null;
            
            MockJournalEntryService
                .Setup(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()))
                .Callback<JournalEntryDto>(je => capturedJournalEntry = je)
                .ReturnsAsync(new JournalEntryDto { Id = 1 });
                
            // Act
            await _service.ProcessTaxRemittanceAsync(amount, taxType, taxPeriod, reference, description);
            
            // Assert
            MockJournalEntryService.Verify(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()), Times.Once);
            
            Assert.NotNull(capturedJournalEntry);
            Assert.Equal("Payroll", capturedJournalEntry.Source);
            Assert.Equal(reference, capturedJournalEntry.Reference);
            Assert.Equal(description, capturedJournalEntry.Description);
            
            // Verify lines
            Assert.Equal(2, capturedJournalEntry.Lines.Count);
            
            // Verify that the entry is balanced
            var totalDebits = capturedJournalEntry.Lines.Sum(l => l.DebitAmount);
            var totalCredits = capturedJournalEntry.Lines.Sum(l => l.CreditAmount);
            Assert.Equal(totalDebits, totalCredits);
            
            // Verify that the tax payable account is debited
            var taxPayableLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 6003);
            Assert.NotNull(taxPayableLine);
            Assert.Equal(amount, taxPayableLine.DebitAmount);
            Assert.Equal(0, taxPayableLine.CreditAmount);
            
            // Verify that the cash/bank account is credited
            var cashLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 1001);
            Assert.NotNull(cashLine);
            Assert.Equal(0, cashLine.DebitAmount);
            Assert.Equal(amount, cashLine.CreditAmount);
        }

        [Fact]
        public async Task ProcessPensionRemittanceAsync_ShouldCreateBalancedJournalEntry()
        {
            // Arrange
            decimal amount = 7500;
            string pensionProvider = "PFC Pensions";
            string pensionPeriod = "Q2-2025";
            string reference = "PEN123456";
            string description = "Q2 2025 pension remittance";
            
            JournalEntryDto capturedJournalEntry = null;
            
            MockJournalEntryService
                .Setup(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()))
                .Callback<JournalEntryDto>(je => capturedJournalEntry = je)
                .ReturnsAsync(new JournalEntryDto { Id = 1 });
                
            // Act
            await _service.ProcessPensionRemittanceAsync(amount, pensionProvider, pensionPeriod, reference, description);
            
            // Assert
            MockJournalEntryService.Verify(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()), Times.Once);
            
            Assert.NotNull(capturedJournalEntry);
            Assert.Equal("Payroll", capturedJournalEntry.Source);
            Assert.Equal(reference, capturedJournalEntry.Reference);
            Assert.Equal(description, capturedJournalEntry.Description);
            
            // Verify lines
            Assert.Equal(2, capturedJournalEntry.Lines.Count);
            
            // Verify that the entry is balanced
            var totalDebits = capturedJournalEntry.Lines.Sum(l => l.DebitAmount);
            var totalCredits = capturedJournalEntry.Lines.Sum(l => l.CreditAmount);
            Assert.Equal(totalDebits, totalCredits);
            
            // Verify that the pension payable account is debited
            var pensionPayableLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 6004);
            Assert.NotNull(pensionPayableLine);
            Assert.Equal(amount, pensionPayableLine.DebitAmount);
            Assert.Equal(0, pensionPayableLine.CreditAmount);
            
            // Verify that the cash/bank account is credited
            var cashLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 1001);
            Assert.NotNull(cashLine);
            Assert.Equal(0, cashLine.DebitAmount);
            Assert.Equal(amount, cashLine.CreditAmount);
        }

        [Fact]
        public async Task ProcessBonusPaymentAsync_ShouldCreateBalancedJournalEntry()
        {
            // Arrange
            int employeeId = 123;
            decimal amount = 10000;
            string reference = "BON123456";
            string description = "Performance bonus payment";
            
            JournalEntryDto capturedJournalEntry = null;
            
            MockJournalEntryService
                .Setup(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()))
                .Callback<JournalEntryDto>(je => capturedJournalEntry = je)
                .ReturnsAsync(new JournalEntryDto { Id = 1 });
                
            // Act
            await _service.ProcessBonusPaymentAsync(employeeId, amount, reference, description);
            
            // Assert
            MockJournalEntryService.Verify(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()), Times.Once);
            
            Assert.NotNull(capturedJournalEntry);
            Assert.Equal("Payroll", capturedJournalEntry.Source);
            Assert.Equal(reference, capturedJournalEntry.Reference);
            Assert.Equal(description, capturedJournalEntry.Description);
            
            // Verify lines
            Assert.Equal(2, capturedJournalEntry.Lines.Count);
            
            // Verify that the entry is balanced
            var totalDebits = capturedJournalEntry.Lines.Sum(l => l.DebitAmount);
            var totalCredits = capturedJournalEntry.Lines.Sum(l => l.CreditAmount);
            Assert.Equal(totalDebits, totalCredits);
            
            // Verify that the payroll expense account is debited
            var expenseLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 6001);
            Assert.NotNull(expenseLine);
            Assert.Equal(amount, expenseLine.DebitAmount);
            Assert.Equal(0, expenseLine.CreditAmount);
            
            // Verify that the cash/bank account is credited
            var cashLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 1001);
            Assert.NotNull(cashLine);
            Assert.Equal(0, cashLine.DebitAmount);
            Assert.Equal(amount, cashLine.CreditAmount);
        }

        [Fact]
        public async Task ProcessExpenseAccrualAsync_ShouldCreateBalancedJournalEntry()
        {
            // Arrange
            decimal amount = 25000;
            string expenseType = "SALARY";
            string period = "AUG-2025";
            string reference = "ACC123456";
            string description = "August 2025 salary accrual";
            
            JournalEntryDto capturedJournalEntry = null;
            
            MockJournalEntryService
                .Setup(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()))
                .Callback<JournalEntryDto>(je => capturedJournalEntry = je)
                .ReturnsAsync(new JournalEntryDto { Id = 1 });
                
            // Act
            await _service.ProcessExpenseAccrualAsync(amount, expenseType, period, reference, description);
            
            // Assert
            MockJournalEntryService.Verify(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()), Times.Once);
            
            Assert.NotNull(capturedJournalEntry);
            Assert.Equal("Payroll", capturedJournalEntry.Source);
            Assert.Equal(reference, capturedJournalEntry.Reference);
            Assert.Equal(description, capturedJournalEntry.Description);
            
            // Verify lines
            Assert.Equal(2, capturedJournalEntry.Lines.Count);
            
            // Verify that the entry is balanced
            var totalDebits = capturedJournalEntry.Lines.Sum(l => l.DebitAmount);
            var totalCredits = capturedJournalEntry.Lines.Sum(l => l.CreditAmount);
            Assert.Equal(totalDebits, totalCredits);
            
            // Verify that the payroll expense account is debited
            var expenseLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 6001);
            Assert.NotNull(expenseLine);
            Assert.Equal(amount, expenseLine.DebitAmount);
            Assert.Equal(0, expenseLine.CreditAmount);
            
            // Verify that the payroll liability account is credited
            var liabilityLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 6002);
            Assert.NotNull(liabilityLine);
            Assert.Equal(0, liabilityLine.DebitAmount);
            Assert.Equal(amount, liabilityLine.CreditAmount);
        }
    }
}