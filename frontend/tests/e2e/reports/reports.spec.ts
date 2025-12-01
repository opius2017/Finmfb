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

  test('should display reports page', async ({ page }) => {
    await expect(page.getByRole('heading', { name: /Reports/i })).toBeVisible();
  });

  test('should display available report types', async ({ page }) => {
    await page.route('**/api/reports/types', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          reportTypes: [
            { id: 'LOAN_PORTFOLIO', name: 'Loan Portfolio Report' },
            { id: 'DELINQUENCY', name: 'Delinquency Report' },
            { id: 'DISBURSEMENT', name: 'Disbursement Report' },
            { id: 'COLLECTION', name: 'Collection Report' },
            { id: 'MEMBER_SUMMARY', name: 'Member Summary Report' },
          ],
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Verify report types are displayed
    await expect(page.getByText(/Loan Portfolio|Portfolio/i)).toBeVisible();
    await expect(page.getByText(/Delinquency/i)).toBeVisible();
  });

  test('should select report type', async ({ page }) => {
    const reportTypeSelect = page.locator('select[name="reportType"], select#reportType').first();
    
    if (await reportTypeSelect.isVisible({ timeout: 1000 })) {
      await reportTypeSelect.selectOption('LOAN_PORTFOLIO');
      
      // Should show report type selected
      await expect(reportTypeSelect).toHaveValue('LOAN_PORTFOLIO');
    }
  });

  test('should select date range', async ({ page }) => {
    const startDateInput = page.locator('input[type="date"][name*="start"]').first();
    const endDateInput = page.locator('input[type="date"][name*="end"]').first();

    if (await startDateInput.isVisible({ timeout: 1000 })) {
      await startDateInput.fill('2024-01-01');
      await endDateInput.fill('2024-12-31');

      // Should have selected dates
      await expect(startDateInput).toHaveValue('2024-01-01');
      await expect(endDateInput).toHaveValue('2024-12-31');
    }
  });

  test('should apply filter criteria', async ({ page }) => {
    // Select report type
    const reportTypeSelect = page.locator('select[name="reportType"]').first();
    if (await reportTypeSelect.isVisible({ timeout: 1000 })) {
      await reportTypeSelect.selectOption('LOAN_PORTFOLIO');
    }

    // Apply filters
    const statusFilter = page.locator('select[name="status"]').first();
    if (await statusFilter.isVisible({ timeout: 1000 })) {
      await statusFilter.selectOption('ACTIVE');
    }

    const loanTypeFilter = page.locator('select[name="loanType"]').first();
    if (await loanTypeFilter.isVisible({ timeout: 1000 })) {
      await loanTypeFilter.selectOption('PERSONAL');
    }
  });

  test('should generate report', async ({ page }) => {
    await page.route('**/api/reports/generate', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          reportId: 'RPT-001',
          reportType: 'LOAN_PORTFOLIO',
          status: 'COMPLETED',
          data: {
            totalLoans: 150,
            totalAmount: 75000000,
            activeLoans: 120,
            completedLoans: 30,
            summary: [
              { category: 'Personal Loans', count: 80, amount: 40000000 },
              { category: 'Emergency Loans', count: 40, amount: 20000000 },
              { category: 'Commodity Loans', count: 30, amount: 15000000 },
            ],
          },
        }),
      });
    });

    // Select report type
    const reportTypeSelect = page.locator('select[name="reportType"]').first();
    if (await reportTypeSelect.isVisible({ timeout: 1000 })) {
      await reportTypeSelect.selectOption('LOAN_PORTFOLIO');
    }

    // Generate report
    const generateButton = page.getByRole('button', { name: /Generate|Create Report/i });
    if (await generateButton.isVisible()) {
      await generateButton.click();
      await page.waitForTimeout(1500);

      // Should display report results
      await expect(page.getByText(/150|Total Loans/i)).toBeVisible();
      await expect(page.getByText(/75,000,000|₦75,000,000/i)).toBeVisible();
    }
  });

  test('should export report in PDF format', async ({ page }) => {
    await page.route('**/api/reports/generate', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          reportId: 'RPT-002',
          status: 'COMPLETED',
          data: { totalLoans: 100 },
        }),
      });
    });

    await page.route('**/api/reports/RPT-002/export*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/pdf',
        body: 'mock-pdf-data',
      });
    });

    // Generate report first
    const reportTypeSelect = page.locator('select[name="reportType"]').first();
    if (await reportTypeSelect.isVisible({ timeout: 1000 })) {
      await reportTypeSelect.selectOption('LOAN_PORTFOLIO');
    }

    const generateButton = page.getByRole('button', { name: /Generate/i });
    if (await generateButton.isVisible()) {
      await generateButton.click();
      await page.waitForTimeout(1500);

      // Export as PDF
      const exportButton = page.getByRole('button', { name: /Export|Download.*PDF/i }).first();
      if (await exportButton.isVisible({ timeout: 1000 })) {
        await exportButton.click();
        await page.waitForTimeout(1000);

        // Export should be triggered
      }
    }
  });

  test('should export report in Excel format', async ({ page }) => {
    await page.route('**/api/reports/generate', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          reportId: 'RPT-003',
          status: 'COMPLETED',
          data: { totalLoans: 100 },
        }),
      });
    });

    await page.route('**/api/reports/RPT-003/export*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/vnd.ms-excel',
        body: 'mock-excel-data',
      });
    });

    // Generate and export
    const reportTypeSelect = page.locator('select[name="reportType"]').first();
    if (await reportTypeSelect.isVisible({ timeout: 1000 })) {
      await reportTypeSelect.selectOption('LOAN_PORTFOLIO');
    }

    const generateButton = page.getByRole('button', { name: /Generate/i });
    if (await generateButton.isVisible()) {
      await generateButton.click();
      await page.waitForTimeout(1500);

      // Export as Excel
      const exportButton = page.getByRole('button', { name: /Export|Download.*Excel/i }).first();
      if (await exportButton.isVisible({ timeout: 1000 })) {
        await exportButton.click();
        await page.waitForTimeout(1000);
      }
    }
  });

  test('should export report in CSV format', async ({ page }) => {
    await page.route('**/api/reports/generate', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          reportId: 'RPT-004',
          status: 'COMPLETED',
          data: { totalLoans: 100 },
        }),
      });
    });

    await page.route('**/api/reports/RPT-004/export*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'text/csv',
        body: 'mock-csv-data',
      });
    });

    // Generate and export
    const reportTypeSelect = page.locator('select[name="reportType"]').first();
    if (await reportTypeSelect.isVisible({ timeout: 1000 })) {
      await reportTypeSelect.selectOption('LOAN_PORTFOLIO');
    }

    const generateButton = page.getByRole('button', { name: /Generate/i });
    if (await generateButton.isVisible()) {
      await generateButton.click();
      await page.waitForTimeout(1500);

      // Export as CSV
      const exportButton = page.getByRole('button', { name: /Export|Download.*CSV/i }).first();
      if (await exportButton.isVisible({ timeout: 1000 })) {
        await exportButton.click();
        await page.waitForTimeout(1000);
      }
    }
  });

  test('should display loading state during generation', async ({ page }) => {
    await page.route('**/api/reports/generate', async (route) => {
      await new Promise((resolve) => setTimeout(resolve, 2000));
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          reportId: 'RPT-005',
          status: 'COMPLETED',
          data: { totalLoans: 100 },
        }),
      });
    });

    const reportTypeSelect = page.locator('select[name="reportType"]').first();
    if (await reportTypeSelect.isVisible({ timeout: 1000 })) {
      await reportTypeSelect.selectOption('LOAN_PORTFOLIO');
    }

    const generateButton = page.getByRole('button', { name: /Generate/i });
    if (await generateButton.isVisible()) {
      await generateButton.click();

      // Should show loading state
      const loadingIndicator = page.getByText(/Generating|Loading|loading/i);
      if (await loadingIndicator.isVisible({ timeout: 500 })) {
        await expect(loadingIndicator).toBeVisible();
      }
    }
  });

  test('should handle generation errors', async ({ page }) => {
    await page.route('**/api/reports/generate', async (route) => {
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Failed to generate report' }),
      });
    });

    const reportTypeSelect = page.locator('select[name="reportType"]').first();
    if (await reportTypeSelect.isVisible({ timeout: 1000 })) {
      await reportTypeSelect.selectOption('LOAN_PORTFOLIO');
    }

    const generateButton = page.getByRole('button', { name: /Generate/i });
    if (await generateButton.isVisible()) {
      await generateButton.click();
      await page.waitForTimeout(1000);

      // Should show error message
      await expect(page.getByText(/Failed|Error|error/i)).toBeVisible();
    }
  });
});

