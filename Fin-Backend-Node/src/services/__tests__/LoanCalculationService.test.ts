import { LoanCalculationService } from '../LoanCalculationService';
import { Decimal } from '@prisma/client/runtime/library';

describe('LoanCalculationService', () => {
  let service: LoanCalculationService;

  beforeEach(() => {
    service = new LoanCalculationService();
  });

  describe('calculateReducingBalanceSchedule', () => {
    it('should calculate correct reducing balance schedule', () => {
      const params = {
        principal: 100000,
        interestRate: 0.12, // 12% annual
        termMonths: 12,
        startDate: new Date('2024-01-01'),
        method: 'reducing_balance' as const,
      };

      const schedule = service.calculateReducingBalanceSchedule(params);

      expect(schedule).toHaveLength(12);
      expect(schedule[0].paymentNumber).toBe(1);
      expect(Number(schedule[11].balance)).toBeCloseTo(0, 0);
      
      // Total principal should equal loan amount
      const totalPrincipal = schedule.reduce(
        (sum, item) => sum + Number(item.principal),
        0
      );
      expect(totalPrincipal).toBeCloseTo(params.principal, 0);
    });

    it('should have decreasing interest and increasing principal over time', () => {
      const params = {
        principal: 100000,
        interestRate: 0.12,
        termMonths: 12,
        startDate: new Date('2024-01-01'),
        method: 'reducing_balance' as const,
      };

      const schedule = service.calculateReducingBalanceSchedule(params);

      // Interest should decrease
      expect(Number(schedule[0].interest)).toBeGreaterThan(
        Number(schedule[11].interest)
      );

      // Principal should increase
      expect(Number(schedule[0].principal)).toBeLessThan(
        Number(schedule[11].principal)
      );
    });
  });

  describe('calculateFlatRateSchedule', () => {
    it('should calculate correct flat rate schedule', () => {
      const params = {
        principal: 100000,
        interestRate: 0.12,
        termMonths: 12,
        startDate: new Date('2024-01-01'),
        method: 'flat_rate' as const,
      };

      const schedule = service.calculateFlatRateSchedule(params);

      expect(schedule).toHaveLength(12);
      
      // All payments should be equal
      const firstPayment = Number(schedule[0].totalPayment);
      schedule.forEach((item) => {
        expect(Number(item.totalPayment)).toBeCloseTo(firstPayment, 2);
      });

      // Interest should be constant
      const firstInterest = Number(schedule[0].interest);
      schedule.forEach((item) => {
        expect(Number(item.interest)).toBeCloseTo(firstInterest, 2);
      });
    });
  });

  describe('calculateInterestAccrual', () => {
    it('should calculate daily interest correctly', () => {
      const principal = 100000;
      const annualRate = 0.12;
      const days = 30;

      const interest = service.calculateInterestAccrual(principal, annualRate, days);

      // Expected: 100000 * (0.12/365) * 30 = 986.30
      expect(Number(interest)).toBeCloseTo(986.30, 2);
    });

    it('should return zero for zero days', () => {
      const interest = service.calculateInterestAccrual(100000, 0.12, 0);
      expect(Number(interest)).toBe(0);
    });
  });

  describe('calculatePeriodInterest', () => {
    it('should calculate interest for date range', () => {
      const principal = 100000;
      const annualRate = 0.12;
      const startDate = new Date('2024-01-01');
      const endDate = new Date('2024-01-31');

      const interest = service.calculatePeriodInterest(
        principal,
        annualRate,
        startDate,
        endDate
      );

      // 30 days of interest
      expect(Number(interest)).toBeCloseTo(986.30, 2);
    });
  });

  describe('calculatePenalty', () => {
    it('should calculate penalty for overdue payment', () => {
      const dueAmount = 10000;
      const dueDate = new Date('2024-01-01');
      const currentDate = new Date('2024-01-11'); // 10 days overdue
      const penaltyRate = 0.01; // 1% per day

      const result = service.calculatePenalty(
        dueAmount,
        dueDate,
        currentDate,
        penaltyRate
      );

      expect(result.daysOverdue).toBe(10);
      // Penalty: 10000 * 0.01 * 10 = 1000
      expect(Number(result.penaltyAmount)).toBeCloseTo(1000, 2);
      expect(Number(result.totalAmountDue)).toBeCloseTo(11000, 2);
    });

    it('should return zero penalty for on-time payment', () => {
      const dueAmount = 10000;
      const dueDate = new Date('2024-01-01');
      const currentDate = new Date('2024-01-01');
      const penaltyRate = 0.01;

      const result = service.calculatePenalty(
        dueAmount,
        dueDate,
        currentDate,
        penaltyRate
      );

      expect(result.daysOverdue).toBe(0);
      expect(Number(result.penaltyAmount)).toBe(0);
      expect(Number(result.totalAmountDue)).toBe(dueAmount);
    });
  });

  describe('calculateTotalLoanCost', () => {
    it('should calculate total cost correctly', () => {
      const params = {
        principal: 100000,
        interestRate: 0.12,
        termMonths: 12,
        startDate: new Date('2024-01-01'),
        method: 'reducing_balance' as const,
      };

      const schedule = service.calculateReducingBalanceSchedule(params);
      const cost = service.calculateTotalLoanCost(schedule);

      expect(Number(cost.totalPrincipal)).toBeCloseTo(params.principal, 0);
      expect(Number(cost.totalInterest)).toBeGreaterThan(0);
      expect(Number(cost.totalAmount)).toBe(
        Number(cost.totalPrincipal) + Number(cost.totalInterest)
      );
    });
  });

  describe('calculateEarlyPayment', () => {
    it('should calculate early payment impact', () => {
      const params = {
        principal: 100000,
        interestRate: 0.12,
        termMonths: 12,
        startDate: new Date('2024-01-01'),
        method: 'reducing_balance' as const,
      };

      const schedule = service.calculateReducingBalanceSchedule(params);
      const paymentDate = new Date('2024-06-01'); // After 5 months
      const paymentAmount = 20000;

      const result = service.calculateEarlyPayment(
        schedule,
        paymentAmount,
        paymentDate,
        params.interestRate
      );

      expect(Number(result.remainingBalance)).toBeLessThan(params.principal);
      expect(Number(result.interestSaved)).toBeGreaterThan(0);
      expect(result.revisedSchedule.length).toBeGreaterThan(0);
    });
  });

  describe('calculateEffectiveRate', () => {
    it('should calculate effective interest rate', () => {
      const principal = 100000;
      const totalInterest = 6000;
      const termMonths = 12;

      const effectiveRate = service.calculateEffectiveRate(
        principal,
        totalInterest,
        termMonths
      );

      expect(effectiveRate).toBeGreaterThan(0);
      expect(effectiveRate).toBeLessThan(1); // Should be less than 100%
    });
  });
});
