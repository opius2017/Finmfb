import React, { useState } from 'react';
import { Dialog } from '@headlessui/react';
import { X } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Input } from '../../../design-system/components/Input';
import { loanService } from '../services/loanService';
import toast from 'react-hot-toast';

interface DisbursementModalProps {
    isOpen: boolean;
    onClose: () => void;
    loanId: string;
    maxAmount: number;
    onSuccess: () => void;
}

export const DisbursementModal: React.FC<DisbursementModalProps> = ({
    isOpen,
    onClose,
    loanId,
    maxAmount,
    onSuccess,
}) => {
    const [amount, setAmount] = useState(maxAmount.toString());
    const [date, setDate] = useState(new Date().toISOString().split('T')[0]);
    const [paymentMethod, setPaymentMethod] = useState('BankTransfer');
    const [reference, setReference] = useState('');
    const [notes, setNotes] = useState('');
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);

        try {
            await loanService.disburseLoan(loanId, {
                amount: parseFloat(amount),
                date: new Date(date),
                paymentMethod,
                transactionReference: reference,
                notes,
            });
            toast.success('Loan disbursed successfully');
            onSuccess();
            onClose();
        } catch (error) {
            console.error(error);
            toast.error('Failed to disburse loan');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Dialog open={isOpen} onClose={onClose} className="relative z-50">
            <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
            <div className="fixed inset-0 flex items-center justify-center p-4">
                <Dialog.Panel className="mx-auto max-w-sm rounded-xl bg-white p-6 shadow-xl w-full">
                    <div className="flex justify-between items-center mb-4">
                        <Dialog.Title className="text-lg font-bold">Disburse Loan</Dialog.Title>
                        <button onClick={onClose} className="text-neutral-500 hover:text-neutral-700">
                            <X size={20} />
                        </button>
                    </div>

                    <form onSubmit={handleSubmit} className="space-y-4">
                        <div>
                            <label className="block text-sm font-medium mb-1">Amount</label>
                            <Input
                                type="number"
                                value={amount}
                                onChange={(e) => setAmount(e.target.value)}
                                max={maxAmount}
                                required
                            />
                        </div>

                        <div>
                            <label className="block text-sm font-medium mb-1">Date</label>
                            <Input
                                type="date"
                                value={date}
                                onChange={(e) => setDate(e.target.value)}
                                required
                            />
                        </div>

                        <div>
                            <label className="block text-sm font-medium mb-1">Payment Method</label>
                            <select
                                className="w-full px-3 py-2 border border-neutral-300 rounded-lg"
                                value={paymentMethod}
                                onChange={(e) => setPaymentMethod(e.target.value)}
                            >
                                <option value="Cash">Cash</option>
                                <option value="BankTransfer">Bank Transfer</option>
                                <option value="Cheque">Cheque</option>
                            </select>
                        </div>

                        <div>
                            <label className="block text-sm font-medium mb-1">Reference</label>
                            <Input
                                value={reference}
                                onChange={(e) => setReference(e.target.value)}
                                placeholder="Transaction ID / Cheque No."
                                required
                            />
                        </div>

                        <div>
                            <label className="block text-sm font-medium mb-1">Notes</label>
                            <textarea
                                className="w-full px-3 py-2 border border-neutral-300 rounded-lg"
                                rows={2}
                                value={notes}
                                onChange={(e) => setNotes(e.target.value)}
                            />
                        </div>

                        <div className="flex justify-end space-x-2 pt-2">
                            <Button variant="outline" onClick={onClose} type="button">
                                Cancel
                            </Button>
                            <Button variant="primary" type="submit" disabled={loading}>
                                {loading ? 'Processing...' : 'Confirm Disbursement'}
                            </Button>
                        </div>
                    </form>
                </Dialog.Panel>
            </div>
        </Dialog>
    );
};
