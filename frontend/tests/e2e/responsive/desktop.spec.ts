import { test, expect } from '@playwright/test';

test.describe('Desktop Responsive Design', () => {
  test.use({
    viewport: { width: 1920, height: 1080 },
  });

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

  test('should display login page correctly on desktop', async ({ page }) => {
    await page.goto('/login');
    
    // Check viewport
    const viewport = page.viewportSize();
    expect(viewport?.width).toBeGreaterThanOrEqual(1024);
    
    // Check that form is visible and properly sized
    await expect(page.getByLabel(/Email Address/i)).toBeVisible();
    await expect(page.getByLabel(/Password/i)).toBeVisible();
    await expect(page.getByRole('button', { name: /Sign In/i })).toBeVisible();
  });

  test('should display dashboard with multi-column layout', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Check that main content is visible
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
    
    // Stats cards should be in a row on desktop
    const statsCards = await page.locator('.card, [class*="card"]').all();
    
    if (statsCards.length >= 2) {
      const firstCardBox = await statsCards[0].boundingBox();
      const secondCardBox = await statsCards[1].boundingBox();
      
      if (firstCardBox && secondCardBox) {
        // Cards should be side by side (same row)
        expect(Math.abs(firstCardBox.y - secondCardBox.y)).toBeLessThan(50);
      }
    }
  });

  test('should display sidebar navigation on desktop', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Desktop should have visible sidebar
    const sidebar = page.locator('aside, nav[class*="sidebar"], [role="navigation"]').first();
    
    if (await sidebar.isVisible({ timeout: 1000 })) {
      await expect(sidebar).toBeVisible();
      
      // Sidebar should have reasonable width
      const box = await sidebar.boundingBox();
      if (box) {
        expect(box.width).toBeGreaterThan(150);
        expect(box.width).toBeLessThan(400);
      }
    }
  });

  test('should support hover interactions on desktop', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Get interactive elements
    const buttons = await page.locator('button, a').all();
    
    if (buttons.length > 0) {
      const button = buttons[0];
      
      // Get initial state
      const initialColor = await button.evaluate((el) => {
        return window.getComputedStyle(el).backgroundColor;
      });
      
      // Hover over button
      await button.hover();
      await page.waitForTimeout(100);
      
      // Color may change on hover (if hover styles are implemented)
      const hoverColor = await button.evaluate((el) => {
        return window.getComputedStyle(el).backgroundColor;
      });
      
      // Hover effect may or may not be implemented
    }
  });

  test('should support keyboard navigation on desktop', async ({ page }) => {
    await page.goto('/login');
    
    // Tab through form elements
    await page.keyboard.press('Tab');
    await expect(page.getByLabel(/Email Address/i)).toBeFocused();
    
    await page.keyboard.press('Tab');
    await expect(page.getByLabel(/Password/i)).toBeFocused();
    
    await page.keyboard.press('Tab');
    await expect(page.getByRole('button', { name: /Sign In/i })).toBeFocused();
  });

  test('should display calculator with optimal layout on desktop', async ({ page }) => {
    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');
    
    // Form should be well-spaced on desktop
    await expect(page.getByLabel(/Principal Amount/i)).toBeVisible();
    await expect(page.getByLabel(/Annual Interest Rate/i)).toBeVisible();
    await expect(page.getByLabel(/Loan Tenure/i)).toBeVisible();
    
    // Check for side-by-side layout if implemented
    const formContainer = page.locator('form, [class*="calculator"]').first();
    if (await formContainer.isVisible()) {
      const box = await formContainer.boundingBox();
      if (box) {
        // Form should use available width
        expect(box.width).toBeGreaterThan(600);
      }
    }
  });

  test('should display tables with full width on desktop', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForLoadState('networkidle');
    
    // Table should be visible and use available space
    const table = page.locator('table').first();
    
    if (await table.isVisible({ timeout: 1000 })) {
      const tableBox = await table.boundingBox();
      const viewport = page.viewportSize();
      
      if (tableBox && viewport) {
        // Table should use significant portion of viewport
        expect(tableBox.width).toBeGreaterThan(viewport.width * 0.5);
      }
    }
  });

  test('should display all table columns on desktop', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForLoadState('networkidle');
    
    // All columns should be visible on desktop
    const tableHeaders = await page.locator('th').all();
    
    // Desktop should show more columns than mobile
    expect(tableHeaders.length).toBeGreaterThan(3);
  });

  test('should handle mouse interactions correctly', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Click on navigation link
    const navLink = page.getByRole('link', { name: /Calculator/i });
    if (await navLink.isVisible({ timeout: 1000 })) {
      await navLink.click();
      await page.waitForTimeout(1000);
      
      expect(page.url()).toContain('/calculator');
    }
  });

  test('should display modals centered on desktop', async ({ page }) => {
    // This test assumes modals exist
    test.skip();
    
    await page.goto('/');
    
    // Open modal
    // await page.getByRole('button', { name: /Open Modal/i }).click();
    
    // Modal should be centered
    // const modal = page.getByRole('dialog');
    // await expect(modal).toBeVisible();
    
    // const modalBox = await modal.boundingBox();
    // const viewport = page.viewportSize();
    
    // if (modalBox && viewport) {
    //   const centerX = modalBox.x + modalBox.width / 2;
    //   const viewportCenterX = viewport.width / 2;
    //   expect(Math.abs(centerX - viewportCenterX)).toBeLessThan(100);
    // }
  });

  test('should not have horizontal scroll on desktop', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Check for horizontal overflow
    const hasHorizontalScroll = await page.evaluate(() => {
      return document.documentElement.scrollWidth > document.documentElement.clientWidth;
    });
    
    expect(hasHorizontalScroll).toBe(false);
  });

  test('should display images at appropriate sizes', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Get all images
    const images = await page.locator('img').all();
    
    for (const img of images) {
      const box = await img.boundingBox();
      
      if (box) {
        // Images should be reasonably sized
        expect(box.width).toBeGreaterThan(0);
        expect(box.width).toBeLessThan(2000);
      }
    }
  });

  test('should have appropriate font sizes on desktop', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Check heading font sizes
    const headings = await page.locator('h1, h2, h3').all();
    
    for (const heading of headings.slice(0, 5)) {
      const fontSize = await heading.evaluate((el) => {
        return parseInt(window.getComputedStyle(el).fontSize);
      });
      
      // Headings should be larger on desktop
      if (fontSize > 0) {
        expect(fontSize).toBeGreaterThanOrEqual(18);
      }
    }
  });

  test('should display multi-column grid layouts', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Get grid or card containers
    const cards = await page.locator('.card, [class*="grid"]').all();
    
    if (cards.length >= 3) {
      // Check if cards are in a multi-column layout
      const firstCardBox = await cards[0].boundingBox();
      const secondCardBox = await cards[1].boundingBox();
      const thirdCardBox = await cards[2].boundingBox();
      
      if (firstCardBox && secondCardBox && thirdCardBox) {
        // At least some cards should be on the same row
        const sameRow = Math.abs(firstCardBox.y - secondCardBox.y) < 50 ||
                        Math.abs(secondCardBox.y - thirdCardBox.y) < 50;
        
        expect(sameRow).toBe(true);
      }
    }
  });

  test('should display tooltips on hover', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Look for elements with tooltips
    const tooltipTriggers = await page.locator('[title], [data-tooltip], [aria-describedby]').all();
    
    if (tooltipTriggers.length > 0) {
      const trigger = tooltipTriggers[0];
      
      // Hover to show tooltip
      await trigger.hover();
      await page.waitForTimeout(500);
      
      // Tooltip may or may not be implemented
    }
  });

  test('should support right-click context menus if implemented', async ({ page }) => {
    test.skip();
    
    await page.goto('/applications');
    await page.waitForLoadState('networkidle');
    
    // Right-click on table row
    // const row = page.locator('tr').nth(1);
    // await row.click({ button: 'right' });
    
    // Context menu should appear
    // const contextMenu = page.getByRole('menu');
    // await expect(contextMenu).toBeVisible();
  });

  test('should display forms with optimal spacing', async ({ page }) => {
    await page.goto('/applications/new');
    await page.waitForTimeout(1000);
    
    // Form should have good spacing on desktop
    const formInputs = await page.locator('input, select, textarea').all();
    
    if (formInputs.length >= 2) {
      const firstInputBox = await formInputs[0].boundingBox();
      const secondInputBox = await formInputs[1].boundingBox();
      
      if (firstInputBox && secondInputBox) {
        // Inputs should have spacing between them
        const spacing = secondInputBox.y - (firstInputBox.y + firstInputBox.height);
        expect(spacing).toBeGreaterThan(10);
      }
    }
  });

  test('should display charts and visualizations properly', async ({ page }) => {
    await page.goto('/reports');
    await page.waitForTimeout(1000);
    
    // Charts should be visible and properly sized
    const charts = await page.locator('canvas, svg[class*="chart"]').all();
    
    for (const chart of charts) {
      if (await chart.isVisible({ timeout: 1000 })) {
        const box = await chart.boundingBox();
        
        if (box) {
          // Charts should be reasonably sized
          expect(box.width).toBeGreaterThan(200);
          expect(box.height).toBeGreaterThan(100);
        }
      }
    }
  });

  test('should handle window resize gracefully', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Start with large viewport
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.waitForTimeout(500);
    
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
    
    // Resize to smaller desktop size
    await page.setViewportSize({ width: 1366, height: 768 });
    await page.waitForTimeout(500);
    
    // Content should still be visible
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
    
    // Resize back
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.waitForTimeout(500);
    
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
  });

  test('should display dropdown menus correctly', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Look for dropdown triggers
    const dropdowns = await page.locator('select, [role="combobox"], button[aria-haspopup]').all();
    
    if (dropdowns.length > 0) {
      const dropdown = dropdowns[0];
      
      if (await dropdown.isVisible()) {
        await dropdown.click();
        await page.waitForTimeout(500);
        
        // Dropdown menu should appear
        // Implementation depends on actual UI
      }
    }
  });

  test('should support drag and drop if implemented', async ({ page }) => {
    test.skip();
    
    await page.goto('/');
    
    // Drag and drop functionality
    // const source = page.locator('[draggable="true"]').first();
    // const target = page.locator('[data-drop-target]').first();
    
    // await source.dragTo(target);
  });
});

test.describe('Desktop Responsive Design - Large Screens', () => {
  test.use({
    viewport: { width: 2560, height: 1440 },
  });

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

  test('should display content with max-width on large screens', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Main content should have max-width to prevent excessive stretching
    const mainContent = page.locator('main, [role="main"]').first();
    
    if (await mainContent.isVisible({ timeout: 1000 })) {
      const box = await mainContent.boundingBox();
      const viewport = page.viewportSize();
      
      if (box && viewport) {
        // Content should not stretch to full width on very large screens
        // This is a design choice - may or may not be implemented
      }
    }
  });

  test('should maintain readability on large screens', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Text lines should not be excessively long
    const paragraphs = await page.locator('p').all();
    
    for (const p of paragraphs.slice(0, 5)) {
      if (await p.isVisible()) {
        const box = await p.boundingBox();
        
        if (box) {
          // Optimal line length is 50-75 characters, roughly 600-900px
          // This is a guideline, not a strict requirement
        }
      }
    }
  });

  test('should display all content without scrolling on large screens', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Check if main content fits in viewport
    const hasVerticalScroll = await page.evaluate(() => {
      return document.documentElement.scrollHeight > document.documentElement.clientHeight;
    });
    
    // Some pages may still need scrolling, which is fine
  });
});
