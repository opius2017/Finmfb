using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Entities.Customers;
using FinTech.Core.Application.Common.Interfaces;

namespace FinTech.Core.Application.Services.LoanCalculation
{
    /// <summary>
    /// Implementation of loan calculator service
    /// Applies Nigerian cooperative lending standards and Central Bank guidelines
    /// </summary>
    public class LoanCalculatorService : ILoanCalculatorService
    {
        private readonly IApplicationDbContext _dbContext;
        private const decimal DEFAULT_MAX_DEDUCTION_RATE = 0.40m; // 40% of income per CBN guidelines
        private const decimal DEFAULT_MIN_SAVINGS_RATIO = 0.25m; // 25% savings-to-loan ratio

        public LoanCalculatorService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<LoanCapacityResult> CalculateMemberLoanCapacity(int memberId, int loanTypeId)
        {
            var member = await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.Id == memberId);

            if (member == null)
                return new LoanCapacityResult { IsEligible = false, EligibilityNotes = "Member not found" };

            var loanType = await _dbContext.LoanTypes
                .FirstOrDefaultAsync(lt => lt.Id == loanTypeId);

            if (loanType == null)
                return new LoanCapacityResult { IsEligible = false, EligibilityNotes = "Loan type not found" };

            // Get member's total savings
            var totalSavings = member.AccountBalance ?? 0;

            // Calculate max loan based on multiplier
            var maxLoanAmount = totalSavings * loanType.MaxLoanMultiplier;

            // Ensure doesn't exceed loan type maximum
            if (loanType.MaxLoanMultiplier > 0)
                maxLoanAmount = Math.Min(maxLoanAmount, loanType.MaxLoanMultiplier * 1000000); // Reasonable cap

            // Check minimum savings requirement
            var meetsSavingsRequirement = totalSavings >= loanType.MinimumSavingsRequired;

            return new LoanCapacityResult
            {
                MemberId = memberId,
                CurrentSavings = totalSavings,
                MaxLoanMultiplier = loanType.MaxLoanMultiplier,
                MaxLoanAmount = maxLoanAmount,
                RecommendedLoanAmount = Math.Min(maxLoanAmount * 0.9m, maxLoanAmount), // 90% of max
                IsEligible = meetsSavingsRequirement,
                EligibilityNotes = meetsSavingsRequirement
                    ? "Member meets savings requirements"
                    : $"Member savings (₦{totalSavings:N2}) below requirement (₦{loanType.MinimumSavingsRequired:N2})"
            };
        }

        public LoanRepaymentCalculation CalculateMonthlyRepayment(decimal principalAmount, decimal annualInterestRate, int loanTermMonths, string repaymentFrequency)
        {
            if (principalAmount <= 0)
                throw new ArgumentException("Principal amount must be positive");
            if (loanTermMonths <= 0)
                throw new ArgumentException("Loan term must be positive");

            decimal monthlyRate = annualInterestRate / 12 / 100;
            decimal monthlyPayment;

            // Calculate using standard amortization formula
            if (Math.Abs(monthlyRate) < 0.0001m)
            {
                // No interest - simple division
                monthlyPayment = Math.Round(principalAmount / loanTermMonths, 2);
            }
            else
            {
                // Amortization formula: P * [r(1+r)^n] / [(1+r)^n - 1]
                var factor = (decimal)Math.Pow((double)(1 + monthlyRate), loanTermMonths);
                monthlyPayment = Math.Round(
                    principalAmount * monthlyRate * factor / (factor - 1), 2);
            }

            // Generate payment schedule
            var schedule = new List<decimal>();
            var paymentDates = new List<DateTime>();
            decimal remainingBalance = principalAmount;
            var currentDate = DateTime.UtcNow.AddMonths(1);

            for (int i = 0; i < loanTermMonths; i++)
            {
                var interestPayment = Math.Round(remainingBalance * monthlyRate, 2);
                var principalPayment = monthlyPayment - interestPayment;

                // Adjust last payment for rounding
                if (i == loanTermMonths - 1)
                {
                    principalPayment = remainingBalance;
                }

                schedule.Add(monthlyPayment);
                paymentDates.Add(currentDate);
                remainingBalance -= principalPayment;
                currentDate = currentDate.AddMonths(1);
            }

            decimal totalInterest = (monthlyPayment * loanTermMonths) - principalAmount;

            return new LoanRepaymentCalculation
            {
                MonthlyPayment = monthlyPayment,
                TotalPayable = principalAmount + totalInterest,
                TotalInterest = Math.Round(totalInterest, 2),
                LoanTermMonths = loanTermMonths,
                MonthlySchedule = schedule.ToArray(),
                PaymentDates = paymentDates.ToArray()
            };
        }

