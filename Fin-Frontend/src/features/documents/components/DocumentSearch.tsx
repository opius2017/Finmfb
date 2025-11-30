import React, { useState } from 'react';
import { Search, Filter, FileText } from 'lucide-react';
import { Button } from '../../../design-system/components/Button';
import { Card } from '../../../design-system/components/Card';
import { Input } from '../../../design-system/components/Input';
import { Document } from '../types/document.types';
import { searchService } from '../services/searchService';

export const DocumentSearch: React.FC = () => {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<Document[]>([]);
  const [loading, setLoading] = useState(false);

  const handleSearch = async () => {
    if (!query.trim()) return;

    setLoading(true);
    try {
      const { documents } = await searchService.search(query);
      setResults(documents);
    } catch (error) {
      console.error('Search failed:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="p-6">
      <h1 className="text-2xl font-bold mb-6">Document Search</h1>

      <Card className="p-6 mb-6">
        <div className="flex space-x-3">
          <div className="flex-1">
            <Input
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder="Search documents..."
              onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
            />
          </div>
          <Button variant="primary" onClick={handleSearch} disabled={loading}>
            <Search className="w-4 h-4 mr-2" />
            Search
          </Button>
          <Button variant="outline">
            <Filter className="w-4 h-4" />
          </Button>
        </div>
      </Card>

      {results.length > 0 && (
        <Card className="p-6">
          <h2 className="font-semibold mb-4">Search Results ({results.length})</h2>
          <div className="space-y-3">
            {results.map((doc) => (
              <div key={doc.id} className="flex items-start space-x-3 p-3 bg-neutral-50 rounded-lg hover:bg-neutral-100 cursor-pointer">
                <FileText className="w-5 h-5 text-neutral-600 mt-1" />
                <div className="flex-1">
                  <div className="font-medium">{doc.name}</div>
                  <div className="text-sm text-neutral-600 mt-1">{doc.description}</div>
                  <div className="flex items-center space-x-2 mt-2">
                    <span className="text-xs px-2 py-1 bg-neutral-200 rounded">{doc.category}</span>
                    <span className="text-xs text-neutral-500">
                      {new Date(doc.uploadedAt).toLocaleDateString()}
                    </span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </Card>
      )}
    </div>
  );
};
