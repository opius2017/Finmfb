import React, { useState, useEffect } from 'react';
import { AlertCircle, CheckCircle, Edit2, Save, X } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Card } from '../../../design-system/components/Card';
import { Input } from '../../../design-system/components/Input';
import { VendorInvoice, OCRField } from '../types/invoice.types';
import { ocrService } from '../services/ocrService';

interface InvoiceValidationProps {
  invoice: Partial<VendorInvoice>;
  onValidated: (invoice: Partial<VendorInvoice>) => void;
  onCancel: () => void;
}

export const InvoiceValidation: React.FC<InvoiceValidationProps> = ({
  invoice,
  onValidated,
  onCancel,
}) => {
  const [editedInvoice, setEditedInvoice] = useState(invoice);
  const [lowConfidenceFields, setLowConfidenceFields] = useState<OCRField[]>([]);
  const [editingField, setEditingField] = useState<string | null>(null);

  useEffect(() => {
    if (invoice.ocrData) {
      const lowConf = ocrService.getLowConfidenceFields(invoice.ocrData, 0.7);
      setLowConfidenceFields(lowConf);
    }
  }, [invoice.ocrData]);

  const handleFieldEdit = (field: string, value: any) => {
    setEditedInvoice((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  const handleSave = () => {
    onValidated({
      ...editedInvoice,
      status: 'validated',
    });
  };

  const getConfidenceColor = (confidence: number) => {
    if (confidence >= 0.9) return 'text-success-600';
    if (confidence >= 0.7) return 'text-warning-600';
    return 'text-error-600';
  };

  const getConfidenceBadge = (confidence: number) => {
    if (confidence >= 0.9) return 'High';
    if (confidence >= 0.7) return 'Medium';
    return 'Low';
  };

  return (
    <Card className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-xl font-bold">Validate Invoice Data</h2>
        {invoice.ocrData && (
          <div className="flex items-center space-x-2">
            <span className="text-sm text-neutral-600">Overall Confidence:</span>
            <span className={`font-semibold ${getConfidenceColor(invoice.ocrData.confidence)}`}>
              {(invoice.ocrData.confidence * 100).toFixed(1)}%
            </span>
          </div>
        )}
      </div>

      {lowConfidenceFields.length > 0 && (
        <div className="mb-6 p-4 bg-warning-50 border border-warning-200 rounded-lg">
          <div className="flex items-start space-x-2">
            <AlertCircle className="w-5 h-5 text-warning-600 flex-shrink-0 mt-0.5" />
            <div>
              <p className="font-semibold text-warning-800">Review Required</p>
              <p className="text-sm text-warning-700">
                {lowConfidenceFields.length} field(s) have low confidence and need manual review
              </p>
            </div>
          </div>
        </div>
      )}

      <div className="space-y-6">
        {/* Vendor Information */}
        <div>
          <h3 className="text-lg font-semibold mb-4">Vendor Information</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1">
                Vendor Name *
              </label>
              {editingField === 'vendorName' ? (
                <div className="flex space-x-2">
                  <Input
                    value={editedInvoice.vendorName || ''}
                    onChange={(e) => handleFieldEdit('vendorName', e.target.value)}
                    autoFocus
                  />
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() => setEditingField(null)}
                  >
                    <Save className="w-4 h-4" />
                  </Button>
                </div>
              ) : (
                <div className="flex items-center justify-between p-2 border border-neutral-300 rounded-lg">
                  <span>{editedInvoice.vendorName || 'Not extracted'}</span>
                  <button
                    onClick={() => setEditingField('vendorName')}
                    className="text-primary-600 hover:text-primary-700"
                  >
                    <Edit2 className="w-4 h-4" />
                  </button>
                </div>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1">
                Vendor ID
              </label>
              <Input
                value={editedInvoice.vendorId || ''}
                onChange={(e) => handleFieldEdit('vendorId', e.target.value)}
                placeholder="Select or enter vendor ID"
              />
            </div>
          </div>
        </div>

        {/* Invoice Details */}
        <div>
          <h3 className="text-lg font-semibold mb-4">Invoice Details</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1">
                Invoice Number *
              </label>
              {editingField === 'invoiceNumber' ? (
                <div className="flex space-x-2">
                  <Input
                    value={editedInvoice.invoiceNumber || ''}
                    onChange={(e) => handleFieldEdit('invoiceNumber', e.target.value)}
                    autoFocus
                  />
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() => setEditingField(null)}
                  >
                    <Save className="w-4 h-4" />
                  </Button>
                </div>
              ) : (
                <div className="flex items-center justify-between p-2 border border-neutral-300 rounded-lg">
                  <span>{editedInvoice.invoiceNumber || 'Not extracted'}</span>
                  <button
                    onClick={() => setEditingField('invoiceNumber')}
                    className="text-primary-600 hover:text-primary-700"
                  >
                    <Edit2 className="w-4 h-4" />
                  </button>
                </div>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1">
                Invoice Date *
              </label>
              <Input
                type="date"
                value={
                  editedInvoice.invoiceDate
                    ? new Date(editedInvoice.invoiceDate).toISOString().split('T')[0]
                    : ''
                }
                onChange={(e) => handleFieldEdit('invoiceDate', new Date(e.target.value))}
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1">
                Due Date *
              </label>
              <Input
                type="date"
                value={
                  editedInvoice.dueDate
                    ? new Date(editedInvoice.dueDate).toISOString().split('T')[0]
                    : ''
                }
                onChange={(e) => handleFieldEdit('dueDate', new Date(e.target.value))}
              />
            </div>
          </div>
        </div>

        {/* Amounts */}
        <div>
          <h3 className="text-lg font-semibold mb-4">Amounts</h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1">
                Subtotal
              </label>
              {editingField === 'subtotal' ? (
                <div className="flex space-x-2">
                  <Input
                    type="number"
                    step="0.01"
                    value={editedInvoice.subtotal || ''}
                    onChange={(e) => handleFieldEdit('subtotal', parseFloat(e.target.value))}
                    autoFocus
                  />
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() => setEditingField(null)}
                  >
                    <Save className="w-4 h-4" />
                  </Button>
                </div>
              ) : (
                <div className="flex items-center justify-between p-2 border border-neutral-300 rounded-lg">
                  <span>
                    {editedInvoice.subtotal
                      ? `₦${editedInvoice.subtotal.toLocaleString()}`
                      : 'Not extracted'}
                  </span>
                  <button
                    onClick={() => setEditingField('subtotal')}
                    className="text-primary-600 hover:text-primary-700"
                  >
                    <Edit2 className="w-4 h-4" />
                  </button>
                </div>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1">
                Tax Amount
              </label>
              <Input
                type="number"
                step="0.01"
                value={editedInvoice.taxAmount || ''}
                onChange={(e) => handleFieldEdit('taxAmount', parseFloat(e.target.value))}
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-neutral-700 mb-1">
                Total Amount *
              </label>
              {editingField === 'total' ? (
                <div className="flex space-x-2">
                  <Input
                    type="number"
                    step="0.01"
                    value={editedInvoice.total || ''}
                    onChange={(e) => handleFieldEdit('total', parseFloat(e.target.value))}
                    autoFocus
                  />
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() => setEditingField(null)}
                  >
                    <Save className="w-4 h-4" />
                  </Button>
                </div>
              ) : (
                <div className="flex items-center justify-between p-2 border border-neutral-300 rounded-lg">
                  <span className="font-semibold">
                    {editedInvoice.total
                      ? `₦${editedInvoice.total.toLocaleString()}`
                      : 'Not extracted'}
                  </span>
                  <button
                    onClick={() => setEditingField('total')}
                    className="text-primary-600 hover:text-primary-700"
                  >
                    <Edit2 className="w-4 h-4" />
                  </button>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* OCR Confidence Details */}
        {invoice.ocrData && invoice.ocrData.fields.length > 0 && (
          <div>
            <h3 className="text-lg font-semibold mb-4">Extraction Confidence</h3>
            <div className="space-y-2">
              {invoice.ocrData.fields.slice(0, 5).map((field, index) => (
                <div
                  key={index}
                  className="flex items-center justify-between p-3 bg-neutral-50 rounded-lg"
                >
                  <div className="flex-1">
                    <span className="font-medium">{field.name}:</span>
                    <span className="ml-2 text-neutral-700">{field.value}</span>
                  </div>
                  <div className="flex items-center space-x-2">
                    <span className={`text-sm ${getConfidenceColor(field.confidence)}`}>
                      {getConfidenceBadge(field.confidence)}
                    </span>
                    <span className="text-sm text-neutral-500">
                      {(field.confidence * 100).toFixed(0)}%
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>

      <div className="mt-8 flex justify-end space-x-3">
        <Button variant="outline" onClick={onCancel}>
          <X className="w-4 h-4 mr-2" />
          Cancel
        </Button>
        <Button
          variant="primary"
          onClick={handleSave}
          disabled={
            !editedInvoice.vendorName ||
            !editedInvoice.invoiceNumber ||
            !editedInvoice.total
          }
        >
          <CheckCircle className="w-4 h-4 mr-2" />
          Validate & Continue
        </Button>
      </div>
    </Card>
  );
};
