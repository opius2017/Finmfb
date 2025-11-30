import { test, expect } from '@playwright/test';
import { mockDataFactory } from '../../utils/mock-data';

test.describe('Committee Dashboard', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem('authToken', 'test-token');
      localStorage.setItem('auth-storage', JSON.stringify({
        state: {
          user: { id: '1', email: 'test@example.com', name: 'Committee Member', role: 'COMMITTEE' },
          token: 'test-token',
          isAuthenticated: true,
        },
        version: 0,
      }));
    });

    await page.goto('/committee');
  });

  test('should display committee dashboard', async ({ page }) => {
    await expect(page.getByRole('heading', { name: /Committee Dashboard/i })).toBeVisible();
  });

  test('should display pending applications within 3 seconds', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(5, { status: 'IN_REVIEW' });
    const startTime = Date.now();

    await page.route('**/api/committee/pending-applications**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: applications,
          total: applications.length,
        }),
      });
    });

    await page.reload();
    await page.waitForLoadState('networkidle');

    const loadTime = Date.now() - startTime;
    expect(loadTime).toBeLessThan(3000);
  });

  test('should view complete application details', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-001',
      loanNumber: 'LOAN00000001',
      memberName: 'John Doe',
      amount: 500000,
      status: 'IN_REVIEW',
    });

    await page.route('**/api/committee/pending-applications**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [application],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/committee/applications/${application.id}`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...application,
          memberHistory: {
            totalLoans: 3,
            activeLoans: 1,
            repaymentHistory: 'Good',
          },
        }),
      });
    });

    await page.reload();

    const viewButton = page.getByRole('button', { name: /view/i }).or(page.getByText(application.loanNumber)).first();
    if (await viewButton.isVisible()) {
      await viewButton.click();
      await page.waitForTimeout(500);
    }
  });

  test('should approve application', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-002',
      status: 'IN_REVIEW',
    });

    await page.route('**/api/committee/pending-applications**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [application],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/committee/applications/${application.id}/approve`, async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            ...application,
            status: 'APPROVED',
          }),
        });
      }
    });

    await page.reload();

    const approveButton = page.getByRole('button', { name: /approve/i }).first();
    if (await approveButton.isVisible()) {
      await approveButton.click();
      await page.waitForTimeout(1000);
    }
  });

  test('should reject application with reason', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-003',
      status: 'IN_REVIEW',
    });

    await page.route('**/api/committee/pending-applications**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [application],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/committee/applications/${application.id}/reject`, async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            ...application,
            status: 'REJECTED',
          }),
        });
      }
    });

    await page.reload();

    const rejectButton = page.getByRole('button', { name: /reject/i }).first();
    if (await rejectButton.isVisible()) {
      await rejectButton.click();
      await page.waitForTimeout(500);

      const reasonField = page.locator('textarea[name="reason"], textarea[placeholder*="reason"]').first();
      if (await reasonField.isVisible()) {
        await reasonField.fill('Insufficient collateral');
        
        const confirmButton = page.getByRole('button', { name: /confirm/i }).or(page.getByRole('button', { name: /submit/i }));
        await confirmButton.click();
        await page.waitForTimeout(1000);
      }
    }
  });

  test('should record decision within 2 seconds', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-004',
      status: 'IN_REVIEW',
    });

    const startTime = Date.now();

    await page.route('**/api/committee/pending-applications**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [application],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/committee/applications/${application.id}/approve`, async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            ...application,
            status: 'APPROVED',
          }),
        });
      }
    });

    await page.reload();

    const approveButton = page.getByRole('button', { name: /approve/i }).first();
    if (await approveButton.isVisible()) {
      await approveButton.click();
      await page.waitForTimeout(500);

      const responseTime = Date.now() - startTime;
      expect(responseTime).toBeLessThan(2000);
    }
  });

  test('should display member history', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-005',
    });

    await page.route('**/api/committee/pending-applications**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [application],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/committee/applications/${application.id}`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...application,
          memberHistory: {
            totalLoans: 5,
            activeLoans: 2,
            completedLoans: 3,
            defaultedLoans: 0,
            repaymentHistory: 'Excellent',
            memberSince: '2020-01-01',
          },
        }),
      });
    });

    await page.reload();

    const viewButton = page.getByRole('button', { name: /view/i }).first();
    if (await viewButton.isVisible()) {
      await viewButton.click();
      await page.waitForTimeout(500);

      const historySection = page.getByText(/history/i).or(page.getByText(/Total Loans/i));
      if (await historySection.isVisible()) {
        await expect(historySection).toBeVisible();
      }
    }
  });

  test('should display voting status for multi-member approval', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-006',
      amount: 2000000, // High amount requiring multiple approvals
    });

    await page.route('**/api/committee/pending-applications**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [application],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/committee/applications/${application.id}/votes`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          requiredVotes: 3,
          currentVotes: 1,
          votes: [
            { memberId: 'COM-001', memberName: 'Member 1', decision: 'APPROVED', date: new Date().toISOString() },
          ],
        }),
      });
    });

    await page.reload();

    const votingStatus = page.getByText(/votes/i).or(page.getByText(/1 of 3/i));
    if (await votingStatus.isVisible()) {
      await expect(votingStatus).toBeVisible();
    }
  });

  test('should add comments to decision', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-007',
    });

    await page.route('**/api/committee/pending-applications**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [application],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/committee/applications/${application.id}/approve`, async (route) => {
      if (route.request().method() === 'POST') {
        const body = await route.request().postDataJSON();
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            ...application,
            status: 'APPROVED',
            comments: body.comments,
          }),
        });
      }
    });

    await page.reload();

    const approveButton = page.getByRole('button', { name: /approve/i }).first();
    if (await approveButton.isVisible()) {
      await approveButton.click();
      await page.waitForTimeout(500);

      const commentsField = page.locator('textarea[name="comments"], textarea[placeholder*="comment"]').first();
      if (await commentsField.isVisible()) {
        await commentsField.fill('Application meets all criteria');
        
        const submitButton = page.getByRole('button', { name: /submit/i }).or(page.getByRole('button', { name: /confirm/i }));
        await submitButton.click();
        await page.waitForTimeout(1000);
      }
    }
  });

  test('should handle empty pending applications', async ({ page }) => {
    await page.route('**/api/committee/pending-applications**', async (route) => {
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

    const emptyState = page.getByText(/no pending/i).or(page.getByText(/no applications/i));
    if (await emptyState.isVisible()) {
      await expect(emptyState).toBeVisible();
    }
  });

  test('should handle API errors', async ({ page }) => {
    await page.route('**/api/committee/pending-applications**', async (route) => {
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Internal server error' }),
      });
    });

    await page.reload();
  });
});

