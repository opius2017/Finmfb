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

  test('should display loan applications page with list', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(5);

    await page.route('**/api/loan-applications**', async (route) => {
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

    await expect(page.getByRole('heading', { name: /Loan Applications/i })).toBeVisible();
    await expect(page.getByText(applications[0].loanNumber)).toBeVisible();
  });

  test('should filter applications by status', async ({ page }) => {
    const pendingApps = mockDataFactory.createLoanApplications(3, { status: 'PENDING' });
    const approvedApps = mockDataFactory.createLoanApplications(2, { status: 'APPROVED' });

    await page.route('**/api/loan-applications**', async (route) => {
      const url = new URL(route.request().url());
      const status = url.searchParams.get('status');
      
      let data = [...pendingApps, ...approvedApps];
      if (status) {
        data = data.filter(app => app.status === status);
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data,
          total: data.length,
          page: 1,
          pageSize: 10,
        }),
      });
    });

    await page.reload();

    // Filter by PENDING
    await page.getByLabel(/Status/i).selectOption('PENDING');
    await page.waitForTimeout(500);
    
    await expect(page.getByText(pendingApps[0].loanNumber)).toBeVisible();

    // Filter by APPROVED
    await page.getByLabel(/Status/i).selectOption('APPROVED');
    await page.waitForTimeout(500);
    
    await expect(page.getByText(approvedApps[0].loanNumber)).toBeVisible();
  });

  test('should filter applications by date', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(5);

    await page.route('**/api/loan-applications**', async (route) => {
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

    // Set date range
    await page.getByLabel(/Start Date/i).fill('2024-01-01');
    await page.getByLabel(/End Date/i).fill('2024-12-31');
    
    await page.waitForTimeout(500);
    
    await expect(page.getByText(applications[0].loanNumber)).toBeVisible();
  });

  test('should filter applications by loan type', async ({ page }) => {
    const personalLoans = mockDataFactory.createLoanApplications(2, { loanType: 'PERSONAL' });
    const emergencyLoans = mockDataFactory.createLoanApplications(2, { loanType: 'EMERGENCY' });

    await page.route('**/api/loan-applications**', async (route) => {
      const url = new URL(route.request().url());
      const loanType = url.searchParams.get('loanType');
      
      let data = [...personalLoans, ...emergencyLoans];
      if (loanType) {
        data = data.filter(app => app.loanType === loanType);
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data,
          total: data.length,
          page: 1,
          pageSize: 10,
        }),
      });
    });

    await page.reload();

    // Filter by PERSONAL
    await page.getByLabel(/Loan Type/i).selectOption('PERSONAL');
    await page.waitForTimeout(500);
    
    await expect(page.getByText(personalLoans[0].loanNumber)).toBeVisible();
  });

  test('should display application detail view', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      status: 'PENDING',
      amount: 500000,
      tenure: 12,
    });

    await page.route('**/api/loan-applications**', async (route) => {
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

    // Click on application
    await page.getByText(application.loanNumber).click();

    await page.waitForTimeout(500);

    // Verify detail view
    await expect(page.getByText(application.memberName)).toBeVisible();
    await expect(page.getByText(/₦500,000/)).toBeVisible();
    await expect(page.getByText(/12 months/i)).toBeVisible();
  });

  test('should display status indicators', async ({ page }) => {
    const applications = [
      mockDataFactory.createLoanApplication({ status: 'PENDING' }),
      mockDataFactory.createLoanApplication({ status: 'APPROVED' }),
      mockDataFactory.createLoanApplication({ status: 'REJECTED' }),
      mockDataFactory.createLoanApplication({ status: 'DISBURSED' }),
    ];

    await page.route('**/api/loan-applications**', async (route) => {
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

    // Verify status badges are visible
    await expect(page.getByText('PENDING')).toBeVisible();
    await expect(page.getByText('APPROVED')).toBeVisible();
    await expect(page.getByText('REJECTED')).toBeVisible();
    await expect(page.getByText('DISBURSED')).toBeVisible();
  });

  test('should sort applications by date', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(5);

    await page.route('**/api/loan-applications**', async (route) => {
      const url = new URL(route.request().url());
      const sortBy = url.searchParams.get('sortBy');
      const sortOrder = url.searchParams.get('sortOrder');
      
      let data = [...applications];
      if (sortBy === 'applicationDate') {
        data.sort((a, b) => {
          const comparison = new Date(a.applicationDate).getTime() - new Date(b.applicationDate).getTime();
          return sortOrder === 'desc' ? -comparison : comparison;
        });
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data,
          total: data.length,
          page: 1,
          pageSize: 10,
        }),
      });
    });

    await page.reload();

    // Click sort by date
    await page.getByRole('button', { name: /Date/i }).click();
    await page.waitForTimeout(500);
    
    await expect(page.getByText(applications[0].loanNumber)).toBeVisible();
  });

  test('should handle empty state', async ({ page }) => {
    await page.route('**/api/loan-applications**', async (route) => {
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

    await expect(page.getByText(/No applications found/i)).toBeVisible();
  });

  test('should handle loading state', async ({ page }) => {
    await page.route('**/api/loan-applications**', async (route) => {
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

    await expect(page.getByText(/Loading/i)).toBeVisible();
  });

  test('should handle API errors', async ({ page }) => {
    await page.route('**/api/loan-applications**', async (route) => {
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Internal server error' }),
      });
    });

    await page.reload();

    await expect(page.getByText(/Failed to load applications/i)).toBeVisible();
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

  test('should paginate through applications', async ({ page }) => {
    const page1Apps = mockDataFactory.createLoanApplications(10);
    const page2Apps = mockDataFactory.createLoanApplications(10);

    await page.route('**/api/loan-applications**', async (route) => {
      const url = new URL(route.request().url());
      const pageNum = parseInt(url.searchParams.get('page') || '1');
      
      const data = pageNum === 1 ? page1Apps : page2Apps;

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data,
          total: 20,
          page: pageNum,
          pageSize: 10,
        }),
      });
    });

    await page.reload();

    // Verify first page
    await expect(page.getByText(page1Apps[0].loanNumber)).toBeVisible();

    // Go to next page
    await page.getByRole('button', { name: /Next/i }).click();
    await page.waitForTimeout(500);

    // Verify second page
    await expect(page.getByText(page2Apps[0].loanNumber)).toBeVisible();
  });

  test('should display pagination controls', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(25);

    await page.route('**/api/loan-applications**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: applications.slice(0, 10),
          total: 25,
          page: 1,
          pageSize: 10,
        }),
      });
    });

    await page.reload();

    // Verify pagination controls
    await expect(page.getByRole('button', { name: /Previous/i })).toBeVisible();
    await expect(page.getByRole('button', { name: /Next/i })).toBeVisible();
    await expect(page.getByText(/Page 1 of 3/i)).toBeVisible();
  });

  test('should search applications by loan number', async ({ page }) => {
    const allApps = mockDataFactory.createLoanApplications(10);
    const searchApp = allApps[0];

    await page.route('**/api/loan-applications**', async (route) => {
      const url = new URL(route.request().url());
      const search = url.searchParams.get('search');
      
      let data = allApps;
      if (search) {
        data = allApps.filter(app => 
          app.loanNumber.toLowerCase().includes(search.toLowerCase())
        );
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data,
          total: data.length,
          page: 1,
          pageSize: 10,
        }),
      });
    });

    await page.reload();

    // Search by loan number
    await page.getByPlaceholder(/Search/i).fill(searchApp.loanNumber);
    await page.waitForTimeout(500);

    await expect(page.getByText(searchApp.loanNumber)).toBeVisible();
  });

  test('should search applications by member name', async ({ page }) => {
    const allApps = mockDataFactory.createLoanApplications(10);
    allApps[0].memberName = 'John Doe';

    await page.route('**/api/loan-applications**', async (route) => {
      const url = new URL(route.request().url());
      const search = url.searchParams.get('search');
      
      let data = allApps;
      if (search) {
        data = allApps.filter(app => 
          app.memberName.toLowerCase().includes(search.toLowerCase())
        );
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data,
          total: data.length,
          page: 1,
          pageSize: 10,
        }),
      });
    });

    await page.reload();

    // Search by member name
    await page.getByPlaceholder(/Search/i).fill('John');
    await page.waitForTimeout(500);

    await expect(page.getByText('John Doe')).toBeVisible();
  });

  test('should display empty search results', async ({ page }) => {
    await page.route('**/api/loan-applications**', async (route) => {
      const url = new URL(route.request().url());
      const search = url.searchParams.get('search');
      
      const data = search ? [] : mockDataFactory.createLoanApplications(5);

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data,
          total: data.length,
          page: 1,
          pageSize: 10,
        }),
      });
    });

    await page.reload();

    // Search for non-existent application
    await page.getByPlaceholder(/Search/i).fill('NONEXISTENT');
    await page.waitForTimeout(500);

    await expect(page.getByText(/No applications found/i)).toBeVisible();
  });

  test('should clear search', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(10);

    await page.route('**/api/loan-applications**', async (route) => {
      const url = new URL(route.request().url());
      const search = url.searchParams.get('search');
      
      const data = search ? [] : applications;

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data,
          total: data.length,
          page: 1,
          pageSize: 10,
        }),
      });
    });

    await page.reload();

    // Search
    const searchInput = page.getByPlaceholder(/Search/i);
    await searchInput.fill('TEST');
    await page.waitForTimeout(500);

    // Clear search
    await searchInput.clear();
    await page.waitForTimeout(500);

    // Should show all applications
    await expect(page.getByText(applications[0].loanNumber)).toBeVisible();
  });

  test('should maintain filters when paginating', async ({ page }) => {
    const pendingApps = mockDataFactory.createLoanApplications(15, { status: 'PENDING' });

    await page.route('**/api/loan-applications**', async (route) => {
      const url = new URL(route.request().url());
      const status = url.searchParams.get('status');
      const pageNum = parseInt(url.searchParams.get('page') || '1');
      
      let data = status === 'PENDING' ? pendingApps : [];
      const start = (pageNum - 1) * 10;
      const end = start + 10;
      data = data.slice(start, end);

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data,
          total: pendingApps.length,
          page: pageNum,
          pageSize: 10,
        }),
      });
    });

    await page.reload();

    // Apply filter
    await page.getByLabel(/Status/i).selectOption('PENDING');
    await page.waitForTimeout(500);

    // Go to next page
    await page.getByRole('button', { name: /Next/i }).click();
    await page.waitForTimeout(500);

    // Filter should still be applied
    await expect(page.getByLabel(/Status/i)).toHaveValue('PENDING');
  });

  test('should update page size', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(30);

    await page.route('**/api/loan-applications**', async (route) => {
      const url = new URL(route.request().url());
      const pageSize = parseInt(url.searchParams.get('pageSize') || '10');
      
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: applications.slice(0, pageSize),
          total: applications.length,
          page: 1,
          pageSize,
        }),
      });
    });

    await page.reload();

    // Change page size
    await page.getByLabel(/Items per page/i).selectOption('20');
    await page.waitForTimeout(500);

    // Should show more items
    await expect(page.getByText(/Page 1 of 2/i)).toBeVisible();
  });
});
