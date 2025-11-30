using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Application.Services.Loans;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace FinTech.Tests.Integration
{
    /// <summary>
    /// Performance integration tests
    /// Tests system performance under load
    /// </summary>
    public class PerformanceIntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;
        private readonly ITestOutputHelper _output;
        private readonly IServiceScope _scope;

        public PerformanceIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
            _scope = _fixture.ServiceProvider.CreateScope();
        }

        [Fact]
        public async Task LoanCalculation_Performance_ShouldHandleMultipleCalculations()
        {
            // Arrange
            var calculatorService = _scope.ServiceProvider.GetRequiredService<ILoanCalculatorService>();
            const int iterations = 1000;
            var testCases = new[]
            {
                new { Principal = 100000m, Rate = 15m, Tenor = 12 },
                new { Principal = 500000m, Rate = 18m, Tenor = 24 },
                new { Principal = 1000000m, Rate = 12m, Tenor = 36 },
                new { Principal = 2000000m, Rate = 20m, Tenor = 48 }
            };

            _output.WriteLine($"Performance test: {iterations} calculations per test case");

            foreach (var testCase in testCases)
            {
                var stopwatch = Stopwatch.StartNew();
        
        
                // Act
                var tasks = new List<Task>();
                for (int i = 0; i < iterations; i++)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        var emi = calculatorService.CalculateEMI(testCase.Principal, testCase.Rate, testCase.Tenor);
                        var schedule = calculatorService.GenerateAmortizationSchedule(
                            testCase.Principal, testCase.Rate, testCase.Tenor, DateTime.UtcNow);
                        return (emi, schedule);
                    }));
                }

                await Task.WhenAll(tasks);
                stopwatch.Stop();

                // Assert
                var avgTimeMs = stopwatch.ElapsedMilliseconds / (double)iterations;
                Assert.True(avgTimeMs < 10, $"Average calculation time should be < 10ms, was {avgTimeMs:F2}ms");

                _output.WriteLine($"✓ Principal: ₦{testCase.Principal:N0}, Rate: {testCase.Rate}%, Tenor: {testCase.Tenor}m");
                _output.WriteLine($"  Total time: {stopwatch.ElapsedMilliseconds}ms, Avg: {avgTimeMs:F2}ms per calculation");
            }
        }

        [Fact]
        public async Task EligibilityCheck_Performance_ShouldHandleConcurrentRequests()
        {
            // Arrange
            var eligibilityService = _scope.ServiceProvider.GetRequiredService<ILoanEligibilityService>();
            const int concurrentRequests = 100;
            
            var baseRequest = new EligibilityCheckRequest
            {
                RequestedAmount = 200000m,
                LoanType = "NORMAL",
                InterestRate = 15m,
                TenorMonths = 12,
                MemberId = "perf-test-member",
                MemberTotalSavings = 150000m,
                MembershipDate = DateTime.UtcNow.AddYears(-2),
                NetSalary = 300000m,
                ExistingMonthlyDeductions = 50000m
            };

            _output.WriteLine($"Performance test: {concurrentRequests} concurrent eligibility checks");

            var stopwatch = Stopwatch.StartNew();

            // Act
            var tasks = Enumerable.Range(0, concurrentRequests)
                .Select(async i =>
                {
                    var request = new EligibilityCheckRequest
                    {
                        RequestedAmount = baseRequest.RequestedAmount + (i * 1000), // Vary amounts
                        LoanType = baseRequest.LoanType,
                        InterestRate = baseRequest.InterestRate,
                        TenorMonths = baseRequest.TenorMonths,
                        MemberId = $"{baseRequest.MemberId}-{i}",
                        MemberTotalSavings = baseRequest.MemberTotalSavings,
                        MembershipDate = baseRequest.MembershipDate,
                        NetSalary = baseRequest.NetSalary,
                        ExistingMonthlyDeductions = baseRequest.ExistingMonthlyDeductions
                    };

                    return await eligibilityService.CheckEligibilityAsync(request);
                })
                .ToList();

            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            var avgTimeMs = stopwatch.ElapsedMilliseconds / (double)concurrentRequests;
            Assert.True(avgTimeMs < 50, $"Average eligibility check should be < 50ms, was {avgTimeMs:F2}ms");
            Assert.Equal(concurrentRequests, results.Length);

            _output.WriteLine($"✓ Total time: {stopwatch.ElapsedMilliseconds}ms");
            _output.WriteLine($"✓ Average time per check: {avgTimeMs:F2}ms");
            _output.WriteLine($"✓ Eligible requests: {results.Count(r => r.IsEligible)}");
        }

        [Fact]
        public async Task AmortizationSchedule_Performance_LargeTenor()
        {
            // Arrange
            var calculatorService = _scope.ServiceProvider.GetRequiredService<ILoanCalculatorService>();
            decimal principal = 5000000m; // ₦5M
            decimal interestRate = 15m;
            int tenorMonths = 120; // 10 years

            _output.WriteLine($"Performance test: Generating {tenorMonths}-month amortization schedule");

            var stopwatch = Stopwatch.StartNew();

            // Act
            var schedule = calculatorService.GenerateAmortizationSchedule(
                principal, interestRate, tenorMonths, DateTime.UtcNow);

            stopwatch.Stop();

            // Assert
            Assert.Equal(tenorMonths, schedule.Count);
            Assert.Equal(0m, schedule.Last().RemainingBalance);
            Assert.True(stopwatch.ElapsedMilliseconds < 100, 
                $"Schedule generation should be < 100ms, was {stopwatch.ElapsedMilliseconds}ms");

            _output.WriteLine($"✓ Generated {schedule.Count} installments in {stopwatch.ElapsedMilliseconds}ms");
            _output.WriteLine($"✓ First installment - Interest: ₦{schedule[0].InterestPayment:N2}, Principal: ₦{schedule[0].PrincipalPayment:N2}");
            _output.WriteLine($"✓ Last installment - Interest: ₦{schedule[^1].InterestPayment:N2}, Principal: ₦{schedule[^1].PrincipalPayment:N2}");
        }

        [Fact]
        public async Task BulkCalculations_Performance_MultipleLoans()
        {
            // Arrange
            var calculatorService = _scope.ServiceProvider.GetRequiredService<ILoanCalculatorService>();
            const int loanCount = 500;

            var loans = Enumerable.Range(1, loanCount)
                .Select(i => new
                {
                    Principal = 100000m + (i * 10000m),
                    Rate = 12m + (i % 10),
                    Tenor = 12 + (i % 36)
                })
                .ToList();

            _output.WriteLine($"Performance test: Calculating {loanCount} loans");

            var stopwatch = Stopwatch.StartNew();

            // Act
            var tasks = loans.Select(loan => Task.Run(() =>
            {
                var emi = calculatorService.CalculateEMI(loan.Principal, loan.Rate, loan.Tenor);
                var totalInterest = calculatorService.CalculateTotalInterest(loan.Principal, loan.Rate, loan.Tenor);
                var totalRepayable = calculatorService.CalculateTotalRepayable(loan.Principal, loan.Rate, loan.Tenor);
                var schedule = calculatorService.GenerateAmortizationSchedule(
                    loan.Principal, loan.Rate, loan.Tenor, DateTime.UtcNow);
                
                return new { EMI = emi, TotalInterest = totalInterest, TotalRepayable = totalRepayable, Schedule = schedule };
            })).ToList();

            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            Assert.Equal(loanCount, results.Length);
            var avgTimeMs = stopwatch.ElapsedMilliseconds / (double)loanCount;
            Assert.True(avgTimeMs < 20, $"Average calculation time should be < 20ms, was {avgTimeMs:F2}ms");

            _output.WriteLine($"✓ Processed {loanCount} loans in {stopwatch.ElapsedMilliseconds}ms");
            _output.WriteLine($"✓ Average time per loan: {avgTimeMs:F2}ms");
            _output.WriteLine($"✓ Throughput: {loanCount / (stopwatch.ElapsedMilliseconds / 1000.0):F0} loans/second");
        }

        [Fact]
        public async Task PaymentAllocation_Performance_BulkProcessing()
        {
            // Arrange
            var calculatorService = _scope.ServiceProvider.GetRequiredService<ILoanCalculatorService>();
            const int paymentCount = 1000;

            var payments = Enumerable.Range(1, paymentCount)
                .Select(i => new
                {
                    Payment = 5000m + (i * 100m),
                    Principal = 100000m,
                    Interest = 2000m,
                    Penalty = 500m
                })
                .ToList();

            _output.WriteLine($"Performance test: Processing {paymentCount} payment allocations");

            var stopwatch = Stopwatch.StartNew();

            // Act
            var tasks = payments.Select(p => Task.Run(() =>
                calculatorService.AllocatePayment(p.Payment, p.Principal, p.Interest, p.Penalty)
            )).ToList();

            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            Assert.Equal(paymentCount, results.Length);
            var avgTimeMs = stopwatch.ElapsedMilliseconds / (double)paymentCount;
            Assert.True(avgTimeMs < 5, $"Average allocation time should be < 5ms, was {avgTimeMs:F2}ms");

            _output.WriteLine($"✓ Processed {paymentCount} allocations in {stopwatch.ElapsedMilliseconds}ms");
            _output.WriteLine($"✓ Average time per allocation: {avgTimeMs:F2}ms");
        }

        [Fact]
        public async Task ComplexScenario_Performance_EndToEndCalculation()
        {
            // Arrange
            var calculatorService = _scope.ServiceProvider.GetRequiredService<ILoanCalculatorService>();
            var eligibilityService = _scope.ServiceProvider.GetRequiredService<ILoanEligibilityService>();
            
            const int scenarios = 100;

            _output.WriteLine($"Performance test: {scenarios} end-to-end loan scenarios");

            var stopwatch = Stopwatch.StartNew();

            // Act
            var tasks = Enumerable.Range(1, scenarios).Select(async i =>
            {
                // Step 1: Eligibility check
                var eligibilityRequest = new EligibilityCheckRequest
                {
                    RequestedAmount = 100000m + (i * 5000m),
                    LoanType = i % 3 == 0 ? "CAR" : i % 2 == 0 ? "COMMODITY" : "NORMAL",
                    InterestRate = 12m + (i % 8),
                    TenorMonths = 12 + (i % 24),
                    MemberId = $"member-{i}",
                    MemberTotalSavings = 50000m + (i * 2000m),
                    MembershipDate = DateTime.UtcNow.AddYears(-2),
                    NetSalary = 200000m,
                    ExistingMonthlyDeductions = 30000m
                };

                var eligibility = await eligibilityService.CheckEligibilityAsync(eligibilityRequest);

                if (!eligibility.IsEligible)
                    return null;

                // Step 2: Calculate loan details
                var emi = calculatorService.CalculateEMI(
                    eligibilityRequest.RequestedAmount, 
                    eligibilityRequest.InterestRate, 
                    eligibilityRequest.TenorMonths);

                var schedule = calculatorService.GenerateAmortizationSchedule(
                    eligibilityRequest.RequestedAmount,
                    eligibilityRequest.InterestRate,
                    eligibilityRequest.TenorMonths,
                    DateTime.UtcNow);

                var summary = calculatorService.CalculateLoanSummary(
                    eligibilityRequest.RequestedAmount,
                    eligibilityRequest.InterestRate,
                    eligibilityRequest.TenorMonths,
                    DateTime.UtcNow);

                return new { Eligibility = eligibility, EMI = emi, Schedule = schedule, Summary = summary };
            }).ToList();

            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            var successfulScenarios = results.Count(r => r != null);
            var avgTimeMs = stopwatch.ElapsedMilliseconds / (double)scenarios;
            
            Assert.True(avgTimeMs < 100, $"Average scenario time should be < 100ms, was {avgTimeMs:F2}ms");
            Assert.True(successfulScenarios > 0, "At least some scenarios should be eligible");

            _output.WriteLine($"✓ Processed {scenarios} scenarios in {stopwatch.ElapsedMilliseconds}ms");
            _output.WriteLine($"✓ Average time per scenario: {avgTimeMs:F2}ms");
            _output.WriteLine($"✓ Successful scenarios: {successfulScenarios}/{scenarios}");
            _output.WriteLine($"✓ Throughput: {scenarios / (stopwatch.ElapsedMilliseconds / 1000.0):F0} scenarios/second");
        }

        public void Dispose()
        {
            _scope?.Dispose();
        }
    }
}
