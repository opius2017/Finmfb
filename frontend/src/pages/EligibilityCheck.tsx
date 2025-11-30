import { useState } from 'react';
import { apiService } from '@/services/api';
import { toast } from 'sonner';
import { CheckCircle, XCircle, TrendingUp, Users, DollarSign, Calendar } from 'lucide-react';

export default function EligibilityCheck() {
  const [memberId, setMemberId] = useState('');
  const [loanProductId, setLoanProductId] = useState('PROD001');
  const [requestedAmount, setRequestedAmount] = useState(1000000);
  const [tenureMonths, setTenureMonths] = useState(12);
  const [result, setResult] = useState<any>(null);
  const [loading, setLoading] = useState(false);

  const checkEligibility = async () => {
    if (!memberId) {
      toast.error('Please enter Member ID');
      return;
    }

    setLoading(true);
    try {
      const data = await apiService.checkEligibility({
        memberId,
        loanProductId,
        requestedAmount,
        tenureMonths,
      });
      setResult(data);
      toast.success(data.isEligible ? 'You are eligible!' : 'Eligibility check complete');
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to check eligibility');
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-NG', {
      style: 'currency',
      currency: 'NGN',
      minimumFractionDigits: 0,
    }).format(amount);
  };

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Eligibility Check</h1>
        <p className="text-gray-600 mt-1">
          Check if you qualify for a loan based on your savings and income
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Input Form */}
        <div className="lg:col-span-1">
          <div className="card sticky top-24">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">
              Loan Details
            </h2>

            <div className="space-y-4">
              <div>
                <label className="label">Member ID</label>
                <input
                  type="text"
                  className="input"
                  placeholder="MEM001"
                  value={memberId}
                  onChange={(e) => setMemberId(e.target.value)}
                />
              </div>

              <div>
                <label className="label">Loan Product</label>
                <select
                  className="input"
                  value={loanProductId}
                  onChange={(e) => setLoanProductId(e.target.value)}
                >
                  <option value="PROD001">Regular Loan (200% multiplier)</option>
                  <option value="PROD002">Premium Loan (300% multiplier)</option>
                  <option value="PROD003">Executive Loan (500% multiplier)</option>
                </select>
              </div>

              <div>
                <label className="label">Requested Amount (₦)</label>
                <input
                  type="number"
                  className="input"
                  value={requestedAmount}
                  onChange={(e) => setRequestedAmount(Number(e.target.value))}
                  step="10000"
                />
              </div>

              <div>
                <label className="label">Tenure (Months)</label>
                <input
                  type="number"
                  className="input"
                  value={tenureMonths}
                  onChange={(e) => setTenureMonths(Number(e.target.value))}
                  min="1"
                  max="360"
                />
              </div>

              <button
                onClick={checkEligibility}
                disabled={loading}
                className="btn btn-primary w-full"
              >
                {loading ? 'Checking...' : 'Check Eligibility'}
              </button>
            </div>
          </div>
        </div>

        {/* Results */}
        <div className="lg:col-span-2 space-y-6">
          {result && (
            <>
              {/* Eligibility Status */}
              <div
                className={`card ${
                  result.isEligible
                    ? 'bg-gradient-to-br from-green-50 to-green-100 border-green-200'
                    : 'bg-gradient-to-br from-red-50 to-red-100 border-red-200'
                }`}
              >
                <div className="flex items-center">
                  {result.isEligible ? (
                    <CheckCircle className="w-12 h-12 text-green-600 mr-4" />
                  ) : (
                    <XCircle className="w-12 h-12 text-red-600 mr-4" />
                  )}
                  <div>
                    <h2 className="text-2xl font-bold text-gray-900">
                      {result.isEligible ? 'You are Eligible!' : 'Not Eligible'}
                    </h2>
                    <p className="text-gray-700 mt-1">
                      {result.isEligible
                        ? `You can borrow up to ${formatCurrency(result.maximumEligibleAmount)}`
                        : 'Please review the requirements below'}
                    </p>
                  </div>
                </div>
              </div>

              {/* Eligibility Criteria */}
              <div className="card">
                <h3 className="text-lg font-semibold text-gray-900 mb-4">
                  Eligibility Criteria
                </h3>
                <div className="space-y-3">
                  {result.eligibilityCriteria.map((criterion: string, index: number) => (
                    <div key={index} className="flex items-start">
                      <CheckCircle className="w-5 h-5 text-green-600 mr-3 mt-0.5 flex-shrink-0" />
                      <p className="text-gray-700">{criterion}</p>
                    </div>
                  ))}
                  {result.failureReasons.map((reason: string, index: number) => (
                    <div key={index} className="flex items-start">
                      <XCircle className="w-5 h-5 text-red-600 mr-3 mt-0.5 flex-shrink-0" />
                      <p className="text-gray-700">{reason}</p>
                    </div>
                  ))}
                </div>
              </div>

              {/* Detailed Checks */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {/* Savings Multiplier */}
                <div className="card">
                  <div className="flex items-center mb-4">
                    <DollarSign className="w-5 h-5 text-primary-600 mr-2" />
                    <h3 className="font-semibold text-gray-900">Savings Multiplier</h3>
                  </div>
                  <div className="space-y-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-gray-600">Total Savings:</span>
                      <span className="font-medium">
                        {formatCurrency(result.savingsMultiplierCheck.totalSavings)}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">Free Equity:</span>
                      <span className="font-medium">
                        {formatCurrency(result.savingsMultiplierCheck.freeEquity)}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">Required Savings:</span>
                      <span className="font-medium">
                        {formatCurrency(result.savingsMultiplierCheck.requiredSavings)}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">Multiplier:</span>
                      <span className="font-medium">
                        {result.savingsMultiplierCheck.savingsMultiplier}x
                      </span>
                    </div>
                    <div className="pt-2 border-t">
                      <span
                        className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${
                          result.savingsMultiplierCheck.passed
                            ? 'bg-green-100 text-green-800'
                            : 'bg-red-100 text-red-800'
                        }`}
                      >
                        {result.savingsMultiplierCheck.passed ? 'PASSED' : 'FAILED'}
                      </span>
                    </div>
                  </div>
                </div>

                {/* Membership Duration */}
                <div className="card">
                  <div className="flex items-center mb-4">
                    <Calendar className="w-5 h-5 text-primary-600 mr-2" />
                    <h3 className="font-semibold text-gray-900">Membership Duration</h3>
                  </div>
                  <div className="space-y-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-gray-600">Member Since:</span>
                      <span className="font-medium">
                        {new Date(result.membershipDurationCheck.membershipStartDate).toLocaleDateString()}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">Membership Months:</span>
                      <span className="font-medium">
                        {result.membershipDurationCheck.membershipMonths} months
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">Required:</span>
                      <span className="font-medium">
                        {result.membershipDurationCheck.requiredMonths} months
                      </span>
                    </div>
                    <div className="pt-2 border-t">
                      <span
                        className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${
                          result.membershipDurationCheck.passed
                            ? 'bg-green-100 text-green-800'
                            : 'bg-red-100 text-red-800'
                        }`}
                      >
                        {result.membershipDurationCheck.passed ? 'PASSED' : 'FAILED'}
                      </span>
                    </div>
                  </div>
                </div>

                {/* Deduction Rate */}
                <div className="card">
                  <div className="flex items-center mb-4">
                    <TrendingUp className="w-5 h-5 text-primary-600 mr-2" />
                    <h3 className="font-semibold text-gray-900">Deduction Rate</h3>
                  </div>
                  <div className="space-y-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-gray-600">Monthly Salary:</span>
                      <span className="font-medium">
                        {formatCurrency(result.deductionRateHeadroom.monthlySalary)}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">Current Deductions:</span>
                      <span className="font-medium">
                        {formatCurrency(result.deductionRateHeadroom.currentMonthlyDeductions)}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">Proposed Deduction:</span>
                      <span className="font-medium">
                        {formatCurrency(result.deductionRateHeadroom.proposedMonthlyDeduction)}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">Deduction Rate:</span>
                      <span className="font-medium">
                        {result.deductionRateHeadroom.proposedDeductionRate.toFixed(1)}%
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">Maximum Allowed:</span>
                      <span className="font-medium">
                        {result.deductionRateHeadroom.maximumAllowedRate}%
                      </span>
                    </div>
                    <div className="pt-2 border-t">
                      <span
                        className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${
                          result.deductionRateHeadroom.passed
                            ? 'bg-green-100 text-green-800'
                            : 'bg-red-100 text-red-800'
                        }`}
                      >
                        {result.deductionRateHeadroom.passed ? 'PASSED' : 'FAILED'}
                      </span>
                    </div>
                  </div>
                </div>

                {/* Debt-to-Income Ratio */}
                <div className="card">
                  <div className="flex items-center mb-4">
                    <Users className="w-5 h-5 text-primary-600 mr-2" />
                    <h3 className="font-semibold text-gray-900">Debt-to-Income Ratio</h3>
                  </div>
                  <div className="space-y-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-gray-600">Monthly Salary:</span>
                      <span className="font-medium">
                        {formatCurrency(result.debtToIncomeRatio.monthlySalary)}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">Current Debt Payments:</span>
                      <span className="font-medium">
                        {formatCurrency(result.debtToIncomeRatio.currentMonthlyDebtPayments)}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">Proposed Payment:</span>
                      <span className="font-medium">
                        {formatCurrency(result.debtToIncomeRatio.proposedMonthlyPayment)}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">DTI Ratio:</span>
                      <span className="font-medium">
                        {result.debtToIncomeRatio.debtToIncomeRatio.toFixed(1)}%
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">Maximum Allowed:</span>
                      <span className="font-medium">
                        {result.debtToIncomeRatio.maximumAllowedRatio}%
                      </span>
                    </div>
                    <div className="pt-2 border-t">
                      <span
                        className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${
                          result.debtToIncomeRatio.passed
                            ? 'bg-green-100 text-green-800'
                            : 'bg-red-100 text-red-800'
                        }`}
                      >
                        {result.debtToIncomeRatio.passed ? 'PASSED' : 'FAILED'}
                      </span>
                    </div>
                  </div>
                </div>
              </div>

              {/* Maximum Eligible Amount */}
              <div className="card bg-gradient-to-br from-primary-50 to-primary-100 border-primary-200">
                <div className="flex items-center justify-between">
                  <div>
                    <h3 className="text-lg font-semibold text-gray-900 mb-1">
                      Maximum Eligible Amount
                    </h3>
                    <p className="text-3xl font-bold text-primary-700">
                      {formatCurrency(result.maximumEligibleAmount)}
                    </p>
                  </div>
                  <DollarSign className="w-12 h-12 text-primary-600 opacity-50" />
                </div>
              </div>
            </>
          )}

          {/* Empty State */}
          {!result && (
            <div className="card text-center py-16">
              <CheckCircle className="w-20 h-20 text-gray-300 mx-auto mb-4" />
              <h3 className="text-xl font-medium text-gray-900 mb-2">
                Check Your Eligibility
              </h3>
              <p className="text-gray-600 max-w-md mx-auto">
                Enter your member ID and loan details to check if you qualify for a loan.
                We'll validate your savings, membership duration, and income.
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
