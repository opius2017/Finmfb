import { getPrismaClient } from '@config/database';
import { logger } from '@utils/logger';
import { AgingAnalysisService } from './AgingAnalysisService';
import { BudgetVarianceService } from './BudgetVarianceService';
import { CashFlowForecastService } from './CashFlowForecastService';

/**
 * Report parameters
 */
export interface ReportParams {
  startDate: Date;
  endDate: Date;
  branchId?: string;
  format?: 'json' | 'pdf' | 'excel';
}

/**
 * Balance sheet data
 */
export interface BalanceSheet {
  asOfDate: Date;
  assets: {
    currentAssets: {
      cash: number;
      accountsReceivable: number;
      loansReceivable: number;
      total: number;
    };
    totalAssets: number;
  };
  liabilities: {
    currentLiabilities: {
      accountsPayable: number;
      total: number;
    };
    totalLiabilities: number;
  };
  equity: {
    memberEquity: number;
    retainedEarnings: number;
    total: number;
  };
  totalLiabilitiesAndEquity: number;
}

/**
 * Income statement data
 */
export interface IncomeStatement {
  period: {
    startDate: Date;
    endDate: Date;
  };
  revenue: {
    interestIncome: number;
    feeIncome: number;
    otherIncome: number;
    totalRevenue: number;
  };
  expenses: {
    operatingExpenses: number;
    interestExpense: number;
    provisions: number;
    totalExpenses: number;
  };
  netIncome: number;
}

/**
 * Cash flow statement data
 */
export interface CashFlowStatement {
  period: {
    startDate: Date;
    endDate: Date;
  };
  operatingActivities: {
    netIncome: number;
    adjustments: number;
    netCashFromOperating: number;
  };
  investingActivities: {
    loansDisbursed: number;
    loansRepaid: number;
    netCashFromInvesting: number;
  };
  financingActivities: {
    depositsReceived: number;
    withdrawals: number;
    netCashFromFinancing: number;
  };
  netCashFlow: number;
  openingCash: number;
  closingCash: number;
}

/**
 * Trial balance data
 */
export interface TrialBalance {
  asOfDate: Date;
  accounts: {
    accountCode: string;
    accountName: string;
    debit: number;
    credit: number;
  }[];
  totals: {
    totalDebit: number;
    totalCredit: number;
  };
}

export class ReportingService {
  private prisma = getPrismaClient();
  private agingService = new AgingAnalysisService();
  private budgetService = new BudgetVarianceService();
  private cashFlowService = new CashFlowForecastService();

  /**
   * Generate balance sheet
   */
  async generateBalanceSheet(params: ReportParams): Promise<BalanceSheet> {
    try {
      logger.info('Generating balance sheet', {
        asOfDate: params.endDate,
      });

      const asOfDate = params.endDate;

      // Get cash balance
      const cashAccounts = await this.prisma.account.findMany({
        where: {
          type: 'CASH',
          ...(params.branchId && { branchId: params.branchId }),
        },
      });
      const cash = cashAccounts.reduce((sum, acc) => sum + Number(acc.balance), 0);

      // Get accounts receivable (outstanding loans)
      const loansReceivable = await this.prisma.loan.aggregate({
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
      });

      const accountsReceivable = Number(loansReceivable._sum.outstandingBalance || 0);

      // Get accounts payable
      const accountsPayable = await this.prisma.transaction.aggregate({
        where: {
          type: 'CREDIT',
          status: 'PENDING',
          ...(params.branchId && {
            account: {
              branchId: params.branchId,
            },
          }),
        },
        _sum: {
          amount: true,
        },
      });

      const payables = Number(accountsPayable._sum.amount || 0);

      // Get member equity (total deposits)
      const memberEquity = await this.prisma.account.aggregate({
        where: {
          type: {
            in: ['SAVINGS', 'SHARES'],
          },
          ...(params.branchId && { branchId: params.branchId }),
        },
        _sum: {
          balance: true,
        },
      });

      const equity = Number(memberEquity._sum.balance || 0);

      // Calculate totals
      const currentAssets = {
        cash: this.round(cash),
        accountsReceivable: this.round(accountsReceivable),
        loansReceivable: this.round(accountsReceivable),
        total: this.round(cash + accountsReceivable),
      };

      const totalAssets = currentAssets.total;

      const currentLiabilities = {
        accountsPayable: this.round(payables),
        total: this.round(payables),
      };

      const totalLiabilities = currentLiabilities.total;

      // Retained earnings = Assets - Liabilities - Member Equity
      const retainedEarnings = totalAssets - totalLiabilities - equity;

      const equitySection = {
        memberEquity: this.round(equity),
        retainedEarnings: this.round(retainedEarnings),
        total: this.round(equity + retainedEarnings),
      };

      return {
        asOfDate,
        assets: {
          currentAssets,
          totalAssets: this.round(totalAssets),
        },
        liabilities: {
          currentLiabilities,
          totalLiabilities: this.round(totalLiabilities),
        },
        equity: equitySection,
        totalLiabilitiesAndEquity: this.round(totalLiabilities + equitySection.total),
      };
    } catch (error) {
      logger.error('Error generating balance sheet:', error);
      throw error;
    }
  }

