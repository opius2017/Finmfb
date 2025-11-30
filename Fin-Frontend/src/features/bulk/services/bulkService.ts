import {
  BulkImport,
  ImportTemplate,
  ImportHistory,
  BulkOperation,
  ExportConfig,
  ImportPreview,
  ColumnMapping,
} from '../types/bulk.types';

export class BulkService {
  private apiEndpoint = '/api/bulk';

  async uploadFile(file: File, entityType: string): Promise<BulkImport> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('entityType', entityType);

    const response = await fetch(`${this.apiEndpoint}/import/upload`, {
      method: 'POST',
      body: formData,
    });
    if (!response.ok) throw new Error('Failed to upload file');
    return response.json();
  }

  async getImport(importId: string): Promise<BulkImport> {
    const response = await fetch(`${this.apiEndpoint}/import/${importId}`);
    if (!response.ok) throw new Error('Failed to fetch import');
    return response.json();
  }

  async getImports(status?: string): Promise<BulkImport[]> {
    const params = status ? `?status=${status}` : '';
    const response = await fetch(`${this.apiEndpoint}/import${params}`);
    if (!response.ok) throw new Error('Failed to fetch imports');
    return response.json();
  }

  async previewImport(importId: string): Promise<ImportPreview> {
    const response = await fetch(`${this.apiEndpoint}/import/${importId}/preview`);
    if (!response.ok) throw new Error('Failed to preview import');
    return response.json();
  }

  async setMapping(importId: string, mapping: ColumnMapping[]): Promise<BulkImport> {
    const response = await fetch(`${this.apiEndpoint}/import/${importId}/mapping`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ mapping }),
    });
    if (!response.ok) throw new Error('Failed to set mapping');
    return response.json();
  }

  async validateImport(importId: string): Promise<BulkImport> {
    const response = await fetch(`${this.apiEndpoint}/import/${importId}/validate`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to validate import');
    return response.json();
  }

  async executeImport(importId: string): Promise<BulkImport> {
    const response = await fetch(`${this.apiEndpoint}/import/${importId}/execute`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to execute import');
    return response.json();
  }

  async cancelImport(importId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/import/${importId}/cancel`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to cancel import');
  }

  async rollbackImport(importId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/import/${importId}/rollback`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to rollback import');
  }

  async getTemplates(): Promise<ImportTemplate[]> {
    const response = await fetch(`${this.apiEndpoint}/templates`);
    if (!response.ok) throw new Error('Failed to fetch templates');
    return response.json();
  }

  async getTemplate(entityType: string): Promise<ImportTemplate> {
    const response = await fetch(`${this.apiEndpoint}/templates/${entityType}`);
    if (!response.ok) throw new Error('Failed to fetch template');
    return response.json();
  }

  async downloadTemplate(entityType: string, format: 'csv' | 'excel'): Promise<Blob> {
    const response = await fetch(`${this.apiEndpoint}/templates/${entityType}/download?format=${format}`);
    if (!response.ok) throw new Error('Failed to download template');
    return response.blob();
  }

  async getImportHistory(limit: number = 50): Promise<ImportHistory[]> {
    const response = await fetch(`${this.apiEndpoint}/import/history?limit=${limit}`);
    if (!response.ok) throw new Error('Failed to fetch import history');
    return response.json();
  }

  async executeBulkOperation(operation: Partial<BulkOperation>): Promise<BulkOperation> {
    const response = await fetch(`${this.apiEndpoint}/operations`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(operation),
    });
    if (!response.ok) throw new Error('Failed to execute bulk operation');
    return response.json();
  }

  async getBulkOperations(): Promise<BulkOperation[]> {
    const response = await fetch(`${this.apiEndpoint}/operations`);
    if (!response.ok) throw new Error('Failed to fetch bulk operations');
    return response.json();
  }

  async exportData(config: ExportConfig): Promise<Blob> {
    const response = await fetch(`${this.apiEndpoint}/export`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(config),
    });
    if (!response.ok) throw new Error('Failed to export data');
    return response.blob();
  }

  // Utility methods
  getStatusColor(status: string): string {
    switch (status) {
      case 'completed': return 'bg-success-100 text-success-800';
      case 'failed': return 'bg-error-100 text-error-800';
      case 'importing':
      case 'validating':
      case 'processing': return 'bg-warning-100 text-warning-800';
      case 'validated': return 'bg-primary-100 text-primary-800';
      default: return 'bg-neutral-100 text-neutral-800';
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(2)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
  }

  calculateSuccessRate(successful: number, total: number): number {
    return total === 0 ? 0 : (successful / total) * 100;
  }

  validateFileType(file: File, allowedTypes: string[]): boolean {
    return allowedTypes.some(type => file.name.endsWith(type));
  }

  validateFileSize(file: File, maxSizeMB: number): boolean {
    const maxBytes = maxSizeMB * 1024 * 1024;
    return file.size <= maxBytes;
  }

  suggestMapping(sourceColumns: string[], targetFields: string[]): ColumnMapping[] {
    const mapping: ColumnMapping[] = [];

    sourceColumns.forEach(sourceCol => {
      const normalized = sourceCol.toLowerCase().replace(/[^a-z0-9]/g, '');
      const match = targetFields.find(field => 
        field.toLowerCase().replace(/[^a-z0-9]/g, '') === normalized
      );

      if (match) {
        mapping.push({
          sourceColumn: sourceCol,
          targetField: match,
          dataType: 'string',
          required: false,
        });
      }
    });

    return mapping;
  }

  formatEntityType(entityType: string): string {
    return entityType.charAt(0).toUpperCase() + entityType.slice(1);
  }

  getOperationIcon(type: string): string {
    const icons: Record<string, string> = {
      approve: '✓',
      reject: '✗',
      delete: '🗑️',
      export: '📤',
      print: '🖨️',
      email: '📧',
      update: '✏️',
    };
    return icons[type] || '⚙️';
  }
}

export const bulkService = new BulkService();
