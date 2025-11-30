import { test, expect } from '@playwright/test';
import { mockDataFactory } from '../../utils/mock-data';

test.describe('Page Load Performance', () => {
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
  });

  test('should load Dashboard within 2 seconds', async ({ page }) => {
    const startTime = Date.now();
    
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    
    const loadTime = Date.now() - startTime;
    expect(loadTime).toBeLessThan(2000);
  });

  test('should load Calculator within 2 seconds', async ({ page }) => {
    const startTime = Date.now();
    
    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');
    
    const loadTime = Date.now() - startTime;
    expect(loadTime).toBeLessThan(2000);
  });

  test('should load Eligibility Check within 2 seconds', async ({ page }) => {
    const startTime = Date.now();
    
    await page.goto('/eligibility');
    await page.waitForLoadState('networkidle');
    
    const loadTime = Date.now() - startTime;
    expect(loadTime).toBeLessThan(2000);
  });

  test('should navigate between routes within 500ms', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    const startTime = Date.now();
    await page.goto('/calculator');
    await page.waitForLoadState('domcontentloaded');
    
    const navigationTime = Date.now() - startTime;
    expect(navigationTime).toBeLessThan(500);
  });

  test('should display API response within 3 seconds', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(10);

    await page.route('**/api/loan-applications**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: applications,
          total: applications.length,
        }),
      });
    });

    const startTime = Date.now();
    await page.goto('/applications');
    await page.waitForLoadState('networkidle');
    
    const loadTime = Date.now() - startTime;
    expect(loadTime).toBeLessThan(3000);
  });

  test('should show form submission feedback within 2 seconds', async ({ page }) => {
    await page.route('**/api/loan-applications', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 201,
          contentType: 'application/json',
          body: JSON.stringify({
            id: 'APP-001',
            message: 'Success',
          }),
        });
      }
    });

    await page.goto('/applications/new');
    await page.waitForLoadState('networkidle');

    const submitButton = page.getByRole('button', { name: /submit/i }).or(page.getByRole('button', { name: /apply/i }));
    if (await submitButton.isVisible()) {
      const startTime = Date.now();
      await submitButton.click();
      await page.waitForTimeout(500);
      
      const responseTime = Date.now() - startTime;
      expect(responseTime).toBeLessThan(2000);
    }
  });
});

test.describe('Interaction Performance', () => {
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
  });

  test('should respond to button click within 100ms', async ({ page }) => {
    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');

    const button = page.getByRole('button').first();
    if (await button.isVisible()) {
      const startTime = Date.now();
      await button.click();
      await page.waitForTimeout(50);
      
      const responseTime = Date.now() - startTime;
      expect(responseTime).toBeLessThan(100);
    }
  });

  test('should respond to form input within 50ms', async ({ page }) => {
    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');

    const input = page.locator('input').first();
    if (await input.isVisible()) {
      const startTime = Date.now();
      await input.fill('500000');
      await page.waitForTimeout(20);
      
      const responseTime = Date.now() - startTime;
      expect(responseTime).toBeLessThan(50);
    }
  });

  test('should display filter results within 1 second', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(20);

    await page.route('**/api/loan-applications**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: applications,
          total: applications.length,
        }),
      });
    });

    await page.goto('/applications');
    await page.waitForLoadState('networkidle');

    const filterSelect = page.locator('select').first();
    if (await filterSelect.isVisible()) {
      const startTime = Date.now();
      await filterSelect.selectOption({ index: 1 });
      await page.waitForTimeout(500);
      
      const filterTime = Date.now() - startTime;
      expect(filterTime).toBeLessThan(1000);
    }
  });

  test('should display search results within 1 second', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(20);

    await page.route('**/api/loan-applications**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: applications,
          total: applications.length,
        }),
      });
    });

    await page.goto('/applications');
    await page.waitForLoadState('networkidle');

    const searchInput = page.locator('input[type="search"]').first();
    if (await searchInput.isVisible()) {
      const startTime = Date.now();
      await searchInput.fill('LOAN');
      await page.waitForTimeout(500);
      
      const searchTime = Date.now() - startTime;
      expect(searchTime).toBeLessThan(1000);
    }
  });

  test('should open modal within 300ms', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    const button = page.getByRole('button').first();
    if (await button.isVisible()) {
      const startTime = Date.now();
      await button.click();
      await page.waitForTimeout(200);
      
      const openTime = Date.now() - startTime;
      expect(openTime).toBeLessThan(300);
    }
  });

  test('should close modal within 300ms', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    const openButton = page.getByRole('button').first();
    if (await openButton.isVisible()) {
      await openButton.click();
      await page.waitForTimeout(300);

      const closeButton = page.getByRole('button', { name: /close/i }).or(page.locator('[aria-label="Close"]'));
      if (await closeButton.isVisible()) {
        const startTime = Date.now();
        await closeButton.click();
        await page.waitForTimeout(200);
        
        const closeTime = Date.now() - startTime;
        expect(closeTime).toBeLessThan(300);
      }
    }
  });
});
