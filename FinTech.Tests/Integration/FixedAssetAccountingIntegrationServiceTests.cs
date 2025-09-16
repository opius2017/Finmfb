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
    public class FixedAssetAccountingIntegrationServiceTests : IntegrationServiceTestBase
    {
        private readonly FixedAssetAccountingIntegrationService _service;
        private readonly Mock<ILogger<FixedAssetAccountingIntegrationService>> _mockLogger;

        public FixedAssetAccountingIntegrationServiceTests()
        {
            _mockLogger = new Mock<ILogger<FixedAssetAccountingIntegrationService>>();
            _service = new FixedAssetAccountingIntegrationService(
                _mockLogger.Object,
                MockJournalEntryService.Object,
                MockChartOfAccountService.Object);
        }

        [Fact]
        public async Task ProcessAssetAcquisitionAsync_ShouldCreateBalancedJournalEntry()
        {
            // Arrange
            int assetId = 123;
            decimal acquisitionCost = 100000;
            decimal taxAmount = 15000;
            string assetCategory = "Computer Equipment";
            string reference = "INV123456";
            string description = "Purchase of computer servers";
            
            JournalEntryDto capturedJournalEntry = null;
            
            MockJournalEntryService
                .Setup(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()))
                .Callback<JournalEntryDto>(je => capturedJournalEntry = je)
                .ReturnsAsync(new JournalEntryDto { Id = 1 });
                
            // Act
            await _service.ProcessAssetAcquisitionAsync(assetId, acquisitionCost, taxAmount, 
                assetCategory, reference, description);
            
            // Assert
            MockJournalEntryService.Verify(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()), Times.Once);
            
            Assert.NotNull(capturedJournalEntry);
            Assert.Equal("FixedAssets", capturedJournalEntry.Source);
            Assert.Equal(reference, capturedJournalEntry.Reference);
            Assert.Equal(description, capturedJournalEntry.Description);
            
            // Verify lines
            Assert.Equal(2, capturedJournalEntry.Lines.Count);
            
            // Verify that the entry is balanced
            var totalDebits = capturedJournalEntry.Lines.Sum(l => l.DebitAmount);
            var totalCredits = capturedJournalEntry.Lines.Sum(l => l.CreditAmount);
            Assert.Equal(totalDebits, totalCredits);
            
            // Verify that the fixed asset account is debited
            var assetLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 7001);
            Assert.NotNull(assetLine);
            Assert.Equal(acquisitionCost + taxAmount, assetLine.DebitAmount);
            Assert.Equal(0, assetLine.CreditAmount);
            
            // Verify that the cash/bank account is credited
            var cashLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 1001);
            Assert.NotNull(cashLine);
            Assert.Equal(0, cashLine.DebitAmount);
            Assert.Equal(acquisitionCost + taxAmount, cashLine.CreditAmount);
        }

        [Fact]
        public async Task ProcessAssetDepreciationAsync_ShouldCreateBalancedJournalEntry()
        {
            // Arrange
            int assetId = 123;
            decimal depreciationAmount = 1500;
            string period = "JUL-2025";
            string reference = "DEP123456";
            string description = "Monthly depreciation";
            string assetCategory = "Computer Equipment";
            
            JournalEntryDto capturedJournalEntry = null;
            
            MockJournalEntryService
                .Setup(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()))
                .Callback<JournalEntryDto>(je => capturedJournalEntry = je)
                .ReturnsAsync(new JournalEntryDto { Id = 1 });
                
            // Setup to return the asset category
            MockChartOfAccountService.Setup(s => s.GetAssetCategoryByIdAsync(assetId))
                .ReturnsAsync(assetCategory);
                
            // Act
            await _service.ProcessAssetDepreciationAsync(assetId, depreciationAmount, period, reference, description);
            
            // Assert
            MockJournalEntryService.Verify(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()), Times.Once);
            
            Assert.NotNull(capturedJournalEntry);
            Assert.Equal("FixedAssets", capturedJournalEntry.Source);
            Assert.Equal(reference, capturedJournalEntry.Reference);
            Assert.Equal(description, capturedJournalEntry.Description);
            
            // Verify lines
            Assert.Equal(2, capturedJournalEntry.Lines.Count);
            
            // Verify that the entry is balanced
            var totalDebits = capturedJournalEntry.Lines.Sum(l => l.DebitAmount);
            var totalCredits = capturedJournalEntry.Lines.Sum(l => l.CreditAmount);
            Assert.Equal(totalDebits, totalCredits);
            
            // Verify that the depreciation expense account is debited
            var expenseLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 7003);
            Assert.NotNull(expenseLine);
            Assert.Equal(depreciationAmount, expenseLine.DebitAmount);
            Assert.Equal(0, expenseLine.CreditAmount);
            
            // Verify that the accumulated depreciation account is credited
            var accumLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 7002);
            Assert.NotNull(accumLine);
            Assert.Equal(0, accumLine.DebitAmount);
            Assert.Equal(depreciationAmount, accumLine.CreditAmount);
        }

        [Fact]
        public async Task ProcessAssetDisposalAsync_WithGain_ShouldCreateBalancedJournalEntry()
        {
            // Arrange
            int assetId = 123;
            decimal disposalProceeds = 25000;
            decimal netBookValue = 20000;
            decimal gainLoss = 5000; // Gain
            string reference = "DISP123456";
            string description = "Disposal of computer equipment";
            string assetCategory = "Computer Equipment";
            
            JournalEntryDto capturedJournalEntry = null;
            
            MockJournalEntryService
                .Setup(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()))
                .Callback<JournalEntryDto>(je => capturedJournalEntry = je)
                .ReturnsAsync(new JournalEntryDto { Id = 1 });
                
            // Setup to return the asset category
            MockChartOfAccountService.Setup(s => s.GetAssetCategoryByIdAsync(assetId))
                .ReturnsAsync(assetCategory);
                
            // Act
            await _service.ProcessAssetDisposalAsync(assetId, disposalProceeds, netBookValue, gainLoss, reference, description);
            
            // Assert
            MockJournalEntryService.Verify(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()), Times.Once);
            
            Assert.NotNull(capturedJournalEntry);
            Assert.Equal("FixedAssets", capturedJournalEntry.Source);
            Assert.Equal(reference, capturedJournalEntry.Reference);
            Assert.Equal(description, capturedJournalEntry.Description);
            
            // Verify lines - should have 3 lines for a gain (cash, asset, gain)
            Assert.Equal(3, capturedJournalEntry.Lines.Count);
            
            // Verify that the entry is balanced
            var totalDebits = capturedJournalEntry.Lines.Sum(l => l.DebitAmount);
            var totalCredits = capturedJournalEntry.Lines.Sum(l => l.CreditAmount);
            Assert.Equal(totalDebits, totalCredits);
            
            // Verify that the cash/bank account is debited with proceeds
            var cashLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 1001);
            Assert.NotNull(cashLine);
            Assert.Equal(disposalProceeds, cashLine.DebitAmount);
            Assert.Equal(0, cashLine.CreditAmount);
            
            // Verify that the fixed asset account is credited with net book value
            var assetLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 7001);
            Assert.NotNull(assetLine);
            Assert.Equal(0, assetLine.DebitAmount);
            Assert.Equal(netBookValue, assetLine.CreditAmount);
            
            // Verify that the disposal gain account is credited with gain
            var gainLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 7004);
            Assert.NotNull(gainLine);
            Assert.Equal(0, gainLine.DebitAmount);
            Assert.Equal(gainLoss, gainLine.CreditAmount);
        }

        [Fact]
        public async Task ProcessAssetDisposalAsync_WithLoss_ShouldCreateBalancedJournalEntry()
        {
            // Arrange
            int assetId = 123;
            decimal disposalProceeds = 15000;
            decimal netBookValue = 20000;
            decimal gainLoss = -5000; // Loss (negative value)
            string reference = "DISP123456";
            string description = "Disposal of computer equipment";
            string assetCategory = "Computer Equipment";
            
            JournalEntryDto capturedJournalEntry = null;
            
            MockJournalEntryService
                .Setup(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()))
                .Callback<JournalEntryDto>(je => capturedJournalEntry = je)
                .ReturnsAsync(new JournalEntryDto { Id = 1 });
                
            // Setup to return the asset category
            MockChartOfAccountService.Setup(s => s.GetAssetCategoryByIdAsync(assetId))
                .ReturnsAsync(assetCategory);
                
            // Act
            await _service.ProcessAssetDisposalAsync(assetId, disposalProceeds, netBookValue, gainLoss, reference, description);
            
            // Assert
            MockJournalEntryService.Verify(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()), Times.Once);
            
            Assert.NotNull(capturedJournalEntry);
            Assert.Equal("FixedAssets", capturedJournalEntry.Source);
            Assert.Equal(reference, capturedJournalEntry.Reference);
            Assert.Equal(description, capturedJournalEntry.Description);
            
            // Verify lines - should have 3 lines for a loss (cash, asset, loss)
            Assert.Equal(3, capturedJournalEntry.Lines.Count);
            
            // Verify that the entry is balanced
            var totalDebits = capturedJournalEntry.Lines.Sum(l => l.DebitAmount);
            var totalCredits = capturedJournalEntry.Lines.Sum(l => l.CreditAmount);
            Assert.Equal(totalDebits, totalCredits);
            
            // Verify that the cash/bank account is debited with proceeds
            var cashLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 1001);
            Assert.NotNull(cashLine);
            Assert.Equal(disposalProceeds, cashLine.DebitAmount);
            Assert.Equal(0, cashLine.CreditAmount);
            
            // Verify that the disposal loss account is debited with loss (absolute value)
            var lossLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 7005);
            Assert.NotNull(lossLine);
            Assert.Equal(System.Math.Abs(gainLoss), lossLine.DebitAmount);
            Assert.Equal(0, lossLine.CreditAmount);
            
            // Verify that the fixed asset account is credited with net book value
            var assetLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 7001);
            Assert.NotNull(assetLine);
            Assert.Equal(0, assetLine.DebitAmount);
            Assert.Equal(netBookValue, assetLine.CreditAmount);
        }

        [Fact]
        public async Task ProcessAssetRevaluationAsync_Positive_ShouldCreateBalancedJournalEntry()
        {
            // Arrange
            int assetId = 123;
            decimal revaluationAmount = 10000; // Positive revaluation
            string reference = "REV123456";
            string description = "Asset revaluation";
            string assetCategory = "Computer Equipment";
            
            JournalEntryDto capturedJournalEntry = null;
            
            MockJournalEntryService
                .Setup(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()))
                .Callback<JournalEntryDto>(je => capturedJournalEntry = je)
                .ReturnsAsync(new JournalEntryDto { Id = 1 });
                
            // Setup to return the asset category
            MockChartOfAccountService.Setup(s => s.GetAssetCategoryByIdAsync(assetId))
                .ReturnsAsync(assetCategory);
                
            // Act
            await _service.ProcessAssetRevaluationAsync(assetId, revaluationAmount, reference, description);
            
            // Assert
            MockJournalEntryService.Verify(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()), Times.Once);
            
            Assert.NotNull(capturedJournalEntry);
            Assert.Equal("FixedAssets", capturedJournalEntry.Source);
            Assert.Equal(reference, capturedJournalEntry.Reference);
            Assert.Equal(description, capturedJournalEntry.Description);
            
            // Verify lines
            Assert.Equal(2, capturedJournalEntry.Lines.Count);
            
            // Verify that the entry is balanced
            var totalDebits = capturedJournalEntry.Lines.Sum(l => l.DebitAmount);
            var totalCredits = capturedJournalEntry.Lines.Sum(l => l.CreditAmount);
            Assert.Equal(totalDebits, totalCredits);
            
            // Verify that the fixed asset account is debited
            var assetLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 7001);
            Assert.NotNull(assetLine);
            Assert.Equal(revaluationAmount, assetLine.DebitAmount);
            Assert.Equal(0, assetLine.CreditAmount);
            
            // Verify that the revaluation reserve account is credited
            var reserveLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 7006);
            Assert.NotNull(reserveLine);
            Assert.Equal(0, reserveLine.DebitAmount);
            Assert.Equal(revaluationAmount, reserveLine.CreditAmount);
        }

        [Fact]
        public async Task ProcessAssetRevaluationAsync_Negative_ShouldCreateBalancedJournalEntry()
        {
            // Arrange
            int assetId = 123;
            decimal revaluationAmount = -8000; // Negative revaluation
            string reference = "REV123456";
            string description = "Asset revaluation";
            string assetCategory = "Computer Equipment";
            
            JournalEntryDto capturedJournalEntry = null;
            
            MockJournalEntryService
                .Setup(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()))
                .Callback<JournalEntryDto>(je => capturedJournalEntry = je)
                .ReturnsAsync(new JournalEntryDto { Id = 1 });
                
            // Setup to return the asset category
            MockChartOfAccountService.Setup(s => s.GetAssetCategoryByIdAsync(assetId))
                .ReturnsAsync(assetCategory);
                
            // Act
            await _service.ProcessAssetRevaluationAsync(assetId, revaluationAmount, reference, description);
            
            // Assert
            MockJournalEntryService.Verify(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()), Times.Once);
            
            Assert.NotNull(capturedJournalEntry);
            Assert.Equal("FixedAssets", capturedJournalEntry.Source);
            Assert.Equal(reference, capturedJournalEntry.Reference);
            Assert.Equal(description, capturedJournalEntry.Description);
            
            // Verify lines
            Assert.Equal(2, capturedJournalEntry.Lines.Count);
            
            // Verify that the entry is balanced
            var totalDebits = capturedJournalEntry.Lines.Sum(l => l.DebitAmount);
            var totalCredits = capturedJournalEntry.Lines.Sum(l => l.CreditAmount);
            Assert.Equal(totalDebits, totalCredits);
            
            // Verify that the revaluation reserve account is debited
            var reserveLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 7006);
            Assert.NotNull(reserveLine);
            Assert.Equal(System.Math.Abs(revaluationAmount), reserveLine.DebitAmount);
            Assert.Equal(0, reserveLine.CreditAmount);
            
            // Verify that the fixed asset account is credited
            var assetLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 7001);
            Assert.NotNull(assetLine);
            Assert.Equal(0, assetLine.DebitAmount);
            Assert.Equal(System.Math.Abs(revaluationAmount), assetLine.CreditAmount);
        }

        [Fact]
        public async Task ProcessAssetImpairmentAsync_ShouldCreateBalancedJournalEntry()
        {
            // Arrange
            int assetId = 123;
            decimal impairmentAmount = 5000;
            string reference = "IMP123456";
            string description = "Asset impairment";
            string assetCategory = "Computer Equipment";
            
            JournalEntryDto capturedJournalEntry = null;
            
            MockJournalEntryService
                .Setup(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()))
                .Callback<JournalEntryDto>(je => capturedJournalEntry = je)
                .ReturnsAsync(new JournalEntryDto { Id = 1 });
                
            // Setup to return the asset category
            MockChartOfAccountService.Setup(s => s.GetAssetCategoryByIdAsync(assetId))
                .ReturnsAsync(assetCategory);
                
            // Act
            await _service.ProcessAssetImpairmentAsync(assetId, impairmentAmount, reference, description);
            
            // Assert
            MockJournalEntryService.Verify(s => s.CreateJournalEntryAsync(It.IsAny<JournalEntryDto>()), Times.Once);
            
            Assert.NotNull(capturedJournalEntry);
            Assert.Equal("FixedAssets", capturedJournalEntry.Source);
            Assert.Equal(reference, capturedJournalEntry.Reference);
            Assert.Equal(description, capturedJournalEntry.Description);
            
            // Verify lines
            Assert.Equal(2, capturedJournalEntry.Lines.Count);
            
            // Verify that the entry is balanced
            var totalDebits = capturedJournalEntry.Lines.Sum(l => l.DebitAmount);
            var totalCredits = capturedJournalEntry.Lines.Sum(l => l.CreditAmount);
            Assert.Equal(totalDebits, totalCredits);
            
            // Verify that the impairment expense account is debited
            var impairmentLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 7007);
            Assert.NotNull(impairmentLine);
            Assert.Equal(impairmentAmount, impairmentLine.DebitAmount);
            Assert.Equal(0, impairmentLine.CreditAmount);
            
            // Verify that the fixed asset account is credited
            var assetLine = capturedJournalEntry.Lines.FirstOrDefault(l => l.AccountId == 7001);
            Assert.NotNull(assetLine);
            Assert.Equal(0, assetLine.DebitAmount);
            Assert.Equal(impairmentAmount, assetLine.CreditAmount);
        }
    }
}