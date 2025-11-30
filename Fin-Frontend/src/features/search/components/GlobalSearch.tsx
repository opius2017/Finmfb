import React, { useState, useEffect, useRef } from 'react';
import { Search, Clock, TrendingUp, X } from 'lucide-react';
import { SearchResult, SearchSuggestion } from '../types/search.types';
import { searchService } from '../services/searchService';
import { debounce } from '../../../utils/performance/optimization';

interface GlobalSearchProps {
  isOpen: boolean;
  onClose: () => void;
}

export const GlobalSearch: React.FC<GlobalSearchProps> = ({ isOpen, onClose }) => {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<SearchResult[]>([]);
  const [suggestions, setSuggestions] = useState<SearchSuggestion[]>([]);
  const [loading, setLoading] = useState(false);
  const [selectedIndex, setSelectedIndex] = useState(0);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (isOpen) {
      inputRef.current?.focus();
      loadSuggestions('');
    } else {
      setQuery('');
      setResults([]);
      setSelectedIndex(0);
    }
  }, [isOpen]);

  useEffect(() => {
    const handleEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape' && isOpen) {
        onClose();
      }
    };

    const handleArrowKeys = (event: KeyboardEvent) => {
      if (!isOpen) return;

      if (event.key === 'ArrowDown') {
        event.preventDefault();
        setSelectedIndex(prev => Math.min(prev + 1, results.length - 1));
      } else if (event.key === 'ArrowUp') {
        event.preventDefault();
        setSelectedIndex(prev => Math.max(prev - 1, 0));
      } else if (event.key === 'Enter' && results[selectedIndex]) {
        event.preventDefault();
        handleResultClick(results[selectedIndex]);
      }
    };

    document.addEventListener('keydown', handleEscape);
    document.addEventListener('keydown', handleArrowKeys);
    
    return () => {
      document.removeEventListener('keydown', handleEscape);
      document.removeEventListener('keydown', handleArrowKeys);
    };
  }, [isOpen, results, selectedIndex, onClose]);

  const loadSuggestions = async (searchQuery: string) => {
    try {
      const data = await searchService.getSuggestions(searchQuery);
      setSuggestions(data);
    } catch (error) {
      console.error('Failed to load suggestions:', error);
    }
  };

  const performSearch = debounce(async (searchQuery: string) => {
    if (!searchQuery || searchQuery.length < 2) {
      setResults([]);
      return;
    }

    setLoading(true);
    try {
      const data = await searchService.quickSearch(searchQuery);
      setResults(data);
      setSelectedIndex(0);
    } catch (error) {
      console.error('Failed to search:', error);
    } finally {
      setLoading(false);
    }
  }, 300);

  const handleQueryChange = (value: string) => {
    setQuery(value);
    performSearch(value);
    if (value) {
      loadSuggestions(value);
    }
  };

  const handleSuggestionClick = (suggestion: SearchSuggestion) => {
    setQuery(suggestion.text);
    performSearch(suggestion.text);
  };

  const handleResultClick = (result: SearchResult) => {
    window.location.href = result.url;
    onClose();
  };

  const handleClearHistory = () => {
    searchService.clearHistory();
    loadSuggestions('');
  };

  if (!isOpen) return null;

  const groupedResults = searchService.groupResultsByType(results);

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-start justify-center pt-20 z-50">
      <div className="w-full max-w-2xl bg-white rounded-lg shadow-2xl overflow-hidden">
        {/* Search Input */}
        <div className="p-4 border-b border-neutral-200">
          <div className="flex items-center space-x-3">
            <Search className="w-5 h-5 text-neutral-400" />
            <input
              ref={inputRef}
              type="text"
              value={query}
              onChange={(e) => handleQueryChange(e.target.value)}
              placeholder="Search customers, invoices, transactions..."
              className="flex-1 text-lg outline-none"
            />
            {query && (
              <button
                onClick={() => handleQueryChange('')}
                className="p-1 hover:bg-neutral-100 rounded"
              >
                <X className="w-4 h-4 text-neutral-400" />
              </button>
            )}
            <kbd className="px-2 py-1 text-xs font-mono bg-neutral-100 border border-neutral-300 rounded">
              Esc
            </kbd>
          </div>
        </div>

        {/* Results/Suggestions */}
        <div className="max-h-96 overflow-y-auto">
          {loading ? (
            <div className="p-8 text-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            </div>
          ) : results.length > 0 ? (
            <div>
              {Object.entries(groupedResults).map(([type, typeResults]) => (
                <div key={type}>
                  <div className="px-4 py-2 bg-neutral-50 text-xs font-semibold text-neutral-600 uppercase">
                    {searchService.formatResultType(type)} ({typeResults.length})
                  </div>
                  {typeResults.map((result, index) => {
                    const globalIndex = results.indexOf(result);
                    return (
                      <button
                        key={result.id}
                        onClick={() => handleResultClick(result)}
                        className={`w-full px-4 py-3 flex items-start space-x-3 hover:bg-neutral-50 transition-colors ${
                          globalIndex === selectedIndex ? 'bg-primary-50' : ''
                        }`}
                      >
                        <span className="text-2xl">{searchService.getResultIcon(result.type)}</span>
                        <div className="flex-1 text-left">
                          <div className="font-medium text-sm">{result.title}</div>
                          {result.subtitle && (
                            <div className="text-xs text-neutral-600">{result.subtitle}</div>
                          )}
                          {result.description && (
                            <div className="text-xs text-neutral-500 mt-1">{result.description}</div>
                          )}
                        </div>
                      </button>
                    );
                  })}
                </div>
              ))}
            </div>
          ) : query.length >= 2 ? (
            <div className="p-8 text-center text-neutral-600">
              <Search className="w-12 h-12 mx-auto mb-3 text-neutral-400" />
              <p>No results found for "{query}"</p>
            </div>
          ) : suggestions.length > 0 ? (
            <div>
              <div className="px-4 py-2 bg-neutral-50 flex items-center justify-between">
                <span className="text-xs font-semibold text-neutral-600 uppercase">
                  Recent Searches
                </span>
                <button
                  onClick={handleClearHistory}
                  className="text-xs text-primary-600 hover:underline"
                >
                  Clear
                </button>
              </div>
              {suggestions.map((suggestion, index) => (
                <button
                  key={index}
                  onClick={() => handleSuggestionClick(suggestion)}
                  className="w-full px-4 py-3 flex items-center space-x-3 hover:bg-neutral-50 transition-colors"
                >
                  {suggestion.type === 'recent' ? (
                    <Clock className="w-4 h-4 text-neutral-400" />
                  ) : (
                    <TrendingUp className="w-4 h-4 text-neutral-400" />
                  )}
                  <span className="flex-1 text-left text-sm">{suggestion.text}</span>
                  {suggestion.count !== undefined && (
                    <span className="text-xs text-neutral-500">{suggestion.count} results</span>
                  )}
                </button>
              ))}
            </div>
          ) : (
            <div className="p-8 text-center text-neutral-600">
              <Search className="w-12 h-12 mx-auto mb-3 text-neutral-400" />
              <p>Start typing to search...</p>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="px-4 py-3 bg-neutral-50 border-t border-neutral-200 flex items-center justify-between text-xs text-neutral-600">
          <div className="flex items-center space-x-4">
            <span className="flex items-center space-x-1">
              <kbd className="px-1.5 py-0.5 font-mono bg-white border border-neutral-300 rounded">↑</kbd>
              <kbd className="px-1.5 py-0.5 font-mono bg-white border border-neutral-300 rounded">↓</kbd>
              <span>Navigate</span>
            </span>
            <span className="flex items-center space-x-1">
              <kbd className="px-1.5 py-0.5 font-mono bg-white border border-neutral-300 rounded">Enter</kbd>
              <span>Select</span>
            </span>
          </div>
          <span>{results.length} results</span>
        </div>
      </div>
    </div>
  );
};
