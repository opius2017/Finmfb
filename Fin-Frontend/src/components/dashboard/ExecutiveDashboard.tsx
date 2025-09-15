import React from 'react';
import { motion } from 'framer-motion';
import {
  TrendingUp,
  TrendingDown,
  DollarSign,
  Users,
  Building,
  Shield,
  AlertTriangle,
  CheckCircle,
  BarChart3,
  PieChart,
} from 'lucide-react';
import { useGetExecutiveDashboardQuery } from '../../services/dashboardApi';
import StatsCard from '../common/StatsCard';
import ChartContainer from '../common/ChartContainer';

const ExecutiveDashboard: React.FC = () => {
  const { data: dashboard, isLoading, error } = useGetExecutiveDashboardQuery();

  if (isLoading) {
    return (
      <div className="animate-pulse space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {[...Array(8)].map((_, i) => (
            <div key={i} className="h-32 bg-gray-200 rounded-xl"></div>
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center h-64">
        <p className="text-red-600">Failed to load executive dashboard</p>
      </div>
    );
  }

  const stats = [
    {
      title: 'Total Assets',
      value: `₦${dashboard?.data?.totalAssets?.toLocaleString() || '0'}`,
      change: '+8.2%',
      trend: 'up' as const,
      icon: Building,
      color: 'emerald' as const,
    },
    {
      title: 'Net Worth',
      value: `₦${dashboard?.data?.netWorth?.toLocaleString() || '0'}`,
      change: '+12.5%',
      trend: 'up' as const,
      icon: TrendingUp,
      color: 'green' as const,
    },
    {
      title: 'Monthly Revenue',
      value: `₦${dashboard?.data?.monthlyRevenue?.toLocaleString() || '0'}`,
      change: '+15.3%',
      trend: 'up' as const,
      icon: DollarSign,
      color: 'blue' as const,
    },
    {
      title: 'Net Income',
      value: `₦${dashboard?.data?.netIncome?.toLocaleString() || '0'}`,
      change: '+18.7%',
      trend: 'up' as const,
      icon: TrendingUp,
      color: 'purple' as const,
    },
    {
      title: 'Customer Growth',
      value: `${dashboard?.data?.customerGrowthRate?.toFixed(1) || '0'}%`,
      change: '+5.2%',
      trend: 'up' as const,
      icon: Users,
      color: 'emerald' as const,
    },
    {
      title: 'Portfolio at Risk',
      value: `${dashboard?.data?.portfolioAtRisk?.toFixed(1) || '0'}%`,
      change: '-2.1%',
      trend: 'down' as const,
      icon: AlertTriangle,
      color: dashboard?.data?.portfolioAtRisk > 5 ? 'red' as const : 'yellow' as const,
    },
    {
      title: 'Capital Adequacy',
      value: `${dashboard?.data?.capitalAdequacyRatio?.toFixed(1) || '0'}%`,
      change: '+1.8%',
      trend: 'up' as const,
      icon: Shield,
      color: dashboard?.data?.capitalAdequacyRatio > 15 ? 'green' as const : 'yellow' as const,
    },
    {
      title: 'Liquidity Ratio',
      value: `${dashboard?.data?.liquidityRatio?.toFixed(1) || '0'}%`,
      change: '+3.2%',
      trend: 'up' as const,
      icon: CheckCircle,
      color: dashboard?.data?.liquidityRatio > 30 ? 'green' as const : 'yellow' as const,
    },
  ];

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Executive Dashboard</h1>
          <p className="text-gray-600">Strategic overview and key performance indicators</p>
        </div>
        <div className="flex items-center space-x-3">
          <button className="flex items-center px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
            <BarChart3 className="w-4 h-4 mr-2" />
            Generate Report
          </button>
          <button className="flex items-center px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 transition-colors">
            <PieChart className="w-4 h-4 mr-2" />
            Board Report
          </button>
        </div>
      </div>

      {/* Key Performance Indicators */}
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

      {/* Financial Performance Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <ChartContainer
          title="Revenue Trend"
          subtitle="Monthly revenue performance over the last 12 months"
          type="line"
          data={dashboard?.data?.revenueByMonth?.map(item => ({
            month: item.month,
            revenue: item.value
          })) || []}
          dataKey="revenue"
          xAxisKey="month"
          color="#059669"
        />
        
        <ChartContainer
          title="Risk Metrics"
          subtitle="Current risk exposure across different categories"
          type="bar"
          data={[
            { category: 'Credit Risk', value: dashboard?.data?.riskMetrics?.creditRisk || 0 },
            { category: 'Operational Risk', value: dashboard?.data?.riskMetrics?.operationalRisk || 0 },
            { category: 'Market Risk', value: dashboard?.data?.riskMetrics?.marketRisk || 0 },
          ]}
          dataKey="value"
          xAxisKey="category"
          color="#dc2626"
        />
      </div>

      {/* Branch Performance and Compliance */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Top Performing Branches */}
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.5 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Top Performing Branches</h3>
          <div className="space-y-4">
            {dashboard?.data?.topPerformingBranches?.slice(0, 5).map((branch, index) => (
              <div key={branch.branchName} className="flex items-center justify-between">
                <div className="flex items-center">
                  <div className="w-8 h-8 bg-emerald-100 rounded-full flex items-center justify-center mr-3">
                    <span className="text-sm font-medium text-emerald-600">{index + 1}</span>
                  </div>
                  <span className="font-medium text-gray-900">{branch.branchName}</span>
                </div>
                <div className="text-right">
                  <span className="text-sm font-medium text-gray-900">
                    ₦{branch.performance.toLocaleString()}
                  </span>
                </div>
              </div>
            )) || (
              <div className="text-center py-8 text-gray-500">
                <Building className="w-12 h-12 mx-auto mb-2 opacity-50" />
                <p>No branch data available</p>
              </div>
            )}
          </div>
        </motion.div>

        {/* Regulatory Compliance Status */}
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.6 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Regulatory Compliance</h3>
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <span className="text-gray-700">Capital Adequacy Ratio</span>
              <div className="flex items-center">
                <div className="w-24 bg-gray-200 rounded-full h-2 mr-3">
                  <div 
                    className={`h-2 rounded-full ${
                      (dashboard?.data?.capitalAdequacyRatio || 0) >= 15 ? 'bg-green-500' : 'bg-yellow-500'
                    }`}
                    style={{ width: `${Math.min((dashboard?.data?.capitalAdequacyRatio || 0) / 20 * 100, 100)}%` }}
                  ></div>
                </div>
                <span className="text-sm font-medium">
                  {dashboard?.data?.capitalAdequacyRatio?.toFixed(1) || '0'}%
                </span>
              </div>
            </div>
            
            <div className="flex items-center justify-between">
              <span className="text-gray-700">Liquidity Ratio</span>
              <div className="flex items-center">
                <div className="w-24 bg-gray-200 rounded-full h-2 mr-3">
                  <div 
                    className={`h-2 rounded-full ${
                      (dashboard?.data?.liquidityRatio || 0) >= 30 ? 'bg-green-500' : 'bg-yellow-500'
                    }`}
                    style={{ width: `${Math.min((dashboard?.data?.liquidityRatio || 0) / 50 * 100, 100)}%` }}
                  ></div>
                </div>
                <span className="text-sm font-medium">
                  {dashboard?.data?.liquidityRatio?.toFixed(1) || '0'}%
                </span>
              </div>
            </div>
            
            <div className="flex items-center justify-between">
              <span className="text-gray-700">Portfolio at Risk</span>
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
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Executive Actions</h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <BarChart3 className="w-6 h-6 text-emerald-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Board Report</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <Shield className="w-6 h-6 text-blue-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Risk Assessment</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <Users className="w-6 h-6 text-green-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Strategic Planning</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <Building className="w-6 h-6 text-purple-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Branch Performance</span>
          </button>
        </div>
      </motion.div>
    </div>
  );
};

export default ExecutiveDashboard;