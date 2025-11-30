import { getPrismaClient } from '@config/database';
import { logger } from '@utils/logger';
import { Prisma } from '@prisma/client';
import { CalculationEngine } from './CalculationEngine';

/**
 * Loan payment data
 */
export interface RecordLoanPaymentData {
  loanId: string;
  amount: number;
  paymentDate: Date;
  paymentMethod: 'CASH' | 'BANK_TRANSFER' | 'ACCOUNT_DEBIT' | 'DEDUCTION';
  accountId?: string;
  reference?: string;
  notes?: string;
  userId: string;
}

/**
 * Payment allocation
 */
interface PaymentAllocation {
  scheduleId: string;
  principal: number;
  interest: number;
  penalty: number;
  totalPayment: number;
}

export class LoanRepaymentService {
  private prisma = getPrismaClient();
  private calculationEngine = new CalculationEngine();

  /**
   * Record loan payment
   */
  async recordLoanPayment(data: RecordLoanPaymentData) {
    try {
      logger.info('Recording loan payment', {
        loanId: data.loanId,
        amount: data.amount,
      });

      // Get loan with schedules
      const loan = await this.prisma.loan.findUnique({
        where: { id: data.loanId },
        include: {
          loanProduct: true,
          schedules: {
            where: {
              isPaid: false,
            },
            orderBy: {
              dueDate: 'asc',
            },
          },
        },
      });

      if (!loan) {
        throw new Error('Loan not found');
      }

      // Validate loan status
      if (loan.status !== 'DISBURSED' && loan.status !== 'ACTIVE') {
        throw new Error('Loan is not active');
      }

      // Validate payment amount
      if (data.amount <= 0) {
        throw new Error('Payment amount must be positive');
      }

      if (data.amount > Number(loan.outstandingBalance)) {
        throw new Error('Payment amount exceeds outstanding balance');
      }

      // Allocate payment to schedules
      const allocations = this.allocatePayment(
        data.amount,
        loan.schedules,
        data.paymentDate,
        Number(loan.loanProduct.penaltyRate)
      );

      // Process payment
      const result = await this.prisma.$transaction(async (tx) => {
        // Create loan payment record
        const payment = await tx.loanPayment.create({
          data: {
            loanId: data.loanId,
            amount: data.amount,
            paymentDate: data.paymentDate,
            paymentMethod: data.paymentMethod,
            reference: data.reference || this.generateReference('LOAN-PAY'),
            notes: data.notes,
            metadata: {
              allocations,
            } as Prisma.JsonObject,
            createdBy: data.userId,
          },
        });

        // Update loan schedules
        for (const allocation of allocations) {
          const schedule = loan.schedules.find(s => s.id === allocation.scheduleId);
          if (!schedule) continue;

          const totalPaid = allocation.totalPayment;
          const scheduleTotalPayment = Number(schedule.totalPayment) + allocation.penalty;

          // Check if schedule is fully paid
          const isPaid = totalPaid >= scheduleTotalPayment;

          await tx.loanSchedule.update({
            where: { id: allocation.scheduleId },
            data: {
              paidAmount: {
                increment: totalPaid,
              },
              isPaid,
              paidDate: isPaid ? data.paymentDate : null,
            },
          });
        }

        // Update loan outstanding balance
        const newOutstandingBalance = Number(loan.outstandingBalance) - data.amount;
        const isFullyPaid = newOutstandingBalance <= 0.01; // Allow for rounding

        await tx.loan.update({
          where: { id: data.loanId },
          data: {
            outstandingBalance: Math.max(0, newOutstandingBalance),
            status: isFullyPaid ? 'CLOSED' : 'ACTIVE',
            closedDate: isFullyPaid ? data.paymentDate : null,
          },
        });

        // If payment method is account debit, create transaction
        if (data.paymentMethod === 'ACCOUNT_DEBIT' && data.accountId) {
          // Check account balance
          const account = await tx.account.findUnique({
            where: { id: data.accountId },
          });

          if (!account) {
            throw new Error('Account not found');
          }

          if (Number(account.balance) < data.amount) {
            throw new Error('Insufficient account balance');
          }

          await tx.transaction.create({
            data: {
              accountId: data.accountId,
              type: 'CREDIT',
              amount: data.amount,
              description: `Loan repayment - ${loan.purpose}`,
              reference: data.reference || this.generateReference('LOAN-PAY'),
              status: 'COMPLETED',
              metadata: {
                loanId: data.loanId,
                paymentDate: data.paymentDate.toISOString(),
              } as Prisma.JsonObject,
              createdBy: data.userId,
            },
          });

          // Update account balance
          await tx.account.update({
            where: { id: data.accountId },
            data: {
              balance: {
                decrement: data.amount,
              },
              updatedAt: new Date(),
            },
          });
        }

        // Create audit log
        await tx.auditLog.create({
          data: {
            userId: data.userId,
            action: 'LOAN_PAYMENT',
            entityType: 'Loan',
            entityId: data.loanId,
            changes: {
              amount: data.amount,
              paymentDate: data.paymentDate.toISOString(),
              paymentMethod: data.paymentMethod,
              newOutstandingBalance: Math.max(0, newOutstandingBalance),
            } as Prisma.JsonObject,
            ipAddress: '',
            userAgent: '',
          },
        });

        return {
          payment,
          allocations,
          newOutstandingBalance: Math.max(0, newOutstandingBalance),
          isFullyPaid,
        };
      });

      logger.info('Loan payment recorded successfully', {
        loanId: data.loanId,
        paymentId: result.payment.id,
        isFullyPaid: result.isFullyPaid,
      });

      return result;
    } catch (error) {
      logger.error('Error recording loan payment:', error);
      throw error;
    }
  }

