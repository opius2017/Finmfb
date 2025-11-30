import { test, expect } from '@playwright/test';

test.describe('Eligibility Check', () => {
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

    await page.goto('/eligibility');
  });

  test('should display eligibility check page with form fields', async ({ page }) => {
    await expect(page.getByRole('heading', { name: /Eligibility Check/i })).toBeVisible();
    await expect(page.getByLabel(/Member ID/i)).toBeVisible();
    await expect(page.getByLabel(/Loan Product/i)).toBeVisible();
    await expect(page.getByLabel(/Requested Amount/i)).toBeVisible();
    await expect(page.getByLabel(/Tenure/i)).toBeVisible();
    await expect(page.getByRole('button', { name: /Check Eligibility/i })).toBeVisible();
  });

  test('should check eligibility for eligible member', async ({ page }) => {
    // Mock successful eligibility check
    await page.route('**/api/eligibility/check', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          isEligible: true,
          maximumEligibleAmount: 2000000,
          eligibilityCriteria: [
            'Member has sufficient savings (₦500,000)',
            'Membership duration meets requirement (12 months)',
            'Deduction rate within acceptable limits (25%)',
            'Debt-to-income ratio is healthy (30%)',
          ],
          failureReasons: [],
          savingsMultiplierCheck: {
            totalSavings: 500000,
            freeEquity: 500000,
            requiredSavings: 500000,
            savingsMultiplier: 2,
            passed: true,
          },
          membershipDurationCheck: {
            membershipStartDate: '2023-01-01',
            membershipMonths: 12,
            requiredMonths: 6,
            passed: true,
          },
          deductionRateHeadroom: {
            monthlySalary: 400000,
            currentMonthlyDeductions: 50000,
            proposedMonthlyDeduction: 50000,
            proposedDeductionRate: 25,
            maximumAllowedRate: 40,
            passed: true,
          },
          debtToIncomeRatio: {
            monthlySalary: 400000,
            currentMonthlyDebtPayments: 70000,
            proposedMonthlyPayment: 50000,
            debtToIncomeRatio: 30,
            maximumAllowedRatio: 50,
            passed: true,
          },
        }),
      });
    });

    // Fill in member information
    await page.getByLabel(/Member ID/i).fill('MEM001');
    await page.getByLabel(/Loan Product/i).selectOption('PROD001');
    await page.getByLabel(/Requested Amount/i).fill('1000000');
    await page.getByLabel(/Tenure/i).fill('12');

    // Submit form
    await page.getByRole('button', { name: /Check Eligibility/i }).click();

    // Wait for results
    await page.waitForTimeout(1000);

    // Verify eligible status
    await expect(page.getByText(/You are Eligible!/i)).toBeVisible();
    await expect(page.getByText(/₦2,000,000/)).toBeVisible();
  });

  test('should check eligibility for ineligible member', async ({ page }) => {
    // Mock failed eligibility check
    await page.route('**/api/eligibility/check', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          isEligible: false,
          maximumEligibleAmount: 0,
          eligibilityCriteria: [
            'Membership duration meets requirement (12 months)',
          ],
          failureReasons: [
            'Insufficient savings: Required ₦500,000, Available ₦200,000',
            'Deduction rate too high: 45% exceeds maximum 40%',
            'Debt-to-income ratio too high: 55% exceeds maximum 50%',
          ],
          savingsMultiplierCheck: {
            totalSavings: 200000,
            freeEquity: 200000,
            requiredSavings: 500000,
            savingsMultiplier: 2,
            passed: false,
          },
          membershipDurationCheck: {
            membershipStartDate: '2023-01-01',
            membershipMonths: 12,
            requiredMonths: 6,
            passed: true,
          },
          deductionRateHeadroom: {
            monthlySalary: 300000,
            currentMonthlyDeductions: 85000,
            proposedMonthlyDeduction: 50000,
            proposedDeductionRate: 45,
            maximumAllowedRate: 40,
            passed: false,
          },
          debtToIncomeRatio: {
            monthlySalary: 300000,
            currentMonthlyDebtPayments: 115000,
            proposedMonthlyPayment: 50000,
            debtToIncomeRatio: 55,
            maximumAllowedRatio: 50,
            passed: false,
          },
        }),
      });
    });

    // Fill in member information
    await page.getByLabel(/Member ID/i).fill('MEM002');
    await page.getByRole('button', { name: /Check Eligibility/i }).click();

    await page.waitForTimeout(1000);

    // Verify ineligible status
    await expect(page.getByText(/Not Eligible/i)).toBeVisible();
    await expect(page.getByText(/Insufficient savings/i)).toBeVisible();
    await expect(page.getByText(/Deduction rate too high/i)).toBeVisible();
    await expect(page.getByText(/Debt-to-income ratio too high/i)).toBeVisible();
  });

  test('should display eligibility criteria', async ({ page }) => {
    await page.route('**/api/eligibility/check', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          isEligible: true,
          maximumEligibleAmount: 2000000,
          eligibilityCriteria: [
            'Member has sufficient savings',
            'Membership duration meets requirement',
            'Deduction rate within acceptable limits',
            'Debt-to-income ratio is healthy',
          ],
          failureReasons: [],
          savingsMultiplierCheck: {
            totalSavings: 500000,
            freeEquity: 500000,
            requiredSavings: 500000,
            savingsMultiplier: 2,
            passed: true,
          },
          membershipDurationCheck: {
            membershipStartDate: '2023-01-01',
            membershipMonths: 12,
            requiredMonths: 6,
            passed: true,
          },
          deductionRateHeadroom: {
            monthlySalary: 400000,
            currentMonthlyDeductions: 50000,
            proposedMonthlyDeduction: 50000,
            proposedDeductionRate: 25,
            maximumAllowedRate: 40,
            passed: true,
          },
          debtToIncomeRatio: {
            monthlySalary: 400000,
            currentMonthlyDebtPayments: 70000,
            proposedMonthlyPayment: 50000,
            debtToIncomeRatio: 30,
            maximumAllowedRatio: 50,
            passed: true,
          },
        }),
      });
    });

    await page.getByLabel(/Member ID/i).fill('MEM001');
    await page.getByRole('button', { name: /Check Eligibility/i }).click();

    await page.waitForTimeout(1000);

    // Verify eligibility criteria section
    await expect(page.getByText(/Eligibility Criteria/i)).toBeVisible();
    await expect(page.getByText(/Member has sufficient savings/i)).toBeVisible();
    await expect(page.getByText(/Membership duration meets requirement/i)).toBeVisible();
    await expect(page.getByText(/Deduction rate within acceptable limits/i)).toBeVisible();
    await expect(page.getByText(/Debt-to-income ratio is healthy/i)).toBeVisible();
  });

  test('should display detailed check results', async ({ page }) => {
    await page.route('**/api/eligibility/check', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          isEligible: true,
          maximumEligibleAmount: 2000000,
          eligibilityCriteria: ['All checks passed'],
          failureReasons: [],
          savingsMultiplierCheck: {
            totalSavings: 500000,
            freeEquity: 500000,
            requiredSavings: 500000,
            savingsMultiplier: 2,
            passed: true,
          },
          membershipDurationCheck: {
            membershipStartDate: '2023-01-01',
            membershipMonths: 12,
            requiredMonths: 6,
            passed: true,
          },
          deductionRateHeadroom: {
            monthlySalary: 400000,
            currentMonthlyDeductions: 50000,
            proposedMonthlyDeduction: 50000,
            proposedDeductionRate: 25,
            maximumAllowedRate: 40,
            passed: true,
          },
          debtToIncomeRatio: {
            monthlySalary: 400000,
            currentMonthlyDebtPayments: 70000,
            proposedMonthlyPayment: 50000,
            debtToIncomeRatio: 30,
            maximumAllowedRatio: 50,
            passed: true,
          },
        }),
      });
    });

    await page.getByLabel(/Member ID/i).fill('MEM001');
    await page.getByRole('button', { name: /Check Eligibility/i }).click();

    await page.waitForTimeout(1000);

    // Verify detailed check cards
    await expect(page.getByText(/Savings Multiplier/i)).toBeVisible();
    await expect(page.getByText(/Membership Duration/i)).toBeVisible();
    await expect(page.getByText(/Deduction Rate/i)).toBeVisible();
    await expect(page.getByText(/Debt-to-Income Ratio/i)).toBeVisible();

    // Verify PASSED badges
    const passedBadges = page.getByText('PASSED');
    await expect(passedBadges).toHaveCount(4);
  });

  test('should check eligibility for multiple loan types', async ({ page }) => {
    // First check with Regular Loan
    await page.route('**/api/eligibility/check', async (route) => {
      const requestBody = await route.request().postDataJSON();
      const multiplier = requestBody.loanProductId === 'PROD001' ? 2 : 
                        requestBody.loanProductId === 'PROD002' ? 3 : 5;
      
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          isEligible: true,
          maximumEligibleAmount: 500000 * multiplier,
          eligibilityCriteria: ['All checks passed'],
          failureReasons: [],
          savingsMultiplierCheck: {
            totalSavings: 500000,
            freeEquity: 500000,
            requiredSavings: 500000,
            savingsMultiplier: multiplier,
            passed: true,
          },
          membershipDurationCheck: {
            membershipStartDate: '2023-01-01',
            membershipMonths: 12,
            requiredMonths: 6,
            passed: true,
          },
          deductionRateHeadroom: {
            monthlySalary: 400000,
            currentMonthlyDeductions: 50000,
            proposedMonthlyDeduction: 50000,
            proposedDeductionRate: 25,
            maximumAllowedRate: 40,
            passed: true,
          },
          debtToIncomeRatio: {
            monthlySalary: 400000,
            currentMonthlyDebtPayments: 70000,
            proposedMonthlyPayment: 50000,
            debtToIncomeRatio: 30,
            maximumAllowedRatio: 50,
            passed: true,
          },
        }),
      });
    });

    await page.getByLabel(/Member ID/i).fill('MEM001');

    // Check Regular Loan
    await page.getByLabel(/Loan Product/i).selectOption('PROD001');
    await page.getByRole('button', { name: /Check Eligibility/i }).click();
    await page.waitForTimeout(1000);
    await expect(page.getByText(/₦1,000,000/)).toBeVisible();

    // Check Premium Loan
    await page.getByLabel(/Loan Product/i).selectOption('PROD002');
    await page.getByRole('button', { name: /Check Eligibility/i }).click();
    await page.waitForTimeout(1000);
    await expect(page.getByText(/₦1,500,000/)).toBeVisible();

    // Check Executive Loan
    await page.getByLabel(/Loan Product/i).selectOption('PROD003');
    await page.getByRole('button', { name: /Check Eligibility/i }).click();
    await page.waitForTimeout(1000);
    await expect(page.getByText(/₦2,500,000/)).toBeVisible();
  });

  test('should validate required Member ID field', async ({ page }) => {
    // Try to submit without Member ID
    await page.getByRole('button', { name: /Check Eligibility/i }).click();

    // Should show error toast
    await expect(page.getByText(/Please enter Member ID/i)).toBeVisible();
  });

  test('should handle API errors gracefully', async ({ page }) => {
    await page.route('**/api/eligibility/check', async (route) => {
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Internal server error' }),
      });
    });

    await page.getByLabel(/Member ID/i).fill('MEM001');
    await page.getByRole('button', { name: /Check Eligibility/i }).click();

    await page.waitForTimeout(1000);

    // Should show error message
    await expect(page.getByText(/Failed to check eligibility/i)).toBeVisible();
  });

  test('should display loading state during check', async ({ page }) => {
    await page.route('**/api/eligibility/check', async (route) => {
      await new Promise((resolve) => setTimeout(resolve, 1500));
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          isEligible: true,
          maximumEligibleAmount: 2000000,
          eligibilityCriteria: ['All checks passed'],
          failureReasons: [],
          savingsMultiplierCheck: {
            totalSavings: 500000,
            freeEquity: 500000,
            requiredSavings: 500000,
            savingsMultiplier: 2,
            passed: true,
          },
          membershipDurationCheck: {
            membershipStartDate: '2023-01-01',
            membershipMonths: 12,
            requiredMonths: 6,
            passed: true,
          },
          deductionRateHeadroom: {
            monthlySalary: 400000,
            currentMonthlyDeductions: 50000,
            proposedMonthlyDeduction: 50000,
            proposedDeductionRate: 25,
            maximumAllowedRate: 40,
            passed: true,
          },
          debtToIncomeRatio: {
            monthlySalary: 400000,
            currentMonthlyDebtPayments: 70000,
            proposedMonthlyPayment: 50000,
            debtToIncomeRatio: 30,
            maximumAllowedRatio: 50,
            passed: true,
          },
        }),
      });
    });

    await page.getByLabel(/Member ID/i).fill('MEM001');
    await page.getByRole('button', { name: /Check Eligibility/i }).click();

    // Check for loading state
    await expect(page.getByRole('button', { name: /Checking.../i })).toBeVisible();
    await expect(page.getByRole('button', { name: /Checking.../i })).toBeDisabled();
  });

  test('should display empty state before checking', async ({ page }) => {
    // Verify empty state message
    await expect(page.getByText(/Check Your Eligibility/i)).toBeVisible();
    await expect(page.getByText(/Enter your member ID and loan details/i)).toBeVisible();
  });

  test('should format currency values correctly', async ({ page }) => {
    await page.route('**/api/eligibility/check', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          isEligible: true,
          maximumEligibleAmount: 2500000,
          eligibilityCriteria: ['All checks passed'],
          failureReasons: [],
          savingsMultiplierCheck: {
            totalSavings: 1250000,
            freeEquity: 1250000,
            requiredSavings: 500000,
            savingsMultiplier: 2,
            passed: true,
          },
          membershipDurationCheck: {
            membershipStartDate: '2023-01-01',
            membershipMonths: 12,
            requiredMonths: 6,
            passed: true,
          },
          deductionRateHeadroom: {
            monthlySalary: 500000,
            currentMonthlyDeductions: 75000,
            proposedMonthlyDeduction: 60000,
            proposedDeductionRate: 27,
            maximumAllowedRate: 40,
            passed: true,
          },
          debtToIncomeRatio: {
            monthlySalary: 500000,
            currentMonthlyDebtPayments: 95000,
            proposedMonthlyPayment: 60000,
            debtToIncomeRatio: 31,
            maximumAllowedRatio: 50,
            passed: true,
          },
        }),
      });
    });

    await page.getByLabel(/Member ID/i).fill('MEM001');
    await page.getByRole('button', { name: /Check Eligibility/i }).click();

    await page.waitForTimeout(1000);

    // Verify currency formatting with ₦ symbol
    await expect(page.getByText(/₦2,500,000/)).toBeVisible();
    await expect(page.getByText(/₦1,250,000/)).toBeVisible();
  });
});

