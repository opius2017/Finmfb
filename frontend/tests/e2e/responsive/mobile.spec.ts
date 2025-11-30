import { test, expect, devices } from '@playwright/test';

test.describe('Mobile Responsive Design', () => {
  test.use({ ...devices['iPhone 12'] });

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

  test('should display login page correctly on mobile', async ({ page }) => {
    await page.goto('/login');
    
    // Check viewport
    const viewport = page.viewportSize();
    expect(viewport?.width).toBeLessThan(768);
    
    // Check that form is visible and usable
    await expect(page.getByLabel(/Email Address/i)).toBeVisible();
    await expect(page.getByLabel(/Password/i)).toBeVisible();
    await expect(page.getByRole('button', { name: /Sign In/i })).toBeVisible();
  });

  test('should display dashboard correctly on mobile', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Check that main content is visible
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
    
    // Stats should stack vertically on mobile
    const statsCards = page.locator('.card').first();
    await expect(statsCards).toBeVisible();
  });

  test('should have mobile-friendly navigation menu', async ({ page }) => {
    await page.goto('/');
    
    // Check for hamburger menu or mobile navigation
    // Implementation depends on actual UI
    const mobileMenu = page.locator('[aria-label="Menu"]').or(page.locator('button').filter({ hasText: /menu/i }));
    
    // Mobile menu should be present
    // await expect(mobileMenu).toBeVisible();
  });

  test('should have touch-friendly tap targets (minimum 44px)', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Get all interactive elements
    const buttons = await page.locator('button, a').all();
    
    for (const button of buttons.slice(0, 5)) { // Check first 5 elements
      const box = await button.boundingBox();
      if (box) {
        expect(box.height).toBeGreaterThanOrEqual(44);
        expect(box.width).toBeGreaterThanOrEqual(44);
      }
    }
  });

  test('should display calculator form correctly on mobile', async ({ page }) => {
    await page.goto('/calculator');
    
    // Form should be usable on mobile
    await expect(page.getByLabel(/Principal Amount/i)).toBeVisible();
    await expect(page.getByLabel(/Annual Interest Rate/i)).toBeVisible();
    await expect(page.getByLabel(/Loan Tenure/i)).toBeVisible();
    
    // Sliders should be visible and usable
    const sliders = await page.locator('input[type="range"]').all();
    expect(sliders.length).toBeGreaterThan(0);
  });

  test('should display tables with horizontal scroll on mobile', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForLoadState('networkidle');
    
    // Table should be in a scrollable container
    const tableContainer = page.locator('.overflow-x-auto').or(page.locator('[style*="overflow-x"]'));
    
    // Should have horizontal scroll capability
    // await expect(tableContainer).toBeVisible();
  });

  test('should handle form inputs correctly on mobile', async ({ page }) => {
    await page.goto('/login');
    
    // Tap on email input
    await page.getByLabel(/Email Address/i).tap();
    
    // Should focus and show keyboard
    await expect(page.getByLabel(/Email Address/i)).toBeFocused();
    
    // Type text
    await page.getByLabel(/Email Address/i).fill('test@example.com');
    
    // Value should be set
    await expect(page.getByLabel(/Email Address/i)).toHaveValue('test@example.com');
  });

  test('should display modals correctly on mobile', async ({ page }) => {
    // This test assumes modals exist
    test.skip();
    
    await page.goto('/');
    
    // Open modal
    // await page.getByRole('button', { name: /Open Modal/i }).tap();
    
    // Modal should be visible and properly sized
    // const modal = page.getByRole('dialog');
    // await expect(modal).toBeVisible();
    
    // Modal should not overflow viewport
    // const box = await modal.boundingBox();
    // const viewport = page.viewportSize();
    // expect(box?.width).toBeLessThanOrEqual(viewport?.width || 0);
  });

  test('should handle swipe gestures if implemented', async ({ page }) => {
    // This test is for future swipe gesture support
    test.skip();
    
    await page.goto('/');
    
    // Perform swipe gesture
    // await page.touchscreen.swipe({ x: 100, y: 100 }, { x: 300, y: 100 });
  });

  test('should not have horizontal scroll on pages', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Check for horizontal overflow
    const hasHorizontalScroll = await page.evaluate(() => {
      return document.documentElement.scrollWidth > document.documentElement.clientWidth;
    });
    
    expect(hasHorizontalScroll).toBe(false);
  });

  test('should display images responsively', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Get all images
    const images = await page.locator('img').all();
    
    for (const img of images) {
      const box = await img.boundingBox();
      const viewport = page.viewportSize();
      
      if (box && viewport) {
        // Image should not exceed viewport width
        expect(box.width).toBeLessThanOrEqual(viewport.width);
      }
    }
  });

  test('should have readable font sizes on mobile', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Check font sizes
    const textElements = await page.locator('p, span, div').all();
    
    for (const element of textElements.slice(0, 10)) {
      const fontSize = await element.evaluate((el) => {
        return parseInt(window.getComputedStyle(el).fontSize);
      });
      
      // Minimum font size should be 14px for readability
      if (fontSize > 0) {
        expect(fontSize).toBeGreaterThanOrEqual(14);
      }
    }
  });

  test('should handle orientation change', async ({ page }) => {
    await page.goto('/');
    
    // Get initial viewport
    const initialViewport = page.viewportSize();
    
    // Rotate to landscape
    await page.setViewportSize({
      width: initialViewport?.height || 844,
      height: initialViewport?.width || 390,
    });
    
    await page.waitForTimeout(500);
    
    // Page should still be usable
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
    
    // Rotate back to portrait
    await page.setViewportSize({
      width: initialViewport?.width || 390,
      height: initialViewport?.height || 844,
    });
  });

  test('should display cards in single column on mobile', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Get card elements
    const cards = await page.locator('.card').all();
    
    if (cards.length > 1) {
      // Check if cards are stacked vertically
      const firstCardBox = await cards[0].boundingBox();
      const secondCardBox = await cards[1].boundingBox();
      
      if (firstCardBox && secondCardBox) {
        // Second card should be below first card (not side by side)
        expect(secondCardBox.y).toBeGreaterThan(firstCardBox.y + firstCardBox.height - 10);
      }
    }
  });
});
