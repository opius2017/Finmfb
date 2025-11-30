import { Decimal } from '@prisma/client/runtime/library';
import { RepositoryFactory } from '@repositories/index';

export interface AgingBucket {
  label: string;
  minDays: number;
  maxDays: number | null;
  amount: Decimal;
  count: number;
  percentage: number;
}

export interface AgingAnalysisResult {
  asOfDate: Date;
  totalAmount: Decimal;
  totalCount: number;
  buckets: AgingBucket[];
  details: AgingDetail[];
}

export interface AgingDetail {
  entityId: string;
  entityName: string;
  amount: Decimal;
  dueDate: Date;
  daysOverdue: number;
  bucket: string;
}

export interface CustomerAgingResult {
  customerId: string;
  customerName: string;
  totalAmount: Decimal;
  buckets: AgingBucket[];
}

export class AgingAnalysisService {
  private accountRepository = RepositoryFactory.getAccountRepository();
  private loanRepository = RepositoryFactory.getLoanRepository();

  /**
   * Standard aging buckets
   */
  private readonly AGING_BUCKETS = [
    { label: 'Current', minDays: 0, maxDays: 0 },
    { label: '1-30 days', minDays: 1, maxDays: 30 },
    { label: '31-60 days', minDays: 31, maxDays: 60 },
    { label: '61-90 days', minDays: 61, maxDays: 90 },
    { label: '90+ days', minDays: 91, maxDays: null },
  ];

