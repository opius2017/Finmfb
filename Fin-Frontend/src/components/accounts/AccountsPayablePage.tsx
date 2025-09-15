import React, { useState } from 'react';
import { motion } from 'framer-motion';
import {
  Receipt,
  Plus,
  Search,
  Filter,
  Download,
  Eye,
  MoreHorizontal,
  Building,
  DollarSign,
  Calendar,
  AlertTriangle,
  CheckCircle,
  Clock,
  FileText,
  ShoppingCart,
  Truck,
} from 'lucide-react';
import { format } from 'date-fns';

interface VendorBill {
  id: string;
  billNumber: string;
  vendorName: string;
  vendorInvoiceNumber: string;
  billDate: string;
  dueDate: string;
  totalAmount: number;
  paidAmount: number;
  outstandingAmount: number;
  status: string;
  description: string;
  daysToDue: number;
}

interface Vendor {
  id: string;
  vendorNumber: string;
  vendorName: string;
  vendorType: string;
  email: string;
  phoneNumber: string;
  totalOutstanding: number;
  status: string;
  createdAt: string;
}

const AccountsPayablePage: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState('all');
  const [activeTab, setActiveTab] = useState<'bills' | 'vendors' | 'purchase-orders'>('bills');

  // Mock bills data
  const bills: VendorBill[] = [
    {
      id: '1',
      billNumber: 'BILL001234',
      vendorName: 'Office Supplies Ltd',
      vendorInvoiceNumber: 'INV-2024-001',
      billDate: '2024-12-15',
      dueDate: '2024-12-30',
      totalAmount: 450000,
      paidAmount: 0,
      outstandingAmount: 450000,
      status: 'Pending',
      description: 'Office stationery and supplies',
      daysToDue: 10,
    },
    {
      id: '2',
      billNumber: 'BILL001235',
      vendorName: 'Tech Solutions Nigeria',
      vendorInvoiceNumber: 'TS-2024-089',
      billDate: '2024-12-10',
      dueDate: '2024-12-25',
      totalAmount: 1200000,
      paidAmount: 1200000,
      outstandingAmount: 0,
      status: 'Paid',
      description: 'IT equipment and software licenses',
      daysToDue: 0,
    },
    {
      id: '3',
      billNumber: 'BILL001236',
      vendorName: 'Cleaning Services Co',
      vendorInvoiceNumber: 'CS-DEC-2024',
      billDate: '2024-12-01',
      dueDate: '2024-12-20',
      totalAmount: 85000,
      paidAmount: 0,
      outstandingAmount: 85000,
      status: 'Overdue',
      description: 'Monthly cleaning services',
      daysToDue: -1,
    },
    {
      id: '4',
      billNumber: 'BILL001237',
      vendorName: 'Security Guard Services',
      vendorInvoiceNumber: 'SGS-2024-12',
      billDate: '2024-12-18',
      dueDate: '2025-01-02',
      totalAmount: 320000,
      paidAmount: 160000,
      outstandingAmount: 160000,
      status: 'Partially Paid',
      description: 'Security services for December',
      daysToDue: 13,
    },
  ];

  // Mock vendors data
  const vendors: Vendor[] = [
    {
      id: '1',
      vendorNumber: 'VEN001',
      vendorName: 'Office Supplies Ltd',
      vendorType: 'Company',
      email: 'accounts@officesupplies.ng',
      phoneNumber: '+234-801-234-5678',
      totalOutstanding: 450000,
      status: 'Active',
      createdAt: '2023-06-15',
    },
    {
      id: '2',
      vendorNumber: 'VEN002',
      vendorName: 'Tech Solutions Nigeria',
      vendorType: 'Company',
      email: 'billing@techsolutions.ng',
      phoneNumber: '+234-802-345-6789',
      totalOutstanding: 0,
      status: 'Active',
      createdAt: '2023-03-20',
    },
    {
      id: '3',
      vendorNumber: 'VEN003',
      vendorName: 'Cleaning Services Co',
      vendorType: 'Company',
      email: 'admin@cleaningco.ng',
      phoneNumber: '+234-803-456-7890',
      totalOutstanding: 85000,
      status: 'Active',
      createdAt: '2023-01-10',
    },
  ];

  const filteredBills = bills.filter((bill) => {
    const matchesSearch = bill.billNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         bill.vendorName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         bill.description.toLowerCase().includes(searchTerm.toLowerCase());
    
    const matchesStatus = filterStatus === 'all' || bill.status.toLowerCase().replace(' ', '') === filterStatus;
    
    return matchesSearch && matchesStatus;
  });

  const filteredVendors = vendors.filter((vendor) => {
    const matchesSearch = vendor.vendorName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         vendor.vendorNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         vendor.email.toLowerCase().includes(searchTerm.toLowerCase());
    
    const matchesStatus = filterStatus === 'all' || vendor.status.toLowerCase() === filterStatus;
    
    return matchesSearch && matchesStatus;
  });

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'pending':
        return 'bg-yellow-100 text-yellow-800';
      case 'paid':
        return 'bg-green-100 text-green-800';
      case 'overdue':
        return 'bg-red-100 text-red-800';
      case 'partially paid':
        return 'bg-blue-100 text-blue-800';
      case 'active':
        return 'bg-green-100 text-green-800';
      case 'inactive':
        return 'bg-gray-100 text-gray-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status.toLowerCase()) {
      case 'pending':
        return <Clock className="w-4 h-4" />;
      case 'paid':
        return <CheckCircle className="w-4 h-4" />;
      case 'overdue':
        return <AlertTriangle className="w-4 h-4" />;
      case 'partially paid':
        return <DollarSign className="w-4 h-4" />;
      case 'active':
        return <CheckCircle className="w-4 h-4" />;
      default:
        return <Clock className="w-4 h-4" />;
    }
  };

  const totalOutstanding = bills.reduce((sum, bill) => sum + bill.outstandingAmount, 0);
  const overdueBills = bills.filter(bill => bill.status === 'Overdue').length;
  const pendingBills = bills.filter(bill => bill.status === 'Pending').length;
  const totalVendors = vendors.length;

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 flex items-center">
            <Receipt className="w-7 h-7 text-emerald-600 mr-3" />
            Accounts Payable Management
          </h1>
          <p className="text-gray-600">Vendor management and bill processing with approval workflows</p>
        </div>
        <div className="flex items-center space-x-3">
          <button className="flex items-center px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
            <Download className="w-4 h-4 mr-2" />
            Export Data
          </button>
          <button className="flex items-center px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 transition-colors">
            <Plus className="w-4 h-4 mr-2" />
            {activeTab === 'bills' ? 'New Bill' : activeTab === 'vendors' ? 'Add Vendor' : 'Create PO'}
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
              <p className="text-sm font-medium text-gray-600 mb-1">Total Outstanding</p>
              <p className="text-2xl font-bold text-gray-900">₦{totalOutstanding.toLocaleString()}</p>
            </div>
            <div className="p-3 rounded-lg bg-emerald-100 text-emerald-600">
              <DollarSign className="w-6 h-6" />
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
              <p className="text-sm font-medium text-gray-600 mb-1">Pending Bills</p>
              <p className="text-2xl font-bold text-gray-900">{pendingBills}</p>
            </div>
            <div className="p-3 rounded-lg bg-yellow-100 text-yellow-600">
              <Clock className="w-6 h-6" />
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
              <p className="text-sm font-medium text-gray-600 mb-1">Overdue Bills</p>
              <p className="text-2xl font-bold text-gray-900">{overdueBills}</p>
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
              <p className="text-sm font-medium text-gray-600 mb-1">Active Vendors</p>
              <p className="text-2xl font-bold text-gray-900">{totalVendors}</p>
            </div>
            <div className="p-3 rounded-lg bg-blue-100 text-blue-600">
              <Building className="w-6 h-6" />
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
              onClick={() => setActiveTab('bills')}
              className={`py-4 px-1 border-b-2 font-medium text-sm transition-colors ${
                activeTab === 'bills'
                  ? 'border-emerald-500 text-emerald-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              Vendor Bills
            </button>
            <button
              onClick={() => setActiveTab('vendors')}
              className={`py-4 px-1 border-b-2 font-medium text-sm transition-colors ${
                activeTab === 'vendors'
                  ? 'border-emerald-500 text-emerald-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              Vendor Management
            </button>
            <button
              onClick={() => setActiveTab('purchase-orders')}
              className={`py-4 px-1 border-b-2 font-medium text-sm transition-colors ${
                activeTab === 'purchase-orders'
                  ? 'border-emerald-500 text-emerald-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              Purchase Orders
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
                placeholder={`Search ${activeTab === 'bills' ? 'bills' : activeTab === 'vendors' ? 'vendors' : 'purchase orders'}...`}
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
                {activeTab === 'bills' ? (
                  <>
                    <option value="pending">Pending</option>
                    <option value="paid">Paid</option>
                    <option value="overdue">Overdue</option>
                    <option value="partiallypaid">Partially Paid</option>
                  </>
                ) : (
                  <>
                    <option value="active">Active</option>
                    <option value="inactive">Inactive</option>
                    <option value="suspended">Suspended</option>
                  </>
                )}
              </select>
            </div>
          </div>
        </div>

        {/* Content */}
        <div className="overflow-x-auto">
          {activeTab === 'bills' ? (
            <table className="w-full">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Bill Details
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Vendor
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
                {filteredBills.map((bill, index) => (
                  <motion.tr
                    key={bill.id}
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: index * 0.05 }}
                    className="hover:bg-gray-50 transition-colors"
                  >
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div>
                        <div className="text-sm font-medium text-gray-900">
                          {bill.billNumber}
                        </div>
                        <div className="text-sm text-gray-500">
                          Invoice: {bill.vendorInvoiceNumber}
                        </div>
                        <div className="text-xs text-gray-400 mt-1">
                          {bill.description}
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">
                        {bill.vendorName}
                      </div>
                      <div className="text-sm text-gray-500">
                        Bill Date: {format(new Date(bill.billDate), 'MMM dd, yyyy')}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">
                        ₦{bill.totalAmount.toLocaleString()}
                      </div>
                      {bill.paidAmount > 0 && (
                        <div className="text-sm text-gray-500">
                          Paid: ₦{bill.paidAmount.toLocaleString()}
                        </div>
                      )}
                      {bill.outstandingAmount > 0 && (
                        <div className="text-sm text-red-600">
                          Outstanding: ₦{bill.outstandingAmount.toLocaleString()}
                        </div>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-gray-900">
                        {format(new Date(bill.dueDate), 'MMM dd, yyyy')}
                      </div>
                      {bill.daysToDue < 0 ? (
                        <div className="text-xs text-red-600">
                          {Math.abs(bill.daysToDue)} days overdue
                        </div>
                      ) : bill.daysToDue <= 7 ? (
                        <div className="text-xs text-orange-600">
                          Due in {bill.daysToDue} days
                        </div>
                      ) : (
                        <div className="text-xs text-gray-500">
                          Due in {bill.daysToDue} days
                        </div>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`inline-flex items-center px-2 py-1 text-xs font-semibold rounded-full ${
                        getStatusColor(bill.status)
                      }`}>
                        {getStatusIcon(bill.status)}
                        <span className="ml-1">{bill.status}</span>
                      </span>
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
          ) : activeTab === 'vendors' ? (
            <table className="w-full">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Vendor Details
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Contact Information
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Outstanding Amount
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
                {filteredVendors.map((vendor, index) => (
                  <motion.tr
                    key={vendor.id}
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: index * 0.05 }}
                    className="hover:bg-gray-50 transition-colors"
                  >
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <div className="w-10 h-10 bg-emerald-600 rounded-full flex items-center justify-center">
                          <span className="text-white font-medium text-sm">
                            {vendor.vendorName.split(' ').map(n => n[0]).join('')}
                          </span>
                        </div>
                        <div className="ml-4">
                          <div className="text-sm font-medium text-gray-900">
                            {vendor.vendorName}
                          </div>
                          <div className="text-sm text-gray-500">
                            {vendor.vendorNumber} • {vendor.vendorType}
                          </div>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-gray-900">{vendor.email}</div>
                      <div className="text-sm text-gray-500">{vendor.phoneNumber}</div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      ₦{vendor.totalOutstanding.toLocaleString()}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`inline-flex items-center px-2 py-1 text-xs font-semibold rounded-full ${
                        getStatusColor(vendor.status)
                      }`}>
                        {getStatusIcon(vendor.status)}
                        <span className="ml-1">{vendor.status}</span>
                      </span>
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
          ) : (
            <div className="p-12 text-center">
              <ShoppingCart className="w-12 h-12 text-gray-400 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">Purchase Orders</h3>
              <p className="text-gray-500">Purchase order management coming soon...</p>
            </div>
          )}
        </div>

        {((activeTab === 'bills' && filteredBills.length === 0) || 
          (activeTab === 'vendors' && filteredVendors.length === 0)) && (
          <div className="px-6 py-12 text-center">
            <Receipt className="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">
              No {activeTab} found
            </h3>
            <p className="text-gray-500">
              Try adjusting your search or filter criteria.
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
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Accounts Payable Actions</h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <Plus className="w-6 h-6 text-emerald-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Add Vendor</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <FileText className="w-6 h-6 text-blue-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Create Bill</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <DollarSign className="w-6 h-6 text-green-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Process Payment</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <Truck className="w-6 h-6 text-purple-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Purchase Order</span>
          </button>
        </div>
      </motion.div>
    </div>
  );
};

export default AccountsPayablePage;