import React, { useState } from 'react';
import { motion } from 'framer-motion';
import {
  Briefcase,
  Plus,
  Search,
  Filter,
  Download,
  Eye,
  MoreHorizontal,
  Users,
  DollarSign,
  Calculator,
  FileText,
  CheckCircle,
  Clock,
  AlertTriangle,
  TrendingUp,
  Calendar,
} from 'lucide-react';
import { format } from 'date-fns';

interface PayrollEntry {
  id: string;
  payrollNumber: string;
  employeeName: string;
  employeeNumber: string;
  department: string;
  position: string;
  basicSalary: number;
  allowances: number;
  grossEarnings: number;
  deductions: number;
  netPay: number;
  payPeriod: string;
  status: string;
  payDate: string;
}

interface Employee {
  id: string;
  employeeNumber: string;
  fullName: string;
  department: string;
  position: string;
  basicSalary: number;
  grossSalary: number;
  status: string;
  hireDate: string;
}

const PayrollManagementPage: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState('');
  const [filterDepartment, setFilterDepartment] = useState('all');
  const [filterStatus, setFilterStatus] = useState('all');
  const [selectedPeriod, setSelectedPeriod] = useState('2024-12');
  const [activeTab, setActiveTab] = useState<'payroll' | 'employees'>('payroll');

  // Mock payroll data
  const payrollEntries: PayrollEntry[] = [
    {
      id: '1',
      payrollNumber: 'PAY202412001',
      employeeName: 'Adebayo Ogundimu',
      employeeNumber: 'EMP001',
      department: 'Operations',
      position: 'Branch Manager',
      basicSalary: 450000,
      allowances: 180000,
      grossEarnings: 630000,
      deductions: 94500,
      netPay: 535500,
      payPeriod: 'December 2024',
      status: 'Paid',
      payDate: '2024-12-28',
    },
    {
      id: '2',
      payrollNumber: 'PAY202412002',
      employeeName: 'Fatima Aliyu',
      employeeNumber: 'EMP002',
      department: 'Credit',
      position: 'Loan Officer',
      basicSalary: 280000,
      allowances: 112000,
      grossEarnings: 392000,
      deductions: 58800,
      netPay: 333200,
      payPeriod: 'December 2024',
      status: 'Approved',
      payDate: '2024-12-30',
    },
    {
      id: '3',
      payrollNumber: 'PAY202412003',
      employeeName: 'Chinedu Okwu',
      employeeNumber: 'EMP003',
      department: 'Finance',
      position: 'Accountant',
      basicSalary: 320000,
      allowances: 128000,
      grossEarnings: 448000,
      deductions: 67200,
      netPay: 380800,
      payPeriod: 'December 2024',
      status: 'Calculated',
      payDate: '2024-12-30',
    },
    {
      id: '4',
      payrollNumber: 'PAY202412004',
      employeeName: 'Kemi Adebisi',
      employeeNumber: 'EMP004',
      department: 'Customer Service',
      position: 'Teller',
      basicSalary: 180000,
      allowances: 72000,
      grossEarnings: 252000,
      deductions: 37800,
      netPay: 214200,
      payPeriod: 'December 2024',
      status: 'Draft',
      payDate: '2024-12-30',
    },
  ];

  // Mock employee data
  const employees: Employee[] = [
    {
      id: '1',
      employeeNumber: 'EMP001',
      fullName: 'Adebayo Ogundimu',
      department: 'Operations',
      position: 'Branch Manager',
      basicSalary: 450000,
      grossSalary: 630000,
      status: 'Active',
      hireDate: '2022-01-15',
    },
    {
      id: '2',
      employeeNumber: 'EMP002',
      fullName: 'Fatima Aliyu',
      department: 'Credit',
      position: 'Loan Officer',
      basicSalary: 280000,
      grossSalary: 392000,
      status: 'Active',
      hireDate: '2022-03-10',
    },
    {
      id: '3',
      employeeNumber: 'EMP003',
      fullName: 'Chinedu Okwu',
      department: 'Finance',
      position: 'Accountant',
      basicSalary: 320000,
      grossSalary: 448000,
      status: 'Active',
      hireDate: '2021-11-20',
    },
    {
      id: '4',
      employeeNumber: 'EMP004',
      fullName: 'Kemi Adebisi',
      department: 'Customer Service',
      position: 'Teller',
      basicSalary: 180000,
      grossSalary: 252000,
      status: 'Active',
      hireDate: '2023-06-01',
    },
  ];

  const filteredPayrollEntries = payrollEntries.filter((entry) => {
    const matchesSearch = entry.employeeName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         entry.employeeNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         entry.payrollNumber.toLowerCase().includes(searchTerm.toLowerCase());
    
    const matchesDepartment = filterDepartment === 'all' || entry.department.toLowerCase() === filterDepartment;
    const matchesStatus = filterStatus === 'all' || entry.status.toLowerCase() === filterStatus;
    
    return matchesSearch && matchesDepartment && matchesStatus;
  });

  const filteredEmployees = employees.filter((employee) => {
    const matchesSearch = employee.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         employee.employeeNumber.toLowerCase().includes(searchTerm.toLowerCase());
    
    const matchesDepartment = filterDepartment === 'all' || employee.department.toLowerCase() === filterDepartment;
    const matchesStatus = filterStatus === 'all' || employee.status.toLowerCase() === filterStatus;
    
    return matchesSearch && matchesDepartment && matchesStatus;
  });

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'paid':
        return 'bg-green-100 text-green-800';
      case 'approved':
        return 'bg-blue-100 text-blue-800';
      case 'calculated':
        return 'bg-yellow-100 text-yellow-800';
      case 'draft':
        return 'bg-gray-100 text-gray-800';
      case 'active':
        return 'bg-green-100 text-green-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status.toLowerCase()) {
      case 'paid':
        return <CheckCircle className="w-4 h-4" />;
      case 'approved':
        return <CheckCircle className="w-4 h-4" />;
      case 'calculated':
        return <Calculator className="w-4 h-4" />;
      case 'draft':
        return <Clock className="w-4 h-4" />;
      case 'active':
        return <CheckCircle className="w-4 h-4" />;
      default:
        return <AlertTriangle className="w-4 h-4" />;
    }
  };

  const totalEmployees = employees.length;
  const totalGrossEarnings = payrollEntries.reduce((sum, entry) => sum + entry.grossEarnings, 0);
  const totalDeductions = payrollEntries.reduce((sum, entry) => sum + entry.deductions, 0);
  const totalNetPay = payrollEntries.reduce((sum, entry) => sum + entry.netPay, 0);
  const pendingApprovals = payrollEntries.filter(entry => entry.status === 'Calculated').length;

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 flex items-center">
            <Briefcase className="w-7 h-7 text-emerald-600 mr-3" />
            Payroll Management System
          </h1>
          <p className="text-gray-600">Automated payroll processing with Nigerian tax compliance</p>
        </div>
        <div className="flex items-center space-x-3">
          <button className="flex items-center px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
            <Download className="w-4 h-4 mr-2" />
            Export Payroll
          </button>
          <button className="flex items-center px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 transition-colors">
            <Calculator className="w-4 h-4 mr-2" />
            Process Payroll
          </button>
        </div>
      </div>

      {/* Payroll Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-6">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 mb-1">Total Employees</p>
              <p className="text-2xl font-bold text-gray-900">{totalEmployees}</p>
              <div className="flex items-center mt-2">
                <TrendingUp className="w-4 h-4 text-green-500 mr-1" />
                <span className="text-sm text-green-600 font-medium">+2 this month</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-emerald-100 text-emerald-600">
              <Users className="w-6 h-6" />
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
              <p className="text-sm font-medium text-gray-600 mb-1">Gross Earnings</p>
              <p className="text-2xl font-bold text-gray-900">₦{totalGrossEarnings.toLocaleString()}</p>
              <div className="flex items-center mt-2">
                <DollarSign className="w-4 h-4 text-blue-500 mr-1" />
                <span className="text-sm text-blue-600 font-medium">Monthly Total</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-blue-100 text-blue-600">
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
              <p className="text-sm font-medium text-gray-600 mb-1">Total Deductions</p>
              <p className="text-2xl font-bold text-gray-900">₦{totalDeductions.toLocaleString()}</p>
              <div className="flex items-center mt-2">
                <Calculator className="w-4 h-4 text-orange-500 mr-1" />
                <span className="text-sm text-orange-600 font-medium">PAYE + Pension</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-orange-100 text-orange-600">
              <Calculator className="w-6 h-6" />
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
              <p className="text-sm font-medium text-gray-600 mb-1">Net Pay</p>
              <p className="text-2xl font-bold text-gray-900">₦{totalNetPay.toLocaleString()}</p>
              <div className="flex items-center mt-2">
                <CheckCircle className="w-4 h-4 text-green-500 mr-1" />
                <span className="text-sm text-green-600 font-medium">Take Home</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-green-100 text-green-600">
              <FileText className="w-6 h-6" />
            </div>
          </div>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.4 }}
          className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
        >
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600 mb-1">Pending Approvals</p>
              <p className="text-2xl font-bold text-gray-900">{pendingApprovals}</p>
              <div className="flex items-center mt-2">
                <Clock className="w-4 h-4 text-yellow-500 mr-1" />
                <span className="text-sm text-yellow-600 font-medium">Needs Review</span>
              </div>
            </div>
            <div className="p-3 rounded-lg bg-yellow-100 text-yellow-600">
              <Clock className="w-6 h-6" />
            </div>
          </div>
        </motion.div>
      </div>

      {/* Tab Navigation */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-200">
        <div className="border-b border-gray-200">
          <nav className="flex space-x-8 px-6">
            <button
              onClick={() => setActiveTab('payroll')}
              className={`py-4 px-1 border-b-2 font-medium text-sm transition-colors ${
                activeTab === 'payroll'
                  ? 'border-emerald-500 text-emerald-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              Payroll Entries
            </button>
            <button
              onClick={() => setActiveTab('employees')}
              className={`py-4 px-1 border-b-2 font-medium text-sm transition-colors ${
                activeTab === 'employees'
                  ? 'border-emerald-500 text-emerald-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              Employee Management
            </button>
          </nav>
        </div>

        {/* Filters and Search */}
        <div className="p-6 border-b border-gray-200">
          <div className="flex flex-col lg:flex-row lg:items-center space-y-4 lg:space-y-0 lg:space-x-4">
            {/* Pay Period Selector (only for payroll tab) */}
            {activeTab === 'payroll' && (
              <div className="flex items-center space-x-2">
                <Calendar className="w-5 h-5 text-gray-400" />
                <label className="text-sm font-medium text-gray-700">Pay Period:</label>
                <select
                  value={selectedPeriod}
                  onChange={(e) => setSelectedPeriod(e.target.value)}
                  className="border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
                >
                  <option value="2024-12">December 2024</option>
                  <option value="2024-11">November 2024</option>
                  <option value="2024-10">October 2024</option>
                </select>
              </div>
            )}

            {/* Search */}
            <div className="flex-1 relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
              <input
                type="text"
                placeholder={`Search ${activeTab === 'payroll' ? 'payroll entries' : 'employees'}...`}
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
              />
            </div>

            {/* Department Filter */}
            <div className="flex items-center space-x-2">
              <Filter className="w-5 h-5 text-gray-400" />
              <select
                value={filterDepartment}
                onChange={(e) => setFilterDepartment(e.target.value)}
                className="border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors"
              >
                <option value="all">All Departments</option>
                <option value="operations">Operations</option>
                <option value="credit">Credit</option>
                <option value="finance">Finance</option>
                <option value="customer service">Customer Service</option>
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
                {activeTab === 'payroll' ? (
                  <>
                    <option value="paid">Paid</option>
                    <option value="approved">Approved</option>
                    <option value="calculated">Calculated</option>
                    <option value="draft">Draft</option>
                  </>
                ) : (
                  <>
                    <option value="active">Active</option>
                    <option value="inactive">Inactive</option>
                    <option value="terminated">Terminated</option>
                  </>
                )}
              </select>
            </div>
          </div>
        </div>

        {/* Content */}
        <div className="overflow-x-auto">
          {activeTab === 'payroll' ? (
            <table className="w-full">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Employee
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Department
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Basic Salary
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Gross Earnings
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Deductions
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Net Pay
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
                {filteredPayrollEntries.map((entry, index) => (
                  <motion.tr
                    key={entry.id}
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: index * 0.05 }}
                    className="hover:bg-gray-50 transition-colors"
                  >
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div>
                        <div className="text-sm font-medium text-gray-900">
                          {entry.employeeName}
                        </div>
                        <div className="text-sm text-gray-500">
                          {entry.employeeNumber} • {entry.position}
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {entry.department}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      ₦{entry.basicSalary.toLocaleString()}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-gray-900">
                        ₦{entry.grossEarnings.toLocaleString()}
                      </div>
                      <div className="text-sm text-gray-500">
                        Allowances: ₦{entry.allowances.toLocaleString()}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      ₦{entry.deductions.toLocaleString()}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      ₦{entry.netPay.toLocaleString()}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`inline-flex items-center px-2 py-1 text-xs font-semibold rounded-full ${
                        getStatusColor(entry.status)
                      }`}>
                        {getStatusIcon(entry.status)}
                        <span className="ml-1">{entry.status}</span>
                      </span>
                      {entry.status === 'Paid' && (
                        <div className="text-xs text-gray-500 mt-1">
                          {format(new Date(entry.payDate), 'MMM dd, yyyy')}
                        </div>
                      )}
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
            <table className="w-full">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Employee
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Department
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Position
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Basic Salary
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Gross Salary
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Hire Date
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
                {filteredEmployees.map((employee, index) => (
                  <motion.tr
                    key={employee.id}
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: index * 0.05 }}
                    className="hover:bg-gray-50 transition-colors"
                  >
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center">
                        <div className="w-10 h-10 bg-emerald-600 rounded-full flex items-center justify-center">
                          <span className="text-white font-medium text-sm">
                            {employee.fullName.split(' ').map(n => n[0]).join('')}
                          </span>
                        </div>
                        <div className="ml-4">
                          <div className="text-sm font-medium text-gray-900">
                            {employee.fullName}
                          </div>
                          <div className="text-sm text-gray-500">
                            {employee.employeeNumber}
                          </div>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {employee.department}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {employee.position}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      ₦{employee.basicSalary.toLocaleString()}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      ₦{employee.grossSalary.toLocaleString()}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {format(new Date(employee.hireDate), 'MMM dd, yyyy')}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`inline-flex items-center px-2 py-1 text-xs font-semibold rounded-full ${
                        getStatusColor(employee.status)
                      }`}>
                        {getStatusIcon(employee.status)}
                        <span className="ml-1">{employee.status}</span>
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
          )}
        </div>

        {((activeTab === 'payroll' && filteredPayrollEntries.length === 0) || 
          (activeTab === 'employees' && filteredEmployees.length === 0)) && (
          <div className="px-6 py-12 text-center">
            <Briefcase className="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">
              No {activeTab === 'payroll' ? 'payroll entries' : 'employees'} found
            </h3>
            <p className="text-gray-500">
              {searchTerm || filterDepartment !== 'all' || filterStatus !== 'all'
                ? 'Try adjusting your search or filter criteria.'
                : `Get started by ${activeTab === 'payroll' ? 'processing payroll' : 'adding employees'}.`
              }
            </p>
          </div>
        )}
      </div>

      {/* Quick Actions Panel */}
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.5 }}
        className="bg-white rounded-xl shadow-sm border border-gray-200 p-6"
      >
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Payroll Management Actions</h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <Plus className="w-6 h-6 text-emerald-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Add Employee</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <Calculator className="w-6 h-6 text-blue-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Process Payroll</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <FileText className="w-6 h-6 text-green-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Tax Reports</span>
          </button>
          <button className="p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors text-center">
            <Download className="w-6 h-6 text-purple-600 mx-auto mb-2" />
            <span className="text-sm font-medium text-gray-900">Export Payslips</span>
          </button>
        </div>
      </motion.div>
    </div>
  );
};

export default PayrollManagementPage;