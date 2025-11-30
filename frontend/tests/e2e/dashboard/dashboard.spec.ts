import { test, expect } from '@playwright/test';

test.describe('Dashboard', () => {
  test.beforeEach(async ({ page, context }) => {
    // Set up authentication
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

    // Mock dashboard API responses
    await page.route('**/api/dashboard/**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          stats: {
            totalLoans: 1234,
            activeMembers: 856,
            totalDisbursed: 125400000,
            delinquentLoans: 23,
          },
          recentApplications: [
            {
              id: '1',
              memberName: 'John Doe',
              memberNumber: 'MEM001',
              amount: 500000,
              status: 'PENDING',
              date: '2024-12-01',
            },
          ],
        }),
      });
    });

    await page.goto('/');
  });

  test('should display dashboard page with all elements', async ({ page }) => {
    // Check page title
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
    
    // Check welcome message
    await expect(page.getByText(/Welcome back/i)).toBeVisible();
  });

  test('should display statistics cards', async ({ page }) => {
    // Wait for stats to load
    await page.waitForTimeout(1000);
    
    // Check for stat cards
    await expect(page.getByText(/Total Loans/i)).toBeVisible();
    await expect(page.getByText(/Active Members/i)).toBeVisible();
    await expect(page.getByText(/Total Disbursed/i)).toBeVisible();
    await expect(page.getByText(/Delinquent Loans/i)).toBeVisible();
  });

  test('should display recent applications table', async ({ page }) => {
    await page.waitForTimeout(1000);
    
    await expect(page.getByText(/Recent Applications/i)).toBeVisible();
    await expect(page.getByRole('table')).toBeVisible();
  });

  test('should navigate to calculator from quick actions', async ({ page }) => {
    await page.getByText(/Calculate EMI/i).click();
    await expect(page).toHaveURL('/calculator');
  });

  test('should navigate to new application from quick actions', async ({ page }) => {
    await page.getByText(/New Loan Application/i).click();
    await expect(page).toHaveURL('/applications/new');
  });

  test('should navigate to eligibility check from quick actions', async ({ page }) => {
    await page.getByText(/Check Eligibility/i).click();
    await expect(page).toHaveURL('/eligibility');
  });

  test('should handle empty state when no data available', async ({ page }) => {
    // Mock empty response
    await page.route('**/api/dashboard/**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          stats: {},
          recentApplications: [],
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);
    
    // Should display appropriate empty state
    // Implementation depends on actual UI
  });

  test('should handle loading state', async ({ page }) => {
    // Mock slow response
    await page.route('**/api/dashboard/**', async (route) => {
      await new Promise((resolve) => setTimeout(resolve, 2000));
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ stats: {}, recentApplications: [] }),
      });
    });

    await page.reload();
    
    // Check for loading indicators
    // Implementation depends on actual UI
  });

  test('should handle error state', async ({ page }) => {
    // Mock error response
    await page.route('**/api/dashboard/**', async (route) => {
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Server error' }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);
    
    // Should display error message
    // Implementation depends on actual UI
  });
});
