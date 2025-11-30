import React, { useState, useCallback } from 'react';
import { clsx } from 'clsx';
import { Upload, FileText, AlertCircle, CheckCircle, X } from 'lucide-react';
import { Button, Card, toastService } from '@/design-system';
import { importService } from '../services/importService';
import type { ImportResult, StatementFormat } from '../types/reconciliation.types';

export interface StatementUploadProps {
  accountId: string;
  accountName: string;
  accountNumber: string;
  onImportComplete: (result: ImportResult) => void;
}

export const StatementUpload: React.FC<StatementUploadProps> = ({
  accountId,
  accountName,
  accountNumber,
  onImportComplete,
}) => {
  const [isDragging, setIsDragging] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [importResult, setImportResult] = useState<ImportResult | null>(null);

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  }, []);

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
  }, []);

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);

    const files = Array.from(e.dataTransfer.files);
    if (files.length > 0) {
      handleFileSelect(files[0]);
    }
  }, []);

  const handleFileSelect = (file: File) => {
    // Validate file size (max 10MB)
    if (file.size > 10 * 1024 * 1024) {
      toastService.error('File size must be less than 10MB');
      return;
    }

    setSelectedFile(file);
    setImportResult(null);
  };

  const handleFileInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (files && files.length > 0) {
      handleFileSelect(files[0]);
    }
  };

  const handleImport = async () => {
    if (!selectedFile) return;

    setIsUploading(true);
    try {
      const result = await importService.importStatement(
        selectedFile,
        accountId,
        accountName,
        accountNumber
      );

      setImportResult(result);

      if (result.success) {
        toastService.success(`Successfully imported ${result.transactionCount} transactions`);
        onImportComplete(result);
      } else {
        toastService.error('Import failed. Please check the errors below.');
      }
    } catch (error) {
      toastService.error('Failed to import statement');
      console.error('Import error:', error);
    } finally {
      setIsUploading(false);
    }
  };

  const handleClear = () => {
    setSelectedFile(null);
    setImportResult(null);
  };

  const supportedFormats = importService.getSupportedFormats();

  return (
    <div className="space-y-6">
      {/* Upload Area */}
      {!selectedFile && (
        <div
          onDragOver={handleDragOver}
          onDragLeave={handleDragLeave}
          onDrop={handleDrop}
          className={clsx(
            'border-2 border-dashed rounded-lg p-12 text-center transition-all',
            isDragging
              ? 'border-primary-500 bg-primary-50 dark:bg-primary-900/20'
              : 'border-neutral-300 dark:border-neutral-700 hover:border-primary-400 dark:hover:border-primary-600'
          )}
        >
          <div className="flex flex-col items-center gap-4">
            <div className={clsx(
              'w-16 h-16 rounded-full flex items-center justify-center',
              'bg-neutral-100 dark:bg-neutral-800'
            )}>
              <Upload className="h-8 w-8 text-neutral-600 dark:text-neutral-400" />
            </div>

            <div>
              <h3 className="text-lg font-semibold text-neutral-900 dark:text-neutral-100 mb-2">
                Upload Bank Statement
              </h3>
              <p className="text-sm text-neutral-600 dark:text-neutral-400 mb-4">
                Drag and drop your file here, or click to browse
              </p>

              <label>
                <input
                  type="file"
                  className="hidden"
                  accept=".csv,.xlsx,.xls,.pdf,.ofx,.mt940,.sta"
                  onChange={handleFileInputChange}
                />
                <Button variant="primary" as="span">
                  Select File
                </Button>
              </label>
            </div>

            <div className="text-xs text-neutral-500 dark:text-neutral-400">
              <p className="mb-2">Supported formats:</p>
              <div className="flex flex-wrap gap-2 justify-center">
                {supportedFormats.map(format => (
                  <span
                    key={format}
                    className="px-2 py-1 rounded bg-neutral-100 dark:bg-neutral-800"
                  >
                    {format}
                  </span>
                ))}
              </div>
              <p className="mt-2">Maximum file size: 10MB</p>
            </div>
          </div>
        </div>
      )}

      {/* Selected File */}
      {selectedFile && !importResult && (
        <Card>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-lg bg-primary-100 dark:bg-primary-900/20 flex items-center justify-center">
                <FileText className="h-5 w-5 text-primary-600 dark:text-primary-400" />
              </div>
              <div>
                <p className="text-sm font-medium text-neutral-900 dark:text-neutral-100">
                  {selectedFile.name}
                </p>
                <p className="text-xs text-neutral-500 dark:text-neutral-400">
                  {(selectedFile.size / 1024).toFixed(2)} KB
                </p>
              </div>
            </div>

            <div className="flex items-center gap-2">
              <Button
                variant="primary"
                onClick={handleImport}
                loading={isUploading}
                disabled={isUploading}
              >
                {isUploading ? 'Importing...' : 'Import'}
              </Button>
              <Button
                variant="ghost"
                onClick={handleClear}
                disabled={isUploading}
              >
                <X className="h-4 w-4" />
              </Button>
            </div>
          </div>
        </Card>
      )}

      {/* Import Result */}
      {importResult && (
        <Card>
          <div className="space-y-4">
            {/* Summary */}
            <div className="flex items-start gap-3">
              {importResult.success ? (
                <CheckCircle className="h-6 w-6 text-success-600 dark:text-success-400 flex-shrink-0" />
              ) : (
                <AlertCircle className="h-6 w-6 text-error-600 dark:text-error-400 flex-shrink-0" />
              )}
              <div className="flex-1">
                <h3 className={clsx(
                  'text-lg font-semibold mb-2',
                  importResult.success
                    ? 'text-success-700 dark:text-success-400'
                    : 'text-error-700 dark:text-error-400'
                )}>
                  {importResult.success ? 'Import Successful' : 'Import Failed'}
                </h3>
                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div>
                    <span className="text-neutral-600 dark:text-neutral-400">Transactions:</span>
                    <span className="ml-2 font-medium text-neutral-900 dark:text-neutral-100">
                      {importResult.transactionCount}
                    </span>
                  </div>
                  {importResult.duplicateCount > 0 && (
                    <div>
                      <span className="text-neutral-600 dark:text-neutral-400">Duplicates removed:</span>
                      <span className="ml-2 font-medium text-neutral-900 dark:text-neutral-100">
                        {importResult.duplicateCount}
                      </span>
                    </div>
                  )}
                </div>
              </div>
            </div>

            {/* Errors */}
            {importResult.errors.length > 0 && (
              <div className="border-t border-neutral-200 dark:border-neutral-700 pt-4">
                <h4 className="text-sm font-semibold text-error-700 dark:text-error-400 mb-2">
                  Errors ({importResult.errors.length})
                </h4>
                <div className="space-y-1 max-h-40 overflow-y-auto">
                  {importResult.errors.map((error, index) => (
                    <div
                      key={index}
                      className="text-xs text-error-600 dark:text-error-400 flex items-start gap-2"
                    >
                      <AlertCircle className="h-3 w-3 flex-shrink-0 mt-0.5" />
                      <span>
                        {error.row && `Row ${error.row}: `}
                        {error.field && `${error.field} - `}
                        {error.message}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Warnings */}
            {importResult.warnings.length > 0 && (
              <div className="border-t border-neutral-200 dark:border-neutral-700 pt-4">
                <h4 className="text-sm font-semibold text-warning-700 dark:text-warning-400 mb-2">
                  Warnings ({importResult.warnings.length})
                </h4>
                <div className="space-y-1 max-h-40 overflow-y-auto">
                  {importResult.warnings.map((warning, index) => (
                    <div
                      key={index}
                      className="text-xs text-warning-600 dark:text-warning-400 flex items-start gap-2"
                    >
                      <AlertCircle className="h-3 w-3 flex-shrink-0 mt-0.5" />
                      <span>
                        {warning.row && `Row ${warning.row}: `}
                        {warning.message}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Actions */}
            <div className="flex justify-end gap-2 pt-4 border-t border-neutral-200 dark:border-neutral-700">
              <Button variant="outline" onClick={handleClear}>
                Upload Another
              </Button>
              {importResult.success && (
                <Button
                  variant="primary"
                  onClick={() => onImportComplete(importResult)}
                >
                  Continue to Reconciliation
                </Button>
              )}
            </div>
          </div>
        </Card>
      )}
    </div>
  );
};
