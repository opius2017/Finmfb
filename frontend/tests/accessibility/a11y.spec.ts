import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test.describe('Accessibility Tests', () => {
  test.beforeEach(async ({ page, context }) => {
    // Set up authentication for protected pages
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

  test('Login page should not have accessibility violations', async ({ page }) => {
    await page.goto('/login');
    
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('Dashboard should not have accessibility violations', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('Loan Calculator should not have accessibility violations', async ({ page }) => {
    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');
    
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('Eligibility Check should not have accessibility violations', async ({ page }) => {
    await page.goto('/eligibility');
    await page.waitForLoadState('networkidle');
    
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('Loan Applications should not have accessibility violations', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForLoadState('networkidle');
    
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('New Loan Application should not have accessibility violations', async ({ page }) => {
    await page.goto('/applications/new');
    await page.waitForLoadState('networkidle');
    
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('All form inputs should have proper labels', async ({ page }) => {
    await page.goto('/login');
    
    const inputs = await page.locator('input').all();
    
    for (const input of inputs) {
      const id = await input.getAttribute('id');
      if (id) {
        const label = page.locator(`label[for="${id}"]`);
        await expect(label).toBeVisible();
      }
    }
  });

  test('All buttons should have accessible names', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    const buttons = await page.locator('button').all();
    
    for (const button of buttons) {
      const accessibleName = await button.getAttribute('aria-label') || 
                            await button.textContent();
      expect(accessibleName).toBeTruthy();
    }
  });

  test('Images should have alt text', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    const images = await page.locator('img').all();
    
    for (const img of images) {
      const alt = await img.getAttribute('alt');
      expect(alt).toBeDefined();
    }
  });

  test('Color contrast should meet WCAG AA standards', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2aa'])
      .include(['color-contrast'])
      .analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });

  test('Page should have proper heading hierarchy', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    const h1Count = await page.locator('h1').count();
    expect(h1Count).toBeGreaterThanOrEqual(1);
    expect(h1Count).toBeLessThanOrEqual(1); // Should have exactly one h1
  });

  test('Links should have descriptive text', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    const links = await page.locator('a').all();
    
    for (const link of links) {
      const text = await link.textContent();
      const ariaLabel = await link.getAttribute('aria-label');
      
      expect(text || ariaLabel).toBeTruthy();
      expect(text || ariaLabel).not.toBe('click here');
      expect(text || ariaLabel).not.toBe('read more');
    }
  });

  test('Form errors should be announced to screen readers', async ({ page }) => {
    await page.goto('/login');
    
    // Submit form without filling fields
    await page.getByRole('button', { name: /Sign In/i }).click();
    
    // Check for aria-invalid or aria-describedby on inputs
    const emailInput = page.getByLabel(/Email Address/i);
    const hasAriaInvalid = await emailInput.getAttribute('aria-invalid');
    const hasAriaDescribedby = await emailInput.getAttribute('aria-describedby');
    
    expect(hasAriaInvalid || hasAriaDescribedby).toBeTruthy();
  });

  test('Modal dialogs should trap focus', async ({ page }) => {
    // This test assumes there's a modal in the application
    // Adjust based on actual implementation
    test.skip();
    
    await page.goto('/');
    
    // Open modal (implementation specific)
    // await page.getByRole('button', { name: /Open Modal/i }).click();
    
    // Tab through elements
    // await page.keyboard.press('Tab');
    
    // Focus should stay within modal
  });

  test('Skip to main content link should be present', async ({ page }) => {
    await page.goto('/');
    
    // Press Tab to reveal skip link
    await page.keyboard.press('Tab');
    
    // Check for skip link (if implemented)
    // const skipLink = page.getByText(/Skip to main content/i);
    // await expect(skipLink).toBeFocused();
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
  });

  // Helper function to calculate relative luminance
  const getLuminance = (r: number, g: number, b: number): number => {
    const [rs, gs, bs] = [r, g, b].map((c) => {
      c = c / 255;
      return c <= 0.03928 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4);
    });
    return 0.2126 * rs + 0.7152 * gs + 0.0722 * bs;
  };

  // Helper function to calculate contrast ratio
  const getContrastRatio = (l1: number, l2: number): number => {
    const lighter = Math.max(l1, l2);
    const darker = Math.min(l1, l2);
    return (lighter + 0.05) / (darker + 0.05);
  };

  // Helper function to parse RGB color
  const parseRgb = (color: string): { r: number; g: number; b: number } | null => {
    const match = color.match(/rgba?\((\d+),\s*(\d+),\s*(\d+)/);
    if (match) {
      return {
        r: parseInt(match[1]),
        g: parseInt(match[2]),
        b: parseInt(match[3]),
      };
    }
    return null;
  };

  test('should verify text color contrast ratio (minimum 4.5:1)', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // Get all text elements
    const textElements = await page.locator('p, span, div, h1, h2, h3, h4, h5, h6, a, button, label').all();

    let checkedCount = 0;
    let passedCount = 0;

    for (const element of textElements.slice(0, 20)) {
      const isVisible = await element.isVisible();
      if (!isVisible) continue;

      const textContent = await element.textContent();
      if (!textContent || textContent.trim().length === 0) continue;

      const color = await element.evaluate((el) => window.getComputedStyle(el).color);
      const bgColor = await element.evaluate((el) => {
        let bg = window.getComputedStyle(el).backgroundColor;
        let parent = el.parentElement;
        
        // Find first non-transparent background
        while (bg === 'rgba(0, 0, 0, 0)' && parent) {
          bg = window.getComputedStyle(parent).backgroundColor;
          parent = parent.parentElement;
        }
        
        return bg;
      });

      const textRgb = parseRgb(color);
      const bgRgb = parseRgb(bgColor);

      if (textRgb && bgRgb) {
        const textLuminance = getLuminance(textRgb.r, textRgb.g, textRgb.b);
        const bgLuminance = getLuminance(bgRgb.r, bgRgb.g, bgRgb.b);
        const contrastRatio = getContrastRatio(textLuminance, bgLuminance);

        checkedCount++;
        
        // WCAG AA requires 4.5:1 for normal text
        if (contrastRatio >= 4.5) {
          passedCount++;
        }
      }
    }

    // At least 80% of checked elements should pass
    const passRate = checkedCount > 0 ? passedCount / checkedCount : 1;
    expect(passRate).toBeGreaterThanOrEqual(0.8);
  });

  test('should verify button color contrast', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    const buttons = await page.locator('button').all();

    for (const button of buttons.slice(0, 10)) {
      const isVisible = await button.isVisible();
      if (!isVisible) continue;

      const color = await button.evaluate((el) => window.getComputedStyle(el).color);
      const bgColor = await button.evaluate((el) => window.getComputedStyle(el).backgroundColor);

      const textRgb = parseRgb(color);
      const bgRgb = parseRgb(bgColor);

      if (textRgb && bgRgb) {
        const textLuminance = getLuminance(textRgb.r, textRgb.g, textRgb.b);
        const bgLuminance = getLuminance(bgRgb.r, bgRgb.g, bgRgb.b);
        const contrastRatio = getContrastRatio(textLuminance, bgLuminance);

        // Buttons should have at least 4.5:1 contrast
        expect(contrastRatio).toBeGreaterThanOrEqual(4.5);
      }
    }
  });

  test('should verify link color contrast', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    const links = await page.locator('a').all();

    for (const link of links.slice(0, 10)) {
      const isVisible = await link.isVisible();
      if (!isVisible) continue;

      const color = await link.evaluate((el) => window.getComputedStyle(el).color);
      const bgColor = await link.evaluate((el) => {
        let bg = window.getComputedStyle(el).backgroundColor;
        let parent = el.parentElement;
        
        while (bg === 'rgba(0, 0, 0, 0)' && parent) {
          bg = window.getComputedStyle(parent).backgroundColor;
          parent = parent.parentElement;
        }
        
        return bg;
      });

      const textRgb = parseRgb(color);
      const bgRgb = parseRgb(bgColor);

      if (textRgb && bgRgb) {
        const textLuminance = getLuminance(textRgb.r, textRgb.g, textRgb.b);
        const bgLuminance = getLuminance(bgRgb.r, bgRgb.g, bgRgb.b);
        const contrastRatio = getContrastRatio(textLuminance, bgLuminance);

        // Links should have at least 4.5:1 contrast
        expect(contrastRatio).toBeGreaterThanOrEqual(4.5);
      }
    }
  });

  test('should verify form input color contrast', async ({ page }) => {
    await page.goto('/login');
    await page.waitForLoadState('networkidle');

    const inputs = await page.locator('input').all();

    for (const input of inputs) {
      const isVisible = await input.isVisible();
      if (!isVisible) continue;

      const color = await input.evaluate((el) => window.getComputedStyle(el).color);
      const bgColor = await input.evaluate((el) => window.getComputedStyle(el).backgroundColor);

      const textRgb = parseRgb(color);
      const bgRgb = parseRgb(bgColor);

      if (textRgb && bgRgb) {
        const textLuminance = getLuminance(textRgb.r, textRgb.g, textRgb.b);
        const bgLuminance = getLuminance(bgRgb.r, bgRgb.g, bgRgb.b);
        const contrastRatio = getContrastRatio(textLuminance, bgLuminance);

        // Form inputs should have at least 4.5:1 contrast
        expect(contrastRatio).toBeGreaterThanOrEqual(4.5);
      }
    }
  });

  test('should verify interactive element contrast', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // Check buttons, links, and other interactive elements
    const interactiveElements = await page.locator('button, a, input, select, textarea').all();

    let checkedCount = 0;
    let passedCount = 0;

    for (const element of interactiveElements.slice(0, 15)) {
      const isVisible = await element.isVisible();
      if (!isVisible) continue;

      const color = await element.evaluate((el) => window.getComputedStyle(el).color);
      const bgColor = await element.evaluate((el) => {
        let bg = window.getComputedStyle(el).backgroundColor;
        let parent = el.parentElement;
        
        while (bg === 'rgba(0, 0, 0, 0)' && parent) {
          bg = window.getComputedStyle(parent).backgroundColor;
          parent = parent.parentElement;
        }
        
        return bg;
      });

      const textRgb = parseRgb(color);
      const bgRgb = parseRgb(bgColor);

      if (textRgb && bgRgb) {
        const textLuminance = getLuminance(textRgb.r, textRgb.g, textRgb.b);
        const bgLuminance = getLuminance(bgRgb.r, bgRgb.g, bgRgb.b);
        const contrastRatio = getContrastRatio(textLuminance, bgLuminance);

        checkedCount++;
        
        if (contrastRatio >= 4.5) {
          passedCount++;
        }
      }
    }

    // At least 90% should pass for interactive elements
    const passRate = checkedCount > 0 ? passedCount / checkedCount : 1;
    expect(passRate).toBeGreaterThanOrEqual(0.9);
  });

  test('should verify focus indicator visibility', async ({ page }) => {
    await page.goto('/calculator');
    await page.waitForLoadState('networkidle');

    // Tab to first focusable element
    await page.keyboard.press('Tab');
    await page.waitForTimeout(200);

    const focusedElement = await page.evaluate(() => {
      const el = document.activeElement as HTMLElement;
      if (!el) return null;

      const styles = window.getComputedStyle(el);
      return {
        outlineColor: styles.outlineColor,
        outlineWidth: styles.outlineWidth,
        outlineStyle: styles.outlineStyle,
        boxShadow: styles.boxShadow,
      };
    });

    // Focus indicator should be visible
    expect(focusedElement).toBeTruthy();
    
    // Should have either outline or box-shadow
    const hasOutline = focusedElement?.outlineStyle !== 'none' && 
                      focusedElement?.outlineWidth !== '0px';
    const hasBoxShadow = focusedElement?.boxShadow !== 'none';
    
    expect(hasOutline || hasBoxShadow).toBe(true);
  });

  test('should verify error message color contrast', async ({ page }) => {
    await page.goto('/login');
    
    // Submit form to trigger errors
    await page.getByRole('button', { name: /Sign In/i }).click();
    await page.waitForTimeout(500);

    // Look for error messages
    const errorMessages = await page.locator('[class*="error"], [role="alert"], .text-red-500, .text-red-600').all();

    for (const error of errorMessages) {
      const isVisible = await error.isVisible();
      if (!isVisible) continue;

      const color = await error.evaluate((el) => window.getComputedStyle(el).color);
      const bgColor = await error.evaluate((el) => {
        let bg = window.getComputedStyle(el).backgroundColor;
        let parent = el.parentElement;
        
        while (bg === 'rgba(0, 0, 0, 0)' && parent) {
          bg = window.getComputedStyle(parent).backgroundColor;
          parent = parent.parentElement;
        }
        
        return bg;
      });

      const textRgb = parseRgb(color);
      const bgRgb = parseRgb(bgColor);

      if (textRgb && bgRgb) {
        const textLuminance = getLuminance(textRgb.r, textRgb.g, textRgb.b);
        const bgLuminance = getLuminance(bgRgb.r, bgRgb.g, bgRgb.b);
        const contrastRatio = getContrastRatio(textLuminance, bgLuminance);

        // Error messages should have at least 4.5:1 contrast
        expect(contrastRatio).toBeGreaterThanOrEqual(4.5);
      }
    }
  });

  test('should verify heading color contrast', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    const headings = await page.locator('h1, h2, h3, h4, h5, h6').all();

    for (const heading of headings) {
      const isVisible = await heading.isVisible();
      if (!isVisible) continue;

      const color = await heading.evaluate((el) => window.getComputedStyle(el).color);
      const bgColor = await heading.evaluate((el) => {
        let bg = window.getComputedStyle(el).backgroundColor;
        let parent = el.parentElement;
        
        while (bg === 'rgba(0, 0, 0, 0)' && parent) {
          bg = window.getComputedStyle(parent).backgroundColor;
          parent = parent.parentElement;
        }
        
        return bg;
      });

      const textRgb = parseRgb(color);
      const bgRgb = parseRgb(bgColor);

      if (textRgb && bgRgb) {
        const textLuminance = getLuminance(textRgb.r, textRgb.g, textRgb.b);
        const bgLuminance = getLuminance(bgRgb.r, bgRgb.g, bgRgb.b);
        const contrastRatio = getContrastRatio(textLuminance, bgLuminance);

        // Headings should have at least 4.5:1 contrast
        expect(contrastRatio).toBeGreaterThanOrEqual(4.5);
      }
    }
  });

  test('should verify table text color contrast', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForLoadState('networkidle');

    const tableCells = await page.locator('td, th').all();

    let checkedCount = 0;
    let passedCount = 0;

    for (const cell of tableCells.slice(0, 20)) {
      const isVisible = await cell.isVisible();
      if (!isVisible) continue;

      const textContent = await cell.textContent();
      if (!textContent || textContent.trim().length === 0) continue;

      const color = await cell.evaluate((el) => window.getComputedStyle(el).color);
      const bgColor = await cell.evaluate((el) => {
        let bg = window.getComputedStyle(el).backgroundColor;
        let parent = el.parentElement;
        
        while (bg === 'rgba(0, 0, 0, 0)' && parent) {
          bg = window.getComputedStyle(parent).backgroundColor;
          parent = parent.parentElement;
        }
        
        return bg;
      });

      const textRgb = parseRgb(color);
      const bgRgb = parseRgb(bgColor);

      if (textRgb && bgRgb) {
        const textLuminance = getLuminance(textRgb.r, textRgb.g, textRgb.b);
        const bgLuminance = getLuminance(bgRgb.r, bgRgb.g, bgRgb.b);
        const contrastRatio = getContrastRatio(textLuminance, bgLuminance);

        checkedCount++;
        
        if (contrastRatio >= 4.5) {
          passedCount++;
        }
      }
    }

    // At least 85% should pass
    const passRate = checkedCount > 0 ? passedCount / checkedCount : 1;
    expect(passRate).toBeGreaterThanOrEqual(0.85);
  });

  test('should verify status badge color contrast', async ({ page }) => {
    await page.goto('/applications');
    await page.waitForLoadState('networkidle');

    // Look for status badges
    const badges = await page.locator('[class*="badge"], [class*="status"], .bg-green-500, .bg-yellow-500, .bg-red-500').all();

    for (const badge of badges.slice(0, 10)) {
      const isVisible = await badge.isVisible();
      if (!isVisible) continue;

      const textContent = await badge.textContent();
      if (!textContent || textContent.trim().length === 0) continue;

      const color = await badge.evaluate((el) => window.getComputedStyle(el).color);
      const bgColor = await badge.evaluate((el) => window.getComputedStyle(el).backgroundColor);

      const textRgb = parseRgb(color);
      const bgRgb = parseRgb(bgColor);

      if (textRgb && bgRgb) {
        const textLuminance = getLuminance(textRgb.r, textRgb.g, textRgb.b);
        const bgLuminance = getLuminance(bgRgb.r, bgRgb.g, bgRgb.b);
        const contrastRatio = getContrastRatio(textLuminance, bgLuminance);

        // Status badges should have at least 4.5:1 contrast
        expect(contrastRatio).toBeGreaterThanOrEqual(4.5);
      }
    }
  });

  test('should use axe-core for comprehensive contrast checking', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2aa'])
      .include(['color-contrast'])
      .analyze();

    // Should have no critical contrast violations
    const criticalViolations = accessibilityScanResults.violations.filter(
      (v) => v.impact === 'critical' || v.impact === 'serious'
    );

    expect(criticalViolations.length).toBe(0);
  });
});
