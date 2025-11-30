import { Request, Response, NextFunction } from 'express';
import { RepositoryFactory } from '@repositories/index';
import { z } from 'zod';
import { createBadRequestError, createNotFoundError } from '@middleware/errorHandler';

// Validation schemas
const createMemberSchema = z.object({
  firstName: z.string().min(2).max(100),
  lastName: z.string().min(2).max(100),
  email: z.string().email().optional(),
  phone: z.string().min(10).max(20),
  dateOfBirth: z.string().datetime().optional(),
  address: z.string().max(500).optional(),
  city: z.string().max(100).optional(),
  state: z.string().max(100).optional(),
  country: z.string().max(100).default('Nigeria'),
  branchId: z.string().uuid().optional(),
});

const updateMemberSchema = createMemberSchema.partial();

const searchMemberSchema = z.object({
  query: z.string().optional(),
  status: z.enum(['ACTIVE', 'INACTIVE', 'SUSPENDED']).optional(),
  branchId: z.string().uuid().optional(),
  page: z.string().optional(),
  limit: z.string().optional(),
});

export class MemberController {
  private memberRepository = RepositoryFactory.getMemberRepository();

  /**
   * @swagger
   * /api/v1/members:
   *   post:
   *     summary: Register a new member
   *     tags: [Members]
   *     security:
   *       - bearerAuth: []
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             required:
   *               - firstName
   *               - lastName
   *               - phone
   *             properties:
   *               firstName:
   *                 type: string
   *               lastName:
   *                 type: string
   *               email:
   *                 type: string
   *               phone:
   *                 type: string
   *               dateOfBirth:
   *                 type: string
   *                 format: date-time
   *               address:
   *                 type: string
   *               city:
   *                 type: string
   *               state:
   *                 type: string
   *               country:
   *                 type: string
   *               branchId:
   *                 type: string
   *     responses:
   *       201:
   *         description: Member registered successfully
   *       400:
   *         description: Validation error
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async create(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const validatedData = createMemberSchema.parse(req.body);

      // Check if email already exists
      if (validatedData.email) {
        const existingMember = await this.memberRepository.findByEmail(validatedData.email);
        if (existingMember) {
          throw createBadRequestError('Email already registered');
        }
      }

      // Generate member number
      const memberNumber = await this.generateMemberNumber();

      const member = await this.memberRepository.create({
        ...validatedData,
        memberNumber,
        dateOfBirth: validatedData.dateOfBirth ? new Date(validatedData.dateOfBirth) : undefined,
      });

      res.status(201).json({
        success: true,
        data: member,
        message: 'Member registered successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/members:
   *   get:
   *     summary: List all members
   *     tags: [Members]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: query
   *         name: query
   *         schema:
   *           type: string
   *         description: Search query (name, email, member number)
   *       - in: query
   *         name: status
   *         schema:
   *           type: string
   *           enum: [ACTIVE, INACTIVE, SUSPENDED]
   *       - in: query
   *         name: branchId
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
   *         description: Members retrieved successfully
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async list(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { query, status, branchId, page = '1', limit = '20' } = searchMemberSchema.parse(req.query);

      const pageNum = parseInt(page);
      const limitNum = parseInt(limit);
      const skip = (pageNum - 1) * limitNum;

      let members;
      let total;

      if (query) {
        // Search by name, email, or member number
        members = await this.memberRepository.search(query, { skip, take: limitNum });
        total = await this.memberRepository.count({
          where: {
            OR: [
              { firstName: { contains: query, mode: 'insensitive' } },
              { lastName: { contains: query, mode: 'insensitive' } },
              { email: { contains: query, mode: 'insensitive' } },
              { memberNumber: { contains: query, mode: 'insensitive' } },
            ],
          },
        });
      } else {
        // Filter by status and/or branch
        const where: any = {};
        if (status) where.status = status;
        if (branchId) where.branchId = branchId;

        members = await this.memberRepository.findMany({
          where,
          skip,
          take: limitNum,
          include: {
            branch: true,
          },
        });

        total = await this.memberRepository.count({ where });
      }

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
   * /api/v1/members/{id}:
   *   get:
   *     summary: Get member by ID
   *     tags: [Members]
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
   *         description: Member retrieved successfully
   *       404:
   *         description: Member not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getById(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;

      const member = await this.memberRepository.findById(id, {
        include: {
          branch: true,
          accounts: true,
          loans: {
            include: {
              loanProduct: true,
            },
          },
        },
      });

      if (!member) {
        throw createNotFoundError('Member');
      }

      res.json({
        success: true,
        data: member,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/members/{id}:
   *   put:
   *     summary: Update member profile
   *     tags: [Members]
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
   *               firstName:
   *                 type: string
   *               lastName:
   *                 type: string
   *               email:
   *                 type: string
   *               phone:
   *                 type: string
   *               address:
   *                 type: string
   *               city:
   *                 type: string
   *               state:
   *                 type: string
   *     responses:
   *       200:
   *         description: Member updated successfully
   *       404:
   *         description: Member not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async update(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;
      const validatedData = updateMemberSchema.parse(req.body);

      const existingMember = await this.memberRepository.findById(id);
      if (!existingMember) {
        throw createNotFoundError('Member');
      }

      // Check if email is being changed and if it's already taken
      if (validatedData.email && validatedData.email !== existingMember.email) {
        const emailExists = await this.memberRepository.findByEmail(validatedData.email);
        if (emailExists) {
          throw createBadRequestError('Email already in use');
        }
      }

      const updatedMember = await this.memberRepository.update(id, {
        ...validatedData,
        dateOfBirth: validatedData.dateOfBirth ? new Date(validatedData.dateOfBirth) : undefined,
      });

      res.json({
        success: true,
        data: updatedMember,
        message: 'Member updated successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/members/{id}/status:
   *   patch:
   *     summary: Update member status
   *     tags: [Members]
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
   *             required:
   *               - status
   *             properties:
   *               status:
   *                 type: string
   *                 enum: [ACTIVE, INACTIVE, SUSPENDED]
   *     responses:
   *       200:
   *         description: Member status updated successfully
   *       404:
   *         description: Member not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async updateStatus(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;
      const { status } = z.object({ status: z.enum(['ACTIVE', 'INACTIVE', 'SUSPENDED']) }).parse(req.body);

      const member = await this.memberRepository.findById(id);
      if (!member) {
        throw createNotFoundError('Member');
      }

      const updatedMember = await this.memberRepository.updateStatus(id, status);

      res.json({
        success: true,
        data: updatedMember,
        message: 'Member status updated successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/members/{id}:
   *   delete:
   *     summary: Delete member (soft delete)
   *     tags: [Members]
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
   *         description: Member deleted successfully
   *       404:
   *         description: Member not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async delete(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;

      const member = await this.memberRepository.findById(id);
      if (!member) {
        throw createNotFoundError('Member');
      }

      // Soft delete
      await this.memberRepository.delete(id);

      res.json({
        success: true,
        message: 'Member deleted successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * Generate unique member number
   */
  private async generateMemberNumber(): Promise<string> {
    const prefix = 'MEM';
    const timestamp = Date.now().toString().slice(-6);
    const random = Math.floor(Math.random() * 1000).toString().padStart(3, '0');
    const memberNumber = `${prefix}${timestamp}${random}`;

    // Check if it already exists (unlikely but possible)
    const existing = await this.memberRepository.findByMemberNumber(memberNumber);
    if (existing) {
      // Recursively generate a new one
      return this.generateMemberNumber();
    }

    return memberNumber;
  }
}

export default MemberController;
