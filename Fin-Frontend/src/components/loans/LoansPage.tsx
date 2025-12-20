import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
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
  ChevronLeft,
  ChevronRight,
  User
} from 'lucide-react';
import { format } from 'date-fns';

interface LoanAccount {
  id: string;
  accountNumber: string;
  customerName: string;
  customerImage?: string;
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
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState('all');
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 50;

  // Mock data - replace with actual API call
  // Generating more mock data to demonstrate pagination
  const loans: LoanAccount[] = Array.from({ length: 65 }).map((_, i) => ({
    id: `${i + 1}`,
    accountNumber: `LN00123${4567 + i}`,
    customerName: i % 2 === 0 ? 'Adebayo Ogundimu' : 'Fatima Aliyu',
    productName: i % 3 === 0 ? 'SME Business Loan' : 'Personal Loan',
    principalAmount: 5000000 + (i * 100000),
    outstandingPrincipal: 3500000,
    outstandingInterest: 125000,
    interestRate: 18.5,
    disbursementDate: '2024-01-15',
    maturityDate: '2025-01-15',
    status: i % 5 === 0 ? 'Delinquent' : 'Active',
    classification: i % 5 === 0 ? 'Substandard' : 'Performing',
    daysPastDue: i % 5 === 0 ? 45 : 0,
  }));

  const filteredLoans = loans.filter((loan) => {
    const matchesSearch = loan.customerName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      loan.accountNumber.includes(searchTerm) ||
      loan.productName.toLowerCase().includes(searchTerm.toLowerCase());

    // Simple mock filter for status
    const matchesStatus = filterStatus === 'all' || loan.status.toLowerCase() === filterStatus;

    return matchesSearch && matchesStatus;
  });

