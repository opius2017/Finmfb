import { Request, Response, NextFunction } from 'express';
import { z } from 'zod';
import { DocumentService } from '@services/DocumentService';
import multer from 'multer';

const upload = multer({
  storage: multer.memoryStorage(),
  limits: {
    fileSize: 10 * 1024 * 1024, // 10MB
  },
});

const documentUploadSchema = z.object({
  name: z.string().min(1),
  description: z.string().optional(),
  category: z.string().min(1),
  entityType: z.string().min(1),
  entityId: z.string().uuid(),
  tags: z.array(z.string()).optional(),
});

const documentUpdateSchema = z.object({
  name: z.string().min(1).optional(),
  description: z.string().optional(),
  category: z.string().min(1).optional(),
  tags: z.array(z.string()).optional(),
});

const versionUploadSchema = z.object({
  changeDescription: z.string().min(1),
});

export class DocumentController {
  private documentService = new DocumentService();

  /**
   * Multer middleware for file upload
   */
  uploadMiddleware = upload.single('file');

  /**
   * @swagger
   * /api/v1/documents:
   *   post:
   *     summary: Upload document
   *     tags: [Documents]
   *     security:
   *       - bearerAuth: []
   *     requestBody:
   *       required: true
   *       content:
   *         multipart/form-data:
   *           schema:
   *             type: object
   *             required:
   *               - file
   *               - name
   *               - category
   *               - entityType
   *               - entityId
   *             properties:
   *               file:
   *                 type: string
   *                 format: binary
   *               name:
   *                 type: string
   *               description:
   *                 type: string
   *               category:
   *                 type: string
   *               entityType:
   *                 type: string
   *               entityId:
   *                 type: string
   *               tags:
   *                 type: array
   *                 items:
   *                   type: string
   *     responses:
   *       201:
   *         description: Document uploaded successfully
   *       400:
   *         description: Validation error
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async uploadDocument(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      if (!req.file) {
        return res.status(400).json({
          success: false,
          error: {
            code: 'BAD_REQUEST',
            message: 'No file uploaded',
          },
        });
      }

      const validatedData = documentUploadSchema.parse({
        ...req.body,
        tags: req.body.tags ? JSON.parse(req.body.tags) : undefined,
      });

      const uploadedBy = req.user?.id;

      const document = await this.documentService.uploadDocument({
        ...validatedData,
        file: {
          buffer: req.file.buffer,
          originalName: req.file.originalname,
          mimeType: req.file.mimetype,
          size: req.file.size,
        },
        uploadedBy: uploadedBy!,
      });

      res.status(201).json({
        success: true,
        data: document,
        message: 'Document uploaded successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/documents/{id}:
   *   get:
   *     summary: Get document by ID
   *     tags: [Documents]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: id
   *         required: true
   *         schema:
   *           type: string
   *     responses:
   *       200:
   *         description: Document retrieved successfully
   *       404:
   *         description: Document not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getDocument(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;

      const document = await this.documentService.getDocumentById(id);

      res.json({
        success: true,
        data: document,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/documents/{id}/download:
   *   get:
   *     summary: Download document
   *     tags: [Documents]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: id
   *         required: true
   *         schema:
   *           type: string
   *     responses:
   *       200:
   *         description: Download URL generated
   *       404:
   *         description: Document not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async downloadDocument(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;

      const result = await this.documentService.downloadDocument(id);

      res.json({
        success: true,
        data: result,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/documents/{id}:
   *   patch:
   *     summary: Update document metadata
   *     tags: [Documents]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: id
   *         required: true
   *         schema:
   *           type: string
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             properties:
   *               name:
   *                 type: string
   *               description:
   *                 type: string
   *               category:
   *                 type: string
   *               tags:
   *                 type: array
   *                 items:
   *                   type: string
   *     responses:
   *       200:
   *         description: Document updated successfully
   *       400:
   *         description: Validation error
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async updateDocument(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;
      const validatedData = documentUpdateSchema.parse(req.body);

      const document = await this.documentService.updateDocument(id, validatedData);

      res.json({
        success: true,
        data: document,
        message: 'Document updated successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/documents/{id}/versions:
   *   post:
   *     summary: Upload new document version
   *     tags: [Documents]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: id
   *         required: true
   *         schema:
   *           type: string
   *     requestBody:
   *       required: true
   *       content:
   *         multipart/form-data:
   *           schema:
   *             type: object
   *             required:
   *               - file
   *               - changeDescription
   *             properties:
   *               file:
   *                 type: string
   *                 format: binary
   *               changeDescription:
   *                 type: string
   *     responses:
   *       201:
   *         description: New version uploaded successfully
   *       400:
   *         description: Validation error
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async uploadNewVersion(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;

      if (!req.file) {
        return res.status(400).json({
          success: false,
          error: {
            code: 'BAD_REQUEST',
            message: 'No file uploaded',
          },
        });
      }

      const { changeDescription } = versionUploadSchema.parse(req.body);
      const uploadedBy = req.user?.id;

      const result = await this.documentService.uploadNewVersion(
        id,
        {
          buffer: req.file.buffer,
          originalName: req.file.originalname,
          mimeType: req.file.mimetype,
          size: req.file.size,
        },
        changeDescription,
        uploadedBy!
      );

      res.status(201).json({
        success: true,
        data: result,
        message: 'New version uploaded successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/documents/{id}/versions:
   *   get:
   *     summary: Get document versions
   *     tags: [Documents]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: id
   *         required: true
   *         schema:
   *           type: string
   *     responses:
   *       200:
   *         description: Versions retrieved successfully
   *       404:
   *         description: Document not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getVersions(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;

      const result = await this.documentService.getDocumentVersions(id);

      res.json({
        success: true,
        data: result,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/documents/{id}/versions/{versionNumber}/download:
   *   get:
   *     summary: Download specific version
   *     tags: [Documents]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: id
   *         required: true
   *         schema:
   *           type: string
   *       - in: path
   *         name: versionNumber
   *         required: true
   *         schema:
   *           type: integer
   *     responses:
   *       200:
   *         description: Download URL generated
   *       404:
   *         description: Version not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async downloadVersion(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id, versionNumber } = req.params;

      const result = await this.documentService.downloadVersion(
        id,
        parseInt(versionNumber)
      );

      res.json({
        success: true,
        data: result,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/documents/{id}:
   *   delete:
   *     summary: Delete document (soft delete)
   *     tags: [Documents]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: id
   *         required: true
   *         schema:
   *           type: string
   *     responses:
   *       200:
   *         description: Document deleted successfully
   *       404:
   *         description: Document not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async deleteDocument(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;
      const deletedBy = req.user?.id;

      const document = await this.documentService.deleteDocument(id, deletedBy!);

      res.json({
        success: true,
        data: document,
        message: 'Document deleted successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/documents/search:
   *   get:
   *     summary: Search documents
   *     tags: [Documents]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: query
   *         name: entityType
   *         schema:
   *           type: string
   *       - in: query
   *         name: entityId
   *         schema:
   *           type: string
   *       - in: query
   *         name: category
   *         schema:
   *           type: string
   *       - in: query
   *         name: search
   *         schema:
   *           type: string
   *       - in: query
   *         name: page
   *         schema:
   *           type: integer
   *           default: 1
   *       - in: query
   *         name: limit
   *         schema:
   *           type: integer
   *           default: 20
   *     responses:
   *       200:
   *         description: Documents retrieved successfully
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async searchDocuments(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const {
        entityType,
        entityId,
        category,
        tags,
        search,
        page,
        limit,
      } = req.query;

      const result = await this.documentService.searchDocuments({
        entityType: entityType as string,
        entityId: entityId as string,
        category: category as string,
        tags: tags ? (tags as string).split(',') : undefined,
        search: search as string,
        page: page ? parseInt(page as string) : undefined,
        limit: limit ? parseInt(limit as string) : undefined,
      });

      res.json({
        success: true,
        data: result.data,
        pagination: result.pagination,
      });
    } catch (error) {
      next(error);
    }
  }
}

export default DocumentController;
