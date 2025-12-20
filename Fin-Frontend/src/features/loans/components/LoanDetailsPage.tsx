import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import {
    ArrowLeft,
    User,
    Calendar,
    DollarSign,
    FileText,
    Shield,
    Clock,
    Download,
    Printer,
    MoreVertical,
    CheckCircle,
    AlertTriangle,
    XCircle,
    Briefcase,
    MapPin,
    Phone,
    Mail
} from 'lucide-react';
import { Loan, LoanStatus } from '../types/loan.types';

// Mock Data for Detail View
const mockLoanDetails: any = {
    id: '1',
    accountNumber: 'LN001234567',
    customerName: 'Adebayo Ogundimu',
    customerImage: null, // Placeholder string or null
    productName: 'SME Business Loan',
    principalAmount: 5000000,
    outstandingPrincipal: 3500000,
    interestRate: 18.5,
    status: 'active',
    disbursementDate: '2024-01-15',
    maturityDate: '2025-01-15',
    monthlyEMI: 450000,
    tenor: 12, // months
    purpose: 'Expansion of retail business in Lagos Island market',
    guarantors: [
        { id: '1', name: 'Funke Akindele', relationship: 'Business Partner', phone: '08012345678', status: 'Approved' },
        { id: '2', name: 'John Okafor', relationship: 'Brother', phone: '08098765432', status: 'Approved' }
    ],
    schedule: Array.from({ length: 12 }).map((_, i) => ({
        installment: i + 1,
        dueDate: new Date(2024, i, 15).toISOString(),
        amount: 450000,
        status: i < 3 ? 'paid' : i === 3 ? 'overdue' : 'pending'
    }))
};

