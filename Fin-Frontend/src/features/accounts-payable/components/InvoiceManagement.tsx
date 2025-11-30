import React, { useState } from 'react';
import { Plus, FileText, Filter, Download } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Card } from '../../../design-system/components/Card';
import { InvoiceCapture } from './InvoiceCapture';
import { InvoiceValidation } from './InvoiceValidation';
import { VendorInvoice } from '../types/invoice.types';

type ViewMode = 'list' | 'capture' | 'validate' | 'edit';

export const InvoiceManagement: React.FC = () => {
  const [viewMode, setViewMode] = useState<ViewMode>('list');
  const [capturedInvoice, setCapturedInvoice] = useState<Partial<VendorInvoice> | null>(null);
  const [invoices, setInvoices] = useState<VendorInvoice[]>([]);

  const handleInvoiceCaptured = (invoice: Partial<VendorInvoice>) => {
    setCapturedInvoice(invoice);
    setViewMode('validate');
  };

  const handleInvoiceValidated = (invoice: Partial<VendorInvoice>) => {
    // In real implementation, save to backend
    console.log('Invoice validated:', invoice);
    setViewMode('list');
    setCapturedInvoice(null);
  };

  const handleCancel = () => {
    setViewMode('list');
    setCapturedInvoice(null);
  };

  if (viewMode === 'capture') {
    return (
      <div className="p-6">
        <InvoiceCapture
          onInvoiceCaptured={handleInvoiceCaptured}
          onCancel={handleCancel}
        />
      </div>
    );
  }

  if (viewMode === 'validate' && capturedInvoice) {
    return (
      <div className="p-6">
        <InvoiceValidation
          invoice={capturedInvoice}
          onValidated={handleInvoiceValidated}
          onCancel={handleCancel}
        />
      </div>
    );
  }

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold">Vendor Invoices</h1>
          <p className="text-neutral-600 mt-1">
            Manage and process vendor invoices with automated OCR
          </p>
        </div>
        <Button
          variant="primary"
          onClick={() => setViewMode('capture')}
        >
          <Plus className="w-4 h-4 mr-2" />
          Capture Invoice
        </Button>
      </div>

      <Card className="p-6">
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center space-x-3">
            <Button variant="outline" size="sm">
              <Filter className="w-4 h-4 mr-2" />
              Filter
            </Button>
            <Button variant="outline" size="sm">
              <Download className="w-4 h-4 mr-2" />
              Export
            </Button>
          </div>
        </div>

        {invoices.length === 0 ? (
          <div className="text-center py-12">
            <FileText className="w-16 h-16 text-neutral-300 mx-auto mb-4" />
            <h3 className="text-lg font-semibold text-neutral-700 mb-2">
              No Invoices Yet
            </h3>
            <p className="text-neutral-600 mb-6">
              Start by capturing your first vendor invoice
            </p>
            <Button
              variant="primary"
              onClick={() => setViewMode('capture')}
            >
              <Plus className="w-4 h-4 mr-2" />
              Capture First Invoice
            </Button>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-neutral-200">
                  <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                    Invoice #
                  </th>
                  <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                    Vendor
                  </th>
                  <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                    Date
                  </th>
                  <th className="text-left py-3 px-4 font-semibold text-neutral-700">
                    Due Date
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
                {invoices.map((invoice) => (
                  <tr key={invoice.id} className="border-b border-neutral-100 hover:bg-neutral-50">
                    <td className="py-3 px-4">{invoice.invoiceNumber}</td>
                    <td className="py-3 px-4">{invoice.vendorName}</td>
                    <td className="py-3 px-4">
                      {new Date(invoice.invoiceDate).toLocaleDateString()}
                    </td>
                    <td className="py-3 px-4">
                      {new Date(invoice.dueDate).toLocaleDateString()}
                    </td>
                    <td className="py-3 px-4 text-right font-semibold">
                      ₦{invoice.total.toLocaleString()}
                    </td>
                    <td className="py-3 px-4">
                      <span className="px-2 py-1 text-xs font-semibold rounded-full bg-warning-100 text-warning-800">
                        {invoice.status}
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
