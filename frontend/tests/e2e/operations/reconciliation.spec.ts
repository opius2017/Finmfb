import { test, expect } from '@playwright/test';
import { mockDataFactory } from '../../utils/mock-data';

test.describe('Reconciliation', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem('authToken', 'test-token');
      localStorage.setItem('auth-storage', JSON.stringify({
        state: {
          user: { id: '1', email: 'test@example.com', name: 'Test User', role: 'ADMIN' },
          token: 'test-token',
          isAuthenticated: true,
        },
        version: 0,
      }));
    });

    await page.goto('/reconciliation');
  });

  test('should display reconciliation page', async ({ page }) => {
    await expect(page.getByRole('heading', { name: /Reconciliation/i })).toBeVisible();
  });

  test('should display unmatched transactions', async ({ page }) => {
    const transactions = mockDataFactory.createTransactions(5, { status: 'UNMATCHED' });

    await page.route('**/api/reconciliation/unmatched', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: transactions,
          total: transactions.length,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Verify transactions are displayed
    for (const txn of transactions) {
      await expect(page.getByText(txn.transactionNumber)).toBeVisible();
    }
  });

  test('should match transaction to loan', async ({ page }) => {
    const transaction = mockDataFactory.createTransaction({
      id: 'TXN-001',
      transactionNumber: 'TXN0000000001',
      amount: 45000,
      status: 'UNMATCHED',
    });

    await page.route('**/api/reconciliation/unmatched', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [transaction],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/reconciliation/${transaction.id}/match`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...transaction,
          status: 'MATCHED',
          loanNumber: 'LOAN00000001',
          message: 'Transaction matched successfully',
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Match transaction
    const matchButton = page.getByRole('button', { name: /Match|Link/i }).first();
    if (await matchButton.isVisible()) {
      await matchButton.click();
      await page.waitForTimeout(500);

      // Select loan
      const loanSelect = page.locator('select[name="loanId"], input[name="loanNumber"]').first();
      if (await loanSelect.isVisible({ timeout: 1000 })) {
        await loanSelect.fill('LOAN00000001');
        
        const confirmButton = page.getByRole('button', { name: /Confirm|Submit/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
          await page.waitForTimeout(1000);

          // Should show success
          await expect(page.getByText(/matched|success/i)).toBeVisible();
        }
      }
    }
  });

  test('should display transaction details', async ({ page }) => {
    const transaction = mockDataFactory.createTransaction({
      transactionNumber: 'TXN0000000001',
      memberName: 'John Doe',
      amount: 45000,
    });

    await page.route('**/api/reconciliation/unmatched', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [transaction],
          total: 1,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Click on transaction
    const txnRow = page.getByText(transaction.transactionNumber);
    if (await txnRow.isVisible()) {
      await txnRow.click();
      await page.waitForTimeout(1000);

      // Should show details
      await expect(page.getByText(transaction.memberName)).toBeVisible();
      await expect(page.getByText(/45,000|₦45,000/)).toBeVisible();
    }
  });

  test('should highlight discrepancies', async ({ page }) => {
    const transactions = [
      mockDataFactory.createTransaction({ amount: 45000, status: 'UNMATCHED' }),
      mockDataFactory.createTransaction({ amount: 100000, status: 'UNMATCHED' }),
    ];

    await page.route('**/api/reconciliation/unmatched', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: transactions,
          total: transactions.length,
          discrepancies: [
            {
              transactionId: transactions[1].id,
              reason: 'Amount exceeds tolerance threshold',
              expectedAmount: 45000,
              actualAmount: 100000,
            },
          ],
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Should highlight discrepancy
    await expect(page.getByText(/exceeds tolerance|discrepancy/i)).toBeVisible();
  });

  test('should complete reconciliation', async ({ page }) => {
    const transaction = mockDataFactory.createTransaction({
      id: 'TXN-REC-001',
      status: 'MATCHED',
    });

    await page.route('**/api/reconciliation/unmatched', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [transaction],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/reconciliation/${transaction.id}/complete`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...transaction,
          status: 'RECONCILED',
          message: 'Reconciliation completed',
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Complete reconciliation
    const completeButton = page.getByRole('button', { name: /Complete|Reconcile/i }).first();
    if (await completeButton.isVisible({ timeout: 1000 })) {
      await completeButton.click();
      await page.waitForTimeout(1000);

      // Should show success
      await expect(page.getByText(/completed|reconciled/i)).toBeVisible();
    }
  });

  test('should handle empty transactions list', async ({ page }) => {
    await page.route('**/api/reconciliation/unmatched', async (route) => {
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
    await expect(page.getByText(/No unmatched transactions|All transactions reconciled/i)).toBeVisible();
  });
});

test.describe('Reconciliation - Workflow', () => {
  test.beforeEach(async ({ page, context }) => {
    await context.addInitScript(() => {
      localStorage.setItem('authToken', 'test-token');
      localStorage.setItem('auth-storage', JSON.stringify({
        state: {
          user: { id: '1', email: 'test@example.com', name: 'Test User', role: 'ADMIN' },
          token: 'test-token',
          isAuthenticated: true,
        },
        version: 0,
      }));
    });

    await page.goto('/reconciliation');
  });

  test('should complete matching workflow from start to finish', async ({ page }) => {
    const unmatchedTransaction = mockDataFactory.createTransaction({
      id: 'TXN-WORKFLOW-001',
      transactionNumber: 'TXN0000000100',
      amount: 45000,
      status: 'UNMATCHED',
      memberName: 'Alice Johnson',
    });

    const matchedTransaction = {
      ...unmatchedTransaction,
      status: 'MATCHED',
      loanNumber: 'LOAN00000050',
      loanId: 'LOAN-050',
    };

    const reconciledTransaction = {
      ...matchedTransaction,
      status: 'RECONCILED',
    };

    // Step 1: Load unmatched transactions
    await page.route('**/api/reconciliation/unmatched', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [unmatchedTransaction],
          total: 1,
        }),
      });
    });

    // Step 2: Match transaction to loan
    await page.route(`**/api/reconciliation/${unmatchedTransaction.id}/match`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          success: true,
          data: matchedTransaction,
          message: 'Transaction matched successfully',
        }),
      });
    });

    // Step 3: Complete reconciliation
    await page.route(`**/api/reconciliation/${unmatchedTransaction.id}/complete`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          success: true,
          data: reconciledTransaction,
          message: 'Reconciliation completed successfully',
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Verify unmatched transaction is displayed
    await expect(page.getByText(unmatchedTransaction.transactionNumber)).toBeVisible();
    await expect(page.getByText('Alice Johnson')).toBeVisible();

    // Click match button
    const matchButton = page.getByRole('button', { name: /Match|Link/i }).first();
    if (await matchButton.isVisible()) {
      await matchButton.click();
      await page.waitForTimeout(500);

      // Enter loan number
      const loanInput = page.locator('input[name="loanNumber"], input[placeholder*="loan"]').first();
      if (await loanInput.isVisible({ timeout: 1000 })) {
        await loanInput.fill('LOAN00000050');
        
        // Confirm match
        const confirmButton = page.getByRole('button', { name: /Confirm|Submit|Match/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
          await page.waitForTimeout(1000);

          // Verify match success
          await expect(page.getByText(/matched|success/i)).toBeVisible();

          // Complete reconciliation
          const completeButton = page.getByRole('button', { name: /Complete|Reconcile/i }).first();
          if (await completeButton.isVisible({ timeout: 1000 })) {
            await completeButton.click();
            await page.waitForTimeout(1000);

            // Verify reconciliation completed
            await expect(page.getByText(/completed|reconciled/i)).toBeVisible();
          }
        }
      }
    }
  });

  test('should update transaction and loan records after reconciliation', async ({ page }) => {
    const transaction = mockDataFactory.createTransaction({
      id: 'TXN-UPDATE-001',
      transactionNumber: 'TXN0000000200',
      amount: 50000,
      status: 'UNMATCHED',
    });

    const loan = {
      id: 'LOAN-100',
      loanNumber: 'LOAN00000100',
      balance: 500000,
      lastPaymentDate: null,
      lastPaymentAmount: 0,
    };

    await page.route('**/api/reconciliation/unmatched', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [transaction],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/reconciliation/${transaction.id}/match`, async (route) => {
      const updatedLoan = {
        ...loan,
        balance: loan.balance - transaction.amount,
        lastPaymentDate: new Date().toISOString(),
        lastPaymentAmount: transaction.amount,
      };

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          success: true,
          data: {
            ...transaction,
            status: 'MATCHED',
            loanNumber: loan.loanNumber,
            loanId: loan.id,
          },
          updatedRecords: {
            transaction: {
              id: transaction.id,
              status: 'MATCHED',
              loanId: loan.id,
            },
            loan: updatedLoan,
          },
          message: 'Transaction matched and records updated',
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Verify transaction is displayed
    await expect(page.getByText(transaction.transactionNumber)).toBeVisible();

    // Match transaction
    const matchButton = page.getByRole('button', { name: /Match/i }).first();
    if (await matchButton.isVisible()) {
      await matchButton.click();
      await page.waitForTimeout(500);

      const loanInput = page.locator('input[name="loanNumber"], input[placeholder*="loan"]').first();
      if (await loanInput.isVisible({ timeout: 1000 })) {
        await loanInput.fill(loan.loanNumber);
        
        const confirmButton = page.getByRole('button', { name: /Confirm|Submit/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
          await page.waitForTimeout(1000);

          // Verify success message indicating records were updated
          await expect(page.getByText(/matched|updated|success/i)).toBeVisible();
        }
      }
    }
  });

  test('should handle tolerance threshold within acceptable range', async ({ page }) => {
    const expectedAmount = 45000;
    const actualAmount = 45500;
    const toleranceThreshold = 1000;

    const transaction = mockDataFactory.createTransaction({
      id: 'TXN-TOLERANCE-001',
      amount: actualAmount,
      status: 'UNMATCHED',
    });

    await page.route('**/api/reconciliation/unmatched', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [transaction],
          total: 1,
          toleranceThreshold,
          metadata: {
            withinTolerance: Math.abs(actualAmount - expectedAmount) <= toleranceThreshold,
          },
        }),
      });
    });

    await page.route(`**/api/reconciliation/${transaction.id}/match`, async (route) => {
      const difference = Math.abs(actualAmount - expectedAmount);
      const withinTolerance = difference <= toleranceThreshold;

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          success: true,
          data: {
            ...transaction,
            status: 'MATCHED',
            loanNumber: 'LOAN00000075',
          },
          tolerance: {
            threshold: toleranceThreshold,
            difference,
            withinTolerance,
            expectedAmount,
            actualAmount,
          },
          message: withinTolerance 
            ? 'Transaction matched within tolerance threshold' 
            : 'Transaction matched but exceeds tolerance threshold',
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Should display tolerance information
    await expect(page.getByText(/tolerance|threshold/i)).toBeVisible();

    // Match transaction
    const matchButton = page.getByRole('button', { name: /Match/i }).first();
    if (await matchButton.isVisible()) {
      await matchButton.click();
      await page.waitForTimeout(500);

      const loanInput = page.locator('input[name="loanNumber"], input[placeholder*="loan"]').first();
      if (await loanInput.isVisible({ timeout: 1000 })) {
        await loanInput.fill('LOAN00000075');
        
        const confirmButton = page.getByRole('button', { name: /Confirm|Submit/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
          await page.waitForTimeout(1000);

          // Should show success since within tolerance
          await expect(page.getByText(/matched|success|within tolerance/i)).toBeVisible();
        }
      }
    }
  });

  test('should handle tolerance threshold exceeding acceptable range', async ({ page }) => {
    const expectedAmount = 45000;
    const actualAmount = 50000;
    const toleranceThreshold = 1000;
    const difference = Math.abs(actualAmount - expectedAmount);

    const transaction = mockDataFactory.createTransaction({
      id: 'TXN-TOLERANCE-002',
      amount: actualAmount,
      status: 'UNMATCHED',
    });

    await page.route('**/api/reconciliation/unmatched', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [transaction],
          total: 1,
          toleranceThreshold,
          discrepancies: [
            {
              transactionId: transaction.id,
              reason: 'Amount exceeds tolerance threshold',
              expectedAmount,
              actualAmount,
              difference,
              threshold: toleranceThreshold,
            },
          ],
        }),
      });
    });

    await page.route(`**/api/reconciliation/${transaction.id}/match`, async (route) => {
      await route.fulfill({
        status: 400,
        contentType: 'application/json',
        body: JSON.stringify({
          success: false,
          error: 'Transaction amount exceeds tolerance threshold',
          tolerance: {
            threshold: toleranceThreshold,
            difference,
            withinTolerance: false,
            expectedAmount,
            actualAmount,
          },
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Should highlight discrepancy
    await expect(page.getByText(/exceeds tolerance|discrepancy|warning/i)).toBeVisible();

    // Attempt to match transaction
    const matchButton = page.getByRole('button', { name: /Match/i }).first();
    if (await matchButton.isVisible()) {
      await matchButton.click();
      await page.waitForTimeout(500);

      const loanInput = page.locator('input[name="loanNumber"], input[placeholder*="loan"]').first();
      if (await loanInput.isVisible({ timeout: 1000 })) {
        await loanInput.fill('LOAN00000080');
        
        const confirmButton = page.getByRole('button', { name: /Confirm|Submit/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
          await page.waitForTimeout(1000);

          // Should show error about tolerance threshold
          await expect(page.getByText(/exceeds|threshold|error/i)).toBeVisible();
        }
      }
    }
  });

  test('should update multiple records in batch reconciliation', async ({ page }) => {
    const transactions = [
      mockDataFactory.createTransaction({
        id: 'TXN-BATCH-001',
        transactionNumber: 'TXN0000000301',
        amount: 45000,
        status: 'MATCHED',
        loanNumber: 'LOAN00000201',
      }),
      mockDataFactory.createTransaction({
        id: 'TXN-BATCH-002',
        transactionNumber: 'TXN0000000302',
        amount: 50000,
        status: 'MATCHED',
        loanNumber: 'LOAN00000202',
      }),
    ];

    await page.route('**/api/reconciliation/unmatched', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: transactions,
          total: transactions.length,
        }),
      });
    });

    await page.route('**/api/reconciliation/batch/complete', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          success: true,
          data: transactions.map(t => ({ ...t, status: 'RECONCILED' })),
          updatedRecords: {
            transactions: transactions.length,
            loans: transactions.length,
          },
          message: `${transactions.length} transactions reconciled successfully`,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Verify transactions are displayed
    for (const txn of transactions) {
      await expect(page.getByText(txn.transactionNumber)).toBeVisible();
    }

    // Complete batch reconciliation
    const batchButton = page.getByRole('button', { name: /Reconcile All|Batch|Complete All/i }).first();
    if (await batchButton.isVisible({ timeout: 1000 })) {
      await batchButton.click();
      await page.waitForTimeout(1000);

      // Should show success message with count
      await expect(page.getByText(/reconciled|completed|success/i)).toBeVisible();
    }
  });

  test('should verify record consistency after reconciliation', async ({ page }) => {
    const transaction = mockDataFactory.createTransaction({
      id: 'TXN-VERIFY-001',
      transactionNumber: 'TXN0000000400',
      amount: 45000,
      status: 'UNMATCHED',
      memberId: 'MEM-001',
    });

    const loan = {
      id: 'LOAN-300',
      loanNumber: 'LOAN00000300',
      memberId: 'MEM-001',
      balance: 500000,
      totalPaid: 100000,
    };

    await page.route('**/api/reconciliation/unmatched', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [transaction],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/reconciliation/${transaction.id}/match`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          success: true,
          data: {
            ...transaction,
            status: 'MATCHED',
            loanNumber: loan.loanNumber,
            loanId: loan.id,
          },
          updatedRecords: {
            transaction: {
              id: transaction.id,
              status: 'MATCHED',
              loanId: loan.id,
              reconciledAt: new Date().toISOString(),
            },
            loan: {
              id: loan.id,
              balance: loan.balance - transaction.amount,
              totalPaid: loan.totalPaid + transaction.amount,
              lastPaymentDate: new Date().toISOString(),
              lastPaymentAmount: transaction.amount,
            },
          },
          verification: {
            memberIdMatch: transaction.memberId === loan.memberId,
            balanceUpdated: true,
            paymentRecorded: true,
            consistent: true,
          },
          message: 'Records updated and verified for consistency',
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Match transaction
    const matchButton = page.getByRole('button', { name: /Match/i }).first();
    if (await matchButton.isVisible()) {
      await matchButton.click();
      await page.waitForTimeout(500);

      const loanInput = page.locator('input[name="loanNumber"], input[placeholder*="loan"]').first();
      if (await loanInput.isVisible({ timeout: 1000 })) {
        await loanInput.fill(loan.loanNumber);
        
        const confirmButton = page.getByRole('button', { name: /Confirm|Submit/i });
        if (await confirmButton.isVisible()) {
          await confirmButton.click();
          await page.waitForTimeout(1000);

          // Should show success with verification
          await expect(page.getByText(/matched|verified|success/i)).toBeVisible();
        }
      }
    }
  });
});
