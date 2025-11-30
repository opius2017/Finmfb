import { test, expect } from '@playwright/test';

test.describe('Login Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
  });

  test('should display login form with all elements', async ({ page }) => {
    // Check page title
    await expect(page).toHaveTitle(/Cooperative Loan Management/);
    
    // Check form elements
    await expect(page.getByRole('heading', { name: /Cooperative Loan Management/i })).toBeVisible();
    await expect(page.getByText(/Sign in to your account/i)).toBeVisible();
    
    // Check input fields
    await expect(page.getByLabel(/Email Address/i)).toBeVisible();
    await expect(page.getByLabel(/Password/i)).toBeVisible();
    
    // Check buttons
    await expect(page.getByRole('button', { name: /Sign In/i })).toBeVisible();
    
    // Check remember me checkbox
    await expect(page.getByText(/Remember me/i)).toBeVisible();
    
    // Check forgot password link
    await expect(page.getByText(/Forgot password/i)).toBeVisible();
    
    // Check demo credentials section
    await expect(page.getByText(/Demo Credentials/i)).toBeVisible();
  });

  test('should successfully login with valid credentials', async ({ page }) => {
    // Mock API response
    await page.route('**/api/auth/login', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          user: {
            id: '1',
            email: 'member@example.com',
            name: 'Test Member',
            role: 'MEMBER',
          },
          token: 'test-auth-token-123',
        }),
      });
    });

    // Fill in login form
    await page.getByLabel(/Email Address/i).fill('member@example.com');
    await page.getByLabel(/Password/i).fill('password123');
    
    // Submit form
    await page.getByRole('button', { name: /Sign In/i }).click();
    
    // Wait for navigation to dashboard
    await page.waitForURL('/', { timeout: 3000 });
    
    // Verify we're on the dashboard
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
    
    // Verify success toast
    await expect(page.getByText(/Login successful/i)).toBeVisible();
  });

  test('should show error message with invalid credentials', async ({ page }) => {
    // Mock API error response
    await page.route('**/api/auth/login', async (route) => {
      await route.fulfill({
        status: 401,
        contentType: 'application/json',
        body: JSON.stringify({
          message: 'Invalid credentials',
        }),
      });
    });

    // Fill in login form with invalid credentials
    await page.getByLabel(/Email Address/i).fill('invalid@example.com');
    await page.getByLabel(/Password/i).fill('wrongpassword');
    
    // Submit form
    await page.getByRole('button', { name: /Sign In/i }).click();
    
    // Wait for error message
    await expect(page.getByText(/Invalid credentials/i)).toBeVisible({ timeout: 3000 });
    
    // Verify we're still on login page
    await expect(page).toHaveURL(/\/login/);
  });

  test('should validate empty email field', async ({ page }) => {
    // Try to submit with empty email
    await page.getByLabel(/Password/i).fill('password123');
    await page.getByRole('button', { name: /Sign In/i }).click();
    
    // Check for HTML5 validation or custom error
    const emailInput = page.getByLabel(/Email Address/i);
    await expect(emailInput).toHaveAttribute('required');
  });

  test('should validate empty password field', async ({ page }) => {
    // Try to submit with empty password
    await page.getByLabel(/Email Address/i).fill('test@example.com');
    await page.getByRole('button', { name: /Sign In/i }).click();
    
    // Check for HTML5 validation or custom error
    const passwordInput = page.getByLabel(/Password/i);
    await expect(passwordInput).toHaveAttribute('required');
  });

  test('should toggle password visibility', async ({ page }) => {
    const passwordInput = page.getByLabel(/Password/i);
    const toggleButton = page.locator('button').filter({ has: page.locator('svg') }).nth(1);
    
    // Initially password should be hidden
    await expect(passwordInput).toHaveAttribute('type', 'password');
    
    // Click toggle button
    await toggleButton.click();
    
    // Password should now be visible
    await expect(passwordInput).toHaveAttribute('type', 'text');
    
    // Click again to hide
    await toggleButton.click();
    
    // Password should be hidden again
    await expect(passwordInput).toHaveAttribute('type', 'password');
  });

  test('should show loading state during login', async ({ page }) => {
    // Mock slow API response
    await page.route('**/api/auth/login', async (route) => {
      await new Promise((resolve) => setTimeout(resolve, 1000));
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          user: {
            id: '1',
            email: 'member@example.com',
            name: 'Test Member',
            role: 'MEMBER',
          },
          token: 'test-auth-token-123',
        }),
      });
    });

    // Fill and submit form
    await page.getByLabel(/Email Address/i).fill('member@example.com');
    await page.getByLabel(/Password/i).fill('password123');
    await page.getByRole('button', { name: /Sign In/i }).click();
    
    // Check for loading state
    await expect(page.getByText(/Signing in/i)).toBeVisible();
    
    // Button should be disabled during loading
    await expect(page.getByRole('button', { name: /Signing in/i })).toBeDisabled();
  });

  test('should autofocus email input on page load', async ({ page }) => {
    const emailInput = page.getByLabel(/Email Address/i);
    await expect(emailInput).toBeFocused();
  });

  test('should have proper form accessibility', async ({ page }) => {
    // Check for proper labels
    const emailInput = page.getByLabel(/Email Address/i);
    const passwordInput = page.getByLabel(/Password/i);
    
    await expect(emailInput).toHaveAttribute('type', 'email');
    await expect(passwordInput).toHaveAttribute('type', 'password');
    
    // Check for placeholders
    await expect(emailInput).toHaveAttribute('placeholder');
    await expect(passwordInput).toHaveAttribute('placeholder');
  });

  test('should handle network errors gracefully', async ({ page }) => {
    // Mock network error
    await page.route('**/api/auth/login', async (route) => {
      await route.abort('failed');
    });

    // Fill and submit form
    await page.getByLabel(/Email Address/i).fill('member@example.com');
    await page.getByLabel(/Password/i).fill('password123');
    await page.getByRole('button', { name: /Sign In/i }).click();
    
    // Should show error message
    await expect(page.getByText(/failed/i).or(page.getByText(/error/i))).toBeVisible({ timeout: 3000 });
  });

  test('should redirect authenticated users away from login', async ({ page, context }) => {
    // Set authentication token in localStorage
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

    // Try to visit login page
    await page.goto('/login');
    
    // Should redirect to dashboard
    await page.waitForURL('/', { timeout: 3000 });
  });

  test('should submit form on Enter key press', async ({ page }) => {
    // Mock API response
    await page.route('**/api/auth/login', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          user: {
            id: '1',
            email: 'member@example.com',
            name: 'Test Member',
            role: 'MEMBER',
          },
          token: 'test-auth-token-123',
        }),
      });
    });

    // Fill in form
    await page.getByLabel(/Email Address/i).fill('member@example.com');
    await page.getByLabel(/Password/i).fill('password123');
    
    // Press Enter
    await page.getByLabel(/Password/i).press('Enter');
    
    // Should navigate to dashboard
    await page.waitForURL('/', { timeout: 3000 });
  });
});
