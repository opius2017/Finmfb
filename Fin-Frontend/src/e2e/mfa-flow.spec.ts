import { test, expect } from '@playwright/test';

test.describe('MFA Authentication Flow', () => {
  test.beforeEach(async ({ page }) => {
    // Go to login page
    await page.goto('/login');
  });

  test('should prompt for MFA and allow successful login with TOTP', async ({ page }) => {
    // Fill in login credentials
    await page.fill('input[id="email"]', 'admin@demo.com');
    await page.fill('input[id="password"]', 'Password123!');
    await page.click('button[type="submit"]');
    
    // Wait for MFA challenge modal to appear
    await page.waitForSelector('div:has-text("Security Verification Required")');
    
    // Verify that MFA challenge prompt is shown
    expect(await page.isVisible('text=Please enter the verification code')).toBeTruthy();
    
    // Get the inputs for the verification code
    const inputs = await page.$$('input[inputmode="numeric"]');
    expect(inputs.length).toBe(6);
    
    // For testing, we would normally use a fixed test code or mock the verification
    // Here we're simulating entering a valid code
    // In a real test environment, you would need to generate a valid TOTP code
    // based on a known shared secret for testing
    const testCode = '123456';
    
    // Fill the code one digit at a time
    for (let i = 0; i < testCode.length; i++) {
      await inputs[i].fill(testCode[i]);
    }
    
    // Mock the API response for MFA verification
    await page.route('**/api/auth/verify-mfa', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ success: true })
      });
    });
    
    // Submit the MFA code
    await page.click('button[type="submit"]');
    
    // Assert successful login (redirect, dashboard, etc.)
    await page.waitForURL('/dashboard');
    expect(await page.isVisible('text=Welcome')).toBeTruthy();
  });
});
