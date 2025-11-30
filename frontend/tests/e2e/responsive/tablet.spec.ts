import { test, expect, devices } from '@playwright/test';

test.describe('Tablet Viewport Tests', () => {
  test.use({ ...devices['iPad Pro'] });

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

  test('should display Dashboard on tablet', async ({ page }) => {
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
  });

  test('should display Calculator on tablet', async ({ page }) => {
    await page.goto('/calculator');
    await expect(page.getByRole('heading', { name: /Calculator/i })).toBeVisible();
  });

  test('should display Eligibility Check on tablet', async ({ page }) => {
    await page.goto('/eligibility');
    await expect(page.getByRole('heading', { name: /Eligibility/i })).toBeVisible();
  });

  test('should adapt layout for tablet', async ({ page }) => {
    const viewportSize = page.viewportSize();
    expect(viewportSize?.width).toBeGreaterThanOrEqual(768);
    expect(viewportSize?.width).toBeLessThan(1024);
  });

  test('should support touch interactions', async ({ page }) => {
    await page.goto('/calculator');
    
    const button = page.getByRole('button', { name: /calculate/i });
    if (await button.isVisible()) {
      await button.tap();
      await page.waitForTimeout(500);
    }
  });
});

test.describe('Desktop Viewport Tests', () => {
  test.use({ ...devices['Desktop Chrome'] });

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

  test('should display all modules on desktop', async ({ page }) => {
    const modules = ['/dashboard', '/calculator', '/eligibility', '/applications'];
    
    for (const module of modules) {
      await page.goto(module);
      await page.waitForLoadState('networkidle');
    }
  });

  test('should support hover interactions', async ({ page }) => {
    await page.goto('/calculator');
    
    const button = page.getByRole('button').first();
    if (await button.isVisible()) {
      await button.hover();
      await page.waitForTimeout(200);
    }
  });

  test('should support keyboard navigation', async ({ page }) => {
    await page.goto('/calculator');
    
    await page.keyboard.press('Tab');
    await page.keyboard.press('Tab');
    await page.keyboard.press('Enter');
  });
});

test.describe('Orientation Change Tests', () => {
  test.use({ ...devices['iPad Pro'] });

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

  test('should handle portrait to landscape transition', async ({ page }) => {
    // Start in portrait
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.waitForTimeout(500);

    // Switch to landscape
    await page.setViewportSize({ width: 1024, height: 768 });
    await page.waitForTimeout(1000);

    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
  });

  test('should adjust layout within 1 second', async ({ page }) => {
    const startTime = Date.now();

    await page.setViewportSize({ width: 768, height: 1024 });
    await page.waitForTimeout(100);

    await page.setViewportSize({ width: 1024, height: 768 });
    await page.waitForLoadState('networkidle');

    const adjustTime = Date.now() - startTime;
    expect(adjustTime).toBeLessThan(1000);
  });
});
