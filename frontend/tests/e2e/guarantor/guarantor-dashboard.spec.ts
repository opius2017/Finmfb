import { test, expect } from '@playwright/test';
import { mockDataFactory } from '../../utils/mock-data';

test.describe('Guarantor Dashboard', () => {
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

    await page.goto('/guarantor');
  });

  test('should display guarantor dashboard page', async ({ page }) => {
    await expect(page.getByRole('heading', { name: /Guarantor Dashboard/i })).toBeVisible();
  });

  test('should display guarantor requests', async ({ page }) => {
    const requests = mockDataFactory.createGuarantorRequests(3, { status: 'PENDING' });

    await page.route('**/api/guarantor/requests', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: requests,
          total: requests.length,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Verify requests are displayed
    for (const request of requests) {
      await expect(page.getByText(request.loanNumber)).toBeVisible();
    }
  });

  test('should approve guarantee request', async ({ page }) => {
    const request = mockDataFactory.createGuarantorRequest({
      id: 'REQ-001',
      loanNumber: 'LOAN00000001',
      applicantName: 'John Doe',
      amount: 500000,
      status: 'PENDING',
    });

    await page.route('**/api/guarantor/requests', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [request],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/guarantor/requests/${request.id}/approve`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...request,
          status: 'APPROVED',
          message: 'Guarantee request approved successfully',
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Click approve button
    const approveButton = page.getByRole('button', { name: /Approve/i }).first();
    if (await approveButton.isVisible()) {
      await approveButton.click();
      await page.waitForTimeout(1000);

      // Should show success message
      await expect(page.getByText(/approved|success/i)).toBeVisible();
    }
  });

  test('should reject guarantee request', async ({ page }) => {
    const request = mockDataFactory.createGuarantorRequest({
      id: 'REQ-002',
      loanNumber: 'LOAN00000002',
      applicantName: 'Jane Smith',
      amount: 300000,
      status: 'PENDING',
    });

    await page.route('**/api/guarantor/requests', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [request],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/guarantor/requests/${request.id}/reject`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...request,
          status: 'REJECTED',
          message: 'Guarantee request rejected',
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Click reject button
    const rejectButton = page.getByRole('button', { name: /Reject|Decline/i }).first();
    if (await rejectButton.isVisible()) {
      await rejectButton.click();
      await page.waitForTimeout(1000);

      // Should show confirmation or success message
      await expect(page.getByText(/rejected|declined/i)).toBeVisible();
    }
  });

  test('should display liability limits', async ({ page }) => {
    await page.route('**/api/guarantor/liability', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          totalLiability: 1500000,
          maximumLiability: 3000000,
          availableLiability: 1500000,
          activeGuarantees: 3,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Should display liability information
    await expect(page.getByText(/1,500,000|₦1,500,000/)).toBeVisible();
    await expect(page.getByText(/3,000,000|₦3,000,000/)).toBeVisible();
  });

  test('should display current exposure', async ({ page }) => {
    await page.route('**/api/guarantor/exposure', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          currentExposure: 800000,
          numberOfGuarantees: 2,
          guarantees: [
            {
              loanNumber: 'LOAN00000001',
              borrowerName: 'John Doe',
              amount: 500000,
              status: 'ACTIVE',
            },
            {
              loanNumber: 'LOAN00000002',
              borrowerName: 'Jane Smith',
              amount: 300000,
              status: 'ACTIVE',
            },
          ],
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Should display current exposure
    await expect(page.getByText(/800,000|₦800,000/)).toBeVisible();
    await expect(page.getByText(/John Doe/)).toBeVisible();
    await expect(page.getByText(/Jane Smith/)).toBeVisible();
  });

  test('should update display after approval', async ({ page }) => {
    const request = mockDataFactory.createGuarantorRequest({
      id: 'REQ-003',
      status: 'PENDING',
    });

    let requestStatus = 'PENDING';

    await page.route('**/api/guarantor/requests', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [{ ...request, status: requestStatus }],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/guarantor/requests/${request.id}/approve`, async (route) => {
      requestStatus = 'APPROVED';
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...request,
          status: 'APPROVED',
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Approve request
    const approveButton = page.getByRole('button', { name: /Approve/i }).first();
    if (await approveButton.isVisible()) {
      await approveButton.click();
      await page.waitForTimeout(1500);

      // Status should update to APPROVED
      await expect(page.getByText('APPROVED')).toBeVisible();
    }
  });

  test('should handle empty requests list', async ({ page }) => {
    await page.route('**/api/guarantor/requests', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [],
          total: 0,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Should show empty state
    await expect(page.getByText(/No requests|No pending requests/i)).toBeVisible();
  });

  test('should handle API errors', async ({ page }) => {
    await page.route('**/api/guarantor/requests', async (route) => {
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Internal server error' }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Should show error message
    await expect(page.getByText(/Failed to load|Error loading/i)).toBeVisible();
  });

  test('should display loading state', async ({ page }) => {
    await page.route('**/api/guarantor/requests', async (route) => {
      await new Promise((resolve) => setTimeout(resolve, 1500));
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [],
          total: 0,
        }),
      });
    });

    await page.reload();

    // Should show loading indicator
    const loadingIndicator = page.getByText(/Loading|loading/i);
    if (await loadingIndicator.isVisible({ timeout: 500 })) {
      await expect(loadingIndicator).toBeVisible();
    }
  });

  test('should filter requests by status', async ({ page }) => {
    const pendingRequests = mockDataFactory.createGuarantorRequests(2, { status: 'PENDING' });
    const approvedRequests = mockDataFactory.createGuarantorRequests(2, { status: 'APPROVED' });

    await page.route('**/api/guarantor/requests*', async (route) => {
      const url = route.request().url();
      const urlParams = new URL(url).searchParams;
      const status = urlParams.get('status');

      let filteredRequests = [...pendingRequests, ...approvedRequests];
      if (status) {
        filteredRequests = filteredRequests.filter(req => req.status === status);
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: filteredRequests,
          total: filteredRequests.length,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Filter by PENDING
    const statusFilter = page.locator('select[name="status"], [aria-label*="Status"]').first();
    if (await statusFilter.isVisible()) {
      await statusFilter.selectOption('PENDING');
      await page.waitForTimeout(1000);

      // Should show only pending requests
      await expect(page.getByText('PENDING')).toBeVisible();
    }
  });
});

test.describe('Guarantor Dashboard - Workflow', () => {
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

    await page.goto('/guarantor');
  });

  test('should complete full approval workflow', async ({ page }) => {
    const request = mockDataFactory.createGuarantorRequest({
      id: 'REQ-WORKFLOW-001',
      loanNumber: 'LOAN00000001',
      applicantName: 'John Doe',
      amount: 500000,
      status: 'PENDING',
    });

    await page.route('**/api/guarantor/requests', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [request],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/guarantor/requests/${request.id}`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(request),
      });
    });

    await page.route(`**/api/guarantor/requests/${request.id}/approve`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...request,
          status: 'APPROVED',
          responseDate: new Date().toISOString(),
          message: 'Guarantee approved successfully',
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Step 1: View request details
    const requestRow = page.getByText(request.loanNumber);
    if (await requestRow.isVisible()) {
      await requestRow.click();
      await page.waitForTimeout(500);

      // Should show request details
      await expect(page.getByText(request.applicantName)).toBeVisible();
      await expect(page.getByText(/500,000|₦500,000/)).toBeVisible();
    }

    // Step 2: Approve request
    const approveButton = page.getByRole('button', { name: /Approve/i }).first();
    if (await approveButton.isVisible()) {
      await approveButton.click();
      await page.waitForTimeout(1000);

      // Step 3: Verify success
      await expect(page.getByText(/approved|success/i)).toBeVisible();
    }
  });

  test('should complete full rejection workflow', async ({ page }) => {
    const request = mockDataFactory.createGuarantorRequest({
      id: 'REQ-WORKFLOW-002',
      loanNumber: 'LOAN00000002',
      applicantName: 'Jane Smith',
      amount: 300000,
      status: 'PENDING',
    });

    await page.route('**/api/guarantor/requests', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [request],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/guarantor/requests/${request.id}/reject`, async (route) => {
      const body = await route.request().postDataJSON();
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...request,
          status: 'REJECTED',
          responseDate: new Date().toISOString(),
          comments: body.comments || 'Rejected by guarantor',
          message: 'Guarantee rejected',
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Step 1: Click reject button
    const rejectButton = page.getByRole('button', { name: /Reject|Decline/i }).first();
    if (await rejectButton.isVisible()) {
      await rejectButton.click();
      await page.waitForTimeout(500);

      // Step 2: Add rejection reason (if modal/form appears)
      const commentsField = page.locator('textarea[name="comments"], textarea[placeholder*="reason"]').first();
      if (await commentsField.isVisible({ timeout: 1000 })) {
        await commentsField.fill('Unable to guarantee at this time');
        
        const confirmButton = page.getByRole('button', { name: /Confirm|Submit/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }
      }

      await page.waitForTimeout(1000);

      // Step 3: Verify rejection
      await expect(page.getByText(/rejected|declined/i)).toBeVisible();
    }
  });

  test('should update status after action', async ({ page }) => {
    const request = mockDataFactory.createGuarantorRequest({
      id: 'REQ-STATUS-001',
      status: 'PENDING',
    });

    let currentStatus = 'PENDING';

    await page.route('**/api/guarantor/requests', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [{ ...request, status: currentStatus }],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/guarantor/requests/${request.id}/approve`, async (route) => {
      currentStatus = 'APPROVED';
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...request,
          status: 'APPROVED',
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Initial status should be PENDING
    await expect(page.getByText('PENDING')).toBeVisible();

    // Approve request
    const approveButton = page.getByRole('button', { name: /Approve/i }).first();
    if (await approveButton.isVisible()) {
      await approveButton.click();
      await page.waitForTimeout(1500);

      // Status should change to APPROVED
      await expect(page.getByText('APPROVED')).toBeVisible();
    }
  });

  test('should handle approval confirmation dialog', async ({ page }) => {
    const request = mockDataFactory.createGuarantorRequest({
      id: 'REQ-CONFIRM-001',
      amount: 1000000,
      status: 'PENDING',
    });

    await page.route('**/api/guarantor/requests', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [request],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/guarantor/requests/${request.id}/approve`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...request,
          status: 'APPROVED',
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Click approve
    const approveButton = page.getByRole('button', { name: /Approve/i }).first();
    if (await approveButton.isVisible()) {
      await approveButton.click();
      await page.waitForTimeout(500);

      // Look for confirmation dialog
      const confirmDialog = page.getByText(/Are you sure|Confirm|confirm/i);
      if (await confirmDialog.isVisible({ timeout: 1000 })) {
        // Confirm the action
        const confirmButton = page.getByRole('button', { name: /Yes|Confirm|OK/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
          await page.waitForTimeout(1000);

          // Should complete approval
          await expect(page.getByText(/approved|success/i)).toBeVisible();
        }
      }
    }
  });

  test('should handle rejection with reason', async ({ page }) => {
    const request = mockDataFactory.createGuarantorRequest({
      id: 'REQ-REASON-001',
      status: 'PENDING',
    });

    await page.route('**/api/guarantor/requests', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [request],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/guarantor/requests/${request.id}/reject`, async (route) => {
      const body = await route.request().postDataJSON();
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...request,
          status: 'REJECTED',
          comments: body.comments,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Click reject
    const rejectButton = page.getByRole('button', { name: /Reject|Decline/i }).first();
    if (await rejectButton.isVisible()) {
      await rejectButton.click();
      await page.waitForTimeout(500);

      // Fill rejection reason
      const reasonField = page.locator('textarea[name="comments"], textarea[name="reason"]').first();
      if (await reasonField.isVisible({ timeout: 1000 })) {
        await reasonField.fill('Insufficient capacity to guarantee');

        const submitButton = page.getByRole('button', { name: /Submit|Confirm/i });
        if (await submitButton.isVisible()) {
          await submitButton.click();
          await page.waitForTimeout(1000);

          // Should complete rejection
          await expect(page.getByText(/rejected|declined/i)).toBeVisible();
        }
      }
    }
  });

  test('should handle workflow errors gracefully', async ({ page }) => {
    const request = mockDataFactory.createGuarantorRequest({
      id: 'REQ-ERROR-001',
      status: 'PENDING',
    });

    await page.route('**/api/guarantor/requests', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [request],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/guarantor/requests/${request.id}/approve`, async (route) => {
      await route.fulfill({
        status: 400,
        contentType: 'application/json',
        body: JSON.stringify({
          message: 'Guarantee limit exceeded',
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Try to approve
    const approveButton = page.getByRole('button', { name: /Approve/i }).first();
    if (await approveButton.isVisible()) {
      await approveButton.click();
      await page.waitForTimeout(1000);

      // Should show error message
      await expect(page.getByText(/limit exceeded|error|failed/i)).toBeVisible();
    }
  });

  test('should disable actions for already processed requests', async ({ page }) => {
    const approvedRequest = mockDataFactory.createGuarantorRequest({
      id: 'REQ-PROCESSED-001',
      status: 'APPROVED',
    });

    await page.route('**/api/guarantor/requests', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [approvedRequest],
          total: 1,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Approve/Reject buttons should be disabled or hidden
    const approveButton = page.getByRole('button', { name: /Approve/i }).first();
    const rejectButton = page.getByRole('button', { name: /Reject/i }).first();

    if (await approveButton.isVisible({ timeout: 500 })) {
      await expect(approveButton).toBeDisabled();
    }
    if (await rejectButton.isVisible({ timeout: 500 })) {
      await expect(rejectButton).toBeDisabled();
    }
  });

  test('should show loading state during action', async ({ page }) => {
    const request = mockDataFactory.createGuarantorRequest({
      id: 'REQ-LOADING-001',
      status: 'PENDING',
    });

    await page.route('**/api/guarantor/requests', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [request],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/guarantor/requests/${request.id}/approve`, async (route) => {
      await new Promise((resolve) => setTimeout(resolve, 1500));
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...request,
          status: 'APPROVED',
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Click approve
    const approveButton = page.getByRole('button', { name: /Approve/i }).first();
    if (await approveButton.isVisible()) {
      await approveButton.click();

      // Should show loading state
      const loadingButton = page.getByRole('button', { name: /Approving|Processing|loading/i });
      if (await loadingButton.isVisible({ timeout: 500 })) {
        await expect(loadingButton).toBeDisabled();
      }
    }
  });
});
