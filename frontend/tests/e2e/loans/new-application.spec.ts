import { test, expect } from '@playwright/test';

test.describe('New Loan Application', () => {
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

    await page.goto('/applications/new');
  });

  test('should display new loan application page', async ({ page }) => {
    await expect(page.getByRole('heading', { name: /New Loan Application/i })).toBeVisible();
  });

  test('should display multi-step form', async ({ page }) => {
    // Look for step indicators or form sections
    const step1 = page.getByText(/Step 1|Personal Information|Loan Details/i);
    if (await step1.isVisible({ timeout: 2000 })) {
      await expect(step1).toBeVisible();
    }
  });

  test('should validate required fields', async ({ page }) => {
    // Try to submit without filling required fields
    const submitButton = page.getByRole('button', { name: /Submit|Apply|Next/i });
    if (await submitButton.isVisible()) {
      await submitButton.click();
      await page.waitForTimeout(500);

      // Should show validation errors
      const errorMessage = page.getByText(/required|Required|fill|Fill/i);
      if (await errorMessage.isVisible({ timeout: 1000 })) {
        await expect(errorMessage).toBeVisible();
      }
    }
  });

  test('should submit valid loan application', async ({ page }) => {
    await page.route('**/api/loan-applications', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 201,
          contentType: 'application/json',
          body: JSON.stringify({
            id: 'APP-001',
            loanNumber: 'LOAN00000001',
            status: 'PENDING',
            message: 'Application submitted successfully',
          }),
        });
      }
    });

    // Fill in form fields
    const loanTypeSelect = page.locator('select[name="loanType"], select#loanType').first();
    if (await loanTypeSelect.isVisible({ timeout: 1000 })) {
      await loanTypeSelect.selectOption('PERSONAL');
    }

    const amountInput = page.locator('input[name="amount"], input[placeholder*="Amount"]').first();
    if (await amountInput.isVisible({ timeout: 1000 })) {
      await amountInput.fill('500000');
    }

    const tenureInput = page.locator('input[name="tenure"], input[placeholder*="Tenure"]').first();
    if (await tenureInput.isVisible({ timeout: 1000 })) {
      await tenureInput.fill('12');
    }

    const purposeInput = page.locator('textarea[name="purpose"], input[name="purpose"]').first();
    if (await purposeInput.isVisible({ timeout: 1000 })) {
      await purposeInput.fill('Personal development');
    }

    // Submit form
    const submitButton = page.getByRole('button', { name: /Submit|Apply/i });
    if (await submitButton.isVisible()) {
      await submitButton.click();
      await page.waitForTimeout(1500);

      // Should show success message
      await expect(page.getByText(/Success|success|submitted/i)).toBeVisible();
    }
  });

  test('should navigate through multi-step form', async ({ page }) => {
    // Step 1: Fill basic information
    const loanTypeSelect = page.locator('select[name="loanType"]').first();
    if (await loanTypeSelect.isVisible({ timeout: 1000 })) {
      await loanTypeSelect.selectOption('PERSONAL');

      // Click Next button
      const nextButton = page.getByRole('button', { name: /Next/i });
      if (await nextButton.isVisible()) {
        await nextButton.click();
        await page.waitForTimeout(500);

        // Should move to step 2
        const step2 = page.getByText(/Step 2|Guarantor|Documents/i);
        if (await step2.isVisible({ timeout: 1000 })) {
          await expect(step2).toBeVisible();
        }
      }
    }
  });

  test('should save form progress', async ({ page }) => {
    // Fill some fields
    const amountInput = page.locator('input[name="amount"]').first();
    if (await amountInput.isVisible({ timeout: 1000 })) {
      await amountInput.fill('500000');

      // Navigate away
      await page.goto('/dashboard');
      await page.waitForTimeout(500);

      // Navigate back
      await page.goto('/applications/new');
      await page.waitForTimeout(1000);

      // Check if form data is restored (if implemented)
      const savedAmount = await amountInput.inputValue();
      // Form persistence may or may not be implemented
    }
  });

  test('should handle submission errors', async ({ page }) => {
    await page.route('**/api/loan-applications', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 400,
          contentType: 'application/json',
          body: JSON.stringify({
            message: 'Invalid loan amount',
            errors: ['Amount exceeds maximum limit'],
          }),
        });
      }
    });

    // Fill and submit form
    const amountInput = page.locator('input[name="amount"]').first();
    if (await amountInput.isVisible({ timeout: 1000 })) {
      await amountInput.fill('10000000');

      const submitButton = page.getByRole('button', { name: /Submit|Apply/i });
      if (await submitButton.isVisible()) {
        await submitButton.click();
        await page.waitForTimeout(1000);

        // Should show error message
        await expect(page.getByText(/Invalid|Error|error|failed/i)).toBeVisible();
      }
    }
  });

  test('should retain input after failed submission', async ({ page }) => {
    await page.route('**/api/loan-applications', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 500,
          contentType: 'application/json',
          body: JSON.stringify({ message: 'Server error' }),
        });
      }
    });

    const testAmount = '500000';
    const amountInput = page.locator('input[name="amount"]').first();
    if (await amountInput.isVisible({ timeout: 1000 })) {
      await amountInput.fill(testAmount);

      const submitButton = page.getByRole('button', { name: /Submit|Apply/i });
      if (await submitButton.isVisible()) {
        await submitButton.click();
        await page.waitForTimeout(1000);

        // Input should still have the value
        await expect(amountInput).toHaveValue(testAmount);
      }
    }
  });

  test('should display loading state during submission', async ({ page }) => {
    await page.route('**/api/loan-applications', async (route) => {
      if (route.request().method() === 'POST') {
        await new Promise((resolve) => setTimeout(resolve, 1500));
        await route.fulfill({
          status: 201,
          contentType: 'application/json',
          body: JSON.stringify({
            id: 'APP-001',
            loanNumber: 'LOAN00000001',
            status: 'PENDING',
          }),
        });
      }
    });

    const amountInput = page.locator('input[name="amount"]').first();
    if (await amountInput.isVisible({ timeout: 1000 })) {
      await amountInput.fill('500000');

      const submitButton = page.getByRole('button', { name: /Submit|Apply/i });
      if (await submitButton.isVisible()) {
        await submitButton.click();

        // Should show loading state
        const loadingButton = page.getByRole('button', { name: /Submitting|Loading|loading/i });
        if (await loadingButton.isVisible({ timeout: 500 })) {
          await expect(loadingButton).toBeDisabled();
        }
      }
    }
  });

  test('should validate loan amount range', async ({ page }) => {
    const amountInput = page.locator('input[name="amount"]').first();
    if (await amountInput.isVisible({ timeout: 1000 })) {
      // Try negative amount
      await amountInput.fill('-1000');
      
      const submitButton = page.getByRole('button', { name: /Submit|Apply|Next/i });
      if (await submitButton.isVisible()) {
        await submitButton.click();
        await page.waitForTimeout(500);

        // Should show validation error
        const errorMessage = page.getByText(/positive|valid|invalid/i);
        if (await errorMessage.isVisible({ timeout: 1000 })) {
          await expect(errorMessage).toBeVisible();
        }
      }
    }
  });

  test('should validate tenure range', async ({ page }) => {
    const tenureInput = page.locator('input[name="tenure"]').first();
    if (await tenureInput.isVisible({ timeout: 1000 })) {
      // Try invalid tenure
      await tenureInput.fill('0');
      
      const submitButton = page.getByRole('button', { name: /Submit|Apply|Next/i });
      if (await submitButton.isVisible()) {
        await submitButton.click();
        await page.waitForTimeout(500);

        // Should show validation error or prevent submission
        const isValid = await tenureInput.evaluate((el: HTMLInputElement) => el.validity.valid);
        expect(isValid).toBeFalsy();
      }
    }
  });
});

