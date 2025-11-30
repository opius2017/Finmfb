import { Decimal } from '@prisma/client/runtime/library';
import { createBadRequestError } from '@middleware/errorHandler';

export interface LoanParams {
  principal: number;
  interestRate: number; // Annual rate as percentage (e.g., 12 for 12%)
  termMonths: number;
  method: 'reducing_balance' | 'flat_rate';
  startDate: Date;
  paymentFrequency: 'monthly' | 'quarterly';
  gracePeriodMonths?: number;
}

export interface LoanScheduleItem {
  paymentNumber: number;
  dueDate: Date;
  principal: number;
  interest: number;
  totalPayment: number;
  balance: number;
  cumulativeInterest: number;
  cumulativePrincipal: number;
}

export interface InterestAccrualParams {
  principal: number;
  annualRate: number;
  startDate: Date;
  endDate: Date;
  method: 'simple' | 'compound';
}

export interface PenaltyCalculationParams {
  overdueAmount: number;
  daysOverdue: number;
  penaltyRate: number; // Daily penalty rate as percentage
  penaltyType: 'flat' | 'percentage';
  flatAmount?: number;
}

export interface EarlyPaymentParams {
  outstandingBalance: number;
  remainingTermMonths: number;
  earlyPaymentPenaltyRate: number; // Percentage of outstanding balance
}

export class CalculationService {
  /**
   * Calculate loan amortization schedule
   */
  calculateLoanSchedule(params: LoanParams): LoanScheduleItem[] {
    this.validateLoanParams(params);

    if (params.method === 'reducing_balance') {
      return this.calculateReducingBalanceSchedule(params);
    } else {
      return this.calculateFlatRateSchedule(params);
    }
  }

  /**
   * Calculate reducing balance amortization schedule
   */
  private calculateReducingBalanceSchedule(params: LoanParams): LoanScheduleItem[] {
    const {
      principal,
      interestRate,
      termMonths,
      startDate,
      paymentFrequency,
      gracePeriodMonths = 0,
    } = params;

    const schedule: LoanScheduleItem[] = [];
    const monthlyRate = interestRate / 100 / 12;
    const paymentsPerYear = paymentFrequency === 'monthly' ? 12 : 4;
    const paymentInterval = 12 / paymentsPerYear;
    const numberOfPayments = Math.ceil(termMonths / paymentInterval);

    // Calculate periodic payment using amortization formula
    // PMT = P * [r(1+r)^n] / [(1+r)^n - 1]
    const periodicRate = monthlyRate * paymentInterval;
    const periodicPayment =
      (principal * periodicRate * Math.pow(1 + periodicRate, numberOfPayments)) /
      (Math.pow(1 + periodicRate, numberOfPayments) - 1);

    let balance = principal;
    let cumulativeInterest = 0;
    let cumulativePrincipal = 0;

    for (let i = 1; i <= numberOfPayments; i++) {
      const dueDate = this.addMonths(startDate, i * paymentInterval);
      const isGracePeriod = i * paymentInterval <= gracePeriodMonths;

      let interest: number;
      let principalPayment: number;
      let totalPayment: number;

      if (isGracePeriod) {
        // During grace period, only pay interest
        interest = balance * periodicRate;
        principalPayment = 0;
        totalPayment = interest;
      } else {
        // Regular payment
        interest = balance * periodicRate;
        principalPayment = periodicPayment - interest;
        totalPayment = periodicPayment;

        // Adjust last payment to account for rounding
        if (i === numberOfPayments) {
          principalPayment = balance;
          totalPayment = principalPayment + interest;
        }
      }

      balance -= principalPayment;
      cumulativeInterest += interest;
      cumulativePrincipal += principalPayment;

      schedule.push({
        paymentNumber: i,
        dueDate,
        principal: this.round(principalPayment),
        interest: this.round(interest),
        totalPayment: this.round(totalPayment),
        balance: this.round(Math.max(0, balance)),
        cumulativeInterest: this.round(cumulativeInterest),
        cumulativePrincipal: this.round(cumulativePrincipal),
      });
    }

    return schedule;
  }

