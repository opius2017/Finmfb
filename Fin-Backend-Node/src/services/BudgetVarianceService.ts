import { getPrismaClient } from '@config/database';
import { logger } from '@utils/logger';

/**
 * Budget variance item
 */
export interface BudgetVarianceItem {
  itemId: string;
  itemName: string;
  category: string;
  budgetAmount: number;
  actualAmount: number;
  variance: number;
  variancePercentage: number;
  status: 'over' | 'under' | 'on_track';
}

/**
 * Budget variance summary
 */
export interface BudgetVarianceSummary {
  budgetId: string;
  budgetName: string;
  period: {
    startDate: Date;
    endDate: Date;
  };
  items: BudgetVarianceItem[];
  totals: {
    totalBudget: number;
    totalActual: number;
    totalVariance: number;
    variancePercentage: number;
  };
  categories: {
    category: string;
    budgetAmount: number;
    actualAmount: number;
    variance: number;
    variancePercentage: number;
  }[];
}

/**
 * Budget variance parameters
 */
export interface BudgetVarianceParams {
  budgetId: string;
  startDate?: Date;
  endDate?: Date;
  categoryFilter?: string[];
  includeZeroVariance?: boolean;
}

/**
 * Variance alert
 */
export interface VarianceAlert {
  itemId: string;
  itemName: string;
  budgetAmount: number;
  actualAmount: number;
  variance: number;
  variancePercentage: number;
  severity: 'critical' | 'warning' | 'info';
  message: string;
}

export class BudgetVarianceService {
  private prisma = getPrismaClient();

  // Variance thresholds
  private readonly CRITICAL_THRESHOLD = 20; // 20% over budget
  private readonly WARNING_THRESHOLD = 10; // 10% over budget

  /**
   * Calculate budget variance
   */
  async calculateBudgetVariance(params: BudgetVarianceParams): Promise<BudgetVarianceSummary> {
    try {
      logger.info('Calculating budget variance', {
        budgetId: params.budgetId,
      });

      // Get budget with items
      const budget = await this.prisma.budget.findUnique({
        where: { id: params.budgetId },
        include: {
          items: true,
        },
      });

      if (!budget) {
        throw new Error(`Budget not found: ${params.budgetId}`);
      }

      // Get actual expenses for the period
      const startDate = params.startDate || budget.startDate;
      const endDate = params.endDate || budget.endDate;

      const actuals = await this.prisma.budgetActual.findMany({
        where: {
          budgetId: params.budgetId,
          date: {
            gte: startDate,
            lte: endDate,
          },
          ...(params.categoryFilter && {
            category: { in: params.categoryFilter },
          }),
        },
      });

      // Group actuals by budget item
      const actualsByItem = new Map<string, number>();
      for (const actual of actuals) {
        const current = actualsByItem.get(actual.budgetItemId) || 0;
        actualsByItem.set(actual.budgetItemId, current + Number(actual.amount));
      }

      // Calculate variance for each item
      const items: BudgetVarianceItem[] = [];
      for (const budgetItem of budget.items) {
        if (params.categoryFilter && !params.categoryFilter.includes(budgetItem.category)) {
          continue;
        }

        const budgetAmount = Number(budgetItem.amount);
        const actualAmount = actualsByItem.get(budgetItem.id) || 0;
        const variance = actualAmount - budgetAmount;
        const variancePercentage = budgetAmount > 0 
          ? (variance / budgetAmount) * 100 
          : 0;

        const item: BudgetVarianceItem = {
          itemId: budgetItem.id,
          itemName: budgetItem.name,
          category: budgetItem.category,
          budgetAmount: this.round(budgetAmount),
          actualAmount: this.round(actualAmount),
          variance: this.round(variance),
          variancePercentage: this.round(variancePercentage, 2),
          status: this.determineStatus(variancePercentage),
        };

        // Filter zero variance if requested
        if (!params.includeZeroVariance && variance === 0) {
          continue;
        }

        items.push(item);
      }

      // Calculate totals
      const totals = this.calculateTotals(items);

      // Calculate category summaries
      const categories = this.calculateCategorySummaries(items);

      return {
        budgetId: budget.id,
        budgetName: budget.name,
        period: {
          startDate,
          endDate,
        },
        items,
        totals,
        categories,
      };
    } catch (error) {
      logger.error('Error calculating budget variance:', error);
      throw error;
    }
  }

