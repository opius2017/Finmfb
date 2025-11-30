// Global Search Types
export interface SearchResult {
  id: string;
  type: SearchResultType;
  title: string;
  subtitle?: string;
  description?: string;
  url: string;
  icon?: string;
  score: number;
  metadata?: Record<string, any>;
}

export type SearchResultType = 
  | 'customer' 
  | 'vendor' 
  | 'invoice' 
  | 'payment' 
  | 'product' 
  | 'transaction' 
  | 'report' 
  | 'page';

export interface SearchQuery {
  query: string;
  filters?: SearchFilter[];
  limit?: number;
  offset?: number;
}

export interface SearchFilter {
  field: string;
  value: any;
  operator?: 'equals' | 'contains' | 'greater_than' | 'less_than';
}

export interface SearchHistory {
  id: string;
  query: string;
  timestamp: Date;
  resultCount: number;
}

export interface SearchSuggestion {
  text: string;
  type: 'recent' | 'popular' | 'autocomplete';
  count?: number;
}

export interface SearchStats {
  totalSearches: number;
  popularQueries: PopularQuery[];
  averageResultCount: number;
  averageResponseTime: number;
}

export interface PopularQuery {
  query: string;
  count: number;
  lastSearched: Date;
}