  /**
   * Calculate flat rate amortization schedule
   */
  private calculateFlatRateSchedule(params: LoanParams): LoanScheduleItem[] {
    const {
      principal,
      interestRate,
      termMonths,
      startDate,
      paymentFrequency,
      gracePeriodMonths = 0,
    } = params;

    const schedule: LoanScheduleItem[] = [];
    const paymentsPerYear = paymentFrequency === 'monthly' ? 12 : 4;
    const paymentInterval = 12 / paymentsPerYear;
    const numberOfPayments = Math.ceil(termMonths / paymentInterval);

    // Calculate total interest (flat rate)
    const totalInterest = (principal * interestRate * termMonths) / (100 * 12);
    const interestPerPayment = totalInterest / numberOfPayments;
    const principalPerPayment = principal / numberOfPayments;
    const totalPaymentAmount = principalPerPayment + interestPerPayment;

    let balance = principal;
    let cumulativeInterest = 0;
    let cumulativePrincipal = 0;

    for (let i = 1; i <= numberOfPayments; i++) {
      const dueDate = this.addMonths(startDate, i * paymentInterval);
      const isGracePeriod = i * paymentInterval <= gracePeriodMonths;

      let interest: number;
      let principalPayment: number;
      let totalPayment: number;

      if (isGracePeriod) {
        // During grace period, only pay interest
        interest = interestPerPayment;
        principalPayment = 0;
        totalPayment = interest;
      } else {
        // Regular payment
        interest = interestPerPayment;
        principalPayment = principalPerPayment;
        totalPayment = totalPaymentAmount;

        // Adjust last payment for rounding
        if (i === numberOfPayments) {
          principalPayment = balance;
          totalPayment = principalPayment + interest;
        }
      }

      balance -= principalPayment;
      cumulativeInterest += interest;
      cumulativePrincipal += principalPayment;

      schedule.push({
        paymentNumber: i,
        dueDate,
        principal: this.round(principalPayment),
        interest: this.round(interest),
        totalPayment: this.round(totalPayment),
        balance: this.round(Math.max(0, balance)),
        cumulativeInterest: this.round(cumulativeInterest),
        cumulativePrincipal: this.round(cumulativePrincipal),
      });
    }

    return schedule;
  }

  /**
   * Calculate interest accrual
   */
  calculateInterestAccrual(params: InterestAccrualParams): number {
    const { principal, annualRate, startDate, endDate, method } = params;

    if (endDate <= startDate) {
      throw createBadRequestError('End date must be after start date');
    }

    const days = this.daysBetween(startDate, endDate);
    const dailyRate = annualRate / 100 / 365;

    if (method === 'simple') {
      // Simple interest: I = P * r * t
      return this.round(principal * dailyRate * days);
    } else {
      // Compound interest: A = P(1 + r)^t - P
      const amount = principal * Math.pow(1 + dailyRate, days);
      return this.round(amount - principal);
    }
  }

  /**
   * Calculate penalty for late payment
   */
  calculatePenalty(params: PenaltyCalculationParams): number {
    const { overdueAmount, daysOverdue, penaltyRate, penaltyType, flatAmount } = params;

    if (daysOverdue <= 0) {
      return 0;
    }

    if (penaltyType === 'flat' && flatAmount) {
      return this.round(flatAmount);
    }

    // Percentage-based penalty
    const dailyPenaltyRate = penaltyRate / 100;
    const penalty = overdueAmount * dailyPenaltyRate * daysOverdue;

    return this.round(penalty);
  }

  /**
   * Calculate early payment penalty
   */
  calculateEarlyPaymentPenalty(params: EarlyPaymentParams): number {
    const { outstandingBalance, earlyPaymentPenaltyRate } = params;

    if (earlyPaymentPenaltyRate <= 0) {
      return 0;
    }

    const penalty = outstandingBalance * (earlyPaymentPenaltyRate / 100);
    return this.round(penalty);
  }

  /**
   * Calculate effective interest rate
   */
  calculateEffectiveRate(nominalRate: number, compoundingFrequency: number): number {
    // Effective Rate = (1 + r/n)^n - 1
    const effectiveRate = Math.pow(1 + nominalRate / 100 / compoundingFrequency, compoundingFrequency) - 1;
    return this.round(effectiveRate * 100, 4);
  }

