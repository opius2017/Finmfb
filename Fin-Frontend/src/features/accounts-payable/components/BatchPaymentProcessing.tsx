import React, { useState, useEffect } from 'react';
import { Plus, Download, Check, X, Calendar, DollarSign, Filter, FileText } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Card } from '../../../design-system/components/Card';
import { Input } from '../../../design-system/components/Input';
import { PaymentBatch, BatchPayment, PaymentFilter } from '../types/payment.types';
import { paymentService } from '../services/paymentService';

type ViewMode = 'list' | 'create' | 'review';

export const BatchPaymentProcessing: React.FC = () => {
  const [viewMode, setViewMode] = useState<ViewMode>('list');
  const [batches, setBatches] = useState<PaymentBatch[]>([]);
  const [eligiblePayments, setEligiblePayments] = useState<BatchPayment[]>([]);
  const [selectedPayments, setSelectedPayments] = useState<Set<string>>(new Set());
  const [currentBatch, setCurrentBatch] = useState<PaymentBatch | null>(null);
  const [filters, setFilters] = useState<PaymentFilter>({});
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (viewMode === 'list') {
      loadBatches();
    }
  }, [viewMode]);

  const loadBatches = async () => {
    setLoading(true);
    try {
      const result = await paymentService.getBatches();
      setBatches(result.batches);
    } catch (error) {
      console.error('Failed to load batches:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadEligiblePayments = async () => {
    setLoading(true);
    try {
      const payments = await paymentService.getEligibleInvoices(filters);
      setEligiblePayments(payments);
    } catch (error) {
      console.error('Failed to load eligible payments:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateBatch = () => {
    setViewMode('create');
    loadEligiblePayments();
  };

  const handleSelectPayment = (paymentId: string) => {
    const newSelection = new Set(selectedPayments);
    if (newSelection.has(paymentId)) {
      newSelection.delete(paymentId);
    } else {
      newSelection.add(paymentId);
    }
    setSelectedPayments(newSelection);
  };

  const handleSelectAll = () => {
    if (selectedPayments.size === eligiblePayments.length) {
      setSelectedPayments(new Set());
    } else {
      setSelectedPayments(new Set(eligiblePayments.map(p => p.id)));
    }
  };

  const handleCreateBatchFromSelection = async () => {
    if (selectedPayments.size === 0) return;

    try {
      const selectedPaymentsList = eligiblePayments.filter(p => 
        selectedPayments.has(p.id)
      );

      const totalAmount = selectedPaymentsList.reduce((sum, p) => 
        sum + (p.discountedAmount || p.amount), 0
      );

      const batch = await paymentService.createBatch({
        batchDate: new Date(),
        scheduledDate: new Date(),
        status: 'draft',
        paymentMethod: 'bank-transfer',
        totalAmount,
        paymentCount: selectedPayments.size,
        payments: selectedPaymentsList,
      });

      setCurrentBatch(batch);
      setViewMode('review');
    } catch (error) {
      console.error('Failed to create batch:', error);
    }
  };

  const handleGenerateFile = async (format: string) => {
    if (!currentBatch) return;

    try {
      const file = await paymentService.generatePaymentFile(currentBatch.id, format);
      
      // Download the file
      const blob = new Blob([file.content], { type: 'text/plain' });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = file.fileName;
      a.click();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Failed to generate payment file:', error);
    }
  };

  const handleSubmitForApproval = async () => {
    if (!currentBatch) return;

    try {
      await paymentService.submitForApproval(currentBatch.id);
      setViewMode('list');
      setCurrentBatch(null);
      setSelectedPayments(new Set());
    } catch (error) {
      console.error('Failed to submit for approval:', error);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'completed':
        return 'bg-success-100 text-success-800';
      case 'approved':
        return 'bg-primary-100 text-primary-800';
      case 'pending-approval':
        return 'bg-warning-100 text-warning-800';
      case 'failed':
        return 'bg-error-100 text-error-800';
      default:
        return 'bg-neutral-100 text-neutral-800';
    }
  };

  if (viewMode === 'create') {
    return (
      <div className="p-6">
        <div className="mb-6">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setViewMode('list')}
          >
            ← Back to Batches
          </Button>
        </div>

        <Card className="p-6">
          <h2 className="text-xl font-bold mb-6">Create Payment Batch</h2>

          {/* Filters */}
          <div className="mb-6 p-4 bg-neutral-50 rounded-lg">
            <h3 className="font-semibold mb-4">Filter Payments</h3>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <label className="block text-sm font-medium text-neutral-700 mb-1">
                  Due Date From
                </label>
                <Input
                  type="date"
                  value={filters.dueDateFrom?.toISOString().split('T')[0] || ''}
                  onChange={(e) => setFilters({
                    ...filters,
                    dueDateFrom: new Date(e.target.value)
                  })}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-neutral-700 mb-1">
                  Due Date To
                </label>
                <Input
                  type="date"
                  value={filters.dueDateTo?.toISOString().split('T')[0] || ''}
                  onChange={(e) => setFilters({
                    ...filters,
                    dueDateTo: new Date(e.target.value)
                  })}
                />
              </div>
              <div className="flex items-end">
                <Button
                  variant="primary"
                  onClick={loadEligiblePayments}
                  fullWidth
                >
                  <Filter className="w-4 h-4 mr-2" />
                  Apply Filters
                </Button>
              </div>
            </div>
          </div>

          {/* Selection Summary */}
          <div className="mb-4 p-4 bg-primary-50 border border-primary-200 rounded-lg">
            <div className="flex items-center justify-between">
              <div>
                <span className="font-semibold">
                  {selectedPayments.size} payment(s) selected
                </span>
                <span className="text-sm text-neutral-600 ml-4">
                  Total: ₦{eligiblePayments
                    .filter(p => selectedPayments.has(p.id))
                    .reduce((sum, p) => sum + (p.discountedAmount || p.amount), 0)
                    .toLocaleString()}
                </span>
              </div>
              <Button
                variant="primary"
                onClick={handleCreateBatchFromSelection}
                disabled={selectedPayments.size === 0}
              >
                Create Batch
              </Button>
            </div>
          </div>

          {/* Eligible Payments Table */}
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-neutral-200">
                  <th className="text-left py-3 px-4">
                    <input
                      type="checkbox"
                      checked={selectedPayments.size === eligiblePayments.length}
                      onChange={handleSelectAll}
                      className="rounded"
                    />
                  </th>
                  <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                    Vendor
                  </th>
                  <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                    Invoice #
                  </th>
                  <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                    Due Date
                  </th>
                  <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                    Amount
                  </th>
                  <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                    Discount
                  </th>
                  <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                    Net Amount
                  </th>
                </tr>
              </thead>
              <tbody>
                {eligiblePayments.map((payment) => (
                  <tr
                    key={payment.id}
                    className={`border-b border-neutral-100 hover:bg-neutral-50 ${
                      selectedPayments.has(payment.id) ? 'bg-primary-50' : ''
                    }`}
                  >
                    <td className="py-3 px-4">
                      <input
                        type="checkbox"
                        checked={selectedPayments.has(payment.id)}
                        onChange={() => handleSelectPayment(payment.id)}
                        className="rounded"
                      />
                    </td>
                    <td className="py-3 px-4">{payment.vendorName}</td>
                    <td className="py-3 px-4">{payment.invoiceNumber}</td>
                    <td className="py-3 px-4">
                      {new Date(payment.dueDate).toLocaleDateString()}
                    </td>
                    <td className="py-3 px-4 text-right">
                      ₦{payment.amount.toLocaleString()}
                    </td>
                    <td className="py-3 px-4 text-right text-success-600">
                      {payment.earlyPaymentDiscount
                        ? `₦${payment.earlyPaymentDiscount.toLocaleString()}`
                        : '-'}
                    </td>
                    <td className="py-3 px-4 text-right font-semibold">
                      ₦{(payment.discountedAmount || payment.amount).toLocaleString()}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      </div>
    );
  }

  if (viewMode === 'review' && currentBatch) {
    return (
      <div className="p-6">
        <div className="mb-6">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setViewMode('create')}
          >
            ← Back to Selection
          </Button>
        </div>

        <Card className="p-6">
          <h2 className="text-xl font-bold mb-6">Review Payment Batch</h2>

          {/* Batch Summary */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
            <div className="p-4 bg-neutral-50 rounded-lg">
              <div className="text-sm text-neutral-600 mb-1">Batch Number</div>
              <div className="font-semibold">{currentBatch.batchNumber}</div>
            </div>
            <div className="p-4 bg-neutral-50 rounded-lg">
              <div className="text-sm text-neutral-600 mb-1">Payment Count</div>
              <div className="font-semibold">{currentBatch.paymentCount}</div>
            </div>
            <div className="p-4 bg-neutral-50 rounded-lg">
              <div className="text-sm text-neutral-600 mb-1">Total Amount</div>
              <div className="font-semibold text-lg">
                ₦{currentBatch.totalAmount.toLocaleString()}
              </div>
            </div>
            <div className="p-4 bg-neutral-50 rounded-lg">
              <div className="text-sm text-neutral-600 mb-1">Scheduled Date</div>
              <div className="font-semibold">
                {new Date(currentBatch.scheduledDate).toLocaleDateString()}
              </div>
            </div>
          </div>

          {/* Payment File Generation */}
          <div className="mb-6 p-4 bg-primary-50 border border-primary-200 rounded-lg">
            <h3 className="font-semibold mb-3">Generate Payment File</h3>
            <div className="flex space-x-3">
              <Button
                variant="outline"
                size="sm"
                onClick={() => handleGenerateFile('NACHA')}
              >
                <Download className="w-4 h-4 mr-2" />
                NACHA Format
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => handleGenerateFile('SEPA')}
              >
                <Download className="w-4 h-4 mr-2" />
                SEPA Format
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => handleGenerateFile('CSV')}
              >
                <Download className="w-4 h-4 mr-2" />
                CSV Format
              </Button>
            </div>
          </div>

          {/* Payments List */}
          <div className="mb-6">
            <h3 className="font-semibold mb-3">Payments in Batch</h3>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-neutral-200">
                    <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                      Vendor
                    </th>
                    <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                      Invoice #
                    </th>
                    <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                      Bank Account
                    </th>
                    <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                      Amount
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {currentBatch.payments.map((payment) => (
                    <tr key={payment.id} className="border-b border-neutral-100">
                      <td className="py-3 px-4">{payment.vendorName}</td>
                      <td className="py-3 px-4">{payment.invoiceNumber}</td>
                      <td className="py-3 px-4">
                        {payment.vendorBankAccount.accountNumber} - {payment.vendorBankAccount.bankName}
                      </td>
                      <td className="py-3 px-4 text-right font-semibold">
                        ₦{payment.amount.toLocaleString()}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* Actions */}
          <div className="flex justify-end space-x-3">
            <Button variant="outline" onClick={() => setViewMode('list')}>
              Cancel
            </Button>
            <Button variant="primary" onClick={handleSubmitForApproval}>
              <Check className="w-4 h-4 mr-2" />
              Submit for Approval
            </Button>
          </div>
        </Card>
      </div>
    );
  }

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold">Payment Batches</h1>
          <p className="text-neutral-600 mt-1">
            Create and manage batch payments to vendors
          </p>
        </div>
        <Button variant="primary" onClick={handleCreateBatch}>
          <Plus className="w-4 h-4 mr-2" />
          Create Batch
        </Button>
      </div>

      <Card className="p-6">
        {batches.length === 0 ? (
          <div className="text-center py-12">
            <FileText className="w-16 h-16 text-neutral-300 mx-auto mb-4" />
            <h3 className="text-lg font-semibold text-neutral-700 mb-2">
              No Payment Batches
            </h3>
            <p className="text-neutral-600 mb-6">
              Create your first payment batch to process vendor payments
            </p>
            <Button variant="primary" onClick={handleCreateBatch}>
              <Plus className="w-4 h-4 mr-2" />
              Create First Batch
            </Button>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-neutral-200">
                  <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                    Batch #
                  </th>
                  <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                    Date
                  </th>
                  <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                    Scheduled
                  </th>
                  <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                    Payments
                  </th>
                  <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                    Amount
                  </th>
                  <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                    Status
                  </th>
                  <th className="text-right py-3 px-4 font-semibold text-neutral-700">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody>
                {batches.map((batch) => (
                  <tr key={batch.id} className="border-b border-neutral-100 hover:bg-neutral-50">
                    <td className="py-3 px-4 font-medium">{batch.batchNumber}</td>
                    <td className="py-3 px-4">
                      {new Date(batch.batchDate).toLocaleDateString()}
                    </td>
                    <td className="py-3 px-4">
                      {new Date(batch.scheduledDate).toLocaleDateString()}
                    </td>
                    <td className="py-3 px-4 text-right">{batch.paymentCount}</td>
                    <td className="py-3 px-4 text-right font-semibold">
                      ₦{batch.totalAmount.toLocaleString()}
                    </td>
                    <td className="py-3 px-4">
                      <span className={`px-2 py-1 text-xs font-semibold rounded-full ${getStatusColor(batch.status)}`}>
                        {batch.status}
                      </span>
                    </td>
                    <td className="py-3 px-4 text-right">
                      <Button variant="ghost" size="sm">
                        View
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>
    </div>
  );
};