  /**
   * Get variance alerts
   */
  async getVarianceAlerts(params: BudgetVarianceParams): Promise<VarianceAlert[]> {
    const variance = await this.calculateBudgetVariance(params);
    const alerts: VarianceAlert[] = [];

    for (const item of variance.items) {
      if (item.variance > 0) {
        const alert = this.createVarianceAlert(item);
        if (alert) {
          alerts.push(alert);
        }
      }
    }

    // Sort by severity and variance amount
    return alerts.sort((a, b) => {
      const severityOrder = { critical: 0, warning: 1, info: 2 };
      if (severityOrder[a.severity] !== severityOrder[b.severity]) {
        return severityOrder[a.severity] - severityOrder[b.severity];
      }
      return Math.abs(b.variance) - Math.abs(a.variance);
    });
  }

  /**
   * Create variance alert
   */
  private createVarianceAlert(item: BudgetVarianceItem): VarianceAlert | null {
    if (item.variancePercentage >= this.CRITICAL_THRESHOLD) {
      return {
        itemId: item.itemId,
        itemName: item.itemName,
        budgetAmount: item.budgetAmount,
        actualAmount: item.actualAmount,
        variance: item.variance,
        variancePercentage: item.variancePercentage,
        severity: 'critical',
        message: `${item.itemName} is ${item.variancePercentage.toFixed(1)}% over budget (₦${item.variance.toFixed(2)} overspent)`,
      };
    } else if (item.variancePercentage >= this.WARNING_THRESHOLD) {
      return {
        itemId: item.itemId,
        itemName: item.itemName,
        budgetAmount: item.budgetAmount,
        actualAmount: item.actualAmount,
        variance: item.variance,
        variancePercentage: item.variancePercentage,
        severity: 'warning',
        message: `${item.itemName} is ${item.variancePercentage.toFixed(1)}% over budget (₦${item.variance.toFixed(2)} overspent)`,
      };
    }
    return null;
  }

  /**
   * Calculate variance trend over time
   */
  async calculateVarianceTrend(
    budgetId: string,
    startDate: Date,
    endDate: Date,
    interval: 'daily' | 'weekly' | 'monthly' = 'monthly'
  ): Promise<{
    period: string;
    budgetAmount: number;
    actualAmount: number;
    variance: number;
    variancePercentage: number;
  }[]> {
    const periods = this.generatePeriods(startDate, endDate, interval);
    const trend = [];

    for (const period of periods) {
      const variance = await this.calculateBudgetVariance({
        budgetId,
        startDate: period.start,
        endDate: period.end,
      });

      trend.push({
        period: period.label,
        budgetAmount: variance.totals.totalBudget,
        actualAmount: variance.totals.totalActual,
        variance: variance.totals.totalVariance,
        variancePercentage: variance.totals.variancePercentage,
      });
    }

    return trend;
  }

  /**
   * Get budget utilization rate
   */
  async getBudgetUtilization(budgetId: string): Promise<{
    totalBudget: number;
    totalSpent: number;
    totalRemaining: number;
    utilizationRate: number;
    projectedOverrun: number;
    daysRemaining: number;
    burnRate: number; // Average daily spend
  }> {
    const budget = await this.prisma.budget.findUnique({
      where: { id: budgetId },
      include: {
        items: true,
      },
    });

    if (!budget) {
      throw new Error(`Budget not found: ${budgetId}`);
    }

    const variance = await this.calculateBudgetVariance({ budgetId });
    const totalBudget = variance.totals.totalBudget;
    const totalSpent = variance.totals.totalActual;
    const totalRemaining = totalBudget - totalSpent;
    const utilizationRate = totalBudget > 0 ? (totalSpent / totalBudget) * 100 : 0;

    // Calculate days elapsed and remaining
    const now = new Date();
    const startDate = budget.startDate;
    const endDate = budget.endDate;
    const totalDays = Math.ceil((endDate.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24));
    const daysElapsed = Math.ceil((now.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24));
    const daysRemaining = Math.max(0, totalDays - daysElapsed);

    // Calculate burn rate
    const burnRate = daysElapsed > 0 ? totalSpent / daysElapsed : 0;

    // Project overrun
    const projectedTotal = burnRate * totalDays;
    const projectedOverrun = Math.max(0, projectedTotal - totalBudget);

    return {
      totalBudget: this.round(totalBudget),
      totalSpent: this.round(totalSpent),
      totalRemaining: this.round(totalRemaining),
      utilizationRate: this.round(utilizationRate, 2),
      projectedOverrun: this.round(projectedOverrun),
      daysRemaining,
      burnRate: this.round(burnRate, 2),
    };
  }

