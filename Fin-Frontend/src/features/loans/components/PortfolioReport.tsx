import React, { useEffect, useState } from 'react';
import { Card } from '../../../design-system/components/Card';
import { PortfolioReport } from '../types/loan.types';
import { loanReportService } from '../services/loanReportService';
import { PieChart, DollarSign, Users, AlertTriangle, Activity } from 'lucide-react';

export const PortfolioReportView: React.FC = () => {
    const [report, setReport] = useState<PortfolioReport | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchReport = async () => {
            try {
                const data = await loanReportService.getPortfolioReport();
                setReport(data);
            } catch (err: any) {
                setError(err.message || 'Failed to load report');
            } finally {
                setLoading(false);
            }
        };

        fetchReport();
    }, []);

    if (loading) return <div className="p-6">Loading report...</div>;
    if (error) return <div className="p-6 text-error-600">Error: {error}</div>;
    if (!report) return null;

    return (
        <div className="p-6 space-y-6">
            <h1 className="text-2xl font-bold flex items-center">
                <PieChart className="mr-2" /> Loan Portfolio Report
            </h1>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                <Card className="p-4 bg-primary-50 border-primary-100">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="text-sm text-primary-600 font-medium">Total Value</p>
                            <h3 className="text-2xl font-bold text-primary-900">
                                ₦{report.totalPortfolioValue.toLocaleString()}
                            </h3>
                        </div>
                        <DollarSign className="w-8 h-8 text-primary-400" />
                    </div>
                </Card>

                <Card className="p-4 bg-success-50 border-success-100">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="text-sm text-success-600 font-medium">Active Loans</p>
                            <h3 className="text-2xl font-bold text-success-900">
                                {report.totalActiveLoans}
                            </h3>
                        </div>
                        <Users className="w-8 h-8 text-success-400" />
                    </div>
                </Card>

                <Card className="p-4 bg-warning-50 border-warning-100">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="text-sm text-warning-600 font-medium">Portfolio at Risk</p>
                            <h3 className="text-2xl font-bold text-warning-900">
                                {report.portfolioAtRisk}%
                            </h3>
                        </div>
                        <AlertTriangle className="w-8 h-8 text-warning-400" />
                    </div>
                </Card>

                <Card className="p-4 bg-purple-50 border-purple-100">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="text-sm text-purple-600 font-medium">PAR &gt; 30 Days</p>
                            <h3 className="text-2xl font-bold text-purple-900">
                                {report.par30}%
                            </h3>
                        </div>
                        <Activity className="w-8 h-8 text-purple-400" />
                    </div>
                </Card>
            </div>

            <Card className="p-6">
                <h3 className="text-lg font-semibold mb-4">Risk Analysis</h3>
                <div className="space-y-4">
                    <div>
                        <div className="flex justify-between text-sm mb-1">
                            <span>PAR 30 Days</span>
                            <span className="font-medium">{report.par30}%</span>
                        </div>
                        <div className="w-full bg-neutral-100 rounded-full h-2">
                            <div className="bg-warning-500 h-2 rounded-full" style={{ width: `${Math.min(report.par30, 100)}%` }}></div>
                        </div>
                    </div>
                    <div>
                        <div className="flex justify-between text-sm mb-1">
                            <span>PAR 60 Days</span>
                            <span className="font-medium">{report.par60}%</span>
                        </div>
                        <div className="w-full bg-neutral-100 rounded-full h-2">
                            <div className="bg-orange-500 h-2 rounded-full" style={{ width: `${Math.min(report.par60, 100)}%` }}></div>
                        </div>
                    </div>
                    <div>
                        <div className="flex justify-between text-sm mb-1">
                            <span>PAR 90+ Days (Default)</span>
                            <span className="font-medium">{report.par90}%</span>
                        </div>
                        <div className="w-full bg-neutral-100 rounded-full h-2">
                            <div className="bg-error-500 h-2 rounded-full" style={{ width: `${Math.min(report.par90, 100)}%` }}></div>
                        </div>
                    </div>
                </div>
            </Card>
        </div>
    );
};
