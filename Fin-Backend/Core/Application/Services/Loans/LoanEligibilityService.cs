using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Domain.Entities;
using FinTech.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;

using FinTech.Core.Application.Interfaces.Loans;

namespace FinTech.Core.Application.Services.Loans
{
    /// <summary>
    /// Implementation of loan eligibility service for cooperative loans
    /// </summary>
    public class LoanEligibilityService : ILoanEligibilityService
    {
        private readonly IRepository<Member> _memberRepository;
        private readonly IRepository<LoanProduct> _loanProductRepository;
        private readonly IRepository<Loan> _loanRepository;
        private readonly ILoanCalculatorService _calculatorService;
        private readonly ILogger<LoanEligibilityService> _logger;

        // Default configuration values (should be moved to configuration)
        private const decimal DEFAULT_MAX_DEDUCTION_RATE = 50.0m; // 50% of salary
        private const decimal DEFAULT_MAX_DEBT_TO_INCOME_RATIO = 60.0m; // 60%
        private const int DEFAULT_MIN_MEMBERSHIP_MONTHS = 6;

        public LoanEligibilityService(
            IRepository<Member> memberRepository,
            IRepository<LoanProduct> loanProductRepository,
            IRepository<Loan> loanRepository,
            ILoanCalculatorService calculatorService,
            ILogger<LoanEligibilityService> logger)
        {
            _memberRepository = memberRepository;
            _loanProductRepository = loanProductRepository;
            _loanRepository = loanRepository;
            _calculatorService = calculatorService;
            _logger = logger;
        }

        /// <summary>
        /// Check complete eligibility for a loan application
        /// </summary>
        public async Task<LoanEligibilityResultDto> CheckEligibilityAsync(LoanEligibilityRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Checking eligibility for member {MemberId}, product {ProductId}, amount ₦{Amount:N2}",
                    request.MemberId, request.LoanProductId, request.RequestedAmount);

                var member = await _memberRepository.GetByIdAsync(request.MemberId);
                if (member == null)
                {
                    throw new InvalidOperationException($"Member {request.MemberId} not found");
                }

                var loanProduct = await _loanProductRepository.GetByIdAsync(request.LoanProductId);
                if (loanProduct == null)
                {
                    throw new InvalidOperationException($"Loan product {request.LoanProductId} not found");
                }

                var result = new LoanEligibilityResultDto
                {
                    MemberId = member.Id,
                    MemberNumber = member.MemberNumber,
                    MemberName = $"{member.FirstName} {member.LastName}",
                    LoanProductId = loanProduct.Id,
                    LoanProductName = loanProduct.Name,
                    RequestedAmount = request.RequestedAmount,
                    TenureMonths = request.TenureMonths,
                    CheckedAt = DateTime.UtcNow
                };

                // Perform all eligibility checks
                result.SavingsMultiplierCheck = await CheckSavingsMultiplierAsync(
                    request.MemberId, 
                    request.RequestedAmount, 
                    request.LoanProductId);

                result.MembershipDurationCheck = await CheckMembershipDurationAsync(
                    request.MemberId, 
                    // FinTech Best Practice: MinimumMembershipMonths is non-nullable int
                    loanProduct.MinimumMembershipMonths > 0 ? loanProduct.MinimumMembershipMonths : DEFAULT_MIN_MEMBERSHIP_MONTHS);

                result.DeductionRateHeadroom = await CalculateDeductionRateHeadroomAsync(
                    request.MemberId,
                    request.RequestedAmount,
                    request.TenureMonths,
                    loanProduct.InterestRate);

                result.DebtToIncomeRatio = await CheckDebtToIncomeRatioAsync(
                    request.MemberId,
                    request.RequestedAmount,
                    request.TenureMonths,
                    loanProduct.InterestRate);

                // Calculate maximum eligible amount
                var maxEligible = await CalculateMaximumEligibleAmountAsync(request.MemberId, request.LoanProductId);
                result.MaximumEligibleAmount = maxEligible.MaximumAmount;

                // Determine overall eligibility
                result.IsEligible = 
                    result.SavingsMultiplierCheck.Passed &&
                    result.MembershipDurationCheck.Passed &&
                    result.DeductionRateHeadroom.Passed &&
                    result.DebtToIncomeRatio.Passed &&
                    request.RequestedAmount <= result.MaximumEligibleAmount;

