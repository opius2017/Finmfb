import { getPrismaClient } from '@config/database';
import { logger } from '@utils/logger';

/**
 * Analytics parameters
 */
export interface AnalyticsParams {
  startDate: Date;
  endDate: Date;
  branchId?: string;
  compareWithPrevious?: boolean;
}

/**
 * Dashboard metrics
 */
export interface DashboardMetrics {
  members: {
    total: number;
    active: number;
    new: number;
    growth: number;
  };
  loans: {
    total: number;
    active: number;
    disbursed: number;
    outstanding: number;
    portfolioAtRisk: number;
  };
  savings: {
    totalBalance: number;
    totalAccounts: number;
    averageBalance: number;
  };
  transactions: {
    total: number;
    volume: number;
    deposits: number;
    withdrawals: number;
  };
}

/**
 * KPI data
 */
export interface KPIData {
  name: string;
  value: number;
  target?: number;
  unit: string;
  trend: 'up' | 'down' | 'stable';
  change: number;
}

export class AnalyticsService {
  private prisma = getPrismaClient();

  /**
   * Get dashboard metrics
   */
  async getDashboardMetrics(params: AnalyticsParams): Promise<DashboardMetrics> {
    try {
      logger.info('Getting dashboard metrics');

      const [memberMetrics, loanMetrics, savingsMetrics, transactionMetrics] = await Promise.all([
        this.getMemberMetrics(params),
        this.getLoanMetrics(params),
        this.getSavingsMetrics(params),
        this.getTransactionMetrics(params),
      ]);

      return {
        members: memberMetrics,
        loans: loanMetrics,
        savings: savingsMetrics,
        transactions: transactionMetrics,
      };
    } catch (error) {
      logger.error('Error getting dashboard metrics:', error);
      throw error;
    }
  }

  /**
   * Get member metrics
   */
  private async getMemberMetrics(params: AnalyticsParams) {
    const [total, active, newMembers] = await Promise.all([
      this.prisma.member.count({
        where: {
          ...(params.branchId && { branchId: params.branchId }),
        },
      }),
      this.prisma.member.count({
        where: {
          status: 'ACTIVE',
          ...(params.branchId && { branchId: params.branchId }),
        },
      }),
      this.prisma.member.count({
        where: {
          joinDate: {
            gte: params.startDate,
            lte: params.endDate,
          },
          ...(params.branchId && { branchId: params.branchId }),
        },
      }),
    ]);

    // Calculate growth (simplified)
    const growth = total > 0 ? (newMembers / total) * 100 : 0;

    return {
      total,
      active,
      new: newMembers,
      growth: this.round(growth, 2),
    };
  }

  /**
   * Get loan metrics
   */
  private async getLoanMetrics(params: AnalyticsParams) {
    const [total, active, disbursedData, outstandingData] = await Promise.all([
      this.prisma.loan.count({
        where: {
          ...(params.branchId && {
            member: {
              branchId: params.branchId,
            },
          }),
        },
      }),
      this.prisma.loan.count({
        where: {
          status: {
            in: ['DISBURSED', 'ACTIVE'],
          },
          ...(params.branchId && {
            member: {
              branchId: params.branchId,
            },
          }),
        },
      }),
      this.prisma.loan.aggregate({
        where: {
          disbursementDate: {
            gte: params.startDate,
            lte: params.endDate,
          },
          ...(params.branchId && {
            member: {
              branchId: params.branchId,
            },
          }),
        },
        _sum: {
          disbursedAmount: true,
        },
      }),
      this.prisma.loan.aggregate({
        where: {
          status: {
            in: ['DISBURSED', 'ACTIVE'],
          },
          ...(params.branchId && {
            member: {
              branchId: params.branchId,
            },
          }),
        },
        _sum: {
          outstandingBalance: true,
        },
      }),
    ]);

    // Calculate portfolio at risk (loans overdue > 30 days)
    const overdueLoans = await this.prisma.loanSchedule.aggregate({
      where: {
        isPaid: false,
        dueDate: {
          lt: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000),
        },
        loan: {
          status: {
            in: ['DISBURSED', 'ACTIVE'],
          },
          ...(params.branchId && {
            member: {
              branchId: params.branchId,
            },
          }),
        },
      },
      _sum: {
        totalPayment: true,
      },
    });

