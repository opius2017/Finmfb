import React, { useState, useCallback } from 'react';
import { Upload, Camera, Mail, FileText, AlertCircle, CheckCircle, Loader } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Card } from '../../../design-system/components/Card';
import { ocrService } from '../services/ocrService';
import { invoiceService } from '../services/invoiceService';
import { VendorInvoice, OCRData } from '../types/invoice.types';

interface InvoiceCaptureProps {
  onInvoiceCaptured: (invoice: Partial<VendorInvoice>) => void;
  onCancel: () => void;
}

export const InvoiceCapture: React.FC<InvoiceCaptureProps> = ({
  onInvoiceCaptured,
  onCancel,
}) => {
  const [captureMethod, setCaptureMethod] = useState<'upload' | 'camera' | 'email' | null>(null);
  const [file, setFile] = useState<File | null>(null);
  const [processing, setProcessing] = useState(false);
  const [ocrData, setOcrData] = useState<OCRData | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [dragActive, setDragActive] = useState(false);

  const handleDrag = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === 'dragenter' || e.type === 'dragover') {
      setDragActive(true);
    } else if (e.type === 'dragleave') {
      setDragActive(false);
    }
  }, []);

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);

    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      handleFileSelect(e.dataTransfer.files[0]);
    }
  }, []);

  const handleFileSelect = async (selectedFile: File) => {
    // Validate file type
    const validTypes = ['image/jpeg', 'image/png', 'image/jpg', 'application/pdf'];
    if (!validTypes.includes(selectedFile.type)) {
      setError('Please upload a valid image (JPEG, PNG) or PDF file');
      return;
    }

    // Validate file size (max 10MB)
    if (selectedFile.size > 10 * 1024 * 1024) {
      setError('File size must be less than 10MB');
      return;
    }

    setFile(selectedFile);
    setError(null);
    await processFile(selectedFile);
  };

  const processFile = async (fileToProcess: File) => {
    setProcessing(true);
    setError(null);

    try {
      // Extract data using OCR
      const extractedData = await ocrService.extractInvoiceData(fileToProcess);
      setOcrData(extractedData);

      // Parse invoice fields
      const parsedFields = ocrService.parseInvoiceFields(extractedData);

      // Check confidence
      if (!ocrService.isConfidenceAcceptable(extractedData, 0.7)) {
        setError('OCR confidence is low. Please review extracted data carefully.');
      }

      // Match vendor if vendor name was extracted
      let vendorId: string | undefined;
      if (parsedFields.vendorName) {
        const vendorMatches = await invoiceService.matchVendor(parsedFields.vendorName);
        if (vendorMatches.length > 0 && vendorMatches[0].confidence > 0.8) {
          vendorId = vendorMatches[0].id;
        }
      }

      // Check for duplicates if we have enough info
      if (vendorId && parsedFields.invoiceNumber) {
        const duplicateCheck = await invoiceService.checkDuplicate(
          vendorId,
          parsedFields.invoiceNumber,
          parsedFields.total
        );

        if (duplicateCheck.isDuplicate) {
          setError(
            `Possible duplicate invoice found. ${duplicateCheck.duplicates.length} similar invoice(s) exist.`
          );
        }
      }

      // Create partial invoice object
      const partialInvoice: Partial<VendorInvoice> = {
        vendorId,
        vendorName: parsedFields.vendorName,
        invoiceNumber: parsedFields.invoiceNumber,
        invoiceDate: parsedFields.invoiceDate,
        dueDate: parsedFields.dueDate,
        total: parsedFields.total,
        subtotal: parsedFields.subtotal,
        taxAmount: parsedFields.taxAmount,
        captureMethod: captureMethod || 'upload',
        ocrData: extractedData,
        status: 'pending-validation',
      };

      onInvoiceCaptured(partialInvoice);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to process invoice');
    } finally {
      setProcessing(false);
    }
  };

  const handleCameraCapture = () => {
    // In a real implementation, this would open the device camera
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/*';
    input.capture = 'environment';
    input.onchange = (e) => {
      const target = e.target as HTMLInputElement;
      if (target.files && target.files[0]) {
        setCaptureMethod('camera');
        handleFileSelect(target.files[0]);
      }
    };
    input.click();
  };

  const handleEmailForwarding = () => {
    // Show email forwarding instructions
    alert(
      'Forward invoices to: invoices@yourcompany.com\n\n' +
      'Include your vendor code in the subject line for automatic processing.'
    );
  };

  if (processing) {
    return (
      <Card className="p-8">
        <div className="flex flex-col items-center justify-center space-y-4">
          <Loader className="w-12 h-12 animate-spin text-primary-600" />
          <h3 className="text-lg font-semibold">Processing Invoice...</h3>
          <p className="text-sm text-neutral-600">
            Extracting data using OCR. This may take a few moments.
          </p>
        </div>
      </Card>
    );
  }

  if (!captureMethod) {
    return (
      <Card className="p-6">
        <h2 className="text-xl font-bold mb-6">Capture Invoice</h2>
        
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <button
            onClick={() => setCaptureMethod('upload')}
            className="flex flex-col items-center justify-center p-6 border-2 border-dashed border-neutral-300 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <Upload className="w-12 h-12 text-primary-600 mb-3" />
            <h3 className="font-semibold mb-1">Upload File</h3>
            <p className="text-sm text-neutral-600 text-center">
              Drag & drop or browse for invoice files
            </p>
          </button>

          <button
            onClick={handleCameraCapture}
            className="flex flex-col items-center justify-center p-6 border-2 border-dashed border-neutral-300 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <Camera className="w-12 h-12 text-primary-600 mb-3" />
            <h3 className="font-semibold mb-1">Camera Capture</h3>
            <p className="text-sm text-neutral-600 text-center">
              Take a photo of the invoice
            </p>
          </button>

          <button
            onClick={handleEmailForwarding}
            className="flex flex-col items-center justify-center p-6 border-2 border-dashed border-neutral-300 rounded-lg hover:border-primary-500 hover:bg-primary-50 transition-colors"
          >
            <Mail className="w-12 h-12 text-primary-600 mb-3" />
            <h3 className="font-semibold mb-1">Email Forwarding</h3>
            <p className="text-sm text-neutral-600 text-center">
              Forward invoices via email
            </p>
          </button>
        </div>

        <div className="mt-6 flex justify-end">
          <Button variant="outline" onClick={onCancel}>
            Cancel
          </Button>
        </div>
      </Card>
    );
  }

  if (captureMethod === 'upload') {
    return (
      <Card className="p-6">
        <h2 className="text-xl font-bold mb-6">Upload Invoice</h2>

        <div
          className={`border-2 border-dashed rounded-lg p-12 text-center transition-colors ${
            dragActive
              ? 'border-primary-500 bg-primary-50'
              : 'border-neutral-300 hover:border-primary-400'
          }`}
          onDragEnter={handleDrag}
          onDragLeave={handleDrag}
          onDragOver={handleDrag}
          onDrop={handleDrop}
        >
          <FileText className="w-16 h-16 text-neutral-400 mx-auto mb-4" />
          
          {!file ? (
            <>
              <p className="text-lg font-semibold mb-2">
                Drag and drop invoice file here
              </p>
              <p className="text-sm text-neutral-600 mb-4">
                or click to browse (PDF, JPEG, PNG - Max 10MB)
              </p>
              <input
                type="file"
                id="file-upload"
                className="hidden"
                accept="image/jpeg,image/png,image/jpg,application/pdf"
                onChange={(e) => {
                  if (e.target.files && e.target.files[0]) {
                    handleFileSelect(e.target.files[0]);
                  }
                }}
              />
              <label htmlFor="file-upload">
                <Button as="span" variant="primary">
                  Browse Files
                </Button>
              </label>
            </>
          ) : (
            <div className="flex items-center justify-center space-x-2">
              <CheckCircle className="w-6 h-6 text-success-600" />
              <span className="font-semibold">{file.name}</span>
            </div>
          )}
        </div>

        {error && (
          <div className="mt-4 p-4 bg-warning-50 border border-warning-200 rounded-lg flex items-start space-x-2">
            <AlertCircle className="w-5 h-5 text-warning-600 flex-shrink-0 mt-0.5" />
            <p className="text-sm text-warning-800">{error}</p>
          </div>
        )}

        {ocrData && (
          <div className="mt-4 p-4 bg-success-50 border border-success-200 rounded-lg">
            <div className="flex items-center space-x-2 mb-2">
              <CheckCircle className="w-5 h-5 text-success-600" />
              <span className="font-semibold text-success-800">
                Data Extracted Successfully
              </span>
            </div>
            <p className="text-sm text-success-700">
              Confidence: {(ocrData.confidence * 100).toFixed(1)}%
            </p>
            <p className="text-sm text-success-700">
              Processing time: {ocrData.processingTime}ms
            </p>
          </div>
        )}

        <div className="mt-6 flex justify-end space-x-3">
          <Button variant="outline" onClick={onCancel}>
            Cancel
          </Button>
          <Button
            variant="outline"
            onClick={() => {
              setFile(null);
              setOcrData(null);
              setError(null);
              setCaptureMethod(null);
            }}
          >
            Choose Different Method
          </Button>
        </div>
      </Card>
    );
  }

  return null;
};
