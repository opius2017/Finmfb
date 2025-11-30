using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using FinTech.Core.Application.Services.Loans;

namespace Fin-Backend.Tests.Services
{
    public class LoanCalculatorServiceTests
    {
        private readonly Mock<ILogger<LoanCalculatorService>> _loggerMock;
        private readonly LoanCalculatorService _service;

        public LoanCalculatorServiceTests()
        {
            _loggerMock = new Mock<ILogger<LoanCalculatorService>>();
            _service = new LoanCalculatorService(_loggerMock.Object);
        }

        [Theory]
        [InlineData(500000, 12, 12, 44424.11)] // 500k at 12% for 12 months
        [InlineData(1000000, 10, 24, 46144.74)] // 1M at 10% for 24 months
        [InlineData(250000, 15, 6, 43597.50)] // 250k at 15% for 6 months
        public void CalculateEMI_WithValidInputs_ReturnsCorrectEMI(
            decimal principal, 
            decimal rate, 
            int tenure, 
            decimal expectedEMI)
        {
            // Act
            var result = _service.CalculateEMI(principal, rate, tenure);

            // Assert
            result.Should().BeApproximately(expectedEMI, 0.01m);
        }

        [Fact]
        public void CalculateEMI_WithZeroInterestRate_ReturnsPrincipalDividedByTenure()
        {
            // Arrange
            decimal principal = 120000;
            decimal rate = 0;
            int tenure = 12;

            // Act
            var result = _service.CalculateEMI(principal, rate, tenure);

            // Assert
            result.Should().Be(10000); // 120000 / 12
        }

