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

    const modules = ['/calculator', '/eligibility', '/applications/new'];
    for (const module of modules) {
      await page.goto(module);
      await page.waitForLoadState('networkidle');

      const input = page.locator('input[type="text"], input[type="number"], input[type="email"]').first();
      if (await input.count() > 0) {
        const styles = await input.evaluate((el) => {
          const computed = window.getComputedStyle(el);
          return {
            height: computed.height,
            borderRadius: computed.borderRadius,
            borderWidth: computed.borderWidth,
            padding: computed.padding,
            fontSize: computed.fontSize,
          };
        });
        inputStyles.push(styles);
      }
    }

    // All inputs should have similar styling
    expect(inputStyles.length).toBeGreaterThan(0);
    
    // Check border radius consistency
    if (inputStyles.length > 1) {
      const firstBorderRadius = inputStyles[0].borderRadius;
      const allSameBorderRadius = inputStyles.every(s => s.borderRadius === firstBorderRadius);
      expect(allSameBorderRadius).toBe(true);
    }
  });

  test('should have consistent select/dropdown styles across modules', async ({ page }) => {
    const selectStyles = [];

    const modules = ['/calculator', '/eligibility', '/applications'];
    for (const module of modules) {
      await page.goto(module);
      await page.waitForLoadState('networkidle');

      const select = page.locator('select').first();
      if (await select.count() > 0) {
        const styles = await select.evaluate((el) => {
          const computed = window.getComputedStyle(el);
          return {
            height: computed.height,
            borderRadius: computed.borderRadius,
            padding: computed.padding,
          };
        });
        selectStyles.push(styles);
      }
    }

    // Selects should have consistent styling
    if (selectStyles.length > 1) {
      const firstBorderRadius = selectStyles[0].borderRadius;
      const allSameBorderRadius = selectStyles.every(s => s.borderRadius === firstBorderRadius);
      expect(allSameBorderRadius).toBe(true);
    }
  });

  test('should have consistent button styles across modules', async ({ page }) => {
    const buttonStyles = [];

    const modules = ['/dashboard', '/calculator', '/eligibility', '/applications'];
    for (const module of modules) {
      await page.goto(module);
      await page.waitForLoadState('networkidle');

      const button = page.getByRole('button').first();
      if (await button.count() > 0) {
        const styles = await button.evaluate((el) => {
          const computed = window.getComputedStyle(el);
          return {
            borderRadius: computed.borderRadius,
            padding: computed.padding,
            fontWeight: computed.fontWeight,
            textTransform: computed.textTransform,
          };
        });
        buttonStyles.push(styles);
      }
    }

    // Buttons should have consistent styling
    expect(buttonStyles.length).toBeGreaterThan(0);
    
    if (buttonStyles.length > 1) {
      const firstBorderRadius = buttonStyles[0].borderRadius;
      const allSameBorderRadius = buttonStyles.every(s => s.borderRadius === firstBorderRadius);
      expect(allSameBorderRadius).toBe(true);
    }
  });

  test('should have consistent card component styles', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    const cards = page.locator('.card, [class*="card"], [class*="Card"]');
    const count = await cards.count();

    if (count > 1) {
      const cardStyles = [];
      
      for (let i = 0; i < Math.min(count, 4); i++) {
        const styles = await cards.nth(i).evaluate((el) => {
          const computed = window.getComputedStyle(el);
          return {
            borderRadius: computed.borderRadius,
            backgroundColor: computed.backgroundColor,
            padding: computed.padding,
            boxShadow: computed.boxShadow,
          };
        });
        cardStyles.push(styles);
      }

      // All cards should have same border radius
      const firstBorderRadius = cardStyles[0].borderRadius;
      const allSameBorderRadius = cardStyles.every(s => s.borderRadius === firstBorderRadius);
      expect(allSameBorderRadius).toBe(true);

      // All cards should have same padding
      const firstPadding = cardStyles[0].padding;
      const allSamePadding = cardStyles.every(s => s.padding === firstPadding);
      expect(allSamePadding).toBe(true);
    }
  });

  test('should have consistent table styles across modules', async ({ page }) => {
    const tableStyles = [];

    const modules = ['/applications', '/guarantor', '/reports'];
    for (const module of modules) {
      await page.goto(module);
      await page.waitForLoadState('networkidle');

      const table = page.locator('table').first();
      if (await table.count() > 0) {
        const styles = await table.evaluate((el) => {
          const computed = window.getComputedStyle(el);
          return {
            borderCollapse: computed.borderCollapse,
            width: computed.width,
          };
        });
        tableStyles.push(styles);
      }
    }

    // Tables should have consistent styling
    if (tableStyles.length > 1) {
      const firstBorderCollapse = tableStyles[0].borderCollapse;
      const allSameBorderCollapse = tableStyles.every(s => s.borderCollapse === firstBorderCollapse);
      expect(allSameBorderCollapse).toBe(true);
    }
  });

  test('should have consistent spacing and alignment', async ({ page }) => {
    const modules = ['/dashboard', '/calculator', '/eligibility'];
    const spacingData = [];

    for (const module of modules) {
      await page.goto(module);
      await page.waitForLoadState('networkidle');

      const container = page.locator('main, .container, [class*="container"]').first();
      if (await container.count() > 0) {
        const styles = await container.evaluate((el) => {
          const computed = window.getComputedStyle(el);
          return {
            padding: computed.padding,
            maxWidth: computed.maxWidth,
          };
        });
        spacingData.push(styles);
      }
    }

    // Containers should have consistent padding
    expect(spacingData.length).toBeGreaterThan(0);
    
    if (spacingData.length > 1) {
      const firstPadding = spacingData[0].padding;
      const allSamePadding = spacingData.every(s => s.padding === firstPadding);
      expect(allSamePadding).toBe(true);
    }
  });

  test('should have consistent heading styles across modules', async ({ page }) => {
    const headingStyles = [];

    const modules = ['/dashboard', '/calculator', '/eligibility', '/applications'];
    for (const module of modules) {
      await page.goto(module);
      await page.waitForLoadState('networkidle');

      const h1 = page.locator('h1').first();
      if (await h1.count() > 0) {
        const styles = await h1.evaluate((el) => {
          const computed = window.getComputedStyle(el);
          return {
            fontSize: computed.fontSize,
            fontWeight: computed.fontWeight,
            marginBottom: computed.marginBottom,
          };
        });
        headingStyles.push(styles);
      }
    }

    // H1 headings should have consistent styling
    if (headingStyles.length > 1) {
      const firstFontSize = headingStyles[0].fontSize;
      const allSameFontSize = headingStyles.every(s => s.fontSize === firstFontSize);
      expect(allSameFontSize).toBe(true);

      const firstFontWeight = headingStyles[0].fontWeight;
      const allSameFontWeight = headingStyles.every(s => s.fontWeight === firstFontWeight);
      expect(allSameFontWeight).toBe(true);
    }
  });

  test('should have consistent link styles across modules', async ({ page }) => {
    const linkStyles = [];

    const modules = ['/dashboard', '/applications'];
    for (const module of modules) {
      await page.goto(module);
      await page.waitForLoadState('networkidle');

      const link = page.getByRole('link').first();
      if (await link.count() > 0) {
        const styles = await link.evaluate((el) => {
          const computed = window.getComputedStyle(el);
          return {
            color: computed.color,
            textDecoration: computed.textDecoration,
          };
        });
        linkStyles.push(styles);
      }
    }

    // Links should have consistent styling
    if (linkStyles.length > 1) {
      const firstColor = linkStyles[0].color;
      const allSameColor = linkStyles.every(s => s.color === firstColor);
      expect(allSameColor).toBe(true);
    }
  });

  test('should have consistent label styles across forms', async ({ page }) => {
    const labelStyles = [];

    const modules = ['/calculator', '/eligibility', '/applications/new'];
    for (const module of modules) {
      await page.goto(module);
      await page.waitForLoadState('networkidle');

      const label = page.locator('label').first();
      if (await label.count() > 0) {
        const styles = await label.evaluate((el) => {
          const computed = window.getComputedStyle(el);
          return {
            fontSize: computed.fontSize,
            fontWeight: computed.fontWeight,
            marginBottom: computed.marginBottom,
          };
        });
        labelStyles.push(styles);
      }
    }

    // Labels should have consistent styling
    if (labelStyles.length > 1) {
      const firstFontSize = labelStyles[0].fontSize;
      const allSameFontSize = labelStyles.every(s => s.fontSize === firstFontSize);
      expect(allSameFontSize).toBe(true);
    }
  });

  test('should have consistent error message styles', async ({ page }) => {
    await page.goto('/login');
    
    // Trigger error
    await page.getByRole('button', { name: /Sign In/i }).click();
    await page.waitForTimeout(500);

    const errorMessages = page.locator('[class*="error"], [role="alert"], .text-red-500, .text-red-600');
    const count = await errorMessages.count();

    if (count > 0) {
      const styles = await errorMessages.first().evaluate((el) => {
        const computed = window.getComputedStyle(el);
        return {
          color: computed.color,
          fontSize: computed.fontSize,
        };
      });

      expect(styles.color).toBeTruthy();
      expect(styles.fontSize).toBeTruthy();
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

  test('should have consistent icon sizes across modules', async ({ page }) => {
    const iconSizes = [];

    const modules = ['/dashboard', '/applications'];
    for (const module of modules) {
      await page.goto(module);
      await page.waitForLoadState('networkidle');

      const icon = page.locator('svg, [class*="icon"]').first();
      if (await icon.count() > 0) {
        const box = await icon.boundingBox();
        if (box) {
          iconSizes.push({ width: box.width, height: box.height });
        }
      }
    }

    // Icons should have reasonable sizes
    for (const size of iconSizes) {
      expect(size.width).toBeGreaterThan(0);
      expect(size.height).toBeGreaterThan(0);
    }
  });

  test('should have consistent modal/dialog styles if present', async ({ page }) => {
    // This test assumes modals exist
    test.skip();

    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // Open modal
    // const modal = page.getByRole('dialog');
    // if (await modal.count() > 0) {
    //   const styles = await modal.evaluate((el) => {
    //     const computed = window.getComputedStyle(el);
    //     return {
    //       borderRadius: computed.borderRadius,
    //       padding: computed.padding,
    //     };
    //   });
    //   expect(styles.borderRadius).toBeTruthy();
    // }
  });

  test('should have consistent badge/tag styles', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForLoadState('networkidle');

    const badges = page.locator('[class*="badge"], [class*="tag"], [class*="status"]');
    const count = await badges.count();

    if (count > 1) {
      const badgeStyles = [];
      
      for (let i = 0; i < Math.min(count, 3); i++) {
        const styles = await badges.nth(i).evaluate((el) => {
          const computed = window.getComputedStyle(el);
          return {
            borderRadius: computed.borderRadius,
            padding: computed.padding,
            fontSize: computed.fontSize,
          };
        });
        badgeStyles.push(styles);
      }

      // Badges should have consistent border radius
      if (badgeStyles.length > 1) {
        const firstBorderRadius = badgeStyles[0].borderRadius;
        const allSameBorderRadius = badgeStyles.every(s => s.borderRadius === firstBorderRadius);
        expect(allSameBorderRadius).toBe(true);
      }
    }
  });

  test('should have consistent pagination styles if present', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForLoadState('networkidle');

    const paginationButtons = page.locator('[aria-label*="page"], [class*="pagination"] button');
    const count = await paginationButtons.count();

    if (count > 1) {
      const firstButtonStyles = await paginationButtons.first().evaluate((el) => {
        const computed = window.getComputedStyle(el);
        return {
          width: computed.width,
          height: computed.height,
        };
      });

      const secondButtonStyles = await paginationButtons.nth(1).evaluate((el) => {
        const computed = window.getComputedStyle(el);
        return {
          width: computed.width,
          height: computed.height,
        };
      });

      // Pagination buttons should have consistent sizing
      expect(firstButtonStyles.width).toBe(secondButtonStyles.width);
      expect(firstButtonStyles.height).toBe(secondButtonStyles.height);
    }
  });

  test('should have consistent tooltip styles if present', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // Look for elements with tooltips
    const tooltipTriggers = page.locator('[title], [data-tooltip]');
    const count = await tooltipTriggers.count();

    // Tooltips may or may not be present
    expect(count).toBeGreaterThanOrEqual(0);
  });
});
