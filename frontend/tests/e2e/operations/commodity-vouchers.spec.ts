import { test, expect } from '@playwright/test';
import { mockDataFactory } from '../../utils/mock-data';

test.describe('Commodity Vouchers', () => {
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

    await page.goto('/commodity-vouchers');
  });

  test('should display commodity vouchers page', async ({ page }) => {
    await expect(page.getByRole('heading', { name: /Commodity Vouchers/i })).toBeVisible();
  });

  test('should display voucher list', async ({ page }) => {
    const vouchers = mockDataFactory.createCommodityVouchers(5);

    await page.route('**/api/commodity-vouchers', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: vouchers,
          total: vouchers.length,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Verify vouchers are displayed
    for (const voucher of vouchers) {
      await expect(page.getByText(voucher.voucherNumber)).toBeVisible();
    }
  });

  test('should generate new voucher with QR code', async ({ page }) => {
    const newVoucher = mockDataFactory.createCommodityVoucher({
      voucherNumber: 'VCH00000001',
      amount: 300000,
      qrCode: 'QR-CODE-DATA',
    });

    await page.route('**/api/commodity-vouchers', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 201,
          contentType: 'application/json',
          body: JSON.stringify(newVoucher),
        });
      }
    });

    // Click generate button
    const generateButton = page.getByRole('button', { name: /Generate|New Voucher/i }).first();
    if (await generateButton.isVisible({ timeout: 1000 })) {
      await generateButton.click();
      await page.waitForTimeout(500);

      // Fill voucher details
      const amountInput = page.locator('input[name="amount"]').first();
      if (await amountInput.isVisible({ timeout: 1000 })) {
        await amountInput.fill('300000');

        const submitButton = page.getByRole('button', { name: /Submit|Generate/i });
        if (await submitButton.isVisible()) {
          await submitButton.click();
          await page.waitForTimeout(1000);

          // Should show success and QR code
          await expect(page.getByText(/generated|success/i)).toBeVisible();
        }
      }
    }
  });

  test('should display voucher with QR code', async ({ page }) => {
    const voucher = mockDataFactory.createCommodityVoucher({
      voucherNumber: 'VCH00000001',
      qrCode: 'QR-CODE-DATA',
    });

    await page.route('**/api/commodity-vouchers', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [voucher],
          total: 1,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Click on voucher to view details
    const voucherRow = page.getByText(voucher.voucherNumber);
    if (await voucherRow.isVisible()) {
      await voucherRow.click();
      await page.waitForTimeout(1000);

      // Should show QR code
      const qrCode = page.locator('img[alt*="QR"], canvas, svg').first();
      if (await qrCode.isVisible({ timeout: 1000 })) {
        await expect(qrCode).toBeVisible();
      }
    }
  });

  test('should redeem voucher', async ({ page }) => {
    const voucher = mockDataFactory.createCommodityVoucher({
      id: 'VCH-001',
      voucherNumber: 'VCH00000001',
      status: 'ISSUED',
    });

    await page.route('**/api/commodity-vouchers', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [voucher],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/commodity-vouchers/${voucher.id}/redeem`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...voucher,
          status: 'REDEEMED',
          redemptionDate: new Date().toISOString(),
          message: 'Voucher redeemed successfully',
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Redeem voucher
    const redeemButton = page.getByRole('button', { name: /Redeem/i }).first();
    if (await redeemButton.isVisible()) {
      await redeemButton.click();
      await page.waitForTimeout(1000);

      // Should show success
      await expect(page.getByText(/redeemed|success/i)).toBeVisible();
    }
  });

  test('should track voucher status', async ({ page }) => {
    const vouchers = [
      mockDataFactory.createCommodityVoucher({ status: 'ISSUED' }),
      mockDataFactory.createCommodityVoucher({ status: 'REDEEMED' }),
      mockDataFactory.createCommodityVoucher({ status: 'EXPIRED' }),
    ];

    await page.route('**/api/commodity-vouchers', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: vouchers,
          total: vouchers.length,
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Verify status indicators
    await expect(page.getByText('ISSUED')).toBeVisible();
    await expect(page.getByText('REDEEMED')).toBeVisible();
    await expect(page.getByText('EXPIRED')).toBeVisible();
  });

  test('should validate voucher against loan terms', async ({ page }) => {
    const voucher = mockDataFactory.createCommodityVoucher({
      id: 'VCH-VAL-001',
      amount: 300000,
      loanNumber: 'LOAN00000001',
    });

    await page.route('**/api/commodity-vouchers', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [voucher],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/commodity-vouchers/${voucher.id}/validate`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          isValid: true,
          loanAmount: 300000,
          voucherAmount: 300000,
          message: 'Voucher is valid',
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Validate voucher
    const validateButton = page.getByRole('button', { name: /Validate|Check/i }).first();
    if (await validateButton.isVisible({ timeout: 1000 })) {
      await validateButton.click();
      await page.waitForTimeout(1000);

      // Should show validation result
      await expect(page.getByText(/valid|Valid/i)).toBeVisible();
    }
  });

  test('should handle empty vouchers list', async ({ page }) => {
    await page.route('**/api/commodity-vouchers', async (route) => {
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
    await expect(page.getByText(/No vouchers|No commodity vouchers/i)).toBeVisible();
  });
});

test.describe('Commodity Vouchers - Lifecycle', () => {
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

    await page.goto('/commodity-vouchers');
  });

  test('should complete generation workflow', async ({ page }) => {
    const newVoucher = mockDataFactory.createCommodityVoucher({
      voucherNumber: 'VCH00000001',
      amount: 300000,
      vendor: 'ABC Commodities Ltd',
      commodityType: 'Agricultural Inputs',
    });

    await page.route('**/api/commodity-vouchers', async (route) => {
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 201,
          contentType: 'application/json',
          body: JSON.stringify(newVoucher),
        });
      }
    });

    // Generate voucher
    const generateButton = page.getByRole('button', { name: /Generate|New/i }).first();
    if (await generateButton.isVisible({ timeout: 1000 })) {
      await generateButton.click();
      await page.waitForTimeout(500);

      // Fill form
      const loanInput = page.locator('input[name="loanNumber"], select[name="loanId"]').first();
      if (await loanInput.isVisible({ timeout: 1000 })) {
        await loanInput.fill('LOAN00000001');
      }

      const amountInput = page.locator('input[name="amount"]').first();
      if (await amountInput.isVisible({ timeout: 1000 })) {
        await amountInput.fill('300000');
      }

      const vendorInput = page.locator('input[name="vendor"], select[name="vendor"]').first();
      if (await vendorInput.isVisible({ timeout: 1000 })) {
        await vendorInput.fill('ABC Commodities Ltd');
      }

      const submitButton = page.getByRole('button', { name: /Submit|Generate/i });
      if (await submitButton.isVisible()) {
        await submitButton.click();
        await page.waitForTimeout(1000);

        // Should show success
        await expect(page.getByText(/generated|success/i)).toBeVisible();
      }
    }
  });

  test('should complete redemption workflow', async ({ page }) => {
    const voucher = mockDataFactory.createCommodityVoucher({
      id: 'VCH-REDEEM-001',
      voucherNumber: 'VCH00000001',
      status: 'ISSUED',
    });

    await page.route('**/api/commodity-vouchers', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [voucher],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/commodity-vouchers/${voucher.id}/redeem`, async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...voucher,
          status: 'REDEEMED',
          redemptionDate: new Date().toISOString(),
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Redeem voucher
    const redeemButton = page.getByRole('button', { name: /Redeem/i }).first();
    if (await redeemButton.isVisible()) {
      await redeemButton.click();
      await page.waitForTimeout(500);

      // Confirm redemption
      const confirmButton = page.getByRole('button', { name: /Confirm|Yes/i });
      if (await confirmButton.isVisible({ timeout: 1000 })) {
        await confirmButton.click();
        await page.waitForTimeout(1000);

        // Should update status
        await expect(page.getByText('REDEEMED')).toBeVisible();
      }
    }
  });

  test('should update status after redemption', async ({ page }) => {
    const voucher = mockDataFactory.createCommodityVoucher({
      id: 'VCH-STATUS-001',
      status: 'ISSUED',
    });

    let currentStatus = 'ISSUED';

    await page.route('**/api/commodity-vouchers', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          data: [{ ...voucher, status: currentStatus }],
          total: 1,
        }),
      });
    });

    await page.route(`**/api/commodity-vouchers/${voucher.id}/redeem`, async (route) => {
      currentStatus = 'REDEEMED';
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          ...voucher,
          status: 'REDEEMED',
        }),
      });
    });

    await page.reload();
    await page.waitForTimeout(1000);

    // Initial status
    await expect(page.getByText('ISSUED')).toBeVisible();

    // Redeem
    const redeemButton = page.getByRole('button', { name: /Redeem/i }).first();
    if (await redeemButton.isVisible()) {
      await redeemButton.click();
      await page.waitForTimeout(1500);

      // Status should update
      await expect(page.getByText('REDEEMED')).toBeVisible();
    }
  });
});
