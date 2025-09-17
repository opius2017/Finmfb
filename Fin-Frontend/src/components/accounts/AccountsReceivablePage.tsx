import React, { useState } from 'react';
import { motion } from 'framer-motion';
import {
  FileText,
  Plus,
  Search,
  Filter,
  Download,
  Eye,
  MoreHorizontal,
  DollarSign,
  // ...existing code...
  AlertTriangle,
  CheckCircle,
  Clock,
  Send,
  CreditCard,
  // ...existing code...
  TrendingUp,
} from 'lucide-react';
import { format } from 'date-fns';
import JournalEntryDocumentUpload from './JournalEntryDocumentUpload';

interface Invoice {
  id: string;
  invoiceNumber: string;
  customerName: string;
  invoiceDate: string;
  dueDate: string;
  totalAmount: number;
  paidAmount: number;
  outstandingAmount: number;
  status: string;
  description: string;
  daysToDue: number;
}

const AccountsReceivablePage: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState('all');
  const [selectedView, setSelectedView] = useState<'list' | 'aging'>('list');

  // Mock data - replace with actual API call
  const invoices: Invoice[] = [
    {
      id: '1',
      invoiceNumber: 'INV-2024-001',
      customerName: 'Adebayo Enterprises',
      invoiceDate: '2024-12-01',
      dueDate: '2024-12-31',
      totalAmount: 850000,
      paidAmount: 0,
      outstandingAmount: 850000,
      status: 'Sent',
      description: 'Consulting services for December',
      daysToDue: 11,
    },
    {
      id: '2',
      invoiceNumber: 'INV-2024-002',
      customerName: 'Fatima Trading Co',
      invoiceDate: '2024-11-15',
      dueDate: '2024-12-15',
      totalAmount: 1200000,
      paidAmount: 1200000,
      outstandingAmount: 0,
      status: 'Paid',
      description: 'Product sales and delivery',
      daysToDue: 0,
    },
    {
      id: '3',
      invoiceNumber: 'INV-2024-003',
      customerName: 'Chinedu Motors',
      invoiceDate: '2024-11-20',
      dueDate: '2024-12-20',
      totalAmount: 650000,
      paidAmount: 325000,
      outstandingAmount: 325000,
      status: 'Partially Paid',
      description: 'Auto parts and maintenance',
      daysToDue: 0,
    },
    {
      id: '4',
      invoiceNumber: 'INV-2024-004',
      customerName: 'Kemi Fashion House',
      invoiceDate: '2024-11-10',
      dueDate: '2024-12-10',
      totalAmount: 420000,
      paidAmount: 0,
      outstandingAmount: 420000,
      status: 'Overdue',
      description: 'Fashion accessories wholesale',
      daysToDue: -10,
    },
    {
      id: '5',
      invoiceNumber: 'INV-2024-005',
      customerName: 'Lagos Tech Hub',
      invoiceDate: '2024-12-10',
      dueDate: '2025-01-09',
      totalAmount: 2500000,
      paidAmount: 0,
      outstandingAmount: 2500000,
      status: 'Draft',
      description: 'Software development services',
      daysToDue: 20,
    },
  ];

  const filteredInvoices = invoices.filter((invoice) => {
    const matchesSearch = invoice.invoiceNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         invoice.customerName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         invoice.description.toLowerCase().includes(searchTerm.toLowerCase());
    
    const matchesStatus = filterStatus === 'all' || invoice.status.toLowerCase().replace(' ', '') === filterStatus;
    
    return matchesSearch && matchesStatus;
  });

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'draft':
        return 'bg-gray-100 text-gray-800';
      case 'sent':
        return 'bg-blue-100 text-blue-800';
      case 'viewed':
        return 'bg-purple-100 text-purple-800';
      case 'paid':
        return 'bg-green-100 text-green-800';
      case 'partially paid':
        return 'bg-yellow-100 text-yellow-800';
      case 'overdue':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status.toLowerCase()) {
      case 'draft':
        return <FileText className="w-4 h-4" />;
      case 'sent':
        return <Send className="w-4 h-4" />;
      case 'viewed':
        return <Eye className="w-4 h-4" />;
      case 'paid':
        return <CheckCircle className="w-4 h-4" />;
      case 'partially paid':
        return <DollarSign className="w-4 h-4" />;
      case 'overdue':
        return <AlertTriangle className="w-4 h-4" />;
      default:
        return <Clock className="w-4 h-4" />;
    }
  };

  const totalInvoiced = invoices.reduce((sum, invoice) => sum + invoice.totalAmount, 0);
  const totalOutstanding = invoices.reduce((sum, invoice) => sum + invoice.outstandingAmount, 0);
  const totalPaid = invoices.reduce((sum, invoice) => sum + invoice.paidAmount, 0);
  const overdueInvoices = invoices.filter(invoice => invoice.status === 'Overdue').length;

  // Aging analysis
  const agingData = [
    { range: 'Current (0-30 days)', amount: 3350000, count: 3, percentage: 79.8 },
    { range: '31-60 days', amount: 420000, count: 1, percentage: 10.0 },
    { range: '61-90 days', amount: 325000, count: 1, percentage: 7.7 },
    { range: '90+ days', amount: 105000, count: 1, percentage: 2.5 },
  ];

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 flex items-center">
            <FileText className="w-7 h-7 text-emerald-600 mr-3" />
            Accounts Receivable Management
          </h1>
          <p className="text-gray-600">Invoice management and customer payment tracking</p>
        </div>
        <div className="flex items-center space-x-3">
          <button className="flex items-center px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
            <Download className="w-4 h-4 mr-2" />
            Export Invoices
          </button>
          <button className="flex items-center px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 transition-colors">
            <Plus className="w-4 h-4 mr-2" />
            Create Invoice
          </button>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 mb-1">Total Invoiced</p>
              <p className="text-2xl font-bold text-gray-900">₦{totalInvoiced.toLocaleString()}</p>
              <div className="flex items-center mt-2">
                <TrendingUp className="w-4 h-4 text-green-500 mr-1" />
                <span className="text-sm text-green-600 font-medium">+18.5%</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-emerald-100 text-emerald-600">
              <FileText className="w-6 h-6" />
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
              <p className="text-sm font-medium text-gray-600 mb-1">Total Outstanding</p>
              <p className="text-2xl font-bold text-gray-900">₦{totalOutstanding.toLocaleString()}</p>
              <div className="flex items-center mt-2">
                <Clock className="w-4 h-4 text-yellow-500 mr-1" />
                <span className="text-sm text-yellow-600 font-medium">Pending Collection</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-yellow-100 text-yellow-600">
              <DollarSign className="w-6 h-6" />
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
              <p className="text-sm font-medium text-gray-600 mb-1">Total Collected</p>
              <p className="text-2xl font-bold text-gray-900">₦{totalPaid.toLocaleString()}</p>
              <div className="flex items-center mt-2">
                <CheckCircle className="w-4 h-4 text-green-500 mr-1" />
                <span className="text-sm text-green-600 font-medium">
                  {((totalPaid / totalInvoiced) * 100).toFixed(1)}% Collection Rate
                </span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-green-100 text-green-600">
              <CheckCircle className="w-6 h-6" />
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
              <p className="text-sm font-medium text-gray-600 mb-1">Overdue Invoices</p>
              <p className="text-2xl font-bold text-gray-900">{overdueInvoices}</p>
              <div className="flex items-center mt-2">
                <AlertTriangle className="w-4 h-4 text-red-500 mr-1" />
                <span className="text-sm text-red-600 font-medium">Needs Attention</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-red-100 text-red-600">
              <AlertTriangle className="w-6 h-6" />
            </div>
          </div>
        </motion.div>
      </div>

      {/* View Toggle */}
      <div className="flex items-center justify-between">
        <div className="flex items-center bg-gray-100 rounded-lg p-1">
          <button
            onClick={() => setSelectedView('list')}
            className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
              selectedView === 'list'
                ? 'bg-white text-gray-900 shadow-sm'
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            Invoice List
          </button>
          <button
            onClick={() => setSelectedView('aging')}
            className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
              selectedView === 'aging'
                ? 'bg-white text-gray-900 shadow-sm'
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            Aging Analysis
          </button>
        </div>

        {/* Filters */}
        <div className="flex items-center space-x-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
            <input
              type="text"
              placeholder="Search invoices..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
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
              <option value="draft">Draft</option>
              <option value="sent">Sent</option>
              <option value="viewed">Viewed</option>
              <option value="paid">Paid</option>
              <option value="partiallypaid">Partially Paid</option>
              <option value="overdue">Overdue</option>
            </select>
          </div>
        </div>
      </div>

      {/* Main Content */}
      {selectedView === 'list' ? (
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-200">
            <h3 className="text-lg font-semibold text-gray-900">
              Invoices ({filteredInvoices.length})
            </h3>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Invoice Details
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Customer
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Amount
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Due Date
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
                {filteredInvoices.map((invoice, index) => (
                  <motion.tr
                    key={invoice.id}
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: index * 0.05 }}
                    className="hover:bg-gray-50 transition-colors"
                  >
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div>
                        <div className="text-sm font-medium text-gray-900">
                          {invoice.invoiceNumber}
                        </div>
                        <div className="text-sm text-gray-500">
                          {invoice.description}
                        </div>
                        <div className="text-xs text-gray-400 mt-1">
                          Created: {format(new Date(invoice.invoiceDate), 'MMM dd, yyyy')}
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">
                        {invoice.customerName}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">
                        ₦{invoice.totalAmount.toLocaleString()}
                      </div>
                      {invoice.paidAmount > 0 && (
                        <div className="text-sm text-green-600">
                          Paid: ₦{invoice.paidAmount.toLocaleString()}
                        </div>
                      )}
                      {invoice.outstandingAmount > 0 && (
                        <div className="text-sm text-red-600">
                          Outstanding: ₦{invoice.outstandingAmount.toLocaleString()}
                        </div>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-gray-900">
                        {format(new Date(invoice.dueDate), 'MMM dd, yyyy')}
                      </div>
                      {invoice.daysToDue < 0 ? (
                        <div className="text-xs text-red-600">
                          {Math.abs(invoice.daysToDue)} days overdue
                        </div>
                      ) : invoice.daysToDue <= 7 ? (
                        <div className="text-xs text-orange-600">
                          Due in {invoice.daysToDue} days
                        </div>
                      ) : (
                        <div className="text-xs text-gray-500">
                          Due in {invoice.daysToDue} days
                        </div>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`inline-flex items-center px-2 py-1 text-xs font-semibold rounded-full ${
                        getStatusColor(invoice.status)
                      }`}>
                        {getStatusIcon(invoice.status)}
                        <span className="ml-1">{invoice.status}</span>
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                      <div className="flex items-center space-x-2">
                        <button className="text-emerald-600 hover:text-emerald-900 transition-colors">
                          <Eye className="w-4 h-4" />
                        </button>
                        <button className="text-blue-600 hover:text-blue-900 transition-colors">
                          <Send className="w-4 h-4" />
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
        </div>
      ) : (
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-6">Accounts Receivable Aging Analysis</h3>
          <div className="space-y-4">
            {agingData.map((aging, index) => (
              <motion.div
                key={aging.range}
                initial={{ opacity: 0, x: -20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: index * 0.1 }}
                className="flex items-center justify-between p-4 border border-gray-200 rounded-lg"
              >
                <div className="flex items-center">
                  <div className={`w-4 h-4 rounded-full mr-3 ${
                    index === 0 ? 'bg-green-500' :
                    index === 1 ? 'bg-yellow-500' :
                    index === 2 ? 'bg-orange-500' : 'bg-red-500'
                  }`}></div>
                  <div>
                    <span className="font-medium text-gray-900">{aging.range}</span>
                    <div className="text-sm text-gray-500">{aging.count} invoices</div>
                  </div>
                </div>
                <div className="text-right">
                  <span className="text-lg font-bold text-gray-900">
                    ₦{aging.amount.toLocaleString()}
                  </span>
                  <div className="text-sm text-gray-500">{aging.percentage}%</div>
                </div>
                <div className="w-32 bg-gray-200 rounded-full h-2 ml-4">
                  <div 
                    className={`h-2 rounded-full ${
                      index === 0 ? 'bg-green-500' :
                      index === 1 ? 'bg-yellow-500' :
                      index === 2 ? 'bg-orange-500' : 'bg-red-500'
                    }`}
                    style={{ width: `${aging.percentage}%` }}
                  ></div>
                </div>
              </motion.div>
            ))}
          </div>
        </div>
      )}

      {filteredInvoices.length === 0 && selectedView === 'list' && (
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 px-6 py-12 text-center">
          <FileText className="w-12 h-12 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">No invoices found</h3>
          <p className="text-gray-500">
            {searchTerm || filterStatus !== 'all'
              ? 'Try adjusting your search or filter criteria.'
              : 'Get started by creating your first invoice.'
            }
          </p>
        </div>
      )}

      {/* Quick Actions */}
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.5 }}
        className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
      >
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Accounts Receivable Actions</h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <Plus className="w-6 h-6 text-emerald-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Create Invoice</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <CreditCard className="w-6 h-6 text-blue-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Record Payment</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <Send className="w-6 h-6 text-green-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Send Reminder</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <FileText className="w-6 h-6 text-purple-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Aging Report</span>
          </button>
        </div>
      </motion.div>

      {/* Optional: Document Upload for Invoice (Journal Entry) */}
      {filteredInvoices.length > 0 && (
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6 mt-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Attach Document to Invoice</h3>
          {/* Example: Attach to first invoice in list, replace with actual journalEntryId logic */}
          <JournalEntryDocumentUpload journalEntryId={filteredInvoices[0].id} />
        </div>
      )}
    </div>
  );
};

export default AccountsReceivablePage;