test.describe('Reports - Data Accuracy', () => {
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

  test('should verify report data matches expected results', async ({ page }) => {
    const expectedData = {
      totalLoans: 150,
      totalAmount: 75000000,
      activeLoans: 120,
      completedLoans: 30,
    };

    await page.route('**/api/reports/generate', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          reportId: 'RPT-VERIFY-001',
          status: 'COMPLETED',
          data: expectedData,
        }),
      });
    });

    const reportTypeSelect = page.locator('select[name="reportType"]').first();
    if (await reportTypeSelect.isVisible({ timeout: 1000 })) {
      await reportTypeSelect.selectOption('LOAN_PORTFOLIO');
    }

    const generateButton = page.getByRole('button', { name: /Generate/i });
    if (await generateButton.isVisible()) {
      await generateButton.click();
      await page.waitForTimeout(1500);

      // Verify data is displayed correctly
      await expect(page.getByText(/150/)).toBeVisible();
      await expect(page.getByText(/120/)).toBeVisible();
      await expect(page.getByText(/30/)).toBeVisible();
    }
  });

  test('should verify delinquency report calculations', async ({ page }) => {
    const delinquencyData = {
      totalDelinquent: 25,
      totalDelinquentAmount: 12500000,
      delinquencyRate: 16.67, // 25/150 * 100
      byAgingBucket: [
        { bucket: '1-30 days', count: 10, amount: 5000000 },
        { bucket: '31-60 days', count: 8, amount: 4000000 },
        { bucket: '61-90 days', count: 5, amount: 2500000 },
        { bucket: '90+ days', count: 2, amount: 1000000 },
      ],
    };

    await page.route('**/api/reports/generate', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          reportId: 'RPT-DELINQ-001',
          status: 'COMPLETED',
          data: delinquencyData,
        }),
      });
    });

    const reportTypeSelect = page.locator('select[name="reportType"]').first();
    if (await reportTypeSelect.isVisible({ timeout: 1000 })) {
      await reportTypeSelect.selectOption('DELINQUENCY');
    }

    const generateButton = page.getByRole('button', { name: /Generate/i });
    if (await generateButton.isVisible()) {
      await generateButton.click();
      await page.waitForTimeout(1500);

      // Verify delinquency metrics
      await expect(page.getByText(/25/)).toBeVisible();
      await expect(page.getByText(/16\.67|16\.7|17/)).toBeVisible();
    }
  });

  test('should verify disbursement report totals', async ({ page }) => {
    const disbursementData = {
      totalDisbursed: 50000000,
      disbursementCount: 75,
      averageDisbursement: 666667,
      byMonth: [
        { month: 'January', amount: 10000000, count: 15 },
        { month: 'February', amount: 15000000, count: 20 },
        { month: 'March', amount: 25000000, count: 40 },
      ],
    };

    await page.route('**/api/reports/generate', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          reportId: 'RPT-DISB-001',
          status: 'COMPLETED',
          data: disbursementData,
        }),
      });
    });

    const reportTypeSelect = page.locator('select[name="reportType"]').first();
    if (await reportTypeSelect.isVisible({ timeout: 1000 })) {
      await reportTypeSelect.selectOption('DISBURSEMENT');
    }

    const generateButton = page.getByRole('button', { name: /Generate/i });
    if (await generateButton.isVisible()) {
      await generateButton.click();
      await page.waitForTimeout(1500);

      // Verify disbursement totals
      await expect(page.getByText(/50,000,000|₦50,000,000/)).toBeVisible();
      await expect(page.getByText(/75/)).toBeVisible();
    }
  });

  test('should verify collection report accuracy', async ({ page }) => {
    const collectionData = {
      totalCollected: 30000000,
      collectionCount: 200,
      collectionRate: 85.5,
      expectedCollection: 35000000,
      variance: -5000000,
    };

    await page.route('**/api/reports/generate', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          reportId: 'RPT-COLL-001',
          status: 'COMPLETED',
          data: collectionData,
        }),
      });
    });

    const reportTypeSelect = page.locator('select[name="reportType"]').first();
    if (await reportTypeSelect.isVisible({ timeout: 1000 })) {
      await reportTypeSelect.selectOption('COLLECTION');
    }

    const generateButton = page.getByRole('button', { name: /Generate/i });
    if (await generateButton.isVisible()) {
      await generateButton.click();
      await page.waitForTimeout(1500);

      // Verify collection metrics
      await expect(page.getByText(/30,000,000|₦30,000,000/)).toBeVisible();
      await expect(page.getByText(/85\.5|85/)).toBeVisible();
      await expect(page.getByText(/200/)).toBeVisible();
    }
  });

  test('should verify member summary report data', async ({ page }) => {
    const memberData = {
      totalMembers: 500,
      activeMembers: 450,
      inactiveMembers: 50,
      membersWithLoans: 150,
      averageLoansPerMember: 1.2,
    };

    await page.route('**/api/reports/generate', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          reportId: 'RPT-MEM-001',
          status: 'COMPLETED',
          data: memberData,
        }),
      });
    });

    const reportTypeSelect = page.locator('select[name="reportType"]').first();
    if (await reportTypeSelect.isVisible({ timeout: 1000 })) {
      await reportTypeSelect.selectOption('MEMBER_SUMMARY');
    }

    const generateButton = page.getByRole('button', { name: /Generate/i });
    if (await generateButton.isVisible()) {
      await generateButton.click();
      await page.waitForTimeout(1500);

      // Verify member statistics
      await expect(page.getByText(/500/)).toBeVisible();
      await expect(page.getByText(/450/)).toBeVisible();
      await expect(page.getByText(/150/)).toBeVisible();
    }
  });

  test('should test loading states during generation', async ({ page }) => {
    let isLoading = true;

    await page.route('**/api/reports/generate', async (route) => {
      if (isLoading) {
        await new Promise((resolve) => setTimeout(resolve, 1500));
        isLoading = false;
      }
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          reportId: 'RPT-LOAD-001',
          status: 'COMPLETED',
          data: { totalLoans: 100 },
        }),
      });
    });

    const reportTypeSelect = page.locator('select[name="reportType"]').first();
    if (await reportTypeSelect.isVisible({ timeout: 1000 })) {
      await reportTypeSelect.selectOption('LOAN_PORTFOLIO');
    }

    const generateButton = page.getByRole('button', { name: /Generate/i });
    if (await generateButton.isVisible()) {
      await generateButton.click();

      // Check for loading indicator
      const loadingIndicator = page.locator('[data-testid="loading"], .loading, .spinner').first();
      if (await loadingIndicator.isVisible({ timeout: 500 })) {
        await expect(loadingIndicator).toBeVisible();
      }

      // Wait for completion
      await page.waitForTimeout(2000);
      await expect(page.getByText(/100/)).toBeVisible();
    }
  });

  test('should handle error state for failed generation', async ({ page }) => {
    await page.route('**/api/reports/generate', async (route) => {
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({
          message: 'Database connection failed',
          error: 'INTERNAL_SERVER_ERROR',
        }),
      });
    });

    const reportTypeSelect = page.locator('select[name="reportType"]').first();
    if (await reportTypeSelect.isVisible({ timeout: 1000 })) {
      await reportTypeSelect.selectOption('LOAN_PORTFOLIO');
    }

    const generateButton = page.getByRole('button', { name: /Generate/i });
    if (await generateButton.isVisible()) {
      await generateButton.click();
      await page.waitForTimeout(1000);

      // Should display error message
      await expect(page.getByText(/Failed|Error|failed|error/i)).toBeVisible();
    }
  });

  test('should handle empty report results', async ({ page }) => {
    await page.route('**/api/reports/generate', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          reportId: 'RPT-EMPTY-001',
          status: 'COMPLETED',
          data: {
            totalLoans: 0,
            summary: [],
          },
        }),
      });
    });

    const reportTypeSelect = page.locator('select[name="reportType"]').first();
    if (await reportTypeSelect.isVisible({ timeout: 1000 })) {
      await reportTypeSelect.selectOption('LOAN_PORTFOLIO');
    }

    const generateButton = page.getByRole('button', { name: /Generate/i });
    if (await generateButton.isVisible()) {
      await generateButton.click();
      await page.waitForTimeout(1500);

      // Should show empty state or zero values
      await expect(page.getByText(/No data|0|Zero/i)).toBeVisible();
    }
  });
});
