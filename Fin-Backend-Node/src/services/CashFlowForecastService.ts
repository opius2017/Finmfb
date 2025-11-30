import { Decimal } from '@prisma/client/runtime/library';
import { RepositoryFactory } from '@repositories/index';

export interface CashFlowPeriod {
  startDate: Date;
  endDate: Date;
  inflows: CashFlowItem[];
  outflows: CashFlowItem[];
  netCashFlow: Decimal;
  cumulativeCashFlow: Decimal;
  openingBalance: Decimal;
  closingBalance: Decimal;
}

export interface CashFlowItem {
  category: string;
  description: string;
  amount: Decimal;
  date: Date;
  confidence: 'high' | 'medium' | 'low';
  source: string;
}

export interface CashFlowForecast {
  scenarioName: string;
  startDate: Date;
  endDate: Date;
  openingBalance: Decimal;
  periods: CashFlowPeriod[];
  summary: {
    totalInflows: Decimal;
    totalOutflows: Decimal;
    netCashFlow: Decimal;
    closingBalance: Decimal;
    averageMonthlyInflow: Decimal;
    averageMonthlyOutflow: Decimal;
  };
}

export interface ForecastScenario {
  name: string;
  assumptions: {
    loanDisbursementGrowth?: number; // Percentage
    collectionRate?: number; // Percentage of expected collections
    expenseGrowth?: number; // Percentage
    newMemberGrowth?: number; // Percentage
  };
}

export class CashFlowForecastService {
  private loanRepository = RepositoryFactory.getLoanRepository();
  private accountRepository = RepositoryFactory.getAccountRepository();

  /**
   * Generate cash flow forecast
   */
  async generateForecast(
    startDate: Date,
    endDate: Date,
    openingBalance: number,
    scenario: ForecastScenario = { name: 'Base Case', assumptions: {} }
  ): Promise<CashFlowForecast> {
    const periods: CashFlowPeriod[] = [];
    let cumulativeCashFlow = openingBalance;

    // Generate monthly periods
    const currentDate = new Date(startDate);
    
    while (currentDate <= endDate) {
      const periodStart = new Date(currentDate);
      const periodEnd = new Date(currentDate);
      periodEnd.setMonth(periodEnd.getMonth() + 1);
      periodEnd.setDate(0); // Last day of month

      const period = await this.generatePeriodForecast(
        periodStart,
        periodEnd,
        cumulativeCashFlow,
        scenario
      );

      cumulativeCashFlow = Number(period.closingBalance);
      periods.push(period);

      currentDate.setMonth(currentDate.getMonth() + 1);
    }

    // Calculate summary
    const totalInflows = periods.reduce(
      (sum, p) => sum + p.inflows.reduce((s, i) => s + Number(i.amount), 0),
      0
    );
    const totalOutflows = periods.reduce(
      (sum, p) => sum + p.outflows.reduce((s, i) => s + Number(i.amount), 0),
      0
    );
    const netCashFlow = totalInflows - totalOutflows;
    const closingBalance = openingBalance + netCashFlow;

    return {
      scenarioName: scenario.name,
      startDate,
      endDate,
      openingBalance: new Decimal(openingBalance.toFixed(2)),
      periods,
      summary: {
        totalInflows: new Decimal(totalInflows.toFixed(2)),
        totalOutflows: new Decimal(totalOutflows.toFixed(2)),
        netCashFlow: new Decimal(netCashFlow.toFixed(2)),
        closingBalance: new Decimal(closingBalance.toFixed(2)),
        averageMonthlyInflow: new Decimal((totalInflows / periods.length).toFixed(2)),
        averageMonthlyOutflow: new Decimal((totalOutflows / periods.length).toFixed(2)),
      },
    };
  }

