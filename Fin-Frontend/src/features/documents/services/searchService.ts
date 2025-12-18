import { Document } from '../types/document.types';

export class SearchService {
  private apiEndpoint = '/api/documents/search';

  async search(query: string, filters?: any): Promise<{ documents: Document[]; total: number }> {
    const params = new URLSearchParams({ query });
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined) {
          params.append(key, (value || '').toString());
        }
      });
    }

    const response = await fetch(`${this.apiEndpoint}?${params}`);
    if (!response.ok) throw new Error('Failed to search documents');
    return response.json();
  }

  async fullTextSearch(query: string): Promise<Document[]> {
    const response = await fetch(`${this.apiEndpoint}/fulltext`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ query }),
    });
    if (!response.ok) throw new Error('Failed to perform full-text search');
    return response.json();
  }

  async metadataSearch(metadata: Record<string, any>): Promise<Document[]> {
    const response = await fetch(`${this.apiEndpoint}/metadata`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ metadata }),
    });
    if (!response.ok) throw new Error('Failed to search by metadata');
    return response.json();
  }

  async getSuggestions(query: string): Promise<string[]> {
    const response = await fetch(`${this.apiEndpoint}/suggestions?query=${encodeURIComponent(query)}`);
    if (!response.ok) return [];
    return response.json();
  }

  async getRecentDocuments(limit: number = 10): Promise<Document[]> {
    const response = await fetch(`${this.apiEndpoint}/recent?limit=${limit}`);
    if (!response.ok) throw new Error('Failed to fetch recent documents');
    return response.json();
  }

  async indexDocument(documentId: string): Promise<void> {
    await fetch(`${this.apiEndpoint}/index/${documentId}`, {
      method: 'POST',
    });
  }
}

export const searchService = new SearchService();
