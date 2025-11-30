import { logger } from '@utils/logger';
import * as fs from 'fs';
import * as path from 'path';
import * as crypto from 'crypto';

/**
 * File upload data
 */
export interface FileUploadData {
  filename: string;
  originalName: string;
  mimeType: string;
  size: number;
  buffer: Buffer;
  userId: string;
}

/**
 * File metadata
 */
export interface FileMetadata {
  id: string;
  filename: string;
  originalName: string;
  mimeType: string;
  size: number;
  path: string;
  url: string;
  uploadedBy: string;
  uploadedAt: Date;
}

/**
 * Signed URL options
 */
export interface SignedUrlOptions {
  expiresIn?: number; // seconds
  action?: 'read' | 'write';
}

export class FileStorageService {
  private readonly STORAGE_PATH = process.env.FILE_STORAGE_PATH || './storage/files';
  private readonly MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB
  private readonly ALLOWED_MIME_TYPES = [
    'application/pdf',
    'image/jpeg',
    'image/jpg',
    'image/png',
    'image/gif',
    'application/msword',
    'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
    'application/vnd.ms-excel',
    'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    'text/plain',
    'text/csv',
  ];

  constructor() {
    // Ensure storage directory exists
    this.ensureStorageDirectory();
  }

  /**
   * Upload file
   */
  async uploadFile(data: FileUploadData): Promise<FileMetadata> {
    try {
      logger.info('Uploading file', {
        filename: data.originalName,
        size: data.size,
        mimeType: data.mimeType,
      });

      // Validate file
      this.validateFile(data);

      // Generate unique filename
      const fileId = this.generateFileId();
      const extension = path.extname(data.originalName);
      const filename = `${fileId}${extension}`;
      const filePath = path.join(this.STORAGE_PATH, filename);

      // Save file to disk
      await fs.promises.writeFile(filePath, data.buffer);

      // Generate file URL
      const url = this.generateFileUrl(filename);

      const metadata: FileMetadata = {
        id: fileId,
        filename,
        originalName: data.originalName,
        mimeType: data.mimeType,
        size: data.size,
        path: filePath,
        url,
        uploadedBy: data.userId,
        uploadedAt: new Date(),
      };

      logger.info('File uploaded successfully', {
        fileId,
        filename,
      });

      return metadata;
    } catch (error) {
      logger.error('Error uploading file:', error);
      throw error;
    }
  }

  /**
   * Download file
   */
  async downloadFile(filename: string): Promise<Buffer> {
    try {
      const filePath = path.join(this.STORAGE_PATH, filename);

      // Check if file exists
      if (!fs.existsSync(filePath)) {
        throw new Error('File not found');
      }

      // Read file
      const buffer = await fs.promises.readFile(filePath);

      logger.info('File downloaded', {
        filename,
      });

      return buffer;
    } catch (error) {
      logger.error('Error downloading file:', error);
      throw error;
    }
  }

  /**
   * Delete file
   */
  async deleteFile(filename: string): Promise<void> {
    try {
      const filePath = path.join(this.STORAGE_PATH, filename);

      // Check if file exists
      if (!fs.existsSync(filePath)) {
        throw new Error('File not found');
      }

      // Delete file
      await fs.promises.unlink(filePath);

      logger.info('File deleted', {
        filename,
      });
    } catch (error) {
      logger.error('Error deleting file:', error);
      throw error;
    }
  }

  /**
   * Generate signed URL for file access
   */
  generateSignedUrl(filename: string, options: SignedUrlOptions = {}): string {
    const expiresIn = options.expiresIn || 3600; // 1 hour default
    const expiresAt = Date.now() + expiresIn * 1000;

    // Generate signature
    const signature = this.generateSignature(filename, expiresAt);

    // Build URL with signature
    const baseUrl = process.env.API_BASE_URL || 'http://localhost:3000';
    const url = `${baseUrl}/api/v1/documents/download/${filename}?expires=${expiresAt}&signature=${signature}`;

    return url;
  }

  /**
   * Verify signed URL
   */
  verifySignedUrl(filename: string, expiresAt: number, signature: string): boolean {
    // Check if expired
    if (Date.now() > expiresAt) {
      return false;
    }

    // Verify signature
    const expectedSignature = this.generateSignature(filename, expiresAt);
    return signature === expectedSignature;
  }

