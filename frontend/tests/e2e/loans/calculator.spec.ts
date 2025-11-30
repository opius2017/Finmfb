import { test, expect } from '@playwright/test';

test.describe('Loan Calculator', () => {
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

    await page.goto('/calculator');
  });

  test('should display calculator page with all input fields', async ({ page }) => {
    await expect(page.getByRole('heading', { name: /Loan Calculator/i })).toBeVisible();
    await expect(page.getByLabel(/Principal Amount/i)).toBeVisible();
    await expect(page.getByLabel(/Annual Interest Rate/i)).toBeVisible();
    await expect(page.getByLabel(/Loan Tenure/i)).toBeVisible();
  });

  test('should calculate EMI with valid inputs', async ({ page }) => {
    // Mock API response
    await page.route('**/api/loan-calculator/calculate-emi', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          monthlyEMI: 44244.42,
          totalInterest: 30933.04,
          totalPayment: 530933.04,
        }),
      });
    });

    // Fill in values
    await page.getByLabel(/Principal Amount/i).fill('500000');
    await page.getByLabel(/Annual Interest Rate/i).fill('12');
    await page.getByLabel(/Loan Tenure/i).fill('12');

    // Click calculate
    await page.getByRole('button', { name: /Calculate EMI/i }).click();

    // Wait for results
    await page.waitForTimeout(1000);

    // Check for results display
    await expect(page.getByText(/Monthly EMI/i)).toBeVisible();
    await expect(page.getByText(/44,244/)).toBeVisible();
  });

  test('should validate principal amount is positive', async ({ page }) => {
    await page.getByLabel(/Principal Amount/i).fill('-1000');
    await page.getByRole('button', { name: /Calculate EMI/i }).click();

    // Should show validation error or prevent submission
    const principalInput = page.getByLabel(/Principal Amount/i);
    await expect(principalInput).toHaveAttribute('min', '10000');
  });

  test('should validate interest rate is within range', async ({ page }) => {
    await page.getByLabel(/Annual Interest Rate/i).fill('150');
    await page.getByRole('button', { name: /Calculate EMI/i }).click();

    // Should show validation error
    const rateInput = page.getByLabel(/Annual Interest Rate/i);
    await expect(rateInput).toHaveAttribute('max', '50');
  });

  test('should synchronize slider with input field', async ({ page }) => {
    const principalInput = page.getByLabel(/Principal Amount/i);
    const principalSlider = page.locator('input[type="range"]').first();

    // Change slider
    await principalSlider.fill('1000000');

    // Input should update
    await expect(principalInput).toHaveValue('1000000');
  });

  test('should generate amortization schedule', async ({ page }) => {
    // Mock API response
    await page.route('**/api/loan-calculator/amortization-schedule', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          loanNumber: 'LOAN00000001',
          principal: 500000,
          totalInterest: 30933.04,
          totalPayment: 530933.04,
          installments: Array.from({ length: 12 }, (_, i) => ({
            installmentNumber: i + 1,
            dueDate: new Date(2024, i, 1).toISOString(),
            openingBalance: 500000 - (i * 40000),
            emiAmount: 44244.42,
            principalAmount: 40000,
            interestAmount: 4244.42,
            closingBalance: 500000 - ((i + 1) * 40000),
          })),
        }),
      });
    });

    await page.getByLabel(/Principal Amount/i).fill('500000');
    await page.getByLabel(/Annual Interest Rate/i).fill('12');
    await page.getByLabel(/Loan Tenure/i).fill('12');

    await page.getByRole('button', { name: /Generate Schedule/i }).click();

    await page.waitForTimeout(1000);

    // Check for schedule table
    await expect(page.getByText(/Amortization Schedule/i)).toBeVisible();
    await expect(page.getByRole('table')).toBeVisible();
  });

  test('should display loading state during calculation', async ({ page }) => {
    await page.route('**/api/loan-calculator/calculate-emi', async (route) => {
      await new Promise((resolve) => setTimeout(resolve, 1000));
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          monthlyEMI: 44244.42,
          totalInterest: 30933.04,
          totalPayment: 530933.04,
        }),
      });
    });

    await page.getByLabel(/Principal Amount/i).fill('500000');
    await page.getByRole('button', { name: /Calculate EMI/i }).click();

    // Check for loading state
    await expect(page.getByText(/Calculating/i)).toBeVisible();
  });

  test('should handle calculation errors', async ({ page }) => {
    await page.route('**/api/loan-calculator/calculate-emi', async (route) => {
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Calculation failed' }),
      });
    });

    await page.getByLabel(/Principal Amount/i).fill('500000');
    await page.getByRole('button', { name: /Calculate EMI/i }).click();

    await page.waitForTimeout(1000);

    // Should show error message
    await expect(page.getByText(/Failed to calculate/i)).toBeVisible();
  });

  test('should format currency correctly', async ({ page }) => {
    await page.route('**/api/loan-calculator/calculate-emi', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          monthlyEMI: 44244.42,
          totalInterest: 30933.04,
          totalPayment: 530933.04,
        }),
      });
    });

    await page.getByLabel(/Principal Amount/i).fill('500000');
    await page.getByRole('button', { name: /Calculate EMI/i }).click();

    await page.waitForTimeout(1000);

    // Check for proper currency formatting (₦)
    await expect(page.getByText(/₦/)).toBeVisible();
  });
});
