import React, { useState, useEffect } from 'react';
import { Download, FileSpreadsheet } from 'lucide-react';
import { Card } from '../../../design-system/components/Card';
import { Button } from '../../../design-system/components/Button';
import { ImportTemplate } from '../types/bulk.types';
import { bulkService } from '../services/bulkService';

export const ExportTemplates: React.FC = () => {
  const [templates, setTemplates] = useState<ImportTemplate[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadTemplates();
  }, []);

  const loadTemplates = async () => {
    setLoading(true);
    try {
      const data = await bulkService.getTemplates();
      setTemplates(data);
    } catch (error) {
      console.error('Failed to load templates:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleDownload = async (entityType: string, format: 'csv' | 'excel') => {
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

  return (
    <div className="p-6">
      <div className="mb-6">
        <h1 className="text-2xl font-bold">Export Templates</h1>
        <p className="text-sm text-neutral-600 mt-1">
          Download templates for bulk data import
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {loading ? (
          <Card className="p-8 col-span-full">
            <div className="text-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            </div>
          </Card>
        ) : (
          templates.map((template) => (
            <Card key={template.id} className="p-6">
              <div className="flex items-start space-x-3 mb-4">
                <FileSpreadsheet className="w-8 h-8 text-primary-600" />
                <div className="flex-1">
                  <h3 className="font-semibold text-lg">{template.name}</h3>
                  <p className="text-sm text-neutral-600 capitalize">
                    {bulkService.formatEntityType(template.entityType)}
                  </p>
                </div>
              </div>

              <div className="mb-4">
                <div className="text-sm text-neutral-600 mb-2">Columns:</div>
                <div className="flex flex-wrap gap-1">
                  {template.columns.slice(0, 5).map((col) => (
                    <span
                      key={col.name}
                      className="px-2 py-1 text-xs bg-neutral-100 rounded"
                    >
                      {col.name}
                      {col.required && <span className="text-error-600">*</span>}
                    </span>
                  ))}
                  {template.columns.length > 5 && (
                    <span className="px-2 py-1 text-xs bg-neutral-100 rounded">
                      +{template.columns.length - 5} more
                    </span>
                  )}
                </div>
              </div>

              <div className="space-y-2">
                <Button
                  variant="primary"
                  size="sm"
                  className="w-full"
                  onClick={() => handleDownload(template.entityType, 'excel')}
                >
                  <Download className="w-4 h-4 mr-2" />
                  Download Excel
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  className="w-full"
                  onClick={() => handleDownload(template.entityType, 'csv')}
                >
                  <Download className="w-4 h-4 mr-2" />
                  Download CSV
                </Button>
              </div>

              <div className="mt-4 pt-4 border-t border-neutral-200">
                <div className="text-xs text-neutral-500">
                  Version {template.version} • {template.columns.length} columns
                </div>
              </div>
            </Card>
          ))
        )}
      </div>

      {/* Template Details */}
      <Card className="p-6 mt-6">
        <h2 className="text-lg font-semibold mb-4">Template Guidelines</h2>
        
        <div className="space-y-4 text-sm">
          <div>
            <h3 className="font-medium mb-2">File Format</h3>
            <ul className="list-disc list-inside text-neutral-600 space-y-1">
              <li>Supported formats: CSV, Excel (.xlsx, .xls)</li>
              <li>Maximum file size: 10MB</li>
              <li>First row must contain column headers</li>
            </ul>
          </div>

          <div>
            <h3 className="font-medium mb-2">Data Validation</h3>
            <ul className="list-disc list-inside text-neutral-600 space-y-1">
              <li>Required fields are marked with an asterisk (*)</li>
              <li>Date format: YYYY-MM-DD</li>
              <li>Number format: Use decimal point (.) not comma (,)</li>
              <li>Email addresses must be valid</li>
            </ul>
          </div>

          <div>
            <h3 className="font-medium mb-2">Sample Data</h3>
            <p className="text-neutral-600">
              Each template includes sample data to help you understand the expected format.
              You can replace the sample data with your actual data.
            </p>
          </div>
        </div>
      </Card>
    </div>
  );
};
