using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Application.Services.Loans;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinTech.Tests.Integration
{
    /// <summary>
    /// Integration tests for repayment and delinquency workflows
    /// </summary>
    public class RepaymentWorkflowIntegrationTests : IDisposable
    {
        private readonly Mock<IRepository<Loan>> _loanRepository;
        private readonly Mock<IRepository<LoanTransaction>> _transactionRepository;
        private readonly Mock<IRepository<LoanRepaymentSchedule>> _scheduleRepository;
        private readonly Mock<IRepository<Member>> _memberRepository;
        private readonly Mock<IRepository<GuarantorConsent>> _guarantorConsentRepository;
        private readonly Mock<ILoanRegisterService> _registerService;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<ILogger<LoanRepaymentService>> _repaymentLogger;
        private readonly Mock<ILogger<DelinquencyManagementService>> _delinquencyLogger;

        private readonly ILoanCalculatorService _calculatorService;
        private readonly ILoanRepaymentService _repaymentService;
        private readonly IDelinquencyManagementService _delinquencyService;

        public RepaymentWorkflowIntegrationTests()
        {
            // Setup mocks
            _loanRepository = new Mock<IRepository<Loan>>();
            _transactionRepository = new Mock<IRepository<LoanTransaction>>();
            _scheduleRepository = new Mock<IRepository<LoanRepaymentSchedule>>();
            _memberRepository = new Mock<IRepository<Member>>();
            _guarantorConsentRepository = new Mock<IRepository<GuarantorConsent>>();
            _registerService = new Mock<ILoanRegisterService>();
            _unitOfWork = new Mock<IUnitOfWork>();
            _repaymentLogger = new Mock<ILogger<LoanRepaymentService>>();
            _delinquencyLogger = new Mock<ILogger<DelinquencyManagementService>>();

            // Initialize services
            _calculatorService = new LoanCalculatorService();

            _repaymentService = new LoanRepaymentService(
                _loanRepository.Object,
                _transactionRepository.Object,
                _scheduleRepository.Object,
                _memberRepository.Object,
                _calculatorService,
                _registerService.Object,
                _unitOfWork.Object,
                _repaymentLogger.Object);

            _delinquencyService = new DelinquencyManagementService(
                _loanRepository.Object,
                _memberRepository.Object,
                _guarantorConsentRepository.Object,
                _calculatorService,
                _unitOfWork.Object,
                _delinquencyLogger.Object);
        }

        [Fact]
        public async Task CompleteRepaymentWorkflow_FullPayment_Success()
        {
            // Arrange
            var loan = CreateTestLoan(100000m, 15m, 12);
            var member = CreateTestMember("M001", 50000m);

            _loanRepository.Setup(r => r.GetByIdAsync(loan.Id))
                .ReturnsAsync(loan);
            _memberRepository.Setup(r => r.GetByIdAsync(member.Id))
                .ReturnsAsync(member);

            var schedules = CreateTestSchedules(loan.Id, 12, loan.MonthlyInstallment);
            _scheduleRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(schedules);

            // Act - Process monthly payment
            var repaymentRequest = new RepaymentRequest
            {
                LoanId = loan.Id,
                Amount = loan.MonthlyInstallment,
                PaymentMethod = "BANK_TRANSFER",
                TransactionReference = "TXN123456",
                ProcessedBy = "SYSTEM"
            };

            var result = await _repaymentService.ProcessRepaymentAsync(repaymentRequest);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(loan.MonthlyInstallment, result.AmountPaid);
            Assert.True(result.PrincipalPaid > 0);
            Assert.True(result.InterestPaid > 0);
            Assert.True(result.RemainingBalance < loan.OutstandingBalance);
            Assert.False(result.IsLoanFullyPaid);

            // Verify interest-first allocation
            Assert.True(result.InterestPaid > 0, "Interest should be paid first");
        }

        [Fact]
        public async Task PartialPayment_CorrectAllocation_Success()
        {
            // Arrange
            var loan = CreateTestLoan(100000m, 15m, 12);
            _loanRepository.Setup(r => r.GetByIdAsync(loan.Id))
                .ReturnsAsync(loan);

            var member = CreateTestMember("M001", 50000m);
            _memberRepository.Setup(r => r.GetByIdAsync(member.Id))
                .ReturnsAsync(member);

            _scheduleRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<LoanRepaymentSchedule>());

            // Act - Process partial payment (half of EMI)
            var partialAmount = loan.MonthlyInstallment / 2;
            var repaymentRequest = new RepaymentRequest
            {
                LoanId = loan.Id,
                Amount = partialAmount,
                PaymentMethod = "CASH",
                TransactionReference = "CASH001",
                ProcessedBy = "TELLER"
            };

            var result = await _repaymentService.ProcessPartialPaymentAsync(repaymentRequest);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(partialAmount, result.AmountPaid);
            Assert.True(result.RemainingBalance > 0);
            Assert.False(result.IsLoanFullyPaid);
        }

        [Fact]
        public async Task MultiplePayments_LoanClosure_Success()
        {
            // Arrange
            var loan = CreateTestLoan(100000m, 15m, 12);
            loan.OutstandingBalance = loan.MonthlyInstallment; // Last payment
            loan.PrincipalPaid = loan.PrincipalAmount - 8500m;
            loan.InterestPaid = loan.TotalRepayableAmount - loan.PrincipalAmount - 100m;

            _loanRepository.Setup(r => r.GetByIdAsync(loan.Id))
                .ReturnsAsync(loan);

            var member = CreateTestMember("M001", 50000m);
            _memberRepository.Setup(r => r.GetByIdAsync(member.Id))
                .ReturnsAsync(member);

            _scheduleRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<LoanRepaymentSchedule>());

            // Act - Final payment
            var repaymentRequest = new RepaymentRequest
            {
                LoanId = loan.Id,
                Amount = loan.MonthlyInstallment,
                PaymentMethod = "BANK_TRANSFER",
                TransactionReference = "FINAL_PAYMENT",
                ProcessedBy = "SYSTEM"
            };

            var result = await _repaymentService.ProcessRepaymentAsync(repaymentRequest);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.IsLoanFullyPaid);
            Assert.Contains("fully repaid", result.Message.ToLower());
        }

        [Fact]
        public async Task DelinquencyDetection_OverdueLoan_PenaltyApplied()
        {
            // Arrange
            var loan = CreateTestLoan(100000m, 15m, 12);
            loan.NextPaymentDate = DateTime.UtcNow.AddDays(-5); // 5 days overdue
            loan.DaysInArrears = 0; // Not yet detected

            _loanRepository.Setup(r => r.GetByIdAsync(loan.Id))
                .ReturnsAsync(loan);

            // Act
            var status = await _delinquencyService.CheckLoanDelinquencyAsync(loan.Id);

            // Assert
            Assert.True(status.IsOverdue);
            Assert.Equal(5, status.DaysOverdue);
            Assert.True(status.PenaltyAmount > 0);
            Assert.True(status.OverdueAmount > 0);
            Assert.Equal("PERFORMING", status.Classification); // Still performing (< 30 days)
        }

        [Fact]
        public async Task DelinquencyDetection_30DaysOverdue_ClassificationChanged()
        {
            // Arrange
            var loan = CreateTestLoan(100000m, 15m, 12);
            loan.NextPaymentDate = DateTime.UtcNow.AddDays(-35); // 35 days overdue

            _loanRepository.Setup(r => r.GetByIdAsync(loan.Id))
                .ReturnsAsync(loan);

            // Act
            var status = await _delinquencyService.CheckLoanDelinquencyAsync(loan.Id);

            // Assert
            Assert.True(status.IsOverdue);
            Assert.Equal(35, status.DaysOverdue);
            Assert.Equal("SPECIAL_MENTION", status.Classification); // 31-90 days
            Assert.True(status.ClassificationChanged);
        }

        [Fact]
        public async Task DelinquencyDetection_90DaysOverdue_SubstandardClassification()
        {
            // Arrange
            var loan = CreateTestLoan(100000m, 15m, 12);
            loan.NextPaymentDate = DateTime.UtcNow.AddDays(-95); // 95 days overdue

            _loanRepository.Setup(r => r.GetByIdAsync(loan.Id))
                .ReturnsAsync(loan);

            // Act
            var status = await _delinquencyService.CheckLoanDelinquencyAsync(loan.Id);

            // Assert
            Assert.True(status.IsOverdue);
            Assert.Equal(95, status.DaysOverdue);
            Assert.Equal("SUBSTANDARD", status.Classification); // 91-180 days
        }

        [Fact]
        public async Task DailyDelinquencyCheck_MultipleLoans_CorrectIdentification()
        {
            // Arrange
            var loan1 = CreateTestLoan(100000m, 15m, 12);
            loan1.NextPaymentDate = DateTime.UtcNow.AddDays(-3); // 3 days overdue

            var loan2 = CreateTestLoan(200000m, 15m, 24);
            loan2.NextPaymentDate = DateTime.UtcNow.AddDays(-7); // 7 days overdue

            var loan3 = CreateTestLoan(150000m, 15m, 12);
            loan3.NextPaymentDate = DateTime.UtcNow.AddDays(5); // Not overdue

            var allLoans = new[] { loan1, loan2, loan3 };
            _loanRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(allLoans);

            // Act
            var result = await _delinquencyService.PerformDailyDelinquencyCheckAsync();

            // Assert
            Assert.Equal(3, result.LoansChecked);
            Assert.Equal(2, result.DelinquentLoans.Count); // Only loan1 and loan2
            Assert.True(result.PenaltiesApplied > 0);
        }

        [Fact]
        public void PenaltyCalculation_IncreasingOverDays_CorrectAmount()
        {
            // Arrange
            decimal overdueAmount = 10000m;
            decimal penaltyRatePerDay = 0.1m; // 0.1% per day

            // Act & Assert - Day 1
            var penalty1Day = _calculatorService.CalculatePenalty(overdueAmount, 1, penaltyRatePerDay);
            Assert.Equal(10m, penalty1Day); // 10000 * 0.001 * 1 = 10

            // Day 7
            var penalty7Days = _calculatorService.CalculatePenalty(overdueAmount, 7, penaltyRatePerDay);
            Assert.Equal(70m, penalty7Days); // 10000 * 0.001 * 7 = 70

            // Day 30
            var penalty30Days = _calculatorService.CalculatePenalty(overdueAmount, 30, penaltyRatePerDay);
            Assert.Equal(300m, penalty30Days); // 10000 * 0.001 * 30 = 300
        }

        [Fact]
        public async Task OverdueLoans_FilterByDays_CorrectResults()
        {
            // Arrange
            var loan1 = CreateTestLoan(100000m, 15m, 12);
            loan1.NextPaymentDate = DateTime.UtcNow.AddDays(-5);

            var loan2 = CreateTestLoan(200000m, 15m, 24);
            loan2.NextPaymentDate = DateTime.UtcNow.AddDays(-10);

            var loan3 = CreateTestLoan(150000m, 15m, 12);
            loan3.NextPaymentDate = DateTime.UtcNow.AddDays(-3);

            var allLoans = new[] { loan1, loan2, loan3 };
            _loanRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(allLoans);

            // Act - Get loans overdue by at least 7 days
            var overdueLoans = await _delinquencyService.IdentifyOverdueLoansAsync(minDaysOverdue: 7);

            // Assert
            Assert.Single(overdueLoans); // Only loan2 (10 days overdue)
            Assert.Equal(loan2.Id, overdueLoans[0].LoanId);
        }

        #region Helper Methods

        private Loan CreateTestLoan(decimal principal, decimal interestRate, int tenorMonths)
        {
            var emi = _calculatorService.CalculateEMI(principal, interestRate, tenorMonths);
            var totalRepayable = _calculatorService.CalculateTotalRepayable(principal, interestRate, tenorMonths);

            return new Loan
            {
                Id = Guid.NewGuid().ToString(),
                LoanNumber = $"LH/2024/{new Random().Next(1, 999):D3}",
                MemberId = Guid.NewGuid().ToString(),
                PrincipalAmount = principal,
                InterestRate = interestRate,
                RepaymentPeriodMonths = tenorMonths,
                MonthlyInstallment = emi,
                TotalRepayableAmount = totalRepayable,
                OutstandingBalance = totalRepayable,
                PrincipalPaid = 0,
                InterestPaid = 0,
                DisbursementDate = DateTime.UtcNow.AddMonths(-1),
                MaturityDate = DateTime.UtcNow.AddMonths(tenorMonths - 1),
                LoanStatus = "ACTIVE",
                PaymentFrequency = "MONTHLY",
                NextPaymentDate = DateTime.UtcNow.AddDays(25),
                DaysInArrears = 0,
                ArrearsAmount = 0,
                PenaltyAmount = 0,
                Classification = "PERFORMING",
                CreatedAt = DateTime.UtcNow
            };
        }

        private Member CreateTestMember(string memberNumber, decimal savings)
        {
            return new Member
            {
                Id = Guid.NewGuid().ToString(),
                MemberNumber = memberNumber,
                FirstName = "Test",
                LastName = "Member",
                Email = $"{memberNumber}@test.com",
                TotalSavings = savings,
                OutstandingLoanBalance = 0,
                IsActive = true,
                MembershipDate = DateTime.UtcNow.AddYears(-2),
                MembershipStatus = "ACTIVE",
                CreatedAt = DateTime.UtcNow
            };
        }

        private List<LoanRepaymentSchedule> CreateTestSchedules(string loanId, int count, decimal emi)
        {
            var schedules = new List<LoanRepaymentSchedule>();
            for (int i = 1; i <= count; i++)
            {
                schedules.Add(new LoanRepaymentSchedule
                {
                    Id = Guid.NewGuid().ToString(),
                    LoanId = loanId,
                    InstallmentNumber = i,
                    DueDate = DateTime.UtcNow.AddMonths(i),
                    PrincipalAmount = emi * 0.7m, // Approximate
                    InterestAmount = emi * 0.3m,
                    Status = "PENDING",
                    CreatedAt = DateTime.UtcNow
                });
            }
            return schedules;
        }

        #endregion

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}
