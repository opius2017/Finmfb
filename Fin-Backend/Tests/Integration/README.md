# Integration Tests - Cooperative Loan Management System

## Overview

This directory contains comprehensive integration tests for the Cooperative Loan Management System. These tests validate the complete workflows and interactions between services, ensuring the system functions correctly end-to-end.

## Test Suites

### 1. LoanWorkflowIntegrationTests.cs
Tests the complete loan lifecycle from application to repayment:
- **Complete Normal Loan Workflow**: Tests the entire process including eligibility check, guarantor consent, committee review, loan calculation, and verification
- **Eligibility Validation**: Tests insufficient savings scenarios
- **Deduction Rate Validation**: Tests scenarios where deduction rates exceed limits
- **Loan Calculation**: Validates reducing balance amortization schedules
- **Payment Allocation**: Tests the correct allocation order (Penalty → Interest → Principal)
- **Guarantor Eligibility**: Tests insufficient free equity scenarios

**Key Features Tested:**
- Savings multiplier validation (200%, 300%, 500%)
- Membership duration checks
- Deduction rate headroom calculation
- Guarantor equity locking mechanism
- Committee review workflow
- Reducing balance EMI calculations

### 2. RepaymentWorkflowIntegrationTests.cs
Tests repayment processing and delinquency management:
- **Complete Repayment Workflow**: Tests full monthly payment processing
- **Partial Payments**: Validates correct allocation of partial payments
- **Loan Closure**: Tests final payment and loan completion
- **Delinquency Detection**: Tests overdue loan identification and penalty application
- **Classification Changes**: Tests CBN-compliant loan classification (Performing → Special Mention → Substandard → Doubtful → Loss)
- **Daily Delinquency Checks**: Tests bulk processing of multiple loans
- **Penalty Calculations**: Validates penalty amounts over time
- **Overdue Filtering**: Tests filtering loans by days overdue

**Key Features Tested:**
- Interest-first payment allocation
- Reducing balance updates
- Amortization schedule updates
- Penalty calculation (0.1% per day)
- CBN classification rules (30, 90, 180, 360 days)
- Bulk delinquency processing

### 3. ServiceIntegrationTests.cs
Tests individual service integrations and calculations:
- **Loan Calculator Service**: Tests EMI, interest, and amortization calculations
- **Eligibility Service**: Tests savings multiplier validation for different loan types (NORMAL, COMMODITY, CAR)
- **Payment Allocation**: Tests correct allocation across penalty, interest, and principal
- **Delinquency Classification**: Tests penalty calculations for different overdue periods
- **Early Repayment**: Tests early settlement calculations with accrued interest

**Test Coverage:**
- 6 different loan type scenarios (Theory tests)
- Reducing balance verification
- Payment allocation logic
- Early repayment calculations

### 4. PerformanceIntegrationTests.cs
Tests system performance under load:
- **Loan Calculation Performance**: Tests 1000 calculations per scenario (< 10ms average)
- **Concurrent Eligibility Checks**: Tests 100 concurrent requests (< 50ms average)
- **Large Tenor Schedules**: Tests 120-month amortization generation (< 100ms)
- **Bulk Calculations**: Tests 500 simultaneous loan calculations (< 20ms average)
- **Payment Allocation Performance**: Tests 1000 payment allocations (< 5ms average)
- **End-to-End Scenarios**: Tests 100 complete loan scenarios (< 100ms average)

**Performance Benchmarks:**
- EMI calculation: < 10ms
- Eligibility check: < 50ms
- Amortization schedule (120 months): < 100ms
- Bulk processing: > 50 loans/second
- Payment allocation: < 5ms

### 5. IntegrationTestFixture.cs
Provides test infrastructure:
- In-memory database setup
- Service provider configuration
- Dependency injection setup
- Test data seeding
- Shared test context

## Running the Tests

### Run All Integration Tests
```bash
dotnet test Fin-Backend/Tests/Integration --filter Category=Integration
```

### Run Specific Test Suite
```bash
# Loan workflow tests
dotnet test --filter FullyQualifiedName~LoanWorkflowIntegrationTests

# Repayment tests
dotnet test --filter FullyQualifiedName~RepaymentWorkflowIntegrationTests

# Service tests
dotnet test --filter FullyQualifiedName~ServiceIntegrationTests

# Performance tests
dotnet test --filter FullyQualifiedName~PerformanceIntegrationTests
```

### Run Specific Test
```bash
dotnet test --filter "FullyQualifiedName~CompleteNormalLoanWorkflow_Success"
```

## Test Data

All tests use in-memory test data created through helper methods:
- **CreateTestMember**: Creates member entities with specified savings
- **CreateTestLoanApplication**: Creates loan application entities
- **CreateTestLoan**: Creates active loan entities
- **CreateTestSchedules**: Creates repayment schedule entries

## Key Validations

### Cooperative-Specific Rules
1. **Savings Multiplier**:
   - NORMAL loans: 200% (₦100k savings → ₦200k max loan)
   - COMMODITY loans: 300% (₦100k savings → ₦300k max loan)
   - CAR loans: 500% (₦100k savings → ₦500k max loan)

2. **Deduction Rate**:
   - Maximum 45% of net salary
   - Includes existing deductions + new loan EMI

3. **Guarantor Equity**:
   - Free equity = Total savings - Locked equity
   - Must have sufficient free equity to guarantee

4. **Loan Classification** (CBN Guidelines):
   - Performing: 0-30 days overdue
   - Special Mention: 31-90 days
   - Substandard: 91-180 days
   - Doubtful: 181-360 days
   - Loss: > 360 days

5. **Payment Allocation Order**:
   - 1st: Penalty charges
   - 2nd: Accrued interest
   - 3rd: Principal amount
   - 4th: Overpayment (if any)

## Test Coverage

- **Loan Workflow**: 6 test cases
- **Repayment Workflow**: 8 test cases
- **Service Integration**: 5 test cases (including 6 theory variations)
- **Performance**: 6 test cases
- **Total**: 25+ test scenarios

## Dependencies

- xUnit: Test framework
- Moq: Mocking framework
- Microsoft.EntityFrameworkCore.InMemory: In-memory database
- Microsoft.Extensions.DependencyInjection: DI container

## Notes

- All tests use in-memory databases for isolation
- Tests are independent and can run in parallel
- Performance tests include benchmarks and assertions
- All monetary values use Nigerian Naira (₦)
- Tests follow AAA pattern (Arrange, Act, Assert)

## Future Enhancements

- Add tests for notification delivery
- Add tests for commodity loan workflows
- Add tests for loan register serial number generation
- Add tests for monthly threshold management
- Add tests for committee voting mechanisms
- Add load tests with realistic data volumes
