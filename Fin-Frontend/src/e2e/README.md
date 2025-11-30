# E2E Tests - Frontend CRUD Operations

## Overview

This directory contains end-to-end tests for all frontend CRUD operations using Playwright.

## Test Files

### crud-operations.spec.ts
Complete test suite covering:
- Customer Management (Create, Read, Update, Delete)
- Loan Management (Create, Read, Update, Repayment)
- Inventory Management (Create, Read, Update, Delete)
- Accounts Payable (Create, Read, Payment)
- Accounts Receivable (Create, Read, Payment)
- Payroll Management (Create, Read, Process)
- Dashboard Navigation
- Search & Filter
- Form Validation

### mfa-flow.spec.ts
Multi-factor authentication flow tests

## Quick Start

```bash
# Run all tests
npm run test:e2e

# Run in UI mode (recommended)
npm run test:e2e:ui

# Run specific test
npx playwright test --grep "Customer Management"

# View report
npm run test:e2e:report
```

## Test Structure

```typescript
test.describe('Feature Name', () => {
  test.beforeEach(async ({ page }) => {
    // Setup: Login, navigate, etc.
  });

  test('should perform action', async ({ page }) => {
    // Arrange: Setup test data
    // Act: Perform action
    // Assert: Verify result
  });
});
```

## Helper Functions

### login(page)
Logs in with test credentials

### waitForApiResponse(page, endpoint)
Waits for specific API response

## Test Data

Test credentials:
- Email: admin@fintech.com
- Password: Admin@123

## Configuration

See `playwright.config.ts` in project root for:
- Browser settings
- Timeout configuration
- Screenshot/video settings
- Reporting options

## Debugging

```bash
# Debug mode
npm run test:e2e:debug

# Headed mode (see browser)
npm run test:e2e:headed

# UI mode (interactive)
npm run test:e2e:ui
```

## Documentation

- **CRUD-TESTING-GUIDE.md** - Comprehensive guide
- **QUICK-TEST-GUIDE.md** - Quick reference
- **FRONTEND-TESTING-COMPLETE.md** - Implementation summary

## CI/CD

Tests are ready for CI/CD integration:

```yaml
- run: npm install
- run: npx playwright install --with-deps
- run: npm run test:e2e
```

## Support

For issues or questions, see:
- HTML report: `npm run test:e2e:report`
- Documentation: `CRUD-TESTING-GUIDE.md`
- Playwright docs: https://playwright.dev/