  /**
   * Get file info
   */
  async getFileInfo(filename: string): Promise<{
    exists: boolean;
    size?: number;
    mimeType?: string;
  }> {
    try {
      const filePath = path.join(this.STORAGE_PATH, filename);

      if (!fs.existsSync(filePath)) {
        return { exists: false };
      }

      const stats = await fs.promises.stat(filePath);
      const extension = path.extname(filename).toLowerCase();
      const mimeType = this.getMimeTypeFromExtension(extension);

      return {
        exists: true,
        size: stats.size,
        mimeType,
      };
    } catch (error) {
      logger.error('Error getting file info:', error);
      return { exists: false };
    }
  }

  /**
   * Validate file
   */
  private validateFile(data: FileUploadData): void {
    // Check file size
    if (data.size > this.MAX_FILE_SIZE) {
      throw new Error(`File size exceeds maximum allowed size of ${this.MAX_FILE_SIZE / 1024 / 1024}MB`);
    }

    // Check mime type
    if (!this.ALLOWED_MIME_TYPES.includes(data.mimeType)) {
      throw new Error(`File type ${data.mimeType} is not allowed`);
    }

    // Check filename
    if (!data.originalName || data.originalName.length === 0) {
      throw new Error('Filename is required');
    }

    // Check for malicious filenames
    if (data.originalName.includes('..') || data.originalName.includes('/') || data.originalName.includes('\\')) {
      throw new Error('Invalid filename');
    }
  }

  /**
   * Scan file for viruses (placeholder)
   */
  async scanFile(buffer: Buffer): Promise<{ isClean: boolean; threats?: string[] }> {
    // TODO: Integrate with virus scanning service (e.g., ClamAV)
    // For now, return clean
    logger.info('Virus scan performed (placeholder)');
    return { isClean: true };
  }

  /**
   * Generate unique file ID
   */
  private generateFileId(): string {
    return crypto.randomBytes(16).toString('hex');
  }

  /**
   * Generate file URL
   */
  private generateFileUrl(filename: string): string {
    const baseUrl = process.env.API_BASE_URL || 'http://localhost:3000';
    return `${baseUrl}/api/v1/documents/download/${filename}`;
  }

  /**
   * Generate signature for signed URL
   */
  private generateSignature(filename: string, expiresAt: number): string {
    const secret = process.env.FILE_SIGNATURE_SECRET || 'default-secret-change-in-production';
    const data = `${filename}:${expiresAt}`;
    return crypto.createHmac('sha256', secret).update(data).digest('hex');
  }

  /**
   * Get mime type from file extension
   */
  private getMimeTypeFromExtension(extension: string): string {
    const mimeTypes: Record<string, string> = {
      '.pdf': 'application/pdf',
      '.jpg': 'image/jpeg',
      '.jpeg': 'image/jpeg',
      '.png': 'image/png',
      '.gif': 'image/gif',
      '.doc': 'application/msword',
      '.docx': 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
      '.xls': 'application/vnd.ms-excel',
      '.xlsx': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      '.txt': 'text/plain',
      '.csv': 'text/csv',
    };

    return mimeTypes[extension] || 'application/octet-stream';
  }

  /**
   * Ensure storage directory exists
   */
  private ensureStorageDirectory(): void {
    if (!fs.existsSync(this.STORAGE_PATH)) {
      fs.mkdirSync(this.STORAGE_PATH, { recursive: true });
      logger.info('Storage directory created', {
        path: this.STORAGE_PATH,
      });
    }
  }

  /**
   * Get storage statistics
   */
  async getStorageStats(): Promise<{
    totalFiles: number;
    totalSize: number;
    averageSize: number;
  }> {
    try {
      const files = await fs.promises.readdir(this.STORAGE_PATH);
      let totalSize = 0;

      for (const file of files) {
        const filePath = path.join(this.STORAGE_PATH, file);
        const stats = await fs.promises.stat(filePath);
        if (stats.isFile()) {
          totalSize += stats.size;
        }
      }

      return {
        totalFiles: files.length,
        totalSize,
        averageSize: files.length > 0 ? totalSize / files.length : 0,
      };
    } catch (error) {
      logger.error('Error getting storage stats:', error);
      throw error;
    }
  }
}

export default FileStorageService;
