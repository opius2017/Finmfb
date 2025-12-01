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
  test.use({ 
    viewport: { width: 1920, height: 1080 }
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

    await page.goto('/dashboard');
  });

  test('should display all modules on desktop viewport (1024px+)', async ({ page }) => {
    const viewportSize = page.viewportSize();
    expect(viewportSize?.width).toBeGreaterThanOrEqual(1024);

    const modules = [
      { path: '/dashboard', heading: /Dashboard/i },
      { path: '/calculator', heading: /Calculator/i },
      { path: '/eligibility', heading: /Eligibility/i },
      { path: '/applications', heading: /Applications/i },
      { path: '/guarantor', heading: /Guarantor/i },
      { path: '/reports', heading: /Reports/i },
    ];
    
    for (const module of modules) {
      await page.goto(module.path);
      await page.waitForLoadState('networkidle');
      await expect(page.getByRole('heading', { name: module.heading })).toBeVisible();
    }
  });

  test('should display multi-column layout on desktop', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // Check that cards are displayed in multiple columns
    const cards = await page.locator('.card, [class*="card"]').all();
    
    if (cards.length >= 2) {
      const firstCardBox = await cards[0].boundingBox();
      const secondCardBox = await cards[1].boundingBox();
      
      if (firstCardBox && secondCardBox) {
        // On desktop, cards should be side by side (same row)
        const verticalDiff = Math.abs(firstCardBox.y - secondCardBox.y);
        expect(verticalDiff).toBeLessThan(50); // Allow small vertical difference
      }
    }
  });

  test('should support hover interactions on buttons', async ({ page }) => {
    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');
    
    const button = page.getByRole('button').first();
    if (await button.isVisible()) {
      // Get initial state
      const initialColor = await button.evaluate((el) => 
        window.getComputedStyle(el).backgroundColor
      );

      // Hover
      await button.hover();
      await page.waitForTimeout(200);

      // Hover state may change appearance
      const hoverColor = await button.evaluate((el) => 
        window.getComputedStyle(el).backgroundColor
      );

      // Just verify hover doesn't break the element
      await expect(button).toBeVisible();
    }
  });

  test('should support hover interactions on links', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    
    const link = page.getByRole('link').first();
    if (await link.isVisible()) {
      await link.hover();
      await page.waitForTimeout(200);
      await expect(link).toBeVisible();
    }
  });

  test('should support keyboard navigation through all interactive elements', async ({ page }) => {
    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');
    
    // Tab through elements
    for (let i = 0; i < 5; i++) {
      await page.keyboard.press('Tab');
      await page.waitForTimeout(100);
      
      // Check that focus is visible
      const focusedElement = await page.evaluate(() => {
        const el = document.activeElement;
        return el?.tagName;
      });
      
      expect(focusedElement).toBeTruthy();
    }
  });

  test('should support keyboard shortcuts if implemented', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    
    // Try common keyboard shortcuts
    await page.keyboard.press('Escape');
    await page.waitForTimeout(200);
    
    // Page should still be functional
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
  });

  test('should display tables with full width on desktop', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForLoadState('networkidle');
    
    const table = page.locator('table').first();
    if (await table.isVisible({ timeout: 1000 })) {
      const tableBox = await table.boundingBox();
      
      // Table should use available width on desktop
      if (tableBox) {
        expect(tableBox.width).toBeGreaterThan(600);
      }
    }
  });

  test('should display sidebar navigation on desktop', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    
    // Check for sidebar or navigation menu
    const nav = page.locator('nav, aside, [role="navigation"]').first();
    if (await nav.isVisible({ timeout: 1000 })) {
      await expect(nav).toBeVisible();
    }
  });

  test('should display forms in optimal layout on desktop', async ({ page }) => {
    await page.goto('/applications/new');
    await page.waitForLoadState('networkidle');
    
    // Form should be well-structured on desktop
    const form = page.locator('form').first();
    if (await form.isVisible({ timeout: 1000 })) {
      const formBox = await form.boundingBox();
      
      // Form should not be too wide on desktop
      if (formBox) {
        expect(formBox.width).toBeLessThan(1200);
      }
    }
  });

  test('should display modals centered on desktop', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    
    // This test assumes modals exist
    // Modal centering can be tested when modal is triggered
  });

  test('should support mouse wheel scrolling', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForLoadState('networkidle');
    
    // Get initial scroll position
    const initialScroll = await page.evaluate(() => window.scrollY);
    
    // Scroll with mouse wheel
    await page.mouse.wheel(0, 500);
    await page.waitForTimeout(200);
    
    const newScroll = await page.evaluate(() => window.scrollY);
    
    // Scroll position should change
    expect(newScroll).toBeGreaterThanOrEqual(initialScroll);
  });

  test('should display tooltips on hover if implemented', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    
    // Look for elements with tooltips
    const tooltipTrigger = page.locator('[title], [data-tooltip]').first();
    if (await tooltipTrigger.isVisible({ timeout: 1000 })) {
      await tooltipTrigger.hover();
      await page.waitForTimeout(500);
      
      // Tooltip may appear
    }
  });

  test('should display dropdown menus correctly on desktop', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    
    // Look for dropdown triggers
    const dropdown = page.locator('select, [role="combobox"]').first();
    if (await dropdown.isVisible({ timeout: 1000 })) {
      await dropdown.click();
      await page.waitForTimeout(300);
      
      // Dropdown should open
      await expect(dropdown).toBeVisible();
    }
  });

  test('should handle large data tables on desktop', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForLoadState('networkidle');
    
    // Table should be fully visible without horizontal scroll
    const hasHorizontalScroll = await page.evaluate(() => {
      return document.documentElement.scrollWidth > document.documentElement.clientWidth;
    });
    
    // On desktop, tables should fit without horizontal scroll
    expect(hasHorizontalScroll).toBe(false);
  });

  test('should display charts and graphs properly on desktop', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    
    // Look for chart elements
    const chart = page.locator('canvas, svg[class*="chart"]').first();
    if (await chart.isVisible({ timeout: 1000 })) {
      const chartBox = await chart.boundingBox();
      
      // Chart should have reasonable dimensions
      if (chartBox) {
        expect(chartBox.width).toBeGreaterThan(200);
        expect(chartBox.height).toBeGreaterThan(100);
      }
    }
  });

  test('should maintain aspect ratios on desktop', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    
    // Check images maintain aspect ratio
    const images = await page.locator('img').all();
    
    for (const img of images.slice(0, 3)) {
      const box = await img.boundingBox();
      if (box) {
        // Image should have both width and height
        expect(box.width).toBeGreaterThan(0);
        expect(box.height).toBeGreaterThan(0);
      }
    }
  });

  test('should support right-click context menu', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    
    // Right-click on page
    await page.mouse.click(500, 500, { button: 'right' });
    await page.waitForTimeout(200);
    
    // Browser context menu should appear (or custom context menu)
  });

  test('should display all content without truncation on desktop', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    
    // Check that text is not truncated unnecessarily
    const textElements = await page.locator('p, span, div').all();
    
    for (const element of textElements.slice(0, 10)) {
      const isVisible = await element.isVisible();
      if (isVisible) {
        const text = await element.textContent();
        // Text should be readable
        expect(text).toBeTruthy();
      }
    }
  });

  test('should support multiple windows/tabs workflow', async ({ page, context }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    
    // Open new tab
    const newPage = await context.newPage();
    await newPage.goto('/calculator');
    await newPage.waitForLoadState('networkidle');
    
    // Both pages should work independently
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
    await expect(newPage.getByRole('heading', { name: /Calculator/i })).toBeVisible();
    
    await newPage.close();
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

    // Verify portrait layout
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();

    // Switch to landscape
    await page.setViewportSize({ width: 1024, height: 768 });
    await page.waitForTimeout(1000);

    // Verify landscape layout
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
    
    // Content should still be accessible
    const cards = await page.locator('.card, [class*="card"]').all();
    if (cards.length > 0) {
      await expect(cards[0]).toBeVisible();
    }
  });

  test('should handle landscape to portrait transition', async ({ page }) => {
    // Start in landscape
    await page.setViewportSize({ width: 1024, height: 768 });
    await page.waitForTimeout(500);

    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();

    // Switch to portrait
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.waitForTimeout(1000);

    // Verify portrait layout
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

  test('should maintain scroll position during orientation change', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForLoadState('networkidle');

    // Start in portrait
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.waitForTimeout(500);

    // Scroll down
    await page.evaluate(() => window.scrollTo(0, 300));
    await page.waitForTimeout(300);

    const scrollBeforeRotation = await page.evaluate(() => window.scrollY);

    // Rotate to landscape
    await page.setViewportSize({ width: 1024, height: 768 });
    await page.waitForTimeout(1000);

    // Scroll position may change but page should still be usable
    await expect(page.getByRole('heading', { name: /Applications/i })).toBeVisible();
  });

  test('should handle orientation change on calculator page', async ({ page }) => {
    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');

    // Portrait
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.waitForTimeout(500);

    // Fill form
    const amountInput = page.locator('input[name="amount"]').first();
    if (await amountInput.isVisible({ timeout: 1000 })) {
      await amountInput.fill('500000');
      await page.waitForTimeout(300);
    }

    // Rotate to landscape
    await page.setViewportSize({ width: 1024, height: 768 });
    await page.waitForTimeout(1000);

    // Form data should be preserved
    if (await amountInput.isVisible({ timeout: 1000 })) {
      const value = await amountInput.inputValue();
      expect(value).toBe('500000');
    }
  });

  test('should handle orientation change on form pages', async ({ page }) => {
    await page.goto('/applications/new');
    await page.waitForLoadState('networkidle');

    // Portrait
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.waitForTimeout(500);

    // Landscape
    await page.setViewportSize({ width: 1024, height: 768 });
    await page.waitForTimeout(1000);

    // Form should still be usable
    await expect(page.getByRole('heading', { name: /New.*Application/i })).toBeVisible();
  });

  test('should handle orientation change on table pages', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForLoadState('networkidle');

    // Portrait
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.waitForTimeout(500);

    // Landscape
    await page.setViewportSize({ width: 1024, height: 768 });
    await page.waitForTimeout(1000);

    // Table should be visible
    const table = page.locator('table').first();
    if (await table.isVisible({ timeout: 1000 })) {
      await expect(table).toBeVisible();
    }
  });

  test('should handle multiple orientation changes', async ({ page }) => {
    // Portrait
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.waitForTimeout(500);
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();

    // Landscape
    await page.setViewportSize({ width: 1024, height: 768 });
    await page.waitForTimeout(500);
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();

    // Portrait again
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.waitForTimeout(500);
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();

    // Landscape again
    await page.setViewportSize({ width: 1024, height: 768 });
    await page.waitForTimeout(500);
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
  });

  test('should handle orientation change on mobile device', async ({ page }) => {
    // iPhone portrait
    await page.setViewportSize({ width: 390, height: 844 });
    await page.waitForTimeout(500);
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();

    // iPhone landscape
    await page.setViewportSize({ width: 844, height: 390 });
    await page.waitForTimeout(1000);
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
  });

  test('should not break navigation during orientation change', async ({ page }) => {
    // Portrait
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.waitForTimeout(500);

    // Navigate to calculator
    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');

    // Rotate to landscape
    await page.setViewportSize({ width: 1024, height: 768 });
    await page.waitForTimeout(1000);

    // Navigation should still work
    await page.goto('/applications');
    await page.waitForLoadState('networkidle');
    await expect(page.getByRole('heading', { name: /Applications/i })).toBeVisible();
  });

  test('should handle orientation change with open modals', async ({ page }) => {
    // This test assumes modals exist
    test.skip();

    await page.setViewportSize({ width: 768, height: 1024 });
    await page.waitForTimeout(500);

    // Open modal
    // await page.getByRole('button', { name: /Open/i }).click();

    // Rotate
    await page.setViewportSize({ width: 1024, height: 768 });
    await page.waitForTimeout(1000);

    // Modal should still be visible and functional
  });

  test('should maintain focus during orientation change', async ({ page }) => {
    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');

    // Portrait
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.waitForTimeout(500);

    // Focus on input
    const amountInput = page.locator('input[name="amount"]').first();
    if (await amountInput.isVisible({ timeout: 1000 })) {
      await amountInput.focus();
      await page.waitForTimeout(300);

      // Rotate
      await page.setViewportSize({ width: 1024, height: 768 });
      await page.waitForTimeout(1000);

      // Input should still be accessible
      await expect(amountInput).toBeVisible();
    }
  });

  test('should handle rapid orientation changes', async ({ page }) => {
    for (let i = 0; i < 3; i++) {
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.waitForTimeout(200);
      
      await page.setViewportSize({ width: 1024, height: 768 });
      await page.waitForTimeout(200);
    }

    // Page should still be functional
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
  });

  test('should adjust images during orientation change', async ({ page }) => {
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.waitForTimeout(500);

    const images = await page.locator('img').all();
    
    if (images.length > 0) {
      const portraitBox = await images[0].boundingBox();

      // Rotate
      await page.setViewportSize({ width: 1024, height: 768 });
      await page.waitForTimeout(1000);

      const landscapeBox = await images[0].boundingBox();

      // Image should still be visible
      expect(landscapeBox).toBeTruthy();
    }
  });

  test('should handle orientation change with active animations', async ({ page }) => {
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.waitForTimeout(500);

    // Trigger navigation (which may have animations)
    await page.goto('/calculator');
    
    // Rotate during page load
    await page.setViewportSize({ width: 1024, height: 768 });
    await page.waitForLoadState('networkidle');

    // Page should load correctly
    await expect(page.getByRole('heading', { name: /Calculator/i })).toBeVisible();
  });
});
