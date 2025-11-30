import { executeInTransaction } from '@config/database';
import { createBadRequestError, createNotFoundError } from '@middleware/errorHandler';
import { FileStorageService } from './FileStorageService';

export interface DocumentUploadData {
  name: string;
  description?: string;
  category: string;
  entityType: string;
  entityId: string;
  file: {
    buffer: Buffer;
    originalName: string;
    mimeType: string;
    size: number;
  };
  tags?: string[];
  uploadedBy: string;
}

export interface DocumentUpdateData {
  name?: string;
  description?: string;
  category?: string;
  tags?: string[];;
}

export interface DocumentSearchParams {
  entityType?: string;
  entityId?: string;
  category?: string;
  tags?: string[];
  search?: string;
  page?: number;
  limit?: number;
}

export class DocumentService {
  private fileStorageService: FileStorageService;

  constructor() {
    this.fileStorageService = new FileStorageService();
  }

  /**
   * Upload document
   */
  async uploadDocument(data: DocumentUploadData) {
    // Validate file
    this.validateFile(data.file);

    // Upload file to storage
    const storedFile = await this.fileStorageService.uploadFile(
      {
        buffer: data.file.buffer,
        originalName: data.file.originalName,
        mimeType: data.file.mimeType,
        size: data.file.size,
      },
      {
        entityType: data.entityType,
        entityId: data.entityId,
        category: data.category,
      }
    );

    // Create document record
    const document = await executeInTransaction(async (prisma) => {
      return prisma.document.create({
        data: {
          name: data.name,
          description: data.description,
          category: data.category,
          entityType: data.entityType,
          entityId: data.entityId,
          filename: storedFile.key,
          originalName: data.file.originalName,
          mimeType: data.file.mimeType,
          size: data.file.size,
          url: storedFile.url,
          tags: data.tags || [],
          currentVersion: 1,
          uploadedBy: data.uploadedBy,
        },
      });
    });

    // Create initial version
    await executeInTransaction(async (prisma) => {
      await prisma.documentVersion.create({
        data: {
          documentId: document.id,
          versionNumber: 1,
          filename: storedFile.key,
          originalName: data.file.originalName,
          mimeType: data.file.mimeType,
          size: data.file.size,
          url: storedFile.url,
          changeDescription: 'Initial upload',
          uploadedBy: data.uploadedBy,
        },
      });
    });

    return document;
  }

  /**
   * Get document by ID
   */
  async getDocumentById(id: string) {
    const document = await executeInTransaction(async (prisma) => {
      return prisma.document.findUnique({
        where: { id, isDeleted: false },
        include: {
          versions: {
            orderBy: { versionNumber: 'desc' },
          },
        },
      });
    });

    if (!document) {
      throw createNotFoundError('Document');
    }

    return document;
  }

  /**
   * Download document
   */
  async downloadDocument(id: string) {
    const document = await this.getDocumentById(id);

    // Generate signed URL for download
    const downloadUrl = await this.fileStorageService.generateSignedUrl(
      document.filename,
      3600 // 1 hour expiry
    );

    return {
      document,
      downloadUrl,
    };
  }

  /**
   * Update document metadata
   */
  async updateDocument(id: string, data: DocumentUpdateData) {
    const document = await this.getDocumentById(id);

    const updatedDocument = await executeInTransaction(async (prisma) => {
      return prisma.document.update({
        where: { id },
        data: {
          name: data.name || document.name,
          description: data.description !== undefined ? data.description : document.description,
          category: data.category || document.category,
          tags: data.tags || document.tags,
        },
      });
    });

    return updatedDocument;
  }

  /**
   * Upload new version
   */
  async uploadNewVersion(
    documentId: string,
    file: {
      buffer: Buffer;
      originalName: string;
      mimeType: string;
      size: number;
    },
    changeDescription: string,
    uploadedBy: string
  ) {
    const document = await this.getDocumentById(documentId);

    // Validate file
    this.validateFile(file);

    // Upload new file version
    const storedFile = await this.fileStorageService.uploadFile(
      file,
      {
        entityType: document.entityType,
        entityId: document.entityId,
        category: document.category,
        version: document.currentVersion + 1,
      }
    );

    // Update document and create version
    const result = await executeInTransaction(async (prisma) => {
      const newVersion = document.currentVersion + 1;

      // Update document
      const updatedDocument = await prisma.document.update({
        where: { id: documentId },
        data: {
          filename: storedFile.key,
          originalName: file.originalName,
          mimeType: file.mimeType,
          size: file.size,
          url: storedFile.url,
          currentVersion: newVersion,
        },
      });

      // Create version record
      const version = await prisma.documentVersion.create({
        data: {
          documentId,
          versionNumber: newVersion,
          filename: storedFile.key,
          originalName: file.originalName,
          mimeType: file.mimeType,
          size: file.size,
          url: storedFile.url,
          changeDescription,
          uploadedBy,
        },
      });

      return { document: updatedDocument, version };
    });

    return result;
  }

