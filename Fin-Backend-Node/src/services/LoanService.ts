import { executeInTransaction } from '@config/database';
import { createBadRequestError, createNotFoundError } from '@middleware/errorHandler';
import { CalculationService, LoanParams } from './CalculationService';
import { WorkflowService } from './WorkflowService';
import { NotificationDispatcher } from './NotificationDispatcher';

export interface LoanApplicationData {
  memberId: string;
  loanProductId: string;
  requestedAmount: number;
  termMonths: number;
  purpose: string;
  collateralDescription?: string;
  guarantors: Array<{
    memberId: string;
    guaranteedAmount: number;
  }>;
  userId: string;
}

export interface LoanApprovalData {
  loanId: string;
  approvedAmount: number;
  approverId: string;
  comment?: string;
}

export interface LoanDisbursementData {
  loanId: string;
  disbursementAccountId: string;
  disbursedBy: string;
  notes?: string;
}

export interface LoanPaymentData {
  loanId: string;
  amount: number;
  paymentMethod: string;
  reference?: string;
  notes?: string;
  paidBy: string;
}

export class LoanService {
  private calculationService: CalculationService;
  private workflowService: WorkflowService;
  private notificationDispatcher: NotificationDispatcher;

  constructor() {
    this.calculationService = new CalculationService();
    this.workflowService = new WorkflowService();
    this.notificationDispatcher = new NotificationDispatcher();
  }

  /**
   * Submit loan application
   */
  async submitLoanApplication(data: LoanApplicationData) {
    // Validate member exists
    const member = await executeInTransaction(async (prisma) => {
      return prisma.member.findUnique({
        where: { id: data.memberId },
        include: {
          accounts: true,
          loans: {
            where: {
              status: {
                in: ['PENDING', 'APPROVED', 'DISBURSED', 'ACTIVE'],
              },
            },
          },
        },
      });
    });

    if (!member) {
      throw createNotFoundError('Member');
    }

    if (member.status !== 'ACTIVE') {
      throw createBadRequestError('Member account is not active');
    }

    // Validate loan product
    const loanProduct = await executeInTransaction(async (prisma) => {
      return prisma.loanProduct.findUnique({
        where: { id: data.loanProductId },
      });
    });

    if (!loanProduct) {
      throw createNotFoundError('Loan product');
    }

    if (!loanProduct.isActive) {
      throw createBadRequestError('Loan product is not active');
    }

    // Validate loan amount
    if (data.requestedAmount < Number(loanProduct.minAmount)) {
      throw createBadRequestError(`Minimum loan amount is ₦${loanProduct.minAmount}`);
    }

    if (data.requestedAmount > Number(loanProduct.maxAmount)) {
      throw createBadRequestError(`Maximum loan amount is ₦${loanProduct.maxAmount}`);
    }

    // Validate term
    if (data.termMonths < loanProduct.minTermMonths) {
      throw createBadRequestError(`Minimum term is ${loanProduct.minTermMonths} months`);
    }

    if (data.termMonths > loanProduct.maxTermMonths) {
      throw createBadRequestError(`Maximum term is ${loanProduct.maxTermMonths} months`);
    }

    // Check for existing active loans
    if (member.loans.length > 0) {
      throw createBadRequestError('Member has existing active loans');
    }

    // Validate guarantors
    if (data.guarantors.length === 0) {
      throw createBadRequestError('At least one guarantor is required');
    }

    // Create loan application
    const loan = await executeInTransaction(async (prisma) => {
      const newLoan = await prisma.loan.create({
        data: {
          memberId: data.memberId,
          loanProductId: data.loanProductId,
          requestedAmount: data.requestedAmount,
          interestRate: loanProduct.interestRate,
          termMonths: data.termMonths,
          purpose: data.purpose,
          collateralDescription: data.collateralDescription,
          status: 'PENDING',
          createdBy: data.userId,
        },
        include: {
          member: true,
          loanProduct: true,
        },
      });

      // Create guarantors
      for (const guarantor of data.guarantors) {
        await prisma.guarantor.create({
          data: {
            loanId: newLoan.id,
            memberId: guarantor.memberId,
            guaranteedAmount: guarantor.guaranteedAmount,
            status: 'PENDING',
          },
        });
      }

      return newLoan;
    });

    // Start approval workflow
    await this.workflowService.startWorkflow(
      'loan_approval',
      'loan',
      loan.id,
      data.userId,
      {
        amount: data.requestedAmount,
        loanProductId: data.loanProductId,
        memberId: data.memberId,
      }
    );

    return loan;
  }