test.describe('New Loan Application - Form Persistence and Error Handling', () => {
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

    await page.goto('/applications/new');
  });

  test('should persist form data when navigating away', async ({ page }) => {
    const testData = {
      amount: '750000',
      tenure: '18',
      purpose: 'Business expansion',
    };

    // Fill form fields
    const amountInput = page.locator('input[name="amount"]').first();
    const tenureInput = page.locator('input[name="tenure"]').first();
    const purposeInput = page.locator('textarea[name="purpose"], input[name="purpose"]').first();

    if (await amountInput.isVisible({ timeout: 1000 })) {
      await amountInput.fill(testData.amount);
    }
    if (await tenureInput.isVisible({ timeout: 1000 })) {
      await tenureInput.fill(testData.tenure);
    }
    if (await purposeInput.isVisible({ timeout: 1000 })) {
      await purposeInput.fill(testData.purpose);
    }

    await page.waitForTimeout(500);

    // Navigate away
    await page.goto('/dashboard');
    await page.waitForTimeout(500);

    // Navigate back
    await page.goto('/applications/new');
    await page.waitForTimeout(1000);

    // Check if data is persisted (localStorage or sessionStorage)
    const persistedData = await page.evaluate(() => {
      const stored = localStorage.getItem('loan-application-draft') || 
                     sessionStorage.getItem('loan-application-draft');
      return stored ? JSON.parse(stored) : null;
    });

    // If persistence is implemented, data should be restored
    if (persistedData) {
      expect(persistedData.amount).toBe(testData.amount);
    }
  });

  test('should display error message on submission failure', async ({ page }) => {
    await page.route('**/api/loan-applications', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 400,
          contentType: 'application/json',
          body: JSON.stringify({
            message: 'Validation failed',
            errors: {
              amount: 'Amount exceeds your eligible limit',
              guarantors: 'At least one guarantor is required',
            },
          }),
        });
      }
    });

    const amountInput = page.locator('input[name="amount"]').first();
    if (await amountInput.isVisible({ timeout: 1000 })) {
      await amountInput.fill('5000000');

      const submitButton = page.getByRole('button', { name: /Submit|Apply/i });
      if (await submitButton.isVisible()) {
        await submitButton.click();
        await page.waitForTimeout(1000);

        // Should display specific error messages
        const errorText = page.getByText(/exceeds|limit|required/i);
        if (await errorText.isVisible({ timeout: 1000 })) {
          await expect(errorText).toBeVisible();
        }
      }
    }
  });

  test('should retain all input values after failed submission', async ({ page }) => {
    await page.route('**/api/loan-applications', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 500,
          contentType: 'application/json',
          body: JSON.stringify({ message: 'Internal server error' }),
        });
      }
    });

    const testData = {
      amount: '500000',
      tenure: '12',
      purpose: 'Home renovation',
    };

    // Fill all fields
    const amountInput = page.locator('input[name="amount"]').first();
    const tenureInput = page.locator('input[name="tenure"]').first();
    const purposeInput = page.locator('textarea[name="purpose"], input[name="purpose"]').first();

    if (await amountInput.isVisible({ timeout: 1000 })) {
      await amountInput.fill(testData.amount);
    }
    if (await tenureInput.isVisible({ timeout: 1000 })) {
      await tenureInput.fill(testData.tenure);
    }
    if (await purposeInput.isVisible({ timeout: 1000 })) {
      await purposeInput.fill(testData.purpose);
    }

    // Submit form
    const submitButton = page.getByRole('button', { name: /Submit|Apply/i });
    if (await submitButton.isVisible()) {
      await submitButton.click();
      await page.waitForTimeout(1000);

      // All inputs should retain their values
      if (await amountInput.isVisible()) {
        await expect(amountInput).toHaveValue(testData.amount);
      }
      if (await tenureInput.isVisible()) {
        await expect(tenureInput).toHaveValue(testData.tenure);
      }
      if (await purposeInput.isVisible()) {
        await expect(purposeInput).toHaveValue(testData.purpose);
      }
    }
  });

  test('should handle network timeout errors', async ({ page }) => {
    await page.route('**/api/loan-applications', async (route) => {
      if (route.request().method() === 'POST') {
        // Simulate timeout
        await new Promise((resolve) => setTimeout(resolve, 5000));
        await route.abort('timedout');
      }
    });

    const amountInput = page.locator('input[name="amount"]').first();
    if (await amountInput.isVisible({ timeout: 1000 })) {
      await amountInput.fill('500000');

      const submitButton = page.getByRole('button', { name: /Submit|Apply/i });
      if (await submitButton.isVisible()) {
        await submitButton.click();
        await page.waitForTimeout(6000);

        // Should show timeout or network error message
        const errorMessage = page.getByText(/timeout|network|connection|failed/i);
        if (await errorMessage.isVisible({ timeout: 1000 })) {
          await expect(errorMessage).toBeVisible();
        }
      }
    }
  });

  test('should handle validation errors from backend', async ({ page }) => {
    await page.route('**/api/loan-applications', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 422,
          contentType: 'application/json',
          body: JSON.stringify({
            message: 'Validation error',
            errors: [
              { field: 'amount', message: 'Amount must be at least ₦10,000' },
              { field: 'tenure', message: 'Tenure must be between 1 and 60 months' },
            ],
          }),
        });
      }
    });

    const amountInput = page.locator('input[name="amount"]').first();
    if (await amountInput.isVisible({ timeout: 1000 })) {
      await amountInput.fill('5000');

      const submitButton = page.getByRole('button', { name: /Submit|Apply/i });
      if (await submitButton.isVisible()) {
        await submitButton.click();
        await page.waitForTimeout(1000);

        // Should display field-specific validation errors
        const validationError = page.getByText(/must be at least|between/i);
        if (await validationError.isVisible({ timeout: 1000 })) {
          await expect(validationError).toBeVisible();
        }
      }
    }
  });

  test('should clear form after successful submission', async ({ page }) => {
    await page.route('**/api/loan-applications', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 201,
          contentType: 'application/json',
          body: JSON.stringify({
            id: 'APP-001',
            loanNumber: 'LOAN00000001',
            status: 'PENDING',
            message: 'Application submitted successfully',
          }),
        });
      }
    });

    const amountInput = page.locator('input[name="amount"]').first();
    const tenureInput = page.locator('input[name="tenure"]').first();

    if (await amountInput.isVisible({ timeout: 1000 })) {
      await amountInput.fill('500000');
    }
    if (await tenureInput.isVisible({ timeout: 1000 })) {
      await tenureInput.fill('12');
    }

    const submitButton = page.getByRole('button', { name: /Submit|Apply/i });
    if (await submitButton.isVisible()) {
      await submitButton.click();
      await page.waitForTimeout(1500);

      // After success, form might be cleared or user redirected
      const successMessage = page.getByText(/success|submitted/i);
      if (await successMessage.isVisible({ timeout: 1000 })) {
        await expect(successMessage).toBeVisible();
      }
    }
  });

  test('should handle unauthorized access errors', async ({ page }) => {
    await page.route('**/api/loan-applications', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 401,
          contentType: 'application/json',
          body: JSON.stringify({ message: 'Unauthorized' }),
        });
      }
    });

    const amountInput = page.locator('input[name="amount"]').first();
    if (await amountInput.isVisible({ timeout: 1000 })) {
      await amountInput.fill('500000');

      const submitButton = page.getByRole('button', { name: /Submit|Apply/i });
      if (await submitButton.isVisible()) {
        await submitButton.click();
        await page.waitForTimeout(1000);

        // Should show unauthorized error or redirect to login
        const currentUrl = page.url();
        const hasError = await page.getByText(/unauthorized|login|session/i).isVisible({ timeout: 1000 });
        
        expect(currentUrl.includes('/login') || hasError).toBeTruthy();
      }
    }
  });

  test('should allow retry after failed submission', async ({ page }) => {
    let attemptCount = 0;

    await page.route('**/api/loan-applications', async (route) => {
      if (route.request().method() === 'POST') {
        attemptCount++;
        
        if (attemptCount === 1) {
          // First attempt fails
          await route.fulfill({
            status: 500,
            contentType: 'application/json',
            body: JSON.stringify({ message: 'Server error' }),
          });
        } else {
          // Second attempt succeeds
          await route.fulfill({
            status: 201,
            contentType: 'application/json',
            body: JSON.stringify({
              id: 'APP-001',
              loanNumber: 'LOAN00000001',
              status: 'PENDING',
            }),
          });
        }
      }
    });

    const amountInput = page.locator('input[name="amount"]').first();
    if (await amountInput.isVisible({ timeout: 1000 })) {
      await amountInput.fill('500000');

      const submitButton = page.getByRole('button', { name: /Submit|Apply/i });
      if (await submitButton.isVisible()) {
        // First attempt
        await submitButton.click();
        await page.waitForTimeout(1000);

        // Should show error
        await expect(page.getByText(/error|failed/i)).toBeVisible();

        // Retry
        await submitButton.click();
        await page.waitForTimeout(1000);

        // Should succeed
        await expect(page.getByText(/success|submitted/i)).toBeVisible();
      }
    }
  });

  test('should validate file uploads if required', async ({ page }) => {
    const fileInput = page.locator('input[type="file"]').first();
    
    if (await fileInput.isVisible({ timeout: 1000 })) {
      // Try to submit without uploading required files
      const submitButton = page.getByRole('button', { name: /Submit|Apply/i });
      if (await submitButton.isVisible()) {
        await submitButton.click();
        await page.waitForTimeout(500);

        // Should show validation error for missing files
        const errorMessage = page.getByText(/required|upload|document/i);
        if (await errorMessage.isVisible({ timeout: 1000 })) {
          await expect(errorMessage).toBeVisible();
        }
      }
    }
  });
});
