import { 
  TrendingUp, Users, DollarSign, AlertCircle, 
  CheckCircle, Clock, FileText, ArrowUpRight 
} from 'lucide-react';
import { Link } from 'react-router-dom';

export default function Dashboard() {
  // Mock data - replace with actual API calls
  const stats = [
    {
      name: 'Total Loans',
      value: '1,234',
      change: '+12.5%',
      trend: 'up',
      icon: FileText,
      color: 'bg-blue-500',
    },
    {
      name: 'Active Members',
      value: '856',
      change: '+5.2%',
      trend: 'up',
      icon: Users,
      color: 'bg-green-500',
    },
    {
      name: 'Total Disbursed',
      value: '₦125.4M',
      change: '+18.3%',
      trend: 'up',
      icon: DollarSign,
      color: 'bg-purple-500',
    },
    {
      name: 'Delinquent Loans',
      value: '23',
      change: '-3.1%',
      trend: 'down',
      icon: AlertCircle,
      color: 'bg-red-500',
    },
  ];

  const recentApplications = [
    {
      id: '1',
      memberName: 'John Doe',
      memberNumber: 'MEM001',
      amount: 500000,
      status: 'PENDING',
      date: '2024-12-01',
    },
    {
      id: '2',
      memberName: 'Jane Smith',
      memberNumber: 'MEM002',
      amount: 750000,
      status: 'APPROVED',
      date: '2024-12-02',
    },
    {
      id: '3',
      memberName: 'Bob Johnson',
      memberNumber: 'MEM003',
      amount: 1000000,
      status: 'IN_REVIEW',
      date: '2024-12-03',
    },
  ];

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'APPROVED':
        return 'bg-green-100 text-green-800';
      case 'PENDING':
        return 'bg-yellow-100 text-yellow-800';
      case 'IN_REVIEW':
        return 'bg-blue-100 text-blue-800';
      case 'REJECTED':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
        <p className="text-gray-600 mt-1">
          Welcome back! Here's what's happening with your cooperative today.
        </p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {stats.map((stat) => {
          const Icon = stat.icon;
          return (
            <div key={stat.name} className="card">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-600 mb-1">{stat.name}</p>
                  <p className="text-2xl font-bold text-gray-900">{stat.value}</p>
                  <div className="flex items-center mt-2">
                    <span
                      className={`text-sm font-medium ${
                        stat.trend === 'up' ? 'text-green-600' : 'text-red-600'
                      }`}
                    >
                      {stat.change}
                    </span>
                    <span className="text-sm text-gray-500 ml-2">vs last month</span>
                  </div>
                </div>
                <div className={`${stat.color} p-3 rounded-lg`}>
                  <Icon className="w-6 h-6 text-white" />
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Quick Actions */}
      <div className="card">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Quick Actions</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <Link
            to="/applications/new"
            className="flex items-center p-4 border-2 border-dashed border-gray-300 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <FileText className="w-8 h-8 text-primary-600 mr-3" />
            <div>
              <p className="font-medium text-gray-900">New Loan Application</p>
              <p className="text-sm text-gray-500">Apply for a new loan</p>
            </div>
          </Link>

          <Link
            to="/calculator"
            className="flex items-center p-4 border-2 border-dashed border-gray-300 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <TrendingUp className="w-8 h-8 text-primary-600 mr-3" />
            <div>
              <p className="font-medium text-gray-900">Calculate EMI</p>
              <p className="text-sm text-gray-500">Estimate loan payments</p>
            </div>
          </Link>

          <Link
            to="/eligibility"
            className="flex items-center p-4 border-2 border-dashed border-gray-300 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <CheckCircle className="w-8 h-8 text-primary-600 mr-3" />
            <div>
              <p className="font-medium text-gray-900">Check Eligibility</p>
              <p className="text-sm text-gray-500">See if you qualify</p>
            </div>
          </Link>
        </div>
      </div>

      {/* Recent Applications */}
      <div className="card">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold text-gray-900">Recent Applications</h2>
          <Link
            to="/applications"
            className="text-sm text-primary-600 hover:text-primary-700 font-medium flex items-center"
          >
            View all
            <ArrowUpRight className="w-4 h-4 ml-1" />
          </Link>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-gray-200">
                <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">
                  Member
                </th>
                <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">
                  Amount
                </th>
                <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">
                  Status
                </th>
                <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">
                  Date
                </th>
                <th className="text-right py-3 px-4 text-sm font-semibold text-gray-700">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              {recentApplications.map((app) => (
                <tr key={app.id} className="border-b border-gray-100 hover:bg-gray-50">
                  <td className="py-3 px-4">
                    <div>
                      <p className="font-medium text-gray-900">{app.memberName}</p>
                      <p className="text-sm text-gray-500">{app.memberNumber}</p>
                    </div>
                  </td>
                  <td className="py-3 px-4">
                    <p className="font-medium text-gray-900">
                      ₦{app.amount.toLocaleString()}
                    </p>
                  </td>
                  <td className="py-3 px-4">
                    <span
                      className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getStatusColor(
                        app.status
                      )}`}
                    >
                      {app.status.replace('_', ' ')}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-sm text-gray-600">{app.date}</td>
                  <td className="py-3 px-4 text-right">
                    <button className="text-primary-600 hover:text-primary-700 text-sm font-medium">
                      View Details
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Activity Timeline */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="card">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">
            Recent Activity
          </h2>
          <div className="space-y-4">
            {[
              {
                action: 'Loan Application Submitted',
                user: 'John Doe',
                time: '2 hours ago',
                icon: FileText,
                color: 'bg-blue-100 text-blue-600',
              },
              {
                action: 'Deduction Schedule Generated',
                user: 'System',
                time: '5 hours ago',
                icon: CheckCircle,
                color: 'bg-green-100 text-green-600',
              },
              {
                action: 'Committee Review Completed',
                user: 'Jane Smith',
                time: '1 day ago',
                icon: Clock,
                color: 'bg-purple-100 text-purple-600',
              },
            ].map((activity, index) => {
              const Icon = activity.icon;
              return (
                <div key={index} className="flex items-start">
                  <div className={`${activity.color} p-2 rounded-lg mr-3`}>
                    <Icon className="w-4 h-4" />
                  </div>
                  <div className="flex-1">
                    <p className="text-sm font-medium text-gray-900">
                      {activity.action}
                    </p>
                    <p className="text-xs text-gray-500">
                      {activity.user} • {activity.time}
                    </p>
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        <div className="card">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">
            Upcoming Tasks
          </h2>
          <div className="space-y-3">
            {[
              { task: 'Review pending applications', count: 5, priority: 'high' },
              { task: 'Approve deduction schedule', count: 1, priority: 'medium' },
              { task: 'Process guarantor consents', count: 3, priority: 'low' },
            ].map((item, index) => (
              <div
                key={index}
                className="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
              >
                <div className="flex items-center">
                  <div
                    className={`w-2 h-2 rounded-full mr-3 ${
                      item.priority === 'high'
                        ? 'bg-red-500'
                        : item.priority === 'medium'
                        ? 'bg-yellow-500'
                        : 'bg-green-500'
                    }`}
                  />
                  <p className="text-sm font-medium text-gray-900">{item.task}</p>
                </div>
                <span className="text-xs font-semibold text-gray-600 bg-white px-2 py-1 rounded">
                  {item.count}
                </span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