        public async Task<EligibilityCheckResult> CheckMemberEligibility(int memberId, int loanTypeId, decimal requestedAmount)
        {
            var member = await _dbContext.Customers
                .Include(c => c.Loans)
                .FirstOrDefaultAsync(c => c.Id == memberId);

            var loanType = await _dbContext.LoanTypes
                .FirstOrDefaultAsync(lt => lt.Id == loanTypeId);

            var passedCriteria = new List<string>();
            var failedCriteria = new List<string>();
            var creditScore = await CalculateMemberCreditScore(memberId);

            // Check credit score
            if (creditScore >= 60)
            {
                passedCriteria.Add("Good credit score");
            }
            else
            {
                failedCriteria.Add($"Low credit score: {creditScore}");
            }

            // Check minimum membership period
            if (member.MemberSince.HasValue)
            {
                var membershipMonths = (DateTime.UtcNow - member.MemberSince.Value).Days / 30;
                if (membershipMonths >= loanType.MinLoanTermMonths)
                {
                    passedCriteria.Add("Sufficient membership period");
                }
                else
                {
                    failedCriteria.Add($"Insufficient membership period: {membershipMonths} months");
                }
            }

            // Check savings requirement
            var savingsAnalysis = await AnalyzeSavingsRequirement(memberId, requestedAmount, loanTypeId);
            if (savingsAnalysis.MeetsMinimumSavings && savingsAnalysis.MeetsMinimumRatio)
            {
                passedCriteria.Add("Meets savings requirement");
            }
            else
            {
                failedCriteria.Add("Insufficient savings");
            }

            // Check active loans limit
            var activeLoans = member.Loans?.Count(l => l.Status == "ACTIVE") ?? 0;
            if (loanType.MinGuarantorsRequired > 0 && activeLoans < 2)
            {
                passedCriteria.Add("Active loans within limits");
            }
            else if (activeLoans >= 2)
            {
                failedCriteria.Add("Too many active loans");
            }

            // Determine risk rating
            string riskRating = creditScore >= 80 ? "Low"
                : creditScore >= 60 ? "Medium"
                : creditScore >= 40 ? "High"
                : "Critical";

            bool requiresCommitteeReview = failedCriteria.Any() || riskRating != "Low" || requestedAmount > 5000000;

            return new EligibilityCheckResult
            {
                MemberId = memberId,
                IsEligible = !failedCriteria.Any() && riskRating != "Critical",
                CreditScore = creditScore,
                RiskRating = riskRating,
                FailedCriteria = failedCriteria.ToArray(),
                PassedCriteria = passedCriteria.ToArray(),
                RequiresCommitteeReview = requiresCommitteeReview,
                Message = failedCriteria.Any()
                    ? $"Eligibility concerns: {string.Join(", ", failedCriteria)}"
                    : "Member meets all eligibility criteria"
            };
        }

        public async Task<DeductionAnalysisResult> AnalyzeDeductionCompliance(int memberId, decimal monthlyDeduction, decimal monthlyIncome)
        {
            // Get member's current deductions
            var member = await _dbContext.Customers
                .Include(c => c.Loans)
                .FirstOrDefaultAsync(c => c.Id == memberId);

            var currentDeductions = member.Loans?
                .Where(l => l.Status == "ACTIVE")
                .Sum(l => l.MonthlyPayment) ?? 0;

            var totalDeductionsAfter = currentDeductions + monthlyDeduction;
            var maxAllowedDeduction = monthlyIncome * DEFAULT_MAX_DEDUCTION_RATE;
            var deductionPercentage = monthlyIncome > 0 ? (totalDeductionsAfter / monthlyIncome) : 100;

            return new DeductionAnalysisResult
            {
                RequestedMonthlyDeduction = monthlyDeduction,
                MemberMonthlyIncome = monthlyIncome,
                CurrentDeductions = currentDeductions,
                TotalDeductionsAfterLoan = totalDeductionsAfter,
                MaxAllowedDeduction = maxAllowedDeduction,
                DeductionPercentage = deductionPercentage,
                IsCompliant = deductionPercentage <= DEFAULT_MAX_DEDUCTION_RATE,
                ComplianceStatus = deductionPercentage <= DEFAULT_MAX_DEDUCTION_RATE ? "Compliant" : "Exceeds limit",
                Message = deductionPercentage <= DEFAULT_MAX_DEDUCTION_RATE
                    ? $"Deduction {deductionPercentage:P} is within 40% CBN guideline"
                    : $"Deduction {deductionPercentage:P} exceeds 40% CBN maximum"
            };
        }

