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

  test('should display guarantor dashboard with requests', async ({ page }) => {
    const requests = mockDataFactory.createGuarantorRequests(3, { status: 'PENDING' });

    await page.route('**/api/guarantor/requests**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: requests,
          total: requests.length,
        }),
      });
    });

    await page.route('**/api/guarantor/summary', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          totalCommitments: 3,
          totalLiability: 1500000,
          availableCapacity: 500000,
          liabilityLimit: 2000000,
        }),
      });
    });

    await page.reload();

    await expect(page.getByRole('heading', { name: /Guarantor Dashboard/i })).toBeVisible();
    await expect(page.getByText(requests[0].loanNumber)).toBeVisible();
  });

  test('should display guarantor requests list', async ({ page }) => {
    const requests = mockDataFactory.createGuarantorRequests(5);

    await page.route('**/api/guarantor/requests**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: requests,
          total: requests.length,
        }),
      });
    });

    await page.route('**/api/guarantor/summary', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          totalCommitments: 5,
          totalLiability: 2500000,
          availableCapacity: 500000,
          liabilityLimit: 3000000,
        }),
      });
    });

    await page.reload();

    // Verify all requests are displayed
    for (const request of requests) {
      await expect(page.getByText(request.loanNumber)).toBeVisible();
    }
  });

  test('should approve guarantee request', async ({ page }) => {
    const request = mockDataFactory.createGuarantorRequest({ status: 'PENDING' });

    await page.route('**/api/guarantor/requests**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [request],
          total: 1,
        }),
      });
    });

    await page.route('**/api/guarantor/summary', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          totalCommitments: 1,
          totalLiability: 500000,
          availableCapacity: 1500000,
          liabilityLimit: 2000000,
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
          responseDate: new Date().toISOString(),
        }),
      });
    });

    await page.reload();

    // Click approve button
    await page.getByRole('button', { name: /Approve/i }).first().click();

    // Confirm approval
    await page.getByRole('button', { name: /Confirm/i }).click();

    await page.waitForTimeout(1000);

    // Should show success message
    await expect(page.getByText(/approved successfully/i)).toBeVisible();
  });

  test('should reject guarantee request', async ({ page }) => {
    const request = mockDataFactory.createGuarantorRequest({ status: 'PENDING' });

    await page.route('**/api/guarantor/requests**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [request],
          total: 1,
        }),
      });
    });

    await page.route('**/api/guarantor/summary', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          totalCommitments: 0,
          totalLiability: 0,
          availableCapacity: 2000000,
          liabilityLimit: 2000000,
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
          responseDate: new Date().toISOString(),
        }),
      });
    });

    await page.reload();

    // Click reject button
    await page.getByRole('button', { name: /Reject/i }).first().click();

    // Add rejection reason
    await page.getByLabel(/Reason/i).fill('Unable to guarantee at this time');

    // Confirm rejection
    await page.getByRole('button', { name: /Confirm/i }).click();

    await page.waitForTimeout(1000);

    // Should show success message
    await expect(page.getByText(/rejected successfully/i)).toBeVisible();
  });

  test('should display liability limits', async ({ page }) => {
    await page.route('**/api/guarantor/requests**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [],
          total: 0,
        }),
      });
    });

    await page.route('**/api/guarantor/summary', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          totalCommitments: 3,
          totalLiability: 1500000,
          availableCapacity: 500000,
          liabilityLimit: 2000000,
        }),
      });
    });

    await page.reload();

    // Verify liability information
    await expect(page.getByText(/Total Liability/i)).toBeVisible();
    await expect(page.getByText(/₦1,500,000/)).toBeVisible();
    await expect(page.getByText(/Available Capacity/i)).toBeVisible();
    await expect(page.getByText(/₦500,000/)).toBeVisible();
    await expect(page.getByText(/Liability Limit/i)).toBeVisible();
    await expect(page.getByText(/₦2,000,000/)).toBeVisible();
  });

  test('should display current exposure', async ({ page }) => {
    await page.route('**/api/guarantor/requests**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [],
          total: 0,
        }),
      });
    });

    await page.route('**/api/guarantor/summary', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          totalCommitments: 5,
          totalLiability: 2500000,
          availableCapacity: 500000,
          liabilityLimit: 3000000,
          exposurePercentage: 83.33,
        }),
      });
    });

    await page.reload();

    // Verify exposure information
    await expect(page.getByText(/Current Exposure/i)).toBeVisible();
    await expect(page.getByText(/83/)).toBeVisible();
  });

  test('should view request details', async ({ page }) => {
    const request = mockDataFactory.createGuarantorRequest({
      applicantName: 'John Doe',
      amount: 500000,
      status: 'PENDING',
    });

    await page.route('**/api/guarantor/requests**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [request],
          total: 1,
        }),
      });
    });

    await page.route('**/api/guarantor/summary', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          totalCommitments: 1,
          totalLiability: 500000,
          availableCapacity: 1500000,
          liabilityLimit: 2000000,
        }),
      });
    });

    await page.route(`**/api/guarantor/requests/${request.id}`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...request,
          applicantDetails: {
            memberNumber: 'MEM001',
            email: 'john.doe@example.com',
            phone: '+234-800-000-0000',
            department: 'Engineering',
          },
        }),
      });
    });

    await page.reload();

    // Click on request to view details
    await page.getByText(request.loanNumber).click();

    await page.waitForTimeout(500);

    // Verify details are displayed
    await expect(page.getByText('John Doe')).toBeVisible();
    await expect(page.getByText(/₦500,000/)).toBeVisible();
    await expect(page.getByText('MEM001')).toBeVisible();
  });

  test('should filter requests by status', async ({ page }) => {
    const pendingRequests = mockDataFactory.createGuarantorRequests(2, { status: 'PENDING' });
    const approvedRequests = mockDataFactory.createGuarantorRequests(2, { status: 'APPROVED' });

    await page.route('**/api/guarantor/requests**', async (route) => {
      const url = new URL(route.request().url());
      const status = url.searchParams.get('status');
      
      let data = [...pendingRequests, ...approvedRequests];
      if (status) {
        data = data.filter(req => req.status === status);
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data,
          total: data.length,
        }),
      });
    });

    await page.route('**/api/guarantor/summary', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          totalCommitments: 4,
          totalLiability: 2000000,
          availableCapacity: 0,
          liabilityLimit: 2000000,
        }),
      });
    });

    await page.reload();

    // Filter by PENDING
    await page.getByLabel(/Status/i).selectOption('PENDING');
    await page.waitForTimeout(500);
    
    await expect(page.getByText(pendingRequests[0].loanNumber)).toBeVisible();

    // Filter by APPROVED
    await page.getByLabel(/Status/i).selectOption('APPROVED');
    await page.waitForTimeout(500);
    
    await expect(page.getByText(approvedRequests[0].loanNumber)).toBeVisible();
  });

  test('should handle empty state', async ({ page }) => {
    await page.route('**/api/guarantor/requests**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [],
          total: 0,
        }),
      });
    });

    await page.route('**/api/guarantor/summary', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          totalCommitments: 0,
          totalLiability: 0,
          availableCapacity: 2000000,
          liabilityLimit: 2000000,
        }),
      });
    });

    await page.reload();

    await expect(page.getByText(/No guarantor requests/i)).toBeVisible();
  });

  test('should handle API errors', async ({ page }) => {
    await page.route('**/api/guarantor/requests**', async (route) => {
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Internal server error' }),
      });
    });

    await page.reload();

    await expect(page.getByText(/Failed to load/i)).toBeVisible();
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

  test('should complete approval workflow', async ({ page }) => {
    const request = mockDataFactory.createGuarantorRequest({ status: 'PENDING' });

    await page.route('**/api/guarantor/requests**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [request],
          total: 1,
        }),
      });
    });

    await page.route('**/api/guarantor/summary', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          totalCommitments: 0,
          totalLiability: 0,
          availableCapacity: 2000000,
          liabilityLimit: 2000000,
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

    // View request details
    await page.getByText(request.loanNumber).click();
    await page.waitForTimeout(500);

    // Approve
    await page.getByRole('button', { name: /Approve/i }).click();
    await page.getByRole('button', { name: /Confirm/i }).click();

    await page.waitForTimeout(1000);

    // Verify status update
    await expect(page.getByText(/APPROVED/i)).toBeVisible();
  });

  test('should complete rejection workflow', async ({ page }) => {
    const request = mockDataFactory.createGuarantorRequest({ status: 'PENDING' });

    await page.route('**/api/guarantor/requests**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [request],
          total: 1,
        }),
      });
    });

    await page.route('**/api/guarantor/summary', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          totalCommitments: 0,
          totalLiability: 0,
          availableCapacity: 2000000,
          liabilityLimit: 2000000,
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
        }),
      });
    });

    await page.reload();

    // Reject
    await page.getByRole('button', { name: /Reject/i }).first().click();
    await page.getByLabel(/Reason/i).fill('Insufficient capacity');
    await page.getByRole('button', { name: /Confirm/i }).click();

    await page.waitForTimeout(1000);

    // Verify status update
    await expect(page.getByText(/rejected successfully/i)).toBeVisible();
  });

  test('should update display after action', async ({ page }) => {
    const request = mockDataFactory.createGuarantorRequest({ status: 'PENDING' });

    let requestStatus = 'PENDING';

    await page.route('**/api/guarantor/requests**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [{
            ...request,
            status: requestStatus,
          }],
          total: 1,
        }),
      });
    });

    await page.route('**/api/guarantor/summary', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          totalCommitments: requestStatus === 'APPROVED' ? 1 : 0,
          totalLiability: requestStatus === 'APPROVED' ? 500000 : 0,
          availableCapacity: requestStatus === 'APPROVED' ? 1500000 : 2000000,
          liabilityLimit: 2000000,
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

    // Initial state
    await expect(page.getByText(/₦2,000,000/)).toBeVisible(); // Available capacity

    // Approve request
    await page.getByRole('button', { name: /Approve/i }).first().click();
    await page.getByRole('button', { name: /Confirm/i }).click();

    await page.waitForTimeout(1000);

    // Reload to see updated summary
    await page.reload();

    // Updated state
    await expect(page.getByText(/₦1,500,000/)).toBeVisible(); // Reduced capacity
  });

  test('should prevent approval when exceeding liability limit', async ({ page }) => {
    const request = mockDataFactory.createGuarantorRequest({ 
      status: 'PENDING',
      amount: 1000000,
    });

    await page.route('**/api/guarantor/requests**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [request],
          total: 1,
        }),
      });
    });

    await page.route('**/api/guarantor/summary', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          totalCommitments: 3,
          totalLiability: 1900000,
          availableCapacity: 100000,
          liabilityLimit: 2000000,
        }),
      });
    });

    await page.route(`**/api/guarantor/requests/${request.id}/approve`, async (route) => {
      await route.fulfill({
        status: 400,
        contentType: 'application/json',
        body: JSON.stringify({
          message: 'Approval would exceed liability limit',
        }),
      });
    });

    await page.reload();

    // Try to approve
    await page.getByRole('button', { name: /Approve/i }).first().click();
    await page.getByRole('button', { name: /Confirm/i }).click();

    await page.waitForTimeout(1000);

    // Should show error
    await expect(page.getByText(/exceed liability limit/i)).toBeVisible();
  });

  test('should show loading state during action', async ({ page }) => {
    const request = mockDataFactory.createGuarantorRequest({ status: 'PENDING' });

    await page.route('**/api/guarantor/requests**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [request],
          total: 1,
        }),
      });
    });

    await page.route('**/api/guarantor/summary', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          totalCommitments: 0,
          totalLiability: 0,
          availableCapacity: 2000000,
          liabilityLimit: 2000000,
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

    // Approve
    await page.getByRole('button', { name: /Approve/i }).first().click();
    await page.getByRole('button', { name: /Confirm/i }).click();

    // Should show loading state
    await expect(page.getByText(/Processing/i)).toBeVisible();
  });
});
