import React, { useState } from 'react';
import { motion } from 'framer-motion';
import {
  Shield,
  Clock,
  CheckCircle,
  XCircle,
  AlertTriangle,
  Eye,
  User,
  Calendar,
  DollarSign,
  FileText,
  Filter,
  Search,
} from 'lucide-react';
import { format } from 'date-fns';

interface MakerCheckerTransaction {
  id: string;
  transactionReference: string;
  entityName: string;
  operation: string;
  amount?: number;
  priority: number;
  makerName: string;
  makerTimestamp: string;
  checkerName?: string;
  checkerTimestamp?: string;
  status: string;
  expiryDate?: string;
  description: string;
}

const MakerCheckerPage: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState('all');
  const [filterPriority, setFilterPriority] = useState('all');
  const [activeTab, setActiveTab] = useState<'pending' | 'my-transactions' | 'all'>('pending');

  // Mock data - replace with actual API call
  const transactions: MakerCheckerTransaction[] = [
    {
      id: '1',
      transactionReference: 'MC20241220001',
      entityName: 'LoanAccount',
      operation: 'Disburse',
      amount: 5000000,
      priority: 3,
      makerName: 'Adebayo Ogundimu',
      makerTimestamp: '2024-12-20T10:30:00Z',
      status: 'PendingApproval',
      expiryDate: '2024-12-21T10:30:00Z',
      description: 'Loan disbursement of ₦5,000,000',
    },
    {
      id: '2',
      transactionReference: 'MC20241220002',
      entityName: 'JournalEntry',
      operation: 'Post',
      amount: 2500000,
      priority: 2,
      makerName: 'Fatima Aliyu',
      makerTimestamp: '2024-12-20T11:15:00Z',
      status: 'PendingApproval',
      description: 'Journal entry posting of ₦2,500,000',
    },
    {
      id: '3',
      transactionReference: 'MC20241220003',
      entityName: 'VendorPayment',
      operation: 'Payment',
      amount: 750000,
      priority: 1,
      makerName: 'Chinedu Okwu',
      makerTimestamp: '2024-12-20T09:45:00Z',
      checkerName: 'Kemi Adebisi',
      checkerTimestamp: '2024-12-20T14:20:00Z',
      status: 'Approved',
      description: 'Vendor payment of ₦750,000',
    },
    {
      id: '4',
      transactionReference: 'MC20241220004',
      entityName: 'Employee',
      operation: 'Create',
      priority: 1,
      makerName: 'Kemi Adebisi',
      makerTimestamp: '2024-12-20T08:30:00Z',
      checkerName: 'Adebayo Ogundimu',
      checkerTimestamp: '2024-12-20T13:45:00Z',
      status: 'Rejected',
      description: 'New employee registration',
    },
  ];

  const filteredTransactions = transactions.filter((transaction) => {
    const matchesSearch = transaction.transactionReference.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         transaction.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         transaction.makerName.toLowerCase().includes(searchTerm.toLowerCase());
    
    const matchesStatus = filterStatus === 'all' || transaction.status.toLowerCase().replace('pending', '').replace('approval', '') === filterStatus;
    const matchesPriority = filterPriority === 'all' || transaction.priority.toString() === filterPriority;
    
    // Filter by tab
    if (activeTab === 'pending') {
      return matchesSearch && matchesStatus && matchesPriority && transaction.status === 'PendingApproval';
    } else if (activeTab === 'my-transactions') {
      // In a real app, this would filter by current user's transactions
      return matchesSearch && matchesStatus && matchesPriority;
    }
    
    return matchesSearch && matchesStatus && matchesPriority;
  });

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'pendingapproval':
        return 'bg-yellow-100 text-yellow-800';
      case 'approved':
        return 'bg-green-100 text-green-800';
      case 'rejected':
        return 'bg-red-100 text-red-800';
      case 'expired':
        return 'bg-gray-100 text-gray-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status.toLowerCase()) {
      case 'pendingapproval':
        return <Clock className="w-4 h-4" />;
      case 'approved':
        return <CheckCircle className="w-4 h-4" />;
      case 'rejected':
        return <XCircle className="w-4 h-4" />;
      case 'expired':
        return <AlertTriangle className="w-4 h-4" />;
      default:
        return <Clock className="w-4 h-4" />;
    }
  };

  const getPriorityColor = (priority: number) => {
    switch (priority) {
      case 1:
        return 'bg-blue-100 text-blue-800';
      case 2:
        return 'bg-yellow-100 text-yellow-800';
      case 3:
        return 'bg-orange-100 text-orange-800';
      case 4:
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getPriorityText = (priority: number) => {
    switch (priority) {
      case 1:
        return 'Low';
      case 2:
        return 'Medium';
      case 3:
        return 'High';
      case 4:
        return 'Critical';
      default:
        return 'Unknown';
    }
  };

  const pendingCount = transactions.filter(t => t.status === 'PendingApproval').length;
  const approvedToday = transactions.filter(t => t.status === 'Approved' && 
    new Date(t.checkerTimestamp || '').toDateString() === new Date().toDateString()).length;
  const rejectedToday = transactions.filter(t => t.status === 'Rejected' && 
    new Date(t.checkerTimestamp || '').toDateString() === new Date().toDateString()).length;
  const highPriorityPending = transactions.filter(t => t.status === 'PendingApproval' && t.priority >= 3).length;

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 flex items-center">
            <Shield className="w-7 h-7 text-emerald-600 mr-3" />
            Maker-Checker Workflow
          </h1>
          <p className="text-gray-600">Dual authorization system for critical transactions</p>
        </div>
        <div className="flex items-center space-x-3">
          <div className="bg-white rounded-lg border border-gray-200 px-4 py-2">
            <div className="flex items-center space-x-2">
              <div className="w-2 h-2 bg-yellow-500 rounded-full"></div>
              <span className="text-sm font-medium text-gray-700">{pendingCount} Pending Approvals</span>
            </div>
          </div>
        </div>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 mb-1">Pending Approvals</p>
              <p className="text-2xl font-bold text-gray-900">{pendingCount}</p>
            </div>
            <div className="p-3 rounded-lg bg-yellow-100 text-yellow-600">
              <Clock className="w-6 h-6" />
            </div>
          </div>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 mb-1">Approved Today</p>
              <p className="text-2xl font-bold text-gray-900">{approvedToday}</p>
            </div>
            <div className="p-3 rounded-lg bg-green-100 text-green-600">
              <CheckCircle className="w-6 h-6" />
            </div>
          </div>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.2 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 mb-1">Rejected Today</p>
              <p className="text-2xl font-bold text-gray-900">{rejectedToday}</p>
            </div>
            <div className="p-3 rounded-lg bg-red-100 text-red-600">
              <XCircle className="w-6 h-6" />
            </div>
          </div>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 mb-1">High Priority</p>
              <p className="text-2xl font-bold text-gray-900">{highPriorityPending}</p>
            </div>
            <div className="p-3 rounded-lg bg-orange-100 text-orange-600">
              <AlertTriangle className="w-6 h-6" />
            </div>
          </div>
        </motion.div>
      </div>

      {/* Main Content */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-200">
        {/* Tab Navigation */}
        <div className="border-b border-gray-200">
          <nav className="flex space-x-8 px-6">
            <button
              onClick={() => setActiveTab('pending')}
              className={`py-4 px-1 border-b-2 font-medium text-sm transition-colors ${
                activeTab === 'pending'
                  ? 'border-emerald-500 text-emerald-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              Pending Approvals ({pendingCount})
            </button>
            <button
              onClick={() => setActiveTab('my-transactions')}
              className={`py-4 px-1 border-b-2 font-medium text-sm transition-colors ${
                activeTab === 'my-transactions'
                  ? 'border-emerald-500 text-emerald-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              My Transactions
            </button>
            <button
              onClick={() => setActiveTab('all')}
              className={`py-4 px-1 border-b-2 font-medium text-sm transition-colors ${
                activeTab === 'all'
                  ? 'border-emerald-500 text-emerald-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              All Transactions
            </button>
          </nav>
        </div>

        {/* Filters */}
        <div className="p-6 border-b border-gray-200">
          <div className="flex flex-col lg:flex-row lg:items-center space-y-4 lg:space-y-0 lg:space-x-4">
            {/* Search */}
            <div className="flex-1 relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
              <input
                type="text"
                placeholder="Search by reference, description, or maker..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
              />
            </div>

            {/* Status Filter */}
            <div className="flex items-center space-x-2">
              <Filter className="w-5 h-5 text-gray-400" />
              <select
                value={filterStatus}
                onChange={(e) => setFilterStatus(e.target.value)}
                className="border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
              >
                <option value="all">All Status</option>
                <option value="approved">Approved</option>
                <option value="rejected">Rejected</option>
                <option value="expired">Expired</option>
              </select>
            </div>

            {/* Priority Filter */}
            <div className="flex items-center space-x-2">
              <select
                value={filterPriority}
                onChange={(e) => setFilterPriority(e.target.value)}
                className="border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
              >
                <option value="all">All Priorities</option>
                <option value="1">Low Priority</option>
                <option value="2">Medium Priority</option>
                <option value="3">High Priority</option>
                <option value="4">Critical Priority</option>
              </select>
            </div>
          </div>
        </div>

        {/* Transactions Table */}
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Transaction Details
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Amount
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Priority
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Maker
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Checker
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {filteredTransactions.map((transaction, index) => (
                <motion.tr
                  key={transaction.id}
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: index * 0.05 }}
                  className="hover:bg-gray-50 transition-colors"
                >
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div>
                      <div className="text-sm font-medium text-gray-900">
                        {transaction.transactionReference}
                      </div>
                      <div className="text-sm text-gray-500">
                        {transaction.description}
                      </div>
                      <div className="text-xs text-gray-400 mt-1">
                        {transaction.entityName} • {transaction.operation}
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    {transaction.amount ? (
                      <div className="flex items-center">
                        <DollarSign className="w-4 h-4 text-gray-400 mr-1" />
                        <span className="text-sm font-medium text-gray-900">
                          ₦{transaction.amount.toLocaleString()}
                        </span>
                      </div>
                    ) : (
                      <span className="text-sm text-gray-500">N/A</span>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`inline-flex items-center px-2 py-1 text-xs font-semibold rounded-full ${
                      getPriorityColor(transaction.priority)
                    }`}>
                      {getPriorityText(transaction.priority)}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center">
                      <User className="w-4 h-4 text-gray-400 mr-2" />
                      <div>
                        <div className="text-sm font-medium text-gray-900">
                          {transaction.makerName}
                        </div>
                        <div className="text-xs text-gray-500">
                          {format(new Date(transaction.makerTimestamp), 'MMM dd, HH:mm')}
                        </div>
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    {transaction.checkerName ? (
                      <div className="flex items-center">
                        <User className="w-4 h-4 text-gray-400 mr-2" />
                        <div>
                          <div className="text-sm font-medium text-gray-900">
                            {transaction.checkerName}
                          </div>
                          <div className="text-xs text-gray-500">
                            {transaction.checkerTimestamp && 
                              format(new Date(transaction.checkerTimestamp), 'MMM dd, HH:mm')
                            }
                          </div>
                        </div>
                      </div>
                    ) : (
                      <span className="text-sm text-gray-500">Pending</span>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`inline-flex items-center px-2 py-1 text-xs font-semibold rounded-full ${
                      getStatusColor(transaction.status)
                    }`}>
                      {getStatusIcon(transaction.status)}
                      <span className="ml-1">{transaction.status.replace('Pending', 'Pending ')}</span>
                    </span>
                    {transaction.expiryDate && transaction.status === 'PendingApproval' && (
                      <div className="text-xs text-orange-600 mt-1 flex items-center">
                        <Calendar className="w-3 h-3 mr-1" />
                        Expires {format(new Date(transaction.expiryDate), 'MMM dd, HH:mm')}
                      </div>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <div className="flex items-center space-x-2">
                      <button className="text-emerald-600 hover:text-emerald-900 transition-colors">
                        <Eye className="w-4 h-4" />
                      </button>
                      {transaction.status === 'PendingApproval' && (
                        <>
                          <button className="text-green-600 hover:text-green-900 transition-colors">
                            <CheckCircle className="w-4 h-4" />
                          </button>
                          <button className="text-red-600 hover:text-red-900 transition-colors">
                            <XCircle className="w-4 h-4" />
                          </button>
                        </>
                      )}
                    </div>
                  </td>
                </motion.tr>
              ))}
            </tbody>
          </table>
        </div>

        {filteredTransactions.length === 0 && (
          <div className="px-6 py-12 text-center">
            <Shield className="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">No transactions found</h3>
            <p className="text-gray-500">
              {searchTerm || filterStatus !== 'all' || filterPriority !== 'all'
                ? 'Try adjusting your search or filter criteria.'
                : 'No maker-checker transactions available.'
              }
            </p>
          </div>
        )}
      </div>

      {/* Quick Actions */}
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.5 }}
        className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
      >
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Workflow Information</h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="text-center">
            <div className="w-12 h-12 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-3">
              <User className="w-6 h-6 text-blue-600" />
            </div>
            <h4 className="text-sm font-semibold text-gray-900 mb-2">Maker</h4>
            <p className="text-xs text-gray-600">
              Initiates transactions and submits for approval. Cannot approve own transactions.
            </p>
          </div>
          <div className="text-center">
            <div className="w-12 h-12 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-3">
              <CheckCircle className="w-6 h-6 text-green-600" />
            </div>
            <h4 className="text-sm font-semibold text-gray-900 mb-2">Checker</h4>
            <p className="text-xs text-gray-600">
              Reviews and approves/rejects transactions. Must be different from maker.
            </p>
          </div>
          <div className="text-center">
            <div className="w-12 h-12 bg-purple-100 rounded-full flex items-center justify-center mx-auto mb-3">
              <Shield className="w-6 h-6 text-purple-600" />
            </div>
            <h4 className="text-sm font-semibold text-gray-900 mb-2">Audit Trail</h4>
            <p className="text-xs text-gray-600">
              Complete audit log with timestamps and user identification for compliance.
            </p>
          </div>
        </div>
      </motion.div>
    </div>
  );
};

export default MakerCheckerPage;