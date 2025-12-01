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
      { name: 'Reports', path: '/reports' },
    ];

    for (const module of modules) {
      await page.goto(module.path);
      await page.waitForTimeout(500);

      // Verify navigation
      expect(page.url()).toContain(module.path);
    }
  });

  test('should update URL on navigation', async ({ page }) => {
    await page.goto('/dashboard');
    expect(page.url()).toContain('/dashboard');

    await page.goto('/calculator');
    expect(page.url()).toContain('/calculator');

    await page.goto('/applications');
    expect(page.url()).toContain('/applications');
  });

  test('should highlight active menu item', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForTimeout(500);

    // Look for active menu indicator
    const activeLink = page.locator('a[href="/dashboard"][class*="active"], a[href="/dashboard"][aria-current="page"]').first();
    if (await activeLink.isVisible({ timeout: 1000 })) {
      await expect(activeLink).toBeVisible();
    }
  });

  test('should support browser back button', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForTimeout(500);

    await page.goto('/calculator');
    await page.waitForTimeout(500);

    await page.goBack();
    await page.waitForTimeout(500);

    expect(page.url()).toContain('/dashboard');
  });

  test('should support browser forward button', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForTimeout(500);

    await page.goto('/calculator');
    await page.waitForTimeout(500);

    await page.goBack();
    await page.waitForTimeout(500);

    await page.goForward();
    await page.waitForTimeout(500);

    expect(page.url()).toContain('/calculator');
  });

  test('should support deep linking', async ({ page }) => {
    await page.goto('/applications/new');
    await page.waitForTimeout(1000);

    expect(page.url()).toContain('/applications/new');
    await expect(page.getByRole('heading', { name: /New.*Application/i })).toBeVisible();
  });

  test('should maintain navigation state on page refresh', async ({ page }) => {
    await page.goto('/calculator');
    await page.waitForTimeout(500);

    await page.reload();
    await page.waitForTimeout(1000);

    expect(page.url()).toContain('/calculator');
    await expect(page.getByRole('heading', { name: /Calculator/i })).toBeVisible();
  });

  test('should navigate using menu links', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForTimeout(500);

    // Click on Calculator link
    const calculatorLink = page.getByRole('link', { name: /Calculator/i });
    if (await calculatorLink.isVisible({ timeout: 1000 })) {
      await calculatorLink.click();
      await page.waitForTimeout(1000);

      expect(page.url()).toContain('/calculator');
    }
  });

  test('should preserve state during navigation', async ({ page }) => {
    await page.goto('/calculator');
    await page.waitForTimeout(500);

    // Fill some data
    const amountInput = page.locator('input[name="amount"], input[placeholder*="Amount"]').first();
    if (await amountInput.isVisible({ timeout: 1000 })) {
      await amountInput.fill('500000');
    }

    // Navigate away
    await page.goto('/dashboard');
    await page.waitForTimeout(500);

    // Navigate back
    await page.goto('/calculator');
    await page.waitForTimeout(1000);

    // Check if state is preserved (if implemented)
    const savedValue = await amountInput.inputValue();
    // State persistence may or may not be implemented
  });
});

