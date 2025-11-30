import React, { useState, useEffect } from 'react';
import { Upload, Download, CheckCircle, XCircle, AlertCircle, FileText } from 'lucide-react';
import { Card } from '../../../design-system/components/Card';
import { Button } from '../../../design-system/components/Button';
import { BulkImport as BulkImportType, ImportTemplate, ColumnMapping } from '../types/bulk.types';
import { bulkService } from '../services/bulkService';

export const BulkImport: React.FC = () => {
  const [imports, setImports] = useState<BulkImportType[]>([]);
  const [templates, setTemplates] = useState<ImportTemplate[]>([]);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [selectedEntity, setSelectedEntity] = useState<string>('customers');
  const [currentImport, setCurrentImport] = useState<BulkImportType | null>(null);
  const [step, setStep] = useState<'upload' | 'mapping' | 'validation' | 'import'>('upload');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [importsData, templatesData] = await Promise.all([
        bulkService.getImports(),
        bulkService.getTemplates(),
      ]);
      setImports(importsData);
      setTemplates(templatesData);
    } catch (error) {
      console.error('Failed to load data:', error);
    }
  };

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      if (!bulkService.validateFileType(file, ['.csv', '.xlsx', '.xls'])) {
        alert('Invalid file type. Please upload CSV or Excel files.');
        return;
      }
      if (!bulkService.validateFileSize(file, 10)) {
        alert('File size exceeds 10MB limit.');
        return;
      }
      setSelectedFile(file);
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) return;

    setLoading(true);
    try {
      const importData = await bulkService.uploadFile(selectedFile, selectedEntity);
      setCurrentImport(importData);
      setStep('mapping');
      await loadData();
    } catch (error) {
      console.error('Failed to upload file:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleDownloadTemplate = async (entityType: string, format: 'csv' | 'excel') => {
    try {
      const blob = await bulkService.downloadTemplate(entityType, format);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${entityType}-template.${format === 'excel' ? 'xlsx' : 'csv'}`;
      a.click();
    } catch (error) {
      console.error('Failed to download template:', error);
    }
  };

  const handleValidate = async () => {
    if (!currentImport) return;

    setLoading(true);
    try {
      const validated = await bulkService.validateImport(currentImport.id);
      setCurrentImport(validated);
      setStep('validation');
    } catch (error) {
      console.error('Failed to validate import:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleExecute = async () => {
    if (!currentImport) return;

    if (!confirm('Execute this import? This action cannot be undone.')) return;

    setLoading(true);
    try {
      const executed = await bulkService.executeImport(currentImport.id);
      setCurrentImport(executed);
      setStep('import');
      await loadData();
    } catch (error) {
      console.error('Failed to execute import:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleRollback = async (importId: string) => {
    if (!confirm('Rollback this import? All imported records will be deleted.')) return;

    try {
      await bulkService.rollbackImport(importId);
      await loadData();
      alert('Import rolled back successfully');
    } catch (error) {
      console.error('Failed to rollback import:', error);
    }
  };

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold">Bulk Import</h1>
          <p className="text-sm text-neutral-600 mt-1">
            Import data from CSV or Excel files
          </p>
        </div>
      </div>

      {/* Upload Section */}
      {step === 'upload' && (
        <Card className="p-6 mb-6">
          <h2 className="text-lg font-semibold mb-4">Upload File</h2>

          <div className="mb-4">
            <label className="block text-sm font-medium text-neutral-700 mb-2">
              Entity Type
            </label>
            <select
              value={selectedEntity}
              onChange={(e) => setSelectedEntity(e.target.value)}
              className="w-full px-3 py-2 border border-neutral-300 rounded-lg"
            >
              <option value="customers">Customers</option>
              <option value="vendors">Vendors</option>
              <option value="invoices">Invoices</option>
              <option value="payments">Payments</option>
              <option value="products">Products</option>
              <option value="transactions">Transactions</option>
            </select>
          </div>

          <div className="border-2 border-dashed border-neutral-300 rounded-lg p-8 text-center mb-4">
            <Upload className="w-12 h-12 mx-auto mb-3 text-neutral-400" />
            <p className="text-sm text-neutral-600 mb-2">
              Drag and drop your file here, or click to browse
            </p>
            <input
              type="file"
              accept=".csv,.xlsx,.xls"
              onChange={handleFileSelect}
              className="hidden"
              id="file-upload"
            />
            <label htmlFor="file-upload">
              <Button variant="outline" as="span">
                Choose File
              </Button>
            </label>
            {selectedFile && (
              <div className="mt-3 text-sm text-neutral-700">
                Selected: {selectedFile.name} ({bulkService.formatFileSize(selectedFile.size)})
              </div>
            )}
          </div>

          <div className="flex items-center justify-between">
            <div className="text-sm text-neutral-600">
              Need a template?{' '}
              <button
                onClick={() => handleDownloadTemplate(selectedEntity, 'excel')}
                className="text-primary-600 hover:underline"
              >
                Download Excel
              </button>
              {' or '}
              <button
                onClick={() => handleDownloadTemplate(selectedEntity, 'csv')}
                className="text-primary-600 hover:underline"
              >
                Download CSV
              </button>
            </div>
            <Button
              variant="primary"
              onClick={handleUpload}
              disabled={!selectedFile || loading}
            >
              Upload & Continue
            </Button>
          </div>
        </Card>
      )}

      {/* Validation Results */}
      {currentImport && currentImport.validationResults && (
        <Card className="p-6 mb-6">
          <h2 className="text-lg font-semibold mb-4">Validation Results</h2>

          <div className="grid grid-cols-3 gap-4 mb-4">
            <div className="p-4 bg-neutral-50 rounded-lg">
              <div className="text-sm text-neutral-600">Total Rows</div>
              <div className="text-2xl font-bold">
                {currentImport.validationResults.totalRows}
              </div>
            </div>
            <div className="p-4 bg-success-50 rounded-lg">
              <div className="text-sm text-success-600">Valid Rows</div>
              <div className="text-2xl font-bold text-success-700">
                {currentImport.validationResults.validRows}
              </div>
            </div>
            <div className="p-4 bg-error-50 rounded-lg">
              <div className="text-sm text-error-600">Invalid Rows</div>
              <div className="text-2xl font-bold text-error-700">
                {currentImport.validationResults.invalidRows}
              </div>
            </div>
          </div>

          {currentImport.validationResults.errors.length > 0 && (
            <div className="mb-4">
              <h3 className="font-semibold mb-2">Errors</h3>
              <div className="space-y-2 max-h-64 overflow-y-auto">
                {currentImport.validationResults.errors.slice(0, 10).map((error, index) => (
                  <div key={index} className="p-2 bg-error-50 border border-error-200 rounded text-sm">
                    <span className="font-medium">Row {error.row}:</span> {error.error}
                  </div>
                ))}
              </div>
            </div>
          )}

          <div className="flex justify-end space-x-3">
            <Button variant="outline" onClick={() => setStep('upload')}>
              Cancel
            </Button>
            {currentImport.validationResults.isValid && (
              <Button variant="primary" onClick={handleExecute} disabled={loading}>
                Import Data
              </Button>
            )}
          </div>
        </Card>
      )}

      {/* Import History */}
      <Card className="p-6">
        <h2 className="text-lg font-semibold mb-4">Import History</h2>

        <div className="space-y-3">
          {imports.map((imp) => (
            <div
              key={imp.id}
              className="flex items-center justify-between p-3 bg-neutral-50 rounded-lg"
            >
              <div className="flex-1">
                <div className="flex items-center space-x-2 mb-1">
                  <FileText className="w-4 h-4 text-neutral-600" />
                  <span className="font-medium text-sm">{imp.fileName}</span>
                  <span className={`px-2 py-1 text-xs font-semibold rounded-full ${
                    bulkService.getStatusColor(imp.status)
                  }`}>
                    {imp.status}
                  </span>
                </div>
                <div className="text-xs text-neutral-600">
                  {bulkService.formatEntityType(imp.entityType)} • 
                  {new Date(imp.createdAt).toLocaleString()}
                  {imp.importResults && (
                    <> • {imp.importResults.successfulRecords} imported, {imp.importResults.failedRecords} failed</>
                  )}
                </div>
              </div>

              {imp.status === 'completed' && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handleRollback(imp.id)}
                >
                  Rollback
                </Button>
              )}
            </div>
          ))}
        </div>
      </Card>
    </div>
  );
};
