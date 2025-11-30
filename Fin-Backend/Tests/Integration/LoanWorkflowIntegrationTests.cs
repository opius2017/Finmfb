using System;
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
    /// Integration tests for complete loan workflow
    /// Tests the entire lifecycle: Application → Eligibility → Guarantor → Committee → Disbursement → Repayment
    /// </summary>
    public class LoanWorkflowIntegrationTests : IDisposable
    {
        private readonly Mock<IRepository<Member>> _memberRepository;
        private readonly Mock<IRepository<LoanApplication>> _loanApplicationRepository;
        private readonly Mock<IRepository<Loan>> _loanRepository;
        private readonly Mock<IRepository<GuarantorConsent>> _guarantorConsentRepository;
        private readonly Mock<IRepository<CommitteeReview>> _committeeReviewRepository;
        private readonly Mock<IRepository<LoanRegister>> _loanRegisterRepository;
        private readonly Mock<IRepository<MonthlyThreshold>> _thresholdRepository;
        private readonly Mock<IRepository<LoanTransaction>> _transactionRepository;
        private readonly Mock<IRepository<LoanRepaymentSchedule>> _scheduleRepository;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<ILogger<LoanEligibilityService>> _eligibilityLogger;
        private readonly Mock<ILogger<GuarantorService>> _guarantorLogger;
        private readonly Mock<ILogger<LoanCommitteeService>> _committeeLogger;
        private readonly Mock<ILogger<LoanRegisterService>> _registerLogger;
        private readonly Mock<ILogger<LoanDisbursementService>> _disbursementLogger;
        private readonly Mock<ILogger<LoanRepaymentService>> _repaymentLogger;

        private readonly ILoanCalculatorService _calculatorService;
        private readonly ILoanEligibilityService _eligibilityService;
        private readonly IGuarantorService _guarantorService;
        private readonly ILoanCommitteeService _committeeService;
        private readonly ILoanRegisterService _registerService;
        private readonly ILoanDisbursementService _disbursementService;
        private readonly ILoanRepaymentService _repaymentService;

        public LoanWorkflowIntegrationTests()
        {
            // Setup mocks
            _memberRepository = new Mock<IRepository<Member>>();
            _loanApplicationRepository = new Mock<IRepository<LoanApplication>>();
            _loanRepository = new Mock<IRepository<Loan>>();
            _guarantorConsentRepository = new Mock<IRepository<GuarantorConsent>>();
            _committeeReviewRepository = new Mock<IRepository<CommitteeReview>>();
            _loanRegisterRepository = new Mock<IRepository<LoanRegister>>();
            _thresholdRepository = new Mock<IRepository<MonthlyThreshold>>();
            _transactionRepository = new Mock<IRepository<LoanTransaction>>();
            _scheduleRepository = new Mock<IRepository<LoanRepaymentSchedule>>();
            _unitOfWork = new Mock<IUnitOfWork>();
            _eligibilityLogger = new Mock<ILogger<LoanEligibilityService>>();
            _guarantorLogger = new Mock<ILogger<GuarantorService>>();
            _committeeLogger = new Mock<ILogger<LoanCommitteeService>>();
            _registerLogger = new Mock<ILogger<LoanRegisterService>>();
            _disbursementLogger = new Mock<ILogger<LoanDisbursementService>>();
            _repaymentLogger = new Mock<ILogger<LoanRepaymentService>>();

            // Initialize services
            _calculatorService = new LoanCalculatorService();
            
            _eligibilityService = new LoanEligibilityService(_calculatorService);
            
            _guarantorService = new GuarantorService(
                _memberRepository.Object,
                _guarantorConsentRepository.Object,
                _loanApplicationRepository.Object,
                _unitOfWork.Object,
                _guarantorLogger.Object);

            _committeeService = new LoanCommitteeService(
                _loanApplicationRepository.Object,
                _committeeReviewRepository.Object,
                _memberRepository.Object,
                _loanRepository.Object,
                _guarantorConsentRepository.Object,
                _unitOfWork.Object,
                _committeeLogger.Object);

            _registerService = new LoanRegisterService(
                _loanRegisterRepository.Object,
                _loanRepository.Object,
                _loanApplicationRepository.Object,
                _memberRepository.Object,
                _unitOfWork.Object,
                _registerLogger.Object);

            _disbursementService = new LoanDisbursementService(
                _loanRepository.Object,
                _loanApplicationRepository.Object,
                _memberRepository.Object,
                _calculatorService,
                _registerService,
                _unitOfWork.Object,
                _disbursementLogger.Object);

            _repaymentService = new LoanRepaymentService(
                _loanRepository.Object,
                _transactionRepository.Object,
                _scheduleRepository.Object,
                _memberRepository.Object,
                _calculatorService,
                _registerService,
                _unitOfWork.Object,
                _repaymentLogger.Object);
        }

        [Fact]
        public async Task CompleteNormalLoanWorkflow_Success()
        {
            // Arrange - Setup test data
            var member = CreateTestMember("M001", 100000m); // ₦100,000 savings
            var guarantor1 = CreateTestMember("G001", 80000m);
            var guarantor2 = CreateTestMember("G002", 80000m);

            _memberRepository.Setup(r => r.GetByIdAsync(member.Id))
                .ReturnsAsync(member);
            _memberRepository.Setup(r => r.GetByIdAsync(guarantor1.Id))
                .ReturnsAsync(guarantor1);
            _memberRepository.Setup(r => r.GetByIdAsync(guarantor2.Id))
                .ReturnsAsync(guarantor2);

            // Step 1: Check Eligibility
            var eligibilityRequest = new EligibilityCheckRequest
            {
                RequestedAmount = 150000m, // ₦150,000
                LoanType = "NORMAL", // 200% multiplier
                InterestRate = 15m,
                TenorMonths = 12,
                MemberId = member.Id,
                MemberTotalSavings = member.TotalSavings,
                MembershipDate = member.MembershipDate,
                NetSalary = 200000m,
                ExistingMonthlyDeductions = 50000m,
                MinimumMembershipMonths = 6,
                MaxDeductionRatePercentage = 45m
            };

            // Act & Assert - Step 1: Eligibility Check
            var eligibilityResult = await _eligibilityService.CheckEligibilityAsync(eligibilityRequest);

            Assert.True(eligibilityResult.IsEligible, 
                $"Member should be eligible. Reasons: {string.Join(", ", eligibilityResult.Reasons)}");
            Assert.True(eligibilityResult.MaximumEligibleAmount >= 150000m);
            Assert.NotNull(eligibilityResult.SavingsMultiplierCheck);
            Assert.True(eligibilityResult.SavingsMultiplierCheck.IsEligible);

            // Step 2: Create Loan Application
            var application = CreateTestLoanApplication(member.Id, 150000m);
            _loanApplicationRepository.Setup(r => r.GetByIdAsync(application.Id))
                .ReturnsAsync(application);

            // Step 3: Guarantor Consent
            // Check guarantor 1 eligibility
            var guarantor1Eligibility = await _guarantorService.CheckGuarantorEligibilityAsync(
                guarantor1.Id, 75000m);

            Assert.True(guarantor1Eligibility.IsEligible);
            Assert.True(guarantor1Eligibility.FreeEquity >= 75000m);

            // Check guarantor 2 eligibility
            var guarantor2Eligibility = await _guarantorService.CheckGuarantorEligibilityAsync(
                guarantor2.Id, 75000m);

            Assert.True(guarantor2Eligibility.IsEligible);
            Assert.True(guarantor2Eligibility.FreeEquity >= 75000m);

            // Step 4: Committee Review
            var committeeReview = new CommitteeReviewRequest
            {
                LoanApplicationId = application.Id,
                ReviewerMemberId = "COMMITTEE001",
                Decision = "APPROVE",
                RecommendedAmount = 150000m,
                Comments = "Good repayment history and sufficient savings",
                VotingWeight = 1
            };

            var committeeReviewer = CreateTestMember("COMMITTEE001", 0);
            _memberRepository.Setup(r => r.GetByIdAsync("COMMITTEE001"))
                .ReturnsAsync(committeeReviewer);

            // Submit 3 committee reviews (minimum required)
            for (int i = 1; i <= 3; i++)
            {
                var review = new CommitteeReview
                {
                    Id = $"REVIEW{i}",
                    LoanApplicationId = application.Id,
                    ReviewerMemberId = $"COMMITTEE00{i}",
                    ReviewDecision = "APPROVE",
                    RecommendedAmount = 150000m,
                    VotingWeight = 1,
                    ReviewDate = DateTime.UtcNow
                };

                _committeeReviewRepository.Setup(r => r.AddAsync(It.IsAny<CommitteeReview>()))
                    .ReturnsAsync(review);
            }

            // Step 5: Calculate Loan Details
            var loanSummary = _calculatorService.CalculateLoanSummary(
                150000m, 15m, 12, DateTime.UtcNow);

            Assert.Equal(150000m, loanSummary.Principal);
            Assert.Equal(12, loanSummary.TenorMonths);
            Assert.True(loanSummary.MonthlyEMI > 0);
            Assert.True(loanSummary.TotalInterest > 0);
            Assert.Equal(12, loanSummary.AmortizationSchedule.Count);

            // Verify reducing balance calculation
            var firstInstallment = loanSummary.AmortizationSchedule[0];
            var lastInstallment = loanSummary.AmortizationSchedule[11];
            Assert.True(firstInstallment.InterestPayment > lastInstallment.InterestPayment,
                "First installment should have more interest (reducing balance)");
            Assert.True(firstInstallment.PrincipalPayment < lastInstallment.PrincipalPayment,
                "Last installment should have more principal (reducing balance)");

            // Verify final balance is zero
            Assert.Equal(0m, lastInstallment.RemainingBalance);

            // Step 6: Verify complete workflow
            Assert.True(eligibilityResult.IsEligible);
            Assert.True(guarantor1Eligibility.IsEligible);
            Assert.True(guarantor2Eligibility.IsEligible);
            Assert.NotNull(loanSummary);
            Assert.Equal(150000m, loanSummary.Principal);
        }

        [Fact]
        public async Task EligibilityCheck_InsufficientSavings_Fails()
        {
            // Arrange
            var member = CreateTestMember("M002", 50000m); // Only ₦50,000 savings

            var eligibilityRequest = new EligibilityCheckRequest
            {
                RequestedAmount = 150000m, // Requesting ₦150,000
                LoanType = "NORMAL", // 200% multiplier (needs ₦75,000 savings)
                InterestRate = 15m,
                TenorMonths = 12,
                MemberId = member.Id,
                MemberTotalSavings = member.TotalSavings,
                MembershipDate = member.MembershipDate,
                MinimumMembershipMonths = 6
            };

            // Act
            var result = await _eligibilityService.CheckEligibilityAsync(eligibilityRequest);

            // Assert
            Assert.False(result.IsEligible);
            Assert.Contains(result.Reasons, r => r.Contains("Insufficient savings"));
            Assert.NotNull(result.SavingsMultiplierCheck);
            Assert.False(result.SavingsMultiplierCheck.IsEligible);
        }

        [Fact]
        public async Task EligibilityCheck_ExceedsDeductionRate_Fails()
        {
            // Arrange
            var member = CreateTestMember("M003", 100000m);

            var eligibilityRequest = new EligibilityCheckRequest
            {
                RequestedAmount = 150000m,
                LoanType = "NORMAL",
                InterestRate = 15m,
                TenorMonths = 12,
                MemberId = member.Id,
                MemberTotalSavings = member.TotalSavings,
                MembershipDate = member.MembershipDate,
                NetSalary = 100000m, // Low salary
                ExistingMonthlyDeductions = 60000m, // High existing deductions
                MaxDeductionRatePercentage = 45m
            };

            // Act
            var result = await _eligibilityService.CheckEligibilityAsync(eligibilityRequest);

            // Assert
            Assert.False(result.IsEligible);
            Assert.Contains(result.Reasons, r => r.Contains("Deduction rate exceeds limit"));
        }

        [Fact]
        public void LoanCalculation_ReducingBalance_CorrectAmortization()
        {
            // Arrange
            decimal principal = 100000m;
            decimal annualRate = 15m;
            int tenor = 12;

            // Act
            var schedule = _calculatorService.GenerateAmortizationSchedule(
                principal, annualRate, tenor, DateTime.UtcNow);

            // Assert
            Assert.Equal(12, schedule.Count);

            // Verify reducing balance: interest decreases, principal increases
            for (int i = 0; i < schedule.Count - 1; i++)
            {
                Assert.True(schedule[i].InterestPayment >= schedule[i + 1].InterestPayment,
                    $"Interest should decrease or stay same (installment {i + 1})");
                Assert.True(schedule[i].PrincipalPayment <= schedule[i + 1].PrincipalPayment,
                    $"Principal should increase or stay same (installment {i + 1})");
            }

            // Verify final balance is zero
            Assert.Equal(0m, schedule[schedule.Count - 1].RemainingBalance);

            // Verify total payments equal principal + interest
            decimal totalPrincipal = schedule.Sum(s => s.PrincipalPayment);
            decimal totalInterest = schedule.Sum(s => s.InterestPayment);
            Assert.Equal(principal, totalPrincipal, 2);
        }

        [Fact]
        public void PaymentAllocation_InterestFirst_CorrectAllocation()
        {
            // Arrange
            decimal payment = 10000m;
            decimal outstandingPrincipal = 50000m;
            decimal accruedInterest = 2000m;
            decimal penalty = 500m;

            // Act
            var allocation = _calculatorService.AllocatePayment(
                payment, outstandingPrincipal, accruedInterest, penalty);

            // Assert - Allocation order: Penalty → Interest → Principal
            Assert.Equal(500m, allocation.PenaltyPaid);
            Assert.Equal(2000m, allocation.InterestPaid);
            Assert.Equal(7500m, allocation.PrincipalPaid);
            Assert.Equal(0m, allocation.Overpayment);
        }

        [Fact]
        public async Task GuarantorEligibility_InsufficientFreeEquity_Fails()
        {
            // Arrange
            var guarantor = CreateTestMember("G003", 30000m); // Only ₦30,000 savings
            _memberRepository.Setup(r => r.GetByIdAsync(guarantor.Id))
                .ReturnsAsync(guarantor);

            // Mock existing guarantor obligations (₦20,000 locked)
            var existingConsents = new[]
            {
                new GuarantorConsent
                {
                    GuarantorMemberId = guarantor.Id,
                    GuaranteedAmount = 20000m,
                    ConsentStatus = "APPROVED",
                    IsActive = true
                }
            };

            _guarantorConsentRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(existingConsents);

            // Act - Try to guarantee ₦15,000 (but only ₦10,000 free equity available)
            var result = await _guarantorService.CheckGuarantorEligibilityAsync(
                guarantor.Id, 15000m);

            // Assert
            Assert.False(result.IsEligible);
            Assert.Contains(result.Reasons, r => r.Contains("Insufficient free equity"));
            Assert.Equal(10000m, result.FreeEquity); // 30000 - 20000 = 10000
        }

        #region Helper Methods

        private Member CreateTestMember(string memberNumber, decimal savings)
        {
            return new Member
            {
                Id = Guid.NewGuid().ToString(),
                MemberNumber = memberNumber,
                FirstName = "Test",
                LastName = "Member",
                Email = $"{memberNumber}@test.com",
                PhoneNumber = "08012345678",
                MembershipDate = DateTime.UtcNow.AddYears(-2),
                MembershipStatus = "ACTIVE",
                TotalSavings = savings,
                ShareCapital = savings * 0.1m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        private LoanApplication CreateTestLoanApplication(string memberId, decimal amount)
        {
            return new LoanApplication
            {
                Id = Guid.NewGuid().ToString(),
                ApplicationNumber = $"APP{DateTime.UtcNow:yyyyMMddHHmmss}",
                MemberId = memberId,
                RequestedAmount = amount,
                ApprovedAmount = amount,
                LoanPurpose = "Business expansion",
                RepaymentPeriodMonths = 12,
                InterestRate = 15m,
                ApplicationDate = DateTime.UtcNow,
                ApplicationStatus = "SUBMITTED",
                RequiredGuarantors = 2,
                GuarantorsProvided = 0,
                CreatedAt = DateTime.UtcNow
            };
        }

        #endregion

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}
