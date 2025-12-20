import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import {
    MoreHorizontal,
    Clock,
    CheckCircle,
    XCircle,
    AlertCircle,
    Filter,
    Search,
    User,
    Calendar,
    DollarSign
} from 'lucide-react';
import { LoanApplication } from '../types/loan.types';
import { loanService } from '../services/loanService';

// Kanban Column Component
const KanbanColumn = ({ title, count, status, children, color }: any) => (
    <div className="flex flex-col h-full min-w-[320px] bg-gray-50/50 rounded-2xl p-4 border border-gray-100">
        <div className={`flex items-center justify-between mb-4 pb-3 border-b border-${color}-200`}>
            <div className="flex items-center space-x-2">
                <div className={`w-3 h-3 rounded-full bg-${color}-500 shadow-sm shadow-${color}-200`}></div>
                <h3 className="font-bold text-gray-700">{title}</h3>
            </div>
            <span className={`bg-${color}-100 text-${color}-700 text-xs font-bold px-2.5 py-1 rounded-full`}>
                {count}
            </span>
        </div>
        <div className="flex-1 overflow-y-auto space-y-3 pr-2 scrollbar-hide">
            {children}
        </div>
    </div>
);

// Application Card Component
const ApplicationCard = ({ application, onClick }: { application: LoanApplication; onClick: () => void }) => (
    <motion.div
        layoutId={application.id}
        onClick={onClick}
        whileHover={{ y: -3, boxShadow: "0 10px 30px -10px rgba(0,0,0,0.1)" }}
        className="bg-white p-4 rounded-xl border border-gray-100 shadow-sm cursor-pointer group hover:border-indigo-200 transition-all"
    >
        <div className="flex justify-between items-start mb-3">
            <div className="flex items-center space-x-3">
                <div className="w-10 h-10 rounded-full bg-gradient-to-br from-indigo-500 to-purple-600 flex items-center justify-center text-white font-bold text-sm shadow-md">
                    {application.memberName.split(' ').map((n: string) => n[0]).join('').substring(0, 2)}
                </div>
                <div>
                    <h4 className="font-bold text-gray-900 line-clamp-1">{application.memberName}</h4>
                    <span className="text-xs text-gray-500 flex items-center">
                        <Calendar className="w-3 h-3 mr-1" />
                        {new Date(application.submittedAt).toLocaleDateString()}
                    </span>
                </div>
            </div>
            <button className="text-gray-400 hover:text-indigo-600 transition-colors">
                <MoreHorizontal className="w-5 h-5" />
            </button>
        </div>

        <div className="space-y-2 mb-3">
            <div className="flex items-center justify-between text-sm">
                <span className="text-gray-500">Amount</span>
                <span className="font-bold text-gray-900 flex items-center">
                    <DollarSign className="w-3 h-3 text-emerald-500 mr-0.5" />
                    {application.requestedAmount.toLocaleString()}
                </span>
            </div>
            <div className="flex items-center justify-between text-sm">
                <span className="text-gray-500">Product</span>
                <span className="font-medium text-indigo-600 bg-indigo-50 px-2 py-0.5 rounded-md">
                    {application.loanType}
                </span>
            </div>
        </div>

        <div className="flex items-center justify-between pt-3 border-t border-gray-50">
            <div className="flex -space-x-2">
                {/* Placeholder for approvers avatars */}
                <div className="w-6 h-6 rounded-full bg-gray-200 border-2 border-white"></div>
                <div className="w-6 h-6 rounded-full bg-gray-300 border-2 border-white"></div>
            </div>
            <div className="flex items-center text-xs font-medium text-amber-600 bg-amber-50 px-2 py-1 rounded-full">
                <Clock className="w-3 h-3 mr-1" />
                {application.status}
            </div>
        </div>
    </motion.div>
);

const ApplicationKanban = () => {
    const [applications, setApplications] = useState<LoanApplication[]>([]);
    const [loading, setLoading] = useState(true);

    // Mock data for initial view if API is empty (to demonstrate UI)
    const mockData: any[] = [
        { id: '1', memberName: 'John Doe', requestedAmount: 500000, status: 'submitted', loanType: 'normal', submittedAt: new Date().toISOString() },
        { id: '2', memberName: 'Jane Smith', requestedAmount: 1200000, status: 'under_review', loanType: 'car', submittedAt: new Date().toISOString() },
        { id: '3', memberName: 'Robert Johnson', requestedAmount: 350000, status: 'approved', loanType: 'commodity', submittedAt: new Date().toISOString() },
        { id: '4', memberName: 'Sarah Connor', requestedAmount: 750000, status: 'rejected', loanType: 'normal', submittedAt: new Date().toISOString() },
    ];

    useEffect(() => {
        const fetchApps = async () => {
            try {
                // TODO: Replace with actual API call
                // const data = await loanService.getApplications();
                // setApplications(data);
                setApplications(mockData); // Using mock for now until service is confirmed
            } catch (error) {
                console.error("Failed to fetch applications", error);
            } finally {
                setLoading(false);
            }
        };
        fetchApps();
    }, []);

    const getAppsByStatus = (status: string) => applications.filter(app => app.status === status);

    return (
        <div className="h-[calc(100vh-100px)] flex flex-col space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">Application Pipeline</h1>
                    <p className="text-gray-500">Manage loan requests across stages</p>
                </div>
                <div className="flex space-x-3">
                    <div className="relative">
                        <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
                        <input
                            type="text"
                            placeholder="Search applicants..."
                            className="pl-10 pr-4 py-2 border border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500 focus:border-transparent outline-none"
                        />
                    </div>
                    <button className="flex items-center px-4 py-2 bg-white border border-gray-200 rounded-xl hover:bg-gray-50">
                        <Filter className="w-4 h-4 mr-2" /> Filter
                    </button>
                </div>
            </div>

            <div className="flex-1 overflow-x-auto pb-4">
                <div className="flex space-x-6 h-full min-w-max px-2">
                    <KanbanColumn title="New Applications" count={getAppsByStatus('submitted').length} status="submitted" color="amber">
                        {getAppsByStatus('submitted').map(app => (
                            <ApplicationCard key={app.id} application={app} onClick={() => { }} />
                        ))}
                    </KanbanColumn>

                    <KanbanColumn title="In Review" count={getAppsByStatus('under_review').length} status="under_review" color="blue">
                        {getAppsByStatus('under_review').map(app => (
                            <ApplicationCard key={app.id} application={app} onClick={() => { }} />
                        ))}
                    </KanbanColumn>

                    <KanbanColumn title="Approved" count={getAppsByStatus('approved').length} status="approved" color="emerald">
                        {getAppsByStatus('approved').map(app => (
                            <ApplicationCard key={app.id} application={app} onClick={() => { }} />
                        ))}
                    </KanbanColumn>

                    <KanbanColumn title="Rejected" count={getAppsByStatus('rejected').length} status="rejected" color="rose">
                        {getAppsByStatus('rejected').map(app => (
                            <ApplicationCard key={app.id} application={app} onClick={() => { }} />
                        ))}
                    </KanbanColumn>
                </div>
            </div>
        </div>
    );
};

export default ApplicationKanban;