  /**
   * Check loan eligibility
   */
  async checkLoanEligibility(memberId: string, requestedAmount: number) {
    const member = await executeInTransaction(async (prisma) => {
      return prisma.member.findUnique({
        where: { id: memberId },
        include: {
          accounts: true,
          loans: {
            where: {
              status: {
                in: ['ACTIVE', 'DISBURSED'],
              },
            },
          },
        },
      });
    });

    if (!member) {
      throw createNotFoundError('Member');
    }

    const eligibility = {
      isEligible: true,
      reasons: [] as string[],
      maxLoanAmount: 0,
      savingsBalance: 0,
      existingLoans: member.loans.length,
    };

    // Check member status
    if (member.status !== 'ACTIVE') {
      eligibility.isEligible = false;
      eligibility.reasons.push('Member account is not active');
    }

    // Check for existing loans
    if (member.loans.length > 0) {
      eligibility.isEligible = false;
      eligibility.reasons.push('Member has existing active loans');
    }

    // Calculate savings balance
    const savingsAccounts = member.accounts.filter(a => a.type === 'SAVINGS' && a.status === 'ACTIVE');
    eligibility.savingsBalance = savingsAccounts.reduce((sum, acc) => sum + Number(acc.balance), 0);

    // Calculate max loan amount (e.g., 3x savings balance)
    eligibility.maxLoanAmount = eligibility.savingsBalance * 3;

    // Check if requested amount exceeds max
    if (requestedAmount > eligibility.maxLoanAmount) {
      eligibility.isEligible = false;
      eligibility.reasons.push(`Requested amount exceeds maximum eligible amount of ₦${eligibility.maxLoanAmount}`);
    }

    return eligibility;
  }

  /**
   * Approve loan
   */
  async approveLoan(data: LoanApprovalData) {
    const loan = await executeInTransaction(async (prisma) => {
      return prisma.loan.findUnique({
        where: { id: data.loanId },
        include: {
          member: true,
          loanProduct: true,
        },
      });
    });

    if (!loan) {
      throw createNotFoundError('Loan');
    }

    if (loan.status !== 'PENDING') {
      throw createBadRequestError('Only pending loans can be approved');
    }

    // Validate approved amount
    if (data.approvedAmount > Number(loan.loanProduct.maxAmount)) {
      throw createBadRequestError('Approved amount exceeds maximum loan amount');
    }

    // Update loan
    const updatedLoan = await executeInTransaction(async (prisma) => {
      return prisma.loan.update({
        where: { id: data.loanId },
        data: {
          status: 'APPROVED',
          approvedAmount: data.approvedAmount,
          approvalDate: new Date(),
        },
        include: {
          member: true,
          loanProduct: true,
        },
      });
    });

    // Send notification
    await this.notificationDispatcher.sendApprovalGranted(
      data.loanId,
      'Loan Application',
      'loan',
      data.loanId,
      loan.member.id,
      data.approverId
    );

    return updatedLoan;
  }

  /**
   * Reject loan
   */
  async rejectLoan(loanId: string, reason: string, rejectedBy: string) {
    const loan = await executeInTransaction(async (prisma) => {
      return prisma.loan.findUnique({
        where: { id: loanId },
        include: {
          member: true,
        },
      });
    });

    if (!loan) {
      throw createNotFoundError('Loan');
    }

    if (loan.status !== 'PENDING') {
      throw createBadRequestError('Only pending loans can be rejected');
    }

    // Update loan
    const updatedLoan = await executeInTransaction(async (prisma) => {
      return prisma.loan.update({
        where: { id: loanId },
        data: {
          status: 'REJECTED',
          metadata: {
            rejectionReason: reason,
            rejectedBy,
            rejectedAt: new Date().toISOString(),
          },
        },
      });
    });

    // Send notification
    await this.notificationDispatcher.sendApprovalRejected(
      loanId,
      'Loan Application',
      'loan',
      loanId,
      loan.member.id,
      rejectedBy,
      reason
    );

    return updatedLoan;
  }

