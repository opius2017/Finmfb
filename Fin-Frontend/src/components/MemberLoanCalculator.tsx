import React, { useState, useEffect } from 'react';
import axios from 'axios';

interface LoanCapacity {
  memberId: number;
  currentSavings: number;
  maxLoanMultiplier: number;
  maxLoanAmount: number;
  recommendedLoanAmount: number;
  isEligible: boolean;
  eligibilityNotes: string;
}

interface RepaymentSchedule {
  monthlyPayment: number;
  totalPayable: number;
  totalInterest: number;
  loanTermMonths: number;
}

/**
 * Member Loan Calculator Component
 * Self-service tool for members to check borrowing capacity and calculate repayments
 * Promotes transparency and financial literacy aligned with microfinance best practices
 */
export const MemberLoanCalculator: React.FC<{ memberId: number }> = ({ memberId }) => {
  const [loanTypeId, setLoanTypeId] = useState<number>(1);
  const [loanCapacity, setLoanCapacity] = useState<LoanCapacity | null>(null);
  const [repaymentSchedule, setRepaymentSchedule] = useState<RepaymentSchedule | null>(null);
  const [requestedAmount, setRequestedAmount] = useState<number>(0);
  const [loanTerm, setLoanTerm] = useState<number>(24);
  const [interestRate, setInterestRate] = useState<number>(18);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string>('');

  // Fetch loan capacity on component load
  useEffect(() => {
    fetchLoanCapacity();
  }, [memberId, loanTypeId]);

  const fetchLoanCapacity = async () => {
    setLoading(true);
    setError('');
    try {
      const response = await axios.get(`/api/v1/loan-calculator/member/${memberId}/loan-capacity/${loanTypeId}`);
      setLoanCapacity(response.data);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to fetch loan capacity');
    } finally {
      setLoading(false);
    }
  };

  const calculateRepayment = async () => {
    if (!requestedAmount || requestedAmount <= 0) {
      setError('Please enter a valid loan amount');
      return;
    }

    setLoading(true);
    setError('');
    try {
      const response = await axios.post('/api/v1/loan-calculator/calculate-repayment', {
        principalAmount: requestedAmount,
        annualInterestRate: interestRate,
        loanTermMonths: loanTerm,
        repaymentFrequency: 'Monthly'
      });
      setRepaymentSchedule(response.data);
    } catch (err: any) {
      setError(err.response?.data?.error || 'Failed to calculate repayment');
    } finally {
      setLoading(false);
    }
  };

  const loanTypes = [
    { id: 1, name: 'Normal Loan' },
    { id: 2, name: 'Commodity Loan' },
    { id: 3, name: 'Car Loan' }
  ];

  return (
    <div className="bg-white rounded-lg shadow-lg p-6 max-w-4xl mx-auto">
      <h2 className="text-2xl font-bold text-gray-800 mb-6">Loan Calculator</h2>

      {/* Error Message */}
      {error && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
          {error}
        </div>
      )}

      {/* Loan Capacity Section */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
        <div className="bg-blue-50 p-4 rounded-lg">
          <label className="block text-sm font-medium text-gray-700 mb-2">Loan Type</label>
          <select
            value={loanTypeId}
            onChange={(e) => setLoanTypeId(Number(e.target.value))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md"
          >
            {loanTypes.map((type) => (
              <option key={type.id} value={type.id}>
                {type.name}
              </option>
            ))}
          </select>
        </div>

        {loanCapacity && (
          <>
            <div className="bg-green-50 p-4 rounded-lg">
              <p className="text-sm font-medium text-gray-700">Current Savings</p>
              <p className="text-2xl font-bold text-green-600">₦{(loanCapacity.currentSavings || 0).toLocaleString()}</p>
            </div>
            <div className="bg-blue-50 p-4 rounded-lg">
              <p className="text-sm font-medium text-gray-700">Max Borrowing Capacity</p>
              <p className="text-2xl font-bold text-blue-600">₦{loanCapacity.maxLoanAmount.toLocaleString()}</p>
            </div>
            <div className="bg-purple-50 p-4 rounded-lg">
              <p className="text-sm font-medium text-gray-700">Recommended Loan Amount</p>
              <p className="text-2xl font-bold text-purple-600">₦{loanCapacity.recommendedLoanAmount.toLocaleString()}</p>
            </div>
            <div className={`p-4 rounded-lg ${loanCapacity.isEligible ? 'bg-green-50' : 'bg-orange-50'}`}>
              <p className="text-sm font-medium text-gray-700">Eligibility</p>
              <p className={`text-sm font-semibold ${loanCapacity.isEligible ? 'text-green-600' : 'text-orange-600'}`}>
                {loanCapacity.eligibilityNotes}
              </p>
            </div>
          </>
        )}
      </div>

      {/* Repayment Calculator */}
      <div className="bg-gray-50 p-6 rounded-lg">
        <h3 className="text-lg font-semibold text-gray-800 mb-4">Calculate Monthly Repayment</h3>
        
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Requested Amount (₦)</label>
            <input
              type="number"
              value={requestedAmount}
              onChange={(e) => setRequestedAmount(Number(e.target.value))}
              placeholder="500,000"
              className="w-full px-3 py-2 border border-gray-300 rounded-md"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Annual Interest Rate (%)</label>
            <input
              type="number"
              value={interestRate}
              onChange={(e) => setInterestRate(Number(e.target.value))}
              step="0.5"
              min="5"
              max="50"
              className="w-full px-3 py-2 border border-gray-300 rounded-md"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Loan Term (months)</label>
            <select
              value={loanTerm}
              onChange={(e) => setLoanTerm(Number(e.target.value))}
              className="w-full px-3 py-2 border border-gray-300 rounded-md"
            >
              {[12, 18, 24, 36, 48, 60].map((term) => (
                <option key={term} value={term}>
                  {term} months
                </option>
              ))}
            </select>
          </div>
          <div className="flex items-end">
            <button
              onClick={calculateRepayment}
              disabled={loading}
              className="w-full bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700 disabled:bg-gray-400"
            >
              {loading ? 'Calculating...' : 'Calculate'}
            </button>
          </div>
        </div>

        {/* Repayment Results */}
        {repaymentSchedule && (
          <div className="bg-white p-4 rounded-lg mt-4 border-l-4 border-blue-600">
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <div>
                <p className="text-xs font-medium text-gray-600">Monthly Payment</p>
                <p className="text-xl font-bold text-blue-600">₦{repaymentSchedule.monthlyPayment.toLocaleString()}</p>
              </div>
              <div>
                <p className="text-xs font-medium text-gray-600">Total Interest</p>
                <p className="text-xl font-bold text-orange-600">₦{repaymentSchedule.totalInterest.toLocaleString()}</p>
              </div>
              <div>
                <p className="text-xs font-medium text-gray-600">Total to Repay</p>
                <p className="text-xl font-bold text-green-600">₦{repaymentSchedule.totalPayable.toLocaleString()}</p>
              </div>
              <div>
                <p className="text-xs font-medium text-gray-600">Loan Term</p>
                <p className="text-xl font-bold text-purple-600">{repaymentSchedule.loanTermMonths}M</p>
              </div>
            </div>

            <div className="mt-4 p-3 bg-blue-50 rounded">
              <p className="text-xs text-gray-600 font-semibold mb-2">Important Information:</p>
              <ul className="text-xs text-gray-700 space-y-1">
                <li>• Monthly payment must not exceed 40% of your monthly income (CBN Guideline)</li>
                <li>• You must maintain minimum savings of 25% of the loan amount</li>
                <li>• Guarantors may be required depending on loan amount</li>
                <li>• Loans above ₦5M require Loan Committee approval</li>
              </ul>
            </div>
          </div>
        )}
      </div>

      {/* Information Box */}
      <div className="mt-6 bg-yellow-50 border border-yellow-200 p-4 rounded-lg">
        <h4 className="font-semibold text-yellow-900 mb-2">📋 Application Process</h4>
        <ol className="text-sm text-yellow-800 space-y-1">
          <li>1. Use calculator to determine eligible loan amount</li>
          <li>2. Submit loan application online or visit branch</li>
          <li>3. Credit officer reviews and performs risk assessment</li>
          <li>4. Loans ≥₦5M go to Loan Committee for approval</li>
          <li>5. Once approved, funds disbursed to your account</li>
          <li>6. Monthly repayments deducted from salary</li>
        </ol>
      </div>
    </div>
  );
};

export default MemberLoanCalculator;
