import { Document, DocumentFilter, DocumentStats, UploadProgress } from '../types/document.types';

export class DocumentService {
  private apiEndpoint = '/api/documents';

  async getDocuments(filter?: DocumentFilter): Promise<{ documents: Document[]; total: number }> {
    const params = new URLSearchParams();
    if (filter) {
      Object.entries(filter).forEach(([key, value]) => {
        if (value !== undefined) {
          params.append(key, value.toString());
        }
      });
    }

    const response = await fetch(`${this.apiEndpoint}?${params}`);
    if (!response.ok) throw new Error('Failed to fetch documents');
    return response.json();
  }

  async getDocument(documentId: string): Promise<Document> {
    const response = await fetch(`${this.apiEndpoint}/${documentId}`);
    if (!response.ok) throw new Error('Failed to fetch document');
    return response.json();
  }

  async uploadDocument(
    file: File,
    metadata: Partial<Document>,
    onProgress?: (progress: UploadProgress) => void
  ): Promise<Document> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('metadata', JSON.stringify(metadata));

    const xhr = new XMLHttpRequest();

    return new Promise((resolve, reject) => {
      xhr.upload.addEventListener('progress', (e) => {
        if (e.lengthComputable && onProgress) {
          onProgress({
            fileId: file.name,
            fileName: file.name,
            progress: (e.loaded / e.total) * 100,
            status: 'uploading',
          });
        }
      });

      xhr.addEventListener('load', () => {
        if (xhr.status === 200) {
          resolve(JSON.parse(xhr.responseText));
        } else {
          reject(new Error('Upload failed'));
        }
      });

      xhr.addEventListener('error', () => reject(new Error('Upload failed')));

      xhr.open('POST', this.apiEndpoint);
      xhr.send(formData);
    });
  }

  async updateDocument(documentId: string, updates: Partial<Document>): Promise<Document> {
    const response = await fetch(`${this.apiEndpoint}/${documentId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(updates),
    });
    if (!response.ok) throw new Error('Failed to update document');
    return response.json();
  }

  async deleteDocument(documentId: string): Promise<void> {
    const response = await fetch(`${this.apiEndpoint}/${documentId}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to delete document');
  }

  async downloadDocument(documentId: string): Promise<Blob> {
    const response = await fetch(`${this.apiEndpoint}/${documentId}/download`);
    if (!response.ok) throw new Error('Failed to download document');
    return response.blob();
  }

  async getStats(): Promise<DocumentStats> {
    const response = await fetch(`${this.apiEndpoint}/stats`);
    if (!response.ok) throw new Error('Failed to fetch stats');
    return response.json();
  }

  validateFile(file: File): { valid: boolean; error?: string } {
    const maxSize = 50 * 1024 * 1024; // 50MB
    const allowedTypes = [
      'application/pdf',
      'image/jpeg',
      'image/png',
      'image/jpg',
      'application/msword',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
      'application/vnd.ms-excel',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    ];

    if (file.size > maxSize) {
      return { valid: false, error: 'File size exceeds 50MB limit' };
    }

    if (!allowedTypes.includes(file.type)) {
      return { valid: false, error: 'File type not supported' };
    }

    return { valid: true };
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }
}

export const documentService = new DocumentService();
