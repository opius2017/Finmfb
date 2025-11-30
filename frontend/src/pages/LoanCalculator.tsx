import { useState } from 'react';
import { apiService } from '@/services/api';
import { toast } from 'sonner';
import { Calculator, TrendingUp, DollarSign, Calendar, Download } from 'lucide-react';

export default function LoanCalculator() {
  const [principal, setPrincipal] = useState(500000);
  const [rate, setRate] = useState(12);
  const [tenure, setTenure] = useState(12);
  const [result, setResult] = useState<any>(null);
  const [schedule, setSchedule] = useState<any>(null);
  const [loading, setLoading] = useState(false);

  const calculateEMI = async () => {
    setLoading(true);
    try {
      const data = await apiService.calculateEMI({
        principal,
        annualInterestRate: rate,
        tenureMonths: tenure,
      });
      setResult(data);
      toast.success('EMI calculated successfully');
    } catch (error) {
      toast.error('Failed to calculate EMI');
    } finally {
      setLoading(false);
    }
  };

  const generateSchedule = async () => {
    setLoading(true);
    try {
      const data = await apiService.generateAmortizationSchedule({
        principal,
        annualInterestRate: rate,
        tenureMonths: tenure,
        startDate: new Date().toISOString(),
        loanNumber: 'CALC-' + Date.now(),
      });
      setSchedule(data);
      toast.success('Schedule generated successfully');
    } catch (error) {
      toast.error('Failed to generate schedule');
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-NG', {
      style: 'currency',
      currency: 'NGN',
      minimumFractionDigits: 2,
    }).format(amount);
  };

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Loan Calculator</h1>
        <p className="text-gray-600 mt-1">
          Calculate your monthly EMI and view amortization schedule
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Input Form */}
        <div className="lg:col-span-1">
          <div className="card sticky top-24">
            <div className="flex items-center mb-6">
              <Calculator className="w-6 h-6 text-primary-600 mr-2" />
              <h2 className="text-lg font-semibold text-gray-900">
                Loan Details
              </h2>
            </div>

            <div className="space-y-6">
              {/* Principal Amount */}
              <div>
                <label className="label">Principal Amount (₦)</label>
                <input
                  type="number"
                  className="input"
                  value={principal}
                  onChange={(e) => setPrincipal(Number(e.target.value))}
                  min="10000"
                  step="10000"
                />
                <input
                  type="range"
                  className="w-full mt-2"
                  min="10000"
                  max="10000000"
                  step="10000"
                  value={principal}
                  onChange={(e) => setPrincipal(Number(e.target.value))}
                />
                <div className="flex justify-between text-xs text-gray-500 mt-1">
                  <span>₦10K</span>
                  <span>₦10M</span>
                </div>
              </div>

              {/* Interest Rate */}
              <div>
                <label className="label">Annual Interest Rate (%)</label>
                <input
                  type="number"
                  className="input"
                  value={rate}
                  onChange={(e) => setRate(Number(e.target.value))}
                  min="1"
                  max="50"
                  step="0.5"
                />
                <input
                  type="range"
                  className="w-full mt-2"
                  min="1"
                  max="50"
                  step="0.5"
                  value={rate}
                  onChange={(e) => setRate(Number(e.target.value))}
                />
                <div className="flex justify-between text-xs text-gray-500 mt-1">
                  <span>1%</span>
                  <span>50%</span>
                </div>
              </div>

              {/* Tenure */}
              <div>
                <label className="label">Loan Tenure (Months)</label>
                <input
                  type="number"
                  className="input"
                  value={tenure}
                  onChange={(e) => setTenure(Number(e.target.value))}
                  min="1"
                  max="360"
                />
                <input
                  type="range"
                  className="w-full mt-2"
                  min="1"
                  max="360"
                  value={tenure}
                  onChange={(e) => setTenure(Number(e.target.value))}
                />
                <div className="flex justify-between text-xs text-gray-500 mt-1">
                  <span>1 month</span>
                  <span>30 years</span>
                </div>
              </div>

              {/* Calculate Buttons */}
              <div className="space-y-2">
                <button
                  onClick={calculateEMI}
                  disabled={loading}
                  className="btn btn-primary w-full"
                >
                  {loading ? 'Calculating...' : 'Calculate EMI'}
                </button>
                <button
                  onClick={generateSchedule}
                  disabled={loading}
                  className="btn btn-secondary w-full"
                >
                  {loading ? 'Generating...' : 'Generate Schedule'}
                </button>
              </div>
            </div>
          </div>
        </div>

        {/* Results */}
        <div className="lg:col-span-2 space-y-6">
          {/* EMI Result Cards */}
          {result && (
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="card bg-gradient-to-br from-primary-500 to-primary-600 text-white">
                <div className="flex items-center justify-between mb-2">
                  <span className="text-sm opacity-90">Monthly EMI</span>
                  <DollarSign className="w-5 h-5 opacity-75" />
                </div>
                <p className="text-3xl font-bold">
                  {formatCurrency(result.monthlyEMI)}
                </p>
              </div>

              <div className="card bg-gradient-to-br from-green-500 to-green-600 text-white">
                <div className="flex items-center justify-between mb-2">
                  <span className="text-sm opacity-90">Total Interest</span>
                  <TrendingUp className="w-5 h-5 opacity-75" />
                </div>
                <p className="text-3xl font-bold">
                  {formatCurrency(result.totalInterest)}
                </p>
              </div>

              <div className="card bg-gradient-to-br from-purple-500 to-purple-600 text-white">
                <div className="flex items-center justify-between mb-2">
                  <span className="text-sm opacity-90">Total Payment</span>
                  <Calendar className="w-5 h-5 opacity-75" />
                </div>
                <p className="text-3xl font-bold">
                  {formatCurrency(result.totalPayment)}
                </p>
              </div>
            </div>
          )}

          {/* Amortization Schedule */}
          {schedule && (
            <div className="card">
              <div className="flex items-center justify-between mb-4">
                <h2 className="text-lg font-semibold text-gray-900">
                  Amortization Schedule
                </h2>
                <button className="btn btn-secondary text-sm">
                  <Download className="w-4 h-4 mr-2" />
                  Export
                </button>
              </div>

              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b-2 border-gray-200">
                      <th className="text-left py-3 px-2 font-semibold text-gray-700">
                        #
                      </th>
                      <th className="text-left py-3 px-2 font-semibold text-gray-700">
                        Due Date
                      </th>
                      <th className="text-right py-3 px-2 font-semibold text-gray-700">
                        Opening Balance
                      </th>
                      <th className="text-right py-3 px-2 font-semibold text-gray-700">
                        EMI
                      </th>
                      <th className="text-right py-3 px-2 font-semibold text-gray-700">
                        Principal
                      </th>
                      <th className="text-right py-3 px-2 font-semibold text-gray-700">
                        Interest
                      </th>
                      <th className="text-right py-3 px-2 font-semibold text-gray-700">
                        Closing Balance
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {schedule.installments.map((inst: any, index: number) => (
                      <tr
                        key={index}
                        className="border-b border-gray-100 hover:bg-gray-50"
                      >
                        <td className="py-3 px-2 font-medium text-gray-900">
                          {inst.installmentNumber}
                        </td>
                        <td className="py-3 px-2 text-gray-600">
                          {new Date(inst.dueDate).toLocaleDateString()}
                        </td>
                        <td className="py-3 px-2 text-right text-gray-900">
                          {formatCurrency(inst.openingBalance)}
                        </td>
                        <td className="py-3 px-2 text-right font-medium text-gray-900">
                          {formatCurrency(inst.emiAmount)}
                        </td>
                        <td className="py-3 px-2 text-right text-green-600">
                          {formatCurrency(inst.principalAmount)}
                        </td>
                        <td className="py-3 px-2 text-right text-red-600">
                          {formatCurrency(inst.interestAmount)}
                        </td>
                        <td className="py-3 px-2 text-right font-medium text-gray-900">
                          {formatCurrency(inst.closingBalance)}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                  <tfoot>
                    <tr className="bg-gray-50 font-semibold">
                      <td colSpan={4} className="py-3 px-2 text-right">
                        Total:
                      </td>
                      <td className="py-3 px-2 text-right text-green-600">
                        {formatCurrency(schedule.principal)}
                      </td>
                      <td className="py-3 px-2 text-right text-red-600">
                        {formatCurrency(schedule.totalInterest)}
                      </td>
                      <td className="py-3 px-2 text-right">
                        {formatCurrency(schedule.totalPayment)}
                      </td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            </div>
          )}

          {/* Empty State */}
          {!result && !schedule && (
            <div className="card text-center py-12">
              <Calculator className="w-16 h-16 text-gray-300 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">
                No Calculations Yet
              </h3>
              <p className="text-gray-600">
                Enter loan details and click calculate to see results
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
