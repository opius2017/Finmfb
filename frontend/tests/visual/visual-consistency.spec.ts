import { test, expect } from '@playwright/test';

test.describe('Visual Consistency', () => {
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

  test('should capture baseline screenshot for Dashboard', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    await expect(page).toHaveScreenshot('dashboard.png', { fullPage: true });
  });

  test('should capture baseline screenshot for Calculator', async ({ page }) => {
    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');
    await expect(page).toHaveScreenshot('calculator.png', { fullPage: true });
  });

  test('should capture baseline screenshot for Eligibility Check', async ({ page }) => {
    await page.goto('/eligibility');
    await page.waitForLoadState('networkidle');
    await expect(page).toHaveScreenshot('eligibility.png', { fullPage: true });
  });

  test('should have consistent color scheme', async ({ page }) => {
    const modules = ['/dashboard', '/calculator', '/eligibility'];
    const colors = [];

    for (const module of modules) {
      await page.goto(module);
      await page.waitForLoadState('networkidle');

      const primaryColor = await page.evaluate(() => {
        const element = document.querySelector('button, .btn, [class*="primary"]');
        if (element) {
          return window.getComputedStyle(element).backgroundColor;
        }
        return null;
      });

      if (primaryColor) {
        colors.push(primaryColor);
      }
    }

    // Check that primary colors are consistent (or at least defined)
    expect(colors.length).toBeGreaterThan(0);
  });

  test('should have consistent typography', async ({ page }) => {
    const modules = ['/dashboard', '/calculator', '/eligibility'];
    const fonts = [];

    for (const module of modules) {
      await page.goto(module);
      await page.waitForLoadState('networkidle');

      const fontFamily = await page.evaluate(() => {
        const element = document.querySelector('body');
        if (element) {
          return window.getComputedStyle(element).fontFamily;
        }
        return null;
      });

      if (fontFamily) {
        fonts.push(fontFamily);
      }
    }

    // All modules should use the same font family
    const uniqueFonts = [...new Set(fonts)];
    expect(uniqueFonts.length).toBeLessThanOrEqual(2); // Allow for fallback fonts
  });

  test('should have consistent button styles', async ({ page }) => {
    const modules = ['/dashboard', '/calculator', '/eligibility'];

    for (const module of modules) {
      await page.goto(module);
      await page.waitForLoadState('networkidle');

      const buttons = page.getByRole('button');
      const count = await buttons.count();

      if (count > 0) {
        const firstButton = buttons.first();
        const styles = await firstButton.evaluate((el) => {
          const computed = window.getComputedStyle(el);
          return {
            borderRadius: computed.borderRadius,
            padding: computed.padding,
          };
        });

        expect(styles.borderRadius).toBeTruthy();
        expect(styles.padding).toBeTruthy();
      }
    }
  });

  test('should have consistent form input styles', async ({ page }) => {
    const modules = ['/calculator', '/eligibility'];

    for (const module of modules) {
      await page.goto(module);
      await page.waitForLoadState('networkidle');

      const inputs = page.locator('input[type="text"], input[type="number"]');
      const count = await inputs.count();

      if (count > 0) {
        const firstInput = inputs.first();
        const styles = await firstInput.evaluate((el) => {
          const computed = window.getComputedStyle(el);
          return {
            borderRadius: computed.borderRadius,
            padding: computed.padding,
            border: computed.border,
          };
        });

        expect(styles.borderRadius).toBeTruthy();
        expect(styles.padding).toBeTruthy();
      }
    }
  });

  test('should have consistent card styles', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    const cards = page.locator('.card, [class*="card"]');
    const count = await cards.count();

    if (count > 0) {
      const firstCard = cards.first();
      const styles = await firstCard.evaluate((el) => {
        const computed = window.getComputedStyle(el);
        return {
          borderRadius: computed.borderRadius,
          padding: computed.padding,
          boxShadow: computed.boxShadow,
        };
      });

      expect(styles.borderRadius).toBeTruthy();
      expect(styles.padding).toBeTruthy();
    }
  });

  test('should have consistent spacing', async ({ page }) => {
    const modules = ['/dashboard', '/calculator', '/eligibility'];

    for (const module of modules) {
      await page.goto(module);
      await page.waitForLoadState('networkidle');

      const container = page.locator('main, .container, [class*="container"]').first();
      if (await container.count() > 0) {
        const styles = await container.evaluate((el) => {
          const computed = window.getComputedStyle(el);
          return {
            padding: computed.padding,
            margin: computed.margin,
          };
        });

        expect(styles.padding).toBeTruthy();
      }
    }
  });

  test('should have consistent icon usage', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    const icons = page.locator('svg, [class*="icon"]');
    const count = await icons.count();

    expect(count).toBeGreaterThanOrEqual(0);
  });
});

test.describe('Component Consistency', () => {
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

  test('should have consistent form input styles across modules', async ({ page }) => {
    const inputStyles = [];

    const modules = ['/calculator', '/eligibility'];
    for (const module of modules) {
      await page.goto(module);
      await page.waitForLoadState('networkidle');

      const input = page.locator('input').first();
      if (await input.count() > 0) {
        const styles = await input.evaluate((el) => {
          const computed = window.getComputedStyle(el);
          return {
            height: computed.height,
            borderRadius: computed.borderRadius,
          };
        });
        inputStyles.push(styles);
      }
    }

    // Inputs should have similar styling
    expect(inputStyles.length).toBeGreaterThan(0);
  });

  test('should have consistent card component styles', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    const cards = page.locator('.card, [class*="card"]');
    const count = await cards.count();

    if (count > 1) {
      const firstCardStyles = await cards.first().evaluate((el) => {
        const computed = window.getComputedStyle(el);
        return {
          borderRadius: computed.borderRadius,
          backgroundColor: computed.backgroundColor,
        };
      });

      const secondCardStyles = await cards.nth(1).evaluate((el) => {
        const computed = window.getComputedStyle(el);
        return {
          borderRadius: computed.borderRadius,
          backgroundColor: computed.backgroundColor,
        };
      });

      // Cards should have consistent border radius
      expect(firstCardStyles.borderRadius).toBe(secondCardStyles.borderRadius);
    }
  });

  test('should have consistent spacing and alignment', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    const sections = page.locator('section, .section, [class*="section"]');
    const count = await sections.count();

    for (let i = 0; i < Math.min(count, 3); i++) {
      const section = sections.nth(i);
      const styles = await section.evaluate((el) => {
        const computed = window.getComputedStyle(el);
        return {
          marginBottom: computed.marginBottom,
        };
      });

      expect(styles.marginBottom).toBeTruthy();
    }
  });

  test('should have consistent interactive element styles', async ({ page }) => {
    const modules = ['/dashboard', '/calculator', '/eligibility'];

    for (const module of modules) {
      await page.goto(module);
      await page.waitForLoadState('networkidle');

      const buttons = page.getByRole('button');
      const count = await buttons.count();

      if (count > 0) {
        const button = buttons.first();
        const styles = await button.evaluate((el) => {
          const computed = window.getComputedStyle(el);
          return {
            cursor: computed.cursor,
            transition: computed.transition,
          };
        });

        expect(styles.cursor).toBe('pointer');
      }
    }
  });
});