  /**
   * Calculate total loan cost
   */
  calculateTotalLoanCost(schedule: LoanScheduleItem[]): {
    totalInterest: number;
    totalPrincipal: number;
    totalPayments: number;
    averageMonthlyPayment: number;
  } {
    const totalInterest = schedule.reduce((sum, item) => sum + item.interest, 0);
    const totalPrincipal = schedule.reduce((sum, item) => sum + item.principal, 0);
    const totalPayments = schedule.reduce((sum, item) => sum + item.totalPayment, 0);
    const averageMonthlyPayment = totalPayments / schedule.length;

    return {
      totalInterest: this.round(totalInterest),
      totalPrincipal: this.round(totalPrincipal),
      totalPayments: this.round(totalPayments),
      averageMonthlyPayment: this.round(averageMonthlyPayment),
    };
  }

  /**
   * Calculate remaining balance at a specific date
   */
  calculateRemainingBalance(
    schedule: LoanScheduleItem[],
    asOfDate: Date,
    paymentsMade: number[]
  ): number {
    let balance = schedule[0]?.balance || 0;

    for (let i = 0; i < schedule.length; i++) {
      if (schedule[i].dueDate > asOfDate) {
        break;
      }

      const paymentMade = paymentsMade[i] || 0;
      balance = schedule[i].balance + (schedule[i].totalPayment - paymentMade);
    }

    return this.round(balance);
  }

  /**
   * Validate loan parameters
   */
  private validateLoanParams(params: LoanParams): void {
    if (params.principal <= 0) {
      throw createBadRequestError('Principal must be greater than 0');
    }

    if (params.interestRate < 0) {
      throw createBadRequestError('Interest rate cannot be negative');
    }

    if (params.termMonths <= 0) {
      throw createBadRequestError('Term must be greater than 0');
    }

    if (params.gracePeriodMonths && params.gracePeriodMonths >= params.termMonths) {
      throw createBadRequestError('Grace period cannot be greater than or equal to loan term');
    }
  }

  /**
   * Add months to a date
   */
  private addMonths(date: Date, months: number): Date {
    const result = new Date(date);
    result.setMonth(result.getMonth() + months);
    return result;
  }

  /**
   * Calculate days between two dates
   */
  private daysBetween(startDate: Date, endDate: Date): number {
    const msPerDay = 1000 * 60 * 60 * 24;
    const start = new Date(startDate).setHours(0, 0, 0, 0);
    const end = new Date(endDate).setHours(0, 0, 0, 0);
    return Math.floor((end - start) / msPerDay);
  }

  /**
   * Round to 2 decimal places (or specified precision)
   */
  private round(value: number, decimals: number = 2): number {
    return Math.round(value * Math.pow(10, decimals)) / Math.pow(10, decimals);
  }
}

export default CalculationService;

export interface AgingBucket {
  label: string;
  minDays: number;
  maxDays: number | null;
  amount: number;
  count: number;
  percentage: number;
}

export interface AgingAnalysisParams {
  accountType: 'AR' | 'AP'; // Accounts Receivable or Accounts Payable
  asOfDate: Date;
  items: AgingItem[];
}

export interface AgingItem {
  id: string;
  customerId?: string;
  vendorId?: string;
  amount: number;
  dueDate: Date;
  invoiceNumber?: string;
  description?: string;
}

export interface AgingReport {
  asOfDate: Date;
  accountType: 'AR' | 'AP';
  totalAmount: number;
  totalCount: number;
  buckets: AgingBucket[];
  byEntity: AgingByEntity[];
}

export interface AgingByEntity {
  entityId: string;
  entityName?: string;
  totalAmount: number;
  buckets: AgingBucket[];
}

export interface CashFlowForecastParams {
  startDate: Date;
  endDate: Date;
  scenarios: ForecastScenario[];
  historicalData?: HistoricalCashFlow[];
}

export interface ForecastScenario {
  name: string;
  probability: number;
  assumptions: {
    receivablesCollectionRate: number; // Percentage
    payablesPaymentRate: number; // Percentage
    additionalInflows?: CashFlowItem[];
    additionalOutflows?: CashFlowItem[];
  };
}

export interface CashFlowItem {
  date: Date;
  amount: number;
  description: string;
  category: string;
}

export interface HistoricalCashFlow {
  date: Date;
  inflows: number;
  outflows: number;
  netCashFlow: number;
}

export interface CashFlowForecast {
  startDate: Date;
  endDate: Date;
  scenarios: ScenarioForecast[];
  summary: ForecastSummary;
}