  /**
   * Allocate payment to schedules
   */
  private allocatePayment(
    paymentAmount: number,
    schedules: any[],
    paymentDate: Date,
    penaltyRate: number
  ): PaymentAllocation[] {
    const allocations: PaymentAllocation[] = [];
    let remainingAmount = paymentAmount;

    for (const schedule of schedules) {
      if (remainingAmount <= 0) break;

      const scheduleTotalPayment = Number(schedule.totalPayment);
      const schedulePrincipal = Number(schedule.principal);
      const scheduleInterest = Number(schedule.interest);
      const paidAmount = Number(schedule.paidAmount || 0);
      const remainingScheduleAmount = scheduleTotalPayment - paidAmount;

      // Calculate penalty if overdue
      let penalty = 0;
      if (paymentDate > schedule.dueDate) {
        const daysOverdue = Math.ceil(
          (paymentDate.getTime() - schedule.dueDate.getTime()) / (1000 * 60 * 60 * 24)
        );
        penalty = this.calculationEngine.calculateLatePenalty(
          remainingScheduleAmount,
          daysOverdue,
          penaltyRate
        );
      }

      const totalDue = remainingScheduleAmount + penalty;
      const amountToAllocate = Math.min(remainingAmount, totalDue);

      // Allocate payment: penalty first, then interest, then principal
      let allocatedPenalty = Math.min(amountToAllocate, penalty);
      let remaining = amountToAllocate - allocatedPenalty;

      const remainingInterest = scheduleInterest - (paidAmount > scheduleInterest ? scheduleInterest : paidAmount);
      let allocatedInterest = Math.min(remaining, Math.max(0, remainingInterest));
      remaining -= allocatedInterest;

      let allocatedPrincipal = remaining;

      allocations.push({
        scheduleId: schedule.id,
        principal: this.round(allocatedPrincipal),
        interest: this.round(allocatedInterest),
        penalty: this.round(allocatedPenalty),
        totalPayment: this.round(amountToAllocate),
      });

      remainingAmount -= amountToAllocate;
    }

    return allocations;
  }

  /**
   * Get loan payment history
   */
  async getLoanPaymentHistory(loanId: string) {
    try {
      const payments = await this.prisma.loanPayment.findMany({
        where: { loanId },
        orderBy: {
          paymentDate: 'desc',
        },
      });

      return payments;
    } catch (error) {
      logger.error('Error getting loan payment history:', error);
      throw error;
    }
  }