  /**
   * Disburse loan
   */
  async disburseLoan(data: LoanDisbursementData) {
    const loan = await executeInTransaction(async (prisma) => {
      return prisma.loan.findUnique({
        where: { id: data.loanId },
        include: {
          member: true,
          loanProduct: true,
        },
      });
    });

    if (!loan) {
      throw createNotFoundError('Loan');
    }

    if (loan.status !== 'APPROVED') {
      throw createBadRequestError('Only approved loans can be disbursed');
    }

    // Validate disbursement account
    const account = await executeInTransaction(async (prisma) => {
      return prisma.account.findUnique({
        where: { id: data.disbursementAccountId },
      });
    });

    if (!account) {
      throw createNotFoundError('Disbursement account');
    }

    if (account.memberId !== loan.memberId) {
      throw createBadRequestError('Account does not belong to loan member');
    }

    // Calculate loan schedule
    const loanParams: LoanParams = {
      principal: Number(loan.approvedAmount),
      interestRate: Number(loan.interestRate) * 100, // Convert to percentage
      termMonths: loan.termMonths,
      method: loan.loanProduct.calculationMethod as 'reducing_balance' | 'flat_rate',
      startDate: new Date(),
      paymentFrequency: 'monthly',
    };

    const schedule = this.calculationService.calculateLoanSchedule(loanParams);

    // Disburse loan
    const result = await executeInTransaction(async (prisma) => {
      // Update loan
      const updatedLoan = await prisma.loan.update({
        where: { id: data.loanId },
        data: {
          status: 'DISBURSED',
          disbursedAmount: loan.approvedAmount,
          outstandingBalance: loan.approvedAmount,
          disbursementDate: new Date(),
        },
      });

      // Create loan schedule
      for (const item of schedule) {
        await prisma.loanSchedule.create({
          data: {
            loanId: data.loanId,
            paymentNumber: item.paymentNumber,
            dueDate: item.dueDate,
            principal: item.principal,
            interest: item.interest,
            totalPayment: item.totalPayment,
            balance: item.balance,
          },
        });
      }

      // Credit member account
      await prisma.account.update({
        where: { id: data.disbursementAccountId },
        data: {
          balance: {
            increment: loan.approvedAmount,
          },
        },
      });

      // Create transaction record
      await prisma.transaction.create({
        data: {
          accountId: data.disbursementAccountId,
          type: 'DEBIT',
          amount: loan.approvedAmount,
          description: `Loan disbursement - ${loan.loanProduct.name}`,
          reference: `LOAN-DISB-${Date.now()}`,
          status: 'COMPLETED',
          metadata: {
            loanId: data.loanId,
            disbursementType: 'LOAN',
          },
          createdBy: data.disbursedBy,
        },
      });

      return updatedLoan;
    });

    return result;
  }

  /**
   * Record loan payment
   */
  async recordLoanPayment(data: LoanPaymentData) {
    const loan = await executeInTransaction(async (prisma) => {
      return prisma.loan.findUnique({
        where: { id: data.loanId },
        include: {
          schedules: {
            where: { isPaid: false },
            orderBy: { paymentNumber: 'asc' },
          },
        },
      });
    });

    if (!loan) {
      throw createNotFoundError('Loan');
    }

    if (!['DISBURSED', 'ACTIVE'].includes(loan.status)) {
      throw createBadRequestError('Loan is not active');
    }

    if (data.amount <= 0) {
      throw createBadRequestError('Payment amount must be greater than zero');
    }

    // Record payment
    const result = await executeInTransaction(async (prisma) => {
      // Create payment record
      const payment = await prisma.loanPayment.create({
        data: {
          loanId: data.loanId,
          amount: data.amount,
          paymentDate: new Date(),
          paymentMethod: data.paymentMethod,
          reference: data.reference || `LOAN-PAY-${Date.now()}`,
          notes: data.notes,
          createdBy: data.paidBy,
        },
      });

      // Apply payment to schedules
      let remainingAmount = data.amount;
      const updatedSchedules = [];

      for (const schedule of loan.schedules) {
        if (remainingAmount <= 0) break;

        const amountDue = Number(schedule.totalPayment) - Number(schedule.paidAmount);
        const paymentToApply = Math.min(remainingAmount, amountDue);

        const updatedSchedule = await prisma.loanSchedule.update({
          where: { id: schedule.id },
          data: {
            paidAmount: {
              increment: paymentToApply,
            },
            isPaid: Number(schedule.paidAmount) + paymentToApply >= Number(schedule.totalPayment),
            paidDate: Number(schedule.paidAmount) + paymentToApply >= Number(schedule.totalPayment) ? new Date() : null,
          },
        });

        updatedSchedules.push(updatedSchedule);
        remainingAmount -= paymentToApply;
      }

      // Update loan outstanding balance
      const newOutstandingBalance = Number(loan.outstandingBalance) - data.amount;
      const updatedLoan = await prisma.loan.update({
        where: { id: data.loanId },
        data: {
          outstandingBalance: Math.max(0, newOutstandingBalance),
          status: newOutstandingBalance <= 0 ? 'CLOSED' : 'ACTIVE',
          closedDate: newOutstandingBalance <= 0 ? new Date() : null,
        },
      });

      return {
        payment,
        loan: updatedLoan,
        updatedSchedules,
      };
    });

    return result;
  }