export interface ScenarioForecast {
  scenarioName: string;
  probability: number;
  periods: CashFlowPeriod[];
  totalInflows: number;
  totalOutflows: number;
  netCashFlow: number;
  endingBalance: number;
}

export interface CashFlowPeriod {
  date: Date;
  inflows: number;
  outflows: number;
  netCashFlow: number;
  cumulativeCashFlow: number;
}

export interface ForecastSummary {
  weightedAverageNetCashFlow: number;
  bestCase: number;
  worstCase: number;
  mostLikely: number;
}

/**
 * Extended CalculationService with aging analysis and cash flow forecasting
 */
export class ExtendedCalculationService extends CalculationService {
  /**
   * Calculate aging analysis
   */
  calculateAgingAnalysis(params: AgingAnalysisParams): AgingReport {
    const { accountType, asOfDate, items } = params;

    // Define aging buckets
    const bucketDefinitions = [
      { label: 'Current', minDays: 0, maxDays: 0 },
      { label: '1-30 days', minDays: 1, maxDays: 30 },
      { label: '31-60 days', minDays: 31, maxDays: 60 },
      { label: '61-90 days', minDays: 61, maxDays: 90 },
      { label: '91-120 days', minDays: 91, maxDays: 120 },
      { label: 'Over 120 days', minDays: 121, maxDays: null },
    ];

    // Initialize buckets
    const buckets: AgingBucket[] = bucketDefinitions.map((def) => ({
      ...def,
      amount: 0,
      count: 0,
      percentage: 0,
    }));

    // Group by entity
    const entityMap = new Map<string, AgingItem[]>();

    // Process each item
    items.forEach((item) => {
      const daysOverdue = this.daysBetween(item.dueDate, asOfDate);
      const bucketIndex = this.getBucketIndex(daysOverdue, bucketDefinitions);

      if (bucketIndex >= 0) {
        buckets[bucketIndex].amount += item.amount;
        buckets[bucketIndex].count += 1;
      }

      // Group by entity
      const entityId = item.customerId || item.vendorId || 'unknown';
      if (!entityMap.has(entityId)) {
        entityMap.set(entityId, []);
      }
      entityMap.get(entityId)!.push(item);
    });

    // Calculate totals
    const totalAmount = buckets.reduce((sum, bucket) => sum + bucket.amount, 0);
    const totalCount = buckets.reduce((sum, bucket) => sum + bucket.count, 0);

    // Calculate percentages
    buckets.forEach((bucket) => {
      bucket.percentage = totalAmount > 0 ? this.round((bucket.amount / totalAmount) * 100) : 0;
      bucket.amount = this.round(bucket.amount);
    });

    // Calculate aging by entity
    const byEntity: AgingByEntity[] = [];
    entityMap.forEach((entityItems, entityId) => {
      const entityBuckets: AgingBucket[] = bucketDefinitions.map((def) => ({
        ...def,
        amount: 0,
        count: 0,
        percentage: 0,
      }));

      entityItems.forEach((item) => {
        const daysOverdue = this.daysBetween(item.dueDate, asOfDate);
        const bucketIndex = this.getBucketIndex(daysOverdue, bucketDefinitions);

        if (bucketIndex >= 0) {
          entityBuckets[bucketIndex].amount += item.amount;
          entityBuckets[bucketIndex].count += 1;
        }
      });

      const entityTotal = entityBuckets.reduce((sum, bucket) => sum + bucket.amount, 0);

      entityBuckets.forEach((bucket) => {
        bucket.percentage = entityTotal > 0 ? this.round((bucket.amount / entityTotal) * 100) : 0;
        bucket.amount = this.round(bucket.amount);
      });

      byEntity.push({
        entityId,
        totalAmount: this.round(entityTotal),
        buckets: entityBuckets,
      });
    });

    // Sort by total amount descending
    byEntity.sort((a, b) => b.totalAmount - a.totalAmount);

    return {
      asOfDate,
      accountType,
      totalAmount: this.round(totalAmount),
      totalCount,
      buckets,
      byEntity,
    };
  }

  /**
   * Get bucket index for days overdue
   */
  private getBucketIndex(
    daysOverdue: number,
    bucketDefinitions: Array<{ minDays: number; maxDays: number | null }>
  ): number {
    for (let i = 0; i < bucketDefinitions.length; i++) {
      const bucket = bucketDefinitions[i];
      if (bucket.maxDays === null) {
        if (daysOverdue >= bucket.minDays) {
          return i;
        }
      } else {
        if (daysOverdue >= bucket.minDays && daysOverdue <= bucket.maxDays) {
          return i;
        }
      }
    }
    return -1;
  }