        public LoanCostEstimate EstimateLoanCost(decimal principalAmount, decimal annualInterestRate, int loanTermMonths, decimal processingFeePercent)
        {
            var calculation = CalculateMonthlyRepayment(principalAmount, annualInterestRate, loanTermMonths, "Monthly");
            var processingFee = principalAmount * (processingFeePercent / 100);

            return new LoanCostEstimate
            {
                PrincipalAmount = principalAmount,
                TotalInterest = calculation.TotalInterest,
                ProcessingFee = Math.Round(processingFee, 2),
                OtherFees = 0, // To be populated based on loan type
                TotalCost = calculation.TotalInterest + processingFee,
                TotalPayable = calculation.TotalPayable + processingFee,
                EffectiveInterestRate = Math.Round((calculation.TotalInterest / principalAmount) * (12 / loanTermMonths), 2)
            };
        }

        public async Task<decimal> CalculateMemberCreditScore(int memberId)
        {
            var member = await _dbContext.Customers
                .Include(c => c.Loans)
                .ThenInclude(l => l.Transactions)
                .FirstOrDefaultAsync(c => c.Id == memberId);

            if (member?.Loans == null || !member.Loans.Any())
                return 50m; // Base score for new members

            decimal score = 50m; // Base score

            // Reward for successful loans
            var successfulLoans = member.Loans.Count(l => l.Status == "CLOSED");
            score += Math.Min(successfulLoans * 5, 20); // Max 20 points

            // Penalty for defaults
            var defaultLoans = member.Loans.Count(l => l.Status == "WRITTEN_OFF");
            score -= Math.Min(defaultLoans * 10, 20); // Max -20 points

            // Check repayment timeliness on active loans
            foreach (var loan in member.Loans.Where(l => l.Status == "ACTIVE" || l.Status == "CLOSED"))
            {
                var latePayments = loan.Transactions?.Count(t => t.TransactionType == "REPAYMENT" && t.IsLate) ?? 0;
                if (latePayments == 0)
                    score += 5;
                else
                    score -= Math.Min(latePayments * 2, 10);
            }

            // Reward for savings consistency
            if ((member.AccountBalance ?? 0) > 0)
                score += 10;

            // Ensure score stays within 0-100 range
            return Math.Max(0, Math.Min(100, score));
        }

        public async Task<SavingsAnalysisResult> AnalyzeSavingsRequirement(int memberId, decimal requestedLoanAmount, int loanTypeId)
        {
            var member = await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.Id == memberId);

            var loanType = await _dbContext.LoanTypes
                .FirstOrDefaultAsync(lt => lt.Id == loanTypeId);

            var currentSavings = member?.AccountBalance ?? 0;
            var minimumRequired = loanType?.MinimumSavingsRequired ?? 0;
            var savingsRatio = currentSavings / (requestedLoanAmount > 0 ? requestedLoanAmount : 1);
            var minimumRatioRequired = DEFAULT_MIN_SAVINGS_RATIO;

            return new SavingsAnalysisResult
            {
                MemberId = memberId,
                CurrentSavings = currentSavings,
                MinimumSavingsRequired = minimumRequired,
                SavingsToLoanRatio = Math.Round(savingsRatio, 2),
                MinimumSavingsRatioRequired = minimumRatioRequired,
                MeetsMinimumSavings = currentSavings >= minimumRequired,
                MeetsMinimumRatio = savingsRatio >= minimumRatioRequired,
                AnalysisMessage = savingsRatio >= minimumRatioRequired && currentSavings >= minimumRequired
                    ? $"Member meets requirements: {currentSavings:C} savings, {savingsRatio:P} ratio"
                    : $"Member falls short: {currentSavings:C} savings (need ₦{minimumRequired:N2}), {savingsRatio:P} ratio (need {minimumRatioRequired:P})"
            };
        }
    }
}
