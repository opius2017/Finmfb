import React from 'react';
import { motion } from 'framer-motion';
import {
  Wallet,
  TrendingUp,
  TrendingDown,
  Users,
  DollarSign,
  Calendar,
  BarChart3,
  // ...existing code...
  Plus,
  // ...existing code...
} from 'lucide-react';
import { useGetDepositDashboardQuery, useRunDepositSweepsMutation, useTrackDormancyMutation } from '../../services/dashboardApi';
import StatsCard from '../common/StatsCard';
import ChartContainer from '../common/ChartContainer';

const DepositDashboard: React.FC = () => {
  const { data: dashboard, isLoading, error } = useGetDepositDashboardQuery();
  const [runSweeps, { data: sweepResults, isLoading: sweepsLoading }] = useRunDepositSweepsMutation();
  const [trackDormancy, { data: dormancyResults, isLoading: dormancyLoading }] = useTrackDormancyMutation();

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
        <p className="text-red-600">Failed to load deposit dashboard</p>
      </div>
    );
  }

  const stats = [
    {
      title: 'Total Deposits',
      value: `₦${dashboard?.data?.totalDeposits?.toLocaleString() || '0'}`,
      change: '+15.3%',
      trend: 'up' as const,
      icon: Wallet,
      color: 'emerald' as const,
    },
    {
      title: 'Active Accounts',
      value: dashboard?.data?.activeAccounts?.toLocaleString() || '0',
      change: '+8.7%',
      trend: 'up' as const,
      icon: Users,
      color: 'blue' as const,
    },
    {
      title: 'New Accounts',
      value: dashboard?.data?.newAccountsThisMonth?.toLocaleString() || '0',
      change: '+12.1%',
      trend: 'up' as const,
      icon: TrendingUp,
      color: 'green' as const,
    },
    {
      title: 'Avg Balance',
      value: `₦${dashboard?.data?.averageAccountBalance?.toLocaleString() || '0'}`,
      change: '+5.4%',
      trend: 'up' as const,
      icon: DollarSign,
      color: 'purple' as const,
    },
    {
      title: 'Interest Paid',
      value: `₦${dashboard?.data?.totalInterestPaid?.toLocaleString() || '0'}`,
      change: '+18.2%',
      trend: 'up' as const,
      icon: TrendingUp,
      color: 'green' as const,
    },
    {
      title: 'Dormant Accounts',
      value: dashboard?.data?.dormantAccounts?.toLocaleString() || '0',
      change: '-3.1%',
      trend: 'down' as const,
      icon: TrendingDown,
      color: 'yellow' as const,
    },
  ];

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Deposit Portfolio Dashboard</h1>
          <p className="text-gray-600">Monitor deposit accounts and customer savings</p>
        </div>
        <div className="flex items-center space-x-3">
          <button className="flex items-center px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
            <BarChart3 className="w-4 h-4 mr-2" />
            Deposit Report
          </button>
          <button className="flex items-center px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 transition-colors">
            <Plus className="w-4 h-4 mr-2" />
            Open Account
          </button>
        </div>
      </div>

      {/* Deposit Metrics */}
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
          title="Monthly Deposit Growth"
          subtitle="Deposit volume trends over the last 12 months"
          type="line"
          data={dashboard?.data?.monthlyGrowth?.map(item => ({
            month: item.month,
            deposits: item.value
          })) || []}
          dataKey="deposits"
          xAxisKey="month"
          color="#059669"
        />
        
        <ChartContainer
          title="Transaction Volume"
          subtitle="Monthly transaction count and value"
          type="bar"
          data={dashboard?.data?.transactionVolume?.map(item => ({
            month: item.month,
            transactions: item.value
          })) || []}
          dataKey="transactions"
          xAxisKey="month"
          color="#3b82f6"
        />
      </div>

      {/* Product Performance and Account Analysis */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Deposits by Product */}
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.5 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Deposits by Product</h3>
          <div className="space-y-4">
            {dashboard?.data?.depositsByProduct?.map((product, index) => {
              const totalDeposits = dashboard?.data?.totalDeposits || 1;
              const percentage = (product.amount / totalDeposits) * 100;

              return (
                <div key={product.productName} className="flex items-center justify-between">
                  <div className="flex items-center">
                    <div className={`w-4 h-4 rounded-full mr-3 ${
                      index === 0 ? 'bg-emerald-500' :
                      index === 1 ? 'bg-blue-500' :
                      index === 2 ? 'bg-purple-500' : 'bg-gray-500'
                    }`}></div>
                    <div>
                      <span className="font-medium text-gray-900">{product.productName}</span>
                      <div className="text-sm text-gray-500">{product.count} accounts</div>
                    </div>
                  </div>
                  <div className="text-right">
                    <span className="text-sm font-medium text-gray-900">
                      ₦{product.amount.toLocaleString()}
                    </span>
                    <div className="text-sm text-gray-500">{percentage.toFixed(1)}%</div>
                  </div>
                </div>
              );
            }) || (
              <div className="text-center py-8 text-gray-500">
                <Wallet className="w-12 h-12 mx-auto mb-2 opacity-50" />
                <p>No product data available</p>
              </div>
            )}
          </div>
        </motion.div>

        {/* Account Status Overview */}
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.6 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Account Status Overview</h3>
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <span className="text-gray-700">Active Accounts</span>
              <div className="flex items-center">
                <div className="w-24 bg-gray-200 rounded-full h-2 mr-3">
                  <div className="h-2 rounded-full bg-green-500" style={{ width: '85%' }}></div>
                </div>
                <span className="text-sm font-medium">{dashboard?.data?.activeAccounts || 0}</span>
              </div>
            </div>
            
            <div className="flex items-center justify-between">
              <span className="text-gray-700">Dormant Accounts</span>
              <div className="flex items-center">
                <div className="w-24 bg-gray-200 rounded-full h-2 mr-3">
                  <div className="h-2 rounded-full bg-yellow-500" style={{ width: '15%' }}></div>
                </div>
                <span className="text-sm font-medium">{dashboard?.data?.dormantAccounts || 0}</span>
              </div>
            </div>
            
            <div className="flex items-center justify-between">
              <span className="text-gray-700">Average Balance</span>
              <div className="flex items-center">
                <DollarSign className="w-4 h-4 text-emerald-600 mr-2" />
                <span className="text-sm font-medium">
                  ₦{dashboard?.data?.averageAccountBalance?.toLocaleString() || '0'}
                </span>
              </div>
            </div>

            <div className="pt-4 border-t border-gray-200">
              <div className="flex items-center justify-between text-sm">
                <span className="text-gray-600">Interest Paid This Month</span>
                <span className="font-medium text-emerald-600">
                  ₦{dashboard?.data?.totalInterestPaid?.toLocaleString() || '0'}
                </span>
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
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Deposit Management Actions</h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <Plus className="w-6 h-6 text-emerald-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Open Account</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <DollarSign className="w-6 h-6 text-blue-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Process Transaction</span>
          </button>
          <button
            className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center"
            onClick={() => runSweeps()}
            disabled={sweepsLoading}
          >
            <BarChart3 className="w-6 h-6 text-purple-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">
              {sweepsLoading ? 'Running Sweeps...' : 'Run Automated Sweeps'}
            </span>
          </button>
          <button
            className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center"
            onClick={() => trackDormancy({ dormancyDays: 90 })}
            disabled={dormancyLoading}
          >
            <Calendar className="w-6 h-6 text-green-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">
              {dormancyLoading ? 'Tracking Dormancy...' : 'Track Dormant Accounts'}
            </span>
          </button>
        </div>
        {/* Results display */}
        {sweepResults && (
          <div className="mt-6">
            <h4 className="font-semibold mb-2">Sweep Results</h4>
            <ul className="list-disc ml-6 text-sm">
              {sweepResults.map((r, i) => (
                <li key={i} className={r.status === 'Success' ? 'text-emerald-700' : 'text-red-600'}>
                  {r.message}
                </li>
              ))}
            </ul>
          </div>
        )}
        {dormancyResults && (
          <div className="mt-6">
            <h4 className="font-semibold mb-2">Dormancy Tracking Results</h4>
            <ul className="list-disc ml-6 text-sm">
              {dormancyResults.map((r, i) => (
                <li key={i} className={r.status === 'Dormant' ? 'text-yellow-700' : 'text-gray-700'}>
                  {r.message}
                </li>
              ))}
            </ul>
          </div>
        )}
      </motion.div>
    </div>
  );
};

export default DepositDashboard;