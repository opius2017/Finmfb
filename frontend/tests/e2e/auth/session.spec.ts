import { test, expect } from '@playwright/test';

test.describe('Session Management', () => {
  test('should persist authentication state after page refresh', async ({ page, context }) => {
    // Login first
    await page.goto('/login');
    
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

    await page.getByLabel(/Email Address/i).fill('member@example.com');
    await page.getByLabel(/Password/i).fill('password123');
    await page.getByRole('button', { name: /Sign In/i }).click();
    
    await page.waitForURL('/', { timeout: 3000 });
    
    // Refresh the page
    await page.reload();
    
    // Should still be on dashboard (not redirected to login)
    await expect(page).toHaveURL('/');
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
  });

  test('should store auth token in localStorage after login', async ({ page }) => {
    await page.goto('/login');
    
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

    await page.getByLabel(/Email Address/i).fill('member@example.com');
    await page.getByLabel(/Password/i).fill('password123');
    await page.getByRole('button', { name: /Sign In/i }).click();
    
    await page.waitForURL('/', { timeout: 3000 });
    
    // Check localStorage for token
    const token = await page.evaluate(() => localStorage.getItem('authToken'));
    expect(token).toBe('test-auth-token-123');
    
    // Check for auth storage
    const authStorage = await page.evaluate(() => localStorage.getItem('auth-storage'));
    expect(authStorage).toBeTruthy();
    
    const parsedAuth = JSON.parse(authStorage!);
    expect(parsedAuth.state.isAuthenticated).toBe(true);
    expect(parsedAuth.state.user.email).toBe('member@example.com');
  });

  test('should clear auth token on logout', async ({ page, context }) => {
    // Set up authenticated state
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

    await page.goto('/');
    
    // Find and click logout button (assuming it's in the navigation)
    // This might need adjustment based on actual UI
    await page.getByRole('button', { name: /logout/i }).or(page.getByText(/logout/i)).click();
    
    // Should redirect to login
    await page.waitForURL('/login', { timeout: 3000 });
    
    // Check that token is cleared
    const token = await page.evaluate(() => localStorage.getItem('authToken'));
    expect(token).toBeNull();
    
    // Check that auth storage is cleared
    const authStorage = await page.evaluate(() => localStorage.getItem('auth-storage'));
    if (authStorage) {
      const parsedAuth = JSON.parse(authStorage);
      expect(parsedAuth.state.isAuthenticated).toBe(false);
      expect(parsedAuth.state.user).toBeNull();
    }
  });

  test('should redirect to login on 401 unauthorized response', async ({ page, context }) => {
    // Set up authenticated state
    await context.addInitScript(() => {
      localStorage.setItem('authToken', 'expired-token');
      localStorage.setItem('auth-storage', JSON.stringify({
        state: {
          user: { id: '1', email: 'test@example.com', name: 'Test User', role: 'MEMBER' },
          token: 'expired-token',
          isAuthenticated: true,
        },
        version: 0,
      }));
    });

    // Mock 401 response for any API call
    await page.route('**/api/**', async (route) => {
      await route.fulfill({
        status: 401,
        contentType: 'application/json',
        body: JSON.stringify({
          message: 'Unauthorized',
        }),
      });
    });

    await page.goto('/');
    
    // Make an API call that will return 401
    // This should trigger the redirect to login
    await page.waitForURL('/login', { timeout: 5000 });
    
    // Verify token is cleared
    const token = await page.evaluate(() => localStorage.getItem('authToken'));
    expect(token).toBeNull();
  });

  test('should include auth token in API requests', async ({ page, context }) => {
    // Set up authenticated state
    await context.addInitScript(() => {
      localStorage.setItem('authToken', 'test-auth-token-123');
      localStorage.setItem('auth-storage', JSON.stringify({
        state: {
          user: { id: '1', email: 'test@example.com', name: 'Test User', role: 'MEMBER' },
          token: 'test-auth-token-123',
          isAuthenticated: true,
        },
        version: 0,
      }));
    });

    // Intercept API requests to check for auth header
    let authHeaderFound = false;
    await page.route('**/api/**', async (route) => {
      const headers = route.request().headers();
      if (headers['authorization'] === 'Bearer test-auth-token-123') {
        authHeaderFound = true;
      }
      
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ data: [] }),
      });
    });

    await page.goto('/');
    
    // Wait a bit for any API calls to be made
    await page.waitForTimeout(1000);
    
    // Verify auth header was included
    expect(authHeaderFound).toBe(true);
  });

  test('should handle concurrent sessions across tabs', async ({ browser }) => {
    // Create two contexts (simulating two tabs)
    const context1 = await browser.newContext();
    const context2 = await browser.newContext();
    
    const page1 = await context1.newPage();
    const page2 = await context2.newPage();

    // Login in first tab
    await page1.goto('/login');
    await page1.route('**/api/auth/login', async (route) => {
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

    await page1.getByLabel(/Email Address/i).fill('member@example.com');
    await page1.getByLabel(/Password/i).fill('password123');
    await page1.getByRole('button', { name: /Sign In/i }).click();
    await page1.waitForURL('/', { timeout: 3000 });

    // Second tab should still require login (different context)
    await page2.goto('/login');
    await expect(page2).toHaveURL('/login');

    await context1.close();
    await context2.close();
  });

  test('should maintain session for remember me option', async ({ page }) => {
    await page.goto('/login');
    
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

    // Check remember me
    await page.getByText(/Remember me/i).click();
    
    await page.getByLabel(/Email Address/i).fill('member@example.com');
    await page.getByLabel(/Password/i).fill('password123');
    await page.getByRole('button', { name: /Sign In/i }).click();
    
    await page.waitForURL('/', { timeout: 3000 });
    
    // Token should be stored
    const token = await page.evaluate(() => localStorage.getItem('authToken'));
    expect(token).toBeTruthy();
  });

  test('should handle session expiry gracefully', async ({ page, context }) => {
    // Set up authenticated state with expired token
    await context.addInitScript(() => {
      localStorage.setItem('authToken', 'expired-token');
      localStorage.setItem('auth-storage', JSON.stringify({
        state: {
          user: { id: '1', email: 'test@example.com', name: 'Test User', role: 'MEMBER' },
          token: 'expired-token',
          isAuthenticated: true,
        },
        version: 0,
      }));
    });

    // Mock expired token response
    await page.route('**/api/**', async (route) => {
      await route.fulfill({
        status: 401,
        contentType: 'application/json',
        body: JSON.stringify({
          message: 'Token expired',
        }),
      });
    });

    await page.goto('/');
    
    // Should redirect to login
    await page.waitForURL('/login', { timeout: 5000 });
    
    // Should show appropriate message (if implemented)
    // await expect(page.getByText(/session expired/i)).toBeVisible();
  });

  test('should prevent access to protected routes without authentication', async ({ page }) => {
    // Clear any existing auth
    await page.goto('/login');
    await page.evaluate(() => {
      localStorage.clear();
    });

    // Try to access protected route
    await page.goto('/');
    
    // Should redirect to login
    await page.waitForURL('/login', { timeout: 3000 });
  });

  test('should handle token refresh if implemented', async ({ page, context }) => {
    // This test is for future implementation of token refresh
    // Skip if not implemented
    test.skip();
    
    // Set up authenticated state with token about to expire
    await context.addInitScript(() => {
      localStorage.setItem('authToken', 'about-to-expire-token');
      localStorage.setItem('auth-storage', JSON.stringify({
        state: {
          user: { id: '1', email: 'test@example.com', name: 'Test User', role: 'MEMBER' },
          token: 'about-to-expire-token',
          isAuthenticated: true,
        },
        version: 0,
      }));
    });

    // Mock token refresh endpoint
    await page.route('**/api/auth/refresh', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          token: 'new-refreshed-token',
        }),
      });
    });

    await page.goto('/');
    
    // Wait for token refresh
    await page.waitForTimeout(2000);
    
    // Check for new token
    const token = await page.evaluate(() => localStorage.getItem('authToken'));
    expect(token).toBe('new-refreshed-token');
  });
});