  /**
   * Compare budgets across periods
   */
  async compareBudgetPeriods(
    budgetIds: string[]
  ): Promise<{
    budgetId: string;
    budgetName: string;
    period: string;
    totalBudget: number;
    totalActual: number;
    variance: number;
    variancePercentage: number;
  }[]> {
    const comparisons = [];

    for (const budgetId of budgetIds) {
      const variance = await this.calculateBudgetVariance({ budgetId });
      const budget = await this.prisma.budget.findUnique({
        where: { id: budgetId },
      });

      if (budget) {
        comparisons.push({
          budgetId: budget.id,
          budgetName: budget.name,
          period: `${budget.startDate.toISOString().split('T')[0]} to ${budget.endDate.toISOString().split('T')[0]}`,
          totalBudget: variance.totals.totalBudget,
          totalActual: variance.totals.totalActual,
          variance: variance.totals.totalVariance,
          variancePercentage: variance.totals.variancePercentage,
        });
      }
    }

    return comparisons;
  }

  /**
   * Determine status based on variance percentage
   */
  private determineStatus(variancePercentage: number): 'over' | 'under' | 'on_track' {
    if (variancePercentage > 5) {
      return 'over';
    } else if (variancePercentage < -5) {
      return 'under';
    } else {
      return 'on_track';
    }
  }

  /**
   * Calculate totals
   */
  private calculateTotals(items: BudgetVarianceItem[]) {
    const totalBudget = items.reduce((sum, item) => sum + item.budgetAmount, 0);
    const totalActual = items.reduce((sum, item) => sum + item.actualAmount, 0);
    const totalVariance = totalActual - totalBudget;
    const variancePercentage = totalBudget > 0 ? (totalVariance / totalBudget) * 100 : 0;

    return {
      totalBudget: this.round(totalBudget),
      totalActual: this.round(totalActual),
      totalVariance: this.round(totalVariance),
      variancePercentage: this.round(variancePercentage, 2),
    };
  }

  /**
   * Calculate category summaries
   */
  private calculateCategorySummaries(items: BudgetVarianceItem[]) {
    const categoryMap = new Map<string, {
      budgetAmount: number;
      actualAmount: number;
    }>();

    for (const item of items) {
      if (!categoryMap.has(item.category)) {
        categoryMap.set(item.category, {
          budgetAmount: 0,
          actualAmount: 0,
        });
      }

      const category = categoryMap.get(item.category)!;
      category.budgetAmount += item.budgetAmount;
      category.actualAmount += item.actualAmount;
    }

    return Array.from(categoryMap.entries()).map(([category, data]) => {
      const variance = data.actualAmount - data.budgetAmount;
      const variancePercentage = data.budgetAmount > 0 
        ? (variance / data.budgetAmount) * 100 
        : 0;

      return {
        category,
        budgetAmount: this.round(data.budgetAmount),
        actualAmount: this.round(data.actualAmount),
        variance: this.round(variance),
        variancePercentage: this.round(variancePercentage, 2),
      };
    });
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
        label = `${periodStart.toLocaleString('default', { month: 'long' })} ${periodStart.getFullYear()}`;
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
   * Helper: Round to specified decimal places
   */
  private round(value: number, decimals: number = 2): number {
    return Math.round(value * Math.pow(10, decimals)) / Math.pow(10, decimals);
  }
}

export default BudgetVarianceService;