  /**
   * Get overdue loans
   */
  async getOverdueLoans(memberId?: string) {
    try {
      const where: Prisma.LoanWhereInput = {
        status: {
          in: ['DISBURSED', 'ACTIVE'],
        },
        ...(memberId && { memberId }),
        schedules: {
          some: {
            isPaid: false,
            dueDate: {
              lt: new Date(),
            },
          },
        },
      };

      const overdueLoans = await this.prisma.loan.findMany({
        where,
        include: {
          member: true,
          loanProduct: true,
          schedules: {
            where: {
              isPaid: false,
              dueDate: {
                lt: new Date(),
              },
            },
            orderBy: {
              dueDate: 'asc',
            },
          },
        },
      });

      return overdueLoans.map(loan => {
        const oldestOverdue = loan.schedules[0];
        const daysOverdue = oldestOverdue
          ? Math.ceil(
              (new Date().getTime() - oldestOverdue.dueDate.getTime()) / (1000 * 60 * 60 * 24)
            )
          : 0;

        const totalOverdueAmount = loan.schedules.reduce(
          (sum, schedule) => sum + Number(schedule.totalPayment) - Number(schedule.paidAmount || 0),
          0
        );

        return {
          ...loan,
          daysOverdue,
          totalOverdueAmount: this.round(totalOverdueAmount),
          overdueSchedulesCount: loan.schedules.length,
        };
      });
    } catch (error) {
      logger.error('Error getting overdue loans:', error);
      throw error;
    }
  }

  /**
   * Calculate early payoff amount
   */
  async calculateEarlyPayoff(loanId: string, payoffDate: Date) {
    try {
      const loan = await this.prisma.loan.findUnique({
        where: { id: loanId },
        include: {
          loanProduct: true,
          schedules: {
            where: {
              isPaid: false,
            },
          },
        },
      });

      if (!loan) {
        throw new Error('Loan not found');
      }

      // Calculate remaining principal
      const remainingPrincipal = loan.schedules.reduce(
        (sum, schedule) => sum + Number(schedule.principal),
        0
      );

      // Calculate accrued interest up to payoff date
      const accruedInterest = loan.schedules
        .filter(schedule => schedule.dueDate <= payoffDate)
        .reduce((sum, schedule) => sum + Number(schedule.interest), 0);

      // Calculate any penalties
      const overdueSchedules = loan.schedules.filter(
        schedule => schedule.dueDate < payoffDate
      );

      let totalPenalty = 0;
      for (const schedule of overdueSchedules) {
        const daysOverdue = Math.ceil(
          (payoffDate.getTime() - schedule.dueDate.getTime()) / (1000 * 60 * 60 * 24)
        );
        if (daysOverdue > 0) {
          const penalty = this.calculationEngine.calculateLatePenalty(
            Number(schedule.totalPayment),
            daysOverdue,
            Number(loan.loanProduct.penaltyRate)
          );
          totalPenalty += penalty;
        }
      }

      const totalPayoffAmount = remainingPrincipal + accruedInterest + totalPenalty;

      // Calculate interest saved
      const totalScheduledInterest = loan.schedules.reduce(
        (sum, schedule) => sum + Number(schedule.interest),
        0
      );
      const interestSaved = totalScheduledInterest - accruedInterest;

      return {
        remainingPrincipal: this.round(remainingPrincipal),
        accruedInterest: this.round(accruedInterest),
        totalPenalty: this.round(totalPenalty),
        totalPayoffAmount: this.round(totalPayoffAmount),
        interestSaved: this.round(interestSaved),
        payoffDate,
      };
    } catch (error) {
      logger.error('Error calculating early payoff:', error);
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

  /**
   * Round to 2 decimal places
   */
  private round(value: number): number {
    return Math.round(value * 100) / 100;
  }
}

export default LoanRepaymentService;
