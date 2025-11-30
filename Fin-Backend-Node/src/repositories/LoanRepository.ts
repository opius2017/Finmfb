import { Loan, LoanStatus } from '@prisma/client';
import { BaseRepository } from './BaseRepository';

export class LoanRepository extends BaseRepository<Loan> {
  constructor() {
    super('loan');
  }

  /**
   * Find loan by loan number
   */
  async findByLoanNumber(loanNumber: string): Promise<Loan | null> {
    return this.model.findUnique({
      where: { loanNumber },
      include: {
        member: true,
        product: true,
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
  }

  /**
   * Find loans by member
   */
  async findByMember(memberId: string): Promise<Loan[]> {
    return this.model.findMany({
      where: { memberId },
      include: {
        product: true,
        schedules: true,
      },
      orderBy: { createdAt: 'desc' },
    });
  }

  /**
   * Find loans by status
   */
  async findByStatus(status: LoanStatus): Promise<Loan[]> {
    return this.model.findMany({
      where: { status },
      include: {
        member: true,
        product: true,
      },
    });
  }

  /**
   * Find loans by product
   */
  async findByProduct(productId: string): Promise<Loan[]> {
    return this.model.findMany({
      where: { productId },
      include: {
        member: true,
      },
    });
  }

  /**
   * Update loan status
   */
  async updateStatus(id: string, status: LoanStatus): Promise<Loan> {
    return this.model.update({
      where: { id },
      data: { status },
    });
  }

  /**
   * Update outstanding balance
   */
  async updateOutstandingBalance(id: string, balance: number): Promise<Loan> {
    return this.model.update({
      where: { id },
      data: {
        outstandingBalance: balance,
      },
    });
  }

  /**
   * Approve loan
   */
  async approveLoan(id: string): Promise<Loan> {
    return this.model.update({
      where: { id },
      data: {
        status: LoanStatus.APPROVED,
        approvalDate: new Date(),
      },
    });
  }

  /**
   * Disburse loan
   */
  async disburseLoan(id: string, maturityDate: Date): Promise<Loan> {
    return this.model.update({
      where: { id },
      data: {
        status: LoanStatus.DISBURSED,
        disbursementDate: new Date(),
        maturityDate,
      },
    });
  }

  /**
   * Get active loans
   */
  async findActiveLoans(): Promise<Loan[]> {
    return this.model.findMany({
      where: {
        status: {
          in: [LoanStatus.DISBURSED, LoanStatus.ACTIVE],
        },
      },
      include: {
        member: true,
        product: true,
      },
    });
  }

  /**
   * Get overdue loans
   */
  async findOverdueLoans(): Promise<Loan[]> {
    const today = new Date();

    return this.model.findMany({
      where: {
        status: {
          in: [LoanStatus.DISBURSED, LoanStatus.ACTIVE],
        },
        schedules: {
          some: {
            dueDate: {
              lt: today,
            },
            isPaid: false,
          },
        },
      },
      include: {
        member: true,
        product: true,
        schedules: {
          where: {
            dueDate: {
              lt: today,
            },
            isPaid: false,
          },
        },
      },
    });
  }

  /**
   * Get loan portfolio summary
   */
  async getPortfolioSummary(): Promise<{
    totalLoans: number;
    totalDisbursed: number;
    totalOutstanding: number;
    totalOverdue: number;
  }> {
    const [totalLoans, disbursedSum, outstandingSum] = await Promise.all([
      this.model.count({
        where: {
          status: {
            in: [LoanStatus.DISBURSED, LoanStatus.ACTIVE],
          },
        },
      }),
      this.model.aggregate({
        where: {
          status: {
            in: [LoanStatus.DISBURSED, LoanStatus.ACTIVE],
          },
        },
        _sum: {
          principal: true,
        },
      }),
      this.model.aggregate({
        where: {
          status: {
            in: [LoanStatus.DISBURSED, LoanStatus.ACTIVE],
          },
        },
        _sum: {
          outstandingBalance: true,
        },
      }),
    ]);

    const overdueLoans = await this.findOverdueLoans();
    const totalOverdue = overdueLoans.reduce(
      (sum, loan) => sum + loan.outstandingBalance.toNumber(),
      0
    );

    return {
      totalLoans,
      totalDisbursed: disbursedSum._sum.principal?.toNumber() || 0,
      totalOutstanding: outstandingSum._sum.outstandingBalance?.toNumber() || 0,
      totalOverdue,
    };
  }
}

export default LoanRepository;
