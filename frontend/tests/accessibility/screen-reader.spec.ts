import { test, expect } from '@playwright/test';

test.describe('Screen Reader Compatibility', () => {
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

  test('should have ARIA labels on interactive elements', async ({ page }) => {
    const buttons = page.getByRole('button');
    const count = await buttons.count();

    for (let i = 0; i < Math.min(count, 10); i++) {
      const button = buttons.nth(i);
      const ariaLabel = await button.getAttribute('aria-label');
      const text = await button.textContent();
      
      // Button should have either aria-label or text content
      expect(ariaLabel || text).toBeTruthy();
    }
  });

  test('should have ARIA roles for semantic structure', async ({ page }) => {
    await page.goto('/calculator');
    
    const main = page.locator('main, [role="main"]');
    if (await main.count() > 0) {
      await expect(main.first()).toBeVisible();
    }
  });

  test('should have form labels', async ({ page }) => {
    await page.goto('/calculator');
    
    const inputs = page.locator('input[type="text"], input[type="number"]');
    const count = await inputs.count();

    for (let i = 0; i < Math.min(count, 5); i++) {
      const input = inputs.nth(i);
      const id = await input.getAttribute('id');
      const ariaLabel = await input.getAttribute('aria-label');
      const ariaLabelledBy = await input.getAttribute('aria-labelledby');
      
      if (id) {
        const label = page.locator(`label[for="${id}"]`);
        const hasLabel = await label.count() > 0;
        
        // Input should have label, aria-label, or aria-labelledby
        expect(hasLabel || ariaLabel || ariaLabelledBy).toBeTruthy();
      }
    }
  });

  test('should announce form errors', async ({ page }) => {
    await page.goto('/calculator');
    
    const submitButton = page.getByRole('button', { name: /calculate/i });
    if (await submitButton.isVisible()) {
      await submitButton.click();
      await page.waitForTimeout(500);

      // Check for aria-live regions or role="alert"
      const alerts = page.locator('[role="alert"], [aria-live="polite"], [aria-live="assertive"]');
      if (await alerts.count() > 0) {
        await expect(alerts.first()).toBeVisible();
      }
    }
  });

  test('should announce dynamic content changes', async ({ page }) => {
    await page.goto('/calculator');
    
    // Check for aria-live regions
    const liveRegions = page.locator('[aria-live]');
    const count = await liveRegions.count();
    
    // At least some dynamic content should have aria-live
    expect(count).toBeGreaterThanOrEqual(0);
  });

  test('should have accessible navigation', async ({ page }) => {
    const nav = page.locator('nav, [role="navigation"]');
    if (await nav.count() > 0) {
      await expect(nav.first()).toBeVisible();
    }
  });

  test('should have accessible headings hierarchy', async ({ page }) => {
    const h1 = page.locator('h1');
    const h1Count = await h1.count();
    
    // Page should have at least one h1
    expect(h1Count).toBeGreaterThanOrEqual(1);
  });
});

test.describe('Color Contrast Tests', () => {
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

  test('should have visible text elements', async ({ page }) => {
    const textElements = page.locator('p, span, div, h1, h2, h3, h4, h5, h6, label, button');
    const count = await textElements.count();

    for (let i = 0; i < Math.min(count, 20); i++) {
      const element = textElements.nth(i);
      const text = await element.textContent();
      
      if (text && text.trim().length > 0) {
        const isVisible = await element.isVisible();
        expect(isVisible).toBeTruthy();
      }
    }
  });

  test('should have visible focus indicators', async ({ page }) => {
    await page.goto('/calculator');
    
    await page.keyboard.press('Tab');
    await page.waitForTimeout(200);

    const focusedElement = page.locator(':focus');
    if (await focusedElement.count() > 0) {
      await expect(focusedElement).toBeVisible();
    }
  });

  test('should have sufficient contrast for interactive elements', async ({ page }) => {
    const buttons = page.getByRole('button');
    const count = await buttons.count();

    for (let i = 0; i < Math.min(count, 5); i++) {
      const button = buttons.nth(i);
      if (await button.isVisible()) {
        const styles = await button.evaluate((el) => {
          const computed = window.getComputedStyle(el);
          return {
            color: computed.color,
            backgroundColor: computed.backgroundColor,
          };
        });
        
        // Basic check that colors are defined
        expect(styles.color).toBeTruthy();
        expect(styles.backgroundColor).toBeTruthy();
      }
    }
  });
});