  /**
   * Calculate cash flow forecast
   */
  calculateCashFlowForecast(params: CashFlowForecastParams): CashFlowForecast {
    const { startDate, endDate, scenarios } = params;

    const scenarioForecasts: ScenarioForecast[] = scenarios.map((scenario) =>
      this.forecastScenario(scenario, startDate, endDate)
    );

    // Calculate weighted averages
    const weightedNetCashFlow = scenarioForecasts.reduce(
      (sum, forecast) => sum + forecast.netCashFlow * forecast.probability,
      0
    );

    const netCashFlows = scenarioForecasts.map((f) => f.netCashFlow);
    const bestCase = Math.max(...netCashFlows);
    const worstCase = Math.min(...netCashFlows);
    const mostLikely = scenarioForecasts.find((f) => f.probability === Math.max(...scenarios.map((s) => s.probability)))
      ?.netCashFlow || 0;

    return {
      startDate,
      endDate,
      scenarios: scenarioForecasts,
      summary: {
        weightedAverageNetCashFlow: this.round(weightedNetCashFlow),
        bestCase: this.round(bestCase),
        worstCase: this.round(worstCase),
        mostLikely: this.round(mostLikely),
      },
    };
  }

  /**
   * Forecast a single scenario
   */
  private forecastScenario(
    scenario: ForecastScenario,
    startDate: Date,
    endDate: Date
  ): ScenarioForecast {
    const periods: CashFlowPeriod[] = [];
    const monthsBetween = this.monthsBetween(startDate, endDate);

    let cumulativeCashFlow = 0;

    for (let i = 0; i <= monthsBetween; i++) {
      const periodDate = this.addMonths(startDate, i);

      // Calculate inflows (simplified - would use actual receivables data)
      const baseInflows = 100000; // Placeholder
      const inflows = baseInflows * (scenario.assumptions.receivablesCollectionRate / 100);

      // Calculate outflows (simplified - would use actual payables data)
      const baseOutflows = 80000; // Placeholder
      const outflows = baseOutflows * (scenario.assumptions.payablesPaymentRate / 100);

      // Add additional cash flows
      const additionalInflows = scenario.assumptions.additionalInflows
        ?.filter((item) => this.isSameMonth(item.date, periodDate))
        .reduce((sum, item) => sum + item.amount, 0) || 0;

      const additionalOutflows = scenario.assumptions.additionalOutflows
        ?.filter((item) => this.isSameMonth(item.date, periodDate))
        .reduce((sum, item) => sum + item.amount, 0) || 0;

      const totalInflows = inflows + additionalInflows;
      const totalOutflows = outflows + additionalOutflows;
      const netCashFlow = totalInflows - totalOutflows;
      cumulativeCashFlow += netCashFlow;

      periods.push({
        date: periodDate,
        inflows: this.round(totalInflows),
        outflows: this.round(totalOutflows),
        netCashFlow: this.round(netCashFlow),
        cumulativeCashFlow: this.round(cumulativeCashFlow),
      });
    }

    const totalInflows = periods.reduce((sum, p) => sum + p.inflows, 0);
    const totalOutflows = periods.reduce((sum, p) => sum + p.outflows, 0);

    return {
      scenarioName: scenario.name,
      probability: scenario.probability,
      periods,
      totalInflows: this.round(totalInflows),
      totalOutflows: this.round(totalOutflows),
      netCashFlow: this.round(totalInflows - totalOutflows),
      endingBalance: this.round(cumulativeCashFlow),
    };
  }

  /**
   * Calculate months between two dates
   */
  private monthsBetween(startDate: Date, endDate: Date): number {
    const start = new Date(startDate);
    const end = new Date(endDate);
    return (end.getFullYear() - start.getFullYear()) * 12 + (end.getMonth() - start.getMonth());
  }

  /**
   * Check if two dates are in the same month
   */
  private isSameMonth(date1: Date, date2: Date): boolean {
    return date1.getFullYear() === date2.getFullYear() && date1.getMonth() === date2.getMonth();
  }
}
