# Testing Setup Guide

## Installation

To set up the testing infrastructure, run the following command in the `frontend` directory:

```bash
npm install --save-dev @playwright/test @axe-core/playwright @testing-library/user-event msw happy-dom @vitest/ui @vitest/coverage-v8
```

After installing the dependencies, install Playwright browsers:

```bash
npx playwright install
```

## Test Structure

The test suite is organized as follows:

```
frontend/tests/
├── e2e/                    # End-to-end tests with Playwright
│   ├── auth/              # Authentication tests
│   ├── dashboard/         # Dashboard tests
│   ├── loans/             # Loan-related tests
│   ├── guarantor/         # Guarantor tests
│   ├── committee/         # Committee tests
│   ├── operations/        # Operations tests
│   └── reports/           # Reports tests
├── integration/           # Integration tests
├── unit/                  # Unit tests
│   ├── components/        # Component tests
│   └── pages/             # Page tests
├── accessibility/         # Accessibility tests
├── visual/                # Visual regression tests
└── utils/                 # Test utilities and helpers
```

## Running Tests

### Unit and Integration Tests (Vitest)

```bash
# Run all unit and integration tests
npm test

# Run tests in watch mode
npm run test:watch

# Run tests with UI
npm run test:ui

# Generate coverage report
npm run test:coverage
```

### E2E Tests (Playwright)

```bash
# Run all E2E tests
npx playwright test

# Run tests in headed mode (see browser)
npx playwright test --headed

# Run tests in specific browser
npx playwright test --project=chromium

# Run specific test file
npx playwright test tests/e2e/auth/login.spec.ts

# Run tests in debug mode
npx playwright test --debug

# Show test report
npx playwright show-report
```

### Accessibility Tests

```bash
# Run accessibility tests
npx playwright test tests/accessibility/

# Run with specific browser
npx playwright test tests/accessibility/ --project=chromium
```

### Visual Regression Tests

```bash
# Run visual tests
npx playwright test tests/visual/

# Update baseline screenshots
npx playwright test tests/visual/ --update-snapshots
```

## Configuration Files

- `playwright.config.ts` - Playwright E2E test configuration
- `vitest.config.ts` - Vitest unit/integration test configuration
- `tests/setup.ts` - Test environment setup and global mocks

## Test Utilities

Test utilities and helpers are located in `tests/utils/`:

- `mock-data.ts` - Mock data factory for test fixtures
- `custom-render.tsx` - Custom render function with providers
- `test-helpers.ts` - Common test helper functions

## CI/CD Integration

Tests are automatically run in the CI/CD pipeline:

1. Lint code
2. Run unit tests
3. Run integration tests
4. Run E2E tests (parallel across browsers)
5. Run accessibility tests
6. Generate coverage reports
7. Upload test artifacts

## Best Practices

1. **Isolation**: Each test should be independent and not rely on other tests
2. **Cleanup**: Always clean up after tests (handled automatically)
3. **Mocking**: Use mock data and API responses for consistent testing
4. **Assertions**: Use descriptive assertions with clear error messages
5. **Selectors**: Prefer data-testid or role-based selectors over CSS classes
6. **Async**: Always await async operations properly
7. **Coverage**: Aim for >80% code coverage
8. **Performance**: Keep tests fast and focused

## Troubleshooting

### Playwright Issues

If Playwright tests fail to start:
```bash
# Reinstall browsers
npx playwright install --force

# Clear cache
npx playwright install --force chromium
```

### Vitest Issues

If Vitest tests fail:
```bash
# Clear cache
npm run test -- --clearCache

# Run with verbose output
npm run test -- --reporter=verbose
```

### Port Conflicts

If the dev server fails to start:
- Check if port 5173 is already in use
- Kill the process using the port or change the port in vite.config.ts

## Additional Resources

- [Playwright Documentation](https://playwright.dev/)
- [Vitest Documentation](https://vitest.dev/)
- [Testing Library Documentation](https://testing-library.com/)
- [Axe Accessibility Testing](https://www.deque.com/axe/)
