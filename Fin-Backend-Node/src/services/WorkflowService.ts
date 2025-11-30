import { executeInTransaction } from '@config/database';
import { createBadRequestError, createNotFoundError } from '@middleware/errorHandler';

export interface WorkflowStep {
  id: string;
  name: string;
  type: 'approval' | 'notification' | 'calculation' | 'integration';
  approvers?: {
    roles?: string[];
    userIds?: string[];
    minApprovals?: number;
  };
  nextSteps: string[];
  timeout?: number; // in hours
  onTimeout?: 'escalate' | 'reject' | 'approve';
}

export interface WorkflowDefinition {
  id: string;
  name: string;
  type: string;
  description?: string;
  steps: WorkflowStep[];
  rules: {
    amountThresholds?: Array<{
      min: number;
      max: number;
      requiredApprovers: string[];
    }>;
    conditions?: Array<{
      field: string;
      operator: 'eq' | 'gt' | 'lt' | 'gte' | 'lte' | 'in';
      value: any;
      action: string;
    }>;
  };
  isActive: boolean;
}

export interface WorkflowInstance {
  id: string;
  workflowDefinitionId: string;
  entityType: string;
  entityId: string;
  currentStepId: string;
  status: 'pending' | 'in_progress' | 'completed' | 'rejected' | 'timeout';
  initiatorId: string;
  data: Record<string, any>;
  history: WorkflowHistoryEntry[];
  createdAt: Date;
  updatedAt: Date;
}

export interface WorkflowHistoryEntry {
  stepId: string;
  stepName: string;
  action: 'started' | 'approved' | 'rejected' | 'timeout' | 'completed';
  actorId?: string;
  comment?: string;
  timestamp: Date;
  metadata?: Record<string, any>;
}

export class WorkflowService {
  /**
   * Create a workflow definition
   */
  async createWorkflowDefinition(definition: Omit<WorkflowDefinition, 'id'>): Promise<WorkflowDefinition> {
    // Validate workflow definition
    this.validateWorkflowDefinition(definition);

    const workflow = await executeInTransaction(async (prisma) => {
      return prisma.workflow.create({
        data: {
          name: definition.name,
          type: definition.type,
          definition: definition as any,
          isActive: definition.isActive,
        },
      });
    });

    return {
      id: workflow.id,
      ...definition,
    };
  }

  /**
   * Start a workflow instance
   */
  async startWorkflow(
    workflowType: string,
    entityType: string,
    entityId: string,
    initiatorId: string,
    data: Record<string, any>
  ): Promise<WorkflowInstance> {
    // Get workflow definition
    const workflowDef = await this.getWorkflowDefinitionByType(workflowType);
    
    if (!workflowDef || !workflowDef.isActive) {
      throw createNotFoundError('Active workflow definition');
    }

    // Determine starting step based on rules
    const startingStep = this.determineStartingStep(workflowDef, data);

    // Create workflow instance
    const instance = await executeInTransaction(async (prisma) => {
      return prisma.approvalRequest.create({
        data: {
          transactionId: entityId,
          requestedBy: initiatorId,
          status: 'PENDING',
          approvalLevel: 1,
          requiredApprovers: startingStep.approvers?.minApprovals || 1,
          approverRoles: startingStep.approvers?.roles || [],
          metadata: {
            workflowDefinitionId: workflowDef.id,
            workflowType,
            entityType,
            currentStepId: startingStep.id,
            data,
            history: [{
              stepId: startingStep.id,
              stepName: startingStep.name,
              action: 'started',
              timestamp: new Date(),
            }],
          },
        },
      });
    });

    return {
      id: instance.id,
      workflowDefinitionId: workflowDef.id,
      entityType,
      entityId,
      currentStepId: startingStep.id,
      status: 'in_progress',
      initiatorId,
      data,
      history: [{
        stepId: startingStep.id,
        stepName: startingStep.name,
        action: 'started',
        timestamp: new Date(),
      }],
      createdAt: instance.createdAt,
      updatedAt: instance.updatedAt,
    };
  }

