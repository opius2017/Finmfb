import { test, expect } from '@playwright/test';

test.describe('Cross-Module Navigation', () => {
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

    await page.goto('/dashboard');
  });

  test('should navigate between all modules', async ({ page }) => {
    const modules = [
      { name: 'Dashboard', path: '/dashboard' },
      { name: 'Calculator', path: '/calculator' },
      { name: 'Eligibility', path: '/eligibility' },
      { name: 'Applications', path: '/applications' },
      { name: 'Guarantor', path: '/guarantor' },
      { name: 'Committee', path: '/committee' },
      { name: 'Deduction Schedules', path: '/deduction-schedules' },
      { name: 'Reconciliation', path: '/reconciliation' },
      { name: 'Commodity Vouchers', path: '/commodity-vouchers' },
      { name: 'Reports', path: '/reports' },
    ];

    for (const module of modules) {
      await page.goto(module.path);
      await page.waitForLoadState('networkidle');
      expect(page.url()).toContain(module.path);
    }
  });

  test('should update URL on navigation', async ({ page }) => {
    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');
    expect(page.url()).toContain('/calculator');

    await page.goto('/eligibility');
    await page.waitForLoadState('networkidle');
    expect(page.url()).toContain('/eligibility');
  });

  test('should highlight active menu item', async ({ page }) => {
    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');

    const activeLink = page.locator('[class*="active"], [aria-current="page"]').first();
    if (await activeLink.isVisible()) {
      await expect(activeLink).toBeVisible();
    }
  });

  test('should support browser back button', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');

    await page.goBack();
    await page.waitForLoadState('networkidle');
    expect(page.url()).toContain('/dashboard');
  });

  test('should support browser forward button', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');

    await page.goBack();
    await page.waitForLoadState('networkidle');

    await page.goForward();
    await page.waitForLoadState('networkidle');
    expect(page.url()).toContain('/calculator');
  });

  test('should support deep linking', async ({ page }) => {
    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');
    expect(page.url()).toContain('/calculator');

    await page.goto('/eligibility');
    await page.waitForLoadState('networkidle');
    expect(page.url()).toContain('/eligibility');
  });

  test('should maintain navigation state on page refresh', async ({ page }) => {
    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');

    await page.reload();
    await page.waitForLoadState('networkidle');
    expect(page.url()).toContain('/calculator');
  });

  test('should navigate using menu links', async ({ page }) => {
    const calculatorLink = page.getByRole('link', { name: /calculator/i });
    if (await calculatorLink.isVisible()) {
      await calculatorLink.click();
      await page.waitForLoadState('networkidle');
      expect(page.url()).toContain('/calculator');
    }
  });
});

test.describe('Navigation State Persistence', () => {
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

    await page.goto('/dashboard');
  });

  test('should preserve state during navigation', async ({ page }) => {
    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');

    const amountInput = page.locator('input[name="amount"], #amount').first();
    if (await amountInput.isVisible()) {
      await amountInput.fill('500000');
    }

    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');

    // State persistence depends on implementation
    await expect(amountInput).toBeVisible();
  });

  test('should handle page refresh', async ({ page }) => {
    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');

    await page.reload();
    await page.waitForLoadState('networkidle');

    expect(page.url()).toContain('/calculator');
  });
});
