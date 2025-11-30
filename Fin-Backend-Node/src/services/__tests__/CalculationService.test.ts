import { CalculationService, ExtendedCalculationService } from '../CalculationService';

describe('CalculationService', () => {
  let service: CalculationService;

  beforeEach(() => {
    service = new CalculationService();
  });

  describe('Loan Amortization - Reducing Balance', () => {
    it('should calculate reducing balance schedule correctly', () => {
      const params = {
        principal: 100000,
        interestRate: 12, // 12% annual
        termMonths: 12,
        method: 'reducing_balance' as const,
        startDate: new Date('2024-01-01'),
        paymentFrequency: 'monthly' as const,
      };

      const schedule = service.calculateLoanSchedule(params);

      expect(schedule).toHaveLength(12);
      expect(schedule[0].balance).toBeLessThan(params.principal);
      expect(schedule[11].balance).toBeCloseTo(0, 2);

      // Verify total principal equals loan amount
      const totalPrincipal = schedule.reduce((sum, item) => sum + item.principal, 0);
      expect(totalPrincipal).toBeCloseTo(params.principal, 2);

      // Verify balance decreases over time
      for (let i = 1; i < schedule.length; i++) {
        expect(schedule[i].balance).toBeLessThan(schedule[i - 1].balance);
      }
    });

    it('should handle grace period correctly', () => {
      const params = {
        principal: 100000,
        interestRate: 12,
        termMonths: 12,
        method: 'reducing_balance' as const,
        startDate: new Date('2024-01-01'),
        paymentFrequency: 'monthly' as const,
        gracePeriodMonths: 3,
      };

      const schedule = service.calculateLoanSchedule(params);

      // During grace period, only interest is paid
      expect(schedule[0].principal).toBe(0);
      expect(schedule[1].principal).toBe(0);
      expect(schedule[2].principal).toBe(0);

      // After grace period, principal payments start
      expect(schedule[3].principal).toBeGreaterThan(0);
    });

    it('should calculate quarterly payments correctly', () => {
      const params = {
        principal: 100000,
        interestRate: 12,
        termMonths: 12,
        method: 'reducing_balance' as const,
        startDate: new Date('2024-01-01'),
        paymentFrequency: 'quarterly' as const,
      };

      const schedule = service.calculateLoanSchedule(params);

      expect(schedule).toHaveLength(4); // 4 quarterly payments
      expect(schedule[3].balance).toBeCloseTo(0, 2);
    });
  });

  describe('Loan Amortization - Flat Rate', () => {
    it('should calculate flat rate schedule correctly', () => {
      const params = {
        principal: 100000,
        interestRate: 12,
        termMonths: 12,
        method: 'flat_rate' as const,
        startDate: new Date('2024-01-01'),
        paymentFrequency: 'monthly' as const,
      };

      const schedule = service.calculateLoanSchedule(params);

      expect(schedule).toHaveLength(12);

      // In flat rate, interest per payment should be constant
      const firstInterest = schedule[0].interest;
      for (let i = 1; i < schedule.length - 1; i++) {
        expect(schedule[i].interest).toBeCloseTo(firstInterest, 2);
      }

      // Principal per payment should be constant (except last payment)
      const firstPrincipal = schedule[0].principal;
      for (let i = 1; i < schedule.length - 1; i++) {
        expect(schedule[i].principal).toBeCloseTo(firstPrincipal, 2);
      }

      expect(schedule[11].balance).toBeCloseTo(0, 2);
    });
  });

  describe('Interest Accrual', () => {
    it('should calculate simple interest correctly', () => {
      const params = {
        principal: 100000,
        annualRate: 12,
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-01-31'), // 30 days
        method: 'simple' as const,
      };

      const interest = service.calculateInterestAccrual(params);

      // Expected: 100000 * 0.12 / 365 * 30 = 986.30
      expect(interest).toBeCloseTo(986.30, 2);
    });

    it('should calculate compound interest correctly', () => {
      const params = {
        principal: 100000,
        annualRate: 12,
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-12-31'), // 365 days
        method: 'compound' as const,
      };

      const interest = service.calculateInterestAccrual(params);

      // Compound interest should be higher than simple interest
      expect(interest).toBeGreaterThan(12000);
    });

    it('should throw error for invalid date range', () => {
      const params = {
        principal: 100000,
        annualRate: 12,
        startDate: new Date('2024-01-31'),
        endDate: new Date('2024-01-01'),
        method: 'simple' as const,
      };

      expect(() => service.calculateInterestAccrual(params)).toThrow();
    });
  });

  describe('Penalty Calculation', () => {
    it('should calculate percentage-based penalty correctly', () => {
      const params = {
        overdueAmount: 10000,
        daysOverdue: 30,
        penaltyRate: 0.1, // 0.1% per day
        penaltyType: 'percentage' as const,
      };

      const penalty = service.calculatePenalty(params);

      // Expected: 10000 * 0.001 * 30 = 300
      expect(penalty).toBeCloseTo(300, 2);
    });

    it('should calculate flat penalty correctly', () => {
      const params = {
        overdueAmount: 10000,
        daysOverdue: 30,
        penaltyRate: 0,
        penaltyType: 'flat' as const,
        flatAmount: 500,
      };

      const penalty = service.calculatePenalty(params);

      expect(penalty).toBe(500);
    });

    it('should return zero for no overdue days', () => {
      const params = {
        overdueAmount: 10000,
        daysOverdue: 0,
        penaltyRate: 0.1,
        penaltyType: 'percentage' as const,
      };

      const penalty = service.calculatePenalty(params);

      expect(penalty).toBe(0);
    });
  });

  describe('Early Payment Penalty', () => {
    it('should calculate early payment penalty correctly', () => {
      const params = {
        outstandingBalance: 50000,
        remainingTermMonths: 6,
        earlyPaymentPenaltyRate: 2, // 2% of outstanding balance
      };

      const penalty = service.calculateEarlyPaymentPenalty(params);

      // Expected: 50000 * 0.02 = 1000
      expect(penalty).toBe(1000);
    });

    it('should return zero when no penalty rate', () => {
      const params = {
        outstandingBalance: 50000,
        remainingTermMonths: 6,
        earlyPaymentPenaltyRate: 0,
      };

      const penalty = service.calculateEarlyPaymentPenalty(params);

      expect(penalty).toBe(0);
    });
  });

  describe('Effective Interest Rate', () => {
    it('should calculate effective rate for monthly compounding', () => {
      const effectiveRate = service.calculateEffectiveRate(12, 12);

      // Effective rate should be slightly higher than nominal rate
      expect(effectiveRate).toBeGreaterThan(12);
      expect(effectiveRate).toBeCloseTo(12.68, 2);
    });

    it('should calculate effective rate for quarterly compounding', () => {
      const effectiveRate = service.calculateEffectiveRate(12, 4);

      expect(effectiveRate).toBeGreaterThan(12);
      expect(effectiveRate).toBeCloseTo(12.55, 2);
    });
  });

  describe('Total Loan Cost', () => {
    it('should calculate total loan cost correctly', () => {
      const params = {
        principal: 100000,
        interestRate: 12,
        termMonths: 12,
        method: 'reducing_balance' as const,
        startDate: new Date('2024-01-01'),
        paymentFrequency: 'monthly' as const,
      };

      const schedule = service.calculateLoanSchedule(params);
      const cost = service.calculateTotalLoanCost(schedule);

      expect(cost.totalPrincipal).toBeCloseTo(100000, 2);
      expect(cost.totalInterest).toBeGreaterThan(0);
      expect(cost.totalPayments).toBeCloseTo(cost.totalPrincipal + cost.totalInterest, 2);
      expect(cost.averageMonthlyPayment).toBeGreaterThan(0);
    });
  });

  describe('Validation', () => {
    it('should throw error for invalid principal', () => {
      const params = {
        principal: -100,
        interestRate: 12,
        termMonths: 12,
        method: 'reducing_balance' as const,
        startDate: new Date('2024-01-01'),
        paymentFrequency: 'monthly' as const,
      };

      expect(() => service.calculateLoanSchedule(params)).toThrow('Principal must be greater than 0');
    });

    it('should throw error for negative interest rate', () => {
      const params = {
        principal: 100000,
        interestRate: -5,
        termMonths: 12,
        method: 'reducing_balance' as const,
        startDate: new Date('2024-01-01'),
        paymentFrequency: 'monthly' as const,
      };

      expect(() => service.calculateLoanSchedule(params)).toThrow('Interest rate cannot be negative');
    });

    it('should throw error for invalid term', () => {
      const params = {
        principal: 100000,
        interestRate: 12,
        termMonths: 0,
        method: 'reducing_balance' as const,
        startDate: new Date('2024-01-01'),
        paymentFrequency: 'monthly' as const,
      };

      expect(() => service.calculateLoanSchedule(params)).toThrow('Term must be greater than 0');
    });
  });
});

