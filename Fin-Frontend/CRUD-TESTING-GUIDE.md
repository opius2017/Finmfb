# Frontend CRUD Operations Testing Guide

## Overview

This guide provides comprehensive instructions for testing all CRUD (Create, Read, Update, Delete) operations across the frontend dashboards and pages.

## Test Coverage

### ✅ Modules Tested

1. **Customer Management**
   - Create new customers
   - View customer details
   - Update customer information
   - Delete customers

2. **Loan Management**
   - Create loan applications
   - View loan details
   - Update loan status
   - Process loan repayments

3. **Inventory Management**
   - Create inventory items
   - View item details
   - Update stock quantities
   - Delete inventory items

4. **Accounts Payable**
   - Create payable entries
   - View payable details
   - Mark as paid
   - Track payment history

5. **Accounts Receivable**
   - Create receivable entries
   - View receivable details
   - Record payments received
   - Track collection status

6. **Payroll Management**
   - Create payroll entries
   - View payroll details
   - Process payments
   - Track payment status

7. **Dashboard Navigation**
   - Main dashboard
   - Executive dashboard
   - Loan dashboard
   - Deposit dashboard

8. **Additional Features**
   - Search functionality
   - Filter operations
   - Form validation
   - Error handling

## Prerequisites

### 1. Environment Setup

```bash
# Install dependencies
cd Fin-Frontend
npm install

# Install Playwright browsers
npx playwright install --with-deps
```

### 2. Backend Server

Ensure the backend API is running:

```bash
cd Fin-Backend
dotnet run
```

Backend should be accessible at: `http://localhost:5000`

### 3. Frontend Server

Start the frontend development server:

```bash
cd Fin-Frontend
npm run dev
```

Frontend should be accessible at: `http://localhost:5173`

## Running Tests

### Quick Start

Run all CRUD tests with the automated script:

```bash
cd Fin-Frontend
chmod +x test-crud.sh
./test-crud.sh
```

### Manual Test Execution

#### Run All Tests

```bash
npx playwright test
```

#### Run Specific Test Suite

```bash
# Customer tests only
npx playwright test --grep "Customer Management"

# Loan tests only
npx playwright test --grep "Loan Management"

# Inventory tests only
npx playwright test --grep "Inventory Management"
```

#### Run in Different Browsers

```bash
# Chromium (default)
npx playwright test --project=chromium

# Firefox
npx playwright test --project=firefox

# WebKit (Safari)
npx playwright test --project=webkit

# All browsers
npx playwright test --project=chromium --project=firefox --project=webkit
```

#### Debug Mode

```bash
# Run with Playwright Inspector
npx playwright test --debug

# Run specific test in debug mode
npx playwright test --grep "create a new customer" --debug
```

#### UI Mode (Interactive)

```bash
npx playwright test --ui
```

#### Headed Mode (See Browser)

```bash
npx playwright test --headed
```

## Test Configuration

### Environment Variables

Create a `.env.test` file:

```env
BASE_URL=http://localhost:5173
API_URL=http://localhost:5000
TEST_USER_EMAIL=admin@fintech.com
TEST_USER_PASSWORD=Admin@123
```

### Playwright Configuration

Edit `playwright.config.ts` to customize:

- Test timeout
- Retry attempts
- Screenshot/video settings
- Browser configurations
- Parallel execution

## Test Results

### View HTML Report

After running tests:

```bash
npx playwright show-report
```

This opens an interactive HTML report showing:
- Test pass/fail status
- Execution time
- Screenshots of failures
- Video recordings
- Trace files

### CI/CD Integration

Tests can be integrated into CI/CD pipelines:

```yaml
# GitHub Actions example
- name: Run E2E Tests
  run: |
    npm install
    npx playwright install --with-deps
    npx playwright test
    
- name: Upload Test Results
  if: always()
  uses: actions/upload-artifact@v3
  with:
    name: playwright-report
    path: playwright-report/
```

## Manual Testing Checklist

### Customer Management

- [ ] Navigate to `/customers`
- [ ] Click "Add Customer" button
- [ ] Fill in all required fields
- [ ] Submit form
- [ ] Verify customer appears in list
- [ ] Click on customer to view details
- [ ] Click "Edit" button
- [ ] Update customer information
- [ ] Save changes
- [ ] Verify updates are displayed
- [ ] Click "Delete" button
- [ ] Confirm deletion
- [ ] Verify customer is removed from list

### Loan Management

- [ ] Navigate to `/loans`
- [ ] Click "New Loan" button
- [ ] Fill in loan application form
- [ ] Submit application
- [ ] Verify application appears in list
- [ ] Click on loan to view details
- [ ] Verify loan information is correct
- [ ] Navigate to `/loans/management`
- [ ] Find pending loan
- [ ] Click "Approve" button
- [ ] Confirm approval
- [ ] Verify status updated to "Approved"
- [ ] Click "Make Payment" button
- [ ] Enter payment amount
- [ ] Submit payment
- [ ] Verify payment recorded

