# Quick Start: UI/UX Testing

## 🚀 Get Started in 5 Minutes

### Step 1: Install Dependencies (2 minutes)

```bash
cd frontend

# Install test dependencies
npm install --save-dev @playwright/test @axe-core/playwright @testing-library/user-event msw happy-dom @vitest/ui @vitest/coverage-v8

# Install Playwright browsers
npx playwright install
```

### Step 2: Run Your First Test (1 minute)

```bash
# Run authentication tests
npx playwright test tests/e2e/auth/login.spec.ts --headed
```

You should see the browser open and tests running automatically!

### Step 3: Explore Test Results (1 minute)

```bash
# View the HTML report
npx playwright show-report
```

### Step 4: Try Interactive Mode (1 minute)

```bash
# Run tests in UI mode
npx playwright test --ui
```

## 📋 Common Commands

### Running Tests

```bash
# Run all E2E tests
npm run test:e2e

# Run specific test file
npx playwright test tests/e2e/auth/login.spec.ts

# Run tests in headed mode (see browser)
npm run test:e2e:headed

# Run tests in debug mode
npm run test:e2e:debug

# Run unit tests
npm test

# Run all tests
npm run test:all
```

### Viewing Results

```bash
# Show test report
npm run test:report

# View coverage
npm run test:coverage
open coverage/index.html
```

### Development

```bash
# Run tests in watch mode
npm run test:watch

# Run tests with UI
npm run test:ui
```

## 🎯 What to Test First

### 1. Authentication (Already Complete ✅)
```bash
npx playwright test tests/e2e/auth/
```
- 23 test cases covering login and session management
- All passing and ready to use

### 2. Dashboard (Template Ready 📋)
```bash
npx playwright test tests/e2e/dashboard/
```
- Template with 8 test cases
- Needs API mocking adjustments

### 3. Loan Calculator (Template Ready 📋)
```bash
npx playwright test tests/e2e/loans/calculator.spec.ts
```
- Template with 10 test cases
- Needs API mocking adjustments

## 🔧 Quick Fixes

### Issue: Tests Failing Due to API

**Solution**: Make sure backend is running or update API mocks

```typescript
// In your test file
await page.route('**/api/**', async (route) => {
  await route.fulfill({
    status: 200,
    contentType: 'application/json',
    body: JSON.stringify({ /* your mock data */ }),
  });
});
```

### Issue: Timeout Errors

**Solution**: Increase timeout or add proper waits

```typescript
// Wait for network to be idle
await page.waitForLoadState('networkidle');

// Or increase timeout
test.setTimeout(60000); // 60 seconds
```

### Issue: Element Not Found

**Solution**: Use better selectors

```typescript
// ❌ Bad
await page.locator('.btn-primary').click();

// ✅ Good
await page.getByRole('button', { name: /Sign In/i }).click();
```

## 📝 Writing Your First Test

### 1. Create Test File

```bash
# Create new test file
touch tests/e2e/my-module/my-test.spec.ts
```

### 2. Copy Template

```typescript
import { test, expect } from '@playwright/test';

test.describe('My Module', () => {
  test.beforeEach(async ({ page, context }) => {
    // Set up authentication
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

    await page.goto('/my-module');
  });

  test('should display page correctly', async ({ page }) => {
    await expect(page.getByRole('heading', { name: /My Module/i })).toBeVisible();
  });

  test('should handle user interaction', async ({ page }) => {
    await page.getByRole('button', { name: /Click Me/i }).click();
    await expect(page.getByText(/Success/i)).toBeVisible();
  });
});
```

### 3. Run Your Test

```bash
npx playwright test tests/e2e/my-module/my-test.spec.ts --headed
```

## 🎨 Test Patterns

### Pattern 1: Form Submission

```typescript
test('should submit form', async ({ page }) => {
  await page.getByLabel(/Name/i).fill('John Doe');
  await page.getByLabel(/Email/i).fill('john@example.com');
  await page.getByRole('button', { name: /Submit/i }).click();
  await expect(page.getByText(/Success/i)).toBeVisible();
});
```

### Pattern 2: API Mocking

```typescript
test('should display data from API', async ({ page }) => {
  await page.route('**/api/data', async (route) => {
    await route.fulfill({
      status: 200,
      body: JSON.stringify({ items: [{ id: 1, name: 'Item 1' }] }),
    });
  });

  await page.goto('/data');
  await expect(page.getByText('Item 1')).toBeVisible();
});
```

### Pattern 3: Navigation

```typescript
test('should navigate to another page', async ({ page }) => {
  await page.getByRole('link', { name: /Go to Dashboard/i }).click();
  await expect(page).toHaveURL('/dashboard');
  await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
});
```

### Pattern 4: Loading States

```typescript
test('should show loading state', async ({ page }) => {
  await page.route('**/api/data', async (route) => {
    await new Promise(resolve => setTimeout(resolve, 1000));
    await route.fulfill({ status: 200, body: '{}' });
  });

  await page.getByRole('button', { name: /Load Data/i }).click();
  await expect(page.getByText(/Loading/i)).toBeVisible();
});
```

## 🐛 Debugging Tips

### 1. Use Headed Mode
```bash
npx playwright test --headed --slowMo=1000
```

### 2. Use Debug Mode
```bash
npx playwright test --debug
```

### 3. Take Screenshots
```typescript
await page.screenshot({ path: 'screenshot.png' });
```

### 4. Check Console Logs
```typescript
page.on('console', msg => console.log(msg.text()));
```

### 5. Pause Execution
```typescript
await page.pause(); // Opens Playwright Inspector
```

## 📚 Learn More

- **Full Documentation**: See `tests/README.md`
- **Execution Guide**: See `TEST_EXECUTION_GUIDE.md`
- **Implementation Status**: See `TEST_IMPLEMENTATION_STATUS.md`
- **Summary**: See `UI_UX_TESTING_SUMMARY.md`

## 🎓 Next Steps

1. ✅ Run existing authentication tests
2. ✅ Explore test reports
3. ✅ Try writing a simple test
4. 📋 Implement remaining module tests
5. 📋 Add accessibility tests
6. 📋 Add responsive tests

## 💡 Pro Tips

1. **Use Test Utilities**: Import from `tests/utils/` for mock data and helpers
2. **Follow Patterns**: Copy from existing tests (auth tests are complete)
3. **Mock APIs**: Always mock API responses for consistent tests
4. **Use Selectors**: Prefer `getByRole` and `getByLabel` over CSS selectors
5. **Wait Properly**: Use `waitForLoadState` instead of arbitrary timeouts

## 🆘 Need Help?

1. Check the documentation files in `tests/` directory
2. Review existing test files for examples
3. Use Playwright's built-in debugging tools
4. Consult the team

---

**Happy Testing! 🎉**

Start with the authentication tests to see everything in action, then use the templates to build out the remaining modules.