  /**
   * Generate forecast for a single period
   */
  private async generatePeriodForecast(
    startDate: Date,
    endDate: Date,
    openingBalance: number,
    scenario: ForecastScenario
  ): Promise<CashFlowPeriod> {
    const inflows: CashFlowItem[] = [];
    const outflows: CashFlowItem[] = [];

    // Project loan repayments (inflows)
    const loanRepayments = await this.projectLoanRepayments(
      startDate,
      endDate,
      scenario.assumptions.collectionRate || 0.95
    );
    inflows.push(...loanRepayments);

    // Project member deposits (inflows)
    const deposits = await this.projectMemberDeposits(startDate, endDate, scenario);
    inflows.push(...deposits);

    // Project loan disbursements (outflows)
    const disbursements = await this.projectLoanDisbursements(
      startDate,
      endDate,
      scenario.assumptions.loanDisbursementGrowth || 0
    );
    outflows.push(...disbursements);

    // Project operating expenses (outflows)
    const expenses = await this.projectOperatingExpenses(
      startDate,
      endDate,
      scenario.assumptions.expenseGrowth || 0
    );
    outflows.push(...expenses);

    // Calculate net cash flow
    const totalInflows = inflows.reduce((sum, item) => sum + Number(item.amount), 0);
    const totalOutflows = outflows.reduce((sum, item) => sum + Number(item.amount), 0);
    const netCashFlow = totalInflows - totalOutflows;
    const closingBalance = openingBalance + netCashFlow;

    return {
      startDate,
      endDate,
      inflows,
      outflows,
      netCashFlow: new Decimal(netCashFlow.toFixed(2)),
      cumulativeCashFlow: new Decimal(closingBalance.toFixed(2)),
      openingBalance: new Decimal(openingBalance.toFixed(2)),
      closingBalance: new Decimal(closingBalance.toFixed(2)),
    };
  }

  /**
   * Project loan repayments for period
   */
  private async projectLoanRepayments(
    startDate: Date,
    endDate: Date,
    collectionRate: number
  ): Promise<CashFlowItem[]> {
    const items: CashFlowItem[] = [];

    // Get all active loans
    const loans = await this.loanRepository.findMany({
      where: {
        status: {
          in: ['ACTIVE', 'DISBURSED'],
        },
      },
      include: {
        schedules: {
          where: {
            dueDate: {
              gte: startDate,
              lte: endDate,
            },
            isPaid: false,
          },
        },
      },
    });

    // Sum up expected repayments
    let totalExpectedRepayments = 0;
    
    for (const loan of loans) {
      for (const schedule of loan.schedules) {
        totalExpectedRepayments += Number(schedule.totalPayment);
      }
    }

    // Apply collection rate
    const projectedRepayments = totalExpectedRepayments * collectionRate;

    if (projectedRepayments > 0) {
      items.push({
        category: 'Loan Repayments',
        description: 'Projected loan repayments from members',
        amount: new Decimal(projectedRepayments.toFixed(2)),
        date: endDate,
        confidence: collectionRate >= 0.9 ? 'high' : collectionRate >= 0.8 ? 'medium' : 'low',
        source: 'Loan Schedule',
      });
    }

    return items;
  }

  /**
   * Project member deposits
   */
  private async projectMemberDeposits(
    startDate: Date,
    endDate: Date,
    scenario: ForecastScenario
  ): Promise<CashFlowItem[]> {
    const items: CashFlowItem[] = [];

    // Get historical deposit data
    const historicalDeposits = await this.getHistoricalDeposits(startDate);

    // Project based on historical average with growth
    const growthRate = scenario.assumptions.newMemberGrowth || 0;
    const projectedDeposits = historicalDeposits * (1 + growthRate);

    if (projectedDeposits > 0) {
      items.push({
        category: 'Member Deposits',
        description: 'Projected member savings deposits',
        amount: new Decimal(projectedDeposits.toFixed(2)),
        date: endDate,
        confidence: 'medium',
        source: 'Historical Average',
      });
    }

    return items;
  }

  /**
   * Project loan disbursements
   */
  private async projectLoanDisbursements(
    startDate: Date,
    endDate: Date,
    growthRate: number
  ): Promise<CashFlowItem[]> {
    const items: CashFlowItem[] = [];

    // Get historical disbursement data
    const historicalDisbursements = await this.getHistoricalDisbursements(startDate);

    // Project based on historical average with growth
    const projectedDisbursements = historicalDisbursements * (1 + growthRate);

    if (projectedDisbursements > 0) {
      items.push({
        category: 'Loan Disbursements',
        description: 'Projected loan disbursements to members',
        amount: new Decimal(projectedDisbursements.toFixed(2)),
        date: endDate,
        confidence: 'medium',
        source: 'Historical Average',
      });
    }

    return items;
  }

