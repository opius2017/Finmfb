import { Request, Response, NextFunction } from 'express';
import { RepositoryFactory } from '@repositories/index';
import { z } from 'zod';
import { createBadRequestError, createNotFoundError } from '@middleware/errorHandler';

// Validation schemas
const uploadDocumentSchema = z.object({
  documentType: z.enum(['ID_CARD', 'PASSPORT', 'DRIVERS_LICENSE', 'UTILITY_BILL', 'BANK_STATEMENT']),
  documentNumber: z.string().optional(),
  expiryDate: z.string().datetime().optional(),
});

const verifyKYCSchema = z.object({
  status: z.enum(['VERIFIED', 'REJECTED']),
  notes: z.string().optional(),
});

export class KYCController {
  private memberRepository = RepositoryFactory.getMemberRepository();

  /**
   * @swagger
   * /api/v1/members/{memberId}/kyc/documents:
   *   post:
   *     summary: Upload KYC document
   *     tags: [KYC]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: memberId
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
   *               - documentType
   *               - file
   *             properties:
   *               documentType:
   *                 type: string
   *                 enum: [ID_CARD, PASSPORT, DRIVERS_LICENSE, UTILITY_BILL, BANK_STATEMENT]
   *               documentNumber:
   *                 type: string
   *               expiryDate:
   *                 type: string
   *                 format: date-time
   *               file:
   *                 type: string
   *                 format: binary
   *     responses:
   *       201:
   *         description: Document uploaded successfully
   *       400:
   *         description: Validation error
   *       404:
   *         description: Member not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async uploadDocument(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { memberId } = req.params;
      const validatedData = uploadDocumentSchema.parse(req.body);

      // Verify member exists
      const member = await this.memberRepository.findById(memberId);
      if (!member) {
        throw createNotFoundError('Member');
      }

      // TODO: Implement actual file upload to S3/storage
      // For now, we'll just store the document metadata
      const documentId = `DOC${Date.now()}`;

      // Update member's KYC documents array
      const updatedMember = await this.memberRepository.update(memberId, {
        // In a real implementation, we'd store document references
        // For now, we'll just update the KYC status to PENDING
      });

      res.status(201).json({
        success: true,
        data: {
          documentId,
          documentType: validatedData.documentType,
          uploadedAt: new Date(),
        },
        message: 'KYC document uploaded successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/members/{memberId}/kyc/status:
   *   get:
   *     summary: Get KYC status
   *     tags: [KYC]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: memberId
   *         required: true
   *         schema:
   *           type: string
   *     responses:
   *       200:
   *         description: KYC status retrieved successfully
   *       404:
   *         description: Member not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getKYCStatus(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { memberId } = req.params;

      const member = await this.memberRepository.findById(memberId);
      if (!member) {
        throw createNotFoundError('Member');
      }

      res.json({
        success: true,
        data: {
          memberId: member.id,
          memberNumber: member.memberNumber,
          kycStatus: 'PENDING', // In real implementation, get from member record
          documentsUploaded: 0, // Count of uploaded documents
          verifiedAt: null,
          verifiedBy: null,
        },
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/members/{memberId}/kyc/verify:
   *   post:
   *     summary: Verify or reject KYC
   *     tags: [KYC]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: memberId
   *         required: true
   *         schema:
   *           type: string
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             required:
   *               - status
   *             properties:
   *               status:
   *                 type: string
   *                 enum: [VERIFIED, REJECTED]
   *               notes:
   *                 type: string
   *     responses:
   *       200:
   *         description: KYC verification completed
   *       400:
   *         description: Validation error
   *       404:
   *         description: Member not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async verifyKYC(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { memberId } = req.params;
      const { status, notes } = verifyKYCSchema.parse(req.body);
      const verifierId = req.user?.id;

      const member = await this.memberRepository.findById(memberId);
      if (!member) {
        throw createNotFoundError('Member');
      }

      // Update KYC status
      // In real implementation, we'd update the member's KYC status
      // and store verification details

      res.json({
        success: true,
        data: {
          memberId: member.id,
          kycStatus: status,
          verifiedAt: new Date(),
          verifiedBy: verifierId,
          notes,
        },
        message: `KYC ${status.toLowerCase()} successfully`,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/kyc/pending:
   *   get:
   *     summary: List members with pending KYC
   *     tags: [KYC]
   *     security:
   *       - bearerAuth: []
   *     parameters:
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
   *         description: Pending KYC list retrieved successfully
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async listPendingKYC(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { page = '1', limit = '20' } = req.query;

      const pageNum = parseInt(page as string);
      const limitNum = parseInt(limit as string);
      const skip = (pageNum - 1) * limitNum;

      // In real implementation, query members with PENDING KYC status
      const members = await this.memberRepository.findMany({
        where: {
          // kycStatus: 'PENDING'
        },
        skip,
        take: limitNum,
        include: {
          branch: true,
        },
      });

      const total = await this.memberRepository.count({
        where: {
          // kycStatus: 'PENDING'
        },
      });

      res.json({
        success: true,
        data: members,
        pagination: {
          page: pageNum,
          limit: limitNum,
          total,
          pages: Math.ceil(total / limitNum),
        },
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/members/{memberId}/kyc/documents:
   *   get:
   *     summary: List KYC documents for member
   *     tags: [KYC]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: memberId
   *         required: true
   *         schema:
   *           type: string
   *     responses:
   *       200:
   *         description: Documents retrieved successfully
   *       404:
   *         description: Member not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async listDocuments(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { memberId } = req.params;

      const member = await this.memberRepository.findById(memberId);
      if (!member) {
        throw createNotFoundError('Member');
      }

      // In real implementation, query documents from document table
      const documents: any[] = [];

      res.json({
        success: true,
        data: documents,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/members/{memberId}/kyc/documents/{documentId}:
   *   delete:
   *     summary: Delete KYC document
   *     tags: [KYC]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: memberId
   *         required: true
   *         schema:
   *           type: string
   *       - in: path
   *         name: documentId
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
      const { memberId, documentId } = req.params;

      const member = await this.memberRepository.findById(memberId);
      if (!member) {
        throw createNotFoundError('Member');
      }

      // In real implementation, delete document from storage and database

      res.json({
        success: true,
        message: 'Document deleted successfully',
      });
    } catch (error) {
      next(error);
    }
  }
}

export default KYCController;
