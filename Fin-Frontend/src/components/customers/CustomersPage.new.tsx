import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Search, UserPlus, Filter, ChevronLeft, ChevronRight } from 'lucide-react';
import { useGetCustomersQuery } from '../../services/customersApi';
import { Customer } from '../../services/customersApi';

const ITEMS_PER_PAGE = 10;

interface CustomerFilters {
  search?: string;
  kycLevel?: 1 | 2 | 3;
  kycStatus?: 'pending' | 'in-progress' | 'approved' | 'rejected';
  accountType?: 'individual' | 'business' | 'corporate';
}

const CustomersPage: React.FC = () => {
  const [currentPage, setCurrentPage] = useState(1);
  const [filters, setFilters] = useState<CustomerFilters>({});
  const [showFilters, setShowFilters] = useState(false);

  const { data, isLoading, error } = useGetCustomersQuery({
    ...filters,
    page: currentPage,
    limit: ITEMS_PER_PAGE,
  });

  const handleSearch = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFilters({ ...filters, search: e.target.value });
    setCurrentPage(1);
  };

  const handleFilterChange = (key: keyof CustomerFilters, value: any) => {
    setFilters({ ...filters, [key]: value });
    setCurrentPage(1);
  };

  const totalPages = Math.ceil((data?.total || 0) / ITEMS_PER_PAGE);

  if (isLoading) {
    return (
      <div className="p-6">
        <div className="animate-pulse space-y-4">
          {[...Array(5)].map((_, i) => (
            <div key={i} className="h-16 bg-gray-100 rounded-lg" />
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-6">
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-red-700">
          Failed to load customers. Please try again.
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Customers</h1>
          <p className="mt-1 text-sm text-gray-500">
            Manage customer profiles and verify KYC documents
          </p>
        </div>

        <Link
          to="/customers/new"
          className="flex items-center px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2"
        >
          <UserPlus size={18} className="mr-2" />
          New Customer
        </Link>
      </div>

      <div className="bg-white rounded-lg shadow">
        {/* Search and Filters */}
        <div className="p-4 border-b border-gray-200 space-y-4">
          <div className="flex items-center space-x-4">
            <div className="flex-1 relative">
              <input
                type="text"
                placeholder="Search customers..."
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500"
                value={filters.search || ''}
                onChange={handleSearch}
              />
              <Search className="absolute left-3 top-2.5 h-5 w-5 text-gray-400" />
            </div>
            <button
              onClick={() => setShowFilters(!showFilters)}
              className="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 flex items-center space-x-2"
            >
              <Filter size={18} />
              <span>Filters</span>
            </button>
          </div>

          {/* Filter Options */}
          {showFilters && (
            <motion.div
              initial={{ height: 0, opacity: 0 }}
              animate={{ height: 'auto', opacity: 1 }}
              exit={{ height: 0, opacity: 0 }}
              className="grid grid-cols-3 gap-4"
            >
              <div>
                <label htmlFor="kycLevel" className="block text-sm font-medium text-gray-700 mb-1">
                  KYC Level
                </label>
                <select
                  id="kycLevel"
                  value={filters.kycLevel || ''}
                  onChange={(e) => handleFilterChange('kycLevel', e.target.value ? Number(e.target.value) : undefined)}
                  className="w-full rounded-lg border-gray-300 focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500"
                >
                  <option value="">All Levels</option>
                  <option value="1">Level 1</option>
                  <option value="2">Level 2</option>
                  <option value="3">Level 3</option>
                </select>
              </div>

              <div>
                <label htmlFor="kycStatus" className="block text-sm font-medium text-gray-700 mb-1">
                  KYC Status
                </label>
                <select
                  id="kycStatus"
                  value={filters.kycStatus || ''}
                  onChange={(e) => handleFilterChange('kycStatus', e.target.value || undefined)}
                  className="w-full rounded-lg border-gray-300 focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500"
                >
                  <option value="">All Statuses</option>
                  <option value="pending">Pending</option>
                  <option value="in-progress">In Progress</option>
                  <option value="approved">Approved</option>
                  <option value="rejected">Rejected</option>
                </select>
              </div>

              <div>
                <label htmlFor="accountType" className="block text-sm font-medium text-gray-700 mb-1">
                  Account Type
                </label>
                <select
                  id="accountType"
                  value={filters.accountType || ''}
                  onChange={(e) => handleFilterChange('accountType', e.target.value || undefined)}
                  className="w-full rounded-lg border-gray-300 focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500"
                >
                  <option value="">All Types</option>
                  <option value="individual">Individual</option>
                  <option value="business">Business</option>
                  <option value="corporate">Corporate</option>
                </select>
              </div>
            </motion.div>
          )}
        </div>

        {/* Customer List */}
        <div className="divide-y divide-gray-200">
          {data?.customers.map((customer: Customer) => (
            <Link
              key={customer.id}
              to={`/customers/${customer.id}`}
              className="block hover:bg-gray-50 transition-colors"
            >
              <div className="p-6">
                <div className="flex items-center justify-between">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center space-x-3">
                      <h2 className="text-lg font-medium text-gray-900 truncate">
                        {customer.firstName} {customer.lastName}
                      </h2>
                      <span className={`px-2 py-1 text-xs rounded-full ${getKycStatusStyle(customer.kycStatus)}`}>
                        {customer.kycStatus}
                      </span>
                    </div>
                    <div className="mt-1 flex items-center space-x-6 text-sm text-gray-500">
                      <span>#{customer.customerNumber}</span>
                      <span>•</span>
                      <span>{customer.email || customer.phoneNumber}</span>
                      <span>•</span>
                      <span>KYC Level {customer.kycLevel}</span>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="text-sm font-medium text-gray-900">
                      {customer.accountType}
                    </p>
                    <p className="text-xs text-gray-500">
                      Created {new Date(customer.createdAt).toLocaleDateString()}
                    </p>
                  </div>
                </div>
              </div>
            </Link>
          ))}
        </div>

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="p-4 border-t border-gray-200 flex items-center justify-between">
            <button
              onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
              disabled={currentPage === 1}
              className="px-3 py-2 rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed flex items-center space-x-2"
            >
              <ChevronLeft size={16} />
              <span>Previous</span>
            </button>
            <span className="text-sm text-gray-600">
              Page {currentPage} of {totalPages}
            </span>
            <button
              onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
              disabled={currentPage === totalPages}
              className="px-3 py-2 rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed flex items-center space-x-2"
            >
              <span>Next</span>
              <ChevronRight size={16} />
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

const getKycStatusStyle = (status: Customer['kycStatus']) => {
  switch (status) {
    case 'approved':
      return 'bg-green-100 text-green-800';
    case 'rejected':
      return 'bg-red-100 text-red-800';
    case 'in-progress':
      return 'bg-yellow-100 text-yellow-800';
    default:
      return 'bg-gray-100 text-gray-800';
  }
};

export default CustomersPage;