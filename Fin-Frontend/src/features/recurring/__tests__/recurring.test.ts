import { describe, it, expect, beforeEach, vi } from 'vitest';
import { recurringService } from '../services/recurringService';
import { RecurringTemplate, FrequencyConfig } from '../types/recurring.types';

// Mock fetch globally
global.fetch = vi.fn();

describe('Recurring Transactions Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Schedule Calculations', () => {
    it('should calculate next run date for daily frequency', () => {
      const startDate = new Date('2024-01-01');
      const frequency: FrequencyConfig = {
        type: 'daily',
        interval: 1,
      };

      const nextDate = recurringService.calculateNextRunDate(startDate, frequency);
      
      expect(nextDate.getDate()).toBe(2);
      expect(nextDate.getMonth()).toBe(0); // January
    });

    it('should calculate next run date for weekly frequency', () => {
      const startDate = new Date('2024-01-01');
      const frequency: FrequencyConfig = {
        type: 'weekly',
        interval: 2,
      };

      const nextDate = recurringService.calculateNextRunDate(startDate, frequency);
      
      expect(nextDate.getDate()).toBe(15);
    });

    it('should calculate next run date for monthly frequency', () => {
      const startDate = new Date('2024-01-15');
      const frequency: FrequencyConfig = {
        type: 'monthly',
        interval: 1,
        dayOfMonth: 15,
      };

      const nextDate = recurringService.calculateNextRunDate(startDate, frequency);
      
      expect(nextDate.getMonth()).toBe(1); // February
      expect(nextDate.getDate()).toBe(15);
    });

    it('should calculate next run date for quarterly frequency', () => {
      const startDate = new Date('2024-01-01');
      const frequency: FrequencyConfig = {
        type: 'quarterly',
        interval: 1,
      };

      const nextDate = recurringService.calculateNextRunDate(startDate, frequency);
      
      expect(nextDate.getMonth()).toBe(3); // April
    });

    it('should calculate next run date for yearly frequency', () => {
      const startDate = new Date('2024-01-01');
      const frequency: FrequencyConfig = {
        type: 'yearly',
        interval: 1,
        monthOfYear: 6,
      };

      const nextDate = recurringService.calculateNextRunDate(startDate, frequency);
      
      expect(nextDate.getFullYear()).toBe(2025);
      expect(nextDate.getMonth()).toBe(5); // June (0-indexed)
    });

    it('should generate schedule for multiple months', () => {
      const template: RecurringTemplate = {
        id: '1',
        name: 'Test',
        description: 'Test template',
        transactionType: 'journal',
        frequency: { type: 'monthly', interval: 1 },
        amount: { type: 'fixed', fixedAmount: 1000 },
        accounts: { debitAccount: 'acc1', creditAccount: 'acc2' },
        status: 'active',
        startDate: new Date('2024-01-01'),
        nextRunDate: new Date('2024-01-01'),
        createdBy: 'user1',
        createdAt: new Date(),
        updatedAt: new Date(),
      };

      const schedule = recurringService.generateSchedule(template, 3);
      
      expect(schedule.length).toBeGreaterThan(0);
      expect(schedule.length).toBeLessThanOrEqual(3);
    });
  });

  describe('Amount Formulas', () => {
    it('should validate correct formula', () => {
      const formula = 'baseAmount * (1 + rate / 100)';
      const result = recurringService.validateFormula(formula);
      
      expect(result.valid).toBe(true);
    });

    it('should reject formula with forbidden keywords', () => {
      const formula = 'eval("malicious code")';
      const result = recurringService.validateFormula(formula);
      
      expect(result.valid).toBe(false);
      expect(result.error).toContain('Forbidden keyword');
    });

    it('should detect unbalanced parentheses', () => {
      const formula = '(baseAmount * rate';
      const result = recurringService.validateFormula(formula);
      
      expect(result.valid).toBe(false);
      expect(result.error).toContain('parentheses');
    });

    it('should test formula with variables', async () => {
      const mockResult = { value: 1050 };
      
      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockResult,
      });

      const result = await recurringService.testFormula(
        'baseAmount * (1 + rate / 100)',
        { baseAmount: 1000, rate: 5 }
      );

      expect(result).toBe(1050);
    });
  });

  describe('Template Management', () => {
    it('should create recurring template', async () => {
      const mockTemplate: RecurringTemplate = {
        id: '1',
        name: 'Monthly Rent',
        description: 'Office rent payment',
        transactionType: 'payment',
        frequency: { type: 'monthly', interval: 1 },
        amount: { type: 'fixed', fixedAmount: 50000 },
        accounts: { debitAccount: 'rent', creditAccount: 'bank' },
        status: 'active',
        startDate: new Date('2024-01-01'),
        nextRunDate: new Date('2024-02-01'),
        createdBy: 'user1',
        createdAt: new Date(),
        updatedAt: new Date(),
      };

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockTemplate,
      });

      const result = await recurringService.createTemplate({
        name: 'Monthly Rent',
        transactionType: 'payment',
      });

      expect(result.id).toBe('1');
      expect(result.name).toBe('Monthly Rent');
    });

    it('should activate template', async () => {
      const mockTemplate: RecurringTemplate = {
        id: '1',
        name: 'Test',
        description: 'Test',
        transactionType: 'journal',
        frequency: { type: 'monthly', interval: 1 },
        amount: { type: 'fixed', fixedAmount: 1000 },
        accounts: { debitAccount: 'acc1', creditAccount: 'acc2' },
        status: 'active',
        startDate: new Date(),
        nextRunDate: new Date(),
        createdBy: 'user1',
        createdAt: new Date(),
        updatedAt: new Date(),
      };

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockTemplate,
      });

      const result = await recurringService.activateTemplate('1');

      expect(result.status).toBe('active');
    });

    it('should pause template', async () => {
      const mockTemplate: RecurringTemplate = {
        id: '1',
        name: 'Test',
        description: 'Test',
        transactionType: 'journal',
        frequency: { type: 'monthly', interval: 1 },
        amount: { type: 'fixed', fixedAmount: 1000 },
        accounts: { debitAccount: 'acc1', creditAccount: 'acc2' },
        status: 'paused',
        startDate: new Date(),
        nextRunDate: new Date(),
        createdBy: 'user1',
        createdAt: new Date(),
        updatedAt: new Date(),
      };

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockTemplate,
      });

      const result = await recurringService.pauseTemplate('1');

      expect(result.status).toBe('paused');
    });
  });

  describe('Execution Logic', () => {
    it('should execute template immediately', async () => {
      const mockExecution = {
        id: 'exec-1',
        templateId: '1',
        templateName: 'Test Template',
        scheduledDate: new Date(),
        status: 'pending',
        amount: 1000,
      };

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockExecution,
      });

      const result = await recurringService.executeNow('1');

      expect(result.templateId).toBe('1');
      expect(result.status).toBe('pending');
    });

    it('should approve execution', async () => {
      const mockExecution = {
        id: 'exec-1',
        templateId: '1',
        templateName: 'Test',
        scheduledDate: new Date(),
        status: 'approved',
        amount: 1000,
        approvedBy: 'user1',
        approvedAt: new Date(),
      };

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockExecution,
      });

      const result = await recurringService.approveExecution('exec-1', 'Approved');

      expect(result.status).toBe('approved');
      expect(result.approvedBy).toBe('user1');
    });

    it('should reject execution', async () => {
      const mockExecution = {
        id: 'exec-1',
        templateId: '1',
        templateName: 'Test',
        scheduledDate: new Date(),
        status: 'cancelled',
        amount: 1000,
      };

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockExecution,
      });

      const result = await recurringService.rejectExecution('exec-1', 'Invalid amount');

      expect(result.status).toBe('cancelled');
    });
  });

  describe('Utility Functions', () => {
    it('should format frequency correctly', () => {
      expect(recurringService.formatFrequency({ type: 'daily', interval: 1 })).toBe('Daily');
      expect(recurringService.formatFrequency({ type: 'weekly', interval: 2 })).toBe('Every 2 weekly');
      expect(recurringService.formatFrequency({ type: 'monthly', interval: 1 })).toBe('Monthly');
    });

    it('should estimate monthly amount for fixed templates', () => {
      const template: RecurringTemplate = {
        id: '1',
        name: 'Test',
        description: 'Test',
        transactionType: 'journal',
        frequency: { type: 'weekly', interval: 1 },
        amount: { type: 'fixed', fixedAmount: 1000 },
        accounts: { debitAccount: 'acc1', creditAccount: 'acc2' },
        status: 'active',
        startDate: new Date('2024-01-01'),
        nextRunDate: new Date('2024-01-01'),
        createdBy: 'user1',
        createdAt: new Date(),
        updatedAt: new Date(),
      };

      const estimate = recurringService.estimateMonthlyAmount(template);
      
      expect(estimate).toBeGreaterThan(0);
      // Weekly template should execute ~4 times per month
      expect(estimate).toBeCloseTo(4000, -2);
    });
  });

  describe('Dashboard Data', () => {
    it('should fetch dashboard summary', async () => {
      const mockDashboard = {
        totalTemplates: 10,
        activeTemplates: 7,
        pausedTemplates: 2,
        upcomingExecutions: 15,
        failedExecutions: 1,
        totalMonthlyAmount: 50000,
        recentExecutions: [],
        upcomingSchedule: [],
      };

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockDashboard,
      });

      const result = await recurringService.getDashboard();

      expect(result.totalTemplates).toBe(10);
      expect(result.activeTemplates).toBe(7);
      expect(result.totalMonthlyAmount).toBe(50000);
    });
  });

  describe('Approval Workflow', () => {
    it('should handle maker-checker approval', async () => {
      const mockExecution = {
        id: 'exec-1',
        templateId: '1',
        templateName: 'Test',
        scheduledDate: new Date(),
        status: 'pending',
        amount: 10000,
      };

      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => mockExecution,
      });

      // First, execute the template
      const execution = await recurringService.executeNow('1');
      expect(execution.status).toBe('pending');

      // Then approve it
      (global.fetch as any).mockResolvedValueOnce({
        ok: true,
        json: async () => ({ ...mockExecution, status: 'approved' }),
      });

      const approved = await recurringService.approveExecution(execution.id);
      expect(approved.status).toBe('approved');
    });
  });
});
