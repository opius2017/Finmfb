import React, { useState, useEffect } from 'react';
import { Calculator, DollarSign, Calendar, TrendingUp } from 'lucide-react';
import { loanService } from '../services/loanService';
import { LoanType, LoanCalculatorOutput } from '../types/loan.types';

interface LoanCalculatorProps {
    onCalculationComplete: (data: LoanCalculatorOutput & { principal: number, tenor: number, loanType: LoanType }) => void;
    initialValues?: {
        amount: number;
        tenor: number;
        loanType: LoanType;
    };
}

export const LoanCalculator: React.FC<LoanCalculatorProps> = ({ onCalculationComplete, initialValues }) => {
    const [amount, setAmount] = useState(initialValues?.amount || 100000);
    const [tenor, setTenor] = useState(initialValues?.tenor || 12);
    const [loanType, setLoanType] = useState<LoanType>(initialValues?.loanType || 'normal');
    const [result, setResult] = useState<LoanCalculatorOutput | null>(null);

    // Auto-calculate on change
    useEffect(() => {
        const calculate = () => {
            // Simple local calculation for immediate feedback (mimicking backend logic)
            // In a real app, this might debounce and hit the API
            const rate = loanType === 'normal' ? 15 : loanType === 'car' ? 12 : 5; // Mock rates
            const monthlyRate = rate / 12 / 100;
            const emi = (amount * monthlyRate * Math.pow(1 + monthlyRate, tenor)) / (Math.pow(1 + monthlyRate, tenor) - 1);
            const totalRepayment = emi * tenor;
            const totalInterest = totalRepayment - amount;

            const calcResult: LoanCalculatorOutput = {
                monthlyEMI: emi,
                totalInterest,
                totalRepayment,
                eligibility: {
                    isEligible: true, // simplified
                    reasons: [],
                    maxLoanAmount: amount * 2, // mock
                    requiredSavings: amount * 0.3,
                    currentSavings: amount * 0.5,
                    deductionRateHeadroom: 50000,
                    membershipDuration: 12
                },
                amortizationSchedule: [] // simplified
            };

            setResult(calcResult);
            onCalculationComplete({ ...calcResult, principal: amount, tenor, loanType });
        };

        calculate();
    }, [amount, tenor, loanType]);

    return (
        <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
            <div className="flex items-center space-x-2 mb-6">
                <div className="bg-indigo-100 p-2 rounded-lg text-indigo-600">
                    <Calculator className="w-5 h-5" />
                </div>
                <h3 className="text-lg font-bold text-gray-900">Loan Calculator</h3>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                <div className="space-y-6">
                    {/* Loan Type */}
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">Loan Product</label>
                        <div className="grid grid-cols-3 gap-2">
                            {(['normal', 'commodity', 'car'] as LoanType[]).map((type) => (
                                <button
                                    key={type}
                                    onClick={() => setLoanType(type)}
                                    className={`px-4 py-2 rounded-lg text-sm font-medium capitalize transition-all border ${loanType === type
                                            ? 'bg-indigo-600 text-white border-indigo-600 shadow-md'
                                            : 'bg-white text-gray-600 border-gray-200 hover:bg-gray-50'
                                        }`}
                                >
                                    {type}
                                </button>
                            ))}
                        </div>
                    </div>

                    {/* Amount Slider & Input */}
                    <div>
                        <div className="flex justify-between mb-2">
                            <label className="text-sm font-medium text-gray-700">Request Amount</label>
                            <span className="text-sm font-bold text-indigo-600">₦{amount.toLocaleString()}</span>
                        </div>
                        <input
                            type="range"
                            min="10000"
                            max="10000000"
                            step="10000"
                            value={amount}
                            onChange={(e) => setAmount(Number(e.target.value))}
                            className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer accent-indigo-600"
                        />
                        <div className="mt-2 flex space-x-2">
                            <div className="relative flex-1">
                                <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">₦</span>
                                <input
                                    type="number"
                                    value={amount}
                                    onChange={(e) => setAmount(Number(e.target.value))}
                                    className="w-full pl-7 pr-3 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-indigo-500 outline-none"
                                />
                            </div>
                        </div>
                    </div>

                    {/* Tenor Slider */}
                    <div>
                        <div className="flex justify-between mb-2">
                            <label className="text-sm font-medium text-gray-700">Tenor (Months)</label>
                            <span className="text-sm font-bold text-indigo-600">{tenor} Months</span>
                        </div>
                        <input
                            type="range"
                            min="1"
                            max="48"
                            step="1"
                            value={tenor}
                            onChange={(e) => setTenor(Number(e.target.value))}
                            className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer accent-indigo-600"
                        />
                        <div className="flex justify-between text-xs text-gray-400 mt-1">
                            <span>1 Month</span>
                            <span>48 Months</span>
                        </div>
                    </div>
                </div>

                {/* Results Panel */}
                <div className="bg-gray-50 rounded-xl p-6 border border-gray-200 flex flex-col justify-center space-y-6">
                    <div className="text-center">
                        <span className="text-sm text-gray-500 uppercase tracking-wider font-semibold">Estimated Monthly Payment</span>
                        <div className="text-4xl font-extrabold text-indigo-600 mt-2 flex items-center justify-center">
                            <span className="text-2xl mr-1">₦</span>
                            {result?.monthlyEMI.toLocaleString(undefined, { maximumFractionDigits: 0 })}
                        </div>
                    </div>

                    <div className="space-y-3 pt-4 border-t border-gray-200">
                        <div className="flex justify-between text-sm">
                            <span className="text-gray-600">Principal</span>
                            <span className="font-medium text-gray-900">₦{amount.toLocaleString()}</span>
                        </div>
                        <div className="flex justify-between text-sm">
                            <span className="text-gray-600">Total Interest</span>
                            <span className="font-medium text-emerald-600">+ ₦{result?.totalInterest.toLocaleString(undefined, { maximumFractionDigits: 0 })}</span>
                        </div>
                        <div className="flex justify-between text-sm font-bold pt-2 border-t border-gray-200">
                            <span className="text-gray-800">Total Repayment</span>
                            <span className="text-indigo-700">₦{result?.totalRepayment.toLocaleString(undefined, { maximumFractionDigits: 0 })}</span>
                        </div>
                    </div>

                    {/* Eligibility Badge */}
                    <div className="mt-auto">
                        <div className={`flex items-center justify-center p-3 rounded-xl border ${result?.eligibility.isEligible ? 'bg-emerald-50 border-emerald-200' : 'bg-red-50 border-red-200'}`}>
                            {result?.eligibility.isEligible ? (
                                <>
                                    <TrendingUp className="w-5 h-5 text-emerald-600 mr-2" />
                                    <span className="text-sm font-bold text-emerald-700">Eligible for this amount</span>
                                </>
                            ) : (
                                <span className="text-sm font-bold text-red-700">Limit Exceeded</span>
                            )}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};
