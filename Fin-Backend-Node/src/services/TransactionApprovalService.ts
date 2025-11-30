import { getPrismaClient } from '@config/database';
import { logger } from '@utils/logger';
import { Prisma } from '@prisma/client';

/**
 * Approval request data
 */
export interface CreateApprovalRequestData {
  transactionId: string;
  requestedBy: string;
  reason?: string;
  metadata?: Record<string, any>;
}

/**
 * Approval decision data
 */
export interface ApprovalDecisionData {
  approvalId: string;
  decision: 'APPROVED' | 'REJECTED';
  comment?: string;
  approvedBy: string;
}

/**
 * Approval level configuration
 */
export interface ApprovalLevel {
  level: number;
  minAmount: number;
  maxAmount: number | null;
  requiredApprovers: number;
  approverRoles: string[];
}

export class TransactionApprovalService {
  private prisma = getPrismaClient();

  // Approval level configuration
  private readonly APPROVAL_LEVELS: ApprovalLevel[] = [
    {
      level: 1,
      minAmount: 0,
      maxAmount: 50000,
      requiredApprovers: 1,
      approverRoles: ['MANAGER', 'ADMIN'],
    },
    {
      level: 2,
      minAmount: 50001,
      maxAmount: 200000,
      requiredApprovers: 2,
      approverRoles: ['MANAGER', 'ADMIN'],
    },
    {
      level: 3,
      minAmount: 200001,
      maxAmount: null,
      requiredApprovers: 3,
      approverRoles: ['ADMIN', 'DIRECTOR'],
    },
  ];

  /**
   * Create approval request for transaction
   */
  async createApprovalRequest(data: CreateApprovalRequestData) {
    try {
      logger.info('Creating approval request', {
        transactionId: data.transactionId,
      });

      // Get transaction
      const transaction = await this.prisma.transaction.findUnique({
        where: { id: data.transactionId },
      });

      if (!transaction) {
        throw new Error('Transaction not found');
      }

      if (transaction.status !== 'PENDING') {
        throw new Error('Transaction is not pending approval');
      }

      // Determine approval level
      const approvalLevel = this.determineApprovalLevel(Number(transaction.amount));

      // Create approval request
      const approvalRequest = await this.prisma.approvalRequest.create({
        data: {
          transactionId: data.transactionId,
          requestedBy: data.requestedBy,
          status: 'PENDING',
          approvalLevel: approvalLevel.level,
          requiredApprovers: approvalLevel.requiredApprovers,
          approverRoles: approvalLevel.approverRoles,
          reason: data.reason,
          metadata: data.metadata as Prisma.JsonObject,
        },
      });

      logger.info('Approval request created', {
        approvalRequestId: approvalRequest.id,
        level: approvalLevel.level,
      });

      return approvalRequest;
    } catch (error) {
      logger.error('Error creating approval request:', error);
      throw error;
    }
  }