  /**
   * Approve a workflow step
   */
  async approveStep(
    workflowInstanceId: string,
    approverId: string,
    comment?: string
  ): Promise<WorkflowInstance> {
    const instance = await this.getWorkflowInstance(workflowInstanceId);
    
    if (instance.status !== 'in_progress') {
      throw createBadRequestError('Workflow is not in progress');
    }

    const workflowDef = await this.getWorkflowDefinition(instance.workflowDefinitionId);
    const currentStep = workflowDef.steps.find(s => s.id === instance.currentStepId);

    if (!currentStep) {
      throw createNotFoundError('Current workflow step');
    }

    // Record approval
    await executeInTransaction(async (prisma) => {
      await prisma.approval.create({
        data: {
          approvalRequestId: workflowInstanceId,
          approvedBy: approverId,
          decision: 'APPROVED',
          comment,
        },
      });
    });

    // Check if step is complete
    const approvals = await this.getStepApprovals(workflowInstanceId);
    const requiredApprovals = currentStep.approvers?.minApprovals || 1;

    if (approvals.length >= requiredApprovals) {
      // Move to next step
      return this.moveToNextStep(instance, workflowDef, 'approved', approverId, comment);
    }

    // Update history
    instance.history.push({
      stepId: currentStep.id,
      stepName: currentStep.name,
      action: 'approved',
      actorId: approverId,
      comment,
      timestamp: new Date(),
    });

    return instance;
  }

  /**
   * Reject a workflow step
   */
  async rejectStep(
    workflowInstanceId: string,
    approverId: string,
    reason: string
  ): Promise<WorkflowInstance> {
    const instance = await this.getWorkflowInstance(workflowInstanceId);
    
    if (instance.status !== 'in_progress') {
      throw createBadRequestError('Workflow is not in progress');
    }

    const workflowDef = await this.getWorkflowDefinition(instance.workflowDefinitionId);
    const currentStep = workflowDef.steps.find(s => s.id === instance.currentStepId);

    if (!currentStep) {
      throw createNotFoundError('Current workflow step');
    }

    // Record rejection
    await executeInTransaction(async (prisma) => {
      await prisma.approval.create({
        data: {
          approvalRequestId: workflowInstanceId,
          approvedBy: approverId,
          decision: 'REJECTED',
          comment: reason,
        },
      });

      // Update approval request status
      await prisma.approvalRequest.update({
        where: { id: workflowInstanceId },
        data: {
          status: 'REJECTED',
          completedAt: new Date(),
        },
      });
    });

    instance.status = 'rejected';
    instance.history.push({
      stepId: currentStep.id,
      stepName: currentStep.name,
      action: 'rejected',
      actorId: approverId,
      comment: reason,
      timestamp: new Date(),
    });

    return instance;
  }

  /**
   * Get workflow instance status
   */
  async getWorkflowStatus(workflowInstanceId: string): Promise<WorkflowInstance> {
    return this.getWorkflowInstance(workflowInstanceId);
  }

  /**
   * Get pending approvals for a user
   */
  async getPendingApprovals(userId: string, userRoles: string[]): Promise<WorkflowInstance[]> {
    const approvalRequests = await executeInTransaction(async (prisma) => {
      return prisma.approvalRequest.findMany({
        where: {
          status: 'PENDING',
          OR: [
            {
              approverRoles: {
                hasSome: userRoles,
              },
            },
          ],
        },
        include: {
          approvals: true,
        },
      });
    });

    return approvalRequests.map(req => ({
      id: req.id,
      workflowDefinitionId: (req.metadata as any)?.workflowDefinitionId || '',
      entityType: (req.metadata as any)?.entityType || '',
      entityId: req.transactionId || '',
      currentStepId: (req.metadata as any)?.currentStepId || '',
      status: 'in_progress' as const,
      initiatorId: req.requestedBy,
      data: (req.metadata as any)?.data || {},
      history: (req.metadata as any)?.history || [],
      createdAt: req.createdAt,
      updatedAt: req.updatedAt,
    }));
  }

  /**
   * Private helper methods
   */

  private validateWorkflowDefinition(definition: Omit<WorkflowDefinition, 'id'>): void {
    if (!definition.name || !definition.type) {
      throw createBadRequestError('Workflow name and type are required');
    }

    if (!definition.steps || definition.steps.length === 0) {
      throw createBadRequestError('Workflow must have at least one step');
    }

    // Validate step references
    const stepIds = new Set(definition.steps.map(s => s.id));
    for (const step of definition.steps) {
      for (const nextStepId of step.nextSteps) {
        if (nextStepId !== 'END' && !stepIds.has(nextStepId)) {
          throw createBadRequestError(`Invalid next step reference: ${nextStepId}`);
        }
      }
    }
  }

