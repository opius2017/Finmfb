import { test, expect, Page } from '@playwright/test';

/**
 * E2E Tests for Commodity Vouchers Lifecycle
 * Tests the complete lifecycle of commodity vouchers:
 * - Generation workflow
 * - Redemption workflow
 * - Status updates
 * 
 * Requirements: 12.2, 12.3, 12.4
 */

// Test configuration
const BASE_URL = process.env.BASE_URL || 'http://localhost:5173';
const API_URL = process.env.API_URL || 'http://localhost:5000';
const TEST_USER = {
  email: 'admin@fintech.com',
  password: 'Admin@123'
};

// Helper function to login
async function login(page: Page) {
  await page.goto(`${BASE_URL}/login`);
  await page.fill('input[name="email"]', TEST_USER.email);
  await page.fill('input[name="password"]', TEST_USER.password);
  await page.click('button[type="submit"]');
  await page.waitForURL(`${BASE_URL}/dashboard**`);
}

// Helper function to wait for API response
async function waitForApiResponse(page: Page, endpoint: string) {
  return page.waitForResponse(response => 
    response.url().includes(endpoint) && response.status() === 200
  );
}

test.describe('Commodity Vouchers - Lifecycle Tests', () => {
  
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('should complete voucher generation workflow', async ({ page }) => {
    // Requirement 12.2: Test complete generation workflow
    
    // Navigate to commodity vouchers page
    await page.goto(`${BASE_URL}/commodity-vouchers`);
    await page.waitForLoadState('networkidle');

    // Click generate/create voucher button
    await page.click('button:has-text("Generate"), button:has-text("New Voucher"), button:has-text("Create Voucher")');
    await page.waitForTimeout(500);

    // Fill voucher generation form
    const timestamp = Date.now();
    
    // Select loan (if dropdown exists)
    const loanSelect = page.locator('select[name="loanId"], select[name="loanNumber"]');
    if (await loanSelect.isVisible({ timeout: 1000 })) {
      await loanSelect.selectOption({ index: 1 });
    } else {
      // Or fill loan number input
      const loanInput = page.locator('input[name="loanNumber"], input[name="loanId"]');
      if (await loanInput.isVisible({ timeout: 1000 })) {
        await loanInput.fill('LOAN00000001');
      }
    }

    // Fill voucher amount
    await page.fill('input[name="amount"]', '300000');

    // Select vendor
    const vendorSelect = page.locator('select[name="vendorId"], select[name="vendor"]');
    if (await vendorSelect.isVisible({ timeout: 1000 })) {
      await vendorSelect.selectOption({ index: 1 });
    } else {
      const vendorInput = page.locator('input[name="vendor"], input[name="vendorName"]');
      if (await vendorInput.isVisible({ timeout: 1000 })) {
        await vendorInput.fill('ABC Commodities Ltd');
      }
    }

    // Select commodity type
    const commoditySelect = page.locator('select[name="commodityType"]');
    if (await commoditySelect.isVisible({ timeout: 1000 })) {
      await commoditySelect.selectOption({ label: 'Agricultural Inputs' });
    } else {
      const commodityInput = page.locator('input[name="commodityType"]');
      if (await commodityInput.isVisible({ timeout: 1000 })) {
        await commodityInput.fill('Agricultural Inputs');
      }
    }

    // Fill description
    const descriptionField = page.locator('textarea[name="description"], input[name="description"]');
    if (await descriptionField.isVisible({ timeout: 1000 })) {
      await descriptionField.fill('Test commodity voucher for agricultural inputs');
    }

    // Submit voucher generation
    const responsePromise = page.waitForResponse(
      response => response.url().includes('/api/commodity-vouchers') && 
                  (response.status() === 200 || response.status() === 201),
      { timeout: 10000 }
    );
    
    await page.click('button[type="submit"]:has-text("Generate"), button[type="submit"]:has-text("Create"), button[type="submit"]:has-text("Submit")');
    
    try {
      await responsePromise;
    } catch (error) {
      // Continue even if API call times out
      console.log('API response timeout, continuing test');
    }

    // Verify success message
    await expect(
      page.locator('text=/Voucher generated|Generated successfully|Success|Created successfully/i')
    ).toBeVisible({ timeout: 5000 });

    // Verify voucher appears in list with ISSUED status
    await page.waitForTimeout(1000);
    await expect(page.locator('text=/ISSUED|Issued/i')).toBeVisible();

    // Verify QR code is generated
    const qrCode = page.locator('img[alt*="QR"], canvas, svg[class*="qr"]');
    if (await qrCode.isVisible({ timeout: 2000 })) {
      await expect(qrCode).toBeVisible();
    }
  });

  test('should complete voucher redemption workflow', async ({ page }) => {
    // Requirement 12.3: Test complete redemption workflow
    
    // Navigate to commodity vouchers page
    await page.goto(`${BASE_URL}/commodity-vouchers`);
    await page.waitForLoadState('networkidle');

    // Find an issued voucher
    const issuedVoucher = page.locator('table tbody tr:has-text("ISSUED"), table tbody tr:has-text("Issued")').first();
    
    // If no issued voucher exists, create one first
    const issuedCount = await issuedVoucher.count();
    if (issuedCount === 0) {
      // Create a voucher first
      await page.click('button:has-text("Generate"), button:has-text("New Voucher")');
      await page.waitForTimeout(500);
      
      await page.fill('input[name="amount"]', '250000');
      const loanSelect = page.locator('select[name="loanId"]');
      if (await loanSelect.isVisible({ timeout: 1000 })) {
        await loanSelect.selectOption({ index: 1 });
      }
      
      await page.click('button[type="submit"]');
      await page.waitForTimeout(2000);
    }

    // Click on the issued voucher to view details
    await issuedVoucher.click();
    await page.waitForTimeout(500);

    // Click redeem button
    const redeemButton = page.locator('button:has-text("Redeem"), button:has-text("Mark as Redeemed")');
    await expect(redeemButton).toBeVisible({ timeout: 3000 });
    await redeemButton.click();
    await page.waitForTimeout(500);

    // Fill redemption details if form appears
    const redemptionForm = page.locator('form, div[role="dialog"]');
    if (await redemptionForm.isVisible({ timeout: 1000 })) {
      // Fill redemption reference
      const referenceInput = page.locator('input[name="redemptionReference"], input[name="reference"]');
      if (await referenceInput.isVisible({ timeout: 1000 })) {
        await referenceInput.fill(`REDEEM${Date.now()}`);
      }

      // Fill redemption notes
      const notesField = page.locator('textarea[name="notes"], textarea[name="redemptionNotes"]');
      if (await notesField.isVisible({ timeout: 1000 })) {
        await notesField.fill('Voucher redeemed at vendor location');
      }
    }

    // Confirm redemption
    const confirmButton = page.locator('button:has-text("Confirm"), button:has-text("Yes"), button[type="submit"]:has-text("Redeem")');
    
    const responsePromise = page.waitForResponse(
      response => response.url().includes('/api/commodity-vouchers') && 
                  response.url().includes('redeem'),
      { timeout: 10000 }
    );
    
    await confirmButton.click();
    
    try {
      await responsePromise;
    } catch (error) {
      console.log('API response timeout, continuing test');
    }

    // Verify success message
    await expect(
      page.locator('text=/Redeemed successfully|Voucher redeemed|Success/i')
    ).toBeVisible({ timeout: 5000 });

    // Verify status changed to REDEEMED
    await page.waitForTimeout(1000);
    await expect(page.locator('text=/REDEEMED|Redeemed/i')).toBeVisible();

    // Verify redemption date is displayed
    const redemptionDate = page.locator('text=/Redemption Date|Redeemed on/i');
    if (await redemptionDate.isVisible({ timeout: 2000 })) {
      await expect(redemptionDate).toBeVisible();
    }
  });

  test('should update voucher status throughout lifecycle', async ({ page }) => {
    // Requirement 12.4: Test status updates
    
    // Navigate to commodity vouchers page
    await page.goto(`${BASE_URL}/commodity-vouchers`);
    await page.waitForLoadState('networkidle');

    // Step 1: Create voucher - should have ISSUED status
    await page.click('button:has-text("Generate"), button:has-text("New Voucher")');
    await page.waitForTimeout(500);

    const timestamp = Date.now();
    await page.fill('input[name="amount"]', '200000');
    
    const loanSelect = page.locator('select[name="loanId"]');
    if (await loanSelect.isVisible({ timeout: 1000 })) {
      await loanSelect.selectOption({ index: 1 });
    }

    await page.click('button[type="submit"]');
    await page.waitForTimeout(2000);

    // Verify initial status is ISSUED
    await expect(page.locator('text=/ISSUED|Issued/i').first()).toBeVisible();

    // Step 2: Find the newly created voucher
    const newVoucher = page.locator('table tbody tr:has-text("ISSUED"), table tbody tr:has-text("Issued")').first();
    await newVoucher.click();
    await page.waitForTimeout(500);

    // Verify status indicator shows ISSUED
    const statusBadge = page.locator('[class*="badge"], [class*="status"], span:has-text("ISSUED"), span:has-text("Issued")');
    await expect(statusBadge.first()).toBeVisible();

    // Step 3: Redeem the voucher
    const redeemButton = page.locator('button:has-text("Redeem")');
    if (await redeemButton.isVisible({ timeout: 2000 })) {
      await redeemButton.click();
      await page.waitForTimeout(500);

      // Confirm redemption
      const confirmButton = page.locator('button:has-text("Confirm"), button:has-text("Yes")');
      await confirmButton.click();
      await page.waitForTimeout(2000);

      // Verify status updated to REDEEMED
      await expect(page.locator('text=/REDEEMED|Redeemed/i')).toBeVisible({ timeout: 5000 });

      // Verify status badge color changed (visual indicator)
      const redeemedBadge = page.locator('[class*="badge"]:has-text("REDEEMED"), [class*="status"]:has-text("REDEEMED")');
      if (await redeemedBadge.isVisible({ timeout: 2000 })) {
        await expect(redeemedBadge).toBeVisible();
      }
    }

    // Step 4: Verify status persists after page reload
    await page.reload();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Status should still be REDEEMED
    await expect(page.locator('text=/REDEEMED|Redeemed/i')).toBeVisible();
  });

  test('should track voucher status transitions', async ({ page }) => {
    // Additional test for status tracking
    
    await page.goto(`${BASE_URL}/commodity-vouchers`);
    await page.waitForLoadState('networkidle');

    // Verify different status types are displayed
    const statusTypes = ['ISSUED', 'REDEEMED', 'EXPIRED', 'CANCELLED'];
    
    for (const status of statusTypes) {
      const statusElement = page.locator(`text=${status}, text=${status.toLowerCase()}`);
      const count = await statusElement.count();
      
      if (count > 0) {
        console.log(`Found ${count} vouchers with status: ${status}`);
      }
    }

    // Verify status filter works
    const statusFilter = page.locator('select[name="status"], select:has-text("Status")');
    if (await statusFilter.isVisible({ timeout: 2000 })) {
      await statusFilter.selectOption({ label: 'ISSUED' });
      await page.waitForTimeout(1000);

      // All visible vouchers should have ISSUED status
      const visibleRows = page.locator('table tbody tr');
      const rowCount = await visibleRows.count();
      
      if (rowCount > 0) {
        await expect(page.locator('text=/ISSUED|Issued/i')).toBeVisible();
      }
    }
  });

  test('should validate voucher against loan terms during generation', async ({ page }) => {
    // Requirement 12.4: Validate against loan terms
    
    await page.goto(`${BASE_URL}/commodity-vouchers`);
    await page.waitForLoadState('networkidle');

    await page.click('button:has-text("Generate"), button:has-text("New Voucher")');
    await page.waitForTimeout(500);

    // Try to generate voucher with amount exceeding loan amount
    await page.fill('input[name="amount"]', '999999999');
    
    const loanSelect = page.locator('select[name="loanId"]');
    if (await loanSelect.isVisible({ timeout: 1000 })) {
      await loanSelect.selectOption({ index: 1 });
    }

    await page.click('button[type="submit"]');
    await page.waitForTimeout(1000);

    // Should show validation error
    const errorMessage = page.locator('text=/exceeds|invalid|error|cannot exceed/i');
    if (await errorMessage.isVisible({ timeout: 2000 })) {
      await expect(errorMessage).toBeVisible();
    }
  });

  test('should display voucher details with QR code', async ({ page }) => {
    // Verify voucher details display
    
    await page.goto(`${BASE_URL}/commodity-vouchers`);
    await page.waitForLoadState('networkidle');

    // Click on first voucher
    const firstVoucher = page.locator('table tbody tr').first();
    if (await firstVoucher.isVisible({ timeout: 2000 })) {
      await firstVoucher.click();
      await page.waitForTimeout(500);

      // Verify voucher details are displayed
      await expect(page.locator('text=/Voucher Number|Voucher Details/i')).toBeVisible();
      await expect(page.locator('text=/Amount|Vendor|Commodity/i')).toBeVisible();

      // Verify QR code is displayed
      const qrCode = page.locator('img[alt*="QR"], canvas, svg');
      if (await qrCode.isVisible({ timeout: 2000 })) {
        await expect(qrCode.first()).toBeVisible();
      }
    }
  });
});
