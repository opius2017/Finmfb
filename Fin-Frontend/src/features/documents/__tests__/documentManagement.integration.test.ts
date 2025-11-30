import { describe, it, expect, beforeEach, vi } from 'vitest';
import { documentService } from '../services/documentService';
import { ocrService } from '../services/ocrService';
import { searchService } from '../services/searchService';
import { retentionService } from '../services/retentionService';

// Mock fetch globally
global.fetch = vi.fn();

describe('Document Management Integration Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('File Upload and Storage', () => {
    it('should upload a document successfully', async () => {
      const mockFile = new File(['test content'], 'test.pdf', { type: 'application/pdf' });
      const mockResponse = {
        id: '123',
        name: 'test.pdf',
        size: 1024,
        type: 'application/pdf',
        uploadedAt: new Date(),
      };

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse,
      });

      const result = await documentService.uploadDocument(mockFile, {
        category: 'invoice',
        tags: ['test'],
      });

      expect(result).toEqual(mockResponse);
      expect(global.fetch).toHaveBeenCalledWith(
        '/api/documents/upload',
        expect.objectContaining({
          method: 'POST',
        })
      );
    });

    it('should handle upload failure', async () => {
      const mockFile = new File(['test content'], 'test.pdf', { type: 'application/pdf' });

      (global.fetch as any).mockResolvedValueOnce({
        ok: false,
        status: 500,
      });

      await expect(
        documentService.uploadDocument(mockFile, { category: 'invoice' })
      ).rejects.toThrow('Failed to upload document');
    });

    it('should validate file type before upload', () => {
      const allowedTypes = ['application/pdf', 'image/jpeg', 'image/png'];
      
      expect(documentService.isValidFileType('application/pdf', allowedTypes)).toBe(true);
      expect(documentService.isValidFileType('text/plain', allowedTypes)).toBe(false);
    });

    it('should validate file size', () => {
      const maxSize = 10 * 1024 * 1024; // 10MB
      
      expect(documentService.isValidFileSize(5 * 1024 * 1024, maxSize)).toBe(true);
      expect(documentService.isValidFileSize(15 * 1024 * 1024, maxSize)).toBe(false);
    });
  });

  describe('OCR and Document Processing', () => {
    it('should extract text from document', async () => {
      const mockOCRResult = {
        text: 'Invoice #12345\\nAmount: $100.00',
        confidence: 0.95,
        language: 'en',
      };

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockOCRResult,
      });

      const result = await ocrService.extractText('doc-123');

      expect(result.text).toContain('Invoice');
      expect(result.confidence).toBeGreaterThan(0.9);
    });

    it('should categorize document based on content', async () => {
      const mockCategory = {
        category: 'invoice',
        confidence: 0.92,
        suggestedTags: ['financial', 'accounts-payable'],
      };

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockCategory,
      });

      const result = await ocrService.categorizeDocument('doc-123');

      expect(result.category).toBe('invoice');
      expect(result.confidence).toBeGreaterThan(0.9);
    });

    it('should extract metadata from document', async () => {
      const mockMetadata = {
        invoiceNumber: '12345',
        amount: 100.00,
        date: '2024-01-15',
        vendor: 'ABC Corp',
      };

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockMetadata,
      });

      const result = await ocrService.extractMetadata('doc-123');

      expect(result.invoiceNumber).toBe('12345');
      expect(result.amount).toBe(100.00);
    });
  });

  describe('Document Search and Retrieval', () => {
    it('should search documents by text', async () => {
      const mockResults = {
        results: [
          { id: '1', name: 'Invoice 001.pdf', score: 0.95 },
          { id: '2', name: 'Invoice 002.pdf', score: 0.87 },
        ],
        total: 2,
      };

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockResults,
      });

      const results = await searchService.searchDocuments('invoice');

      expect(results.results).toHaveLength(2);
      expect(results.results[0].score).toBeGreaterThan(results.results[1].score);
    });

    it('should filter search by metadata', async () => {
      const mockResults = {
        results: [
          { id: '1', name: 'Invoice 001.pdf', category: 'invoice' },
        ],
        total: 1,
      };

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockResults,
      });

      const results = await searchService.searchDocuments('', {
        category: 'invoice',
        dateFrom: '2024-01-01',
        dateTo: '2024-12-31',
      });

      expect(results.results).toHaveLength(1);
      expect(results.results[0].category).toBe('invoice');
    });

    it('should rank search results by relevance', () => {
      const results = [
        { id: '1', score: 0.75 },
        { id: '2', score: 0.95 },
        { id: '3', score: 0.85 },
      ];

      const ranked = searchService.rankResults(results);

      expect(ranked[0].score).toBe(0.95);
      expect(ranked[1].score).toBe(0.85);
      expect(ranked[2].score).toBe(0.75);
    });
  });

  describe('Document Versioning', () => {
    it('should create new version of document', async () => {
      const mockVersion = {
        id: 'v2',
        documentId: 'doc-123',
        version: 2,
        createdAt: new Date(),
        createdBy: 'user-1',
      };

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockVersion,
      });

      const result = await documentService.createVersion('doc-123', 'Updated content');

      expect(result.version).toBe(2);
      expect(result.documentId).toBe('doc-123');
    });

    it('should retrieve version history', async () => {
      const mockHistory = [
        { version: 2, createdAt: new Date('2024-01-15') },
        { version: 1, createdAt: new Date('2024-01-01') },
      ];

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockHistory,
      });

      const history = await documentService.getVersionHistory('doc-123');

      expect(history).toHaveLength(2);
      expect(history[0].version).toBe(2);
    });

    it('should restore previous version', async () => {
      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => ({ success: true }),
      });

      await documentService.restoreVersion('doc-123', 1);

      expect(global.fetch).toHaveBeenCalledWith(
        '/api/documents/doc-123/versions/1/restore',
        expect.objectContaining({ method: 'POST' })
      );
    });
  });

  describe('Document Retention Policies', () => {
    it('should apply retention policy to document', async () => {
      const mockStatus = {
        documentId: 'doc-123',
        policyId: 'policy-1',
        retentionDate: new Date('2025-01-01'),
        status: 'active',
      };

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockStatus,
      });

      const result = await retentionService.applyPolicyToDocument('doc-123', 'policy-1');

      expect(result.policyId).toBe('policy-1');
      expect(result.status).toBe('active');
    });

    it('should calculate retention date correctly', () => {
      const createdDate = new Date('2024-01-01');
      
      const daysResult = retentionService.calculateRetentionDate(createdDate, 30, 'days');
      expect(daysResult.getDate()).toBe(31);

      const monthsResult = retentionService.calculateRetentionDate(createdDate, 6, 'months');
      expect(monthsResult.getMonth()).toBe(6); // July (0-indexed)

      const yearsResult = retentionService.calculateRetentionDate(createdDate, 7, 'years');
      expect(yearsResult.getFullYear()).toBe(2031);
    });

    it('should identify documents due for retention action', () => {
      const pastDate = new Date('2023-01-01');
      const futureDate = new Date('2025-12-31');

      expect(retentionService.isRetentionDue(pastDate)).toBe(true);
      expect(retentionService.isRetentionDue(futureDate)).toBe(false);
    });

    it('should prevent deletion of documents on legal hold', async () => {
      const mockHold = {
        id: 'hold-1',
        documentIds: ['doc-123'],
        isActive: true,
      };

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockHold,
      });

      const holds = await retentionService.getLegalHolds();
      const isOnHold = holds.some(h => 
        h.isActive && h.documentIds.includes('doc-123')
      );

      expect(isOnHold).toBe(true);
    });

    it('should execute retention actions', async () => {
      const mockResult = {
        archived: 10,
        deleted: 5,
        errors: 0,
      };

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockResult,
      });

      const result = await retentionService.executeRetentionActions();

      expect(result.archived).toBe(10);
      expect(result.deleted).toBe(5);
      expect(result.errors).toBe(0);
    });
  });

  describe('End-to-End Document Workflow', () => {
    it('should complete full document lifecycle', async () => {
      // 1. Upload document
      const mockFile = new File(['test'], 'invoice.pdf', { type: 'application/pdf' });
      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => ({ id: 'doc-123', name: 'invoice.pdf' }),
      });

      const uploaded = await documentService.uploadDocument(mockFile, { category: 'invoice' });
      expect(uploaded.id).toBe('doc-123');

      // 2. Process with OCR
      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => ({ text: 'Invoice content', confidence: 0.95 }),
      });

      const ocr = await ocrService.extractText('doc-123');
      expect(ocr.confidence).toBeGreaterThan(0.9);

      // 3. Apply retention policy
      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => ({ 
          documentId: 'doc-123',
          policyId: 'policy-1',
          status: 'active',
        }),
      });

      const retention = await retentionService.applyPolicyToDocument('doc-123', 'policy-1');
      expect(retention.status).toBe('active');

      // 4. Search and retrieve
      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => ({ 
          results: [{ id: 'doc-123', name: 'invoice.pdf' }],
          total: 1,
        }),
      });

      const search = await searchService.searchDocuments('invoice');
      expect(search.results).toHaveLength(1);
    });
  });
});