  // Pagination Logic
  const totalPages = Math.ceil(filteredLoans.length / itemsPerPage);
  const paginatedLoans = filteredLoans.slice(
    (currentPage - 1) * itemsPerPage,
    currentPage * itemsPerPage
  );

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
      default:
        return <AlertTriangle className="w-4 h-4" />;
    }
  };

  const handleRowClick = (id: string) => {
    navigate(`/loans/${id}`);
  };

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
          <button
            onClick={() => navigate('/loans/new')}
            className="flex items-center px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 transition-colors shadow-lg hover:shadow-xl"
          >
            <Plus className="w-4 h-4 mr-2" />
            New Loan
          </button>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <div className="bg-white p-6 rounded-xl border border-gray-200 shadow-sm">
          <div className="flex justify-between items-start">
            <div>
              <p className="text-sm font-medium text-gray-500">Total Portfolio</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">₦{loans.reduce((acc, curr) => acc + curr.outstandingPrincipal, 0).toLocaleString()}</p>
            </div>
            <div className="p-2 bg-emerald-50 rounded-lg text-emerald-600">
              <CreditCard className="w-6 h-6" />
            </div>
          </div>
        </div>

        <div className="bg-white p-6 rounded-xl border border-gray-200 shadow-sm">
          <div className="flex justify-between items-start">
            <div>
              <p className="text-sm font-medium text-gray-500">Active Loans</p>
              <p className="text-2xl font-bold text-gray-900 mt-1">{loans.filter(l => l.status === 'Active').length}</p>
            </div>
            <div className="p-2 bg-blue-50 rounded-lg text-blue-600">
              <CheckCircle className="w-6 h-6" />
            </div>
          </div>
        </div>

        <div className="bg-white p-6 rounded-xl border border-gray-200 shadow-sm">
          <div className="flex justify-between items-start">
            <div>
              <p className="text-sm font-medium text-gray-500">PAR (30+ Days)</p>
              <p className="text-2xl font-bold text-orange-600 mt-1">
                ₦{loans.filter(l => l.daysPastDue > 30).reduce((acc, curr) => acc + curr.outstandingPrincipal, 0).toLocaleString()}
              </p>
            </div>
            <div className="p-2 bg-orange-50 rounded-lg text-orange-600">
              <AlertTriangle className="w-6 h-6" />
            </div>
          </div>
        </div>

        <div className="bg-white p-6 rounded-xl border border-gray-200 shadow-sm">
          <div className="flex justify-between items-start">
            <div>
              <p className="text-sm font-medium text-gray-500">NPL Ratio</p>
              <p className="text-2xl font-bold text-red-600 mt-1">
                {((loans.filter(l => l.daysPastDue > 90).length / loans.length) * 100).toFixed(1)}%
              </p>
            </div>
            <div className="p-2 bg-red-50 rounded-lg text-red-600">
              <XCircle className="w-6 h-6" />
            </div>
          </div>
        </div>
      </div>

      {/* Filters and Search */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
        <div className="flex flex-col lg:flex-row lg:items-center space-y-4 lg:space-y-0 lg:space-x-4">
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
              <option value="delinquent">Delinquent</option>
            </select>
          </div>
        </div>
      </div>

      {/* Loans Table */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-200 flex justify-between items-center">
          <h3 className="text-lg font-semibold text-gray-900">
            Loan Portfolio ({filteredLoans.length})
          </h3>
          <span className="text-sm text-gray-500">Showing {Math.min(itemsPerPage, paginatedLoans.length)} of {filteredLoans.length}</span>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Applicant</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Loan Details</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Outstanding</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Maturity</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {paginatedLoans.map((loan, index) => (
                <motion.tr
                  key={loan.id}
                  initial={{ opacity: 0 }}
                  animate={{ opacity: 1 }}
                  transition={{ delay: index * 0.02 }}
                  onClick={() => handleRowClick(loan.id)}
                  className="hover:bg-gray-50 transition-colors cursor-pointer group"
                >
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center">
                      <div className="w-10 h-10 rounded-full bg-gray-200 flex items-center justify-center text-gray-500 font-bold overflow-hidden mr-3 border border-gray-300">
                        {loan.customerImage ? (
                          <img src={loan.customerImage} alt={loan.customerName} className="w-full h-full object-cover" />
                        ) : (
                          <User className="w-5 h-5" />
                        )}
                      </div>
                      <div>
                        <div className="text-sm font-medium text-gray-900 group-hover:text-emerald-700 transition-colors">{loan.customerName}</div>
                        <div className="text-xs text-gray-500">{loan.accountNumber}</div>
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-gray-900">{loan.productName}</div>
                    <div className="text-xs text-gray-500">Rate: {loan.interestRate}%</div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-bold text-gray-900">
                      ₦{loan.outstandingPrincipal.toLocaleString()}
                    </div>
                    <div className="text-xs text-gray-500">
                      Principal: ₦{loan.principalAmount.toLocaleString()}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getClassificationColor(loan.classification)}`}>
                      {getClassificationIcon(loan.classification)}
                      <span className="ml-1">{loan.classification}</span>
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {format(new Date(loan.maturityDate), 'MMM dd, yyyy')}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <button
                      onClick={(e) => { e.stopPropagation(); navigate(`/loans/${loan.id}`); }}
                      className="text-emerald-600 hover:text-emerald-900 mr-3 p-1 hover:bg-emerald-50 rounded"
                    >
                      View Details
                    </button>
                  </td>
                </motion.tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Pagination Controls */}
        <div className="bg-gray-50 px-6 py-4 border-t border-gray-200 flex items-center justify-between">
          <div className="text-sm text-gray-500">
            Page {currentPage} of {totalPages}
          </div>
          <div className="flex space-x-2">
            <button
              onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
              disabled={currentPage === 1}
              className="p-2 border border-gray-300 rounded-lg bg-white disabled:opacity-50 hover:bg-gray-50"
            >
              <ChevronLeft className="w-4 h-4" />
            </button>
            <button
              onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
              disabled={currentPage === totalPages}
              className="p-2 border border-gray-300 rounded-lg bg-white disabled:opacity-50 hover:bg-gray-50"
            >
              <ChevronRight className="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default LoansPage;