    const outstanding = Number(outstandingData._sum.outstandingBalance || 0);
    const overdueAmount = Number(overdueLoans._sum.totalPayment || 0);
    const portfolioAtRisk = outstanding > 0 ? (overdueAmount / outstanding) * 100 : 0;

    return {
      total,
      active,
      disbursed: Number(disbursedData._sum.disbursedAmount || 0),
      outstanding,
      portfolioAtRisk: this.round(portfolioAtRisk, 2),
    };
  }

  /**
   * Get savings metrics
   */
  private async getSavingsMetrics(params: AnalyticsParams) {
    const savingsData = await this.prisma.account.aggregate({
      where: {
        type: 'SAVINGS',
        ...(params.branchId && { branchId: params.branchId }),
      },
      _sum: {
        balance: true,
      },
      _count: true,
    });

    const totalBalance = Number(savingsData._sum.balance || 0);
    const totalAccounts = savingsData._count;
    const averageBalance = totalAccounts > 0 ? totalBalance / totalAccounts : 0;

    return {
      totalBalance,
      totalAccounts,
      averageBalance: this.round(averageBalance, 2),
    };
  }

  /**
   * Get transaction metrics
   */
  private async getTransactionMetrics(params: AnalyticsParams) {
    const [total, volumeData, deposits, withdrawals] = await Promise.all([
      this.prisma.transaction.count({
        where: {
          createdAt: {
            gte: params.startDate,
            lte: params.endDate,
          },
          status: 'COMPLETED',
          ...(params.branchId && {
            account: {
              branchId: params.branchId,
            },
          }),
        },
      }),
      this.prisma.transaction.aggregate({
        where: {
          createdAt: {
            gte: params.startDate,
            lte: params.endDate,
          },
          status: 'COMPLETED',
          ...(params.branchId && {
            account: {
              branchId: params.branchId,
            },
          }),
        },
        _sum: {
          amount: true,
        },
      }),
      this.prisma.transaction.aggregate({
        where: {
          type: 'DEBIT',
          createdAt: {
            gte: params.startDate,
            lte: params.endDate,
          },
          status: 'COMPLETED',
          ...(params.branchId && {
            account: {
              branchId: params.branchId,
            },
          }),
        },
        _sum: {
          amount: true,
        },
      }),
      this.prisma.transaction.aggregate({
        where: {
          type: 'CREDIT',
          createdAt: {
            gte: params.startDate,
            lte: params.endDate,
          },
          status: 'COMPLETED',
          ...(params.branchId && {
            account: {
              branchId: params.branchId,
            },
          }),
        },
        _sum: {
          amount: true,
        },
      }),
    ]);

    return {
      total,
      volume: Number(volumeData._sum.amount || 0),
      deposits: Number(deposits._sum.amount || 0),
      withdrawals: Number(withdrawals._sum.amount || 0),
    };
  }

  /**
   * Get KPIs
   */
  async getKPIs(params: AnalyticsParams): Promise<KPIData[]> {
    try {
      logger.info('Getting KPIs');

      const metrics = await this.getDashboardMetrics(params);

      const kpis: KPIData[] = [
        {
          name: 'Active Members',
          value: metrics.members.active,
          target: metrics.members.total,
          unit: 'members',
          trend: metrics.members.growth > 0 ? 'up' : 'stable',
          change: metrics.members.growth,
        },
        {
          name: 'Portfolio at Risk',
          value: metrics.loans.portfolioAtRisk,
          target: 5, // 5% target
          unit: '%',
          trend: metrics.loans.portfolioAtRisk > 5 ? 'up' : 'down',
          change: metrics.loans.portfolioAtRisk - 5,
        },
        {
          name: 'Loan Portfolio',
          value: metrics.loans.outstanding,
          unit: '₦',
          trend: 'up',
          change: 0,
        },
        {
          name: 'Savings Balance',
          value: metrics.savings.totalBalance,
          unit: '₦',
          trend: 'up',
          change: 0,
        },
        {
          name: 'Transaction Volume',
          value: metrics.transactions.volume,
          unit: '₦',
          trend: 'up',
          change: 0,
        },
      ];

      return kpis;
    } catch (error) {
      logger.error('Error getting KPIs:', error);
      throw error;
    }
  }

  /**
   * Get trend analysis
   */
  async getTrendAnalysis(
    metric: string,
    startDate: Date,
    endDate: Date,
    interval: 'daily' | 'weekly' | 'monthly',
    branchId?: string
  ) {
    try {
      logger.info('Getting trend analysis', { metric, interval });

      const periods = this.generatePeriods(startDate, endDate, interval);
      const data = [];

      for (const period of periods) {
        let value = 0;

        switch (metric) {
          case 'members':
            value = await this.prisma.member.count({
              where: {
                joinDate: {
                  lte: period.end,
                },
                ...(branchId && { branchId }),
              },
            });
            break;

          case 'loans':
            const loanData = await this.prisma.loan.aggregate({
              where: {
                disbursementDate: {
                  gte: period.start,
                  lte: period.end,
                },
                ...(branchId && {
                  member: {
                    branchId,
                  },
                }),
              },
              _sum: {
                disbursedAmount: true,
              },
            });
            value = Number(loanData._sum.disbursedAmount || 0);
            break;

          case 'transactions':
            const txData = await this.prisma.transaction.aggregate({
              where: {
                createdAt: {
                  gte: period.start,
                  lte: period.end,
                },
                status: 'COMPLETED',
                ...(branchId && {
                  account: {
                    branchId,
                  },
                }),
              },
              _sum: {
                amount: true,
              },
            });
            value = Number(txData._sum.amount || 0);
            break;
        }

        data.push({
          period: period.label,
          value: this.round(value, 2),
        });
      }

      return data;
    } catch (error) {
      logger.error('Error getting trend analysis:', error);
      throw error;
    }
  }

  /**
   * Generate time periods
   */
  private generatePeriods(
    startDate: Date,
    endDate: Date,
    interval: 'daily' | 'weekly' | 'monthly'
  ): { start: Date; end: Date; label: string }[] {
    const periods = [];
    let current = new Date(startDate);

    while (current <= endDate) {
      const periodStart = new Date(current);
      let periodEnd: Date;
      let label: string;

      if (interval === 'daily') {
        periodEnd = new Date(current);
        periodEnd.setDate(periodEnd.getDate() + 1);
        label = periodStart.toISOString().split('T')[0];
      } else if (interval === 'weekly') {
        periodEnd = new Date(current);
        periodEnd.setDate(periodEnd.getDate() + 7);
        label = `Week of ${periodStart.toISOString().split('T')[0]}`;
      } else {
        periodEnd = new Date(current);
        periodEnd.setMonth(periodEnd.getMonth() + 1);
        label = `${periodStart.toLocaleString('default', { month: 'short' })} ${periodStart.getFullYear()}`;
      }

      if (periodEnd > endDate) {
        periodEnd = new Date(endDate);
      }

      periods.push({
        start: periodStart,
        end: periodEnd,
        label,
      });

      current = new Date(periodEnd);
      current.setDate(current.getDate() + 1);
    }

    return periods;
  }

  /**
   * Round to specified decimal places
   */
  private round(value: number, decimals: number = 2): number {
    return Math.round(value * Math.pow(10, decimals)) / Math.pow(10, decimals);
  }
}

export default AnalyticsService;
