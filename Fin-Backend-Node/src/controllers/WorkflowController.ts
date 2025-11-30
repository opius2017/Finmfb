import { Request, Response, NextFunction } from 'express';
import { z } from 'zod';
import { WorkflowService } from '@services/WorkflowService';
import { RBACService } from '@services/RBACService';

const startWorkflowSchema = z.object({
  workflowType: z.string(),
  entityType: z.string(),
  entityId: z.string().uuid(),
  data: z.record(z.any()),
});

const approveStepSchema = z.object({
  comment: z.string().optional(),
});

const rejectStepSchema = z.object({
  reason: z.string().min(10),
});

export class WorkflowController {
  private workflowService = new WorkflowService();
  private rbacService = new RBACService();

  /**
   * @swagger
   * /api/v1/workflows/start:
   *   post:
   *     summary: Start a new workflow
   *     tags: [Workflows]
   *     security:
   *       - bearerAuth: []
   *     requestBody:
   *       required: true
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             required:
   *               - workflowType
   *               - entityType
   *               - entityId
   *               - data
   *             properties:
   *               workflowType:
   *                 type: string
   *               entityType:
   *                 type: string
   *               entityId:
   *                 type: string
   *               data:
   *                 type: object
   *     responses:
   *       201:
   *         description: Workflow started successfully
   *       400:
   *         description: Validation error
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async startWorkflow(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const validatedData = startWorkflowSchema.parse(req.body);
      const userId = req.user?.id!;

      const instance = await this.workflowService.startWorkflow(
        validatedData.workflowType,
        validatedData.entityType,
        validatedData.entityId,
        userId,
        validatedData.data
      );

      res.status(201).json({
        success: true,
        data: instance,
        message: 'Workflow started successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/workflows/{id}/approve:
   *   post:
   *     summary: Approve workflow step
   *     tags: [Workflows]
   *     security:
   *       - bearerAuth: []
   *     parameters:
   *       - in: path
   *         name: id
   *         required: true
   *         schema:
   *           type: string
   *     requestBody:
   *       content:
   *         application/json:
   *           schema:
   *             type: object
   *             properties:
   *               comment:
   *                 type: string
   *     responses:
   *       200:
   *         description: Step approved successfully
   *       404:
   *         description: Workflow not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async approveStep(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;
      const { comment } = approveStepSchema.parse(req.body);
      const userId = req.user?.id!;

      const instance = await this.workflowService.approveStep(id, userId, comment);

      res.json({
        success: true,
        data: instance,
        message: 'Step approved successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/workflows/{id}/reject:
   *   post:
   *     summary: Reject workflow step
   *     tags: [Workflows]
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
   *               - reason
   *             properties:
   *               reason:
   *                 type: string
   *     responses:
   *       200:
   *         description: Step rejected successfully
   *       404:
   *         description: Workflow not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async rejectStep(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;
      const { reason } = rejectStepSchema.parse(req.body);
      const userId = req.user?.id!;

      const instance = await this.workflowService.rejectStep(id, userId, reason);

      res.json({
        success: true,
        data: instance,
        message: 'Step rejected successfully',
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/workflows/{id}/status:
   *   get:
   *     summary: Get workflow status
   *     tags: [Workflows]
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
   *         description: Workflow status retrieved successfully
   *       404:
   *         description: Workflow not found
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getStatus(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const { id } = req.params;

      const instance = await this.workflowService.getWorkflowStatus(id);

      res.json({
        success: true,
        data: instance,
      });
    } catch (error) {
      next(error);
    }
  }

  /**
   * @swagger
   * /api/v1/workflows/pending:
   *   get:
   *     summary: Get pending approvals for current user
   *     tags: [Workflows]
   *     security:
   *       - bearerAuth: []
   *     responses:
   *       200:
   *         description: Pending approvals retrieved successfully
   *       401:
   *         $ref: '#/components/responses/UnauthorizedError'
   */
  async getPendingApprovals(req: Request, res: Response, next: NextFunction): Promise<void> {
    try {
      const userId = req.user?.id!;
      const userRoles = await this.rbacService.getUserRoles(userId);

      const pendingApprovals = await this.workflowService.getPendingApprovals(userId, userRoles);

      res.json({
        success: true,
        data: pendingApprovals,
      });
    } catch (error) {
      next(error);
    }
  }
}

export default WorkflowController;
