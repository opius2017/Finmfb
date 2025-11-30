import { Decimal } from '@prisma/client/runtime/library';

export interface LoanScheduleItem {
  paymentNumber: number;
  dueDate: Date;
  principal: Decimal;
  interest: Decimal;
  totalPayment: Decimal;
  balance: Decimal;
}

export interface LoanCalculationParams {
  principal: number;
  interestRate: number; // Annual rate as decimal (e.g., 0.12 for 12%)
  termMonths: number;
  startDate: Date;
  method: 'reducing_balance' | 'flat_rate';
  paymentFrequency?: 'monthly' | 'quarterly';
}

export interface EarlyPaymentResult {
  remainingBalance: Decimal;
  interestSaved: Decimal;
  newMaturityDate: Date;
  revisedSchedule: LoanScheduleItem[];
}

export interface PenaltyCalculation {
  daysOverdue: number;
  penaltyAmount: Decimal;
  totalAmountDue: Decimal;
}

export class LoanCalculationService {
  /**
   * Calculate loan amortization schedule using reducing balance method
   */
  calculateReducingBalanceSchedule(params: LoanCalculationParams): LoanScheduleItem[] {
    const { principal, interestRate, termMonths, startDate, paymentFrequency = 'monthly' } = params;
    
    const monthlyRate = interestRate / 12;
    const schedule: LoanScheduleItem[] = [];
    
    // Calculate monthly payment using reducing balance formula
    // PMT = P * [r(1+r)^n] / [(1+r)^n - 1]
    const monthlyPayment = 
      (principal * monthlyRate * Math.pow(1 + monthlyRate, termMonths)) /
      (Math.pow(1 + monthlyRate, termMonths) - 1);
    
    let balance = principal;
    const paymentInterval = paymentFrequency === 'quarterly' ? 3 : 1;
    
    for (let i = 1; i <= termMonths; i += paymentInterval) {
      const dueDate = new Date(startDate);
      dueDate.setMonth(dueDate.getMonth() + i);
      
      const interest = balance * monthlyRate * paymentInterval;
      const principalPayment = monthlyPayment * paymentInterval - interest;
      balance -= principalPayment;
      
      // Ensure balance doesn't go negative due to rounding
      if (balance < 0) balance = 0;
      
      schedule.push({
        paymentNumber: Math.floor(i / paymentInterval) + 1,
        dueDate,
        principal: new Decimal(principalPayment.toFixed(2)),
        interest: new Decimal(interest.toFixed(2)),
        totalPayment: new Decimal((monthlyPayment * paymentInterval).toFixed(2)),
        balance: new Decimal(balance.toFixed(2)),
      });
    }
    
    return schedule;
  }

  /**
   * Calculate loan amortization schedule using flat rate method
   */
  calculateFlatRateSchedule(params: LoanCalculationParams): LoanScheduleItem[] {
    const { principal, interestRate, termMonths, startDate, paymentFrequency = 'monthly' } = params;
    
    const schedule: LoanScheduleItem[] = [];
    
    // Calculate total interest for entire loan period
    const totalInterest = principal * interestRate * (termMonths / 12);
    const totalAmount = principal + totalInterest;
    
    const paymentInterval = paymentFrequency === 'quarterly' ? 3 : 1;
    const numberOfPayments = Math.ceil(termMonths / paymentInterval);
    const paymentAmount = totalAmount / numberOfPayments;
    const principalPerPayment = principal / numberOfPayments;
    const interestPerPayment = totalInterest / numberOfPayments;
    
    let balance = principal;
    
    for (let i = 1; i <= numberOfPayments; i++) {
      const dueDate = new Date(startDate);
      dueDate.setMonth(dueDate.getMonth() + (i * paymentInterval));
      
      balance -= principalPerPayment;
      if (balance < 0) balance = 0;
      
      schedule.push({
        paymentNumber: i,
        dueDate,
        principal: new Decimal(principalPerPayment.toFixed(2)),
        interest: new Decimal(interestPerPayment.toFixed(2)),
        totalPayment: new Decimal(paymentAmount.toFixed(2)),
        balance: new Decimal(balance.toFixed(2)),
      });
    }
    
    return schedule;
  }

  /**
   * Calculate loan schedule based on method
   */
  calculateLoanSchedule(params: LoanCalculationParams): LoanScheduleItem[] {
    if (params.method === 'reducing_balance') {
      return this.calculateReducingBalanceSchedule(params);
    } else {
      return this.calculateFlatRateSchedule(params);
    }
  }

  /**
   * Calculate daily interest accrual
   */
  calculateInterestAccrual(
    principal: number,
    annualRate: number,
    days: number
  ): Decimal {
    const dailyRate = annualRate / 365;
    const interest = principal * dailyRate * days;
    return new Decimal(interest.toFixed(2));
  }

  /**
   * Calculate interest for a specific period
   */
  calculatePeriodInterest(
    principal: number,
    annualRate: number,
    startDate: Date,
    endDate: Date
  ): Decimal {
    const days = Math.floor(
      (endDate.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24)
    );
    return this.calculateInterestAccrual(principal, annualRate, days);
  }

