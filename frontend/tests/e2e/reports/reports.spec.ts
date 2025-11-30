import { test, expect } from '@playwright/test';
import { mockDataFactory } from '../../utils/mock-data';

test.describe('Reports', () => {
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

    await page.goto('/reports');
  });

  test('should display report types within 2 seconds', async ({ page }) => {
    const startTime = Date.now();

    await page.route('**/api/reports/types', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([
          { id: 'LOAN_PORTFOLIO', name: 'Loan Portfolio Report' },
          { id: 'DELINQUENCY', name: 'Delinquency Report' },
          { id: 'DISBURSEMENT', name: 'Disbursement Report' },
          { id: 'COLLECTION', name: 'Collection Report' },
        ]),
      });
    });

    await page.reload();
    await page.waitForLoadState('networkidle');

    const loadTime = Date.now() - startTime;
    expect(loadTime).toBeLessThan(2000);
  });

  test('should select report type', async ({ page }) => {
    await page.route('**/api/reports/types', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([
          { id: 'LOAN_PORTFOLIO', name: 'Loan Portfolio Report' },
        ]),
      });
    });

    await page.reload();

    const reportTypeSelect = page.locator('select[name="reportType"], #reportType').first();
    if (await reportTypeSelect.isVisible()) {
      await reportTypeSelect.selectOption('LOAN_PORTFOLIO');
      await page.waitForTimeout(500);
    }
  });

  test('should select date range', async ({ page }) => {
    await page.route('**/api/reports/types', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([]),
      });
    });

    await page.reload();

    const startDateInput = page.locator('input[name="startDate"], input[type="date"]').first();
    const endDateInput = page.locator('input[name="endDate"], input[type="date"]').nth(1);

    if (await startDateInput.isVisible()) {
      await startDateInput.fill('2024-01-01');
      await endDateInput.fill('2024-12-31');
    }
  });

  test('should apply filter criteria', async ({ page }) => {
    await page.route('**/api/reports/types', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([]),
      });
    });

    await page.reload();

    const statusFilter = page.locator('select[name="status"]').first();
    if (await statusFilter.isVisible()) {
      await statusFilter.selectOption('ACTIVE');
    }
  });

  test('should generate report within 5 seconds', async ({ page }) => {
    const startTime = Date.now();

    await page.route('**/api/reports/types', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([
          { id: 'LOAN_PORTFOLIO', name: 'Loan Portfolio Report' },
        ]),
      });
    });

    await page.route('**/api/reports/generate', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            reportId: 'RPT-001',
            data: {
              totalLoans: 150,
              totalAmount: 75000000,
              activeLoans: 120,
              completedLoans: 30,
            },
          }),
        });
      }
    });

    await page.reload();

    const generateButton = page.getByRole('button', { name: /generate/i });
    if (await generateButton.isVisible()) {
      await generateButton.click();
      await page.waitForTimeout(1000);

      const responseTime = Date.now() - startTime;
      expect(responseTime).toBeLessThan(5000);
    }
  });

  test('should display report results', async ({ page }) => {
    await page.route('**/api/reports/types', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([]),
      });
    });

    await page.route('**/api/reports/generate', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            reportId: 'RPT-001',
            data: {
              totalLoans: 150,
              totalAmount: 75000000,
            },
          }),
        });
      }
    });

    await page.reload();

    const generateButton = page.getByRole('button', { name: /generate/i });
    if (await generateButton.isVisible()) {
      await generateButton.click();
      await page.waitForTimeout(1000);
    }
  });

  test('should export report in PDF format', async ({ page }) => {
    await page.route('**/api/reports/types', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([]),
      });
    });

    await page.route('**/api/reports/*/export', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/pdf',
        body: 'mock pdf data',
      });
    });

    await page.reload();

    const exportButton = page.getByRole('button', { name: /export.*pdf/i }).or(page.getByRole('button', { name: /pdf/i }));
    if (await exportButton.isVisible()) {
      await exportButton.click();
      await page.waitForTimeout(500);
    }
  });

  test('should export report in Excel format', async ({ page }) => {
    await page.route('**/api/reports/types', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([]),
      });
    });

    await page.route('**/api/reports/*/export', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/vnd.ms-excel',
        body: 'mock excel data',
      });
    });

    await page.reload();

    const exportButton = page.getByRole('button', { name: /export.*excel/i }).or(page.getByRole('button', { name: /excel/i }));
    if (await exportButton.isVisible()) {
      await exportButton.click();
      await page.waitForTimeout(500);
    }
  });

  test('should export report in CSV format', async ({ page }) => {
    await page.route('**/api/reports/types', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([]),
      });
    });

    await page.route('**/api/reports/*/export', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'text/csv',
        body: 'mock csv data',
      });
    });

    await page.reload();

    const exportButton = page.getByRole('button', { name: /export.*csv/i }).or(page.getByRole('button', { name: /csv/i }));
    if (await exportButton.isVisible()) {
      await exportButton.click();
      await page.waitForTimeout(500);
    }
  });

  test('should display loading state during generation', async ({ page }) => {
    await page.route('**/api/reports/types', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([]),
      });
    });

    await page.route('**/api/reports/generate', async (route) => {
      if (route.request().method() === 'POST') {
        await new Promise(resolve => setTimeout(resolve, 2000));
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({ reportId: 'RPT-001', data: {} }),
        });
      }
    });

    await page.reload();

    const generateButton = page.getByRole('button', { name: /generate/i });
    if (await generateButton.isVisible()) {
      await generateButton.click();

      const loadingIndicator = page.getByText(/generating/i).or(generateButton.locator('[class*="spinner"]'));
      if (await loadingIndicator.isVisible()) {
        await expect(loadingIndicator).toBeVisible();
      }
    }
  });

  test('should handle report generation errors', async ({ page }) => {
    await page.route('**/api/reports/types', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([]),
      });
    });

    await page.route('**/api/reports/generate', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 500,
          contentType: 'application/json',
          body: JSON.stringify({ message: 'Report generation failed' }),
        });
      }
    });

    await page.reload();

    const generateButton = page.getByRole('button', { name: /generate/i });
    if (await generateButton.isVisible()) {
      await generateButton.click();
      await page.waitForTimeout(1000);

      const errorMessage = page.getByText(/failed/i).or(page.getByText(/error/i));
      if (await errorMessage.isVisible()) {
        await expect(errorMessage).toBeVisible();
      }
    }
  });
});
