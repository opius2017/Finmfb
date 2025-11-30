/**
 * Accounts Receivable Service
 */

import type { Invoice, AgingReport, CustomerAging, AgingBucket, CreditLimit, ECLCalculation } from '../types/ar.types';

class ARService {
  async generateAgingReport(asOfDate: Date = new Date()): Promise<AgingReport> {
    const invoices = await this.getOutstandingInvoices();
    const customerMap = new Map<string, CustomerAging>();

    // Calculate aging for each invoice
    invoices.forEach(invoice => {
      const daysPastDue = this.calculateDaysPastDue(invoice.dueDate, asOfDate);
      invoice.daysPastDue = daysPastDue;
      invoice.agingBucket = this.getAgingBucket(daysPastDue);

      // Group by customer
      if (!customerMap.has(invoice.customerId)) {
        customerMap.set(invoice.customerId, {
          customerId: invoice.customerId,
          customerName: invoice.customerName,
          totalOutstanding: 0,
          current: 0,
          days30: 0,
          days60: 0,
          days90: 0,
          over90: 0,
          invoices: [],
        });
      }

      const customer = customerMap.get(invoice.customerId)!;
      customer.totalOutstanding += invoice.balance;
      customer.invoices.push(invoice);

      // Categorize by bucket
      switch (invoice.agingBucket) {
        case '0-30': customer.current += invoice.balance; break;
        case '31-60': customer.days30 += invoice.balance; break;
        case '61-90': customer.days60 += invoice.balance; break;
        case '90+': customer.over90 += invoice.balance; break;
      }
    });

    // Calculate bucket totals
    const buckets = {
      '0-30': { count: 0, amount: 0 },
      '31-60': { count: 0, amount: 0 },
      '61-90': { count: 0, amount: 0 },
      '90+': { count: 0, amount: 0 },
    };

    invoices.forEach(inv => {
      buckets[inv.agingBucket].count++;
      buckets[inv.agingBucket].amount += inv.balance;
    });

    return {
      asOfDate,
      totalOutstanding: invoices.reduce((sum, inv) => sum + inv.balance, 0),
      buckets,
      customers: Array.from(customerMap.values()),
    };
  }

  async checkCreditLimit(customerId: string, amount: number): Promise<{ approved: boolean; reason?: string }> {
    const creditLimit = await this.getCreditLimit(customerId);
    
    if (creditLimit.available >= amount) {
      return { approved: true };
    }
    
    return {
      approved: false,
      reason: `Credit limit exceeded. Available: ${creditLimit.available}, Requested: ${amount}`,
    };
  }

  async calculateECL(customerId: string): Promise<ECLCalculation> {
    const invoices = await this.getCustomerInvoices(customerId);
    const overdueCount = invoices.filter(inv => inv.daysPastDue > 0).length;
    const totalCount = invoices.length;
    
    // Simple ECL calculation (in production, use more sophisticated model)
    let stage: 1 | 2 | 3 = 1;
    let provisionRate = 0.01; // 1% for stage 1

    if (overdueCount / totalCount > 0.3) {
      stage = 2;
      provisionRate = 0.05; // 5% for stage 2
    }
    
    if (overdueCount / totalCount > 0.6) {
      stage = 3;
      provisionRate = 0.5; // 50% for stage 3
    }

    const totalOutstanding = invoices.reduce((sum, inv) => sum + inv.balance, 0);
    
    return {
      customerId,
      stage,
      provisionRate,
      provisionAmount: totalOutstanding * provisionRate,
      lastAssessmentDate: new Date(),
    };
  }

  private calculateDaysPastDue(dueDate: Date, asOfDate: Date): number {
    const diff = asOfDate.getTime() - new Date(dueDate).getTime();
    return Math.max(0, Math.floor(diff / (1000 * 60 * 60 * 24)));
  }

  private getAgingBucket(daysPastDue: number): AgingBucket {
    if (daysPastDue <= 30) return '0-30';
    if (daysPastDue <= 60) return '31-60';
    if (daysPastDue <= 90) return '61-90';
    return '90+';
  }

  private async getOutstandingInvoices(): Promise<Invoice[]> {
    // Mock data - replace with API call
    return [
      {
        id: '1',
        invoiceNumber: 'INV-001',
        customerId: 'cust-1',
        customerName: 'Acme Corp',
        invoiceDate: new Date('2024-01-01'),
        dueDate: new Date('2024-01-31'),
        amount: 50000,
        paidAmount: 0,
        balance: 50000,
        status: 'overdue',
        daysPastDue: 0,
        agingBucket: '0-30',
      },
    ];
  }

  private async getCreditLimit(customerId: string): Promise<CreditLimit> {
    return {
      customerId,
      limit: 100000,
      utilized: 50000,
      available: 50000,
      lastReviewDate: new Date(),
    };
  }

  private async getCustomerInvoices(customerId: string): Promise<Invoice[]> {
    const all = await this.getOutstandingInvoices();
    return all.filter(inv => inv.customerId === customerId);
  }
}

export const arService = new ARService();
