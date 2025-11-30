# 🚀 Run Frontend CRUD Tests NOW

## ⚡ Quick Execution (3 Commands)

### Step 1: Install Playwright Browsers (One-time setup)
```bash
cd Fin-Frontend
npx playwright install --with-deps chromium
```

### Step 2: Start Backend (Terminal 1)
```bash
cd Fin-Backend
dotnet run
```
**Wait for**: `Now listening on: http://localhost:5000`

### Step 3: Start Frontend (Terminal 2)
```bash
cd Fin-Frontend
npm run dev
```
**Wait for**: `Local: http://localhost:5173/`

### Step 4: Run Tests (Terminal 3)
```bash
cd Fin-Frontend
npm run test:e2e
```

## 🎯 Expected Output

```
Running 23 tests using 1 worker

✓ Customer Management CRUD › should create a new customer (12s)
✓ Customer Management CRUD › should read/view customer details (8s)
✓ Customer Management CRUD › should update customer information (10s)
✓ Customer Management CRUD › should delete a customer (9s)
✓ Loan Management CRUD › should create a new loan application (15s)
✓ Loan Management CRUD › should view loan details (7s)
✓ Loan Management CRUD › should update loan status (11s)
✓ Loan Management CRUD › should process loan repayment (13s)
✓ Inventory Management CRUD › should create a new inventory item (10s)
✓ Inventory Management CRUD › should update inventory quantity (9s)
✓ Inventory Management CRUD › should delete inventory item (8s)
✓ Accounts Payable CRUD › should create a new payable entry (11s)
✓ Accounts Payable CRUD › should mark payable as paid (10s)
✓ Accounts Receivable CRUD › should create a new receivable entry (11s)
✓ Accounts Receivable CRUD › should record payment received (10s)
✓ Payroll Management CRUD › should create a new payroll entry (10s)
✓ Payroll Management CRUD › should process payroll payment (9s)
✓ Dashboard Navigation › should navigate to all dashboard pages (8s)
✓ Dashboard Navigation › should display correct metrics on dashboards (7s)
✓ Search and Filter › should search customers (6s)
✓ Search and Filter › should filter loans by status (7s)
✓ Form Validation › should validate required fields in customer form (5s)
✓ Form Validation › should validate loan amount limits (6s)

23 passed (4m 32s)
```

## 🎨 Alternative: Interactive UI Mode (Recommended for First Run)

```bash
cd Fin-Frontend
npm run test:e2e:ui
```

This opens an interactive interface where you can:
- ✅ See tests run in real-time
- ✅ Pause and inspect at any point
- ✅ View DOM and network requests
- ✅ Time-travel through test execution
- ✅ Debug failures easily

## 📊 View Test Report

After tests complete:
```bash
npm run test:e2e:report
```

This opens an HTML report showing:
- ✅ Pass/fail status for each test
- ✅ Execution time
- ✅ Screenshots (on failure)
- ✅ Videos (on failure)
- ✅ Detailed traces

## 🐛 If Tests Fail

### Check Backend is Running
```bash
curl http://localhost:5000/health
```
**Expected**: `{"status":"Healthy"}`

### Check Frontend is Running
```bash
curl http://localhost:5173
```
**Expected**: HTML content

### Run in Debug Mode
```bash
npm run test:e2e:debug
```

### Run in Headed Mode (See Browser)
```bash
npm run test:e2e:headed
```

## 🎯 Test Specific Features

### Test Only Customers
```bash
npx playwright test --grep "Customer Management"
```

### Test Only Loans
```bash
npx playwright test --grep "Loan Management"
```

### Test Only Inventory
```bash
npx playwright test --grep "Inventory Management"
```

## 📝 Test Credentials

The tests use these credentials:
- **Email**: admin@fintech.com
- **Password**: Admin@123

Make sure this user exists in your database!

## ⚙️ Troubleshooting

### Issue: "Playwright not found"
```bash
npm install
npx playwright install --with-deps chromium
```

### Issue: "Backend not responding"
```bash
# Check if backend is running
cd Fin-Backend
dotnet run
```

### Issue: "Frontend not responding"
```bash
# Check if frontend is running
cd Fin-Frontend
npm run dev
```

### Issue: "Tests timeout"
Edit `playwright.config.ts`:
```typescript
use: {
  actionTimeout: 30000,  // Increase from 10000
  navigationTimeout: 60000,  // Increase from 30000
}
```

### Issue: "Authentication fails"
1. Check test credentials exist in database
2. Check backend authentication is working
3. Try logging in manually at http://localhost:5173/login

## 🎉 Success Criteria

When all 23 tests pass, it confirms:
- ✅ All CRUD operations work correctly
- ✅ Forms validate properly
- ✅ Navigation works across all pages
- ✅ Search and filter function correctly
- ✅ API integration is working
- ✅ Frontend is production-ready

## 📚 More Information

- **Comprehensive Guide**: `CRUD-TESTING-GUIDE.md`
- **Quick Reference**: `QUICK-TEST-GUIDE.md`
- **Implementation Summary**: `FRONTEND-TESTING-COMPLETE.md`

## 🚀 Ready to Run!

Everything is set up and ready. Just follow the 4 steps above and watch your tests run!

```bash
# Terminal 1
cd Fin-Backend && dotnet run

# Terminal 2
cd Fin-Frontend && npm run dev

# Terminal 3
cd Fin-Frontend && npm run test:e2e:ui
```

**Good luck! 🎉**
