import { DocumentMetadata } from '../types/document.types';

export class OCRService {
  private apiEndpoint = '/api/documents/ocr';

  async processDocument(documentId: string): Promise<DocumentMetadata> {
    const response = await fetch(`${this.apiEndpoint}/process/${documentId}`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to process document');
    return response.json();
  }

  async extractText(file: File): Promise<{ text: string; confidence: number }> {
    const formData = new FormData();
    formData.append('file', file);

    const response = await fetch(`${this.apiEndpoint}/extract`, {
      method: 'POST',
      body: formData,
    });
    if (!response.ok) throw new Error('Failed to extract text');
    return response.json();
  }

  async categorizeDocument(text: string): Promise<{ category: string; confidence: number }> {
    const response = await fetch(`${this.apiEndpoint}/categorize`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ text }),
    });
    if (!response.ok) throw new Error('Failed to categorize document');
    return response.json();
  }

  async extractEntities(text: string): Promise<any[]> {
    const response = await fetch(`${this.apiEndpoint}/entities`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ text }),
    });
    if (!response.ok) throw new Error('Failed to extract entities');
    return response.json();
  }
}

export const ocrService = new OCRService();
