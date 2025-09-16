using FinTech.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinTech.Tests.Integration
{
    public abstract class IntegrationServiceTestBase
    {
        protected readonly Mock<IJournalEntryService> MockJournalEntryService;
        protected readonly Mock<IChartOfAccountService> MockChartOfAccountService;
        protected readonly Mock<ILogger> MockLogger;

        public IntegrationServiceTestBase()
        {
            MockJournalEntryService = new Mock<IJournalEntryService>();
            MockChartOfAccountService = new Mock<IChartOfAccountService>();
            MockLogger = new Mock<ILogger>();

            // Set up common chart of account lookups
            MockChartOfAccountService.Setup(s => s.GetCashAccountIdAsync())
                .ReturnsAsync(1001);
                
            MockChartOfAccountService.Setup(s => s.GetBankAccountIdForCustomerAsync(It.IsAny<int>()))
                .ReturnsAsync(2001);
                
            MockChartOfAccountService.Setup(s => s.GetFeeIncomeAccountIdAsync(It.IsAny<string>()))
                .ReturnsAsync(3001);
                
            MockChartOfAccountService.Setup(s => s.GetInterestExpenseAccountIdAsync())
                .ReturnsAsync(4001);
                
            MockChartOfAccountService.Setup(s => s.GetInterestIncomeAccountIdAsync())
                .ReturnsAsync(4002);
                
            MockChartOfAccountService.Setup(s => s.GetLoanReceivableAccountIdAsync())
                .ReturnsAsync(5001);
                
            MockChartOfAccountService.Setup(s => s.GetLoanWriteOffAccountIdAsync())
                .ReturnsAsync(5002);
                
            MockChartOfAccountService.Setup(s => s.GetPayrollExpenseAccountIdAsync())
                .ReturnsAsync(6001);
                
            MockChartOfAccountService.Setup(s => s.GetPayrollLiabilityAccountIdAsync())
                .ReturnsAsync(6002);
                
            MockChartOfAccountService.Setup(s => s.GetTaxPayableAccountIdAsync())
                .ReturnsAsync(6003);
                
            MockChartOfAccountService.Setup(s => s.GetPensionPayableAccountIdAsync())
                .ReturnsAsync(6004);
                
            MockChartOfAccountService.Setup(s => s.GetFixedAssetAccountIdAsync(It.IsAny<string>()))
                .ReturnsAsync(7001);
                
            MockChartOfAccountService.Setup(s => s.GetAccumulatedDepreciationAccountIdAsync(It.IsAny<string>()))
                .ReturnsAsync(7002);
                
            MockChartOfAccountService.Setup(s => s.GetDepreciationExpenseAccountIdAsync(It.IsAny<string>()))
                .ReturnsAsync(7003);
                
            MockChartOfAccountService.Setup(s => s.GetAssetDisposalGainAccountIdAsync())
                .ReturnsAsync(7004);
                
            MockChartOfAccountService.Setup(s => s.GetAssetDisposalLossAccountIdAsync())
                .ReturnsAsync(7005);
                
            MockChartOfAccountService.Setup(s => s.GetAssetRevaluationReserveAccountIdAsync())
                .ReturnsAsync(7006);
                
            MockChartOfAccountService.Setup(s => s.GetAssetImpairmentAccountIdAsync())
                .ReturnsAsync(7007);
        }
    }
}