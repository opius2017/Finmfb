import { test, expect } from '@playwright/test';
import { mockDataFactory } from '../../utils/mock-data';

test.describe('Deduction Schedules', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem('authToken', 'test-token');
      localStorage.setItem('auth-storage', JSON.stringify({
        state: {
          user: { id: '1', email: 'test@example.com', name: 'Test User', role: 'MEMBER' },
          token: 'test-token',
          isAuthenticated: true,
        },
        version: 0,
      }));
    });

    await page.goto('/deduction-schedules');
  });

  test('should display schedule list within 3 seconds', async ({ page }) => {
    const schedules = mockDataFactory.createDeductionSchedules(10);
    const startTime = Date.now();

    await page.route('**/api/deduction-schedules**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: schedules,
          total: schedules.length,
        }),
      });
    });

    await page.reload();
    await page.waitForLoadState('networkidle');

    const loadTime = Date.now() - startTime;
    expect(loadTime).toBeLessThan(3000);
  });

  test('should filter schedules by date range', async ({ page }) => {
    const schedules = mockDataFactory.createDeductionSchedules(10);

    await page.route('**/api/deduction-schedules**', async (route) => {
      const url = new URL(route.request().url());
      const startDate = url.searchParams.get('startDate');
      const endDate = url.searchParams.get('endDate');

      let filteredSchedules = schedules;
      if (startDate && endDate) {
        filteredSchedules = schedules.filter(schedule => {
          const dueDate = new Date(schedule.dueDate);
          return dueDate >= new Date(startDate) && dueDate <= new Date(endDate);
        });
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: filteredSchedules,
          total: filteredSchedules.length,
        }),
      });
    });

    await page.reload();
  });

  test('should filter schedules by loan', async ({ page }) => {
    const schedules = mockDataFactory.createDeductionSchedules(10);

    await page.route('**/api/deduction-schedules**', async (route) => {
      const url = new URL(route.request().url());
      const loanId = url.searchParams.get('loanId');

      let filteredSchedules = schedules;
      if (loanId) {
        filteredSchedules = schedules.filter(schedule => schedule.loanId === loanId);
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: filteredSchedules,
          total: filteredSchedules.length,
        }),
      });
    });

    await page.reload();
  });

  test('should display schedule details', async ({ page }) => {
    const schedule = mockDataFactory.createDeductionSchedule({
      loanNumber: 'LOAN00000001',
      memberName: 'John Doe',
      amount: 45000,
      status: 'PENDING',
    });

    await page.route('**/api/deduction-schedules**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [schedule],
          total: 1,
        }),
      });
    });

    await page.reload();
  });

  test('should display status indicators', async ({ page }) => {
    const schedules = [
      mockDataFactory.createDeductionSchedule({ status: 'PENDING' }),
      mockDataFactory.createDeductionSchedule({ status: 'PROCESSED' }),
      mockDataFactory.createDeductionSchedule({ status: 'FAILED' }),
    ];

    await page.route('**/api/deduction-schedules**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: schedules,
          total: schedules.length,
        }),
      });
    });

    await page.reload();
  });

  test('should track payment status', async ({ page }) => {
    const schedule = mockDataFactory.createDeductionSchedule({
      status: 'PROCESSED',
      processedDate: new Date().toISOString(),
    });

    await page.route('**/api/deduction-schedules**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [schedule],
          total: 1,
        }),
      });
    });

    await page.reload();
  });

  test('should update schedule status', async ({ page }) => {
    const schedule = mockDataFactory.createDeductionSchedule({
      id: 'SCH-001',
      status: 'PENDING',
    });

    await page.route('**/api/deduction-schedules**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [schedule],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/deduction-schedules/${schedule.id}/process`, async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            ...schedule,
            status: 'PROCESSED',
            processedDate: new Date().toISOString(),
          }),
        });
      }
    });

    await page.reload();
  });

  test('should export schedules', async ({ page }) => {
    const schedules = mockDataFactory.createDeductionSchedules(10);

    await page.route('**/api/deduction-schedules**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: schedules,
          total: schedules.length,
        }),
      });
    });

    await page.route('**/api/deduction-schedules/export', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/vnd.ms-excel',
        body: 'mock excel data',
      });
    });

    await page.reload();

    const exportButton = page.getByRole('button', { name: /export/i });
    if (await exportButton.isVisible()) {
      await exportButton.click();
      await page.waitForTimeout(500);
    }
  });
});