  /**
   * Get document versions
   */
  async getDocumentVersions(documentId: string) {
    const document = await this.getDocumentById(documentId);

    const versions = await executeInTransaction(async (prisma) => {
      return prisma.documentVersion.findMany({
        where: { documentId },
        orderBy: { versionNumber: 'desc' },
      });
    });

    return {
      document,
      versions,
    };
  }

  /**
   * Download specific version
   */
  async downloadVersion(documentId: string, versionNumber: number) {
    const version = await executeInTransaction(async (prisma) => {
      return prisma.documentVersion.findFirst({
        where: {
          documentId,
          versionNumber,
        },
      });
    });

    if (!version) {
      throw createNotFoundError('Document version');
    }

    // Generate signed URL
    const downloadUrl = await this.fileStorageService.generateSignedUrl(
      version.filename,
      3600
    );

    return {
      version,
      downloadUrl,
    };
  }

  /**
   * Delete document (soft delete)
   */
  async deleteDocument(id: string, deletedBy: string) {
    const document = await this.getDocumentById(id);

    const deletedDocument = await executeInTransaction(async (prisma) => {
      return prisma.document.update({
        where: { id },
        data: {
          isDeleted: true,
          deletedAt: new Date(),
          deletedBy,
        },
      });
    });

    return deletedDocument;
  }

  /**
   * Permanently delete document
   */
  async permanentlyDeleteDocument(id: string) {
    const document = await executeInTransaction(async (prisma) => {
      return prisma.document.findUnique({
        where: { id },
        include: {
          versions: true,
        },
      });
    });

    if (!document) {
      throw createNotFoundError('Document');
    }

    // Delete all file versions from storage
    for (const version of document.versions) {
      await this.fileStorageService.deleteFile(version.filename);
    }

    // Delete from database
    await executeInTransaction(async (prisma) => {
      await prisma.documentVersion.deleteMany({
        where: { documentId: id },
      });

      await prisma.document.delete({
        where: { id },
      });
    });

    return { success: true };
  }

  /**
   * Search documents
   */
  async searchDocuments(params: DocumentSearchParams) {
    const {
      entityType,
      entityId,
      category,
      tags,
      search,
      page = 1,
      limit = 20,
    } = params;

    const where: any = {
      isDeleted: false,
      ...(entityType && { entityType }),
      ...(entityId && { entityId }),
      ...(category && { category }),
      ...(tags && tags.length > 0 && {
        tags: {
          hasSome: tags,
        },
      }),
      ...(search && {
        OR: [
          { name: { contains: search, mode: 'insensitive' } },
          { description: { contains: search, mode: 'insensitive' } },
          { originalName: { contains: search, mode: 'insensitive' } },
        ],
      }),
    };

    const [documents, total] = await Promise.all([
      executeInTransaction(async (prisma) => {
        return prisma.document.findMany({
          where,
          include: {
            versions: {
              orderBy: { versionNumber: 'desc' },
              take: 1,
            },
          },
          orderBy: { createdAt: 'desc' },
          skip: (page - 1) * limit,
          take: limit,
        });
      }),
      executeInTransaction(async (prisma) => {
        return prisma.document.count({ where });
      }),
    ]);

    return {
      data: documents,
      pagination: {
        page,
        limit,
        total,
        totalPages: Math.ceil(total / limit),
      },
    };
  }

  /**
   * Get documents by entity
   */
  async getDocumentsByEntity(entityType: string, entityId: string) {
    const documents = await executeInTransaction(async (prisma) => {
      return prisma.document.findMany({
        where: {
          entityType,
          entityId,
          isDeleted: false,
        },
        include: {
          versions: {
            orderBy: { versionNumber: 'desc' },
            take: 1,
          },
        },
        orderBy: { createdAt: 'desc' },
      });
    });

    return documents;
  }

  /**
   * Get documents by category
   */
  async getDocumentsByCategory(category: string) {
    const documents = await executeInTransaction(async (prisma) => {
      return prisma.document.findMany({
        where: {
          category,
          isDeleted: false,
        },
        orderBy: { createdAt: 'desc' },
      });
    });

    return documents;
  }

  /**
   * Validate file
   */
  private validateFile(file: { size: number; mimeType: string }) {
    // Max file size: 10MB
    const MAX_FILE_SIZE = 10 * 1024 * 1024;
    if (file.size > MAX_FILE_SIZE) {
      throw createBadRequestError('File size exceeds maximum limit of 10MB');
    }

    // Allowed file types
    const ALLOWED_TYPES = [
      'application/pdf',
      'image/jpeg',
      'image/png',
      'image/gif',
      'application/msword',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
      'application/vnd.ms-excel',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      'text/plain',
      'text/csv',
    ];

    if (!ALLOWED_TYPES.includes(file.mimeType)) {
      throw createBadRequestError('File type not allowed');
    }
  }
}

export default DocumentService;
