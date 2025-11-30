import { Decimal } from '@prisma/client/runtime/library';
import { logger } from '@utils/logger';

export interface LoanScheduleItem {
  paymentNumber: number;
  dueDate: Date;
  principal: number;
  interest: number;
  totalPayment: number;
  balance: number;
}

export interface LoanCalculationParams {
  principal: number;
  interestRate: number; // Annual rate as decimal (e.g., 0.15 for 15%)
  termMonths: number;
  method: 'reducing_balance' | 'flat_rate';
  startDate: Date;
  paymentFrequency?: 'monthly' | 'quarterly';
}

export class CalculationEngine {
  /**
   * Calculate loan amortization schedule
   */
  calculateLoanSchedule(params: LoanCalculationParams): LoanScheduleItem[] {
    const { method } = params;

    if (method === 'reducing_balance') {
      return this.calculateReducingBalance(params);
    } else if (method === 'flat_rate') {
      return this.calculateFlatRate(params);
    } else {
      throw new Error(`Unsupported loan method: ${method}`);
    }
  }

  /**
   * Calculate reducing balance amortization
   */
  private calculateReducingBalance(params: LoanCalculationParams): LoanScheduleItem[] {
    const { principal, interestRate, termMonths, startDate, paymentFrequency = 'monthly' } = params;
    
    const schedule: LoanScheduleItem[] = [];
    const monthlyRate = interestRate / 12;
    
    // Calculate monthly payment using amortization formula
    const monthlyPayment = principal * 
      (monthlyRate * Math.pow(1 + monthlyRate, termMonths)) / 
      (Math.pow(1 + monthlyRate, termMonths) - 1);
    
    let balance = principal;
    let currentDate = new Date(startDate);
    
    for (let i = 1; i <= termMonths; i++) {
      // Calculate interest on remaining balance
      const interest = balance * monthlyRate;
      
      // Calculate principal payment
      const principalPayment = monthlyPayment - interest;
      
      // Update balance
      balance = Math.max(0, balance - principalPayment);
      
      // Add to schedule
      schedule.push({
        paymentNumber: i,
        dueDate: new Date(currentDate),
        principal: this.round(principalPayment),
        interest: this.round(interest),
        totalPayment: this.round(monthlyPayment),
        balance: this.round(balance),
      });
      
      // Move to next payment date
      currentDate = this.addMonths(currentDate, paymentFrequency === 'quarterly' ? 3 : 1);
    }
    
    return schedule;
  }

  /**
   * Calculate flat rate amortization
   */
  private calculateFlatRate(params: LoanCalculationParams): LoanScheduleItem[] {
    const { principal, interestRate, termMonths, startDate, paymentFrequency = 'monthly' } = params;
    
    const schedule: LoanScheduleItem[] = [];
    
    // Calculate total interest (flat rate on original principal)
    const totalInterest = principal * interestRate * (termMonths / 12);
    
    // Calculate monthly interest and principal
    const monthlyInterest = totalInterest / termMonths;
    const monthlyPrincipal = principal / termMonths;
    const monthlyPayment = monthlyPrincipal + monthlyInterest;
    
    let balance = principal;
    let currentDate = new Date(startDate);
    
    for (let i = 1; i <= termMonths; i++) {
      // Update balance
      balance = Math.max(0, balance - monthlyPrincipal);
      
      // Add to schedule
      schedule.push({
        paymentNumber: i,
        dueDate: new Date(currentDate),
        principal: this.round(monthlyPrincipal),
        interest: this.round(monthlyInterest),
        totalPayment: this.round(monthlyPayment),
        balance: this.round(balance),
      });
      
      // Move to next payment date
      currentDate = this.addMonths(currentDate, paymentFrequency === 'quarterly' ? 3 : 1);
    }
    
    return schedule;
  }

  /**
   * Calculate interest accrual for an account
   */
  calculateInterestAccrual(
    balance: number,
    annualRate: number,
    days: number = 1,
    daysInYear: number = 365
  ): number {
    const dailyRate = annualRate / daysInYear;
    const interest = balance * dailyRate * days;
    return this.round(interest);
  }

  /**
   * Calculate early payment amount
   */
  calculateEarlyPayment(
    principal: number,
    interestRate: number,
    remainingMonths: number,
    method: 'reducing_balance' | 'flat_rate'
  ): number {
    if (method === 'reducing_balance') {
      // For reducing balance, early payment is just the remaining principal
      return this.round(principal);
    } else {
      // For flat rate, include remaining interest
      const remainingInterest = principal * interestRate * (remainingMonths / 12);
      return this.round(principal + remainingInterest);
    }
  }

  /**
   * Calculate penalty for late payment
   */
  calculateLatePenalty(
    amount: number,
    daysLate: number,
    penaltyRate: number = 0.01 // 1% per day default
  ): number {
    const penalty = amount * penaltyRate * daysLate;
    return this.round(penalty);
  }

  /**
   * Calculate effective interest rate (APR)
   */
  calculateEffectiveRate(
    principal: number,
    totalPayments: number,
    termMonths: number
  ): number {
    const totalInterest = totalPayments - principal;
    const averageBalance = principal / 2;
    const effectiveRate = (totalInterest / averageBalance) / (termMonths / 12);
    return this.round(effectiveRate, 4);
  }

  /**
   * Helper: Round to specified decimal places
   */
  private round(value: number, decimals: number = 2): number {
    return Math.round(value * Math.pow(10, decimals)) / Math.pow(10, decimals);
  }

  /**
   * Helper: Add months to date
   */
  private addMonths(date: Date, months: number): Date {
    const result = new Date(date);
    result.setMonth(result.getMonth() + months);
    return result;
  }

  /**
   * Calculate total loan cost
   */
  calculateTotalLoanCost(schedule: LoanScheduleItem[]): {
    totalPrincipal: number;
    totalInterest: number;
    totalPayment: number;
  } {
    const totalPrincipal = schedule.reduce((sum, item) => sum + item.principal, 0);
    const totalInterest = schedule.reduce((sum, item) => sum + item.interest, 0);
    const totalPayment = schedule.reduce((sum, item) => sum + item.totalPayment, 0);

    return {
      totalPrincipal: this.round(totalPrincipal),
      totalInterest: this.round(totalInterest),
      totalPayment: this.round(totalPayment),
    };
  }

  /**
   * Calculate remaining balance at specific payment
   */
  calculateRemainingBalance(
    schedule: LoanScheduleItem[],
    paymentNumber: number
  ): number {
    const payment = schedule.find(item => item.paymentNumber === paymentNumber);
    return payment ? payment.balance : 0;
  }
}

export default CalculationEngine;
