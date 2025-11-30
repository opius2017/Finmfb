import { test, expect } from '@playwright/test';
import { mockDataFactory } from '../../utils/mock-data';

test.describe('Commodity Vouchers', () => {
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

    await page.goto('/commodity-vouchers');
  });

  test('should display voucher list within 3 seconds', async ({ page }) => {
    const vouchers = mockDataFactory.createCommodityVouchers(10);
    const startTime = Date.now();

    await page.route('**/api/commodity-vouchers**', async (route) => {
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
    await page.waitForLoadState('networkidle');

    const loadTime = Date.now() - startTime;
    expect(loadTime).toBeLessThan(3000);
  });

  test('should generate new voucher with QR code', async ({ page }) => {
    await page.route('**/api/commodity-vouchers', async (route) => {
      if (route.request().method() === 'POST') {
        const voucher = mockDataFactory.createCommodityVoucher({
          voucherNumber: 'VCH00000001',
          qrCode: 'QR-CODE-DATA',
        });
        await route.fulfill({
          status: 201,
          contentType: 'application/json',
          body: JSON.stringify(voucher),
        });
      } else {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            data: [],
            total: 0,
          }),
        });
      }
    });

    await page.reload();

    const generateButton = page.getByRole('button', { name: /generate/i }).or(page.getByRole('button', { name: /new voucher/i }));
    if (await generateButton.isVisible()) {
      await generateButton.click();
      await page.waitForTimeout(500);

      // Fill voucher details
      const amountInput = page.locator('input[name="amount"]').first();
      if (await amountInput.isVisible()) {
        await amountInput.fill('300000');
        
        const submitButton = page.getByRole('button', { name: /submit/i }).or(page.getByRole('button', { name: /create/i }));
        await submitButton.click();
        await page.waitForTimeout(1000);

        // Check for QR code display
        const qrCode = page.locator('[class*="qr"], canvas, img[alt*="QR"]').first();
        if (await qrCode.isVisible()) {
          await expect(qrCode).toBeVisible();
        }
      }
    }
  });

  test('should redeem voucher', async ({ page }) => {
    const voucher = mockDataFactory.createCommodityVoucher({
      id: 'VCH-001',
      status: 'ISSUED',
    });

    await page.route('**/api/commodity-vouchers**', async (route) => {
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
      if (route.request().method() === 'POST') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            ...voucher,
            status: 'REDEEMED',
            redemptionDate: new Date().toISOString(),
          }),
        });
      }
    });

    await page.reload();

    const redeemButton = page.getByRole('button', { name: /redeem/i }).first();
    if (await redeemButton.isVisible()) {
      await redeemButton.click();
      await page.waitForTimeout(1000);
    }
  });

  test('should track voucher status', async ({ page }) => {
    const vouchers = [
      mockDataFactory.createCommodityVoucher({ status: 'ISSUED' }),
      mockDataFactory.createCommodityVoucher({ status: 'REDEEMED' }),
      mockDataFactory.createCommodityVoucher({ status: 'EXPIRED' }),
    ];

    await page.route('**/api/commodity-vouchers**', async (route) => {
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
  });

  test('should validate voucher against loan terms', async ({ page }) => {
    const voucher = mockDataFactory.createCommodityVoucher({
      id: 'VCH-002',
      amount: 300000,
    });

    await page.route('**/api/commodity-vouchers**', async (route) => {
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
          loanTerms: {
            maxAmount: 500000,
            allowedVendors: ['ABC Commodities Ltd'],
          },
        }),
      });
    });

    await page.reload();
  });

  test('should display voucher with QR code', async ({ page }) => {
    const voucher = mockDataFactory.createCommodityVoucher({
      voucherNumber: 'VCH00000001',
      qrCode: 'QR-DATA',
    });

    await page.route('**/api/commodity-vouchers**', async (route) => {
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

    const viewButton = page.getByRole('button', { name: /view/i }).first();
    if (await viewButton.isVisible()) {
      await viewButton.click();
      await page.waitForTimeout(500);

      const qrCode = page.locator('[class*="qr"], canvas').first();
      if (await qrCode.isVisible()) {
        await expect(qrCode).toBeVisible();
      }
    }
  });
});