  /**
   * Project operating expenses
   */
  private async projectOperatingExpenses(
    startDate: Date,
    endDate: Date,
    growthRate: number
  ): Promise<CashFlowItem[]> {
    const items: CashFlowItem[] = [];

    // Get historical expense data
    const historicalExpenses = await this.getHistoricalExpenses(startDate);

    // Project based on historical average with growth
    const projectedExpenses = historicalExpenses * (1 + growthRate);

    if (projectedExpenses > 0) {
      items.push({
        category: 'Operating Expenses',
        description: 'Projected operating expenses',
        amount: new Decimal(projectedExpenses.toFixed(2)),
        date: endDate,
        confidence: 'high',
        source: 'Historical Average',
      });
    }

    return items;
  }

  /**
   * Get historical deposits (last 3 months average)
   */
  private async getHistoricalDeposits(beforeDate: Date): Promise<number> {
    const threeMonthsAgo = new Date(beforeDate);
    threeMonthsAgo.setMonth(threeMonthsAgo.getMonth() - 3);

    // Query transactions for deposits
    // This is a simplified version - actual implementation would query transaction table
    return 50000; // Placeholder
  }

  /**
   * Get historical disbursements (last 3 months average)
   */
  private async getHistoricalDisbursements(beforeDate: Date): Promise<number> {
    const threeMonthsAgo = new Date(beforeDate);
    threeMonthsAgo.setMonth(threeMonthsAgo.getMonth() - 3);

    // Query loans for disbursements
    // This is a simplified version - actual implementation would query loan table
    return 45000; // Placeholder
  }

  /**
   * Get historical expenses (last 3 months average)
   */
  private async getHistoricalExpenses(beforeDate: Date): Promise<number> {
    const threeMonthsAgo = new Date(beforeDate);
    threeMonthsAgo.setMonth(threeMonthsAgo.getMonth() - 3);

    // Query budget actuals for expenses
    // This is a simplified version - actual implementation would query budget_actuals table
    return 15000; // Placeholder
  }

  /**
   * Generate multiple scenario forecasts
   */
  async generateScenarioForecasts(
    startDate: Date,
    endDate: Date,
    openingBalance: number,
    scenarios: ForecastScenario[]
  ): Promise<CashFlowForecast[]> {
    const forecasts: CashFlowForecast[] = [];

    for (const scenario of scenarios) {
      const forecast = await this.generateForecast(
        startDate,
        endDate,
        openingBalance,
        scenario
      );
      forecasts.push(forecast);
    }

    return forecasts;
  }

  /**
   * Calculate cash flow variance between scenarios
   */
  calculateScenarioVariance(
    baseScenario: CashFlowForecast,
    compareScenario: CashFlowForecast
  ): {
    inflowVariance: Decimal;
    outflowVariance: Decimal;
    netCashFlowVariance: Decimal;
    closingBalanceVariance: Decimal;
  } {
    const inflowVariance = 
      Number(compareScenario.summary.totalInflows) - 
      Number(baseScenario.summary.totalInflows);
    
    const outflowVariance = 
      Number(compareScenario.summary.totalOutflows) - 
      Number(baseScenario.summary.totalOutflows);
    
    const netCashFlowVariance = 
      Number(compareScenario.summary.netCashFlow) - 
      Number(baseScenario.summary.netCashFlow);
    
    const closingBalanceVariance = 
      Number(compareScenario.summary.closingBalance) - 
      Number(baseScenario.summary.closingBalance);

    return {
      inflowVariance: new Decimal(inflowVariance.toFixed(2)),
      outflowVariance: new Decimal(outflowVariance.toFixed(2)),
      netCashFlowVariance: new Decimal(netCashFlowVariance.toFixed(2)),
      closingBalanceVariance: new Decimal(closingBalanceVariance.toFixed(2)),
    };
  }
}

export default CashFlowForecastService;