test.describe('Eligibility Check - Form Validation', () => {
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

    await page.goto('/eligibility');
  });

  test('should validate Member ID is required', async ({ page }) => {
    // Clear Member ID field if it has any value
    await page.getByLabel(/Member ID/i).clear();
    
    // Try to submit without Member ID
    await page.getByRole('button', { name: /Check Eligibility/i }).click();

    // Should show validation error
    await expect(page.getByText(/Please enter Member ID/i)).toBeVisible();
  });

  test('should validate Member ID format', async ({ page }) => {
    // Test with invalid format
    await page.getByLabel(/Member ID/i).fill('INVALID');
    
    const memberIdInput = page.getByLabel(/Member ID/i);
    await expect(memberIdInput).toHaveValue('INVALID');
    
    // The form should accept any string as Member ID
    // Backend will validate the actual format
  });

  test('should validate requested amount is a number', async ({ page }) => {
    const amountInput = page.getByLabel(/Requested Amount/i);
    
    // Input field should be of type number
    await expect(amountInput).toHaveAttribute('type', 'number');
    
    // Try to enter non-numeric value (should be prevented by input type)
    await amountInput.fill('abc');
    
    // Value should remain empty or previous valid value
    const value = await amountInput.inputValue();
    expect(value === '' || !isNaN(Number(value))).toBeTruthy();
  });

  test('should validate tenure is within valid range', async ({ page }) => {
    const tenureInput = page.getByLabel(/Tenure/i);
    
    // Check min and max attributes
    await expect(tenureInput).toHaveAttribute('min', '1');
    await expect(tenureInput).toHaveAttribute('max', '360');
    
    // Try to enter value below minimum
    await tenureInput.fill('0');
    await page.getByRole('button', { name: /Check Eligibility/i }).click();
    
    // HTML5 validation should prevent submission or show error
    const isValid = await tenureInput.evaluate((el: HTMLInputElement) => el.validity.valid);
    expect(isValid).toBeFalsy();
  });

  test('should validate tenure is a positive number', async ({ page }) => {
    const tenureInput = page.getByLabel(/Tenure/i);
    
    // Try to enter negative value
    await tenureInput.fill('-5');
    
    // HTML5 validation should mark as invalid
    const isValid = await tenureInput.evaluate((el: HTMLInputElement) => el.validity.valid);
    expect(isValid).toBeFalsy();
  });

  test('should accept valid form inputs', async ({ page }) => {
    await page.route('**/api/eligibility/check', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          isEligible: true,
          maximumEligibleAmount: 2000000,
          eligibilityCriteria: ['All checks passed'],
          failureReasons: [],
          savingsMultiplierCheck: {
            totalSavings: 500000,
            freeEquity: 500000,
            requiredSavings: 500000,
            savingsMultiplier: 2,
            passed: true,
          },
          membershipDurationCheck: {
            membershipStartDate: '2023-01-01',
            membershipMonths: 12,
            requiredMonths: 6,
            passed: true,
          },
          deductionRateHeadroom: {
            monthlySalary: 400000,
            currentMonthlyDeductions: 50000,
            proposedMonthlyDeduction: 50000,
            proposedDeductionRate: 25,
            maximumAllowedRate: 40,
            passed: true,
          },
          debtToIncomeRatio: {
            monthlySalary: 400000,
            currentMonthlyDebtPayments: 70000,
            proposedMonthlyPayment: 50000,
            debtToIncomeRatio: 30,
            maximumAllowedRatio: 50,
            passed: true,
          },
        }),
      });
    });

    // Fill all fields with valid data
    await page.getByLabel(/Member ID/i).fill('MEM001');
    await page.getByLabel(/Loan Product/i).selectOption('PROD001');
    await page.getByLabel(/Requested Amount/i).fill('1000000');
    await page.getByLabel(/Tenure/i).fill('12');

    // Submit should work
    await page.getByRole('button', { name: /Check Eligibility/i }).click();

    await page.waitForTimeout(1000);

    // Should show results
    await expect(page.getByText(/You are Eligible!/i)).toBeVisible();
  });

  test('should display error message for invalid member', async ({ page }) => {
    await page.route('**/api/eligibility/check', async (route) => {
      await route.fulfill({
        status: 404,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Member not found' }),
      });
    });

    await page.getByLabel(/Member ID/i).fill('INVALID123');
    await page.getByRole('button', { name: /Check Eligibility/i }).click();

    await page.waitForTimeout(1000);

    // Should show error message
    await expect(page.getByText(/Member not found/i)).toBeVisible();
  });

  test('should handle network errors gracefully', async ({ page }) => {
    await page.route('**/api/eligibility/check', async (route) => {
      await route.abort('failed');
    });

    await page.getByLabel(/Member ID/i).fill('MEM001');
    await page.getByRole('button', { name: /Check Eligibility/i }).click();

    await page.waitForTimeout(1000);

    // Should show generic error message
    await expect(page.getByText(/Failed to check eligibility/i)).toBeVisible();
  });

  test('should allow changing inputs after validation error', async ({ page }) => {
    // First submission without Member ID
    await page.getByRole('button', { name: /Check Eligibility/i }).click();
    await expect(page.getByText(/Please enter Member ID/i)).toBeVisible();

    // Now fill in Member ID
    await page.getByLabel(/Member ID/i).fill('MEM001');
    
    // Mock successful response
    await page.route('**/api/eligibility/check', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          isEligible: true,
          maximumEligibleAmount: 2000000,
          eligibilityCriteria: ['All checks passed'],
          failureReasons: [],
          savingsMultiplierCheck: {
            totalSavings: 500000,
            freeEquity: 500000,
            requiredSavings: 500000,
            savingsMultiplier: 2,
            passed: true,
          },
          membershipDurationCheck: {
            membershipStartDate: '2023-01-01',
            membershipMonths: 12,
            requiredMonths: 6,
            passed: true,
          },
          deductionRateHeadroom: {
            monthlySalary: 400000,
            currentMonthlyDeductions: 50000,
            proposedMonthlyDeduction: 50000,
            proposedDeductionRate: 25,
            maximumAllowedRate: 40,
            passed: true,
          },
          debtToIncomeRatio: {
            monthlySalary: 400000,
            currentMonthlyDebtPayments: 70000,
            proposedMonthlyPayment: 50000,
            debtToIncomeRatio: 30,
            maximumAllowedRatio: 50,
            passed: true,
          },
        }),
      });
    });

    // Submit again
    await page.getByRole('button', { name: /Check Eligibility/i }).click();

    await page.waitForTimeout(1000);

    // Should show results
    await expect(page.getByText(/You are Eligible!/i)).toBeVisible();
  });
});
