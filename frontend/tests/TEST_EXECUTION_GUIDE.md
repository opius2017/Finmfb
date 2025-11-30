# Test Execution Guide

## Overview

This guide provides comprehensive instructions for executing all UI/UX tests for the Cooperative Loan Management System frontend application.

## Prerequisites

### 1. Install Dependencies

```bash
cd frontend

# Install testing dependencies
npm install --save-dev @playwright/test @axe-core/playwright @testing-library/user-event msw happy-dom @vitest/ui @vitest/coverage-v8

# Install Playwright browsers
npx playwright install
```

### 2. Environment Setup

Ensure the backend API is running or properly mocked for E2E tests.

```bash
# Set environment variables (if needed)
export VITE_API_BASE_URL=http://localhost:5000/api
```

## Test Execution Commands

### Unit and Integration Tests (Vitest)

```bash
# Run all unit and integration tests
npm test

# Run tests in watch mode
npm test -- --watch

# Run tests with UI
npm run test:ui

# Run specific test file
npm test -- tests/unit/pages/Dashboard.test.tsx

# Generate coverage report
npm run test:coverage

# Run tests with verbose output
npm test -- --reporter=verbose
```

### E2E Tests (Playwright)

```bash
# Run all E2E tests
npx playwright test

# Run tests in headed mode (see browser)
npx playwright test --headed

# Run tests in UI mode (interactive)
npx playwright test --ui

# Run tests in debug mode
npx playwright test --debug

# Run specific test file
npx playwright test tests/e2e/auth/login.spec.ts

# Run tests in specific browser
npx playwright test --project=chromium
npx playwright test --project=firefox
npx playwright test --project=webkit

# Run tests on mobile devices
npx playwright test --project="Mobile Chrome"
npx playwright test --project="Mobile Safari"

# Run tests on tablet
npx playwright test --project="iPad"

# Run tests in parallel
npx playwright test --workers=4

# Run tests with retries
npx playwright test --retries=2

# Show test report
npx playwright show-report

# Generate HTML report
npx playwright test --reporter=html
```

### Accessibility Tests

```bash
# Run all accessibility tests
npx playwright test tests/accessibility/

# Run specific accessibility test
npx playwright test tests/accessibility/a11y.spec.ts

# Run keyboard navigation tests
npx playwright test tests/accessibility/keyboard.spec.ts

# Run with specific browser for accessibility
npx playwright test tests/accessibility/ --project=chromium
```

### Responsive Design Tests

```bash
# Run all responsive tests
npx playwright test tests/e2e/responsive/

# Run mobile tests
npx playwright test tests/e2e/responsive/mobile.spec.ts --project="Mobile Chrome"

# Run tablet tests
npx playwright test tests/e2e/responsive/tablet.spec.ts --project="iPad"

# Run desktop tests
npx playwright test tests/e2e/responsive/desktop.spec.ts
```

### Visual Regression Tests

```bash
# Run visual tests
npx playwright test tests/visual/

# Update baseline screenshots
npx playwright test tests/visual/ --update-snapshots

# Run visual tests on specific browser
npx playwright test tests/visual/ --project=chromium
```

## Test Organization

### By Module

```bash
# Authentication tests
npx playwright test tests/e2e/auth/

# Dashboard tests
npx playwright test tests/e2e/dashboard/

# Loan-related tests
npx playwright test tests/e2e/loans/

# Guarantor tests
npx playwright test tests/e2e/guarantor/

# Committee tests
npx playwright test tests/e2e/committee/

# Operations tests
npx playwright test tests/e2e/operations/

# Reports tests
npx playwright test tests/e2e/reports/
```

### By Test Type

```bash
# E2E tests
npx playwright test tests/e2e/

# Accessibility tests
npx playwright test tests/accessibility/

# Visual tests
npx playwright test tests/visual/

# Unit tests
npm test -- tests/unit/

# Integration tests
npm test -- tests/integration/
```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: UI Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'
      
      - name: Install dependencies
        run: |
          cd frontend
          npm ci
      
      - name: Install Playwright browsers
        run: |
          cd frontend
          npx playwright install --with-deps
      
      - name: Run unit tests
        run: |
          cd frontend
          npm test
      
      - name: Run E2E tests
        run: |
          cd frontend
          npx playwright test
      
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: playwright-report
          path: frontend/playwright-report/
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: frontend/coverage/lcov.info
```

## Test Reports

### Viewing Reports

```bash
# View Playwright HTML report
npx playwright show-report

