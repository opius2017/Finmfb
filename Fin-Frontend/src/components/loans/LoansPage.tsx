import React, { useState } from 'react';
import { motion } from 'framer-motion';
import {
  CreditCard,
  Plus,
  Search,
  Filter,
  Download,
  Eye,
  MoreHorizontal,
  AlertTriangle,
  CheckCircle,
  Clock,
  XCircle,
} from 'lucide-react';
import { format } from 'date-fns';

interface LoanAccount {
  id: string;
  accountNumber: string;
  customerName: string;
  productName: string;
  principalAmount: number;
  outstandingPrincipal: number;
  outstandingInterest: number;
  interestRate: number;
  disbursementDate: string;
  maturityDate: string;
  status: string;
  classification: string;
  daysPastDue: number;
}

const LoansPage: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState('all');
  const [filterClassification, setFilterClassification] = useState('all');

  // Mock data - replace with actual API call
  const loans: LoanAccount[] = [
    {
      id: '1',
      accountNumber: 'LN001234567',
      customerName: 'Adebayo Ogundimu',
      productName: 'SME Business Loan',
      principalAmount: 5000000,
      outstandingPrincipal: 3500000,
      outstandingInterest: 125000,
      interestRate: 18.5,
      disbursementDate: '2024-01-15',
      maturityDate: '2025-01-15',
      status: 'Active',
      classification: 'Performing',
      daysPastDue: 0,
    },
    {
      id: '2',
      accountNumber: 'LN001234568',
      customerName: 'Fatima Aliyu',
      productName: 'Individual Loan',
      principalAmount: 2000000,
      outstandingPrincipal: 800000,
      outstandingInterest: 45000,
      interestRate: 22.0,
      disbursementDate: '2023-08-10',
      maturityDate: '2024-08-10',
      status: 'Active',
      classification: 'Special Mention',
      daysPastDue: 15,
    },
    {
      id: '3',
      accountNumber: 'LN001234569',
      customerName: 'Chinedu Okwu',
      productName: 'Asset Financing',
      principalAmount: 8000000,
      outstandingPrincipal: 6200000,
      outstandingInterest: 180000,
      interestRate: 16.5,
      disbursementDate: '2024-03-01',
      maturityDate: '2026-03-01',
      status: 'Active',
      classification: 'Performing',
      daysPastDue: 0,
    },
    {
      id: '4',
      accountNumber: 'LN001234570',
      customerName: 'Kemi Adebisi',
      productName: 'Working Capital',
      principalAmount: 3000000,
      outstandingPrincipal: 1500000,
      outstandingInterest: 85000,
      interestRate: 20.0,
      disbursementDate: '2023-11-20',
      maturityDate: '2024-11-20',
      status: 'Active',
      classification: 'Substandard',
      daysPastDue: 45,
    },
  ];

  const filteredLoans = loans.filter((loan) => {
    const matchesSearch = loan.customerName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         loan.accountNumber.includes(searchTerm) ||
                         loan.productName.toLowerCase().includes(searchTerm.toLowerCase());
    
    const matchesStatus = filterStatus === 'all' || loan.status.toLowerCase() === filterStatus;
    const matchesClassification = filterClassification === 'all' || loan.classification.toLowerCase().replace(' ', '') === filterClassification;
    
    return matchesSearch && matchesStatus && matchesClassification;
  });

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'active':
        return 'bg-green-100 text-green-800';
      case 'closed':
        return 'bg-gray-100 text-gray-800';
      case 'written off':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getClassificationColor = (classification: string) => {
    switch (classification.toLowerCase()) {
      case 'performing':
        return 'bg-green-100 text-green-800';
      case 'special mention':
        return 'bg-yellow-100 text-yellow-800';
      case 'substandard':
        return 'bg-orange-100 text-orange-800';
      case 'doubtful':
        return 'bg-red-100 text-red-800';
      case 'lost':
        return 'bg-red-200 text-red-900';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getClassificationIcon = (classification: string) => {
    switch (classification.toLowerCase()) {
      case 'performing':
        return <CheckCircle className="w-4 h-4" />;
      case 'special mention':
        return <Clock className="w-4 h-4" />;
      case 'substandard':
      case 'doubtful':
        return <AlertTriangle className="w-4 h-4" />;
      case 'lost':
        return <XCircle className="w-4 h-4" />;
      default:
        return <CheckCircle className="w-4 h-4" />;
    }
  };

  const totalPortfolio = loans.reduce((sum, loan) => sum + loan.outstandingPrincipal, 0);
  const performingLoans = loans.filter(loan => loan.classification === 'Performing').length;
  const nonPerformingLoans = loans.filter(loan => loan.classification !== 'Performing').length;
  const portfolioAtRisk = loans
    .filter(loan => loan.classification !== 'Performing')
    .reduce((sum, loan) => sum + loan.outstandingPrincipal, 0);

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 flex items-center">
            <CreditCard className="w-7 h-7 text-emerald-600 mr-3" />
            Loan Management
          </h1>
          <p className="text-gray-600">Manage loan portfolio and credit operations</p>
        </div>
        <div className="flex items-center space-x-3">
          <button className="flex items-center px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
            <Download className="w-4 h-4 mr-2" />
            Export
          </button>
          <button className="flex items-center px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 transition-colors">
            <Plus className="w-4 h-4 mr-2" />
            New Loan
          </button>
        </div>
      </div>

      {/* Portfolio Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 mb-1">Total Portfolio</p>
              <p className="text-2xl font-bold text-gray-900">₦{totalPortfolio.toLocaleString()}</p>
            </div>
            <div className="p-3 rounded-lg bg-emerald-100 text-emerald-600">
              <CreditCard className="w-6 h-6" />
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
              <p className="text-sm font-medium text-gray-600 mb-1">Performing Loans</p>
              <p className="text-2xl font-bold text-gray-900">{performingLoans}</p>
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
              <p className="text-sm font-medium text-gray-600 mb-1">Non-Performing</p>
              <p className="text-2xl font-bold text-gray-900">{nonPerformingLoans}</p>
            </div>
            <div className="p-3 rounded-lg bg-red-100 text-red-600">
              <AlertTriangle className="w-6 h-6" />
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
              <p className="text-sm font-medium text-gray-600 mb-1">Portfolio at Risk</p>
              <p className="text-2xl font-bold text-gray-900">₦{portfolioAtRisk.toLocaleString()}</p>
              <p className="text-sm text-red-600 mt-1">
                {totalPortfolio > 0 ? ((portfolioAtRisk / totalPortfolio) * 100).toFixed(1) : 0}% PAR
              </p>
            </div>
            <div className="p-3 rounded-lg bg-orange-100 text-orange-600">
              <AlertTriangle className="w-6 h-6" />
            </div>
          </div>
        </motion.div>
      </div>

      {/* Filters and Search */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
        <div className="flex flex-col lg:flex-row lg:items-center space-y-4 lg:space-y-0 lg:space-x-4">
          {/* Search */}
          <div className="flex-1 relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
            <input
              type="text"
              placeholder="Search by customer name, account number, or product..."
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
              <option value="active">Active</option>
              <option value="closed">Closed</option>
              <option value="written off">Written Off</option>
            </select>
          </div>

          {/* Classification Filter */}
          <div className="flex items-center space-x-2">
            <select
              value={filterClassification}
              onChange={(e) => setFilterClassification(e.target.value)}
              className="border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
            >
              <option value="all">All Classifications</option>
              <option value="performing">Performing</option>
              <option value="specialmention">Special Mention</option>
              <option value="substandard">Substandard</option>
              <option value="doubtful">Doubtful</option>
              <option value="lost">Lost</option>
            </select>
          </div>
        </div>
      </div>

      {/* Loans Table */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-200">
          <h3 className="text-lg font-semibold text-gray-900">
            Loan Portfolio ({filteredLoans.length})
          </h3>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Loan Details
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Customer
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Outstanding
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Interest Rate
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Classification
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Maturity
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {filteredLoans.map((loan, index) => (
                <motion.tr
                  key={loan.id}
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: index * 0.05 }}
                  className="hover:bg-gray-50 transition-colors"
                >
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div>
                      <div className="text-sm font-medium text-gray-900">
                        {loan.accountNumber}
                      </div>
                      <div className="text-sm text-gray-500">
                        {loan.productName}
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-medium text-gray-900">
                      {loan.customerName}
                    </div>
                    <div className="text-sm text-gray-500">
                      Principal: ₦{loan.principalAmount.toLocaleString()}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-medium text-gray-900">
                      ₦{loan.outstandingPrincipal.toLocaleString()}
                    </div>
                    <div className="text-sm text-gray-500">
                      Interest: ₦{loan.outstandingInterest.toLocaleString()}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {loan.interestRate}% p.a.
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`inline-flex items-center px-2 py-1 text-xs font-semibold rounded-full ${
                      getClassificationColor(loan.classification)
                    }`}>
                      {getClassificationIcon(loan.classification)}
                      <span className="ml-1">{loan.classification}</span>
                    </span>
                    {loan.daysPastDue > 0 && (
                      <div className="text-xs text-red-600 mt-1">
                        {loan.daysPastDue} days overdue
                      </div>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {format(new Date(loan.maturityDate), 'MMM dd, yyyy')}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <div className="flex items-center space-x-2">
                      <button className="text-emerald-600 hover:text-emerald-900 transition-colors">
                        <Eye className="w-4 h-4" />
                      </button>
                      <button className="text-gray-400 hover:text-gray-600 transition-colors">
                        <MoreHorizontal className="w-4 h-4" />
                      </button>
                    </div>
                  </td>
                </motion.tr>
              ))}
            </tbody>
          </table>
        </div>

        {filteredLoans.length === 0 && (
          <div className="px-6 py-12 text-center">
            <CreditCard className="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">No loans found</h3>
            <p className="text-gray-500">
              {searchTerm || filterStatus !== 'all' || filterClassification !== 'all'
                ? 'Try adjusting your search or filter criteria.'
                : 'Get started by creating your first loan product.'
              }
            </p>
          </div>
        )}
      </div>
    </div>
  );
};

export default LoansPage;