        [Theory]
        [InlineData(0, 12, 12)] // Zero principal
        [InlineData(-100000, 12, 12)] // Negative principal
        [InlineData(100000, -5, 12)] // Negative rate
        [InlineData(100000, 12, 0)] // Zero tenure
        [InlineData(100000, 12, -6)] // Negative tenure
        public void CalculateEMI_WithInvalidInputs_ThrowsArgumentException(
            decimal principal, 
            decimal rate, 
            int tenure)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _service.CalculateEMI(principal, rate, tenure));
        }

        [Fact]
        public void GenerateAmortizationSchedule_WithValidInputs_ReturnsCompleteSchedule()
        {
            // Arrange
            var request = new AmortizationScheduleRequest
            {
                Principal = 500000,
                AnnualInterestRate = 12,
                TenureMonths = 12,
                StartDate = new DateTime(2024, 1, 1),
                LoanNumber = "LN-2024-001"
            };

            // Act
            var result = _service.GenerateAmortizationSchedule(request);

            // Assert
            result.Should().NotBeNull();
            result.Installments.Should().HaveCount(12);
            result.Principal.Should().Be(500000);
            result.MonthlyEMI.Should().BeGreaterThan(0);
            result.TotalInterest.Should().BeGreaterThan(0);
            
            // First installment should have opening balance equal to principal
            result.Installments[0].OpeningBalance.Should().Be(500000);
            
            // Last installment should have closing balance of zero
            result.Installments[11].ClosingBalance.Should().Be(0);
            
            // Sum of all principal payments should equal principal
            var totalPrincipal = result.Installments.Sum(i => i.PrincipalAmount);
            totalPrincipal.Should().BeApproximately(500000, 1);
        }

        [Fact]
        public void CalculateTotalInterest_WithValidInputs_ReturnsCorrectInterest()
        {
            // Arrange
            decimal principal = 500000;
            decimal rate = 12;
            int tenure = 12;

            // Act
            var result = _service.CalculateTotalInterest(principal, rate, tenure);

            // Assert
            result.Should().BeGreaterThan(0);
            result.Should().BeLessThan(principal); // Interest should be less than principal for 1 year
        }

        [Theory]
        [InlineData(50000, 30, 0.1, 150)] // 50k overdue for 30 days at 0.1% per day
        [InlineData(100000, 15, 0.05, 75)] // 100k overdue for 15 days at 0.05% per day
        [InlineData(25000, 60, 0.2, 300)] // 25k overdue for 60 days at 0.2% per day
        public void CalculatePenalty_WithValidInputs_ReturnsCorrectPenalty(
            decimal overdueAmount,
            int daysOverdue,
            decimal penaltyRate,
            decimal expectedPenalty)
        {
            // Act
            var result = _service.CalculatePenalty(overdueAmount, daysOverdue, penaltyRate);

            // Assert
            result.Should().BeApproximately(expectedPenalty, 0.01m);
        }

        [Fact]
        public void CalculatePenalty_WithZeroOrNegativeInputs_ReturnsZero()
        {
            // Act & Assert
            _service.CalculatePenalty(0, 30, 0.1m).Should().Be(0);
            _service.CalculatePenalty(50000, 0, 0.1m).Should().Be(0);
            _service.CalculatePenalty(50000, 30, 0).Should().Be(0);
            _service.CalculatePenalty(-50000, 30, 0.1m).Should().Be(0);
        }

        [Fact]
        public void CalculateEarlyRepayment_WithFullPayment_ReturnsLoanFullyPaid()
        {
            // Arrange
            var request = new EarlyRepaymentRequest
            {
                OutstandingPrincipal = 300000,
                AnnualInterestRate = 12,
                RemainingTenureMonths = 6,
                EarlyRepaymentAmount = 300000,
                RepaymentDate = DateTime.UtcNow
            };

            // Act
            var result = _service.CalculateEarlyRepayment(request);

            // Assert
            result.Should().NotBeNull();
            result.LoanFullyPaid.Should().BeTrue();
            result.NewOutstandingBalance.Should().Be(0);
            result.InterestSaved.Should().BeGreaterThan(0);
        }

        [Fact]
        public void CalculateEarlyRepayment_WithPartialPayment_ReturnsReducedBalance()
        {
            // Arrange
            var request = new EarlyRepaymentRequest
            {
                OutstandingPrincipal = 300000,
                AnnualInterestRate = 12,
                RemainingTenureMonths = 6,
                EarlyRepaymentAmount = 150000,
                RepaymentDate = DateTime.UtcNow
            };

            // Act
            var result = _service.CalculateEarlyRepayment(request);

            // Assert
            result.Should().NotBeNull();
            result.LoanFullyPaid.Should().BeFalse();
            result.NewOutstandingBalance.Should().Be(150000);
            result.InterestSaved.Should().BeGreaterThan(0);
            result.NewMonthlyEMI.Should().BeGreaterThan(0);
        }

        [Fact]
        public void ValidateLoanParameters_WithValidInputs_ReturnsValid()
        {
            // Act
            var result = _service.ValidateLoanParameters(500000, 12, 12);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData(0, 12, 12, "Principal must be greater than zero")]
        [InlineData(-100000, 12, 12, "Principal must be greater than zero")]
        [InlineData(100000, -5, 12, "Interest rate cannot be negative")]
        [InlineData(100000, 150, 12, "Interest rate exceeds maximum")]
        [InlineData(100000, 12, 0, "Tenure must be greater than zero")]
        [InlineData(100000, 12, 400, "Tenure exceeds maximum")]
        public void ValidateLoanParameters_WithInvalidInputs_ReturnsInvalid(
            decimal principal,
            decimal rate,
            int tenure,
            string expectedError)
        {
            // Act
            var result = _service.ValidateLoanParameters(principal, rate, tenure);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Contains(expectedError, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void CalculateOutstandingBalance_AfterNoPayments_ReturnsPrincipal()
        {
            // Act
            var result = _service.CalculateOutstandingBalance(500000, 12, 12, 0);

            // Assert
            result.Should().Be(500000);
        }

        [Fact]
        public void CalculateOutstandingBalance_AfterAllPayments_ReturnsZero()
        {
            // Act
            var result = _service.CalculateOutstandingBalance(500000, 12, 12, 12);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void CalculateOutstandingBalance_AfterSomePayments_ReturnsReducedBalance()
        {
            // Act
            var result = _service.CalculateOutstandingBalance(500000, 12, 12, 6);

            // Assert
            result.Should().BeGreaterThan(0);
            result.Should().BeLessThan(500000);
        }
    }
}
