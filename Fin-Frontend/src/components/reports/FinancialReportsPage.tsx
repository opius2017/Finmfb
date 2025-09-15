import React, { useState } from 'react';
import { motion } from 'framer-motion';
import {
  FileText,
  Download,
  Calendar,
  Filter,
  Eye,
  Printer,
  BarChart3,
  TrendingUp,
  DollarSign,
  Building,
  Users,
  CreditCard,
} from 'lucide-react';
import { format } from 'date-fns';

interface FinancialReport {
  id: string;
  reportName: string;
  reportType: string;
  period: string;
  generatedDate: string;
  status: string;
  preparedBy: string;
  fileSize: string;
}

const FinancialReportsPage: React.FC = () => {
  const [selectedPeriod, setSelectedPeriod] = useState('2024-12');
  const [filterType, setFilterType] = useState('all');
  const [filterStatus, setFilterStatus] = useState('all');

  // Mock data - replace with actual API call
  const reports: FinancialReport[] = [
    {
      id: '1',
      reportName: 'Balance Sheet',
      reportType: 'Financial Statement',
      period: 'December 2024',
      generatedDate: '2024-12-31T23:59:59Z',
      status: 'Final',
      preparedBy: 'Chinedu Okwu',
      fileSize: '2.4 MB',
    },
    {
      id: '2',
      reportName: 'Profit & Loss Statement',
      reportType: 'Financial Statement',
      period: 'December 2024',
      generatedDate: '2024-12-31T23:59:59Z',
      status: 'Final',
      preparedBy: 'Chinedu Okwu',
      fileSize: '1.8 MB',
    },
    {
      id: '3',
      reportName: 'Cash Flow Statement',
      reportType: 'Financial Statement',
      period: 'December 2024',
      generatedDate: '2024-12-31T23:59:59Z',
      status: 'Draft',
      preparedBy: 'Fatima Aliyu',
      fileSize: '1.2 MB',
    },
    {
      id: '4',
      reportName: 'CBN Prudential Returns',
      reportType: 'Regulatory Report',
      period: 'Q4 2024',
      generatedDate: '2024-12-30T18:00:00Z',
      status: 'Submitted',
      preparedBy: 'Adebayo Ogundimu',
      fileSize: '3.1 MB',
    },
    {
      id: '5',
      reportName: 'NDIC Deposit Report',
      reportType: 'Regulatory Report',
      period: 'December 2024',
      generatedDate: '2024-12-29T16:30:00Z',
      status: 'Final',
      preparedBy: 'Kemi Adebisi',
      fileSize: '1.9 MB',
    },
    {
      id: '6',
      reportName: 'Portfolio at Risk Analysis',
      reportType: 'Management Report',
      period: 'December 2024',
      generatedDate: '2024-12-28T14:15:00Z',
      status: 'Final',
      preparedBy: 'Fatima Aliyu',
      fileSize: '4.2 MB',
    },
  ];

  const filteredReports = reports.filter((report) => {
    const matchesType = filterType === 'all' || report.reportType.toLowerCase().replace(' ', '') === filterType;
    const matchesStatus = filterStatus === 'all' || report.status.toLowerCase() === filterStatus;
    
    return matchesType && matchesStatus;
  });

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'final':
        return 'bg-green-100 text-green-800';
      case 'draft':
        return 'bg-yellow-100 text-yellow-800';
      case 'submitted':
        return 'bg-blue-100 text-blue-800';
      case 'under review':
        return 'bg-orange-100 text-orange-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getReportTypeIcon = (type: string) => {
    switch (type.toLowerCase()) {
      case 'financial statement':
        return <FileText className="w-5 h-5 text-emerald-600" />;
      case 'regulatory report':
        return <Building className="w-5 h-5 text-blue-600" />;
      case 'management report':
        return <BarChart3 className="w-5 h-5 text-purple-600" />;
      default:
        return <FileText className="w-5 h-5 text-gray-600" />;
    }
  };

  // Mock KPI data
  const kpiData = {
    totalAssets: 125000000,
    totalLiabilities: 95000000,
    netWorth: 30000000,
    monthlyRevenue: 8500000,
    monthlyExpenses: 6200000,
    netIncome: 2300000,
    portfolioAtRisk: 3.2,
    capitalAdequacyRatio: 18.5,
  };

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 flex items-center">
            <FileText className="w-7 h-7 text-emerald-600 mr-3" />
            Financial Reports & Analytics
          </h1>
          <p className="text-gray-600">IFRS-compliant financial statements and regulatory reports</p>
        </div>
        <div className="flex items-center space-x-3">
          <button className="flex items-center px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
            <Calendar className="w-4 h-4 mr-2" />
            Schedule Report
          </button>
          <button className="flex items-center px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 transition-colors">
            <FileText className="w-4 h-4 mr-2" />
            Generate Report
          </button>
        </div>
      </div>

      {/* Key Financial Metrics */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 mb-1">Total Assets</p>
              <p className="text-2xl font-bold text-gray-900">₦{kpiData.totalAssets.toLocaleString()}</p>
              <div className="flex items-center mt-2">
                <TrendingUp className="w-4 h-4 text-green-500 mr-1" />
                <span className="text-sm text-green-600 font-medium">+8.2%</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-emerald-100 text-emerald-600">
              <Building className="w-6 h-6" />
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
              <p className="text-sm font-medium text-gray-600 mb-1">Net Worth</p>
              <p className="text-2xl font-bold text-gray-900">₦{kpiData.netWorth.toLocaleString()}</p>
              <div className="flex items-center mt-2">
                <TrendingUp className="w-4 h-4 text-green-500 mr-1" />
                <span className="text-sm text-green-600 font-medium">+12.5%</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-green-100 text-green-600">
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
              <p className="text-sm font-medium text-gray-600 mb-1">Monthly Revenue</p>
              <p className="text-2xl font-bold text-gray-900">₦{kpiData.monthlyRevenue.toLocaleString()}</p>
              <div className="flex items-center mt-2">
                <TrendingUp className="w-4 h-4 text-blue-500 mr-1" />
                <span className="text-sm text-blue-600 font-medium">+15.3%</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-blue-100 text-blue-600">
              <BarChart3 className="w-6 h-6" />
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
              <p className="text-2xl font-bold text-gray-900">{kpiData.portfolioAtRisk}%</p>
              <div className="flex items-center mt-2">
                <span className="text-sm text-green-600 font-medium">Within CBN Limit</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-orange-100 text-orange-600">
              <CreditCard className="w-6 h-6" />
            </div>
          </div>
        </motion.div>
      </div>

      {/* Reports Section */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-200">
        <div className="px-6 py-4 border-b border-gray-200">
          <div className="flex items-center justify-between">
            <h3 className="text-lg font-semibold text-gray-900">Available Reports</h3>
            <div className="flex items-center space-x-4">
              {/* Period Selector */}
              <div className="flex items-center space-x-2">
                <Calendar className="w-5 h-5 text-gray-400" />
                <select
                  value={selectedPeriod}
                  onChange={(e) => setSelectedPeriod(e.target.value)}
                  className="border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
                >
                  <option value="2024-12">December 2024</option>
                  <option value="2024-11">November 2024</option>
                  <option value="2024-10">October 2024</option>
                  <option value="2024-q4">Q4 2024</option>
                  <option value="2024">Year 2024</option>
                </select>
              </div>

              {/* Type Filter */}
              <div className="flex items-center space-x-2">
                <Filter className="w-5 h-5 text-gray-400" />
                <select
                  value={filterType}
                  onChange={(e) => setFilterType(e.target.value)}
                  className="border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
                >
                  <option value="all">All Types</option>
                  <option value="financialstatement">Financial Statements</option>
                  <option value="regulatoryreport">Regulatory Reports</option>
                  <option value="managementreport">Management Reports</option>
                </select>
              </div>

              {/* Status Filter */}
              <div className="flex items-center space-x-2">
                <select
                  value={filterStatus}
                  onChange={(e) => setFilterStatus(e.target.value)}
                  className="border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
                >
                  <option value="all">All Status</option>
                  <option value="final">Final</option>
                  <option value="draft">Draft</option>
                  <option value="submitted">Submitted</option>
                  <option value="under review">Under Review</option>
                </select>
              </div>
            </div>
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Report Details
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Type
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Period
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Generated
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
              {filteredReports.map((report, index) => (
                <motion.tr
                  key={report.id}
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: index * 0.05 }}
                  className="hover:bg-gray-50 transition-colors"
                >
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center">
                      {getReportTypeIcon(report.reportType)}
                      <div className="ml-3">
                        <div className="text-sm font-medium text-gray-900">
                          {report.reportName}
                        </div>
                        <div className="text-sm text-gray-500">
                          {report.fileSize} • Prepared by {report.preparedBy}
                        </div>
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {report.reportType}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {report.period}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {format(new Date(report.generatedDate), 'MMM dd, yyyy HH:mm')}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${
                      getStatusColor(report.status)
                    }`}>
                      {report.status}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <div className="flex items-center space-x-2">
                      <button className="text-emerald-600 hover:text-emerald-900 transition-colors">
                        <Eye className="w-4 h-4" />
                      </button>
                      <button className="text-blue-600 hover:text-blue-900 transition-colors">
                        <Download className="w-4 h-4" />
                      </button>
                      <button className="text-gray-600 hover:text-gray-900 transition-colors">
                        <Printer className="w-4 h-4" />
                      </button>
                    </div>
                  </td>
                </motion.tr>
              ))}
            </tbody>
          </table>
        </div>

        {filteredReports.length === 0 && (
          <div className="px-6 py-12 text-center">
            <FileText className="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">No reports found</h3>
            <p className="text-gray-500">
              Try adjusting your filter criteria or generate a new report.
            </p>
          </div>
        )}
      </div>

      {/* Quick Report Generation */}
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.5 }}
        className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
      >
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Quick Report Generation</h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {/* Financial Statements */}
          <div className="border border-gray-200 rounded-lg p-4">
            <div className="flex items-center mb-3">
              <FileText className="w-5 h-5 text-emerald-600 mr-2" />
              <h4 className="font-semibold text-gray-900">Financial Statements</h4>
            </div>
            <div className="space-y-2">
              <button className="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded transition-colors">
                Balance Sheet
              </button>
              <button className="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded transition-colors">
                Profit & Loss Statement
              </button>
              <button className="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded transition-colors">
                Cash Flow Statement
              </button>
              <button className="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded transition-colors">
                Statement of Equity
              </button>
            </div>
          </div>

          {/* Regulatory Reports */}
          <div className="border border-gray-200 rounded-lg p-4">
            <div className="flex items-center mb-3">
              <Building className="w-5 h-5 text-blue-600 mr-2" />
              <h4 className="font-semibold text-gray-900">Regulatory Reports</h4>
            </div>
            <div className="space-y-2">
              <button className="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded transition-colors">
                CBN Prudential Returns
              </button>
              <button className="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded transition-colors">
                NDIC Deposit Report
              </button>
              <button className="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded transition-colors">
                FIRS Tax Returns
              </button>
              <button className="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded transition-colors">
                PENCOM Returns
              </button>
            </div>
          </div>

          {/* Management Reports */}
          <div className="border border-gray-200 rounded-lg p-4">
            <div className="flex items-center mb-3">
              <BarChart3 className="w-5 h-5 text-purple-600 mr-2" />
              <h4 className="font-semibold text-gray-900">Management Reports</h4>
            </div>
            <div className="space-y-2">
              <button className="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded transition-colors">
                Portfolio Analysis
              </button>
              <button className="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded transition-colors">
                Risk Assessment
              </button>
              <button className="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded transition-colors">
                Branch Performance
              </button>
              <button className="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 rounded transition-colors">
                Customer Analytics
              </button>
            </div>
          </div>
        </div>
      </motion.div>

      {/* Compliance Dashboard */}
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.6 }}
        className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
      >
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Regulatory Compliance Status</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <div className="text-center">
            <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-3">
              <Building className="w-8 h-8 text-green-600" />
            </div>
            <h4 className="text-sm font-semibold text-gray-900 mb-1">CBN Compliance</h4>
            <p className="text-xs text-green-600 font-medium">✓ Up to Date</p>
            <p className="text-xs text-gray-500 mt-1">Last filed: Dec 30, 2024</p>
          </div>
          
          <div className="text-center">
            <div className="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-3">
              <Users className="w-8 h-8 text-blue-600" />
            </div>
            <h4 className="text-sm font-semibold text-gray-900 mb-1">NDIC Reports</h4>
            <p className="text-xs text-blue-600 font-medium">✓ Current</p>
            <p className="text-xs text-gray-500 mt-1">Last filed: Dec 29, 2024</p>
          </div>
          
          <div className="text-center">
            <div className="w-16 h-16 bg-purple-100 rounded-full flex items-center justify-center mx-auto mb-3">
              <DollarSign className="w-8 h-8 text-purple-600" />
            </div>
            <h4 className="text-sm font-semibold text-gray-900 mb-1">FIRS Returns</h4>
            <p className="text-xs text-purple-600 font-medium">✓ Filed</p>
            <p className="text-xs text-gray-500 mt-1">Next due: Jan 21, 2025</p>
          </div>
          
          <div className="text-center">
            <div className="w-16 h-16 bg-emerald-100 rounded-full flex items-center justify-center mx-auto mb-3">
              <BarChart3 className="w-8 h-8 text-emerald-600" />
            </div>
            <h4 className="text-sm font-semibold text-gray-900 mb-1">IFRS Standards</h4>
            <p className="text-xs text-emerald-600 font-medium">✓ Compliant</p>
            <p className="text-xs text-gray-500 mt-1">IFRS 9, 15, 16</p>
          </div>
        </div>
      </motion.div>
    </div>
  );
};

export default FinancialReportsPage;