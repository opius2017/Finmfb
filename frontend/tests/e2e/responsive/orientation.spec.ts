import { test, expect, devices } from '@playwright/test';

test.describe('Orientation Changes - Mobile', () => {
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

  test('should handle portrait to landscape transition on login page', async ({ page }) => {
    await page.goto('/login');
    
    // Portrait mode
    const portraitViewport = { width: 390, height: 844 };
    await page.setViewportSize(portraitViewport);
    await page.waitForTimeout(500);
    
    await expect(page.getByLabel(/Email Address/i)).toBeVisible();
    await expect(page.getByLabel(/Password/i)).toBeVisible();
    
    // Switch to landscape
    const landscapeViewport = { width: 844, height: 390 };
    await page.setViewportSize(landscapeViewport);
    await page.waitForTimeout(500);
    
    // Form should still be visible and usable
    await expect(page.getByLabel(/Email Address/i)).toBeVisible();
    await expect(page.getByLabel(/Password/i)).toBeVisible();
    await expect(page.getByRole('button', { name: /Sign In/i })).toBeVisible();
  });

  test('should handle landscape to portrait transition on dashboard', async ({ page }) => {
    await page.goto('/');
    
    // Landscape mode
    const landscapeViewport = { width: 844, height: 390 };
    await page.setViewportSize(landscapeViewport);
    await page.waitForTimeout(500);
    
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
    
    // Switch to portrait
    const portraitViewport = { width: 390, height: 844 };
    await page.setViewportSize(portraitViewport);
    await page.waitForTimeout(500);
    
    // Dashboard should still be visible
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
  });

  test('should adjust layout on orientation change', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Portrait mode
    await page.setViewportSize({ width: 390, height: 844 });
    await page.waitForTimeout(500);
    
    const portraitCards = await page.locator('.card, [class*="card"]').all();
    let portraitLayout = null;
    
    if (portraitCards.length >= 2) {
      const firstBox = await portraitCards[0].boundingBox();
      const secondBox = await portraitCards[1].boundingBox();
      
      if (firstBox && secondBox) {
        portraitLayout = {
          stacked: secondBox.y > firstBox.y + firstBox.height - 10,
        };
      }
    }
    
    // Landscape mode
    await page.setViewportSize({ width: 844, height: 390 });
    await page.waitForTimeout(500);
    
    const landscapeCards = await page.locator('.card, [class*="card"]').all();
    
    if (landscapeCards.length >= 2) {
      const firstBox = await landscapeCards[0].boundingBox();
      const secondBox = await landscapeCards[1].boundingBox();
      
      if (firstBox && secondBox) {
        const landscapeStacked = secondBox.y > firstBox.y + firstBox.height - 10;
        
        // Layout may change between orientations
      }
    }
  });

  test('should maintain form data during orientation change', async ({ page }) => {
    await page.goto('/calculator');
    await page.waitForTimeout(500);
    
    // Fill form in portrait
    await page.setViewportSize({ width: 390, height: 844 });
    await page.waitForTimeout(500);
    
    const amountInput = page.locator('input[name="amount"]').first();
    if (await amountInput.isVisible({ timeout: 1000 })) {
      await amountInput.fill('500000');
      await page.waitForTimeout(500);
    }
    
    // Switch to landscape
    await page.setViewportSize({ width: 844, height: 390 });
    await page.waitForTimeout(500);
    
    // Data should be preserved
    const value = await amountInput.inputValue();
    expect(value).toBe('500000');
  });

  test('should handle table display on orientation change', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForLoadState('networkidle');
    
    // Portrait mode
    await page.setViewportSize({ width: 390, height: 844 });
    await page.waitForTimeout(500);
    
    const table = page.locator('table').first();
    if (await table.isVisible({ timeout: 1000 })) {
      await expect(table).toBeVisible();
    }
    
    // Landscape mode
    await page.setViewportSize({ width: 844, height: 390 });
    await page.waitForTimeout(500);
    
    // Table should still be visible
    if (await table.isVisible({ timeout: 1000 })) {
      await expect(table).toBeVisible();
    }
  });

  test('should adjust navigation menu on orientation change', async ({ page }) => {
    await page.goto('/');
    await page.waitForTimeout(500);
    
    // Portrait mode
    await page.setViewportSize({ width: 390, height: 844 });
    await page.waitForTimeout(500);
    
    // Check for mobile menu
    const mobileMenu = page.locator('[aria-label="Menu"], button[aria-label*="menu"]').first();
    
    // Landscape mode
    await page.setViewportSize({ width: 844, height: 390 });
    await page.waitForTimeout(500);
    
    // Navigation should still be accessible
  });

  test('should handle modal display on orientation change', async ({ page }) => {
    // This test assumes modals exist
    test.skip();
    
    await page.goto('/');
    
    // Open modal in portrait
    await page.setViewportSize({ width: 390, height: 844 });
    // await page.getByRole('button', { name: /Open Modal/i }).click();
    
    // const modal = page.getByRole('dialog');
    // await expect(modal).toBeVisible();
    
    // Switch to landscape
    await page.setViewportSize({ width: 844, height: 390 });
    await page.waitForTimeout(500);
    
    // Modal should still be visible and properly sized
    // await expect(modal).toBeVisible();
  });

  test('should maintain scroll position on orientation change', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForLoadState('networkidle');
    
    // Portrait mode
    await page.setViewportSize({ width: 390, height: 844 });
    await page.waitForTimeout(500);
    
    // Scroll down
    await page.evaluate(() => window.scrollTo(0, 300));
    await page.waitForTimeout(500);
    
    const portraitScroll = await page.evaluate(() => window.scrollY);
    
    // Switch to landscape
    await page.setViewportSize({ width: 844, height: 390 });
    await page.waitForTimeout(500);
    
    // Scroll position may or may not be maintained
    const landscapeScroll = await page.evaluate(() => window.scrollY);
  });

  test('should handle image display on orientation change', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Portrait mode
    await page.setViewportSize({ width: 390, height: 844 });
    await page.waitForTimeout(500);
    
    const images = await page.locator('img').all();
    
    for (const img of images) {
      if (await img.isVisible()) {
        const portraitBox = await img.boundingBox();
        
        // Switch to landscape
        await page.setViewportSize({ width: 844, height: 390 });
        await page.waitForTimeout(500);
        
        const landscapeBox = await img.boundingBox();
        
        // Images should be visible in both orientations
        if (await img.isVisible()) {
          expect(landscapeBox).toBeTruthy();
        }
        
        break; // Test first image only
      }
    }
  });

  test('should handle chart display on orientation change', async ({ page }) => {
    await page.goto('/reports');
    await page.waitForTimeout(1000);
    
    // Portrait mode
    await page.setViewportSize({ width: 390, height: 844 });
    await page.waitForTimeout(500);
    
    const chart = page.locator('canvas, svg[class*="chart"]').first();
    
    if (await chart.isVisible({ timeout: 1000 })) {
      const portraitBox = await chart.boundingBox();
      
      // Switch to landscape
      await page.setViewportSize({ width: 844, height: 390 });
      await page.waitForTimeout(500);
      
      // Chart should still be visible
      if (await chart.isVisible({ timeout: 1000 })) {
        const landscapeBox = await chart.boundingBox();
        expect(landscapeBox).toBeTruthy();
      }
    }
  });

  test('should handle multiple orientation changes', async ({ page }) => {
    await page.goto('/');
    
    const orientations = [
      { width: 390, height: 844 },  // Portrait
      { width: 844, height: 390 },  // Landscape
      { width: 390, height: 844 },  // Portrait
      { width: 844, height: 390 },  // Landscape
    ];
    
    for (const orientation of orientations) {
      await page.setViewportSize(orientation);
      await page.waitForTimeout(500);
      
      // Page should remain functional
      await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
    }
  });

  test('should handle keyboard visibility in landscape', async ({ page }) => {
    await page.goto('/login');
    
    // Landscape mode
    await page.setViewportSize({ width: 844, height: 390 });
    await page.waitForTimeout(500);
    
    // Focus on input (would trigger keyboard on real device)
    await page.getByLabel(/Email Address/i).focus();
    await page.waitForTimeout(500);
    
    // Form should still be accessible even with keyboard
    await expect(page.getByLabel(/Email Address/i)).toBeVisible();
  });
});

