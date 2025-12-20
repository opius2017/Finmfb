import React, { useEffect, useState } from 'react';
import { Card } from '../../../design-system/components/Card';
import { Button } from '../../../design-system/components/Button';
import { loanService } from '../services/loanService';
import { LoanDelinquency } from '../types/loan.types';
import { AlertCircle, Bell, Clock } from 'lucide-react';
import toast from 'react-hot-toast';

export const DelinquencyList: React.FC = () => {
    const [loans, setLoans] = useState<LoanDelinquency[]>([]);
    const [loading, setLoading] = useState(true);

    const fetchDelinquentLoans = async () => {
        try {
            const data = await loanService.getDelinquentLoans();
            setLoans(data);
        } catch (error) {
            console.error(error);
            toast.error('Failed to load delinquent loans');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchDelinquentLoans();
    }, []);

    const handleNotify = async (loanId: string) => {
        try {
            await loanService.sendDelinquencyNotification(loanId);
            toast.success('Notification sent to guarantors');
        } catch (error) {
            toast.error('Failed to send notification');
        }
    };

    if (loading) return <div className="p-6">Loading delinquency report...</div>;

    return (
        <div className="p-6">
            <h1 className="text-2xl font-bold mb-6 flex items-center text-error-700">
                <AlertCircle className="mr-2" />
                Delinquency Report (NPLs)
            </h1>

            <Card className="overflow-hidden">
                <table className="w-full text-left border-collapse">
                    <thead className="bg-neutral-50 text-neutral-600 font-semibold text-sm">
                        <tr>
                            <th className="p-4 border-b">Member</th>
                            <th className="p-4 border-b">Loan Number</th>
                            <th className="p-4 border-b">Days Overdue</th>
                            <th className="p-4 border-b text-right">Overdue Amount</th>
                            <th className="p-4 border-b text-right">Penalty</th>
                            <th className="p-4 border-b">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {loans.length === 0 ? (
                            <tr>
                                <td colSpan={6} className="p-8 text-center text-neutral-500">
                                    No delinquent loans found. Great job!
                                </td>
                            </tr>
                        ) : (
                            loans.map((loan) => (
                                <tr key={loan.loanId} className="border-b hover:bg-neutral-50">
                                    <td className="p-4 font-medium">{loan.memberName}</td>
                                    <td className="p-4 text-sm text-neutral-600">{loan.loanNumber}</td>
                                    <td className="p-4">
                                        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${loan.daysOverdue > 90 ? 'bg-error-100 text-error-800' :
                                                loan.daysOverdue > 30 ? 'bg-warning-100 text-warning-800' :
                                                    'bg-yellow-100 text-yellow-800'
                                            }`}>
                                            <Clock className="w-3 h-3 mr-1" />
                                            {loan.daysOverdue} days
                                        </span>
                                    </td>
                                    <td className="p-4 text-right font-medium text-error-600">
                                        {loanService.formatCurrency(loan.overdueAmount)}
                                    </td>
                                    <td className="p-4 text-right text-neutral-600">
                                        {loanService.formatCurrency(loan.penaltyAmount)}
                                    </td>
                                    <td className="p-4">
                                        <Button
                                            size="sm"
                                            variant="outline"
                                            onClick={() => handleNotify(loan.loanId)}
                                            disabled={loan.guarantorsNotified}
                                        >
                                            <Bell className="w-4 h-4 mr-1" />
                                            {loan.guarantorsNotified ? 'Notified' : 'Notify Guarantors'}
                                        </Button>
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </Card>
        </div>
    );
};