describe('ExtendedCalculationService - Aging Analysis', () => {
  let service: ExtendedCalculationService;

  beforeEach(() => {
    service = new ExtendedCalculationService();
  });

  it('should calculate aging analysis correctly', () => {
    const params = {
      accountType: 'AR' as const,
      asOfDate: new Date('2024-01-31'),
      items: [
        {
          id: '1',
          customerId: 'cust1',
          amount: 10000,
          dueDate: new Date('2024-01-31'), // Current
        },
        {
          id: '2',
          customerId: 'cust1',
          amount: 5000,
          dueDate: new Date('2024-01-15'), // 16 days overdue
        },
        {
          id: '3',
          customerId: 'cust2',
          amount: 8000,
          dueDate: new Date('2023-12-01'), // 61 days overdue
        },
        {
          id: '4',
          customerId: 'cust2',
          amount: 12000,
          dueDate: new Date('2023-10-01'), // 122 days overdue
        },
      ],
    };

    const report = service.calculateAgingAnalysis(params);

    expect(report.totalAmount).toBe(35000);
    expect(report.totalCount).toBe(4);
    expect(report.buckets).toHaveLength(6);

    // Check bucket amounts
    expect(report.buckets[0].amount).toBe(10000); // Current
    expect(report.buckets[1].amount).toBe(5000); // 1-30 days
    expect(report.buckets[3].amount).toBe(8000); // 61-90 days
    expect(report.buckets[5].amount).toBe(12000); // Over 120 days

    // Check by entity
    expect(report.byEntity).toHaveLength(2);
    expect(report.byEntity[0].totalAmount).toBe(20000); // cust2
    expect(report.byEntity[1].totalAmount).toBe(15000); // cust1
  });

  it('should calculate percentages correctly', () => {
    const params = {
      accountType: 'AR' as const,
      asOfDate: new Date('2024-01-31'),
      items: [
        {
          id: '1',
          customerId: 'cust1',
          amount: 50000,
          dueDate: new Date('2024-01-31'),
        },
        {
          id: '2',
          customerId: 'cust1',
          amount: 50000,
          dueDate: new Date('2024-01-01'),
        },
      ],
    };

    const report = service.calculateAgingAnalysis(params);

    expect(report.buckets[0].percentage).toBe(50); // Current
    expect(report.buckets[1].percentage).toBe(50); // 1-30 days
  });
});

