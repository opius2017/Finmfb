import { test, expect, Page } from '@playwright/test';

/**
 * E2E Tests for CRUD Operations across all Frontend Dashboards
 * Tests Create, Read, Update, Delete operations for:
 * - Customers
 * - Loans
 * - Inventory
 * - Accounts Payable/Receivable
 * - Payroll
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

test.describe('Frontend CRUD Operations - Complete Test Suite', () => {
  
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test.describe('Customer Management CRUD', () => {
    
    test('should create a new customer', async ({ page }) => {
      // Navigate to customers page
      await page.goto(`${BASE_URL}/customers`);
      await page.waitForLoadState('networkidle');

      // Click create customer button
      await page.click('button:has-text("Add Customer"), button:has-text("New Customer"), button:has-text("Create Customer")');
      
      // Fill customer form
      const timestamp = Date.now();
      await page.fill('input[name="firstName"]', `Test`);
      await page.fill('input[name="lastName"]', `Customer${timestamp}`);
      await page.fill('input[name="email"]', `customer${timestamp}@test.com`);
      await page.fill('input[name="phoneNumber"]', `080${timestamp.toString().slice(-8)}`);
      await page.fill('input[name="address"]', '123 Test Street, Lagos');
      
      // Submit form
      const responsePromise = waitForApiResponse(page, '/api/customers');
      await page.click('button[type="submit"]:has-text("Create"), button[type="submit"]:has-text("Save")');
      await responsePromise;
      
      // Verify success message
      await expect(page.locator('text=/Customer created successfully|Success/i')).toBeVisible({ timeout: 5000 });
      
      // Verify customer appears in list
      await page.waitForTimeout(1000);
      await expect(page.locator(`text=Customer${timestamp}`)).toBeVisible();
    });

    test('should read/view customer details', async ({ page }) => {
      await page.goto(`${BASE_URL}/customers`);
      await page.waitForLoadState('networkidle');
      
      // Click on first customer in list
      const firstCustomer = page.locator('table tbody tr').first();
      await firstCustomer.click();
      
      // Verify customer detail page loads
      await page.waitForURL(`${BASE_URL}/customers/**`);
      
      // Verify customer information is displayed
      await expect(page.locator('text=/Customer Details|Profile/i')).toBeVisible();
      await expect(page.locator('text=/Email|Phone|Address/i')).toBeVisible();
    });

    test('should update customer information', async ({ page }) => {
      await page.goto(`${BASE_URL}/customers`);
      await page.waitForLoadState('networkidle');
      
      // Click on first customer
      const firstCustomer = page.locator('table tbody tr').first();
      await firstCustomer.click();
      await page.waitForURL(`${BASE_URL}/customers/**`);
      
      // Click edit button
      await page.click('button:has-text("Edit")');
      
      // Update customer information
      const newAddress = `Updated Address ${Date.now()}`;
      await page.fill('input[name="address"]', newAddress);
      
      // Save changes
      const responsePromise = waitForApiResponse(page, '/api/customers');
      await page.click('button[type="submit"]:has-text("Update"), button[type="submit"]:has-text("Save")');
      await responsePromise;
      
      // Verify success message
      await expect(page.locator('text=/Updated successfully|Success/i')).toBeVisible({ timeout: 5000 });
      
      // Verify updated information is displayed
      await page.waitForTimeout(1000);
      await expect(page.locator(`text=${newAddress}`)).toBeVisible();
    });

    test('should delete a customer', async ({ page }) => {
      await page.goto(`${BASE_URL}/customers`);
      await page.waitForLoadState('networkidle');
      
      // Get customer count before deletion
      const initialCount = await page.locator('table tbody tr').count();
      
      // Click delete button on first customer
      const firstCustomer = page.locator('table tbody tr').first();
      await firstCustomer.locator('button:has-text("Delete"), button[aria-label="Delete"]').click();
      
      // Confirm deletion
      await page.click('button:has-text("Confirm"), button:has-text("Yes"), button:has-text("Delete")');
      
      // Wait for API response
      await waitForApiResponse(page, '/api/customers');
      
      // Verify success message
      await expect(page.locator('text=/Deleted successfully|Success/i')).toBeVisible({ timeout: 5000 });
      
      // Verify customer count decreased
      await page.waitForTimeout(1000);
      const newCount = await page.locator('table tbody tr').count();
      expect(newCount).toBeLessThan(initialCount);
    });
  });

  test.describe('Loan Management CRUD', () => {
    
    test('should create a new loan application', async ({ page }) => {
      await page.goto(`${BASE_URL}/loans`);
      await page.waitForLoadState('networkidle');
      
      // Click create loan button
      await page.click('button:has-text("New Loan"), button:has-text("Apply"), button:has-text("Create Loan")');
      
      // Fill loan application form
      await page.fill('input[name="loanAmount"]', '100000');
      await page.fill('input[name="loanPurpose"]', 'Business expansion');
      await page.selectOption('select[name="loanType"]', { label: 'NORMAL' });
      await page.fill('input[name="tenorMonths"]', '12');
      await page.fill('input[name="interestRate"]', '15');
      
      // Submit form
      const responsePromise = waitForApiResponse(page, '/api/loans');
      await page.click('button[type="submit"]:has-text("Submit"), button[type="submit"]:has-text("Apply")');
      await responsePromise;
      
      // Verify success message
      await expect(page.locator('text=/Loan application submitted|Success/i')).toBeVisible({ timeout: 5000 });
    });

    test('should view loan details', async ({ page }) => {
      await page.goto(`${BASE_URL}/loans`);
      await page.waitForLoadState('networkidle');
      
      // Click on first loan
      const firstLoan = page.locator('table tbody tr').first();
      await firstLoan.click();
      
      // Verify loan details are displayed
      await expect(page.locator('text=/Loan Details|Loan Information/i')).toBeVisible();
      await expect(page.locator('text=/Principal|Interest Rate|Tenor/i')).toBeVisible();
    });

    test('should update loan status', async ({ page }) => {
      await page.goto(`${BASE_URL}/loans/management`);
      await page.waitForLoadState('networkidle');
      
      // Find a pending loan
      const pendingLoan = page.locator('table tbody tr:has-text("Pending"), table tbody tr:has-text("SUBMITTED")').first();
      await pendingLoan.click();
      
      // Click approve/update button
      await page.click('button:has-text("Approve"), button:has-text("Update Status")');
      
      // Confirm action
      const responsePromise = waitForApiResponse(page, '/api/loans');
      await page.click('button:has-text("Confirm"), button:has-text("Yes")');
      await responsePromise;
      
      // Verify success message
      await expect(page.locator('text=/Status updated|Success/i')).toBeVisible({ timeout: 5000 });
    });

    test('should process loan repayment', async ({ page }) => {
      await page.goto(`${BASE_URL}/loans`);
      await page.waitForLoadState('networkidle');
      
      // Find an active loan
      const activeLoan = page.locator('table tbody tr:has-text("Active"), table tbody tr:has-text("ACTIVE")').first();
      await activeLoan.click();
      
      // Click repayment button
      await page.click('button:has-text("Make Payment"), button:has-text("Repay")');
      
      // Fill repayment form
      await page.fill('input[name="amount"]', '10000');
      await page.selectOption('select[name="paymentMethod"]', { label: 'Bank Transfer' });
      await page.fill('input[name="transactionReference"]', `TXN${Date.now()}`);
      
      // Submit repayment
      const responsePromise = waitForApiResponse(page, '/api/loans');
      await page.click('button[type="submit"]:has-text("Submit"), button[type="submit"]:has-text("Pay")');
      await responsePromise;
      
      // Verify success message
      await expect(page.locator('text=/Payment processed|Success/i')).toBeVisible({ timeout: 5000 });
    });
  });

  test.describe('Inventory Management CRUD', () => {
    
    test('should create a new inventory item', async ({ page }) => {
      await page.goto(`${BASE_URL}/inventory`);
      await page.waitForLoadState('networkidle');
      
      // Click create item button
      await page.click('button:has-text("Add Item"), button:has-text("New Item"), button:has-text("Create Item")');
      
      // Fill inventory form
      const timestamp = Date.now();
      await page.fill('input[name="itemName"]', `Test Item ${timestamp}`);
      await page.fill('input[name="itemCode"]', `ITEM${timestamp}`);
      await page.fill('input[name="quantity"]', '100');
      await page.fill('input[name="unitPrice"]', '5000');
      await page.fill('textarea[name="description"]', 'Test inventory item');
      
      // Submit form
      const responsePromise = waitForApiResponse(page, '/api/inventory');
      await page.click('button[type="submit"]:has-text("Create"), button[type="submit"]:has-text("Save")');
      await responsePromise;
      
      // Verify success message
      await expect(page.locator('text=/Item created|Success/i')).toBeVisible({ timeout: 5000 });
      
      // Verify item appears in list
      await page.waitForTimeout(1000);
      await expect(page.locator(`text=Test Item ${timestamp}`)).toBeVisible();
    });

    test('should update inventory quantity', async ({ page }) => {
      await page.goto(`${BASE_URL}/inventory`);
      await page.waitForLoadState('networkidle');
      
      // Click on first item
      const firstItem = page.locator('table tbody tr').first();
      await firstItem.click();
      
      // Click edit/update button
      await page.click('button:has-text("Edit"), button:has-text("Update Stock")');
      
      // Update quantity
      await page.fill('input[name="quantity"]', '150');
      
      // Save changes
      const responsePromise = waitForApiResponse(page, '/api/inventory');
      await page.click('button[type="submit"]:has-text("Update"), button[type="submit"]:has-text("Save")');
      await responsePromise;
      
      // Verify success message
      await expect(page.locator('text=/Updated successfully|Success/i')).toBeVisible({ timeout: 5000 });
    });

    test('should delete inventory item', async ({ page }) => {
      await page.goto(`${BASE_URL}/inventory`);
      await page.waitForLoadState('networkidle');
      
      // Get item count before deletion
      const initialCount = await page.locator('table tbody tr').count();
      
      // Click delete button on first item
      const firstItem = page.locator('table tbody tr').first();
      await firstItem.locator('button:has-text("Delete"), button[aria-label="Delete"]').click();
      
      // Confirm deletion
      await page.click('button:has-text("Confirm"), button:has-text("Yes"), button:has-text("Delete")');
      
      // Wait for API response
      await waitForApiResponse(page, '/api/inventory');
      
      // Verify success message
      await expect(page.locator('text=/Deleted successfully|Success/i')).toBeVisible({ timeout: 5000 });
      
      // Verify item count decreased
      await page.waitForTimeout(1000);
      const newCount = await page.locator('table tbody tr').count();
      expect(newCount).toBeLessThan(initialCount);
    });
  });

  test.describe('Accounts Payable CRUD', () => {
    
    test('should create a new payable entry', async ({ page }) => {
      await page.goto(`${BASE_URL}/accounts-payable`);
      await page.waitForLoadState('networkidle');
      
      // Click create button
      await page.click('button:has-text("New Payable"), button:has-text("Add Payable"), button:has-text("Create")');
      
      // Fill form
      await page.fill('input[name="vendorName"]', `Vendor ${Date.now()}`);
      await page.fill('input[name="amount"]', '50000');
      await page.fill('input[name="invoiceNumber"]', `INV${Date.now()}`);
      await page.fill('input[name="dueDate"]', '2024-12-31');
      await page.fill('textarea[name="description"]', 'Test payable entry');
      
      // Submit form
      const responsePromise = waitForApiResponse(page, '/api/accounts-payable');
      await page.click('button[type="submit"]:has-text("Create"), button[type="submit"]:has-text("Save")');
      await responsePromise;
      
      // Verify success message
      await expect(page.locator('text=/Created successfully|Success/i')).toBeVisible({ timeout: 5000 });
    });

    test('should mark payable as paid', async ({ page }) => {
      await page.goto(`${BASE_URL}/accounts-payable`);
      await page.waitForLoadState('networkidle');
      
      // Find unpaid entry
      const unpaidEntry = page.locator('table tbody tr:has-text("Unpaid"), table tbody tr:has-text("PENDING")').first();
      await unpaidEntry.click();
      
      // Click pay button
      await page.click('button:has-text("Mark as Paid"), button:has-text("Pay")');
      
      // Fill payment details
      await page.fill('input[name="paymentReference"]', `PAY${Date.now()}`);
      await page.selectOption('select[name="paymentMethod"]', { label: 'Bank Transfer' });
      
      // Confirm payment
      const responsePromise = waitForApiResponse(page, '/api/accounts-payable');
      await page.click('button[type="submit"]:has-text("Confirm"), button[type="submit"]:has-text("Pay")');
      await responsePromise;
      
      // Verify success message
      await expect(page.locator('text=/Payment recorded|Success/i')).toBeVisible({ timeout: 5000 });
    });
  });

  test.describe('Accounts Receivable CRUD', () => {
    
    test('should create a new receivable entry', async ({ page }) => {
      await page.goto(`${BASE_URL}/accounts-receivable`);
      await page.waitForLoadState('networkidle');
      
      // Click create button
      await page.click('button:has-text("New Receivable"), button:has-text("Add Receivable"), button:has-text("Create")');
      
      // Fill form
      await page.fill('input[name="customerName"]', `Customer ${Date.now()}`);
      await page.fill('input[name="amount"]', '75000');
      await page.fill('input[name="invoiceNumber"]', `INV${Date.now()}`);
      await page.fill('input[name="dueDate"]', '2024-12-31');
      await page.fill('textarea[name="description"]', 'Test receivable entry');
      
      // Submit form
      const responsePromise = waitForApiResponse(page, '/api/accounts-receivable');
      await page.click('button[type="submit"]:has-text("Create"), button[type="submit"]:has-text("Save")');
      await responsePromise;
      
      // Verify success message
      await expect(page.locator('text=/Created successfully|Success/i')).toBeVisible({ timeout: 5000 });
    });

    test('should record payment received', async ({ page }) => {
      await page.goto(`${BASE_URL}/accounts-receivable`);
      await page.waitForLoadState('networkidle');
      
      // Find unpaid entry
      const unpaidEntry = page.locator('table tbody tr:has-text("Unpaid"), table tbody tr:has-text("PENDING")').first();
      await unpaidEntry.click();
      
      // Click receive payment button
      await page.click('button:has-text("Record Payment"), button:has-text("Receive")');
      
      // Fill payment details
      await page.fill('input[name="amountReceived"]', '75000');
      await page.fill('input[name="paymentReference"]', `RCV${Date.now()}`);
      await page.selectOption('select[name="paymentMethod"]', { label: 'Bank Transfer' });
      
      // Confirm payment
      const responsePromise = waitForApiResponse(page, '/api/accounts-receivable');
      await page.click('button[type="submit"]:has-text("Confirm"), button[type="submit"]:has-text("Record")');
      await responsePromise;
      
      // Verify success message
      await expect(page.locator('text=/Payment recorded|Success/i')).toBeVisible({ timeout: 5000 });
    });
  });

  test.describe('Payroll Management CRUD', () => {
    
    test('should create a new payroll entry', async ({ page }) => {
      await page.goto(`${BASE_URL}/payroll`);
      await page.waitForLoadState('networkidle');
      
      // Click create button
      await page.click('button:has-text("New Payroll"), button:has-text("Add Payroll"), button:has-text("Create")');
      
      // Fill form
      await page.fill('input[name="employeeName"]', `Employee ${Date.now()}`);
      await page.fill('input[name="employeeId"]', `EMP${Date.now()}`);
      await page.fill('input[name="basicSalary"]', '150000');
      await page.fill('input[name="allowances"]', '50000');
      await page.fill('input[name="deductions"]', '20000');
      
      // Submit form
      const responsePromise = waitForApiResponse(page, '/api/payroll');
      await page.click('button[type="submit"]:has-text("Create"), button[type="submit"]:has-text("Save")');
      await responsePromise;
      
      // Verify success message
      await expect(page.locator('text=/Created successfully|Success/i')).toBeVisible({ timeout: 5000 });
    });

    test('should process payroll payment', async ({ page }) => {
      await page.goto(`${BASE_URL}/payroll`);
      await page.waitForLoadState('networkidle');
      
      // Find pending payroll
      const pendingPayroll = page.locator('table tbody tr:has-text("Pending"), table tbody tr:has-text("PENDING")').first();
      await pendingPayroll.click();
      
      // Click process payment button
      await page.click('button:has-text("Process Payment"), button:has-text("Pay")');
      
      // Confirm payment
      const responsePromise = waitForApiResponse(page, '/api/payroll');
      await page.click('button:has-text("Confirm"), button:has-text("Yes")');
      await responsePromise;
      
      // Verify success message
      await expect(page.locator('text=/Payment processed|Success/i')).toBeVisible({ timeout: 5000 });
    });
  });

  test.describe('Dashboard Navigation', () => {
    
    test('should navigate to all dashboard pages', async ({ page }) => {
      // Main dashboard
      await page.goto(`${BASE_URL}/dashboard`);
      await expect(page.locator('text=/Dashboard|Overview/i')).toBeVisible();
      
      // Executive dashboard
      await page.goto(`${BASE_URL}/dashboard/executive`);
      await expect(page.locator('text=/Executive|Overview/i')).toBeVisible();
      
      // Loan dashboard
      await page.goto(`${BASE_URL}/dashboard/loans`);
      await expect(page.locator('text=/Loan|Portfolio/i')).toBeVisible();
      
      // Deposit dashboard
      await page.goto(`${BASE_URL}/dashboard/deposits`);
      await expect(page.locator('text=/Deposit|Savings/i')).toBeVisible();
    });

    test('should display correct metrics on dashboards', async ({ page }) => {
      await page.goto(`${BASE_URL}/dashboard`);
      await page.waitForLoadState('networkidle');
      
      // Verify key metrics are displayed
      await expect(page.locator('text=/Total|Active|Pending/i')).toBeVisible();
      await expect(page.locator('text=/₦|NGN/i')).toBeVisible(); // Currency symbol
    });
  });

  test.describe('Search and Filter Functionality', () => {
    
    test('should search customers', async ({ page }) => {
      await page.goto(`${BASE_URL}/customers`);
      await page.waitForLoadState('networkidle');
      
      // Enter search term
      await page.fill('input[placeholder*="Search"], input[type="search"]', 'test');
      await page.waitForTimeout(1000);
      
      // Verify filtered results
      const rows = await page.locator('table tbody tr').count();
      expect(rows).toBeGreaterThan(0);
    });

    test('should filter loans by status', async ({ page }) => {
      await page.goto(`${BASE_URL}/loans`);
      await page.waitForLoadState('networkidle');
      
      // Apply status filter
      await page.selectOption('select[name="status"], select:has-text("Status")', { label: 'Active' });
      await page.waitForTimeout(1000);
      
      // Verify filtered results show only active loans
      const activeLoans = page.locator('table tbody tr:has-text("Active"), table tbody tr:has-text("ACTIVE")');
      const count = await activeLoans.count();
      expect(count).toBeGreaterThan(0);
    });
  });

  test.describe('Form Validation', () => {
    
    test('should validate required fields in customer form', async ({ page }) => {
      await page.goto(`${BASE_URL}/customers`);
      await page.waitForLoadState('networkidle');
      
      // Click create customer
      await page.click('button:has-text("Add Customer"), button:has-text("New Customer")');
      
      // Try to submit empty form
      await page.click('button[type="submit"]');
      
      // Verify validation errors
      await expect(page.locator('text=/required|Required|This field is required/i')).toBeVisible();
    });

    test('should validate loan amount limits', async ({ page }) => {
      await page.goto(`${BASE_URL}/loans`);
      await page.waitForLoadState('networkidle');
      
      // Click create loan
      await page.click('button:has-text("New Loan"), button:has-text("Apply")');
      
      // Enter invalid amount (too low)
      await page.fill('input[name="loanAmount"]', '100');
      await page.click('button[type="submit"]');
      
      // Verify validation error
      await expect(page.locator('text=/minimum|Minimum|invalid/i')).toBeVisible();
    });
  });
});