const LoanDetailsPage = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const [activeTab, setActiveTab] = useState<'overview' | 'schedule' | 'guarantors' | 'documents'>('overview');

    // Helper to get status badge
    const getStatusBadge = (status: LoanStatus) => {
        switch (status) {
            case 'active': return <span className="bg-green-100 text-green-800 px-3 py-1 rounded-full text-sm font-medium flex items-center"><CheckCircle className="w-4 h-4 mr-1" /> Active</span>;
            case 'delinquent': return <span className="bg-red-100 text-red-800 px-3 py-1 rounded-full text-sm font-medium flex items-center"><AlertTriangle className="w-4 h-4 mr-1" /> Delinquent</span>;
            default: return <span className="bg-gray-100 text-gray-800 px-3 py-1 rounded-full text-sm font-medium">{status}</span>;
        }
    };

    return (
        <div className="space-y-6">
            {/* Header / Navigation */}
            <button onClick={() => navigate('/loans')} className="flex items-center text-gray-500 hover:text-gray-900 transition-colors">
                <ArrowLeft className="w-5 h-5 mr-1" /> Back to Portfolio
            </button>

            {/* Main Profile Card */}
            <div className="bg-white rounded-2xl shadow-sm border border-gray-200 overflow-hidden">
                {/* Cover / Top Section */}
                <div className="h-32 bg-gradient-to-r from-emerald-600 to-teal-600 relative">
                    <div className="absolute top-4 right-4 flex space-x-2">
                        <button className="bg-white/20 hover:bg-white/30 text-white p-2 rounded-lg backdrop-blur-sm">
                            <Printer className="w-5 h-5" />
                        </button>
                        <button className="bg-white/20 hover:bg-white/30 text-white p-2 rounded-lg backdrop-blur-sm">
                            <MoreVertical className="w-5 h-5" />
                        </button>
                    </div>
                </div>

                <div className="px-8 pb-8">
                    <div className="relative flex justify-between items-end -mt-12 mb-6">
                        <div className="flex items-end">
                            <div className="w-24 h-24 rounded-2xl bg-white p-1 shadow-lg mr-4">
                                <div className="w-full h-full bg-gray-200 rounded-xl flex items-center justify-center text-2xl font-bold text-gray-500">
                                    {mockLoanDetails.customerName.charAt(0)}
                                </div>
                                {/* <img src={...} className="w-full h-full object-cover rounded-xl" /> */}
                            </div>
                            <div className="mb-1">
                                <h1 className="text-2xl font-bold text-gray-900">{mockLoanDetails.customerName}</h1>
                                <div className="flex items-center text-gray-500 text-sm mt-1">
                                    <Briefcase className="w-4 h-4 mr-1" /> {mockLoanDetails.accountNumber}
                                    <span className="mx-2">•</span>
                                    <MapPin className="w-4 h-4 mr-1" /> Lagos, Nigeria
                                </div>
                            </div>
                        </div>
                        <div className="mb-1">
                            {getStatusBadge(mockLoanDetails.status)}
                        </div>
                    </div>

                    {/* Metrics Grid */}
                    <div className="grid grid-cols-1 md:grid-cols-4 gap-4 border-t border-gray-100 pt-6">
                        <div>
                            <p className="text-sm text-gray-500 mb-1">Principal Amount</p>
                            <p className="text-xl font-bold text-gray-900">₦{mockLoanDetails.principalAmount.toLocaleString()}</p>
                        </div>
                        <div>
                            <p className="text-sm text-gray-500 mb-1">Outstanding Balance</p>
                            <p className="text-xl font-bold text-emerald-600">₦{mockLoanDetails.outstandingPrincipal.toLocaleString()}</p>
                        </div>
                        <div>
                            <p className="text-sm text-gray-500 mb-1">Next Payment</p>
                            <p className="text-xl font-bold text-gray-900 flex items-center">
                                ₦{mockLoanDetails.monthlyEMI.toLocaleString()}
                                <span className="text-xs font-normal text-red-500 ml-2 bg-red-50 px-1 rounded">Due in 5 days</span>
                            </p>
                        </div>
                        <div>
                            <p className="text-sm text-gray-500 mb-1">Product</p>
                            <span className="inline-flex items-center px-2.5 py-0.5 rounded-md text-sm font-medium bg-indigo-100 text-indigo-800">
                                {mockLoanDetails.productName}
                            </span>
                        </div>
                    </div>
                </div>

                {/* Tabs */}
                <div className="px-8 border-t border-gray-200">
                    <div className="flex space-x-8">
                        {['overview', 'schedule', 'guarantors', 'documents'].map((tab) => (
                            <button
                                key={tab}
                                onClick={() => setActiveTab(tab as any)}
                                className={`py-4 text-sm font-medium border-b-2 transition-colors capitalize ${activeTab === tab
                                    ? 'border-emerald-500 text-emerald-600'
                                    : 'border-transparent text-gray-500 hover:text-gray-700'
                                    }`}
                            >
                                {tab}
                            </button>
                        ))}
                    </div>
                </div>
            </div>

            {/* Tab Content */}
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                {/* Left Column (Main Content) */}
                <div className="lg:col-span-2 space-y-6">
                    {activeTab === 'overview' && (
                        <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="space-y-6">
                            {/* Loan Information */}
                            <div className="bg-white p-6 rounded-xl border border-gray-200 shadow-sm">
                                <h3 className="font-bold text-gray-900 mb-4">Loan Information</h3>
                                <div className="grid grid-cols-2 gap-y-4 gap-x-8">
                                    <div>
                                        <span className="text-sm text-gray-500 block">Date Disbursed</span>
                                        <span className="text-gray-900 font-medium">{new Date(mockLoanDetails.disbursementDate).toLocaleDateString()}</span>
                                    </div>
                                    <div>
                                        <span className="text-sm text-gray-500 block">Maturity Date</span>
                                        <span className="text-gray-900 font-medium">{new Date(mockLoanDetails.maturityDate).toLocaleDateString()}</span>
                                    </div>
                                    <div>
                                        <span className="text-sm text-gray-500 block">Interest Rate</span>
                                        <span className="text-gray-900 font-medium">{mockLoanDetails.interestRate}% p.a.</span>
                                    </div>
                                    <div>
                                        <span className="text-sm text-gray-500 block">Tenor</span>
                                        <span className="text-gray-900 font-medium">{mockLoanDetails.tenor} Months</span>
                                    </div>
                                    <div className="col-span-2">
                                        <span className="text-sm text-gray-500 block">Purpose</span>
                                        <span className="text-gray-900">{mockLoanDetails.purpose}</span>
                                    </div>
                                </div>
                            </div>

                            {/* Recent Activity / Transactions mock */}
                            <div className="bg-white p-6 rounded-xl border border-gray-200 shadow-sm">
                                <h3 className="font-bold text-gray-900 mb-4">Recent Transactions</h3>
                                <div className="space-y-4">
                                    {[1, 2, 3].map((i) => (
                                        <div key={i} className="flex justify-between items-center py-2 border-b border-gray-50 last:border-0">
                                            <div className="flex items-center">
                                                <div className="w-8 h-8 rounded-full bg-emerald-100 flex items-center justify-center text-emerald-600 mr-3">
                                                    <ArrowLeft className="w-4 h-4 rotate-45" />
                                                </div>
                                                <div>
                                                    <p className="text-sm font-medium text-gray-900">Repayment Received</p>
                                                    <p className="text-xs text-gray-500">Oct {15 + i}, 2024</p>
                                                </div>
                                            </div>
                                            <span className="font-bold text-emerald-600">+₦450,000</span>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        </motion.div>
                    )}

                    {activeTab === 'schedule' && (
                        <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
                            <table className="w-full text-sm text-left">
                                <thead className="bg-gray-50 text-gray-500 font-medium">
                                    <tr>
                                        <th className="px-6 py-3">#</th>
                                        <th className="px-6 py-3">Due Date</th>
                                        <th className="px-6 py-3">Amount</th>
                                        <th className="px-6 py-3">Status</th>
                                    </tr>
                                </thead>
                                <tbody className="divide-y divide-gray-100">
                                    {mockLoanDetails.schedule.map((item: any) => (
                                        <tr key={item.installment} className="hover:bg-gray-50">
                                            <td className="px-6 py-3 font-medium">{item.installment}</td>
                                            <td className="px-6 py-3">{new Date(item.dueDate).toLocaleDateString()}</td>
                                            <td className="px-6 py-3">₦{item.amount.toLocaleString()}</td>
                                            <td className="px-6 py-3">
                                                <span className={`px-2 py-1 rounded-full text-xs font-medium capitalize ${item.status === 'paid' ? 'bg-green-100 text-green-700' :
                                                    item.status === 'overdue' ? 'bg-red-100 text-red-700' :
                                                        'bg-gray-100 text-gray-600'
                                                    }`}>
                                                    {item.status}
                                                </span>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}

                    {activeTab === 'guarantors' && (
                        <div className="space-y-4">
                            {mockLoanDetails.guarantors.map((g: any) => (
                                <div key={g.id} className="bg-white p-4 rounded-xl border border-gray-200 shadow-sm flex items-center justify-between">
                                    <div className="flex items-center space-x-4">
                                        <div className="w-10 h-10 rounded-full bg-blue-100 flex items-center justify-center text-blue-600 font-bold">
                                            {g.name[0]}
                                        </div>
                                        <div>
                                            <p className="font-bold text-gray-900">{g.name}</p>
                                            <div className="flex text-xs text-gray-500 space-x-3">
                                                <span>{g.relationship}</span>
                                                <span>{g.phone}</span>
                                            </div>
                                        </div>
                                    </div>
                                    <span className="px-3 py-1 bg-green-50 text-green-700 rounded-lg text-sm font-medium">{g.status}</span>
                                </div>
                            ))}
                        </div>
                    )}
                </div>

                {/* Right Column (Contact & Actions) */}
                <div className="space-y-6">
                    <div className="bg-white p-6 rounded-xl border border-gray-200 shadow-sm">
                        <h3 className="font-bold text-gray-900 mb-4">Contact Info</h3>
                        <div className="space-y-3 text-sm">
                            <div className="flex items-center text-gray-600">
                                <Phone className="w-4 h-4 mr-3" /> 08012345678
                            </div>
                            <div className="flex items-center text-gray-600">
                                <Mail className="w-4 h-4 mr-3" /> adebayo@example.com
                            </div>
                            <div className="flex items-center text-gray-600">
                                <MapPin className="w-4 h-4 mr-3" /> 123 Broad Street, Lagos
                            </div>
                        </div>
                    </div>

                    <button className="w-full py-3 bg-indigo-600 text-white rounded-xl shadow-lg hover:bg-indigo-700 font-medium">
                        Make Repayment
                    </button>
                    <button className="w-full py-3 bg-white border border-gray-200 text-gray-700 rounded-xl hover:bg-gray-50 font-medium">
                        Refinance Loan
                    </button>
                </div>
            </div>
        </div>
    );
};

export default LoanDetailsPage;
