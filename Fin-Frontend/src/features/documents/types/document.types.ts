// Document Management Types
export interface Document {
  id: string;
  name: string;
  description?: string;
  fileType: string;
  fileSize: number;
  mimeType: string;
  url: string;
  thumbnailUrl?: string;
  category: DocumentCategory;
  tags: string[];
  metadata: DocumentMetadata;
  version: number;
  versions: DocumentVersion[];
  status: DocumentStatus;
  uploadedBy: string;
  uploadedAt: Date;
  updatedAt: Date;
  accessLevel: AccessLevel;
  retentionPolicy?: RetentionPolicy;
}

export type DocumentCategory = 
  | 'invoice' 
  | 'receipt' 
  | 'contract' 
  | 'report' 
  | 'statement' 
  | 'tax' 
  | 'legal' 
  | 'other';

export type DocumentStatus = 'draft' | 'active' | 'archived' | 'deleted';

export type AccessLevel = 'private' | 'internal' | 'public';

export interface DocumentMetadata {
  extractedText?: string;
  ocrConfidence?: number;
  pageCount?: number;
  language?: string;
  keywords?: string[];
  entities?: ExtractedEntity[];
  customFields?: Record<string, any>;
}

export interface ExtractedEntity {
  type: string;
  value: string;
  confidence: number;
}

export interface DocumentVersion {
  version: number;
  fileUrl: string;
  fileSize: number;
  uploadedBy: string;
  uploadedAt: Date;
  changes?: string;
}

export interface RetentionPolicy {
  id: string;
  name: string;
  retentionPeriod: number; // days
  deleteAfterExpiry: boolean;
  legalHold: boolean;
}

export interface UploadProgress {
  fileId: string;
  fileName: string;
  progress: number;
  status: 'uploading' | 'processing' | 'complete' | 'error';
  error?: string;
}

export interface DocumentFilter {
  category?: DocumentCategory;
  status?: DocumentStatus;
  uploadedBy?: string;
  fromDate?: Date;
  toDate?: Date;
  tags?: string[];
  search?: string;
}

export interface DocumentStats {
  totalDocuments: number;
  totalSize: number;
  documentsByCategory: CategoryCount[];
  documentsByType: TypeCount[];
  recentUploads: number;
}

export interface CategoryCount {
  category: DocumentCategory;
  count: number;
  size: number;
}

export interface TypeCount {
  fileType: string;
  count: number;
}
