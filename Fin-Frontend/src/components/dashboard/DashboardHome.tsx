import React from 'react';
import { motion } from 'framer-motion';
import { useGetDashboardOverviewQuery } from '../../services/dashboardApi';
import StatsCard from '../common/StatsCard';
import ChartContainer from '../common/ChartContainer';
import {
  Users,
  Wallet,
  TrendingUp,
  Activity,
} from 'lucide-react';

const DashboardHome: React.FC = () => {
  const { data: overview, isLoading, error } = useGetDashboardOverviewQuery();

  if (isLoading) {
    return (
      <div className="animate-pulse space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {[...Array(4)].map((_, i) => (
            <div key={i} className="h-32 bg-gray-200 rounded-xl"></div>
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center h-64">
        <p className="text-red-600">Failed to load dashboard data</p>
      </div>
    );
  }

  const stats = [
    {
      title: 'Total Customers',
      value: overview?.data?.totalCustomers?.toLocaleString() || '0',
      change: '+12%',
      trend: 'up' as const,
      icon: Users,
      color: 'emerald' as const,
    },
    {
      title: 'Deposit Accounts',
      value: overview?.data?.totalDepositAccounts?.toLocaleString() || '0',
      change: '+8%',
      trend: 'up' as const,
      icon: Wallet,
      color: 'blue' as const,
    },
    {
      title: 'Total Deposits',
      value: `₦${overview?.data?.totalDeposits?.toLocaleString() || '0'}`,
      change: '+15%',
      trend: 'up' as const,
      icon: TrendingUp,
      color: 'green' as const,
    },
    {
      title: "Today's Transactions",
      value: overview?.data?.totalTransactionsToday?.toLocaleString() || '0',
      change: '+5%',
      trend: 'up' as const,
      icon: Activity,
      color: 'purple' as const,
    },
  ];

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
          <p className="text-gray-600">Welcome back! Here's what's happening today.</p>
        </div>
        <div className="text-sm text-gray-500">
          Last updated: {new Date().toLocaleTimeString()}
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
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
          title="Monthly Deposits Trend"
          subtitle="Deposit volume over the last 12 months"
          type="line"
          data={[
            { month: 'Jan', deposits: 4500000 },
            { month: 'Feb', deposits: 5200000 },
            { month: 'Mar', deposits: 4800000 },
            { month: 'Apr', deposits: 6100000 },
            { month: 'May', deposits: 5800000 },
            { month: 'Jun', deposits: 7200000 },
          ]}
          dataKey="deposits"
          xAxisKey="month"
          color="#059669"
        />
        
        <ChartContainer
          title="Customer Growth"
          subtitle="New customer acquisitions this year"
          type="bar"
          data={[
            { month: 'Jan', customers: 45 },
            { month: 'Feb', customers: 52 },
            { month: 'Mar', customers: 48 },
            { month: 'Apr', customers: 61 },
            { month: 'May', customers: 58 },
            { month: 'Jun', customers: 72 },
          ]}
          dataKey="customers"
          xAxisKey="month"
          color="#3b82f6"
        />
      </div>

      {/* Quick Actions */}
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.5 }}
        className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
      >
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Quick Actions</h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <Users className="w-6 h-6 text-emerald-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Add Customer</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <Wallet className="w-6 h-6 text-blue-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Open Account</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <TrendingUp className="w-6 h-6 text-green-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Process Transaction</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <Activity className="w-6 h-6 text-purple-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">View Reports</span>
          </button>
        </div>
      </motion.div>
    </div>
  );
};

export default DashboardHome;