describe('ExtendedCalculationService - Cash Flow Forecast', () => {
  let service: ExtendedCalculationService;

  beforeEach(() => {
    service = new ExtendedCalculationService();
  });

  it('should calculate cash flow forecast correctly', () => {
    const params = {
      startDate: new Date('2024-01-01'),
      endDate: new Date('2024-03-01'),
      scenarios: [
        {
          name: 'Optimistic',
          probability: 0.3,
          assumptions: {
            receivablesCollectionRate: 95,
            payablesPaymentRate: 100,
          },
        },
        {
          name: 'Most Likely',
          probability: 0.5,
          assumptions: {
            receivablesCollectionRate: 85,
            payablesPaymentRate: 100,
          },
        },
        {
          name: 'Pessimistic',
          probability: 0.2,
          assumptions: {
            receivablesCollectionRate: 70,
            payablesPaymentRate: 100,
          },
        },
      ],
    };

    const forecast = service.calculateCashFlowForecast(params);

    expect(forecast.scenarios).toHaveLength(3);
    expect(forecast.summary.weightedAverageNetCashFlow).toBeDefined();
    expect(forecast.summary.bestCase).toBeGreaterThanOrEqual(forecast.summary.worstCase);

    // Check that each scenario has periods
    forecast.scenarios.forEach((scenario) => {
      expect(scenario.periods.length).toBeGreaterThan(0);
      expect(scenario.totalInflows).toBeGreaterThan(0);
      expect(scenario.totalOutflows).toBeGreaterThan(0);
    });
  });

  it('should calculate cumulative cash flow correctly', () => {
    const params = {
      startDate: new Date('2024-01-01'),
      endDate: new Date('2024-02-01'),
      scenarios: [
        {
          name: 'Base',
          probability: 1.0,
          assumptions: {
            receivablesCollectionRate: 100,
            payablesPaymentRate: 100,
          },
        },
      ],
    };

    const forecast = service.calculateCashFlowForecast(params);
    const periods = forecast.scenarios[0].periods;

    // Cumulative should increase over time
    for (let i = 1; i < periods.length; i++) {
      expect(periods[i].cumulativeCashFlow).toBe(
        periods[i - 1].cumulativeCashFlow + periods[i].netCashFlow
      );
    }
  });
});
