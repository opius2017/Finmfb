import { arService } from '../services/arService';

describe('AR Service', () => {
  describe('Aging Report Generation', () => {
    it('should generate aging report', async () => {
      const report = await arService.generateAgingReport();

      expect(report).toBeDefined();
      expect(report.asOfDate).toBeInstanceOf(Date);
      expect(report.totalOutstanding).toBeGreaterThanOrEqual(0);
      expect(report.buckets).toBeDefined();
      expect(report.customers).toBeInstanceOf(Array);
    });

    it('should categorize invoices into aging buckets', async () => {
      const report = await arService.generateAgingReport();

      expect(report.buckets['0-30']).toBeDefined();
      expect(report.buckets['31-60']).toBeDefined();
      expect(report.buckets['61-90']).toBeDefined();
      expect(report.buckets['90+']).toBeDefined();
    });

    it('should calculate customer aging correctly', async () => {
      const report = await arService.generateAgingReport();

      report.customers.forEach(customer => {
        expect(customer.customerId).toBeDefined();
        expect(customer.customerName).toBeDefined();
        expect(customer.totalOutstanding).toBeGreaterThanOrEqual(0);
        expect(customer.current).toBeGreaterThanOrEqual(0);
        expect(customer.days30).toBeGreaterThanOrEqual(0);
        expect(customer.days60).toBeGreaterThanOrEqual(0);
        expect(customer.over90).toBeGreaterThanOrEqual(0);
      });
    });
  });

  describe('Credit Limit Management', () => {
    it('should approve transaction within credit limit', async () => {
      const result = await arService.checkCreditLimit('cust-1', 10000);

      expect(result.approved).toBe(true);
      expect(result.reason).toBeUndefined();
    });

    it('should deny transaction exceeding credit limit', async () => {
      const result = await arService.checkCreditLimit('cust-1', 100000);

      expect(result.approved).toBe(false);
      expect(result.reason).toBeDefined();
      expect(result.reason).toContain('Credit limit exceeded');
    });
  });

  describe('ECL Calculation', () => {
    it('should calculate ECL for customer', async () => {
      const ecl = await arService.calculateECL('cust-1');

      expect(ecl).toBeDefined();
      expect(ecl.customerId).toBe('cust-1');
      expect(ecl.stage).toBeGreaterThanOrEqual(1);
      expect(ecl.stage).toBeLessThanOrEqual(3);
      expect(ecl.provisionRate).toBeGreaterThanOrEqual(0);
      expect(ecl.provisionAmount).toBeGreaterThanOrEqual(0);
    });

    it('should assign correct stage based on overdue ratio', async () => {
      const ecl = await arService.calculateECL('cust-1');

      if (ecl.stage === 1) {
        expect(ecl.provisionRate).toBe(0.01);
      } else if (ecl.stage === 2) {
        expect(ecl.provisionRate).toBe(0.05);
      } else if (ecl.stage === 3) {
        expect(ecl.provisionRate).toBe(0.5);
      }
    });
  });

  describe('Days Past Due Calculation', () => {
    it('should calculate days past due correctly', () => {
      const dueDate = new Date('2024-01-01');
      const asOfDate = new Date('2024-01-31');
      
      const daysPastDue = arService['calculateDaysPastDue'](dueDate, asOfDate);
      
      expect(daysPastDue).toBe(30);
    });

    it('should return 0 for future due dates', () => {
      const dueDate = new Date('2024-12-31');
      const asOfDate = new Date('2024-01-01');
      
      const daysPastDue = arService['calculateDaysPastDue'](dueDate, asOfDate);
      
      expect(daysPastDue).toBe(0);
    });
  });

  describe('Aging Bucket Assignment', () => {
    it('should assign 0-30 bucket correctly', () => {
      const bucket = arService['getAgingBucket'](15);
      expect(bucket).toBe('0-30');
    });

    it('should assign 31-60 bucket correctly', () => {
      const bucket = arService['getAgingBucket'](45);
      expect(bucket).toBe('31-60');
    });

    it('should assign 61-90 bucket correctly', () => {
      const bucket = arService['getAgingBucket'](75);
      expect(bucket).toBe('61-90');
    });

    it('should assign 90+ bucket correctly', () => {
      const bucket = arService['getAgingBucket'](120);
      expect(bucket).toBe('90+');
    });
  });
});