# View coverage report
open frontend/coverage/index.html

# View Vitest UI
npm run test:ui
```

### Report Locations

- **Playwright Reports**: `frontend/playwright-report/`
- **Coverage Reports**: `frontend/coverage/`
- **Test Results**: `frontend/test-results/`
- **Screenshots**: `frontend/test-results/[test-name]/`
- **Videos**: `frontend/test-results/[test-name]/video.webm`

## Debugging Tests

### Playwright Debugging

```bash
# Run in debug mode
npx playwright test --debug

# Run specific test in debug mode
npx playwright test tests/e2e/auth/login.spec.ts --debug

# Run with headed browser
npx playwright test --headed --slowMo=1000

# Generate trace
npx playwright test --trace on

# View trace
npx playwright show-trace trace.zip
```

### Vitest Debugging

```bash
# Run with verbose output
npm test -- --reporter=verbose

# Run single test file
npm test -- tests/unit/pages/Dashboard.test.tsx

# Run with debugger
node --inspect-brk node_modules/.bin/vitest
```

## Common Issues and Solutions

### Issue: Playwright browsers not installed

**Solution:**
```bash
npx playwright install --force
```

### Issue: Port 5173 already in use

**Solution:**
```bash
# Kill process using port 5173
# Windows
netstat -ano | findstr :5173
taskkill /PID <PID> /F

# Linux/Mac
lsof -ti:5173 | xargs kill -9
```

### Issue: Tests timing out

**Solution:**
- Increase timeout in playwright.config.ts
- Check if backend API is running
- Verify network connectivity

### Issue: Flaky tests

**Solution:**
- Add proper wait conditions
- Use `waitForLoadState('networkidle')`
- Increase retries in CI
- Check for race conditions

### Issue: Screenshot mismatches

**Solution:**
```bash
# Update baseline screenshots
npx playwright test --update-snapshots

# Run on same OS as CI
# Use Docker for consistent environment
```

## Performance Optimization

### Parallel Execution

```bash
# Run tests in parallel with 4 workers
npx playwright test --workers=4

# Run tests in parallel with max workers
npx playwright test --workers=100%
```

### Selective Test Execution

```bash
# Run only changed tests
npx playwright test --only-changed

# Run tests matching pattern
npx playwright test --grep="login"

# Skip tests matching pattern
npx playwright test --grep-invert="slow"
```

## Best Practices

1. **Run tests locally before pushing**
   ```bash
   npm test && npx playwright test
   ```

2. **Keep tests isolated**
   - Each test should be independent
   - Clean up after tests
   - Use fresh data for each test

3. **Use proper selectors**
   - Prefer role-based selectors
   - Use data-testid for complex elements
   - Avoid CSS class selectors

4. **Handle async operations**
   - Always await async operations
   - Use proper wait conditions
   - Don't use arbitrary timeouts

5. **Mock external dependencies**
   - Mock API responses
   - Mock third-party services
   - Use consistent test data

## Test Coverage Goals

- **Code Coverage**: > 80%
- **Requirement Coverage**: 100%
- **Critical Path Coverage**: 100%
- **Accessibility Compliance**: WCAG 2.1 AA

## Monitoring and Reporting

### Generate Comprehensive Report

```bash
# Run all tests and generate reports
npm test -- --coverage && \
npx playwright test --reporter=html && \
npx playwright show-report
```

### Coverage Thresholds

Configure in `vitest.config.ts`:
```typescript
coverage: {
  lines: 80,
  functions: 80,
  branches: 80,
  statements: 80,
}
```

## Additional Resources

- [Playwright Documentation](https://playwright.dev/)
- [Vitest Documentation](https://vitest.dev/)
- [Testing Library](https://testing-library.com/)
- [Axe Accessibility](https://www.deque.com/axe/)
- [WCAG Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)

## Support

For issues or questions:
1. Check this guide
2. Review test implementation status
3. Consult team documentation
4. Reach out to QA team