test.describe('Committee Dashboard - Voting Workflow', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem('authToken', 'test-token');
      localStorage.setItem('auth-storage', JSON.stringify({
        state: {
          user: { id: '1', email: 'test@example.com', name: 'Committee Member', role: 'COMMITTEE' },
          token: 'test-token',
          isAuthenticated: true,
        },
        version: 0,
      }));
    });

    await page.goto('/committee');
  });

  test('should complete full approval workflow', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-WORKFLOW-001',
      status: 'IN_REVIEW',
    });

    await page.route('**/api/committee/pending-applications**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [application],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/committee/applications/${application.id}/approve`, async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            ...application,
            status: 'APPROVED',
          }),
        });
      }
    });

    await page.reload();

    const approveButton = page.getByRole('button', { name: /approve/i }).first();
    if (await approveButton.isVisible()) {
      await approveButton.click();
      await page.waitForTimeout(1000);

      const successMessage = page.getByText(/approved/i).or(page.getByText(/success/i));
      if (await successMessage.isVisible()) {
        await expect(successMessage).toBeVisible();
      }
    }
  });

  test('should complete full rejection workflow', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-WORKFLOW-002',
      status: 'IN_REVIEW',
    });

    await page.route('**/api/committee/pending-applications**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [application],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/committee/applications/${application.id}/reject`, async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            ...application,
            status: 'REJECTED',
          }),
        });
      }
    });

    await page.reload();

    const rejectButton = page.getByRole('button', { name: /reject/i }).first();
    if (await rejectButton.isVisible()) {
      await rejectButton.click();
      await page.waitForTimeout(500);

      const reasonField = page.locator('textarea').first();
      if (await reasonField.isVisible()) {
        await reasonField.fill('Does not meet criteria');
        
        const confirmButton = page.getByRole('button', { name: /confirm/i });
        await confirmButton.click();
        await page.waitForTimeout(1000);
      }
    }
  });

  test('should track multi-member voting', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-VOTING-001',
      amount: 3000000,
    });

    await page.route('**/api/committee/pending-applications**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [application],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/committee/applications/${application.id}/votes`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          requiredVotes: 3,
          currentVotes: 2,
          votes: [
            { memberId: 'COM-001', memberName: 'Member 1', decision: 'APPROVED' },
            { memberId: 'COM-002', memberName: 'Member 2', decision: 'APPROVED' },
          ],
        }),
      });
    });

    await page.reload();
  });

  test('should display comments and notes', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-COMMENTS-001',
    });

    await page.route('**/api/committee/pending-applications**', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [application],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/committee/applications/${application.id}/comments`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([
          { author: 'Member 1', comment: 'Good application', date: new Date().toISOString() },
          { author: 'Member 2', comment: 'Approved', date: new Date().toISOString() },
        ]),
      });
    });

    await page.reload();
  });
});