  /**
   * Process approval decision
   */
  async processApprovalDecision(data: ApprovalDecisionData) {
    try {
      logger.info('Processing approval decision', {
        approvalId: data.approvalId,
        decision: data.decision,
      });

      // Get approval request
      const approvalRequest = await this.prisma.approvalRequest.findUnique({
        where: { id: data.approvalId },
        include: {
          transaction: true,
          approvals: true,
        },
      });

      if (!approvalRequest) {
        throw new Error('Approval request not found');
      }

      if (approvalRequest.status !== 'PENDING') {
        throw new Error('Approval request is not pending');
      }

      // Verify approver has required role
      const approver = await this.prisma.user.findUnique({
        where: { id: data.approvedBy },
        include: {
          roles: true,
        },
      });

      if (!approver) {
        throw new Error('Approver not found');
      }

      const hasRequiredRole = approver.roles.some(role =>
        approvalRequest.approverRoles.includes(role.name)
      );

      if (!hasRequiredRole) {
        throw new Error('Approver does not have required role');
      }

      // Check if approver already approved
      const existingApproval = approvalRequest.approvals.find(
        a => a.approvedBy === data.approvedBy
      );

      if (existingApproval) {
        throw new Error('Approver has already provided a decision');
      }

      // Create approval record
      const result = await this.prisma.$transaction(async (tx) => {
        // Create approval
        const approval = await tx.approval.create({
          data: {
            approvalRequestId: data.approvalId,
            approvedBy: data.approvedBy,
            decision: data.decision,
            comment: data.comment,
            approvedAt: new Date(),
          },
        });

        // If rejected, update approval request and transaction
        if (data.decision === 'REJECTED') {
          await tx.approvalRequest.update({
            where: { id: data.approvalId },
            data: {
              status: 'REJECTED',
              completedAt: new Date(),
            },
          });

          await tx.transaction.update({
            where: { id: approvalRequest.transactionId },
            data: {
              status: 'FAILED',
              metadata: {
                ...(approvalRequest.transaction.metadata as object),
                rejectionReason: data.comment,
                rejectedBy: data.approvedBy,
                rejectedAt: new Date().toISOString(),
              } as Prisma.JsonObject,
            },
          });

          return { approval, status: 'REJECTED' as const };
        }

        // Count approvals
        const approvalCount = approvalRequest.approvals.filter(
          a => a.decision === 'APPROVED'
        ).length + 1; // +1 for current approval

        // Check if all required approvals are met
        if (approvalCount >= approvalRequest.requiredApprovers) {
          // Update approval request
          await tx.approvalRequest.update({
            where: { id: data.approvalId },
            data: {
              status: 'APPROVED',
              completedAt: new Date(),
            },
          });

          // Execute transaction
          await this.executeTransaction(
            approvalRequest.transactionId,
            data.approvedBy,
            tx
          );

          return { approval, status: 'APPROVED' as const };
        }

        return { approval, status: 'PENDING' as const };
      });

      logger.info('Approval decision processed', {
        approvalId: data.approvalId,
        status: result.status,
      });

      return result;
    } catch (error) {
      logger.error('Error processing approval decision:', error);
      throw error;
    }
  }

  /**
   * Execute approved transaction
   */
  private async executeTransaction(
    transactionId: string,
    approvedBy: string,
    tx: Prisma.TransactionClient
  ) {
    // Get transaction
    const transaction = await tx.transaction.findUnique({
      where: { id: transactionId },
      include: {
        account: true,
      },
    });

    if (!transaction) {
      throw new Error('Transaction not found');
    }

    // Update account balance based on transaction type
    if (transaction.type === 'DEBIT') {
      await tx.account.update({
        where: { id: transaction.accountId },
        data: {
          balance: {
            increment: transaction.amount,
          },
          updatedAt: new Date(),
        },
      });
    } else {
      await tx.account.update({
        where: { id: transaction.accountId },
        data: {
          balance: {
            decrement: transaction.amount,
          },
          updatedAt: new Date(),
        },
      });
    }

    // Update transaction status
    await tx.transaction.update({
      where: { id: transactionId },
      data: {
        status: 'COMPLETED',
        metadata: {
          ...(transaction.metadata as object),
          approvedBy,
          approvedAt: new Date().toISOString(),
        } as Prisma.JsonObject,
      },
    });

    // Create audit log
    await tx.auditLog.create({
      data: {
        userId: approvedBy,
        action: 'TRANSACTION_APPROVE',
        entityType: 'Transaction',
        entityId: transactionId,
        changes: {
          status: 'COMPLETED',
        } as Prisma.JsonObject,
        ipAddress: '',
        userAgent: '',
      },
    });
  }

  /**
   * Get approval request by ID
   */
  async getApprovalRequest(id: string) {
    try {
      const approvalRequest = await this.prisma.approvalRequest.findUnique({
        where: { id },
        include: {
          transaction: {
            include: {
              account: {
                include: {
                  member: true,
                },
              },
            },
          },
          approvals: {
            include: {
              approver: {
                select: {
                  id: true,
                  email: true,
                  firstName: true,
                  lastName: true,
                },
              },
            },
          },
        },
      });

      if (!approvalRequest) {
        throw new Error('Approval request not found');
      }

      return approvalRequest;
    } catch (error) {
      logger.error('Error getting approval request:', error);
      throw error;
    }
  }

