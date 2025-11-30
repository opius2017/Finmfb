import { getPrismaClient } from '@config/database';
import { logger } from '@utils/logger';
import { Prisma } from '@prisma/client';
import { CalculationEngine, LoanCalculationParams } from './CalculationEngine';

/**
 * Loan disbursement data
 */
export interface DisburseLoanData {
  loanId: string;
  disbursementAmount: number;
  disbursementDate: Date;
  disbursementMethod: 'CASH' | 'BANK_TRANSFER' | 'ACCOUNT_CREDIT';
  accountId?: string;
  reference?: string;
  notes?: string;
  userId: string;
}

export class LoanDisbursementService {
  private prisma = getPrismaClient();
  private calculationEngine = new CalculationEngine();

  /**
   * Disburse approved loan
   */
  async disburseLoan(data: DisburseLoanData) {
    try {
      logger.info('Disbursing loan', {
        loanId: data.loanId,
        amount: data.disbursementAmount,
      });

      // Get loan with details
      const loan = await this.prisma.loan.findUnique({
        where: { id: data.loanId },
        include: {
          member: true,
          loanProduct: true,
          guarantors: true,
        },
      });

      if (!loan) {
        throw new Error('Loan not found');
      }

      // Validate loan status
      if (loan.status !== 'APPROVED') {
        throw new Error('Loan must be approved before disbursement');
      }

      // Validate disbursement amount
      if (data.disbursementAmount > Number(loan.approvedAmount)) {
        throw new Error('Disbursement amount exceeds approved amount');
      }

      if (data.disbursementAmount <= 0) {
        throw new Error('Disbursement amount must be positive');
      }

      // Validate guarantors are approved
      const unapprovedGuarantors = loan.guarantors.filter(
        g => g.status !== 'APPROVED'
      );

      if (unapprovedGuarantors.length > 0) {
        throw new Error('All guarantors must be approved before disbursement');
      }

      // Disburse loan
      const result = await this.prisma.$transaction(async (tx) => {
        // Update loan
        const updatedLoan = await tx.loan.update({
          where: { id: data.loanId },
          data: {
            disbursedAmount: data.disbursementAmount,
            outstandingBalance: data.disbursementAmount,
            status: 'DISBURSED',
            disbursementDate: data.disbursementDate,
            metadata: {
              ...(loan.metadata as object),
              disbursementMethod: data.disbursementMethod,
              disbursementReference: data.reference,
              disbursementNotes: data.notes,
              disbursedBy: data.userId,
            } as Prisma.JsonObject,
          },
        });

        // Generate loan schedule
        const schedule = this.calculationEngine.calculateLoanSchedule({
          principal: data.disbursementAmount,
          interestRate: Number(loan.interestRate),
          termMonths: loan.termMonths,
          method: loan.loanProduct.calculationMethod as 'reducing_balance' | 'flat_rate',
          startDate: data.disbursementDate,
          paymentFrequency: 'monthly',
        });

        // Create loan schedules
        await tx.loanSchedule.createMany({
          data: schedule.map(item => ({
            loanId: data.loanId,
            paymentNumber: item.paymentNumber,
            dueDate: item.dueDate,
            principal: item.principal,
            interest: item.interest,
            totalPayment: item.totalPayment,
            balance: item.balance,
            isPaid: false,
          })),
        });

        // If disbursement method is account credit, create transaction
        if (data.disbursementMethod === 'ACCOUNT_CREDIT' && data.accountId) {
          await tx.transaction.create({
            data: {
              accountId: data.accountId,
              type: 'DEBIT',
              amount: data.disbursementAmount,
              description: `Loan disbursement - ${loan.purpose}`,
              reference: data.reference || this.generateReference('LOAN-DISB'),
              status: 'COMPLETED',
              metadata: {
                loanId: data.loanId,
                disbursementDate: data.disbursementDate.toISOString(),
              } as Prisma.JsonObject,
              createdBy: data.userId,
            },
          });

          // Update account balance
          await tx.account.update({
            where: { id: data.accountId },
            data: {
              balance: {
                increment: data.disbursementAmount,
              },
              updatedAt: new Date(),
            },
          });
        }

        // Create audit log
        await tx.auditLog.create({
          data: {
            userId: data.userId,
            action: 'LOAN_DISBURSE',
            entityType: 'Loan',
            entityId: data.loanId,
            changes: {
              disbursedAmount: data.disbursementAmount,
              disbursementDate: data.disbursementDate.toISOString(),
              disbursementMethod: data.disbursementMethod,
            } as Prisma.JsonObject,
            ipAddress: '',
            userAgent: '',
          },
        });

        return {
          loan: updatedLoan,
          schedule,
        };
      });

      logger.info('Loan disbursed successfully', {
        loanId: data.loanId,
        scheduleCount: result.schedule.length,
      });

      return result;
    } catch (error) {
      logger.error('Error disbursing loan:', error);
      throw error;
    }
  }

  /**
   * Get loan schedule
   */
  async getLoanSchedule(loanId: string) {
    try {
      const schedules = await this.prisma.loanSchedule.findMany({
        where: { loanId },
        orderBy: {
          paymentNumber: 'asc',
        },
      });

      return schedules;
    } catch (error) {
      logger.error('Error getting loan schedule:', error);
      throw error;
    }
  }

  /**
   * Get loan schedule summary
   */
  async getLoanScheduleSummary(loanId: string) {
    try {
      const [totalSchedules, paidSchedules, overdueSchedules, totalPayments] = await Promise.all([
        this.prisma.loanSchedule.count({
          where: { loanId },
        }),
        this.prisma.loanSchedule.count({
          where: {
            loanId,
            isPaid: true,
          },
        }),
        this.prisma.loanSchedule.count({
          where: {
            loanId,
            isPaid: false,
            dueDate: {
              lt: new Date(),
            },
          },
        }),
        this.prisma.loanSchedule.aggregate({
          where: { loanId },
          _sum: {
            totalPayment: true,
            principal: true,
            interest: true,
          },
        }),
      ]);

      return {
        totalSchedules,
        paidSchedules,
        overdueSchedules,
        remainingSchedules: totalSchedules - paidSchedules,
        totalPayment: Number(totalPayments._sum.totalPayment || 0),
        totalPrincipal: Number(totalPayments._sum.principal || 0),
        totalInterest: Number(totalPayments._sum.interest || 0),
      };
    } catch (error) {
      logger.error('Error getting loan schedule summary:', error);
      throw error;
    }
  }

  /**
   * Generate reference number
   */
  private generateReference(prefix: string): string {
    const timestamp = Date.now();
    const random = Math.floor(Math.random() * 10000).toString().padStart(4, '0');
    return `${prefix}-${timestamp}-${random}`;
  }
}

export default LoanDisbursementService;
