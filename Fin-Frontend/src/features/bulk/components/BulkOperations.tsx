import React, { useState } from 'react';
import { CheckSquare, Trash2, Download, Mail, Printer, Edit } from 'lucide-react';
import { Card } from '../../../design-system/components/Card';
import { Button } from '../../../design-system/components/Button';
import { bulkService } from '../services/bulkService';

interface BulkOperationsProps {
  selectedIds: string[];
  entityType: string;
  onComplete: () => void;
}

export const BulkOperations: React.FC<BulkOperationsProps> = ({
  selectedIds,
  entityType,
  onComplete,
}) => {
  const [loading, setLoading] = useState(false);

  const handleBulkOperation = async (operationType: string) => {
    if (selectedIds.length === 0) {
      alert('Please select items first');
      return;
    }

    const confirmMessages: Record<string, string> = {
      approve: 'Approve selected items?',
      reject: 'Reject selected items?',
      delete: 'Delete selected items? This action cannot be undone.',
    };

    if (confirmMessages[operationType]) {
      if (!confirm(confirmMessages[operationType])) return;
    }

    setLoading(true);
    try {
      await bulkService.executeBulkOperation({
        type: operationType as any,
        entityType: entityType as any,
        selectedIds,
      });

      alert(`Bulk ${operationType} completed successfully`);
      onComplete();
    } catch (error) {
      console.error(`Failed to execute bulk ${operationType}:`, error);
      alert(`Failed to execute bulk ${operationType}`);
    } finally {
      setLoading(false);
    }
  };

  const handleBulkExport = async (format: 'csv' | 'excel' | 'pdf') => {
    if (selectedIds.length === 0) {
      alert('Please select items to export');
      return;
    }

    setLoading(true);
    try {
      const blob = await bulkService.exportData({
        entityType: entityType as any,
        format,
        columns: [],
        filters: { ids: selectedIds },
        includeHeaders: true,
      });

      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${entityType}-export.${format === 'excel' ? 'xlsx' : format}`;
      a.click();
    } catch (error) {
      console.error('Failed to export:', error);
    } finally {
      setLoading(false);
    }
  };

  if (selectedIds.length === 0) {
    return null;
  }

  return (
    <Card className="p-4 mb-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-2">
          <CheckSquare className="w-5 h-5 text-primary-600" />
          <span className="font-medium">{selectedIds.length} items selected</span>
        </div>

        <div className="flex items-center space-x-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => handleBulkOperation('approve')}
            disabled={loading}
          >
            <CheckSquare className="w-4 h-4 mr-2" />
            Approve
          </Button>

          <Button
            variant="outline"
            size="sm"
            onClick={() => handleBulkOperation('reject')}
            disabled={loading}
          >
            <CheckSquare className="w-4 h-4 mr-2" />
            Reject
          </Button>

          <Button
            variant="outline"
            size="sm"
            onClick={() => handleBulkExport('excel')}
            disabled={loading}
          >
            <Download className="w-4 h-4 mr-2" />
            Export
          </Button>

          <Button
            variant="outline"
            size="sm"
            onClick={() => handleBulkOperation('email')}
            disabled={loading}
          >
            <Mail className="w-4 h-4 mr-2" />
            Email
          </Button>

          <Button
            variant="outline"
            size="sm"
            onClick={() => handleBulkOperation('print')}
            disabled={loading}
          >
            <Printer className="w-4 h-4 mr-2" />
            Print
          </Button>

          <Button
            variant="outline"
            size="sm"
            onClick={() => handleBulkOperation('delete')}
            disabled={loading}
          >
            <Trash2 className="w-4 h-4 mr-2 text-error-600" />
            Delete
          </Button>
        </div>
      </div>
    </Card>
  );
};