  /**
   * Get loan details with schedule
   */
  async getLoanDetails(loanId: string) {
    const loan = await executeInTransaction(async (prisma) => {
      return prisma.loan.findUnique({
        where: { id: loanId },
        include: {
          member: true,
          loanProduct: true,
          schedules: {
            orderBy: { paymentNumber: 'asc' },
          },
          payments: {
            orderBy: { paymentDate: 'desc' },
          },
          guarantors: {
            include: {
              member: true,
            },
          },
        },
      });
    });

    if (!loan) {
      throw createNotFoundError('Loan');
    }

    return loan;
  }

  /**
   * Get loan portfolio summary
   */
  async getLoanPortfolioSummary() {
    const summary = await executeInTransaction(async (prisma) => {
      const [
        totalLoans,
        activeLoans,
        totalDisbursed,
        totalOutstanding,
        totalCollected,
        overdueLoans,
      ] = await Promise.all([
        prisma.loan.count(),
        prisma.loan.count({
          where: {
            status: {
              in: ['DISBURSED', 'ACTIVE'],
            },
          },
        }),
        prisma.loan.aggregate({
          where: {
            status: {
              in: ['DISBURSED', 'ACTIVE', 'CLOSED'],
            },
          },
          _sum: {
            disbursedAmount: true,
          },
        }),
        prisma.loan.aggregate({
          where: {
            status: {
              in: ['DISBURSED', 'ACTIVE'],
            },
          },
          _sum: {
            outstandingBalance: true,
          },
        }),
        prisma.loanPayment.aggregate({
          _sum: {
            amount: true,
          },
        }),
        prisma.loanSchedule.count({
          where: {
            isPaid: false,
            dueDate: {
              lt: new Date(),
            },
          },
        }),
      ]);

      return {
        totalLoans,
        activeLoans,
        totalDisbursed: Number(totalDisbursed._sum.disbursedAmount || 0),
        totalOutstanding: Number(totalOutstanding._sum.outstandingBalance || 0),
        totalCollected: Number(totalCollected._sum.amount || 0),
        overdueLoans,
        collectionRate: totalDisbursed._sum.disbursedAmount 
          ? (Number(totalCollected._sum.amount || 0) / Number(totalDisbursed._sum.disbursedAmount)) * 100
          : 0,
      };
    });

    return summary;
  }

  /**
   * Get loan aging report
   */
  async getLoanAgingReport() {
    const today = new Date();
    
    const overdueSchedules = await executeInTransaction(async (prisma) => {
      return prisma.loanSchedule.findMany({
        where: {
          isPaid: false,
          dueDate: {
            lt: today,
          },
        },
        include: {
          loan: {
            include: {
              member: true,
              loanProduct: true,
            },
          },
        },
      });
    });

    // Group by aging buckets
    const agingBuckets = {
      current: { count: 0, amount: 0 },
      '1-30': { count: 0, amount: 0 },
      '31-60': { count: 0, amount: 0 },
      '61-90': { count: 0, amount: 0 },
      '90+': { count: 0, amount: 0 },
    };

    for (const schedule of overdueSchedules) {
      const daysOverdue = Math.floor((today.getTime() - schedule.dueDate.getTime()) / (1000 * 60 * 60 * 24));
      const amountDue = Number(schedule.totalPayment) - Number(schedule.paidAmount);

      if (daysOverdue <= 0) {
        agingBuckets.current.count++;
        agingBuckets.current.amount += amountDue;
      } else if (daysOverdue <= 30) {
        agingBuckets['1-30'].count++;
        agingBuckets['1-30'].amount += amountDue;
      } else if (daysOverdue <= 60) {
        agingBuckets['31-60'].count++;
        agingBuckets['31-60'].amount += amountDue;
      } else if (daysOverdue <= 90) {
        agingBuckets['61-90'].count++;
        agingBuckets['61-90'].amount += amountDue;
      } else {
        agingBuckets['90+'].count++;
        agingBuckets['90+'].amount += amountDue;
      }
    }

    return agingBuckets;
  }
}

export default LoanService;
