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
        body: JSON.stringify({
          success: true,
          message: 'MFA verification successful',
          data: {
            userId: 'test-user-id',
            username: 'admin',
            email: 'admin@demo.com',
            token: 'test-token-123',
            roles: ['Admin']
          }
        })
      });
    });
    
    // Wait for verification to complete
    await page.waitForSelector('text=Verification Successful', { timeout: 5000 });
    
    // Verify redirect to dashboard
    await page.waitForURL('**/dashboard');
    
    // Verify user is logged in (e.g. by checking for user menu or profile element)
    expect(await page.isVisible('text=Welcome back')).toBeTruthy();
  });

  test('should allow using backup code when MFA is not available', async ({ page }) => {
    // Fill in login credentials
    await page.fill('input[id="email"]', 'admin@demo.com');
    await page.fill('input[id="password"]', 'Password123!');
    await page.click('button[type="submit"]');
    
    // Wait for MFA challenge modal to appear
    await page.waitForSelector('div:has-text("Security Verification Required")');
    
    // Click on "Use backup code instead" link
    await page.click('text=Use backup code instead');
    
    // Verify backup code form is shown
    expect(await page.isVisible('text=Use Backup Code')).toBeTruthy();
    
    // Enter a backup code
    await page.fill('input[id="backup-code"]', 'BACKUP123CODE456');
    
    // Mock the API response for backup code verification
    await page.route('**/api/auth/verify-backup-code', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          success: true,
          message: 'Backup code verified successfully',
          data: {
            userId: 'test-user-id',
            username: 'admin',
            email: 'admin@demo.com',
            token: 'test-token-123',
            roles: ['Admin']
          }
        })
      });
    });
    
    // Submit the backup code
    await page.click('button:has-text("Verify Backup Code")');
    
    // Verify redirect to dashboard
    await page.waitForURL('**/dashboard');
    
    // Verify user is logged in
    expect(await page.isVisible('text=Welcome back')).toBeTruthy();
  });

  test('should show error for invalid MFA code', async ({ page }) => {
    // Fill in login credentials
    await page.fill('input[id="email"]', 'admin@demo.com');
    await page.fill('input[id="password"]', 'Password123!');
    await page.click('button[type="submit"]');
    
    // Wait for MFA challenge modal to appear
    await page.waitForSelector('div:has-text("Security Verification Required")');
    
    // Get the inputs for the verification code
    const inputs = await page.$$('input[inputmode="numeric"]');
    
    // Enter an invalid code
    const invalidCode = '000000';
    for (let i = 0; i < invalidCode.length; i++) {
      await inputs[i].fill(invalidCode[i]);
    }
    
    // Mock the API response for failed MFA verification
    await page.route('**/api/auth/verify-mfa', async (route) => {
      await route.fulfill({
        status: 400,
        contentType: 'application/json',
        body: JSON.stringify({
          success: false,
          message: 'Invalid verification code',
        })
      });
    });
    
    // Wait for error notification
    await page.waitForSelector('text=Invalid verification code', { timeout: 5000 });
    
    // Verify user is still on login page with MFA challenge
    expect(await page.isVisible('div:has-text("Security Verification Required")')).toBeTruthy();
  });
});