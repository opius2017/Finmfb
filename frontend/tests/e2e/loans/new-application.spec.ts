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

  test('should display new loan application form', async ({ page }) => {
    await expect(page.getByRole('heading', { name: /New Loan Application/i })).toBeVisible();
    await expect(page.getByLabel(/Loan Type/i)).toBeVisible();
    await expect(page.getByLabel(/Amount/i)).toBeVisible();
    await expect(page.getByLabel(/Tenure/i)).toBeVisible();
    await expect(page.getByLabel(/Purpose/i)).toBeVisible();
  });

  test('should navigate through multi-step form', async ({ page }) => {
    // Step 1: Loan Details
    await expect(page.getByText(/Step 1/i)).toBeVisible();
    await page.getByLabel(/Loan Type/i).selectOption('PERSONAL');
    await page.getByLabel(/Amount/i).fill('500000');
    await page.getByLabel(/Tenure/i).fill('12');
    await page.getByRole('button', { name: /Next/i }).click();

    // Step 2: Personal Information
    await expect(page.getByText(/Step 2/i)).toBeVisible();
    await page.getByLabel(/Employment Status/i).selectOption('EMPLOYED');
    await page.getByLabel(/Monthly Income/i).fill('400000');
    await page.getByRole('button', { name: /Next/i }).click();

    // Step 3: Guarantors
    await expect(page.getByText(/Step 3/i)).toBeVisible();
    await page.getByRole('button', { name: /Next/i }).click();

    // Step 4: Review
    await expect(page.getByText(/Step 4/i)).toBeVisible();
    await expect(page.getByText(/Review/i)).toBeVisible();
  });

  test('should validate required fields', async ({ page }) => {
    // Try to proceed without filling required fields
    await page.getByRole('button', { name: /Next/i }).click();

    // Should show validation errors
    await expect(page.getByText(/required/i)).toBeVisible();
  });

  test('should validate loan amount', async ({ page }) => {
    const amountInput = page.getByLabel(/Amount/i);
    
    // Test minimum amount
    await amountInput.fill('1000');
    await page.getByRole('button', { name: /Next/i }).click();
    
    await expect(page.getByText(/minimum amount/i)).toBeVisible();
  });

  test('should validate tenure range', async ({ page }) => {
    const tenureInput = page.getByLabel(/Tenure/i);
    
    // Check attributes
    await expect(tenureInput).toHaveAttribute('min', '1');
    await expect(tenureInput).toHaveAttribute('max', '360');
  });

  test('should submit valid application', async ({ page }) => {
    await page.route('**/api/loan-applications', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 201,
          contentType: 'application/json',
          body: JSON.stringify({
            id: 'APP001',
            loanNumber: 'LOAN00000001',
            status: 'PENDING',
            message: 'Application submitted successfully',
          }),
        });
      }
    });

    // Fill Step 1
    await page.getByLabel(/Loan Type/i).selectOption('PERSONAL');
    await page.getByLabel(/Amount/i).fill('500000');
    await page.getByLabel(/Tenure/i).fill('12');
    await page.getByLabel(/Purpose/i).fill('Personal development');
    await page.getByRole('button', { name: /Next/i }).click();

    // Fill Step 2
    await page.getByLabel(/Employment Status/i).selectOption('EMPLOYED');
    await page.getByLabel(/Monthly Income/i).fill('400000');
    await page.getByRole('button', { name: /Next/i }).click();

    // Skip Step 3 (Guarantors)
    await page.getByRole('button', { name: /Next/i }).click();

    // Submit from Step 4
    await page.getByRole('button', { name: /Submit/i }).click();

    await page.waitForTimeout(1000);

    // Should show success message
    await expect(page.getByText(/submitted successfully/i)).toBeVisible();
  });

  test('should display success confirmation', async ({ page }) => {
    await page.route('**/api/loan-applications', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 201,
          contentType: 'application/json',
          body: JSON.stringify({
            id: 'APP001',
            loanNumber: 'LOAN00000001',
            status: 'PENDING',
          }),
        });
      }
    });

    // Complete form
    await page.getByLabel(/Loan Type/i).selectOption('PERSONAL');
    await page.getByLabel(/Amount/i).fill('500000');
    await page.getByLabel(/Tenure/i).fill('12');
    await page.getByLabel(/Purpose/i).fill('Test purpose');
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByLabel(/Employment Status/i).selectOption('EMPLOYED');
    await page.getByLabel(/Monthly Income/i).fill('400000');
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByRole('button', { name: /Submit/i }).click();

    await page.waitForTimeout(1000);

    // Verify success confirmation
    await expect(page.getByText(/LOAN00000001/)).toBeVisible();
  });

  test('should allow going back to previous steps', async ({ page }) => {
    // Go to step 2
    await page.getByLabel(/Loan Type/i).selectOption('PERSONAL');
    await page.getByLabel(/Amount/i).fill('500000');
    await page.getByRole('button', { name: /Next/i }).click();

    // Go back to step 1
    await page.getByRole('button', { name: /Back/i }).click();

    // Should be on step 1
    await expect(page.getByText(/Step 1/i)).toBeVisible();
    await expect(page.getByLabel(/Loan Type/i)).toHaveValue('PERSONAL');
  });

  test('should select guarantors', async ({ page }) => {
    await page.route('**/api/members/guarantors', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([
          { id: 'G1', memberNumber: 'MEM001', name: 'John Doe', availableCapacity: 1000000 },
          { id: 'G2', memberNumber: 'MEM002', name: 'Jane Smith', availableCapacity: 800000 },
        ]),
      });
    });

    // Navigate to guarantors step
    await page.getByLabel(/Loan Type/i).selectOption('PERSONAL');
    await page.getByLabel(/Amount/i).fill('500000');
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByLabel(/Employment Status/i).selectOption('EMPLOYED');
    await page.getByLabel(/Monthly Income/i).fill('400000');
    await page.getByRole('button', { name: /Next/i }).click();

    // Select guarantor
    await page.getByText('John Doe').click();
    
    await expect(page.getByText(/Selected/i)).toBeVisible();
  });

  test('should upload supporting documents', async ({ page }) => {
    // Navigate to documents step
    await page.getByLabel(/Loan Type/i).selectOption('PERSONAL');
    await page.getByLabel(/Amount/i).fill('500000');
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByLabel(/Employment Status/i).selectOption('EMPLOYED');
    await page.getByLabel(/Monthly Income/i).fill('400000');
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByRole('button', { name: /Next/i }).click();

    // Upload file
    const fileInput = page.locator('input[type="file"]');
    await fileInput.setInputFiles({
      name: 'document.pdf',
      mimeType: 'application/pdf',
      buffer: Buffer.from('test file content'),
    });

    await expect(page.getByText('document.pdf')).toBeVisible();
  });

  test('should calculate EMI preview', async ({ page }) => {
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

    await page.getByLabel(/Loan Type/i).selectOption('PERSONAL');
    await page.getByLabel(/Amount/i).fill('500000');
    await page.getByLabel(/Tenure/i).fill('12');

    await page.waitForTimeout(1000);

    // Should show EMI preview
    await expect(page.getByText(/Monthly EMI/i)).toBeVisible();
    await expect(page.getByText(/44,244/)).toBeVisible();
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

  test('should save form progress when navigating away', async ({ page }) => {
    // Fill form
    await page.getByLabel(/Loan Type/i).selectOption('PERSONAL');
    await page.getByLabel(/Amount/i).fill('500000');
    await page.getByLabel(/Tenure/i).fill('12');

    // Navigate away
    await page.goto('/dashboard');

    // Come back
    await page.goto('/applications/new');

    // Form should be restored
    await expect(page.getByLabel(/Loan Type/i)).toHaveValue('PERSONAL');
    await expect(page.getByLabel(/Amount/i)).toHaveValue('500000');
  });

  test('should display error message on submission failure', async ({ page }) => {
    await page.route('**/api/loan-applications', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 400,
          contentType: 'application/json',
          body: JSON.stringify({
            message: 'Insufficient savings balance',
          }),
        });
      }
    });

    // Fill and submit form
    await page.getByLabel(/Loan Type/i).selectOption('PERSONAL');
    await page.getByLabel(/Amount/i).fill('500000');
    await page.getByLabel(/Tenure/i).fill('12');
    await page.getByLabel(/Purpose/i).fill('Test');
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByLabel(/Employment Status/i).selectOption('EMPLOYED');
    await page.getByLabel(/Monthly Income/i).fill('400000');
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByRole('button', { name: /Submit/i }).click();

    await page.waitForTimeout(1000);

    // Should show error message
    await expect(page.getByText(/Insufficient savings balance/i)).toBeVisible();
  });

  test('should retain user input after failed submission', async ({ page }) => {
    await page.route('**/api/loan-applications', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 500,
          contentType: 'application/json',
          body: JSON.stringify({ message: 'Server error' }),
        });
      }
    });

    // Fill form
    await page.getByLabel(/Loan Type/i).selectOption('PERSONAL');
    await page.getByLabel(/Amount/i).fill('500000');
    await page.getByLabel(/Tenure/i).fill('12');
    await page.getByLabel(/Purpose/i).fill('Test purpose');
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByLabel(/Employment Status/i).selectOption('EMPLOYED');
    await page.getByLabel(/Monthly Income/i).fill('400000');
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByRole('button', { name: /Submit/i }).click();

    await page.waitForTimeout(1000);

    // Go back to first step
    await page.getByRole('button', { name: /Back/i }).click();
    await page.getByRole('button', { name: /Back/i }).click();
    await page.getByRole('button', { name: /Back/i }).click();

    // Input should be retained
    await expect(page.getByLabel(/Amount/i)).toHaveValue('500000');
    await expect(page.getByLabel(/Purpose/i)).toHaveValue('Test purpose');
  });

  test('should handle network errors', async ({ page }) => {
    await page.route('**/api/loan-applications', async (route) => {
      await route.abort('failed');
    });

    // Fill and submit
    await page.getByLabel(/Loan Type/i).selectOption('PERSONAL');
    await page.getByLabel(/Amount/i).fill('500000');
    await page.getByLabel(/Tenure/i).fill('12');
    await page.getByLabel(/Purpose/i).fill('Test');
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByLabel(/Employment Status/i).selectOption('EMPLOYED');
    await page.getByLabel(/Monthly Income/i).fill('400000');
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByRole('button', { name: /Submit/i }).click();

    await page.waitForTimeout(1000);

    // Should show network error
    await expect(page.getByText(/network error|failed to submit/i)).toBeVisible();
  });

  test('should validate guarantor requirements', async ({ page }) => {
    await page.route('**/api/members/guarantors', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([]),
      });
    });

    // Navigate to guarantors step
    await page.getByLabel(/Loan Type/i).selectOption('PERSONAL');
    await page.getByLabel(/Amount/i).fill('2000000'); // Large amount requiring guarantors
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByLabel(/Employment Status/i).selectOption('EMPLOYED');
    await page.getByLabel(/Monthly Income/i).fill('400000');
    await page.getByRole('button', { name: /Next/i }).click();

    // Try to proceed without guarantors
    await page.getByRole('button', { name: /Next/i }).click();

    // Should show validation error
    await expect(page.getByText(/guarantor required/i)).toBeVisible();
  });

  test('should show loading state during submission', async ({ page }) => {
    await page.route('**/api/loan-applications', async (route) => {
      if (route.request().method() === 'POST') {
        await new Promise((resolve) => setTimeout(resolve, 1500));
        await route.fulfill({
          status: 201,
          contentType: 'application/json',
          body: JSON.stringify({
            id: 'APP001',
            loanNumber: 'LOAN00000001',
            status: 'PENDING',
          }),
        });
      }
    });

    // Fill and submit
    await page.getByLabel(/Loan Type/i).selectOption('PERSONAL');
    await page.getByLabel(/Amount/i).fill('500000');
    await page.getByLabel(/Tenure/i).fill('12');
    await page.getByLabel(/Purpose/i).fill('Test');
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByLabel(/Employment Status/i).selectOption('EMPLOYED');
    await page.getByLabel(/Monthly Income/i).fill('400000');
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByRole('button', { name: /Submit/i }).click();

    // Should show loading state
    await expect(page.getByText(/Submitting/i)).toBeVisible();
    await expect(page.getByRole('button', { name: /Submit/i })).toBeDisabled();
  });

  test('should clear form after successful submission', async ({ page }) => {
    await page.route('**/api/loan-applications', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 201,
          contentType: 'application/json',
          body: JSON.stringify({
            id: 'APP001',
            loanNumber: 'LOAN00000001',
            status: 'PENDING',
          }),
        });
      }
    });

    // Fill and submit
    await page.getByLabel(/Loan Type/i).selectOption('PERSONAL');
    await page.getByLabel(/Amount/i).fill('500000');
    await page.getByLabel(/Tenure/i).fill('12');
    await page.getByLabel(/Purpose/i).fill('Test');
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByLabel(/Employment Status/i).selectOption('EMPLOYED');
    await page.getByLabel(/Monthly Income/i).fill('400000');
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByRole('button', { name: /Submit/i }).click();

    await page.waitForTimeout(1000);

    // Click "New Application" button
    await page.getByRole('button', { name: /New Application/i }).click();

    // Form should be cleared
    await expect(page.getByLabel(/Amount/i)).toHaveValue('');
  });

  test('should validate file upload size', async ({ page }) => {
    // Navigate to documents step
    await page.getByLabel(/Loan Type/i).selectOption('PERSONAL');
    await page.getByLabel(/Amount/i).fill('500000');
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByLabel(/Employment Status/i).selectOption('EMPLOYED');
    await page.getByLabel(/Monthly Income/i).fill('400000');
    await page.getByRole('button', { name: /Next/i }).click();
    
    await page.getByRole('button', { name: /Next/i }).click();

    // Try to upload large file
    const fileInput = page.locator('input[type="file"]');
    const largeBuffer = Buffer.alloc(11 * 1024 * 1024); // 11MB
    
    await fileInput.setInputFiles({
      name: 'large-document.pdf',
      mimeType: 'application/pdf',
      buffer: largeBuffer,
    });

    // Should show size error
    await expect(page.getByText(/file size exceeds/i)).toBeVisible();
  });
});