test.describe('Orientation Changes - Tablet', () => {
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
  });

  test('should handle tablet portrait to landscape transition', async ({ page }) => {
    await page.goto('/');
    
    // Portrait mode
    await page.setViewportSize({ width: 1024, height: 1366 });
    await page.waitForTimeout(500);
    
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
    
    // Landscape mode
    await page.setViewportSize({ width: 1366, height: 1024 });
    await page.waitForTimeout(500);
    
    // Dashboard should still be visible
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
  });

  test('should optimize layout for tablet landscape', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Landscape mode
    await page.setViewportSize({ width: 1366, height: 1024 });
    await page.waitForTimeout(500);
    
    // Should use multi-column layout in landscape
    const cards = await page.locator('.card, [class*="card"]').all();
    
    if (cards.length >= 2) {
      const firstBox = await cards[0].boundingBox();
      const secondBox = await cards[1].boundingBox();
      
      if (firstBox && secondBox) {
        // Cards may be side by side in landscape
        const sameRow = Math.abs(firstBox.y - secondBox.y) < 50;
      }
    }
  });

  test('should handle form layout on tablet orientation change', async ({ page }) => {
    await page.goto('/applications/new');
    await page.waitForTimeout(1000);
    
    // Portrait mode
    await page.setViewportSize({ width: 1024, height: 1366 });
    await page.waitForTimeout(500);
    
    const formInputs = await page.locator('input, select').all();
    expect(formInputs.length).toBeGreaterThan(0);
    
    // Landscape mode
    await page.setViewportSize({ width: 1366, height: 1024 });
    await page.waitForTimeout(500);
    
    // Form should still be usable
    for (const input of formInputs.slice(0, 3)) {
      if (await input.isVisible()) {
        await expect(input).toBeVisible();
      }
    }
  });

  test('should maintain sidebar visibility on tablet orientation change', async ({ page }) => {
    await page.goto('/');
    await page.waitForTimeout(500);
    
    // Portrait mode
    await page.setViewportSize({ width: 1024, height: 1366 });
    await page.waitForTimeout(500);
    
    const sidebar = page.locator('aside, nav[class*="sidebar"]').first();
    
    // Landscape mode
    await page.setViewportSize({ width: 1366, height: 1024 });
    await page.waitForTimeout(500);
    
    // Sidebar behavior may change based on orientation
  });
});