  /**
   * Generate income statement
   */
  async generateIncomeStatement(params: ReportParams): Promise<IncomeStatement> {
    try {
      logger.info('Generating income statement', {
        startDate: params.startDate,
        endDate: params.endDate,
      });

      // Get interest income from loan payments
      const loanPayments = await this.prisma.loanPayment.aggregate({
        where: {
          paymentDate: {
            gte: params.startDate,
            lte: params.endDate,
          },
          ...(params.branchId && {
            loan: {
              member: {
                branchId: params.branchId,
              },
            },
          }),
        },
        _sum: {
          amount: true,
        },
      });

      // Estimate interest portion (simplified - should track separately)
      const totalPayments = Number(loanPayments._sum.amount || 0);
      const interestIncome = totalPayments * 0.3; // Assume 30% is interest

      // Get fee income from transactions
      const feeIncome = 0; // TODO: Implement fee tracking

      // Get other income
      const otherIncome = 0; // TODO: Implement other income tracking

      const totalRevenue = interestIncome + feeIncome + otherIncome;

      // Get operating expenses from budget actuals
      const expenses = await this.prisma.budgetActual.aggregate({
        where: {
          date: {
            gte: params.startDate,
            lte: params.endDate,
          },
          ...(params.branchId && {
            budget: {
              branchId: params.branchId,
            },
          }),
        },
        _sum: {
          amount: true,
        },
      });

      const operatingExpenses = Number(expenses._sum.amount || 0);

      // Calculate provisions (simplified)
      const provisions = totalRevenue * 0.05; // 5% provision

      const totalExpenses = operatingExpenses + provisions;
      const netIncome = totalRevenue - totalExpenses;

      return {
        period: {
          startDate: params.startDate,
          endDate: params.endDate,
        },
        revenue: {
          interestIncome: this.round(interestIncome),
          feeIncome: this.round(feeIncome),
          otherIncome: this.round(otherIncome),
          totalRevenue: this.round(totalRevenue),
        },
        expenses: {
          operatingExpenses: this.round(operatingExpenses),
          interestExpense: 0,
          provisions: this.round(provisions),
          totalExpenses: this.round(totalExpenses),
        },
        netIncome: this.round(netIncome),
      };
    } catch (error) {
      logger.error('Error generating income statement:', error);
      throw error;
    }
  }