### Inventory Management

- [ ] Navigate to `/inventory`
- [ ] Click "Add Item" button
- [ ] Fill in item details
- [ ] Submit form
- [ ] Verify item appears in list
- [ ] Click on item to view details
- [ ] Click "Edit" button
- [ ] Update quantity
- [ ] Save changes
- [ ] Verify quantity updated
- [ ] Click "Delete" button
- [ ] Confirm deletion
- [ ] Verify item removed

### Accounts Payable

- [ ] Navigate to `/accounts-payable`
- [ ] Click "New Payable" button
- [ ] Fill in payable details
- [ ] Submit form
- [ ] Verify entry appears in list
- [ ] Click on entry to view details
- [ ] Click "Mark as Paid" button
- [ ] Enter payment details
- [ ] Submit payment
- [ ] Verify status updated to "Paid"

### Accounts Receivable

- [ ] Navigate to `/accounts-receivable`
- [ ] Click "New Receivable" button
- [ ] Fill in receivable details
- [ ] Submit form
- [ ] Verify entry appears in list
- [ ] Click on entry to view details
- [ ] Click "Record Payment" button
- [ ] Enter payment received
- [ ] Submit payment
- [ ] Verify status updated

### Payroll Management

- [ ] Navigate to `/payroll`
- [ ] Click "New Payroll" button
- [ ] Fill in employee details
- [ ] Submit form
- [ ] Verify entry appears in list
- [ ] Click on entry to view details
- [ ] Click "Process Payment" button
- [ ] Confirm payment
- [ ] Verify status updated to "Paid"

### Dashboard Navigation

- [ ] Navigate to `/dashboard`
- [ ] Verify main dashboard loads
- [ ] Check all metrics display correctly
- [ ] Navigate to `/dashboard/executive`
- [ ] Verify executive dashboard loads
- [ ] Navigate to `/dashboard/loans`
- [ ] Verify loan dashboard loads
- [ ] Navigate to `/dashboard/deposits`
- [ ] Verify deposit dashboard loads

### Search and Filter

- [ ] Navigate to any list page
- [ ] Enter search term in search box
- [ ] Verify results filter correctly
- [ ] Clear search
- [ ] Verify all results return
- [ ] Apply status filter
- [ ] Verify filtered results
- [ ] Clear filter
- [ ] Verify all results return

### Form Validation

- [ ] Open any create form
- [ ] Try to submit without filling required fields
- [ ] Verify validation errors display
- [ ] Fill in invalid data (e.g., negative numbers)
- [ ] Verify validation errors display
- [ ] Fill in valid data
- [ ] Verify form submits successfully

## Common Issues and Solutions

### Issue: Tests Fail with "Timeout"

**Solution:**
- Increase timeout in `playwright.config.ts`
- Check if backend is running
- Check network connectivity

### Issue: "Element not found"

**Solution:**
- Check if selectors match actual HTML
- Wait for page to load completely
- Use `page.waitForSelector()` before interacting

### Issue: Authentication Fails

**Solution:**
- Verify test credentials are correct
- Check if backend authentication is working
- Clear browser storage before tests

### Issue: Tests Pass Locally but Fail in CI

**Solution:**
- Ensure CI environment has all dependencies
- Use `--workers=1` to run tests sequentially
- Increase timeouts for slower CI environments

## Best Practices

1. **Test Isolation**: Each test should be independent
2. **Clean State**: Reset database/state between tests
3. **Meaningful Names**: Use descriptive test names
4. **Wait Strategies**: Use proper wait strategies instead of fixed timeouts
5. **Error Handling**: Test both success and error scenarios
6. **Data Cleanup**: Clean up test data after tests
7. **Screenshots**: Take screenshots on failures for debugging
8. **Parallel Execution**: Run tests in parallel when possible

## Performance Benchmarks

Expected test execution times:

- Customer CRUD: ~30 seconds
- Loan CRUD: ~45 seconds
- Inventory CRUD: ~25 seconds
- Accounts Payable CRUD: ~30 seconds
- Accounts Receivable CRUD: ~30 seconds
- Payroll CRUD: ~25 seconds
- Dashboard Navigation: ~15 seconds
- Search/Filter: ~20 seconds
- Form Validation: ~15 seconds

**Total Suite**: ~4-5 minutes (single browser)

## Reporting Issues

When reporting test failures, include:

1. Test name that failed
2. Browser and version
3. Screenshot/video of failure
4. Console logs
5. Network logs
6. Steps to reproduce

## Additional Resources

- [Playwright Documentation](https://playwright.dev/)
- [Testing Best Practices](https://playwright.dev/docs/best-practices)
- [Debugging Tests](https://playwright.dev/docs/debug)
- [CI/CD Integration](https://playwright.dev/docs/ci)

## Support

For issues or questions:
- Check the HTML test report
- Review console logs
- Check backend API logs
- Contact the development team