  /**
   * Calculate early payment impact
   */
  calculateEarlyPayment(
    currentSchedule: LoanScheduleItem[],
    paymentAmount: number,
    paymentDate: Date,
    interestRate: number
  ): EarlyPaymentResult {
    // Find current position in schedule
    const currentItem = currentSchedule.find(
      (item) => item.dueDate >= paymentDate
    );
    
    if (!currentItem) {
      throw new Error('Payment date is after loan maturity');
    }
    
    const currentBalance = Number(currentItem.balance);
    const newBalance = Math.max(0, currentBalance - paymentAmount);
    
    // Calculate interest saved
    const remainingSchedule = currentSchedule.filter(
      (item) => item.dueDate > paymentDate
    );
    const originalInterest = remainingSchedule.reduce(
      (sum, item) => sum + Number(item.interest),
      0
    );
    
    // Recalculate schedule with new balance
    const remainingMonths = remainingSchedule.length;
    const revisedSchedule = this.calculateReducingBalanceSchedule({
      principal: newBalance,
      interestRate,
      termMonths: remainingMonths,
      startDate: paymentDate,
      method: 'reducing_balance',
    });
    
    const newInterest = revisedSchedule.reduce(
      (sum, item) => sum + Number(item.interest),
      0
    );
    
    const interestSaved = originalInterest - newInterest;
    const newMaturityDate = revisedSchedule[revisedSchedule.length - 1]?.dueDate || paymentDate;
    
    return {
      remainingBalance: new Decimal(newBalance.toFixed(2)),
      interestSaved: new Decimal(interestSaved.toFixed(2)),
      newMaturityDate,
      revisedSchedule,
    };
  }

  /**
   * Calculate penalty for late payment
   */
  calculatePenalty(
    dueAmount: number,
    dueDate: Date,
    currentDate: Date,
    penaltyRate: number // Daily penalty rate
  ): PenaltyCalculation {
    const daysOverdue = Math.max(
      0,
      Math.floor((currentDate.getTime() - dueDate.getTime()) / (1000 * 60 * 60 * 24))
    );
    
    if (daysOverdue === 0) {
      return {
        daysOverdue: 0,
        penaltyAmount: new Decimal(0),
        totalAmountDue: new Decimal(dueAmount.toFixed(2)),
      };
    }
    
    // Calculate penalty: principal * penalty_rate * days_overdue
    const penaltyAmount = dueAmount * penaltyRate * daysOverdue;
    const totalAmountDue = dueAmount + penaltyAmount;
    
    return {
      daysOverdue,
      penaltyAmount: new Decimal(penaltyAmount.toFixed(2)),
      totalAmountDue: new Decimal(totalAmountDue.toFixed(2)),
    };
  }

  /**
   * Calculate total loan cost
   */
  calculateTotalLoanCost(schedule: LoanScheduleItem[]): {
    totalPrincipal: Decimal;
    totalInterest: Decimal;
    totalAmount: Decimal;
  } {
    const totalPrincipal = schedule.reduce(
      (sum, item) => sum + Number(item.principal),
      0
    );
    const totalInterest = schedule.reduce(
      (sum, item) => sum + Number(item.interest),
      0
    );
    
    return {
      totalPrincipal: new Decimal(totalPrincipal.toFixed(2)),
      totalInterest: new Decimal(totalInterest.toFixed(2)),
      totalAmount: new Decimal((totalPrincipal + totalInterest).toFixed(2)),
    };
  }

  /**
   * Calculate effective interest rate (APR)
   */
  calculateEffectiveRate(
    principal: number,
    totalInterest: number,
    termMonths: number
  ): number {
    const totalAmount = principal + totalInterest;
    const monthlyPayment = totalAmount / termMonths;
    
    // Use Newton-Raphson method to find the rate
    let rate = 0.01; // Initial guess
    const tolerance = 0.0001;
    const maxIterations = 100;
    
    for (let i = 0; i < maxIterations; i++) {
      const pv = this.presentValue(monthlyPayment, rate, termMonths);
      const error = pv - principal;
      
      if (Math.abs(error) < tolerance) {
        break;
      }
      
      const pvDerivative = this.presentValueDerivative(monthlyPayment, rate, termMonths);
      rate = rate - error / pvDerivative;
    }
    
    return rate * 12; // Convert to annual rate
  }

  /**
   * Calculate present value
   */
  private presentValue(payment: number, rate: number, periods: number): number {
    if (rate === 0) return payment * periods;
    return payment * ((1 - Math.pow(1 + rate, -periods)) / rate);
  }

  /**
   * Calculate present value derivative
   */
  private presentValueDerivative(payment: number, rate: number, periods: number): number {
    if (rate === 0) return 0;
    
    const term1 = (1 - Math.pow(1 + rate, -periods)) / (rate * rate);
    const term2 = (periods * Math.pow(1 + rate, -periods - 1)) / rate;
    
    return payment * (term2 - term1);
  }
}

export default LoanCalculationService;
