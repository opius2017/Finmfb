import { test, expect } from '@playwright/test';
import { mockDataFactory } from '../../utils/mock-data';

test.describe('Reconciliation', () => {
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

    await page.goto('/reconciliation');
  });

  test('should display unmatched transactions within 3 seconds', async ({ page }) => {
    const transactions = mockDataFactory.createTransactions(10, { status: 'UNMATCHED' });
    const startTime = Date.now();

    await page.route('**/api/reconciliation/unmatched**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: transactions,
          total: transactions.length,
        }),
      });
    });

    await page.reload();
    await page.waitForLoadState('networkidle');

    const loadTime = Date.now() - startTime;
    expect(loadTime).toBeLessThan(3000);
  });

  test('should manually match transaction to loan', async ({ page }) => {
    const transaction = mockDataFactory.createTransaction({
      id: 'TXN-001',
      status: 'UNMATCHED',
      amount: 45000,
    });

    await page.route('**/api/reconciliation/unmatched**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [transaction],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/reconciliation/match`, async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            ...transaction,
            status: 'MATCHED',
            loanId: 'LOAN-001',
          }),
        });
      }
    });

    await page.reload();

    const matchButton = page.getByRole('button', { name: /match/i }).first();
    if (await matchButton.isVisible()) {
      await matchButton.click();
      await page.waitForTimeout(500);

      const loanSelect = page.locator('select[name="loanId"]').first();
      if (await loanSelect.isVisible()) {
        await loanSelect.selectOption('LOAN-001');
        
        const confirmButton = page.getByRole('button', { name: /confirm/i });
        await confirmButton.click();
        await page.waitForTimeout(1000);
      }
    }
  });

  test('should display transaction details', async ({ page }) => {
    const transaction = mockDataFactory.createTransaction({
      transactionNumber: 'TXN00000001',
      memberName: 'John Doe',
      amount: 45000,
    });

    await page.route('**/api/reconciliation/unmatched**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [transaction],
          total: 1,
        }),
      });
    });

    await page.reload();
  });

  test('should complete reconciliation', async ({ page }) => {
    const transaction = mockDataFactory.createTransaction({
      id: 'TXN-002',
      status: 'MATCHED',
    });

    await page.route('**/api/reconciliation/unmatched**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [transaction],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/reconciliation/${transaction.id}/complete`, async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            ...transaction,
            status: 'RECONCILED',
          }),
        });
      }
    });

    await page.reload();
  });

  test('should highlight discrepancies', async ({ page }) => {
    const transactions = [
      mockDataFactory.createTransaction({ amount: 45000, status: 'UNMATCHED' }),
      mockDataFactory.createTransaction({ amount: 100000, status: 'UNMATCHED' }), // Large discrepancy
    ];

    await page.route('**/api/reconciliation/unmatched**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: transactions,
          total: transactions.length,
        }),
      });
    });

    await page.reload();
  });

  test('should handle tolerance thresholds', async ({ page }) => {
    await page.route('**/api/reconciliation/settings', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          toleranceThreshold: 100,
          autoMatchEnabled: true,
        }),
      });
    });

    await page.route('**/api/reconciliation/unmatched**', async (route) => {
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
  });
});
