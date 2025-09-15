import React from 'react';
import { motion } from 'framer-motion';
import {
  CreditCard,
  TrendingUp,
  AlertTriangle,
  CheckCircle,
  DollarSign,
  Users,
  FileText,
  BarChart3,
} from 'lucide-react';
import { useGetLoanDashboardQuery } from '../../services/dashboardApi';
import StatsCard from '../common/StatsCard';
import ChartContainer from '../common/ChartContainer';

const LoanDashboard: React.FC = () => {
  const { data: dashboard, isLoading, error } = useGetLoanDashboardQuery();

  if (isLoading) {
    return (
      <div className="animate-pulse space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {[...Array(6)].map((_, i) => (
            <div key={i} className="h-32 bg-gray-200 rounded-xl"></div>
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center h-64">
        <p className="text-red-600">Failed to load loan dashboard</p>
      </div>
    );
  }

  const stats = [
    {
      title: 'Total Portfolio',
      value: `₦${dashboard?.data?.totalPortfolio?.toLocaleString() || '0'}`,
      change: '+12.5%',
      trend: 'up' as const,
      icon: CreditCard,
      color: 'emerald' as const,
    },
    {
      title: 'Active Loans',
      value: dashboard?.data?.activeLoans?.toLocaleString() || '0',
      change: '+8.3%',
      trend: 'up' as const,
      icon: FileText,
      color: 'blue' as const,
    },
    {
      title: 'Performing Loans',
      value: dashboard?.data?.performingLoans?.toLocaleString() || '0',
      change: '+5.2%',
      trend: 'up' as const,
      icon: CheckCircle,
      color: 'green' as const,
    },
    {
      title: 'Non-Performing',
      value: dashboard?.data?.nonPerformingLoans?.toLocaleString() || '0',
      change: '-2.1%',
      trend: 'down' as const,
      icon: AlertTriangle,
      color: 'red' as const,
    },
    {
      title: 'Portfolio at Risk',
      value: `${dashboard?.data?.portfolioAtRisk?.toFixed(1) || '0'}%`,
      change: '-1.8%',
      trend: 'down' as const,
      icon: AlertTriangle,
      color: dashboard?.data?.portfolioAtRisk > 5 ? 'red' as const : 'yellow' as const,
    },
    {
      title: 'Avg Interest Rate',
      value: `${dashboard?.data?.averageInterestRate?.toFixed(1) || '0'}%`,
      change: '+0.5%',
      trend: 'up' as const,
      icon: TrendingUp,
      color: 'purple' as const,
    },
  ];

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Loan Portfolio Dashboard</h1>
          <p className="text-gray-600">Monitor loan performance and credit risk metrics</p>
        </div>
        <div className="flex items-center space-x-3">
          <button className="flex items-center px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
            <BarChart3 className="w-4 h-4 mr-2" />
            Portfolio Report
          </button>
          <button className="flex items-center px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 transition-colors">
            <CreditCard className="w-4 h-4 mr-2" />
            New Loan
          </button>
        </div>
      </div>

      {/* Portfolio Metrics */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {stats.map((stat, index) => (
          <motion.div
            key={stat.title}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: index * 0.1 }}
          >
            <StatsCard {...stat} />
          </motion.div>
        ))}
      </div>

      {/* Charts Section */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <ChartContainer
          title="Monthly Disbursements"
          subtitle="Loan disbursement trends over the last 12 months"
          type="line"
          data={dashboard?.data?.monthlyDisbursements?.map(item => ({
            month: item.month,
            disbursements: item.value
          })) || []}
          dataKey="disbursements"
          xAxisKey="month"
          color="#059669"
        />
        
        <ChartContainer
          title="Repayment Performance"
          subtitle="Monthly repayment collection trends"
          type="bar"
          data={dashboard?.data?.repaymentTrends?.map(item => ({
            month: item.month,
            repayments: item.value
          })) || []}
          dataKey="repayments"
          xAxisKey="month"
          color="#3b82f6"
        />
      </div>

      {/* Loan Classification and Risk Analysis */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Loan Classification Breakdown */}
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.5 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Loan Classification</h3>
          <div className="space-y-4">
            {dashboard?.data?.loansByClassification?.map((classification, index) => {
              const getClassificationColor = (name: string) => {
                switch (name.toLowerCase()) {
                  case 'performing': return 'bg-green-500';
                  case 'special mention': return 'bg-yellow-500';
                  case 'substandard': return 'bg-orange-500';
                  case 'doubtful': return 'bg-red-500';
                  case 'lost': return 'bg-red-700';
                  default: return 'bg-gray-500';
                }
              };

              const totalPortfolio = dashboard?.data?.totalPortfolio || 1;
              const percentage = (classification.amount / totalPortfolio) * 100;

              return (
                <div key={classification.classification} className="flex items-center justify-between">
                  <div className="flex items-center">
                    <div className={`w-4 h-4 rounded-full ${getClassificationColor(classification.classification)} mr-3`}></div>
                    <div>
                      <span className="font-medium text-gray-900">{classification.classification}</span>
                      <div className="text-sm text-gray-500">{classification.count} loans</div>
                    </div>
                  </div>
                  <div className="text-right">
                    <span className="text-sm font-medium text-gray-900">
                      ₦{classification.amount.toLocaleString()}
                    </span>
                    <div className="text-sm text-gray-500">{percentage.toFixed(1)}%</div>
                  </div>
                </div>
              );
            }) || (
              <div className="text-center py-8 text-gray-500">
                <CreditCard className="w-12 h-12 mx-auto mb-2 opacity-50" />
                <p>No classification data available</p>
              </div>
            )}
          </div>
        </motion.div>

        {/* Risk Indicators */}
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.6 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Risk Indicators</h3>
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <span className="text-gray-700">Portfolio at Risk (PAR)</span>
              <div className="flex items-center">
                <div className="w-24 bg-gray-200 rounded-full h-2 mr-3">
                  <div 
                    className={`h-2 rounded-full ${
                      (dashboard?.data?.portfolioAtRisk || 0) <= 5 ? 'bg-green-500' : 'bg-red-500'
                    }`}
                    style={{ width: `${Math.min((dashboard?.data?.portfolioAtRisk || 0) / 10 * 100, 100)}%` }}
                  ></div>
                </div>
                <span className="text-sm font-medium">
                  {dashboard?.data?.portfolioAtRisk?.toFixed(1) || '0'}%
                </span>
              </div>
            </div>
            
            <div className="flex items-center justify-between">
              <span className="text-gray-700">Provision Coverage</span>
              <div className="flex items-center">
                <div className="w-24 bg-gray-200 rounded-full h-2 mr-3">
                  <div className="h-2 rounded-full bg-blue-500" style={{ width: '85%' }}></div>
                </div>
                <span className="text-sm font-medium">85%</span>
              </div>
            </div>
            
            <div className="flex items-center justify-between">
              <span className="text-gray-700">Collection Efficiency</span>
              <div className="flex items-center">
                <div className="w-24 bg-gray-200 rounded-full h-2 mr-3">
                  <div className="h-2 rounded-full bg-green-500" style={{ width: '92%' }}></div>
                </div>
                <span className="text-sm font-medium">92%</span>
              </div>
            </div>

            <div className="pt-4 border-t border-gray-200">
              <div className="flex items-center justify-between text-sm">
                <span className="text-gray-600">Total Provisions</span>
                <span className="font-medium">₦{dashboard?.data?.totalProvisions?.toLocaleString() || '0'}</span>
              </div>
            </div>
          </div>
        </motion.div>
      </div>

      {/* Quick Actions */}
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.7 }}
        className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
      >
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Loan Management Actions</h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <CreditCard className="w-6 h-6 text-emerald-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Process Application</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <DollarSign className="w-6 h-6 text-blue-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Disburse Loan</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <AlertTriangle className="w-6 h-6 text-orange-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Review NPLs</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <BarChart3 className="w-6 h-6 text-purple-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Generate Report</span>
          </button>
        </div>
      </motion.div>
    </div>
  );
};

export default LoanDashboard;