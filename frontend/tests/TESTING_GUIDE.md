# Testing Guide

## Overview
This guide provides comprehensive instructions for running, maintaining, and extending the test suite for the Cooperative Loan Management System frontend application.

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Running Tests](#running-tests)
3. [Test Structure](#test-structure)
4. [Writing New Tests](#writing-new-tests)
5. [Test Data Management](#test-data-management)
6. [CI/CD Integration](#cicd-integration)
7. [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Software
- Node.js (v18 or higher)
- npm or yarn
- Modern browser (Chrome, Firefox, or Safari)

### Installation
```bash
# Install dependencies
npm install

# Install Playwright browsers
npx playwright install
```

## Running Tests

### Run All Tests
```bash
# Run all E2E tests
npm run test:e2e

# Run all unit tests
npm run test:unit

# Run all tests
npm test
```

### Run Specific Test Files
```bash
# Run specific test file
npx playwright test tests/e2e/loans/calculator.spec.ts

# Run tests matching pattern
npx playwright test --grep "Loan Calculator"
```

### Run Tests in Different Modes
```bash
# Run in headed mode (see browser)
npx playwright test --headed

# Run in debug mode
npx playwright test --debug

# Run with UI mode
npx playwright test --ui
```

### Run Tests on Specific Browsers
```bash
# Run on Chrome only
npx playwright test --project=chromium

# Run on Firefox only
npx playwright test --project=firefox

# Run on Safari only
npx playwright test --project=webkit
```

### Run Tests on Different Viewports
```bash
# Run mobile tests
npx playwright test --project="Mobile Chrome"

# Run tablet tests
npx playwright test --project="iPad"
```

## Test Structure

### Directory Organization
```
frontend/tests/
├── e2e/                    # End-to-end tests
│   ├── auth/              # Authentication tests
│   ├── dashboard/         # Dashboard tests
│   ├── loans/             # Loan-related tests
│   ├── guarantor/         # Guarantor tests
│   ├── committee/         # Committee tests
│   ├── operations/        # Operations tests
│   ├── reports/           # Reports tests
│   ├── navigation/        # Navigation tests
│   └── responsive/        # Responsive design tests
├── unit/                  # Unit tests
├── integration/           # Integration tests
├── accessibility/         # Accessibility tests
├── visual/               # Visual regression tests
├── utils/                # Test utilities
│   ├── mock-data.ts      # Mock data factory
│   ├── test-helpers.ts   # Helper functions
│   └── custom-render.tsx # Custom render utilities
└── setup.ts              # Test setup configuration
```

### Test File Naming Convention
- E2E tests: `*.spec.ts`
- Unit tests: `*.test.tsx` or `*.test.ts`
- Test utilities: `*.ts`

## Writing New Tests

### Basic Test Structure
```typescript
import { test, expect } from '@playwright/test';

test.describe('Feature Name', () => {
  test.beforeEach(async ({ page, context }) => {
    // Setup authentication
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

    await page.goto('/your-page');
  });

  test('should do something', async ({ page }) => {
    // Arrange
    await page.route('**/api/endpoint', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ data: 'mock-data' }),
      });
    });

    // Act
    await page.getByRole('button', { name: /Click Me/i }).click();

    // Assert
    await expect(page.getByText(/Success/i)).toBeVisible();
  });
});
```

### Using Mock Data Factory
```typescript
import { mockDataFactory } from '../../utils/mock-data';

test('should display loan applications', async ({ page }) => {
  const applications = mockDataFactory.createLoanApplications(5);

  await page.route('**/api/loan-applications', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ data: applications }),
    });
  });

  // Test implementation
});
```

### Best Practices

1. **Use Descriptive Test Names**
   ```typescript
   // Good
   test('should display error message when login fails')
   
   // Bad
   test('test1')
   ```

2. **Follow AAA Pattern**
   - Arrange: Set up test data and mocks
   - Act: Perform the action being tested
   - Assert: Verify the expected outcome

3. **Use Page Object Model for Complex Pages**
   ```typescript
   class LoginPage {
     constructor(private page: Page) {}

     async login(email: string, password: string) {
       await this.page.getByLabel(/Email/i).fill(email);
       await this.page.getByLabel(/Password/i).fill(password);
       await this.page.getByRole('button', { name: /Login/i }).click();
     }
   }
   ```

4. **Mock API Responses**
   Always mock API calls to ensure tests are deterministic and fast.

5. **Use Appropriate Selectors**
   - Prefer: `getByRole`, `getByLabel`, `getByText`
   - Avoid: CSS selectors, XPath

6. **Handle Async Operations**
   ```typescript
   // Wait for element
   await expect(page.getByText(/Success/i)).toBeVisible();

   // Wait for timeout
   await page.waitForTimeout(1000);

   // Wait for network
   await page.waitForResponse('**/api/endpoint');
   ```

## Test Data Management

### Mock Data Factory
The `mockDataFactory` provides methods to generate realistic test data:

```typescript
// Create single entity
const member = mockDataFactory.createMember();
const loan = mockDataFactory.createLoanApplication();

// Create multiple entities
const members = mockDataFactory.createMembers(10);
const loans = mockDataFactory.createLoanApplications(5, { status: 'PENDING' });

// Reset counter for test isolation
mockDataFactory.reset();
```

### Custom Test Data
```typescript
const customLoan = mockDataFactory.createLoanApplication({
  loanNumber: 'LOAN00000001',
  amount: 500000,
  status: 'APPROVED',
});
```

## CI/CD Integration

### GitHub Actions Example
```yaml
name: E2E Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: '18'
      
      - name: Install dependencies
        run: npm ci
      
      - name: Install Playwright browsers
        run: npx playwright install --with-deps
      
      - name: Run tests
        run: npm run test:e2e
      
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: playwright-report
          path: playwright-report/
```

### Running Tests in Docker
```dockerfile
FROM mcr.microsoft.com/playwright:v1.40.0-focal

WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .

CMD ["npm", "run", "test:e2e"]
```

## Troubleshooting

### Common Issues

#### 1. Tests Timing Out
```typescript
// Increase timeout for specific test
test('slow test', async ({ page }) => {
  test.setTimeout(60000); // 60 seconds
  // Test implementation
});
```

#### 2. Flaky Tests
- Add explicit waits: `await expect(element).toBeVisible()`
- Use `waitForLoadState`: `await page.waitForLoadState('networkidle')`
- Increase timeout: `await element.click({ timeout: 10000 })`

#### 3. Authentication Issues
Ensure authentication is set up in `beforeEach`:
```typescript
test.beforeEach(async ({ page, context }) => {
  await context.addInitScript(() => {
    localStorage.setItem('authToken', 'test-token');
    // ... rest of auth setup
  });
});
```

#### 4. API Mocking Not Working
- Check route pattern matches actual API calls
- Verify route is set up before navigation
- Use `page.route('**/*', ...)` to catch all requests for debugging

### Debug Mode
```bash
# Run with debug mode
npx playwright test --debug

# Run with trace
npx playwright test --trace on

# View trace
npx playwright show-trace trace.zip
```

### Viewing Test Reports
```bash
# Generate and open HTML report
npx playwright show-report
```

## Additional Resources

- [Playwright Documentation](https://playwright.dev/)
- [Testing Library Documentation](https://testing-library.com/)
- [Vitest Documentation](https://vitest.dev/)
- [Project README](../README.md)

## Test Coverage

### Viewing Coverage Reports
```bash
# Generate coverage report
npm run test:coverage

# View coverage in browser
npm run test:coverage -- --reporter=html
open coverage/index.html
```

### Coverage Goals
- Overall code coverage: > 80%
- Critical paths: 100%
- UI components: > 85%
- Business logic: > 90%

## Accessibility Testing

### Running Accessibility Tests
```bash
# Run all accessibility tests
npx playwright test tests/accessibility/

# Run with specific tags
npx playwright test --grep "@a11y"
```

### Writing Accessibility Tests
```typescript
import AxeBuilder from '@axe-core/playwright';

test('should not have accessibility violations', async ({ page }) => {
  await page.goto('/dashboard');
  
  const accessibilityScanResults = await new AxeBuilder({ page })
    .withTags(['wcag2a', 'wcag2aa'])
    .analyze();

  expect(accessibilityScanResults.violations).toEqual([]);
});
```

## Visual Regression Testing

### Capturing Baseline Screenshots
```bash
# Capture baseline screenshots
npx playwright test tests/visual/ --update-snapshots
```

### Running Visual Tests
```bash
# Run visual regression tests
npx playwright test tests/visual/

# Update snapshots if changes are intentional
npx playwright test tests/visual/ --update-snapshots
```

### Visual Test Best Practices
- Capture screenshots after page is fully loaded
- Use consistent viewport sizes
- Mask dynamic content (dates, times, IDs)
- Review visual diffs carefully before updating

## Performance Testing

### Running Performance Tests
```bash
# Run performance tests
npx playwright test tests/e2e/performance/
```

### Measuring Performance
```typescript
test('should load page within 2 seconds', async ({ page }) => {
  const startTime = Date.now();
  
  await page.goto('/dashboard');
  await page.waitForLoadState('networkidle');
  
  const loadTime = Date.now() - startTime;
  expect(loadTime).toBeLessThan(2000);
});
```

## Responsive Testing

### Testing Different Viewports
```bash
# Run mobile tests
npx playwright test tests/e2e/responsive/mobile.spec.ts

# Run tablet tests
npx playwright test tests/e2e/responsive/tablet.spec.ts

# Run desktop tests
npx playwright test tests/e2e/responsive/tablet.spec.ts --grep "Desktop"
```

### Custom Viewport Testing
```typescript
test('should work on custom viewport', async ({ page }) => {
  await page.setViewportSize({ width: 1440, height: 900 });
  await page.goto('/dashboard');
  // Test implementation
});
```

## Test Maintenance

### Updating Tests After Code Changes
1. Run tests to identify failures
2. Update test expectations if behavior changed intentionally
3. Fix tests if they're incorrectly written
4. Update mock data if API contracts changed

### Refactoring Tests
- Extract common setup to `beforeEach`
- Create helper functions for repeated actions
- Use Page Object Model for complex interactions
- Keep tests focused and independent

### Removing Obsolete Tests
- Delete tests for removed features
- Archive tests for deprecated functionality
- Update test documentation

## Test Data Seeding

### Seeding Test Database
```typescript
// In test setup
test.beforeAll(async () => {
  // Seed database with test data
  await seedTestData();
});

test.afterAll(async () => {
  // Clean up test data
  await cleanupTestData();
});
```

### Using Fixtures
```typescript
// Define fixture
const test = base.extend({
  authenticatedPage: async ({ page, context }, use) => {
    await context.addInitScript(() => {
      localStorage.setItem('authToken', 'test-token');
    });
    await page.goto('/dashboard');
    await use(page);
  },
});

// Use fixture
test('should display dashboard', async ({ authenticatedPage }) => {
  await expect(authenticatedPage.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
});
```

## Parallel Test Execution

### Running Tests in Parallel
```bash
# Run with specific number of workers
npx playwright test --workers=4

# Run with maximum parallelization
npx playwright test --workers=100%
```

### Configuring Parallelization
```typescript
// playwright.config.ts
export default defineConfig({
  workers: process.env.CI ? 2 : undefined,
  fullyParallel: true,
});
```

## Test Reporting

### Generating Reports
```bash
# Generate HTML report
npx playwright test --reporter=html

# Generate JSON report
npx playwright test --reporter=json

# Generate JUnit report for CI
npx playwright test --reporter=junit
```

### Custom Reporters
```typescript
// playwright.config.ts
export default defineConfig({
  reporter: [
    ['html'],
    ['json', { outputFile: 'test-results.json' }],
    ['junit', { outputFile: 'junit-results.xml' }],
  ],
});
```

## Environment-Specific Testing

### Testing Against Different Environments
```bash
# Test against local
BASE_URL=http://localhost:3000 npm run test:e2e

# Test against staging
BASE_URL=https://staging.example.com npm run test:e2e

# Test against production
BASE_URL=https://example.com npm run test:e2e
```

### Environment Configuration
```typescript
// playwright.config.ts
export default defineConfig({
  use: {
    baseURL: process.env.BASE_URL || 'http://localhost:3000',
  },
});
```

## Continuous Testing

### Watch Mode
```bash
# Run tests in watch mode
npm run test:watch

# Run specific tests in watch mode
npx playwright test --watch tests/e2e/loans/
```

### Pre-commit Hooks
```json
// package.json
{
  "husky": {
    "hooks": {
      "pre-commit": "npm run test:unit",
      "pre-push": "npm run test:e2e"
    }
  }
}
```

## Test Isolation

### Ensuring Test Independence
- Each test should set up its own data
- Clean up after tests
- Don't rely on test execution order
- Use unique identifiers for test data

### Example
```typescript
test('should create loan application', async ({ page }) => {
  // Setup unique test data
  const uniqueId = `TEST-${Date.now()}`;
  const loanData = mockDataFactory.createLoanApplication({
    loanNumber: uniqueId,
  });

  // Test implementation

  // Cleanup (if needed)
  await cleanupLoan(uniqueId);
});
```

## Advanced Testing Techniques

### Testing File Uploads
```typescript
test('should upload document', async ({ page }) => {
  await page.goto('/applications/new');
  
  const fileInput = page.locator('input[type="file"]');
  await fileInput.setInputFiles('path/to/test-file.pdf');
  
  await expect(page.getByText(/File uploaded/i)).toBeVisible();
});
```

### Testing Downloads
```typescript
test('should download report', async ({ page }) => {
  await page.goto('/reports');
  
  const downloadPromise = page.waitForEvent('download');
  await page.getByRole('button', { name: /Download/i }).click();
  const download = await downloadPromise;
  
  expect(download.suggestedFilename()).toContain('report');
});
```

### Testing WebSocket Connections
```typescript
test('should receive real-time updates', async ({ page }) => {
  await page.goto('/dashboard');
  
  // Wait for WebSocket connection
  await page.waitForEvent('websocket');
  
  // Trigger update
  // Verify update is displayed
});
```

## Debugging Tips

### Using Console Logs
```typescript
test('debug test', async ({ page }) => {
  page.on('console', msg => console.log('Browser log:', msg.text()));
  
  await page.goto('/dashboard');
  // Test implementation
});
```

### Taking Screenshots
```typescript
test('should display correctly', async ({ page }) => {
  await page.goto('/dashboard');
  
  // Take screenshot for debugging
  await page.screenshot({ path: 'debug-screenshot.png' });
  
  // Test implementation
});
```

### Pausing Test Execution
```typescript
test('debug test', async ({ page }) => {
  await page.goto('/dashboard');
  
  // Pause execution for debugging
  await page.pause();
  
  // Test continues after manual resume
});
```

## Support

For questions or issues:
1. Check this guide
2. Review existing tests for examples
3. Consult Playwright documentation
4. Contact the development team

## Quick Reference

### Common Commands
```bash
# Run all tests
npm test

# Run E2E tests
npm run test:e2e

# Run unit tests
npm run test:unit

# Run in headed mode
npx playwright test --headed

# Run in debug mode
npx playwright test --debug

# Run specific test file
npx playwright test path/to/test.spec.ts

# Update snapshots
npx playwright test --update-snapshots

# View report
npx playwright show-report
```

### Common Assertions
```typescript
// Visibility
await expect(element).toBeVisible();
await expect(element).toBeHidden();

// Text content
await expect(element).toHaveText('Expected text');
await expect(element).toContainText('Partial text');

// Values
await expect(input).toHaveValue('value');

// Attributes
await expect(element).toHaveAttribute('href', '/path');

// Count
await expect(elements).toHaveCount(5);

// URL
await expect(page).toHaveURL('/expected-path');
```