  /**
   * Calculate days overdue
   */
  private calculateDaysOverdue(dueDate: Date, asOfDate: Date): number {
    const diffTime = asOfDate.getTime() - dueDate.getTime();
    const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24));
    return Math.max(0, diffDays);
  }

  /**
   * Determine aging bucket for days overdue
   */
  private getAgingBucket(daysOverdue: number): string {
    for (const bucket of this.AGING_BUCKETS) {
      if (daysOverdue >= bucket.minDays && 
          (bucket.maxDays === null || daysOverdue <= bucket.maxDays)) {
        return bucket.label;
      }
    }
    return 'Current';
  }

  /**
   * Calculate accounts receivable aging
   */
  async calculateARAging(asOfDate: Date = new Date()): Promise<AgingAnalysisResult> {
    // Get all active loans with outstanding balances
    const loans = await this.loanRepository.findMany({
      where: {
        status: {
          in: ['ACTIVE', 'DISBURSED'],
        },
        outstandingBalance: {
          gt: 0,
        },
      },
      include: {
        member: true,
        schedules: {
          where: {
            isPaid: false,
            dueDate: {
              lte: asOfDate,
            },
          },
          orderBy: {
            dueDate: 'asc',
          },
        },
      },
    });

    const details: AgingDetail[] = [];
    const bucketMap = new Map<string, { amount: number; count: number }>();

    // Initialize buckets
    this.AGING_BUCKETS.forEach((bucket) => {
      bucketMap.set(bucket.label, { amount: 0, count: 0 });
    });

    // Process each loan
    for (const loan of loans) {
      for (const schedule of loan.schedules) {
        const daysOverdue = this.calculateDaysOverdue(schedule.dueDate, asOfDate);
        const bucket = this.getAgingBucket(daysOverdue);
        const amount = Number(schedule.totalPayment) - Number(schedule.paidAmount);

        // Add to bucket
        const bucketData = bucketMap.get(bucket)!;
        bucketData.amount += amount;
        bucketData.count += 1;

        // Add detail
        details.push({
          entityId: loan.id,
          entityName: `${loan.member.firstName} ${loan.member.lastName}`,
          amount: new Decimal(amount.toFixed(2)),
          dueDate: schedule.dueDate,
          daysOverdue,
          bucket,
        });
      }
    }

    // Calculate totals
    const totalAmount = Array.from(bucketMap.values()).reduce(
      (sum, bucket) => sum + bucket.amount,
      0
    );
    const totalCount = Array.from(bucketMap.values()).reduce(
      (sum, bucket) => sum + bucket.count,
      0
    );

    // Build buckets array
    const buckets: AgingBucket[] = this.AGING_BUCKETS.map((bucketDef) => {
      const data = bucketMap.get(bucketDef.label)!;
      return {
        ...bucketDef,
        amount: new Decimal(data.amount.toFixed(2)),
        count: data.count,
        percentage: totalAmount > 0 ? (data.amount / totalAmount) * 100 : 0,
      };
    });

    return {
      asOfDate,
      totalAmount: new Decimal(totalAmount.toFixed(2)),
      totalCount,
      buckets,
      details,
    };
  }

  /**
   * Calculate accounts payable aging
   */
  async calculateAPAging(asOfDate: Date = new Date()): Promise<AgingAnalysisResult> {
    // For AP aging, we would query vendor invoices or payables
    // This is a placeholder implementation
    const details: AgingDetail[] = [];
    const bucketMap = new Map<string, { amount: number; count: number }>();

    // Initialize buckets
    this.AGING_BUCKETS.forEach((bucket) => {
      bucketMap.set(bucket.label, { amount: 0, count: 0 });
    });

    // TODO: Implement actual AP aging logic when vendor/payables module is added
    // For now, return empty result
    const totalAmount = 0;
    const totalCount = 0;

    const buckets: AgingBucket[] = this.AGING_BUCKETS.map((bucketDef) => {
      const data = bucketMap.get(bucketDef.label)!;
      return {
        ...bucketDef,
        amount: new Decimal(data.amount.toFixed(2)),
        count: data.count,
        percentage: 0,
      };
    });

    return {
      asOfDate,
      totalAmount: new Decimal(totalAmount.toFixed(2)),
      totalCount,
      buckets,
      details,
    };
  }

  /**
   * Calculate aging by customer/member
   */
  async calculateAgingByCustomer(asOfDate: Date = new Date()): Promise<CustomerAgingResult[]> {
    const arAging = await this.calculateARAging(asOfDate);
    
    // Group by customer
    const customerMap = new Map<string, {
      name: string;
      buckets: Map<string, { amount: number; count: number }>;
    }>();

    for (const detail of arAging.details) {
      if (!customerMap.has(detail.entityId)) {
        customerMap.set(detail.entityId, {
          name: detail.entityName,
          buckets: new Map(
            this.AGING_BUCKETS.map((b) => [b.label, { amount: 0, count: 0 }])
          ),
        });
      }

      const customer = customerMap.get(detail.entityId)!;
      const bucketData = customer.buckets.get(detail.bucket)!;
      bucketData.amount += Number(detail.amount);
      bucketData.count += 1;
    }

    // Build result
    const results: CustomerAgingResult[] = [];
    
    for (const [customerId, customer] of customerMap.entries()) {
      const totalAmount = Array.from(customer.buckets.values()).reduce(
        (sum, bucket) => sum + bucket.amount,
        0
      );

      const buckets: AgingBucket[] = this.AGING_BUCKETS.map((bucketDef) => {
        const data = customer.buckets.get(bucketDef.label)!;
        return {
          ...bucketDef,
          amount: new Decimal(data.amount.toFixed(2)),
          count: data.count,
          percentage: totalAmount > 0 ? (data.amount / totalAmount) * 100 : 0,
        };
      });

      results.push({
        customerId,
        customerName: customer.name,
        totalAmount: new Decimal(totalAmount.toFixed(2)),
        buckets,
      });
    }

    // Sort by total amount descending
    results.sort((a, b) => Number(b.totalAmount) - Number(a.totalAmount));

    return results;
  }

  /**
   * Generate aging summary report
   */
  async generateAgingSummaryReport(
    type: 'AR' | 'AP',
    asOfDate: Date = new Date()
  ): Promise<{
    summary: AgingAnalysisResult;
    byCustomer: CustomerAgingResult[];
    insights: {
      overduePercentage: number;
      averageDaysOverdue: number;
      largestOverdueAmount: Decimal;
      customersWithOverdue: number;
    };
  }> {
    const summary = type === 'AR' 
      ? await this.calculateARAging(asOfDate)
      : await this.calculateAPAging(asOfDate);
    
    const byCustomer = type === 'AR'
      ? await this.calculateAgingByCustomer(asOfDate)
      : [];

    // Calculate insights
    const overdueDetails = summary.details.filter((d) => d.daysOverdue > 0);
    const overdueAmount = overdueDetails.reduce(
      (sum, d) => sum + Number(d.amount),
      0
    );
    const overduePercentage = Number(summary.totalAmount) > 0
      ? (overdueAmount / Number(summary.totalAmount)) * 100
      : 0;

    const averageDaysOverdue = overdueDetails.length > 0
      ? overdueDetails.reduce((sum, d) => sum + d.daysOverdue, 0) / overdueDetails.length
      : 0;

    const largestOverdueAmount = overdueDetails.length > 0
      ? new Decimal(
          Math.max(...overdueDetails.map((d) => Number(d.amount))).toFixed(2)
        )
      : new Decimal(0);

    const customersWithOverdue = new Set(
      overdueDetails.map((d) => d.entityId)
    ).size;

    return {
      summary,
      byCustomer,
      insights: {
        overduePercentage,
        averageDaysOverdue,
        largestOverdueAmount,
        customersWithOverdue,
      },
    };
  }

  /**
   * Get aging trend over time
   */
  async getAgingTrend(
    type: 'AR' | 'AP',
    startDate: Date,
    endDate: Date,
    interval: 'daily' | 'weekly' | 'monthly' = 'monthly'
  ): Promise<Array<{
    date: Date;
    totalAmount: Decimal;
    overdueAmount: Decimal;
    overduePercentage: number;
  }>> {
    const results: Array<{
      date: Date;
      totalAmount: Decimal;
      overdueAmount: Decimal;
      overduePercentage: number;
    }> = [];

    const currentDate = new Date(startDate);
    
    while (currentDate <= endDate) {
      const aging = type === 'AR'
        ? await this.calculateARAging(currentDate)
        : await this.calculateAPAging(currentDate);

      const overdueAmount = aging.buckets
        .filter((b) => b.minDays > 0)
        .reduce((sum, b) => sum + Number(b.amount), 0);

      const overduePercentage = Number(aging.totalAmount) > 0
        ? (overdueAmount / Number(aging.totalAmount)) * 100
        : 0;

      results.push({
        date: new Date(currentDate),
        totalAmount: aging.totalAmount,
        overdueAmount: new Decimal(overdueAmount.toFixed(2)),
        overduePercentage,
      });

      // Move to next interval
      if (interval === 'daily') {
        currentDate.setDate(currentDate.getDate() + 1);
      } else if (interval === 'weekly') {
        currentDate.setDate(currentDate.getDate() + 7);
      } else {
        currentDate.setMonth(currentDate.getMonth() + 1);
      }
    }

    return results;
  }
}

export default AgingAnalysisService;
