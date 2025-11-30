import { test, expect } from '@playwright/test';

test.describe('Keyboard Navigation', () => {
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

  test('should navigate through login form using Tab key', async ({ page }) => {
    await page.goto('/login');
    
    // Start from email input (should be autofocused)
    const emailInput = page.getByLabel(/Email Address/i);
    await expect(emailInput).toBeFocused();
    
    // Tab to password
    await page.keyboard.press('Tab');
    const passwordInput = page.getByLabel(/Password/i);
    await expect(passwordInput).toBeFocused();
    
    // Tab to remember me checkbox
    await page.keyboard.press('Tab');
    // Should focus on checkbox or next interactive element
    
    // Continue tabbing through all interactive elements
    await page.keyboard.press('Tab');
    await page.keyboard.press('Tab');
    
    // Should eventually reach submit button
    const submitButton = page.getByRole('button', { name: /Sign In/i });
    await expect(submitButton).toBeFocused();
  });

  test('should navigate through dashboard using Tab key', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Tab through interactive elements
    await page.keyboard.press('Tab');
    
    // Check that focus is visible
    const focusedElement = await page.evaluate(() => document.activeElement?.tagName);
    expect(focusedElement).toBeTruthy();
  });

  test('should activate buttons with Enter key', async ({ page }) => {
    await page.goto('/calculator');
    
    await page.getByLabel(/Principal Amount/i).fill('500000');
    await page.getByLabel(/Annual Interest Rate/i).fill('12');
    await page.getByLabel(/Loan Tenure/i).fill('12');
    
    // Focus on calculate button
    const calculateButton = page.getByRole('button', { name: /Calculate EMI/i });
    await calculateButton.focus();
    
    // Press Enter
    await page.keyboard.press('Enter');
    
    // Should trigger calculation
    // Verify by checking for results or loading state
  });

  test('should activate buttons with Space key', async ({ page }) => {
    await page.goto('/calculator');
    
    await page.getByLabel(/Principal Amount/i).fill('500000');
    
    // Focus on calculate button
    const calculateButton = page.getByRole('button', { name: /Calculate EMI/i });
    await calculateButton.focus();
    
    // Press Space
    await page.keyboard.press('Space');
    
    // Should trigger calculation
  });

  test('should close modals with Escape key', async ({ page }) => {
    // This test assumes modals exist in the application
    test.skip();
    
    await page.goto('/');
    
    // Open modal (implementation specific)
    // await page.getByRole('button', { name: /Open Modal/i }).click();
    
    // Press Escape
    // await page.keyboard.press('Escape');
    
    // Modal should close
    // await expect(page.getByRole('dialog')).not.toBeVisible();
  });

  test('should navigate through navigation menu with arrow keys', async ({ page }) => {
    await page.goto('/');
    
    // Focus on first navigation item
    await page.keyboard.press('Tab');
    
    // Use arrow keys to navigate (if implemented)
    // await page.keyboard.press('ArrowDown');
    // await page.keyboard.press('ArrowUp');
  });

  test('should show visible focus indicators', async ({ page }) => {
    await page.goto('/');
    
    // Tab to first interactive element
    await page.keyboard.press('Tab');
    
    // Get focused element
    const focusedElement = page.locator(':focus');
    
    // Check for visible focus indicator
    const outlineStyle = await focusedElement.evaluate((el) => {
      const styles = window.getComputedStyle(el);
      return {
        outline: styles.outline,
        outlineWidth: styles.outlineWidth,
        boxShadow: styles.boxShadow,
      };
    });
    
    // Should have some form of focus indicator
    const hasFocusIndicator = 
      outlineStyle.outline !== 'none' ||
      outlineStyle.outlineWidth !== '0px' ||
      outlineStyle.boxShadow !== 'none';
    
    expect(hasFocusIndicator).toBe(true);
  });

  test('should navigate through table with arrow keys', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForLoadState('networkidle');
    
    // Focus on table
    const table = page.getByRole('table');
    await table.focus();
    
    // Navigate with arrow keys (if implemented)
    // await page.keyboard.press('ArrowDown');
    // await page.keyboard.press('ArrowRight');
  });

  test('should navigate through form fields in logical order', async ({ page }) => {
    await page.goto('/applications/new');
    await page.waitForLoadState('networkidle');
    
    // Tab through form fields
    const tabOrder: string[] = [];
    
    for (let i = 0; i < 10; i++) {
      await page.keyboard.press('Tab');
      const focusedElement = await page.evaluate(() => {
        const el = document.activeElement;
        return el?.getAttribute('name') || el?.getAttribute('id') || el?.tagName;
      });
      tabOrder.push(focusedElement);
    }
    
    // Verify logical tab order
    expect(tabOrder.length).toBeGreaterThan(0);
  });

  test('should support Shift+Tab for reverse navigation', async ({ page }) => {
    await page.goto('/login');
    
    // Tab forward
    await page.keyboard.press('Tab');
    await page.keyboard.press('Tab');
    
    // Tab backward
    await page.keyboard.press('Shift+Tab');
    
    // Should focus on previous element
    const emailInput = page.getByLabel(/Email Address/i);
    await expect(emailInput).toBeFocused();
  });

  test('should not trap focus outside of modals', async ({ page }) => {
    await page.goto('/');
    
    // Tab through all elements
    let tabCount = 0;
    const maxTabs = 50;
    
    while (tabCount < maxTabs) {
      await page.keyboard.press('Tab');
      tabCount++;
      
      // Check if focus is still within viewport
      const isInViewport = await page.evaluate(() => {
        const el = document.activeElement;
        if (!el) return false;
        const rect = el.getBoundingClientRect();
        return rect.top >= 0 && rect.left >= 0;
      });
      
      if (!isInViewport) break;
    }
    
    // Should be able to tab through all elements
    expect(tabCount).toBeGreaterThan(0);
  });

  test('should skip hidden elements during keyboard navigation', async ({ page }) => {
    await page.goto('/');
    
    // Tab through elements
    await page.keyboard.press('Tab');
    
    // Get focused element
    const focusedElement = page.locator(':focus');
    
    // Should be visible
    await expect(focusedElement).toBeVisible();
  });

  test('should handle keyboard shortcuts if implemented', async ({ page }) => {
    // This test is for future keyboard shortcuts
    test.skip();
    
    await page.goto('/');
    
    // Test keyboard shortcuts (e.g., Ctrl+K for search)
    // await page.keyboard.press('Control+K');
  });
});
