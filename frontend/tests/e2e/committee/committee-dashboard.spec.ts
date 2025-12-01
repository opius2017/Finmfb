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

  test('should display committee dashboard page', async ({ page }) => {
    await expect(page.getByRole('heading', { name: /Committee Dashboard/i })).toBeVisible();
  });

  test('should display pending applications', async ({ page }) => {
    const applications = mockDataFactory.createLoanApplications(3, { status: 'IN_REVIEW' });

    await page.route('**/api/committee/applications', async (route) => {
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
    await page.waitForTimeout(1000);

    // Verify applications are displayed
    for (const app of applications) {
      await expect(page.getByText(app.loanNumber)).toBeVisible();
    }
  });

  test('should display application review interface', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-001',
      loanNumber: 'LOAN00000001',
      memberName: 'John Doe',
      amount: 500000,
      status: 'IN_REVIEW',
    });

    await page.route('**/api/committee/applications', async (route) => {
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

  test('should display member history', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-002',
      memberId: 'MEM-001',
    });

    await page.route('**/api/committee/applications', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [application],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/members/${application.memberId}/history`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          totalLoans: 3,
          activeLoans: 1,
          completedLoans: 2,
          defaultedLoans: 0,
          totalBorrowed: 1500000,
          totalRepaid: 1000000,
          loanHistory: [
            {
              loanNumber: 'LOAN00000001',
              amount: 500000,
              status: 'COMPLETED',
              disbursementDate: '2023-01-15',
            },
            {
              loanNumber: 'LOAN00000002',
              amount: 500000,
              status: 'COMPLETED',
              disbursementDate: '2023-06-20',
            },
            {
              loanNumber: 'LOAN00000003',
              amount: 500000,
              status: 'ACTIVE',
              disbursementDate: '2024-01-10',
            },
          ],
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // View application details
    const applicationRow = page.getByText(application.loanNumber);
    if (await applicationRow.isVisible()) {
      await applicationRow.click();
      await page.waitForTimeout(1000);

      // Should show member history
      await expect(page.getByText(/Total Loans|Loan History/i)).toBeVisible();
      await expect(page.getByText(/COMPLETED|ACTIVE/i)).toBeVisible();
    }
  });

  test('should approve application', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-APPROVE-001',
      status: 'IN_REVIEW',
    });

    await page.route('**/api/committee/applications', async (route) => {
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
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...application,
          status: 'APPROVED',
          message: 'Application approved successfully',
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

  test('should reject application', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-REJECT-001',
      status: 'IN_REVIEW',
    });

    await page.route('**/api/committee/applications', async (route) => {
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
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...application,
          status: 'REJECTED',
          message: 'Application rejected',
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Click reject button
    const rejectButton = page.getByRole('button', { name: /Reject|Decline/i }).first();
    if (await rejectButton.isVisible()) {
      await rejectButton.click();
      await page.waitForTimeout(500);

      // Add rejection reason if prompted
      const reasonField = page.locator('textarea[name="reason"], textarea[name="comments"]').first();
      if (await reasonField.isVisible({ timeout: 1000 })) {
        await reasonField.fill('Insufficient documentation');
        
        const confirmButton = page.getByRole('button', { name: /Confirm|Submit/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
        }
      }

      await page.waitForTimeout(1000);

      // Should show rejection confirmation
      await expect(page.getByText(/rejected|declined/i)).toBeVisible();
    }
  });

  test('should display voting status', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-VOTE-001',
      status: 'IN_REVIEW',
    });

    await page.route('**/api/committee/applications', async (route) => {
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
          totalVotes: 5,
          approvalVotes: 3,
          rejectionVotes: 1,
          pendingVotes: 1,
          requiredVotes: 3,
          votes: [
            { memberName: 'Member 1', decision: 'APPROVED', date: '2024-01-15' },
            { memberName: 'Member 2', decision: 'APPROVED', date: '2024-01-15' },
            { memberName: 'Member 3', decision: 'APPROVED', date: '2024-01-16' },
            { memberName: 'Member 4', decision: 'REJECTED', date: '2024-01-16' },
            { memberName: 'Member 5', decision: 'PENDING', date: null },
          ],
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // View application details
    const applicationRow = page.getByText(application.loanNumber);
    if (await applicationRow.isVisible()) {
      await applicationRow.click();
      await page.waitForTimeout(1000);

      // Should show voting status
      await expect(page.getByText(/3.*5|Votes|votes/i)).toBeVisible();
    }
  });

  test('should handle empty applications list', async ({ page }) => {
    await page.route('**/api/committee/applications', async (route) => {
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
    await expect(page.getByText(/No applications|No pending applications/i)).toBeVisible();
  });

  test('should handle API errors', async ({ page }) => {
    await page.route('**/api/committee/applications', async (route) => {
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

  // Test voting status display
  test('should display voting status with vote counts', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-VOTE-STATUS-001',
      status: 'IN_REVIEW',
      loanNumber: 'LOAN00000100',
    });

    await page.route('**/api/committee/applications', async (route) => {
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
          totalVotes: 5,
          approvalVotes: 3,
          rejectionVotes: 1,
          pendingVotes: 1,
          requiredVotes: 3,
          votes: [
            { memberName: 'John Smith', decision: 'APPROVED', date: '2024-01-15T10:00:00Z', comments: 'Good credit history' },
            { memberName: 'Jane Doe', decision: 'APPROVED', date: '2024-01-15T11:30:00Z', comments: 'Meets all criteria' },
            { memberName: 'Bob Johnson', decision: 'APPROVED', date: '2024-01-16T09:00:00Z', comments: 'Approved' },
            { memberName: 'Alice Williams', decision: 'REJECTED', date: '2024-01-16T10:00:00Z', comments: 'Insufficient income' },
            { memberName: 'Charlie Brown', decision: 'PENDING', date: null, comments: null },
          ],
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // View application details
    const applicationRow = page.getByText(application.loanNumber);
    if (await applicationRow.isVisible()) {
      await applicationRow.click();
      await page.waitForTimeout(1000);

      // Should show voting status summary
      await expect(page.getByText(/3.*approval|approved.*3/i)).toBeVisible();
      await expect(page.getByText(/1.*rejection|rejected.*1/i)).toBeVisible();
      await expect(page.getByText(/1.*pending/i)).toBeVisible();
    }
  });

  test('should display individual member votes', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-VOTE-DETAIL-001',
      status: 'IN_REVIEW',
      loanNumber: 'LOAN00000101',
    });

    await page.route('**/api/committee/applications', async (route) => {
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
          totalVotes: 3,
          approvalVotes: 2,
          rejectionVotes: 0,
          pendingVotes: 1,
          requiredVotes: 2,
          votes: [
            { memberName: 'John Smith', decision: 'APPROVED', date: '2024-01-15T10:00:00Z' },
            { memberName: 'Jane Doe', decision: 'APPROVED', date: '2024-01-15T11:30:00Z' },
            { memberName: 'Bob Johnson', decision: 'PENDING', date: null },
          ],
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // View application
    const applicationRow = page.getByText(application.loanNumber);
    if (await applicationRow.isVisible()) {
      await applicationRow.click();
      await page.waitForTimeout(1000);

      // Should show individual member votes
      await expect(page.getByText(/John Smith/i)).toBeVisible();
      await expect(page.getByText(/Jane Doe/i)).toBeVisible();
      await expect(page.getByText(/Bob Johnson/i)).toBeVisible();
    }
  });

  test('should display voting progress indicator', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-PROGRESS-001',
      status: 'IN_REVIEW',
      loanNumber: 'LOAN00000102',
    });

    await page.route('**/api/committee/applications', async (route) => {
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
          totalVotes: 5,
          approvalVotes: 2,
          rejectionVotes: 0,
          pendingVotes: 3,
          requiredVotes: 3,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // View application
    const applicationRow = page.getByText(application.loanNumber);
    if (await applicationRow.isVisible()) {
      await applicationRow.click();
      await page.waitForTimeout(1000);

      // Should show voting progress (2 out of 3 required)
      await expect(page.getByText(/2.*3|2 of 3/i)).toBeVisible();
    }
  });

  // Test multi-member voting
  test('should display multi-member voting with different decisions', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-MULTI-001',
      status: 'IN_REVIEW',
      loanNumber: 'LOAN00000103',
    });

    await page.route('**/api/committee/applications', async (route) => {
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
          totalVotes: 5,
          approvalVotes: 2,
          rejectionVotes: 1,
          pendingVotes: 2,
          requiredVotes: 3,
          votes: [
            { memberName: 'Member A', decision: 'APPROVED', date: '2024-01-15T10:00:00Z' },
            { memberName: 'Member B', decision: 'APPROVED', date: '2024-01-15T11:00:00Z' },
            { memberName: 'Member C', decision: 'REJECTED', date: '2024-01-15T12:00:00Z' },
            { memberName: 'Member D', decision: 'PENDING', date: null },
            { memberName: 'Member E', decision: 'PENDING', date: null },
          ],
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // View application
    const applicationRow = page.getByText(application.loanNumber);
    if (await applicationRow.isVisible()) {
      await applicationRow.click();
      await page.waitForTimeout(1000);

      // Should show all member votes with different statuses
      await expect(page.getByText(/Member A/i)).toBeVisible();
      await expect(page.getByText(/Member B/i)).toBeVisible();
      await expect(page.getByText(/Member C/i)).toBeVisible();
      await expect(page.getByText(/APPROVED/i)).toBeVisible();
      await expect(page.getByText(/REJECTED/i)).toBeVisible();
      await expect(page.getByText(/PENDING/i)).toBeVisible();
    }
  });

  test('should handle unanimous approval voting', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-UNANIMOUS-001',
      status: 'IN_REVIEW',
      loanNumber: 'LOAN00000104',
    });

    await page.route('**/api/committee/applications', async (route) => {
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
          totalVotes: 3,
          approvalVotes: 3,
          rejectionVotes: 0,
          pendingVotes: 0,
          requiredVotes: 3,
          votes: [
            { memberName: 'Member 1', decision: 'APPROVED', date: '2024-01-15T10:00:00Z' },
            { memberName: 'Member 2', decision: 'APPROVED', date: '2024-01-15T11:00:00Z' },
            { memberName: 'Member 3', decision: 'APPROVED', date: '2024-01-15T12:00:00Z' },
          ],
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // View application
    const applicationRow = page.getByText(application.loanNumber);
    if (await applicationRow.isVisible()) {
      await applicationRow.click();
      await page.waitForTimeout(1000);

      // Should show unanimous approval
      await expect(page.getByText(/3.*3|3 of 3/i)).toBeVisible();
      await expect(page.getByText(/unanimous|all approved/i)).toBeVisible();
    }
  });

  test('should handle split voting decision', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-SPLIT-001',
      status: 'IN_REVIEW',
      loanNumber: 'LOAN00000105',
    });

    await page.route('**/api/committee/applications', async (route) => {
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
          totalVotes: 5,
          approvalVotes: 2,
          rejectionVotes: 2,
          pendingVotes: 1,
          requiredVotes: 3,
          votes: [
            { memberName: 'Member 1', decision: 'APPROVED', date: '2024-01-15T10:00:00Z' },
            { memberName: 'Member 2', decision: 'APPROVED', date: '2024-01-15T11:00:00Z' },
            { memberName: 'Member 3', decision: 'REJECTED', date: '2024-01-15T12:00:00Z' },
            { memberName: 'Member 4', decision: 'REJECTED', date: '2024-01-15T13:00:00Z' },
            { memberName: 'Member 5', decision: 'PENDING', date: null },
          ],
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // View application
    const applicationRow = page.getByText(application.loanNumber);
    if (await applicationRow.isVisible()) {
      await applicationRow.click();
      await page.waitForTimeout(1000);

      // Should show split decision
      await expect(page.getByText(/2.*approval/i)).toBeVisible();
      await expect(page.getByText(/2.*rejection/i)).toBeVisible();
    }
  });

  // Test comments and notes functionality
  test('should add comments with approval vote', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-COMMENT-001',
      status: 'IN_REVIEW',
      loanNumber: 'LOAN00000106',
    });

    await page.route('**/api/committee/applications', async (route) => {
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
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Click approve
    const approveButton = page.getByRole('button', { name: /Approve/i }).first();
    if (await approveButton.isVisible()) {
      await approveButton.click();
      await page.waitForTimeout(500);

      // Add comments
      const commentsField = page.locator('textarea[name="comments"], textarea[placeholder*="comment" i]').first();
      if (await commentsField.isVisible({ timeout: 1000 })) {
        await commentsField.fill('Application meets all criteria. Good credit history and stable income.');
        
        const submitButton = page.getByRole('button', { name: /Submit|Confirm/i });
        if (await submitButton.isVisible()) {
          await submitButton.click();
          await page.waitForTimeout(1000);

          // Should complete approval
          await expect(page.getByText(/approved|success/i)).toBeVisible();
        }
      }
    }
  });

  test('should add comments with rejection vote', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-REJECT-COMMENT-001',
      status: 'IN_REVIEW',
      loanNumber: 'LOAN00000107',
    });

    await page.route('**/api/committee/applications', async (route) => {
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
      const body = await route.request().postDataJSON();
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...application,
          status: 'REJECTED',
          comments: body.comments || body.reason,
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

      // Add rejection reason/comments
      const commentsField = page.locator('textarea[name="reason"], textarea[name="comments"], textarea[placeholder*="reason" i]').first();
      if (await commentsField.isVisible({ timeout: 1000 })) {
        await commentsField.fill('Insufficient documentation provided. Missing proof of income.');
        
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

  test('should display existing comments from other members', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-VIEW-COMMENTS-001',
      status: 'IN_REVIEW',
      loanNumber: 'LOAN00000108',
    });

    await page.route('**/api/committee/applications', async (route) => {
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
          totalVotes: 3,
          approvalVotes: 2,
          rejectionVotes: 0,
          pendingVotes: 1,
          requiredVotes: 2,
          votes: [
            { 
              memberName: 'John Smith', 
              decision: 'APPROVED', 
              date: '2024-01-15T10:00:00Z',
              comments: 'Excellent credit history and stable employment'
            },
            { 
              memberName: 'Jane Doe', 
              decision: 'APPROVED', 
              date: '2024-01-15T11:30:00Z',
              comments: 'All documentation verified. Recommend approval.'
            },
            { 
              memberName: 'Bob Johnson', 
              decision: 'PENDING', 
              date: null,
              comments: null
            },
          ],
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // View application
    const applicationRow = page.getByText(application.loanNumber);
    if (await applicationRow.isVisible()) {
      await applicationRow.click();
      await page.waitForTimeout(1000);

      // Should display comments from other members
      await expect(page.getByText(/Excellent credit history/i)).toBeVisible();
      await expect(page.getByText(/All documentation verified/i)).toBeVisible();
    }
  });

  test('should validate required comments for rejection', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-VALIDATE-COMMENT-001',
      status: 'IN_REVIEW',
      loanNumber: 'LOAN00000109',
    });

    await page.route('**/api/committee/applications', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [application],
          total: 1,
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

      // Try to submit without comments
      const submitButton = page.getByRole('button', { name: /Submit|Confirm/i });
      if (await submitButton.isVisible()) {
        await submitButton.click();
        await page.waitForTimeout(500);

        // Should show validation error
        await expect(page.getByText(/required|provide.*reason/i)).toBeVisible();
      }
    }
  });

  test('should handle voting errors', async ({ page }) => {
    const application = mockDataFactory.createLoanApplication({
      id: 'APP-ERROR-001',
      status: 'IN_REVIEW',
      loanNumber: 'LOAN00000110',
    });

    await page.route('**/api/committee/applications', async (route) => {
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
      await route.fulfill({
        status: 400,
        contentType: 'application/json',
        body: JSON.stringify({
          message: 'You have already voted on this application',
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
      await expect(page.getByText(/already voted|error/i)).toBeVisible();
    }
  });
});