  /**
   * Generate cash flow statement
   */
  async generateCashFlowStatement(params: ReportParams): Promise<CashFlowStatement> {
    try {
      logger.info('Generating cash flow statement', {
        startDate: params.startDate,
        endDate: params.endDate,
      });

      // Get net income from income statement
      const incomeStatement = await this.generateIncomeStatement(params);
      const netIncome = incomeStatement.netIncome;

      // Get loans disbursed
      const loansDisbursed = await this.prisma.loan.aggregate({
        where: {
          disbursementDate: {
            gte: params.startDate,
            lte: params.endDate,
          },
          status: {
            in: ['DISBURSED', 'ACTIVE', 'CLOSED'],
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
      });

      // Get loans repaid
      const loansRepaid = await this.prisma.loanPayment.aggregate({
        where: {
          paymentDate: {
            gte: params.startDate,
            lte: params.endDate,
          },
          ...(params.branchId && {
            loan: {
              member: {
                branchId: params.branchId,
              },
            },
          }),
        },
        _sum: {
          amount: true,
        },
      });

      // Get deposits and withdrawals
      const deposits = await this.prisma.transaction.aggregate({
        where: {
          type: 'DEBIT',
          status: 'COMPLETED',
          createdAt: {
            gte: params.startDate,
            lte: params.endDate,
          },
          ...(params.branchId && {
            account: {
              branchId: params.branchId,
            },
          }),
        },
        _sum: {
          amount: true,
        },
      });

      const withdrawals = await this.prisma.transaction.aggregate({
        where: {
          type: 'CREDIT',
          status: 'COMPLETED',
          createdAt: {
            gte: params.startDate,
            lte: params.endDate,
          },
          ...(params.branchId && {
            account: {
              branchId: params.branchId,
            },
          }),
        },
        _sum: {
          amount: true,
        },
      });

      const disbursed = Number(loansDisbursed._sum.disbursedAmount || 0);
      const repaid = Number(loansRepaid._sum.amount || 0);
      const depositsAmount = Number(deposits._sum.amount || 0);
      const withdrawalsAmount = Number(withdrawals._sum.amount || 0);

      const netCashFromOperating = netIncome;
      const netCashFromInvesting = repaid - disbursed;
      const netCashFromFinancing = depositsAmount - withdrawalsAmount;
      const netCashFlow = netCashFromOperating + netCashFromInvesting + netCashFromFinancing;

      // Get opening and closing cash
      const openingCash = await this.getCashBalance(params.startDate, params.branchId);
      const closingCash = openingCash + netCashFlow;

      return {
        period: {
          startDate: params.startDate,
          endDate: params.endDate,
        },
        operatingActivities: {
          netIncome: this.round(netIncome),
          adjustments: 0,
          netCashFromOperating: this.round(netCashFromOperating),
        },
        investingActivities: {
          loansDisbursed: this.round(-disbursed),
          loansRepaid: this.round(repaid),
          netCashFromInvesting: this.round(netCashFromInvesting),
        },
        financingActivities: {
          depositsReceived: this.round(depositsAmount),
          withdrawals: this.round(-withdrawalsAmount),
          netCashFromFinancing: this.round(netCashFromFinancing),
        },
        netCashFlow: this.round(netCashFlow),
        openingCash: this.round(openingCash),
        closingCash: this.round(closingCash),
      };
    } catch (error) {
      logger.error('Error generating cash flow statement:', error);
      throw error;
    }
  }

  /**
   * Generate trial balance
   */
  async generateTrialBalance(params: ReportParams): Promise<TrialBalance> {
    try {
      logger.info('Generating trial balance', {
        asOfDate: params.endDate,
      });

      // Get all accounts with balances
      const accounts = await this.prisma.account.findMany({
        where: {
          ...(params.branchId && { branchId: params.branchId }),
        },
      });

      const trialBalanceAccounts = accounts.map(account => {
        const balance = Number(account.balance);
        return {
          accountCode: account.accountNumber,
          accountName: `${account.type} - ${account.accountNumber}`,
          debit: balance >= 0 ? balance : 0,
          credit: balance < 0 ? Math.abs(balance) : 0,
        };
      });

      const totalDebit = trialBalanceAccounts.reduce((sum, acc) => sum + acc.debit, 0);
      const totalCredit = trialBalanceAccounts.reduce((sum, acc) => sum + acc.credit, 0);

      return {
        asOfDate: params.endDate,
        accounts: trialBalanceAccounts.map(acc => ({
          ...acc,
          debit: this.round(acc.debit),
          credit: this.round(acc.credit),
        })),
        totals: {
          totalDebit: this.round(totalDebit),
          totalCredit: this.round(totalCredit),
        },
      };
    } catch (error) {
      logger.error('Error generating trial balance:', error);
      throw error;
    }
  }

  /**
   * Generate comprehensive financial report
   */
  async generateFinancialReport(params: ReportParams) {
    try {
      logger.info('Generating comprehensive financial report');

      const [balanceSheet, incomeStatement, cashFlowStatement, trialBalance] = await Promise.all([
        this.generateBalanceSheet(params),
        this.generateIncomeStatement(params),
        this.generateCashFlowStatement(params),
        this.generateTrialBalance(params),
      ]);

      return {
        reportDate: new Date(),
        period: {
          startDate: params.startDate,
          endDate: params.endDate,
        },
        balanceSheet,
        incomeStatement,
        cashFlowStatement,
        trialBalance,
      };
    } catch (error) {
      logger.error('Error generating financial report:', error);
      throw error;
    }
  }

  /**
   * Get cash balance at specific date
   */
  private async getCashBalance(date: Date, branchId?: string): Promise<number> {
    const cashAccounts = await this.prisma.account.findMany({
      where: {
        type: 'CASH',
        ...(branchId && { branchId }),
      },
    });

    return cashAccounts.reduce((sum, acc) => sum + Number(acc.balance), 0);
  }

  /**
   * Round to 2 decimal places
   */
  private round(value: number): number {
    return Math.round(value * 100) / 100;
  }
}

export default ReportingService;
