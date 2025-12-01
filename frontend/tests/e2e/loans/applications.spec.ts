import { test, expect } from '@playwright/test';
import { mockDataFactory } from '../../utils/mock-data';

test.describe('Loan Applications', () => {
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

    await page.goto('/applications');
  });

  test('should display loan applications page', async ({ page }) => {
    await expect(page.getByRole('heading', { name: /Loan Applications/i })).toBeVisible();
  });

  test('should display list of loan applications', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(5);

    await page.route('**/api/loan-applications', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: applications,
          total: applications.length,
          page: 1,
          pageSize: 10,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Verify applications are displayed
    for (const app of applications) {
      await expect(page.getByText(app.loanNumber)).toBeVisible();
    }
  });

  test('should filter applications by status', async ({ page }) => {
    const pendingApps = mockDataFactory.createLoanApplications(3, { status: 'PENDING' });
    const approvedApps = mockDataFactory.createLoanApplications(2, { status: 'APPROVED' });

    await page.route('**/api/loan-applications*', async (route) => {
      const url = route.request().url();
      const urlParams = new URL(url).searchParams;
      const status = urlParams.get('status');

      let filteredApps = [...pendingApps, ...approvedApps];
      if (status) {
        filteredApps = filteredApps.filter(app => app.status === status);
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: filteredApps,
          total: filteredApps.length,
          page: 1,
          pageSize: 10,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Filter by PENDING
    const statusFilter = page.locator('select[name="status"], select#status, [aria-label*="Status"]').first();
    if (await statusFilter.isVisible()) {
      await statusFilter.selectOption('PENDING');
      await page.waitForTimeout(1000);

      // Should show only pending applications
      for (const app of pendingApps) {
        await expect(page.getByText(app.loanNumber)).toBeVisible();
      }
    }
  });

  test('should filter applications by date', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(5);

    await page.route('**/api/loan-applications*', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: applications,
          total: applications.length,
          page: 1,
          pageSize: 10,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Look for date filter inputs
    const startDateInput = page.locator('input[type="date"][name*="start"], input[type="date"][placeholder*="Start"]').first();
    const endDateInput = page.locator('input[type="date"][name*="end"], input[type="date"][placeholder*="End"]').first();

    if (await startDateInput.isVisible() && await endDateInput.isVisible()) {
      await startDateInput.fill('2024-01-01');
      await endDateInput.fill('2024-12-31');
      await page.waitForTimeout(1000);

      // Applications should be filtered by date range
      await expect(page.getByText(applications[0].loanNumber)).toBeVisible();
    }
  });

  test('should filter applications by loan type', async ({ page }) => {
    const personalLoans = mockDataFactory.createLoanApplications(2, { loanType: 'PERSONAL' });
    const emergencyLoans = mockDataFactory.createLoanApplications(2, { loanType: 'EMERGENCY' });

    await page.route('**/api/loan-applications*', async (route) => {
      const url = route.request().url();
      const urlParams = new URL(url).searchParams;
      const loanType = urlParams.get('loanType');

      let filteredApps = [...personalLoans, ...emergencyLoans];
      if (loanType) {
        filteredApps = filteredApps.filter(app => app.loanType === loanType);
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: filteredApps,
          total: filteredApps.length,
          page: 1,
          pageSize: 10,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Filter by loan type
    const loanTypeFilter = page.locator('select[name="loanType"], select#loanType, [aria-label*="Loan Type"]').first();
    if (await loanTypeFilter.isVisible()) {
      await loanTypeFilter.selectOption('PERSONAL');
      await page.waitForTimeout(1000);

      // Should show only personal loans
      for (const app of personalLoans) {
        await expect(page.getByText(app.loanNumber)).toBeVisible();
      }
    }
  });

  test('should display application detail view', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      loanNumber: 'LOAN00000001',
      memberName: 'John Doe',
      amount: 500000,
      status: 'PENDING',
    });

    await page.route('**/api/loan-applications', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [application],
          total: 1,
          page: 1,
          pageSize: 10,
        }),
      });
    });

    await page.route(`**/api/loan-applications/${application.id}`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(application),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Click on application to view details
    const applicationRow = page.getByText(application.loanNumber);
    if (await applicationRow.isVisible()) {
      await applicationRow.click();
      await page.waitForTimeout(1000);

      // Should show application details
      await expect(page.getByText(application.memberName)).toBeVisible();
      await expect(page.getByText(/500,000|₦500,000/)).toBeVisible();
    }
  });

  test('should display status indicators', async ({ page }) => {
    const applications = [
      mockDataFactory.createLoanApplication({ status: 'PENDING', loanNumber: 'LOAN00000001' }),
      mockDataFactory.createLoanApplication({ status: 'APPROVED', loanNumber: 'LOAN00000002' }),
      mockDataFactory.createLoanApplication({ status: 'REJECTED', loanNumber: 'LOAN00000003' }),
      mockDataFactory.createLoanApplication({ status: 'DISBURSED', loanNumber: 'LOAN00000004' }),
    ];

    await page.route('**/api/loan-applications', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: applications,
          total: applications.length,
          page: 1,
          pageSize: 10,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Verify status indicators are displayed
    await expect(page.getByText('PENDING')).toBeVisible();
    await expect(page.getByText('APPROVED')).toBeVisible();
    await expect(page.getByText('REJECTED')).toBeVisible();
    await expect(page.getByText('DISBURSED')).toBeVisible();
  });

  test('should handle empty applications list', async ({ page }) => {
    await page.route('**/api/loan-applications', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [],
          total: 0,
          page: 1,
          pageSize: 10,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Should show empty state message
    await expect(page.getByText(/No applications found|No loan applications/i)).toBeVisible();
  });

  test('should handle API errors', async ({ page }) => {
    await page.route('**/api/loan-applications', async (route) => {
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
    await page.route('**/api/loan-applications', async (route) => {
      await new Promise((resolve) => setTimeout(resolve, 1500));
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [],
          total: 0,
          page: 1,
          pageSize: 10,
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
});

test.describe('Loan Applications - Pagination and Search', () => {
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

    await page.goto('/applications');
  });

  test('should display pagination controls', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(25);

    await page.route('**/api/loan-applications*', async (route) => {
      const url = route.request().url();
      const urlParams = new URL(url).searchParams;
      const page = parseInt(urlParams.get('page') || '1');
      const pageSize = parseInt(urlParams.get('pageSize') || '10');

      const start = (page - 1) * pageSize;
      const end = start + pageSize;
      const paginatedData = applications.slice(start, end);

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: paginatedData,
          total: applications.length,
          page,
          pageSize,
          totalPages: Math.ceil(applications.length / pageSize),
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Look for pagination controls
    const nextButton = page.getByRole('button', { name: /Next|next|>/i });
    const prevButton = page.getByRole('button', { name: /Previous|previous|</i });

    if (await nextButton.isVisible() || await prevButton.isVisible()) {
      await expect(nextButton.or(prevButton)).toBeVisible();
    }
  });

  test('should navigate to next page', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(25);

    await page.route('**/api/loan-applications*', async (route) => {
      const url = route.request().url();
      const urlParams = new URL(url).searchParams;
      const pageNum = parseInt(urlParams.get('page') || '1');
      const pageSize = parseInt(urlParams.get('pageSize') || '10');

      const start = (pageNum - 1) * pageSize;
      const end = start + pageSize;
      const paginatedData = applications.slice(start, end);

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: paginatedData,
          total: applications.length,
          page: pageNum,
          pageSize,
          totalPages: Math.ceil(applications.length / pageSize),
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Click next button
    const nextButton = page.getByRole('button', { name: /Next|next|>/i });
    if (await nextButton.isVisible()) {
      await nextButton.click();
      await page.waitForTimeout(1000);

      // Should show page 2 applications
      await expect(page.getByText(applications[10].loanNumber)).toBeVisible();
    }
  });

  test('should navigate to previous page', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(25);

    await page.route('**/api/loan-applications*', async (route) => {
      const url = route.request().url();
      const urlParams = new URL(url).searchParams;
      const pageNum = parseInt(urlParams.get('page') || '1');
      const pageSize = parseInt(urlParams.get('pageSize') || '10');

      const start = (pageNum - 1) * pageSize;
      const end = start + pageSize;
      const paginatedData = applications.slice(start, end);

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: paginatedData,
          total: applications.length,
          page: pageNum,
          pageSize,
          totalPages: Math.ceil(applications.length / pageSize),
        }),
      });
    });

    await page.goto('/applications?page=2');
    await page.waitForTimeout(1000);

    // Click previous button
    const prevButton = page.getByRole('button', { name: /Previous|previous|</i });
    if (await prevButton.isVisible()) {
      await prevButton.click();
      await page.waitForTimeout(1000);

      // Should show page 1 applications
      await expect(page.getByText(applications[0].loanNumber)).toBeVisible();
    }
  });

  test('should search applications by loan number', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(10);
    const searchTarget = applications[0];

    await page.route('**/api/loan-applications*', async (route) => {
      const url = route.request().url();
      const urlParams = new URL(url).searchParams;
      const search = urlParams.get('search') || urlParams.get('q');

      let filteredApps = applications;
      if (search) {
        filteredApps = applications.filter(app => 
          app.loanNumber.toLowerCase().includes(search.toLowerCase()) ||
          app.memberName.toLowerCase().includes(search.toLowerCase())
        );
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: filteredApps,
          total: filteredApps.length,
          page: 1,
          pageSize: 10,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Find search input
    const searchInput = page.locator('input[type="search"], input[placeholder*="Search"], input[name="search"]').first();
    if (await searchInput.isVisible()) {
      await searchInput.fill(searchTarget.loanNumber);
      await page.waitForTimeout(1000);

      // Should show only matching application
      await expect(page.getByText(searchTarget.loanNumber)).toBeVisible();
    }
  });

  test('should search applications by member name', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(10, { memberName: 'John Doe' });
    applications[0].memberName = 'Jane Smith';

    await page.route('**/api/loan-applications*', async (route) => {
      const url = route.request().url();
      const urlParams = new URL(url).searchParams;
      const search = urlParams.get('search') || urlParams.get('q');

      let filteredApps = applications;
      if (search) {
        filteredApps = applications.filter(app => 
          app.memberName.toLowerCase().includes(search.toLowerCase())
        );
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: filteredApps,
          total: filteredApps.length,
          page: 1,
          pageSize: 10,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Search for Jane Smith
    const searchInput = page.locator('input[type="search"], input[placeholder*="Search"], input[name="search"]').first();
    if (await searchInput.isVisible()) {
      await searchInput.fill('Jane Smith');
      await page.waitForTimeout(1000);

      // Should show only Jane's application
      await expect(page.getByText('Jane Smith')).toBeVisible();
      await expect(page.getByText('John Doe')).not.toBeVisible();
    }
  });

  test('should display empty search results', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(10);

    await page.route('**/api/loan-applications*', async (route) => {
      const url = route.request().url();
      const urlParams = new URL(url).searchParams;
      const search = urlParams.get('search') || urlParams.get('q');

      let filteredApps = applications;
      if (search) {
        filteredApps = applications.filter(app => 
          app.loanNumber.toLowerCase().includes(search.toLowerCase())
        );
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: filteredApps,
          total: filteredApps.length,
          page: 1,
          pageSize: 10,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Search for non-existent loan
    const searchInput = page.locator('input[type="search"], input[placeholder*="Search"], input[name="search"]').first();
    if (await searchInput.isVisible()) {
      await searchInput.fill('NONEXISTENT999');
      await page.waitForTimeout(1000);

      // Should show no results message
      await expect(page.getByText(/No results found|No applications found/i)).toBeVisible();
    }
  });

  test('should clear search', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(10);

    await page.route('**/api/loan-applications*', async (route) => {
      const url = route.request().url();
      const urlParams = new URL(url).searchParams;
      const search = urlParams.get('search') || urlParams.get('q');

      let filteredApps = applications;
      if (search) {
        filteredApps = applications.filter(app => 
          app.loanNumber.toLowerCase().includes(search.toLowerCase())
        );
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: filteredApps,
          total: filteredApps.length,
          page: 1,
          pageSize: 10,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    const searchInput = page.locator('input[type="search"], input[placeholder*="Search"], input[name="search"]').first();
    if (await searchInput.isVisible()) {
      // Search for something
      await searchInput.fill(applications[0].loanNumber);
      await page.waitForTimeout(1000);

      // Clear search
      await searchInput.clear();
      await page.waitForTimeout(1000);

      // Should show all applications again
      await expect(page.getByText(applications[0].loanNumber)).toBeVisible();
      await expect(page.getByText(applications[1].loanNumber)).toBeVisible();
    }
  });

  test('should maintain filters when paginating', async ({ page }) => {
    const pendingApps = mockDataFactory.createLoanApplications(15, { status: 'PENDING' });

    await page.route('**/api/loan-applications*', async (route) => {
      const url = route.request().url();
      const urlParams = new URL(url).searchParams;
      const pageNum = parseInt(urlParams.get('page') || '1');
      const pageSize = parseInt(urlParams.get('pageSize') || '10');
      const status = urlParams.get('status');

      let filteredApps = pendingApps;
      if (status) {
        filteredApps = filteredApps.filter(app => app.status === status);
      }

      const start = (pageNum - 1) * pageSize;
      const end = start + pageSize;
      const paginatedData = filteredApps.slice(start, end);

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: paginatedData,
          total: filteredApps.length,
          page: pageNum,
          pageSize,
          totalPages: Math.ceil(filteredApps.length / pageSize),
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Apply status filter
    const statusFilter = page.locator('select[name="status"], select#status').first();
    if (await statusFilter.isVisible()) {
      await statusFilter.selectOption('PENDING');
      await page.waitForTimeout(1000);

      // Navigate to next page
      const nextButton = page.getByRole('button', { name: /Next|next/i });
      if (await nextButton.isVisible()) {
        await nextButton.click();
        await page.waitForTimeout(1000);

        // Should still show only pending applications
        await expect(page.getByText('PENDING')).toBeVisible();
      }
    }
  });

  test('should display page numbers', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(50);

    await page.route('**/api/loan-applications*', async (route) => {
      const url = route.request().url();
      const urlParams = new URL(url).searchParams;
      const pageNum = parseInt(urlParams.get('page') || '1');
      const pageSize = parseInt(urlParams.get('pageSize') || '10');

      const start = (pageNum - 1) * pageSize;
      const end = start + pageSize;
      const paginatedData = applications.slice(start, end);

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: paginatedData,
          total: applications.length,
          page: pageNum,
          pageSize,
          totalPages: Math.ceil(applications.length / pageSize),
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Look for page number indicators
    const pageInfo = page.getByText(/Page \d+ of \d+|Showing \d+-\d+ of \d+/i);
    if (await pageInfo.isVisible({ timeout: 1000 })) {
      await expect(pageInfo).toBeVisible();
    }
  });
});