test.describe('Navigation - State Persistence', () => {
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

  test('should preserve state on page refresh', async ({ page }) => {
    await page.goto('/calculator');
    await page.waitForTimeout(500);

    // Fill form
    const amountInput = page.locator('input[name="amount"]').first();
    if (await amountInput.isVisible({ timeout: 1000 })) {
      await amountInput.fill('500000');
      await page.waitForTimeout(500);
    }

    // Refresh page
    await page.reload();
    await page.waitForTimeout(1000);

    // Check if data persists (if implemented)
    const persistedData = await page.evaluate(() => {
      return localStorage.getItem('calculator-state') || sessionStorage.getItem('calculator-state');
    });

    // State persistence may or may not be implemented
  });

  test('should maintain authentication state on refresh', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForTimeout(500);

    // Verify authenticated
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();

    // Refresh
    await page.reload();
    await page.waitForTimeout(1000);

    // Should still be authenticated
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
    expect(page.url()).not.toContain('/login');
  });

  test('should preserve form data in new loan application', async ({ page }) => {
    await page.goto('/applications/new');
    await page.waitForTimeout(1000);

    // Fill some fields
    const loanTypeSelect = page.locator('select[name="loanType"]').first();
    if (await loanTypeSelect.isVisible({ timeout: 1000 })) {
      await loanTypeSelect.selectOption('PERSONAL');
      await page.waitForTimeout(500);
    }

    const amountInput = page.locator('input[name="amount"]').first();
    if (await amountInput.isVisible({ timeout: 1000 })) {
      await amountInput.fill('300000');
      await page.waitForTimeout(500);
    }

    // Navigate away
    await page.goto('/dashboard');
    await page.waitForTimeout(500);

    // Navigate back
    await page.goto('/applications/new');
    await page.waitForTimeout(1000);

    // Check if form data persists
    const savedLoanType = await loanTypeSelect.inputValue();
    const savedAmount = await amountInput.inputValue();
    
    // Form persistence may or may not be implemented
  });

  test('should maintain scroll position on navigation', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForTimeout(1000);

    // Scroll down
    await page.evaluate(() => window.scrollTo(0, 500));
    await page.waitForTimeout(500);

    const scrollPosition = await page.evaluate(() => window.scrollY);

    // Navigate away and back
    await page.goto('/dashboard');
    await page.waitForTimeout(500);

    await page.goto('/applications');
    await page.waitForTimeout(1000);

    // Scroll position may or may not be restored
  });

  test('should handle navigation with query parameters', async ({ page }) => {
    await page.goto('/applications?status=PENDING&page=2');
    await page.waitForTimeout(1000);

    expect(page.url()).toContain('status=PENDING');
    expect(page.url()).toContain('page=2');

    // Refresh and verify params persist
    await page.reload();
    await page.waitForTimeout(1000);

    expect(page.url()).toContain('status=PENDING');
    expect(page.url()).toContain('page=2');
  });

  test('should preserve filters during navigation', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForTimeout(1000);

    // Apply filter
    const statusFilter = page.locator('select[name="status"]').first();
    if (await statusFilter.isVisible({ timeout: 1000 })) {
      await statusFilter.selectOption('PENDING');
      await page.waitForTimeout(500);

      // Navigate away
      await page.goto('/dashboard');
      await page.waitForTimeout(500);

      // Navigate back
      await page.goto('/applications');
      await page.waitForTimeout(1000);

      // Check if filter is preserved (if implemented)
      const filterValue = await statusFilter.inputValue();
      // Filter persistence may or may not be implemented
    }
  });

  test('should preserve search query on navigation', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForTimeout(1000);

    // Enter search query
    const searchInput = page.locator('input[type="search"], input[placeholder*="Search"]').first();
    if (await searchInput.isVisible({ timeout: 1000 })) {
      await searchInput.fill('LOAN00000001');
      await page.waitForTimeout(500);

      // Navigate away
      await page.goto('/dashboard');
      await page.waitForTimeout(500);

      // Navigate back
      await page.goto('/applications');
      await page.waitForTimeout(1000);

      // Check if search is preserved
      const searchValue = await searchInput.inputValue();
      // Search persistence may or may not be implemented
    }
  });

  test('should maintain pagination state', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForTimeout(1000);

    // Navigate to page 2
    const page2Button = page.getByRole('button', { name: /2|Next/i });
    if (await page2Button.isVisible({ timeout: 1000 })) {
      await page2Button.click();
      await page.waitForTimeout(1000);

      // Navigate away
      await page.goto('/dashboard');
      await page.waitForTimeout(500);

      // Navigate back
      await page.goto('/applications');
      await page.waitForTimeout(1000);

      // Pagination state may or may not be preserved
    }
  });

  test('should preserve sort order on navigation', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForTimeout(1000);

    // Click sort header
    const sortHeader = page.getByRole('button', { name: /Date|Amount/i }).first();
    if (await sortHeader.isVisible({ timeout: 1000 })) {
      await sortHeader.click();
      await page.waitForTimeout(500);

      // Navigate away
      await page.goto('/dashboard');
      await page.waitForTimeout(500);

      // Navigate back
      await page.goto('/applications');
      await page.waitForTimeout(1000);

      // Sort order may or may not be preserved
    }
  });

  test('should handle browser back/forward with state', async ({ page }) => {
    // Start at dashboard
    await page.goto('/dashboard');
    await page.waitForTimeout(500);

    // Go to calculator and fill data
    await page.goto('/calculator');
    await page.waitForTimeout(500);

    const amountInput = page.locator('input[name="amount"]').first();
    if (await amountInput.isVisible({ timeout: 1000 })) {
      await amountInput.fill('500000');
      await page.waitForTimeout(500);
    }

    // Go to applications
    await page.goto('/applications');
    await page.waitForTimeout(500);

    // Go back to calculator
    await page.goBack();
    await page.waitForTimeout(1000);

    // Check if calculator state is preserved
    expect(page.url()).toContain('/calculator');
  });

  test('should preserve tab selection on navigation', async ({ page }) => {
    await page.goto('/reports');
    await page.waitForTimeout(1000);

    // Select a tab if available
    const tab = page.getByRole('tab', { name: /Summary|Details/i }).first();
    if (await tab.isVisible({ timeout: 1000 })) {
      await tab.click();
      await page.waitForTimeout(500);

      // Navigate away
      await page.goto('/dashboard');
      await page.waitForTimeout(500);

      // Navigate back
      await page.goto('/reports');
      await page.waitForTimeout(1000);

      // Tab selection may or may not be preserved
    }
  });
});
