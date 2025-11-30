using System;
using System.Linq;
using Xunit;
using FinTech.Core.Application.Services.Loans;
using FinTech.Core.Application.DTOs.Loans;

namespace FinTech.Tests.Services
{
    /// <summary>
    /// Unit tests for LoanCalculatorService
    /// Ensures accuracy of all loan calculations using reducing balance method
    /// </summary>
    public class LoanCalculatorServiceTests
    {
        private readonly ILoanCalculatorService _calculator;

        public LoanCalculatorServiceTests()
        {
            _calculator = new LoanCalculatorService();
        }

        #region EMI Calculation Tests

        [Fact]
        public void CalculateEMI_WithValidInputs_ReturnsCorrectEMI()
        {
            // Arrange
            decimal principal = 100000m;
            decimal annualRate = 12m; // 12% per annum
            int tenor = 12; // 12 months

            // Act
            decimal emi = _calculator.CalculateEMI(principal, annualRate, tenor);

            // Assert
            // Expected EMI ≈ ₦8,884.88 (using reducing balance formula)
            Assert.InRange(emi, 8884m, 8885m);
        }

        [Fact]
        public void CalculateEMI_WithZeroInterestRate_ReturnsSimpleDivision()
        {
            // Arrange
            decimal principal = 120000m;
            decimal annualRate = 0m;
            int tenor = 12;

            // Act
            decimal emi = _calculator.CalculateEMI(principal, annualRate, tenor);

            // Assert
            Assert.Equal(10000m, emi); // 120000 / 12 = 10000
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1000)]
        public void CalculateEMI_WithInvalidPrincipal_ThrowsException(decimal principal)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _calculator.CalculateEMI(principal, 12m, 12));
        }

        [Fact]
        public void CalculateEMI_WithNegativeInterestRate_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _calculator.CalculateEMI(100000m, -5m, 12));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-12)]
        public void CalculateEMI_WithInvalidTenor_ThrowsException(int tenor)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                _calculator.CalculateEMI(100000m, 12m, tenor));
        }

        #endregion

        #region Total Interest Calculation Tests

        [Fact]
        public void CalculateTotalInterest_WithValidInputs_ReturnsCorrectInterest()
        {
            // Arrange
            decimal principal = 100000m;
            decimal annualRate = 12m;
            int tenor = 12;

            // Act
            decimal totalInterest = _calculator.CalculateTotalInterest(principal, annualRate, tenor);

            // Assert
            // Total repayable ≈ ₦106,618.56, so interest ≈ ₦6,618.56
            Assert.InRange(totalInterest, 6600m, 6650m);
        }

        [Fact]
        public void CalculateTotalInterest_WithZeroRate_ReturnsZero()
        {
            // Arrange
            decimal principal = 100000m;
            decimal annualRate = 0m;
            int tenor = 12;

            // Act
            decimal totalInterest = _calculator.CalculateTotalInterest(principal, annualRate, tenor);

            // Assert
            Assert.Equal(0m, totalInterest);
        }

        #endregion

        #region Total Repayable Calculation Tests

        [Fact]
        public void CalculateTotalRepayable_WithValidInputs_ReturnsCorrectTotal()
        {
            // Arrange
            decimal principal = 100000m;
            decimal annualRate = 12m;
            int tenor = 12;

            // Act
            decimal totalRepayable = _calculator.CalculateTotalRepayable(principal, annualRate, tenor);

            // Assert
            // Expected ≈ ₦106,618.56
            Assert.InRange(totalRepayable, 106600m, 106650m);
        }

        #endregion

        #region Amortization Schedule Tests

        [Fact]
        public void GenerateAmortizationSchedule_CreatesCorrectNumberOfInstallments()
        {
            // Arrange
            decimal principal = 100000m;
            decimal annualRate = 12m;
            int tenor = 12;
            DateTime startDate = new DateTime(2024, 1, 1);

            // Act
            var schedule = _calculator.GenerateAmortizationSchedule(principal, annualRate, tenor, startDate);

            // Assert
            Assert.Equal(tenor, schedule.Count);
        }

        [Fact]
        public void GenerateAmortizationSchedule_FirstInstallmentHasHigherInterest()
        {
            // Arrange
            decimal principal = 100000m;
            decimal annualRate = 12m;
            int tenor = 12;
            DateTime startDate = new DateTime(2024, 1, 1);

            // Act
            var schedule = _calculator.GenerateAmortizationSchedule(principal, annualRate, tenor, startDate);

            // Assert
            var firstInstallment = schedule.First();
            var lastInstallment = schedule.Last();
            
            // First installment should have more interest, less principal
            Assert.True(firstInstallment.InterestPayment > lastInstallment.InterestPayment);
            Assert.True(firstInstallment.PrincipalPayment < lastInstallment.PrincipalPayment);
        }

        [Fact]
        public void GenerateAmortizationSchedule_FinalBalanceIsZero()
        {
            // Arrange
            decimal principal = 100000m;
            decimal annualRate = 12m;
            int tenor = 12;
            DateTime startDate = new DateTime(2024, 1, 1);

            // Act
            var schedule = _calculator.GenerateAmortizationSchedule(principal, annualRate, tenor, startDate);

            // Assert
            Assert.Equal(0m, schedule.Last().RemainingBalance);
        }

        [Fact]
        public void GenerateAmortizationSchedule_DueDatesAreCorrect()
        {
            // Arrange
            decimal principal = 100000m;
            decimal annualRate = 12m;
            int tenor = 6;
            DateTime startDate = new DateTime(2024, 1, 1);

            // Act
            var schedule = _calculator.GenerateAmortizationSchedule(principal, annualRate, tenor, startDate);

            // Assert
            Assert.Equal(new DateTime(2024, 2, 1), schedule[0].DueDate);
            Assert.Equal(new DateTime(2024, 3, 1), schedule[1].DueDate);
            Assert.Equal(new DateTime(2024, 7, 1), schedule[5].DueDate);
        }

        [Fact]
        public void GenerateAmortizationSchedule_AllInstallmentsHaveSameEMI()
        {
            // Arrange
            decimal principal = 100000m;
            decimal annualRate = 12m;
            int tenor = 12;
            DateTime startDate = new DateTime(2024, 1, 1);

            // Act
            var schedule = _calculator.GenerateAmortizationSchedule(principal, annualRate, tenor, startDate);

            // Assert
            decimal firstEMI = schedule.First().EMI;
            Assert.All(schedule, item => Assert.Equal(firstEMI, item.EMI));
        }

        #endregion

        #region Penalty Calculation Tests

        [Fact]
        public void CalculatePenalty_WithValidInputs_ReturnsCorrectPenalty()
        {
            // Arrange
            decimal overdueAmount = 10000m;
            int daysOverdue = 10;
            decimal penaltyRatePerDay = 0.1m; // 0.1% per day

            // Act
            decimal penalty = _calculator.CalculatePenalty(overdueAmount, daysOverdue, penaltyRatePerDay);

            // Assert
            // 10000 * 0.001 * 10 = 100
            Assert.Equal(100m, penalty);
        }

        [Theory]
        [InlineData(0, 10, 0.1)]
        [InlineData(10000, 0, 0.1)]
        [InlineData(-1000, 10, 0.1)]
        [InlineData(10000, -5, 0.1)]
        public void CalculatePenalty_WithInvalidInputs_ReturnsZero(decimal amount, int days, decimal rate)
        {
            // Act
            decimal penalty = _calculator.CalculatePenalty(amount, days, rate);

            // Assert
            Assert.Equal(0m, penalty);
        }

        #endregion

        #region Early Repayment Tests

        [Fact]
        public void CalculateEarlyRepayment_ReturnsCorrectAmount()
        {
            // Arrange
            decimal originalPrincipal = 100000m;
            decimal principalPaid = 30000m;
            decimal annualRate = 12m;
            DateTime disbursementDate = new DateTime(2024, 1, 1);
            DateTime repaymentDate = new DateTime(2024, 7, 1); // 6 months later

            // Act
            var result = _calculator.CalculateEarlyRepayment(
                originalPrincipal, principalPaid, annualRate, disbursementDate, repaymentDate);

            // Assert
            Assert.Equal(70000m, result.OutstandingPrincipal);
            Assert.True(result.AccruedInterest > 0);
            Assert.Equal(result.OutstandingPrincipal + result.AccruedInterest, result.TotalEarlyRepaymentAmount);
        }

        #endregion

        #region Payment Allocation Tests

        [Fact]
        public void AllocatePayment_AllocatesToPenaltyFirst()
        {
            // Arrange
            decimal payment = 5000m;
            decimal principal = 100000m;
            decimal interest = 1000m;
            decimal penalty = 500m;

            // Act
            var allocation = _calculator.AllocatePayment(payment, principal, interest, penalty);

            // Assert
            Assert.Equal(500m, allocation.PenaltyPaid);
            Assert.Equal(1000m, allocation.InterestPaid);
            Assert.Equal(3500m, allocation.PrincipalPaid);
            Assert.Equal(0m, allocation.Overpayment);
        }

        [Fact]
        public void AllocatePayment_AllocatesToInterestSecond()
        {
            // Arrange
            decimal payment = 1500m;
            decimal principal = 100000m;
            decimal interest = 1000m;
            decimal penalty = 500m;

            // Act
            var allocation = _calculator.AllocatePayment(payment, principal, interest, penalty);

            // Assert
            Assert.Equal(500m, allocation.PenaltyPaid);
            Assert.Equal(1000m, allocation.InterestPaid);
            Assert.Equal(0m, allocation.PrincipalPaid);
            Assert.Equal(0m, allocation.Overpayment);
        }

        [Fact]
        public void AllocatePayment_HandlesOverpayment()
        {
            // Arrange
            decimal payment = 150000m;
            decimal principal = 100000m;
            decimal interest = 1000m;
            decimal penalty = 500m;

            // Act
            var allocation = _calculator.AllocatePayment(payment, principal, interest, penalty);

            // Assert
            Assert.Equal(500m, allocation.PenaltyPaid);
            Assert.Equal(1000m, allocation.InterestPaid);
            Assert.Equal(100000m, allocation.PrincipalPaid);
            Assert.Equal(48500m, allocation.Overpayment);
        }

        #endregion

        #region Deduction Rate Impact Tests

        [Fact]
        public void CalculateDeductionRateImpact_WithinLimit_ReturnsTrue()
        {
            // Arrange
            decimal netSalary = 200000m;
            decimal existingDeductions = 50000m;
            decimal proposedEMI = 30000m;
            decimal maxRate = 45m; // 45%

            // Act
            var impact = _calculator.CalculateDeductionRateImpact(
                netSalary, existingDeductions, proposedEMI, maxRate);

            // Assert
            Assert.Equal(40m, impact.DeductionRatePercentage); // (50000 + 30000) / 200000 = 40%
            Assert.True(impact.IsWithinLimit);
            Assert.Equal(10000m, impact.RemainingHeadroom); // 90000 - 80000 = 10000
        }

        [Fact]
        public void CalculateDeductionRateImpact_ExceedsLimit_ReturnsFalse()
        {
            // Arrange
            decimal netSalary = 200000m;
            decimal existingDeductions = 70000m;
            decimal proposedEMI = 30000m;
            decimal maxRate = 45m; // 45%

            // Act
            var impact = _calculator.CalculateDeductionRateImpact(
                netSalary, existingDeductions, proposedEMI, maxRate);

            // Assert
            Assert.Equal(50m, impact.DeductionRatePercentage); // (70000 + 30000) / 200000 = 50%
            Assert.False(impact.IsWithinLimit);
            Assert.True(impact.RemainingHeadroom < 0);
        }

        #endregion

        #region Loan Summary Tests

        [Fact]
        public void CalculateLoanSummary_ReturnsCompleteInformation()
        {
            // Arrange
            decimal principal = 100000m;
            decimal annualRate = 12m;
            int tenor = 12;
            DateTime startDate = new DateTime(2024, 1, 1);

            // Act
            var summary = _calculator.CalculateLoanSummary(principal, annualRate, tenor, startDate);

            // Assert
            Assert.Equal(principal, summary.Principal);
            Assert.Equal(annualRate, summary.AnnualInterestRate);
            Assert.Equal(tenor, summary.TenorMonths);
            Assert.True(summary.MonthlyEMI > 0);
            Assert.True(summary.TotalInterest > 0);
            Assert.Equal(summary.Principal + summary.TotalInterest, summary.TotalRepayable);
            Assert.Equal(tenor, summary.AmortizationSchedule.Count);
            Assert.Equal(new DateTime(2024, 2, 1), summary.FirstPaymentDate);
            Assert.Equal(new DateTime(2025, 1, 1), summary.LastPaymentDate);
        }

        #endregion

        #region Real-World Scenario Tests

        [Fact]
        public void RealWorldScenario_NormalLoan_200KFor12Months()
        {
            // Arrange - Normal loan: ₦200,000 at 15% for 12 months
            decimal principal = 200000m;
            decimal annualRate = 15m;
            int tenor = 12;
            DateTime startDate = DateTime.Today;

            // Act
            var summary = _calculator.CalculateLoanSummary(principal, annualRate, tenor, startDate);

            // Assert
            Assert.InRange(summary.MonthlyEMI, 18000m, 18500m);
            Assert.InRange(summary.TotalInterest, 16000m, 17000m);
            Assert.Equal(12, summary.AmortizationSchedule.Count);
            Assert.Equal(0m, summary.AmortizationSchedule.Last().RemainingBalance);
        }

        [Fact]
        public void RealWorldScenario_CommodityLoan_500KFor24Months()
        {
            // Arrange - Commodity loan: ₦500,000 at 10% for 24 months
            decimal principal = 500000m;
            decimal annualRate = 10m;
            int tenor = 24;
            DateTime startDate = DateTime.Today;

            // Act
            var summary = _calculator.CalculateLoanSummary(principal, annualRate, tenor, startDate);

            // Assert
            Assert.InRange(summary.MonthlyEMI, 23000m, 24000m);
            Assert.InRange(summary.TotalInterest, 52000m, 55000m);
            Assert.Equal(24, summary.AmortizationSchedule.Count);
        }

        [Fact]
        public void RealWorldScenario_CarLoan_2MFor36Months()
        {
            // Arrange - Car loan: ₦2,000,000 at 18% for 36 months
            decimal principal = 2000000m;
            decimal annualRate = 18m;
            int tenor = 36;
            DateTime startDate = DateTime.Today;

            // Act
            var summary = _calculator.CalculateLoanSummary(principal, annualRate, tenor, startDate);

            // Assert
            Assert.InRange(summary.MonthlyEMI, 72000m, 73000m);
            Assert.InRange(summary.TotalInterest, 590000m, 620000m);
            Assert.Equal(36, summary.AmortizationSchedule.Count);
        }

        #endregion
    }
}
