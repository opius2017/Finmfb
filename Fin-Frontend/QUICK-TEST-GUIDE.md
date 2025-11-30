# Quick Test Guide - Frontend CRUD Operations

## 🚀 Quick Start (3 Steps)

### 1. Start Backend
```bash
cd Fin-Backend
dotnet run
```

### 2. Start Frontend
```bash
cd Fin-Frontend
npm run dev
```

### 3. Run Tests
```bash
cd Fin-Frontend
npm run test:e2e
```

## 📋 Available Test Commands

| Command | Description |
|---------|-------------|
| `npm run test:e2e` | Run all E2E tests |
| `npm run test:e2e:ui` | Run tests in interactive UI mode |
| `npm run test:e2e:debug` | Run tests in debug mode |
| `npm run test:e2e:headed` | Run tests with visible browser |
| `npm run test:e2e:chromium` | Run tests in Chrome only |
| `npm run test:e2e:firefox` | Run tests in Firefox only |
| `npm run test:e2e:webkit` | Run tests in Safari only |
| `npm run test:e2e:report` | View HTML test report |
| `npm run test:crud` | Run automated test script |

## 🎯 Test Specific Features

### Test Customers Only
```bash
npx playwright test --grep "Customer Management"
```

### Test Loans Only
```bash
npx playwright test --grep "Loan Management"
```

### Test Inventory Only
```bash
npx playwright test --grep "Inventory Management"
```

### Test Accounts Only
```bash
npx playwright test --grep "Accounts"
```

### Test Payroll Only
```bash
npx playwright test --grep "Payroll"
```

## 🔍 Debug Failed Tests

### View Last Test Report
```bash
npm run test:e2e:report
```

### Run Single Test in Debug Mode
```bash
npx playwright test --grep "create a new customer" --debug
```

### Run with Visible Browser
```bash
npm run test:e2e:headed
```

## ✅ What Gets Tested

- ✅ **Customer Management**: Create, Read, Update, Delete
- ✅ **Loan Management**: Apply, View, Approve, Repay
- ✅ **Inventory Management**: Add, View, Update, Delete
- ✅ **Accounts Payable**: Create, View, Pay
- ✅ **Accounts Receivable**: Create, View, Receive Payment
- ✅ **Payroll Management**: Create, View, Process
- ✅ **Dashboard Navigation**: All dashboard pages
- ✅ **Search & Filter**: Search and filter functionality
- ✅ **Form Validation**: Required fields and validation

## 🐛 Troubleshooting

### Tests Timeout
```bash
# Increase timeout in playwright.config.ts
use: {
  actionTimeout: 30000,
  navigationTimeout: 60000,
}
```

### Backend Not Running
```bash
# Check if backend is accessible
curl http://localhost:5000/health
```

### Frontend Not Running
```bash
# Check if frontend is accessible
curl http://localhost:5173
```

### Clear Test Data
```bash
# Reset database (if needed)
cd Fin-Backend
dotnet ef database drop
dotnet ef database update
```

## 📊 Expected Results

### All Tests Pass
```
✓ Customer Management CRUD (4 tests)
✓ Loan Management CRUD (4 tests)
✓ Inventory Management CRUD (3 tests)
✓ Accounts Payable CRUD (2 tests)
✓ Accounts Receivable CRUD (2 tests)
✓ Payroll Management CRUD (2 tests)
✓ Dashboard Navigation (2 tests)
✓ Search and Filter (2 tests)
✓ Form Validation (2 tests)

Total: 23 tests passed
Time: ~4-5 minutes
```

## 🎥 View Test Execution

### Interactive UI Mode (Recommended for Development)
```bash
npm run test:e2e:ui
```

Features:
- Watch tests run in real-time
- Pause and step through tests
- Inspect DOM at any point
- Time travel through test execution
- View network requests

### Headed Mode (See Browser)
```bash
npm run test:e2e:headed
```

## 📝 Test Credentials

Default test user:
- **Email**: admin@fintech.com
- **Password**: Admin@123

## 🔄 CI/CD Integration

### GitHub Actions
```yaml
- name: Run E2E Tests
  run: |
    npm install
    npx playwright install --with-deps
    npm run test:e2e
```

### GitLab CI
```yaml
e2e-tests:
  script:
    - npm install
    - npx playwright install --with-deps
    - npm run test:e2e
  artifacts:
    when: always
    paths:
      - playwright-report/
```

## 📈 Performance

Expected execution times:
- Single test: 5-15 seconds
- Full suite (single browser): 4-5 minutes
- Full suite (all browsers): 12-15 minutes

## 🆘 Need Help?

1. Check the HTML report: `npm run test:e2e:report`
2. Run in debug mode: `npm run test:e2e:debug`
3. Check console logs in the report
4. Review screenshots/videos of failures
5. Consult CRUD-TESTING-GUIDE.md for detailed info

## 🎉 Success Indicators

All tests passing means:
- ✅ All CRUD operations work correctly
- ✅ Forms validate properly
- ✅ Navigation works across all pages
- ✅ Search and filter function correctly
- ✅ API integration is working
- ✅ Frontend is production-ready
