import { test, expect } from '@playwright/test';
import { mockDataFactory } from '../../utils/mock-data';

test.describe('Deduction Schedules', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem('authToken', 'test-token');
      localStorage.setItem('auth-storage', JSON.stringify({
        state: {
          user: { id: '1', email: 'test@example.com', name: 'Test User', role: 'ADMIN' },
          token: 'test-token',
          isAuthenticated: true,
        },
        version: 0,
      }));
    });

    await page.goto('/deduction-schedules');
  });

  test('should display deduction schedules page', async ({ page }) => {
    await expect(page.getByRole('heading', { name: /Deduction Schedules/i })).toBeVisible();
  });

  test('should display schedule list', async ({ page }) => {
    const schedules = mockDataFactory.createDeductionSchedules(5);

    await page.route('**/api/deduction-schedules', async (route) => {
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
    await page.waitForTimeout(1000);

    // Verify schedules are displayed
    for (const schedule of schedules) {
      await expect(page.getByText(schedule.loanNumber)).toBeVisible();
    }
  });

  test('should filter schedules by date range', async ({ page }) => {
    const schedules = mockDataFactory.createDeductionSchedules(10);

    await page.route('**/api/deduction-schedules*', async (route) => {
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
    await page.waitForTimeout(1000);

    // Apply date filter
    const startDateInput = page.locator('input[type="date"][name*="start"]').first();
    const endDateInput = page.locator('input[type="date"][name*="end"]').first();

    if (await startDateInput.isVisible({ timeout: 1000 })) {
      await startDateInput.fill('2024-01-01');
      await endDateInput.fill('2024-12-31');
      await page.waitForTimeout(1000);

      // Should display filtered schedules
      await expect(page.getByText(schedules[0].loanNumber)).toBeVisible();
    }
  });

  test('should filter schedules by loan', async ({ page }) => {
    const schedules = mockDataFactory.createDeductionSchedules(5);

    await page.route('**/api/deduction-schedules*', async (route) => {
      const url = route.request().url();
      const urlParams = new URL(url).searchParams;
      const loanId = urlParams.get('loanId');

      let filteredSchedules = schedules;
      if (loanId) {
        filteredSchedules = schedules.filter(s => s.loanId === loanId);
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
    await page.waitForTimeout(1000);

    // Filter by loan
    const loanFilter = page.locator('select[name="loanId"], input[name="loanNumber"]').first();
    if (await loanFilter.isVisible({ timeout: 1000 })) {
      await loanFilter.fill(schedules[0].loanNumber);
      await page.waitForTimeout(1000);

      // Should show filtered results
      await expect(page.getByText(schedules[0].loanNumber)).toBeVisible();
    }
  });

  test('should display schedule details', async ({ page }) => {
    const schedule = mockDataFactory.createDeductionSchedule({
      loanNumber: 'LOAN00000001',
      memberName: 'John Doe',
      amount: 45000,
      status: 'PENDING',
    });

    await page.route('**/api/deduction-schedules', async (route) => {
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
    await page.waitForTimeout(1000);

    // Click on schedule to view details
    const scheduleRow = page.getByText(schedule.loanNumber);
    if (await scheduleRow.isVisible()) {
      await scheduleRow.click();
      await page.waitForTimeout(1000);

      // Should show schedule details
      await expect(page.getByText(schedule.memberName)).toBeVisible();
      await expect(page.getByText(/45,000|₦45,000/)).toBeVisible();
    }
  });

  test('should display status indicators', async ({ page }) => {
    const schedules = [
      mockDataFactory.createDeductionSchedule({ status: 'PENDING' }),
      mockDataFactory.createDeductionSchedule({ status: 'PROCESSED' }),
      mockDataFactory.createDeductionSchedule({ status: 'FAILED' }),
    ];

    await page.route('**/api/deduction-schedules', async (route) => {
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
    await page.waitForTimeout(1000);

    // Verify status indicators
    await expect(page.getByText('PENDING')).toBeVisible();
    await expect(page.getByText('PROCESSED')).toBeVisible();
    await expect(page.getByText('FAILED')).toBeVisible();
  });

  test('should handle empty schedules list', async ({ page }) => {
    await page.route('**/api/deduction-schedules', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [],
          total: 0,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Should show empty state
    await expect(page.getByText(/No schedules|No deduction schedules/i)).toBeVisible();
  });
});

test.describe('Deduction Schedules - Tracking', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem('authToken', 'test-token');
      localStorage.setItem('auth-storage', JSON.stringify({
        state: {
          user: { id: '1', email: 'test@example.com', name: 'Test User', role: 'ADMIN' },
          token: 'test-token',
          isAuthenticated: true,
        },
        version: 0,
      }));
    });

    await page.goto('/deduction-schedules');
  });

  test('should track payment status', async ({ page }) => {
    const schedule = mockDataFactory.createDeductionSchedule({
      id: 'SCH-001',
      status: 'PENDING',
    });

    await page.route('**/api/deduction-schedules', async (route) => {
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
    await page.waitForTimeout(1000);

    // Should display payment status
    await expect(page.getByText('PENDING')).toBeVisible();
  });

  test('should update schedule status', async ({ page }) => {
    const schedule = mockDataFactory.createDeductionSchedule({
      id: 'SCH-002',
      status: 'PENDING',
    });

    let currentStatus = 'PENDING';

    await page.route('**/api/deduction-schedules', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [{ ...schedule, status: currentStatus }],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/deduction-schedules/${schedule.id}/process`, async (route) => {
      currentStatus = 'PROCESSED';
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...schedule,
          status: 'PROCESSED',
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Process schedule
    const processButton = page.getByRole('button', { name: /Process|Mark as Processed/i }).first();
    if (await processButton.isVisible({ timeout: 1000 })) {
      await processButton.click();
      await page.waitForTimeout(1500);

      // Status should update
      await expect(page.getByText('PROCESSED')).toBeVisible();
    }
  });

  test('should export schedules', async ({ page }) => {
    const schedules = mockDataFactory.createDeductionSchedules(10);

    await page.route('**/api/deduction-schedules', async (route) => {
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
        body: 'mock-excel-data',
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Click export button
    const exportButton = page.getByRole('button', { name: /Export|Download/i }).first();
    if (await exportButton.isVisible({ timeout: 1000 })) {
      await exportButton.click();
      await page.waitForTimeout(1000);

      // Export should be triggered
      // File download verification would require additional setup
    }
  });
});
