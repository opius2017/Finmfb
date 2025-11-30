import React, { useState, useEffect } from 'react';
import { DollarSign, Users, Calculator, CheckCircle } from 'lucide-react';
import { Card } from '../../../design-system/components/Card';
import { Button } from '../../../design-system/components/Button';
import { Input } from '../../../design-system/components/Input';
import { LoanEligibility, LoanCalculatorOutput, Guarantor } from '../types/loan.types';
import { loanService } from '../services/loanService';

export const LoanApplication: React.FC = () => {
  const [step, setStep] = useState(1);
  const [loanType, setLoanType] = useState('normal');
  const [amount, setAmount] = useState('');
  const [tenor, setTenor] = useState('12');
  const [purpose, setPurpose] = useState('');
  const [eligibility, setEligibility] = useState<LoanEligibility | null>(null);
  const [calculation, setCalculation] = useState<LoanCalculatorOutput | null>(null);
  const [guarantors, setGuarantors] = useState<Guarantor[]>([]);
  const [loading, setLoading] = useState(false);

  const handleCheckEligibility = async () => {
    setLoading(true);
    try {
      const result = await loanService.checkEligibility('current-member-id', loanType, parseFloat(amount));
      setEligibility(result);
      
      if (result.isEligible) {
        const calc = await loanService.calculateLoan({
          loanType: loanType as any,
          principalAmount: parseFloat(amount),
          tenor: parseInt(tenor),
        });
        setCalculation(calc);
        setStep(2);
      }
    } catch (error) {
      console.error('Failed to check eligibility:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async () => {
    setLoading(true);
    try {
      await loanService.createApplication({
        memberId: 'current-member-id',
        loanType: loanType as any,
        requestedAmount: parseFloat(amount),
        purpose,
        tenor: parseInt(tenor),
        guarantors,
      });
      alert('Loan application submitted successfully!');
    } catch (error) {
      console.error('Failed to submit application:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="p-6 max-w-4xl mx-auto">
      <h1 className="text-2xl font-bold mb-6">Loan Application</h1>

      {/* Progress Steps */}
      <div className="flex items-center justify-between mb-8">
        {[1, 2, 3].map((s) => (
          <div key={s} className="flex items-center flex-1">
            <div className={`w-10 h-10 rounded-full flex items-center justify-center ${
              step >= s ? 'bg-primary-600 text-white' : 'bg-neutral-200 text-neutral-600'
            }`}>
              {s}
            </div>
            {s < 3 && <div className={`flex-1 h-1 mx-2 ${step > s ? 'bg-primary-600' : 'bg-neutral-200'}`} />}
          </div>
        ))}
      </div>

      {/* Step 1: Loan Details */}
      {step === 1 && (
        <Card className="p-6">
          <h2 className="text-lg font-semibold mb-4">Loan Details</h2>
          
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-2">Loan Type</label>
              <select
                value={loanType}
                onChange={(e) => setLoanType(e.target.value)}
                className="w-full px-3 py-2 border border-neutral-300 rounded-lg"
              >
                <option value="normal">Normal Loan (200% of Savings)</option>
                <option value="commodity">Commodity Loan (300% of Savings)</option>
                <option value="car">Car Loan (500% of Savings)</option>
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">Requested Amount (₦)</label>
              <Input
                type="number"
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
                placeholder="Enter amount"
              />
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">Tenor (Months)</label>
              <Input
                type="number"
                value={tenor}
                onChange={(e) => setTenor(e.target.value)}
                placeholder="Enter tenor"
              />
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">Purpose</label>
              <textarea
                value={purpose}
                onChange={(e) => setPurpose(e.target.value)}
                className="w-full px-3 py-2 border border-neutral-300 rounded-lg"
                rows={3}
                placeholder="Describe the purpose of this loan"
              />
            </div>

            <Button
              variant="primary"
              onClick={handleCheckEligibility}
              disabled={!amount || !tenor || !purpose || loading}
              className="w-full"
            >
              <Calculator className="w-4 h-4 mr-2" />
              Check Eligibility & Calculate
            </Button>
          </div>
        </Card>
      )}

      {/* Step 2: Eligibility & Calculation */}
      {step === 2 && eligibility && calculation && (
        <Card className="p-6">
          <h2 className="text-lg font-semibold mb-4">Eligibility & Calculation</h2>

          {eligibility.isEligible ? (
            <div className="space-y-4">
              <div className="p-4 bg-success-50 border border-success-200 rounded-lg">
                <div className="flex items-center space-x-2 mb-2">
                  <CheckCircle className="w-5 h-5 text-success-600" />
                  <span className="font-semibold text-success-900">You are eligible for this loan!</span>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="p-4 bg-neutral-50 rounded-lg">
                  <div className="text-sm text-neutral-600">Monthly EMI</div>
                  <div className="text-2xl font-bold">{loanService.formatCurrency(calculation.monthlyEMI)}</div>
                </div>
                <div className="p-4 bg-neutral-50 rounded-lg">
                  <div className="text-sm text-neutral-600">Total Interest</div>
                  <div className="text-2xl font-bold">{loanService.formatCurrency(calculation.totalInterest)}</div>
                </div>
                <div className="p-4 bg-neutral-50 rounded-lg">
                  <div className="text-sm text-neutral-600">Total Repayment</div>
                  <div className="text-2xl font-bold">{loanService.formatCurrency(calculation.totalRepayment)}</div>
                </div>
                <div className="p-4 bg-neutral-50 rounded-lg">
                  <div className="text-sm text-neutral-600">Deduction Headroom</div>
                  <div className="text-2xl font-bold">{eligibility.deductionRateHeadroom.toFixed(1)}%</div>
                </div>
              </div>

              <div className="flex space-x-3">
                <Button variant="outline" onClick={() => setStep(1)}>Back</Button>
                <Button variant="primary" onClick={() => setStep(3)} className="flex-1">
                  <Users className="w-4 h-4 mr-2" />
                  Add Guarantors
                </Button>
              </div>
            </div>
          ) : (
            <div className="p-4 bg-error-50 border border-error-200 rounded-lg">
              <div className="font-semibold text-error-900 mb-2">Not Eligible</div>
              <ul className="list-disc list-inside text-sm text-error-700">
                {eligibility.reasons.map((reason, index) => (
                  <li key={index}>{reason}</li>
                ))}
              </ul>
              <Button variant="outline" onClick={() => setStep(1)} className="mt-4">
                Back
              </Button>
            </div>
          )}
        </Card>
      )}

      {/* Step 3: Guarantors */}
      {step === 3 && (
        <Card className="p-6">
          <h2 className="text-lg font-semibold mb-4">Add Guarantors</h2>
          <p className="text-sm text-neutral-600 mb-4">
            You need at least 2 guarantors with sufficient free equity to proceed.
          </p>

          {/* Guarantor form would go here */}
          
          <div className="flex space-x-3 mt-6">
            <Button variant="outline" onClick={() => setStep(2)}>Back</Button>
            <Button
              variant="primary"
              onClick={handleSubmit}
              disabled={guarantors.length < 2 || loading}
              className="flex-1"
            >
              Submit Application
            </Button>
          </div>
        </Card>
      )}
    </div>
  );
};