  /**
   * Get pending approval requests
   */
  async getPendingApprovalRequests(userId: string, page: number = 1, limit: number = 20) {
    try {
      // Get user roles
      const user = await this.prisma.user.findUnique({
        where: { id: userId },
        include: {
          roles: true,
        },
      });

      if (!user) {
        throw new Error('User not found');
      }

      const userRoles = user.roles.map(r => r.name);

      // Get pending approval requests where user has required role
      const where: Prisma.ApprovalRequestWhereInput = {
        status: 'PENDING',
        approverRoles: {
          hasSome: userRoles,
        },
        // Exclude requests where user already approved
        approvals: {
          none: {
            approvedBy: userId,
          },
        },
      };

      const [total, requests] = await Promise.all([
        this.prisma.approvalRequest.count({ where }),
        this.prisma.approvalRequest.findMany({
          where,
          include: {
            transaction: {
              include: {
                account: {
                  include: {
                    member: true,
                  },
                },
              },
            },
            approvals: {
              include: {
                approver: {
                  select: {
                    id: true,
                    email: true,
                    firstName: true,
                    lastName: true,
                  },
                },
              },
            },
          },
          orderBy: {
            createdAt: 'desc',
          },
          skip: (page - 1) * limit,
          take: limit,
        }),
      ]);

      return {
        data: requests,
        pagination: {
          page,
          limit,
          total,
          totalPages: Math.ceil(total / limit),
        },
      };
    } catch (error) {
      logger.error('Error getting pending approval requests:', error);
      throw error;
    }
  }

  /**
   * Get approval history for transaction
   */
  async getApprovalHistory(transactionId: string) {
    try {
      const approvalRequests = await this.prisma.approvalRequest.findMany({
        where: { transactionId },
        include: {
          approvals: {
            include: {
              approver: {
                select: {
                  id: true,
                  email: true,
                  firstName: true,
                  lastName: true,
                },
              },
            },
            orderBy: {
              approvedAt: 'asc',
            },
          },
        },
        orderBy: {
          createdAt: 'desc',
        },
      });

      return approvalRequests;
    } catch (error) {
      logger.error('Error getting approval history:', error);
      throw error;
    }
  }

  /**
   * Determine approval level based on amount
   */
  private determineApprovalLevel(amount: number): ApprovalLevel {
    for (const level of this.APPROVAL_LEVELS) {
      if (amount >= level.minAmount && (level.maxAmount === null || amount <= level.maxAmount)) {
        return level;
      }
    }

    // Default to highest level
    return this.APPROVAL_LEVELS[this.APPROVAL_LEVELS.length - 1];
  }

  /**
   * Check if transaction requires approval
   */
  async requiresApproval(amount: number): Promise<{
    requiresApproval: boolean;
    level?: number;
    requiredApprovers?: number;
    approverRoles?: string[];
  }> {
    // Transactions below threshold don't require approval
    const APPROVAL_THRESHOLD = 10000;

    if (amount < APPROVAL_THRESHOLD) {
      return {
        requiresApproval: false,
      };
    }

    const level = this.determineApprovalLevel(amount);

    return {
      requiresApproval: true,
      level: level.level,
      requiredApprovers: level.requiredApprovers,
      approverRoles: level.approverRoles,
    };
  }

  /**
   * Cancel approval request
   */
  async cancelApprovalRequest(approvalId: string, userId: string, reason: string) {
    try {
      logger.info('Cancelling approval request', {
        approvalId,
        userId,
      });

      const approvalRequest = await this.prisma.approvalRequest.findUnique({
        where: { id: approvalId },
      });

      if (!approvalRequest) {
        throw new Error('Approval request not found');
      }

      if (approvalRequest.status !== 'PENDING') {
        throw new Error('Only pending approval requests can be cancelled');
      }

      if (approvalRequest.requestedBy !== userId) {
        throw new Error('Only the requester can cancel the approval request');
      }

      await this.prisma.$transaction(async (tx) => {
        // Update approval request
        await tx.approvalRequest.update({
          where: { id: approvalId },
          data: {
            status: 'CANCELLED',
            completedAt: new Date(),
            metadata: {
              ...(approvalRequest.metadata as object),
              cancellationReason: reason,
              cancelledBy: userId,
              cancelledAt: new Date().toISOString(),
            } as Prisma.JsonObject,
          },
        });

        // Update transaction
        await tx.transaction.update({
          where: { id: approvalRequest.transactionId },
          data: {
            status: 'FAILED',
            metadata: {
              cancellationReason: reason,
              cancelledBy: userId,
              cancelledAt: new Date().toISOString(),
            } as Prisma.JsonObject,
          },
        });
      });

      logger.info('Approval request cancelled', {
        approvalId,
      });
    } catch (error) {
      logger.error('Error cancelling approval request:', error);
      throw error;
    }
  }
}

export default TransactionApprovalService;