                // Build criteria list
                result.EligibilityCriteria = BuildEligibilityCriteria(result);
                result.FailureReasons = BuildFailureReasons(result);

                _logger.LogInformation(
                    "Eligibility check completed for member {MemberId}: Eligible={IsEligible}, MaxAmount=₦{MaxAmount:N2}",
                    request.MemberId, result.IsEligible, result.MaximumEligibleAmount);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking eligibility for member {MemberId}", request.MemberId);
                throw;
            }
        }

        /// <summary>
        /// Check savings multiplier eligibility
        /// </summary>
        public async Task<SavingsMultiplierCheckDto> CheckSavingsMultiplierAsync(
            string memberId, 
            decimal requestedAmount, 
            string loanProductId)
        {
            try
            {
                var member = await _memberRepository.GetByIdAsync(memberId);
                if (member == null)
                {
                    throw new InvalidOperationException($"Member {memberId} not found");
                }

                var loanProduct = await _loanProductRepository.GetByIdAsync(loanProductId);
                if (loanProduct == null)
                {
                    throw new InvalidOperationException($"Loan product {loanProductId} not found");
                }

                // Get savings multiplier from loan product (e.g., 2.0 for 200%, 3.0 for 300%, 5.0 for 500%)
                // FinTech Best Practice: SavingsMultiplier is non-nullable decimal
                decimal savingsMultiplier = loanProduct.SavingsMultiplier > 0 ? loanProduct.SavingsMultiplier : 2.0m;
                decimal requiredSavings = requestedAmount / savingsMultiplier;
                decimal actualMultiplier = member.FreeEquity > 0 ? requestedAmount / member.FreeEquity : 0;

                var result = new SavingsMultiplierCheckDto
                {
                    TotalSavings = member.TotalSavings,
                    FreeEquity = member.FreeEquity,
                    LockedEquity = member.LockedEquity,
                    RequestedAmount = requestedAmount,
                    RequiredSavings = requiredSavings,
                    SavingsMultiplier = savingsMultiplier,
                    ActualMultiplier = actualMultiplier,
                    Passed = member.FreeEquity >= requiredSavings
                };

                if (result.Passed)
                {
                    result.Message = $"Member has sufficient free equity (₦{member.FreeEquity:N2}) for requested amount";
                }
                else
                {
                    decimal shortfall = requiredSavings - member.FreeEquity;
                    result.Message = $"Insufficient free equity. Required: ₦{requiredSavings:N2}, Available: ₦{member.FreeEquity:N2}, Shortfall: ₦{shortfall:N2}";
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking savings multiplier for member {MemberId}", memberId);
                throw;
            }
        }

        /// <summary>
        /// Check membership duration eligibility
        /// </summary>
        public async Task<MembershipDurationCheckDto> CheckMembershipDurationAsync(string memberId, int minimumMonths)
        {
            try
            {
                var member = await _memberRepository.GetByIdAsync(memberId);
                if (member == null)
                {
                    throw new InvalidOperationException($"Member {memberId} not found");
                }

                var membershipMonths = (int)((DateTime.UtcNow - member.MembershipStartDate).TotalDays / 30.44);

                var result = new MembershipDurationCheckDto
                {
                    MembershipStartDate = member.MembershipStartDate,
                    MembershipMonths = membershipMonths,
                    RequiredMonths = minimumMonths,
                    Passed = membershipMonths >= minimumMonths
                };

                if (result.Passed)
                {
                    result.Message = $"Member has been active for {membershipMonths} months (required: {minimumMonths} months)";
                }
                else
                {
                    int shortfall = minimumMonths - membershipMonths;
                    result.Message = $"Membership duration insufficient. Required: {minimumMonths} months, Current: {membershipMonths} months, Shortfall: {shortfall} months";
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking membership duration for member {MemberId}", memberId);
                throw;
            }
        }

        /// <summary>
        /// Calculate deduction rate headroom
        /// </summary>
        public async Task<DeductionRateHeadroomDto> CalculateDeductionRateHeadroomAsync(
            string memberId,
            decimal requestedAmount,
            int tenureMonths,
            decimal interestRate)
        {
            try
            {
                var member = await _memberRepository.GetByIdAsync(memberId);
                if (member == null)
                {
                    throw new InvalidOperationException($"Member {memberId} not found");
                }

                // Calculate proposed monthly deduction (EMI)
                decimal proposedMonthlyDeduction = _calculatorService.CalculateEMI(
                    requestedAmount,
                    interestRate,
                    tenureMonths);

                // Get current monthly deductions from active loans
                // FinTech Best Practice: Use GetAllAsync with LINQ instead of FindAsync
                var allLoans = await _loanRepository.GetAllAsync();
                var activeLoans = allLoans.Where(l => 
                    l.MemberId == memberId && 
                    l.Status == "ACTIVE");

                decimal currentMonthlyDeductions = activeLoans.Sum(l => l.MonthlyRepaymentAmount);

                decimal totalMonthlyDeductions = currentMonthlyDeductions + proposedMonthlyDeduction;
                decimal currentDeductionRate = member.MonthlySalary > 0 
                    ? (currentMonthlyDeductions / member.MonthlySalary) * 100 
                    : 0;
                decimal proposedDeductionRate = member.MonthlySalary > 0 
                    ? (totalMonthlyDeductions / member.MonthlySalary) * 100 
                    : 0;
                decimal availableHeadroom = DEFAULT_MAX_DEDUCTION_RATE - currentDeductionRate;

                var result = new DeductionRateHeadroomDto
                {
                    MonthlySalary = member.MonthlySalary,
                    CurrentMonthlyDeductions = currentMonthlyDeductions,
                    ProposedMonthlyDeduction = proposedMonthlyDeduction,
                    TotalMonthlyDeductions = totalMonthlyDeductions,
                    CurrentDeductionRate = currentDeductionRate,
                    ProposedDeductionRate = proposedDeductionRate,
                    MaximumAllowedRate = DEFAULT_MAX_DEDUCTION_RATE,
                    AvailableHeadroom = availableHeadroom,
                    Passed = proposedDeductionRate <= DEFAULT_MAX_DEDUCTION_RATE
                };

                if (result.Passed)
                {
                    result.Message = $"Deduction rate within limits ({proposedDeductionRate:N2}% of salary, max: {DEFAULT_MAX_DEDUCTION_RATE}%)";
                }
                else
                {
                    decimal excess = proposedDeductionRate - DEFAULT_MAX_DEDUCTION_RATE;
                    result.Message = $"Deduction rate exceeds limit. Proposed: {proposedDeductionRate:N2}%, Maximum: {DEFAULT_MAX_DEDUCTION_RATE}%, Excess: {excess:N2}%";
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating deduction rate headroom for member {MemberId}", memberId);
                throw;
            }
        }

        /// <summary>
        /// Check debt-to-income ratio
        /// </summary>
        public async Task<DebtToIncomeRatioDto> CheckDebtToIncomeRatioAsync(
            string memberId,
            decimal requestedAmount,
            int tenureMonths,
            decimal interestRate)
        {
            try
            {
                var member = await _memberRepository.GetByIdAsync(memberId);
                if (member == null)
                {
                    throw new InvalidOperationException($"Member {memberId} not found");
                }

                // Calculate proposed monthly payment
                decimal proposedMonthlyPayment = _calculatorService.CalculateEMI(
                    requestedAmount,
                    interestRate,
                    tenureMonths);

                // Get current debt payments
                // FinTech Best Practice: Use GetAllAsync with LINQ instead of FindAsync
                var allLoans = await _loanRepository.GetAllAsync();
                var activeLoans = allLoans.Where(l => 
                    l.MemberId == memberId && 
                    l.Status == "ACTIVE");

                decimal currentMonthlyDebtPayments = activeLoans.Sum(l => l.MonthlyRepaymentAmount);
                decimal totalMonthlyDebtPayments = currentMonthlyDebtPayments + proposedMonthlyPayment;
                decimal debtToIncomeRatio = member.MonthlySalary > 0 
                    ? (totalMonthlyDebtPayments / member.MonthlySalary) * 100 
                    : 0;

                var result = new DebtToIncomeRatioDto
                {
                    MonthlySalary = member.MonthlySalary,
                    CurrentMonthlyDebtPayments = currentMonthlyDebtPayments,
                    ProposedMonthlyPayment = proposedMonthlyPayment,
                    TotalMonthlyDebtPayments = totalMonthlyDebtPayments,
                    DebtToIncomeRatio = debtToIncomeRatio,
                    MaximumAllowedRatio = DEFAULT_MAX_DEBT_TO_INCOME_RATIO,
                    Passed = debtToIncomeRatio <= DEFAULT_MAX_DEBT_TO_INCOME_RATIO
                };

                if (result.Passed)
                {
                    result.Message = $"Debt-to-income ratio within limits ({debtToIncomeRatio:N2}%, max: {DEFAULT_MAX_DEBT_TO_INCOME_RATIO}%)";
                }
                else
                {
                    decimal excess = debtToIncomeRatio - DEFAULT_MAX_DEBT_TO_INCOME_RATIO;
                    result.Message = $"Debt-to-income ratio exceeds limit. Current: {debtToIncomeRatio:N2}%, Maximum: {DEFAULT_MAX_DEBT_TO_INCOME_RATIO}%, Excess: {excess:N2}%";
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking debt-to-income ratio for member {MemberId}", memberId);
                throw;
            }
        }

        /// <summary>
        /// Generate comprehensive eligibility report
        /// </summary>
        public async Task<EligibilityReportDto> GenerateEligibilityReportAsync(string memberId, string loanProductId)
        {
            try
            {
                var member = await _memberRepository.GetByIdAsync(memberId);
                if (member == null)
                {
                    throw new InvalidOperationException($"Member {memberId} not found");
                }

                var loanProduct = await _loanProductRepository.GetByIdAsync(loanProductId);
                if (loanProduct == null)
                {
                    throw new InvalidOperationException($"Loan product {loanProductId} not found");
                }

                // FinTech Best Practice: Use GetAllAsync with LINQ instead of FindAsync
                var allLoans = await _loanRepository.GetAllAsync();
                var activeLoans = allLoans.Where(l => 
                    l.MemberId == memberId && 
                    l.Status == "ACTIVE");

                var maxEligible = await CalculateMaximumEligibleAmountAsync(memberId, loanProductId);

                var report = new EligibilityReportDto
                {
                    MemberId = member.Id,
                    MemberNumber = member.MemberNumber,
                    MemberName = $"{member.FirstName} {member.LastName}",
                    LoanProductName = loanProduct.Name,
                    MaximumEligibleAmount = maxEligible.MaximumAmount,
                    IsEligible = maxEligible.MaximumAmount > 0,
                    GeneratedAt = DateTime.UtcNow,
                    FinancialSummary = new MemberFinancialSummary
                    {
                        TotalSavings = member.TotalSavings,
                        FreeEquity = member.FreeEquity,
                        LockedEquity = member.LockedEquity,
                        MonthlySalary = member.MonthlySalary,
                        ActiveLoansCount = activeLoans.Count(),
                        TotalOutstandingBalance = activeLoans.Sum(l => l.OutstandingBalance),
                        CurrentMonthlyDeductions = activeLoans.Sum(l => l.MonthlyRepaymentAmount),
                        MembershipMonths = (int)((DateTime.UtcNow - member.MembershipStartDate).TotalDays / 30.44),
                        // FinTech Best Practice: RepaymentScore is nullable string, use null-coalescing
                        RepaymentScore = member.RepaymentScore ?? "N/A"
                    }
                };

                // Build criteria
                report.Criteria = await BuildDetailedCriteria(member, loanProduct, maxEligible);

                // Build recommendations
                report.Recommendations = BuildRecommendations(member, loanProduct, maxEligible);

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating eligibility report for member {MemberId}", memberId);
                throw;
            }
        }

        /// <summary>
        /// Calculate maximum eligible loan amount
        /// </summary>
        public async Task<MaximumEligibleAmountDto> CalculateMaximumEligibleAmountAsync(string memberId, string loanProductId)
        {
            try
            {
                var member = await _memberRepository.GetByIdAsync(memberId);
                if (member == null)
                {
                    throw new InvalidOperationException($"Member {memberId} not found");
                }

                var loanProduct = await _loanProductRepository.GetByIdAsync(loanProductId);
                if (loanProduct == null)
                {
                    throw new InvalidOperationException($"Loan product {loanProductId} not found");
                }

                var result = new MaximumEligibleAmountDto
                {
                    Constraints = new List<string>()
                };

                // 1. Savings-based limit
                decimal savingsMultiplier = loanProduct.SavingsMultiplier;
                result.SavingsBasedLimit = member.FreeEquity * savingsMultiplier;
                result.Constraints.Add($"Savings limit: ₦{result.SavingsBasedLimit:N2} (Free equity ₦{member.FreeEquity:N2} × {savingsMultiplier})");

                // 2. Income-based limit (deduction rate)
                // FinTech Best Practice: Use GetAllAsync with LINQ instead of FindAsync
                var allLoans = await _loanRepository.GetAllAsync();
                var activeLoans = allLoans.Where(l => 
                    l.MemberId == memberId && 
                    l.Status == "ACTIVE");

                decimal currentMonthlyDeductions = activeLoans.Sum(l => l.MonthlyRepaymentAmount);
                decimal availableForDeduction = (member.MonthlySalary * (DEFAULT_MAX_DEDUCTION_RATE / 100)) - currentMonthlyDeductions;
                
                // Calculate max loan based on available deduction capacity
                // FinTech Best Practice: MaxTenureMonths is non-nullable NotMapped property
                int defaultTenure = loanProduct.MaxTenureMonths > 0 ? loanProduct.MaxTenureMonths : 12;
                result.IncomeBasedLimit = CalculateMaxLoanFromEMI(availableForDeduction, loanProduct.InterestRate, defaultTenure);
                result.Constraints.Add($"Income limit: ₦{result.IncomeBasedLimit:N2} (Based on {DEFAULT_MAX_DEDUCTION_RATE}% deduction rate)");

                // 3. Product maximum limit
                decimal productMaxLimit = loanProduct.MaxAmount;
                result.Constraints.Add($"Product limit: ₦{productMaxLimit:N2}");

                // 4. Deduction rate based limit (same as income-based for now)
                result.DeductionRateBasedLimit = result.IncomeBasedLimit;

                // Determine maximum (minimum of all limits)
                result.MaximumAmount = Math.Min(
                    Math.Min(result.SavingsBasedLimit, result.IncomeBasedLimit),
                    productMaxLimit);

                // Determine limiting factor
                if (result.MaximumAmount == result.SavingsBasedLimit)
                {
                    result.LimitingFactor = "Savings/Free Equity";
                }
                else if (result.MaximumAmount == result.IncomeBasedLimit)
                {
                    result.LimitingFactor = "Monthly Income/Deduction Rate";
                }
                else
                {
                    result.LimitingFactor = "Product Maximum Limit";
                }

                _logger.LogInformation(
                    "Maximum eligible amount for member {MemberId}: ₦{MaxAmount:N2} (Limited by: {LimitingFactor})",
                    memberId, result.MaximumAmount, result.LimitingFactor);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating maximum eligible amount for member {MemberId}", memberId);
                throw;
            }
        }

        #region Helper Methods

        private decimal CalculateMaxLoanFromEMI(decimal availableEMI, decimal interestRate, int tenureMonths)
        {
            if (availableEMI <= 0) return 0;

            // Reverse EMI formula to get principal
            // P = EMI × ((1 + r)^n - 1) / (r × (1 + r)^n)
            decimal monthlyRate = (interestRate / 12) / 100;
            
            if (monthlyRate == 0) return availableEMI * tenureMonths;

            decimal powerTerm = (decimal)Math.Pow((double)(1 + monthlyRate), tenureMonths);
            decimal principal = availableEMI * ((powerTerm - 1) / (monthlyRate * powerTerm));

            return Math.Round(principal, 2);
        }

        private List<string> BuildEligibilityCriteria(LoanEligibilityResultDto result)
        {
            var criteria = new List<string>();

            if (result.SavingsMultiplierCheck.Passed)
                criteria.Add("✓ Sufficient savings/free equity");
            
            if (result.MembershipDurationCheck.Passed)
                criteria.Add("✓ Meets minimum membership duration");
            
            if (result.DeductionRateHeadroom.Passed)
                criteria.Add("✓ Deduction rate within acceptable limits");
            
            if (result.DebtToIncomeRatio.Passed)
                criteria.Add("✓ Debt-to-income ratio acceptable");

            return criteria;
        }

        private List<string> BuildFailureReasons(LoanEligibilityResultDto result)
        {
            var reasons = new List<string>();

            if (!result.SavingsMultiplierCheck.Passed)
                reasons.Add(result.SavingsMultiplierCheck.Message);
            
            if (!result.MembershipDurationCheck.Passed)
                reasons.Add(result.MembershipDurationCheck.Message);
            
            if (!result.DeductionRateHeadroom.Passed)
                reasons.Add(result.DeductionRateHeadroom.Message);
            
            if (!result.DebtToIncomeRatio.Passed)
                reasons.Add(result.DebtToIncomeRatio.Message);

            if (result.RequestedAmount > result.MaximumEligibleAmount)
                reasons.Add($"Requested amount (₦{result.RequestedAmount:N2}) exceeds maximum eligible amount (₦{result.MaximumEligibleAmount:N2})");

            return reasons;
        }

        private async Task<List<EligibilityCriterion>> BuildDetailedCriteria(
            Member member, 
            LoanProduct loanProduct, 
            MaximumEligibleAmountDto maxEligible)
        {
            var criteria = new List<EligibilityCriterion>();

            // Savings criterion
            // FinTech Best Practice: SavingsMultiplier is non-nullable decimal
            decimal savingsMultiplier = loanProduct.SavingsMultiplier > 0 ? loanProduct.SavingsMultiplier : 2.0m;
            decimal requiredSavings = maxEligible.MaximumAmount / savingsMultiplier;
            criteria.Add(new EligibilityCriterion
            {
                Name = "Savings Requirement",
                // FinTech Best Practice: SavingsMultiplier is non-nullable
                Description = $"Free equity must support loan amount (multiplier: {savingsMultiplier}x)",
                Passed = member.FreeEquity >= requiredSavings,
                Status = member.FreeEquity >= requiredSavings ? "PASSED" : "FAILED",
                Details = $"Free Equity: ₦{member.FreeEquity:N2}, Required: ₦{requiredSavings:N2}"
            });

            // Membership duration criterion
            int membershipMonths = (int)((DateTime.UtcNow - member.MembershipStartDate).TotalDays / 30.44);
            // FinTech Best Practice: MinimumMembershipMonths is non-nullable int
            int requiredMonths = loanProduct.MinimumMembershipMonths > 0 ? loanProduct.MinimumMembershipMonths : DEFAULT_MIN_MEMBERSHIP_MONTHS;
            criteria.Add(new EligibilityCriterion
            {
                Name = "Membership Duration",
                Description = $"Minimum {requiredMonths} months membership required",
                Passed = membershipMonths >= requiredMonths,
                Status = membershipMonths >= requiredMonths ? "PASSED" : "FAILED",
                Details = $"Current: {membershipMonths} months, Required: {requiredMonths} months"
            });

            return criteria;
        }

        private List<string> BuildRecommendations(
            Member member, 
            LoanProduct loanProduct, 
            MaximumEligibleAmountDto maxEligible)
        {
            var recommendations = new List<string>();

            if (maxEligible.LimitingFactor == "Savings/Free Equity")
            {
                recommendations.Add("Consider increasing your savings contributions to qualify for a higher loan amount");
                recommendations.Add($"Current free equity: ₦{member.FreeEquity:N2}");
            }

            if (maxEligible.LimitingFactor == "Monthly Income/Deduction Rate")
            {
                recommendations.Add("Your current income limits the loan amount you can service");
                recommendations.Add("Consider a longer tenure to reduce monthly payments");
            }

            if (maxEligible.MaximumAmount > 0)
            {
                recommendations.Add($"You are eligible for up to ₦{maxEligible.MaximumAmount:N2}");
            }

            return recommendations;
        }

        #endregion
    }
}
