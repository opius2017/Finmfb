import {
  SearchResult,
  SearchQuery,
  SearchHistory,
  SearchSuggestion,
  SearchStats,
} from '../types/search.types';

export class SearchService {
  private apiEndpoint = '/api/search';
  private searchHistory: SearchHistory[] = [];
  private maxHistorySize = 50;

  async search(query: SearchQuery): Promise<SearchResult[]> {
    const response = await fetch(`${this.apiEndpoint}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(query),
    });
    if (!response.ok) throw new Error('Failed to search');
    
    const results = await response.json();
    this.addToHistory(query.query, results.length);
    return results;
  }

  async quickSearch(query: string): Promise<SearchResult[]> {
    if (!query || query.length < 2) return [];

    const response = await fetch(`${this.apiEndpoint}/quick?q=${encodeURIComponent(query)}`);
    if (!response.ok) throw new Error('Failed to quick search');
    return response.json();
  }

  async getSuggestions(query: string): Promise<SearchSuggestion[]> {
    if (!query) {
      return this.getRecentSearches();
    }

    const response = await fetch(`${this.apiEndpoint}/suggestions?q=${encodeURIComponent(query)}`);
    if (!response.ok) throw new Error('Failed to get suggestions');
    return response.json();
  }

  async getStats(): Promise<SearchStats> {
    const response = await fetch(`${this.apiEndpoint}/stats`);
    if (!response.ok) throw new Error('Failed to get search stats');
    return response.json();
  }

  getSearchHistory(): SearchHistory[] {
    const stored = localStorage.getItem('searchHistory');
    if (stored) {
      this.searchHistory = JSON.parse(stored);
    }
    return this.searchHistory;
  }

  addToHistory(query: string, resultCount: number): void {
    const history: SearchHistory = {
      id: Date.now().toString(),
      query,
      timestamp: new Date(),
      resultCount,
    };

    this.searchHistory.unshift(history);
    
    // Remove duplicates
    this.searchHistory = this.searchHistory.filter((item, index, self) =>
      index === self.findIndex(t => t.query.toLowerCase() === item.query.toLowerCase())
    );

    // Limit size
    if (this.searchHistory.length > this.maxHistorySize) {
      this.searchHistory = this.searchHistory.slice(0, this.maxHistorySize);
    }

    localStorage.setItem('searchHistory', JSON.stringify(this.searchHistory));
  }

  clearHistory(): void {
    this.searchHistory = [];
    localStorage.removeItem('searchHistory');
  }

  getRecentSearches(): SearchSuggestion[] {
    return this.getSearchHistory()
      .slice(0, 5)
      .map(h => ({
        text: h.query,
        type: 'recent' as const,
        count: h.resultCount,
      }));
  }

  // Utility methods
  highlightMatch(text: string, query: string): string {
    if (!query) return text;

    const regex = new RegExp(`(${query})`, 'gi');
    return text.replace(regex, '<mark>$1</mark>');
  }

  getResultIcon(type: string): string {
    const icons: Record<string, string> = {
      customer: '👤',
      vendor: '🏢',
      invoice: '📄',
      payment: '💳',
      product: '📦',
      transaction: '💰',
      report: '📊',
      page: '📑',
    };
    return icons[type] || '📄';
  }

  formatResultType(type: string): string {
    return type.charAt(0).toUpperCase() + type.slice(1);
  }

  rankResults(results: SearchResult[]): SearchResult[] {
    return results.sort((a, b) => b.score - a.score);
  }

  filterResults(results: SearchResult[], type?: string): SearchResult[] {
    if (!type) return results;
    return results.filter(r => r.type === type);
  }

  groupResultsByType(results: SearchResult[]): Record<string, SearchResult[]> {
    return results.reduce((acc, result) => {
      if (!acc[result.type]) {
        acc[result.type] = [];
      }
      acc[result.type].push(result);
      return acc;
    }, {} as Record<string, SearchResult[]>);
  }
}

export const searchService = new SearchService();
