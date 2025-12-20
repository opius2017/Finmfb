import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { DollarSign, Search, Filter, CheckCircle } from 'lucide-react';
import { DisbursementModal } from './DisbursementModal';

// Mock data for loans ready for disbursement
const mockReadyLoans = [
    { id: 'LN001', applicantName: 'Alice Johnson', amount: 1500000, approvedDate: '2023-10-25', purpose: 'Business Expansion' },
    { id: 'LN002', applicantName: 'Bob Williams', amount: 500000, approvedDate: '2023-10-26', purpose: 'Personal Needs' },
    { id: 'LN003', applicantName: 'Charlie Brown', amount: 2000000, approvedDate: '2023-10-24', purpose: 'Home Renovation' },
];

const DisbursementPage = () => {
    const [selectedLoan, setSelectedLoan] = useState<any>(null);
    const [isModalOpen, setIsModalOpen] = useState(false);

    const handleDisburseClick = (loan: any) => {
        setSelectedLoan(loan);
        setIsModalOpen(true);
    };

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">Loan Disbursement</h1>
                    <p className="text-gray-600">Review and release funds for approved loans</p>
                </div>
            </div>

            <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
                <div className="p-4 border-b border-gray-200 flex justify-between items-center bg-gray-50">
                    <div className="relative w-64">
                        <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
                        <input
                            type="text"
                            placeholder="Search loans..."
                            className="pl-10 pr-4 py-2 w-full border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-transparent"
                        />
                    </div>
                    <button className="flex items-center text-gray-600 hover:text-gray-900">
                        <Filter className="w-4 h-4 mr-2" />
                        Filter
                    </button>
                </div>

                <table className="w-full text-left">
                    <thead className="bg-gray-50 text-gray-500 font-medium text-sm">
                        <tr>
                            <th className="px-6 py-4">Loan ID</th>
                            <th className="px-6 py-4">Applicant</th>
                            <th className="px-6 py-4">Amount</th>
                            <th className="px-6 py-4">Approved Date</th>
                            <th className="px-6 py-4">Purpose</th>
                            <th className="px-6 py-4">Action</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-100">
                        {mockReadyLoans.map((loan) => (
                            <tr key={loan.id} className="hover:bg-gray-50 transition-colors">
                                <td className="px-6 py-4 font-medium text-gray-900">{loan.id}</td>
                                <td className="px-6 py-4">
                                    <div className="flex items-center">
                                        <div className="w-8 h-8 rounded-full bg-emerald-100 text-emerald-600 flex items-center justify-center font-bold text-xs mr-3">
                                            {loan.applicantName.charAt(0)}
                                        </div>
                                        {loan.applicantName}
                                    </div>
                                </td>
                                <td className="px-6 py-4 font-bold text-gray-700">₦{loan.amount.toLocaleString()}</td>
                                <td className="px-6 py-4 text-gray-500">{loan.approvedDate}</td>
                                <td className="px-6 py-4 text-gray-500">{loan.purpose}</td>
                                <td className="px-6 py-4">
                                    <button
                                        onClick={() => handleDisburseClick(loan)}
                                        className="flex items-center px-3 py-1.5 bg-emerald-600 text-white text-sm rounded-lg hover:bg-emerald-700 transition-colors shadow-sm"
                                    >
                                        <DollarSign className="w-3 h-3 mr-1" />
                                        Disburse
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            {isModalOpen && selectedLoan && (
                <DisbursementModal
                    isOpen={isModalOpen}
                    onClose={() => setIsModalOpen(false)}
                    loanId={selectedLoan.id}
                    maxAmount={selectedLoan.amount}
                    onSuccess={() => {
                        console.log("Disbursed!");
                        setIsModalOpen(false);
                    }}
                />
            )}
        </div>
    );
};

export default DisbursementPage;