  private async getWorkflowDefinitionByType(type: string): Promise<WorkflowDefinition | null> {
    const workflow = await executeInTransaction(async (prisma) => {
      return prisma.workflow.findFirst({
        where: {
          type,
          isActive: true,
        },
      });
    });

    if (!workflow) return null;

    return {
      id: workflow.id,
      ...(workflow.definition as any),
    };
  }

  private async getWorkflowDefinition(id: string): Promise<WorkflowDefinition> {
    const workflow = await executeInTransaction(async (prisma) => {
      return prisma.workflow.findUnique({
        where: { id },
      });
    });

    if (!workflow) {
      throw createNotFoundError('Workflow definition');
    }

    return {
      id: workflow.id,
      ...(workflow.definition as any),
    };
  }

  private async getWorkflowInstance(id: string): Promise<WorkflowInstance> {
    const approvalRequest = await executeInTransaction(async (prisma) => {
      return prisma.approvalRequest.findUnique({
        where: { id },
        include: {
          approvals: true,
        },
      });
    });

    if (!approvalRequest) {
      throw createNotFoundError('Workflow instance');
    }

    const metadata = approvalRequest.metadata as any;

    return {
      id: approvalRequest.id,
      workflowDefinitionId: metadata?.workflowDefinitionId || '',
      entityType: metadata?.entityType || '',
      entityId: approvalRequest.transactionId || '',
      currentStepId: metadata?.currentStepId || '',
      status: this.mapStatus(approvalRequest.status),
      initiatorId: approvalRequest.requestedBy,
      data: metadata?.data || {},
      history: metadata?.history || [],
      createdAt: approvalRequest.createdAt,
      updatedAt: approvalRequest.updatedAt,
    };
  }

  private async getStepApprovals(workflowInstanceId: string): Promise<any[]> {
    return executeInTransaction(async (prisma) => {
      return prisma.approval.findMany({
        where: {
          approvalRequestId: workflowInstanceId,
          decision: 'APPROVED',
        },
      });
    });
  }

  private determineStartingStep(workflowDef: WorkflowDefinition, data: Record<string, any>): WorkflowStep {
    // Apply rules to determine starting step
    if (workflowDef.rules.amountThresholds && data.amount) {
      for (const threshold of workflowDef.rules.amountThresholds) {
        if (data.amount >= threshold.min && data.amount <= threshold.max) {
          // Find step with matching approvers
          const step = workflowDef.steps[0]; // Simplified - would match based on threshold
          return step;
        }
      }
    }

    // Default to first step
    return workflowDef.steps[0];
  }

  private async moveToNextStep(
    instance: WorkflowInstance,
    workflowDef: WorkflowDefinition,
    action: string,
    actorId: string,
    comment?: string
  ): Promise<WorkflowInstance> {
    const currentStep = workflowDef.steps.find(s => s.id === instance.currentStepId);
    
    if (!currentStep) {
      throw createNotFoundError('Current step');
    }

    // Add to history
    instance.history.push({
      stepId: currentStep.id,
      stepName: currentStep.name,
      action: action as any,
      actorId,
      comment,
      timestamp: new Date(),
    });

    // Check if workflow is complete
    if (currentStep.nextSteps.includes('END')) {
      instance.status = 'completed';
      
      await executeInTransaction(async (prisma) => {
        await prisma.approvalRequest.update({
          where: { id: instance.id },
          data: {
            status: 'APPROVED',
            completedAt: new Date(),
          },
        });
      });

      return instance;
    }

    // Move to next step
    const nextStepId = currentStep.nextSteps[0];
    const nextStep = workflowDef.steps.find(s => s.id === nextStepId);

    if (!nextStep) {
      throw createNotFoundError('Next step');
    }

    instance.currentStepId = nextStepId;

    await executeInTransaction(async (prisma) => {
      await prisma.approvalRequest.update({
        where: { id: instance.id },
        data: {
          approvalLevel: instance.history.length + 1,
          metadata: {
            ...(instance.data as any),
            currentStepId: nextStepId,
            history: instance.history,
          },
        },
      });
    });

    return instance;
  }

  private mapStatus(status: string): 'pending' | 'in_progress' | 'completed' | 'rejected' | 'timeout' {
    switch (status) {
      case 'PENDING':
        return 'in_progress';
      case 'APPROVED':
        return 'completed';
      case 'REJECTED':
        return 'rejected';
      default:
        return 'pending';
    }
  }
}

export default WorkflowService;
