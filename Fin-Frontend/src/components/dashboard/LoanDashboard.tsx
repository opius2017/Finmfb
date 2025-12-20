// @ts-nocheck
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
  ArrowRight,
  Activity,
  PieChart
} from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { useGetLoanDashboardQuery } from '../../services/dashboardApi';
import StatsCard from '../common/StatsCard';
import ChartContainer from '../common/ChartContainer';

const LoanDashboard: React.FC = () => {
  const navigate = useNavigate();
  const { data: dashboard, isLoading, error } = useGetLoanDashboardQuery();

  const containerVariants = {
    hidden: { opacity: 0 },
    visible: {
      opacity: 1,
      transition: {
        staggerChildren: 0.1
      }
    }
  };

  const itemVariants = {
    hidden: { y: 20, opacity: 0 },
    visible: {
      y: 0,
      opacity: 1,
      transition: { type: "spring", stiffness: 100 }
    }
  };

  if (isLoading) {
    return (
      <div className="animate-pulse space-y-8 p-6">
        <div className="h-10 w-1/3 bg-gray-200 rounded-lg"></div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {[...Array(4)].map((_, i) => (
            <div key={i} className="h-40 bg-gray-200 rounded-2xl"></div>
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center h-screen bg-gray-50">
        <div className="text-center p-8 bg-white rounded-2xl shadow-xl">
          <AlertTriangle className="w-16 h-16 text-red-500 mx-auto mb-4" />
          <h2 className="text-2xl font-bold text-gray-800 mb-2">Dashboard Unavailable</h2>
          <p className="text-gray-600 mb-6">We couldn't load your loan data. Please try again.</p>
          <button
            onClick={() => window.location.reload()}
            className="px-6 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
          >
            Retry Connection
          </button>
        </div>
      </div>
    );
  }

  const stats = [
    {
      title: 'Total Portfolio',
      value: `₦${dashboard?.data?.totalPortfolio?.toLocaleString() || '0'}`,
      change: '+12.5%',
      trend: 'up' as const,
      icon: PieChart,
      color: 'emerald' as const,
      description: 'Total principal outstanding'
    },
    {
      title: 'Active Loans',
      value: dashboard?.data?.activeLoans?.toLocaleString() || '0',
      change: '+8.3%',
      trend: 'up' as const,
      icon: Users,
      color: 'blue' as const,
      description: 'Currently servicing borrowers'
    },
    {
      title: 'Portfolio at Risk',
      value: `${dashboard?.data?.portfolioAtRisk?.toFixed(1) || '0'}%`,
      change: '-1.8%',
      trend: 'down' as const,
      icon: AlertTriangle,
      color: (dashboard?.data?.portfolioAtRisk || 0) > 5 ? 'red' as const : 'yellow' as const,
      description: 'Loans overdue > 30 days'
    },
    {
      title: 'Interest Yield',
      value: `${dashboard?.data?.averageInterestRate?.toFixed(1) || '0'}%`,
      change: '+0.5%',
      trend: 'up' as const,
      icon: TrendingUp,
      color: 'purple' as const,
      description: 'Weighted avg. interest rate'
    },
  ];

  const quickActions = [
    {
      title: 'Process Application',
      icon: FileText,
      color: 'bg-indigo-500',
      path: '/loans/applications',
      desc: 'Review and approve pending requests'
    },
    {
      title: 'Disburse Loan',
      icon: DollarSign,
      color: 'bg-emerald-500',
      path: '/loans/disbursement',
      desc: 'Release funds to approved borrowers'
    },
    {
      title: 'Review NPLs',
      icon: AlertTriangle,
      color: 'bg-rose-500',
      path: '/loans/delinquency',
      desc: 'Manage overdue and non-performing loans'
    },
    {
      title: 'Generate Report',
      icon: BarChart3,
      color: 'bg-amber-500',
      path: '/loans/reports',
      desc: 'Create portfolio and regulatory reports'
    }
  ];

  return (
    <motion.div
      variants={containerVariants}
      initial="hidden"
      animate="visible"
      className="space-y-8 p-6 bg-gray-50/50 min-h-screen"
    >
      {/* Page Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-extrabold text-gray-900 tracking-tight">
            Loan Portfolio
          </h1>
          <p className="text-gray-500 mt-1 text-lg">
            Real-time performance metrics and management
          </p>
        </div>
        <div className="flex items-center space-x-3">
          <motion.button
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
            onClick={() => navigate('/loans/new')}
            className="flex items-center px-6 py-3 bg-gradient-to-r from-emerald-600 to-emerald-500 text-white rounded-xl shadow-lg shadow-emerald-200 hover:shadow-emerald-300 transition-all font-medium"
          >
            <CreditCard className="w-5 h-5 mr-2" />
            New Loan Application
          </motion.button>
        </div>
      </div>

      {/* Quick Actions Grid - Glassmorphism */}
      <motion.div variants={itemVariants} className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {quickActions.map((action, index) => (
          <motion.div
            key={action.title}
            whileHover={{ y: -5, transition: { duration: 0.2 } }}
            onClick={() => navigate(action.path)}
            className="group relative overflow-hidden bg-white/70 backdrop-blur-lg border border-white/20 rounded-2xl p-6 shadow-sm hover:shadow-xl transition-all cursor-pointer"
          >
            <div className={`absolute top-0 right-0 w-24 h-24 -mr-8 -mt-8 rounded-full opacity-10 ${action.color}`}></div>

            <div className="flex items-start justify-between mb-4">
              <div className={`p-3 rounded-xl ${action.color} text-white shadow-md`}>
                <action.icon className="w-6 h-6" />
              </div>
              <ArrowRight className="w-5 h-5 text-gray-300 group-hover:text-gray-600 transition-colors" />
            </div>

            <h3 className="text-lg font-bold text-gray-900 group-hover:text-indigo-600 transition-colors">
              {action.title}
            </h3>
            <p className="text-sm text-gray-500 mt-1 leading-relaxed">
              {action.desc}
            </p>
          </motion.div>
        ))}
      </motion.div>

      {/* Portfolio Metrics */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {stats.map((stat, index) => (
          <motion.div key={stat.title} variants={itemVariants}>
            <StatsCard {...stat} />
          </motion.div>
        ))}
      </div>

      {/* Main Content Areas */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Charts Column (Span 2) */}
        <div className="lg:col-span-2 space-y-8">
          <motion.div
            variants={itemVariants}
            className="bg-white rounded-3xl shadow-sm border border-gray-100 p-6"
          >
            <div className="flex items-center justify-between mb-6">
              <h3 className="text-xl font-bold text-gray-900">Disbursement Trends</h3>
              <select className="bg-gray-50 border-none text-sm font-medium text-gray-600 rounded-lg p-2 cursor-pointer hover:bg-gray-100 transition-colors">
                <option>Last 12 Months</option>
                <option>This Year</option>
                <option>All Time</option>
              </select>
            </div>
            <ChartContainer
              title=""
              subtitle=""
              type="line"
              data={dashboard?.data?.monthlyDisbursements?.map(item => ({
                month: item.month,
                disbursements: item.value
              })) || []}
              dataKey="disbursements"
              xAxisKey="month"
              color="#10b981"
            />
          </motion.div>

          <motion.div
            variants={itemVariants}
            className="bg-white rounded-3xl shadow-sm border border-gray-100 p-6"
          >
            <h3 className="text-xl font-bold text-gray-900 mb-6">Repayment Efficiency</h3>
            <ChartContainer
              title=""
              subtitle=""
              type="bar"
              data={dashboard?.data?.repaymentTrends?.map(item => ({
                month: item.month,
                repayments: item.value
              })) || []}
              dataKey="repayments"
              xAxisKey="month"
              color="#6366f1"
            />
          </motion.div>
        </div>

        {/* Side Panel (Span 1) */}
        <div className="space-y-8">
          {/* Risk Allocation - Heatmap Style List */}
          <motion.div
            variants={itemVariants}
            className="bg-white rounded-3xl shadow-sm border border-gray-100 overflow-hidden"
          >
            <div className="p-6 border-b border-gray-50">
              <h3 className="text-lg font-bold text-gray-900 flex items-center">
                <Activity className="w-5 h-5 mr-2 text-indigo-500" />
                Portfolio Quality
              </h3>
            </div>
            <div className="p-4 space-y-3">
              {dashboard?.data?.loansByClassification?.map((item) => {
                const getColors = (name: string) => {
                  switch (name.toLowerCase()) {
                    case 'performing': return 'bg-emerald-50 text-emerald-700 border-emerald-100';
                    case 'special mention': return 'bg-amber-50 text-amber-700 border-amber-100';
                    case 'substandard': return 'bg-orange-50 text-orange-700 border-orange-100';
                    case 'doubtful': return 'bg-rose-50 text-rose-700 border-rose-100';
                    case 'lost': return 'bg-red-50 text-red-700 border-red-100';
                    default: return 'bg-gray-50 text-gray-700 border-gray-100';
                  }
                };
                const total = dashboard?.data?.totalPortfolio || 1; // avoid div/0
                const percent = ((item.amount / total) * 100).toFixed(1);

                return (
                  <div key={item.classification} className={`flex items-center justify-between p-3 rounded-xl border ${getColors(item.classification)}`}>
                    <div>
                      <span className="text-sm font-semibold">{item.classification}</span>
                      <div className="text-xs opacity-80">{item.count} loans</div>
                    </div>
                    <div className="text-right">
                      <div className="text-sm font-bold">₦{item.amount.toLocaleString()}</div>
                      <div className="text-xs font-medium bg-white/50 px-2 py-0.5 rounded-full inline-block mt-1">
                        {percent}%
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          </motion.div>

          {/* Quick Stats Grid */}
          <motion.div variants={itemVariants} className="grid grid-cols-2 gap-4">
            <div className="bg-indigo-600 rounded-3xl p-5 text-white shadow-lg shadow-indigo-200">
              <div className="opacity-80 text-sm font-medium mb-1">Total Provisions</div>
              <div className="text-xl font-bold tracking-tight">
                ₦{dashboard?.data?.totalProvisions?.toLocaleString() || '0'}
              </div>
            </div>
            <div className="bg-emerald-600 rounded-3xl p-5 text-white shadow-lg shadow-emerald-200">
              <div className="opacity-80 text-sm font-medium mb-1">Collection Rate</div>
              <div className="text-xl font-bold tracking-tight">92.4%</div>
            </div>
          </motion.div>
        </div>
      </div>
    </motion.div>
  );
};

export default LoanDashboard;