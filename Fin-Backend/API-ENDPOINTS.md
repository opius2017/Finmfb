# Cooperative Loan Management System - API Endpoints

## Overview
Complete REST API documentation for the Cooperative Loan Management System.

**Base URL:** `https://api.yourdomain.com/api`

**Authentication:** All endpoints require JWT Bearer token unless specified otherwise.

---

## 1. Loan Application Controller
**Base Path:** `/api/LoanApplication`

### Check Eligibility
- **POST** `/check-eligibility`
- **Description:** Checks if a member is eligible for a loan
- **Request Body:**
```json
{
  "requestedAmount": 100000,
  "loanType": "NORMAL",
  "interestRate": 15,
  "tenorMonths": 12,
  "memberId": "member-id",
  "memberTotalSavings": 50000,
  "membershipDate": "2020-01-01",
  "netSalary": 200000,
  "existingMonthlyDeductions": 50000
}
```
- **Response:** `EligibilityCheckResult`

### Calculate Loan
- **POST** `/calculate`
- **Description:** Calculates EMI and generates amortization schedule
- **Request Body:**
```json
{
  "principal": 100000,
  "interestRate": 15,
  "tenorMonths": 12,
  "startDate": "2024-01-01"
}
```
- **Response:** `LoanCalculationSummary`

### Calculate Deduction Impact
- **POST** `/calculate-deduction-impact`
- **Description:** Calculates impact on salary deduction rate
- **Request Body:**
```json
{
  "loanAmount": 100000,
  "interestRate": 15,
  "tenorMonths": 12,
  "netSalary": 200000,
  "existingDeductions": 50000,
  "maxDeductionRate": 45
}
```
- **Response:** `DeductionRateImpact`

### Generate Eligibility Report
- **POST** `/eligibility-report`
- **Description:** Generates comprehensive eligibility report
- **Response:** `EligibilityReport`

---

## 2. Guarantor Controller
**Base Path:** `/api/Guarantor`

### Check Guarantor Eligibility
- **GET** `/check-eligibility/{guarantorMemberId}?guaranteedAmount=50000`
- **Description:** Checks if a member can be a guarantor
- **Response:** `GuarantorEligibilityResult`

### Send Consent Request
- **POST** `/send-consent-request`
- **Description:** Sends digital consent request to guarantor
- **Request Body:**
```json
{
  "loanApplicationId": "app-id",
  "guarantorMemberId": "member-id",
  "guaranteedAmount": 50000
}
```
- **Response:** `GuarantorConsentRequest`

### Approve Consent
- **POST** `/approve-consent/{consentId}`
- **Description:** Approves guarantor consent (locks equity)
- **Request Body:**
```json
{
  "notes": "I agree to guarantee this loan"
}
```
- **Response:** `GuarantorConsentResponse`

### Reject Consent
- **POST** `/reject-consent/{consentId}`
- **Description:** Rejects guarantor consent
- **Request Body:**
```json
{
  "rejectionReason": "Insufficient free equity"
}
```
- **Response:** `GuarantorConsentResponse`

### Get Guarantor Dashboard
- **GET** `/dashboard/{memberId}`
- **Description:** Gets guarantor obligations and free equity
- **Response:** `GuarantorDashboard`

### Release All Guarantors
- **POST** `/release-all/{loanApplicationId}`
- **Description:** Releases all guarantors for an application
- **Authorization:** Admin, Finance
- **Response:** Success message

---

## 3. Committee Controller
**Base Path:** `/api/Committee`
**Authorization:** Committee, Admin roles required

### Get Member Credit Profile
- **GET** `/member-profile/{memberId}`
- **Description:** Gets comprehensive credit profile for review
- **Response:** `MemberCreditProfile`

### Submit Review
- **POST** `/submit-review`
- **Description:** Submits committee member review
- **Request Body:**
```json
{
  "loanApplicationId": "app-id",
  "decision": "APPROVE",
  "recommendedAmount": 100000,
  "comments": "Good repayment history",
  "votingWeight": 1
}
```
- **Response:** `CommitteeReviewResult`

### Get Application Reviews
- **GET** `/application-reviews/{loanApplicationId}`
- **Description:** Gets all reviews for an application
- **Response:** `List<CommitteeReviewSummary>`

### Evaluate Decision
- **GET** `/evaluate-decision/{loanApplicationId}`
- **Description:** Evaluates committee decision based on votes
- **Response:** `CommitteeDecision`

### Get Committee Dashboard
- **GET** `/dashboard`
- **Description:** Gets pending applications for review
- **Response:** `CommitteeDashboard`

### Get Repayment Score
- **GET** `/repayment-score/{memberId}`
- **Description:** Calculates member's repayment score
- **Response:** `{ memberId, repaymentScore }`

---

## 4. Disbursement Controller
**Base Path:** `/api/Disbursement`
**Authorization:** Finance, Admin roles required

### Disburse Cash Loan
- **POST** `/disburse-cash-loan`
- **Description:** Disburses approved loan to member's account
- **Request Body:**
```json
{
  "loanApplicationId": "app-id",
  "bankTransferReference": "TXN123456",
  "notes": "Disbursement approved"
}
```
- **Response:** `DisbursementResult`

### Track Transaction
- **GET** `/track-transaction/{transactionId}`
- **Description:** Tracks disbursement transaction status
- **Response:** `TransactionTrackingResult`

### Get Disbursement History
- **GET** `/history/{memberId}`
- **Description:** Gets member's disbursement history
- **Response:** `DisbursementHistory`

### Get Loan Register
- **GET** `/loan-register?fromDate=2024-01-01&toDate=2024-12-31&status=ACTIVE`
- **Description:** Gets complete loan register
- **Response:** `List<LoanRegisterEntry>`

### Get Loan by Serial Number
- **GET** `/loan-register/{serialNumber}`
- **Description:** Gets loan details by serial number (e.g., LH/2024/001)
- **Response:** `LoanRegisterEntry`

### Export Loan Register
- **GET** `/export-register?fromDate=2024-01-01&toDate=2024-12-31`
- **Description:** Exports loan register to CSV
- **Response:** CSV file download

### Get Register Statistics
- **GET** `/register-statistics?year=2024`
- **Description:** Gets loan register statistics
- **Response:** `LoanRegisterStatistics`

### Check Threshold
- **POST** `/check-threshold`
- **Description:** Checks monthly threshold availability
- **Request Body:**
```json
{
  "loanAmount": 100000,
  "targetMonth": 12,
  "targetYear": 2024
}
```
- **Response:** `ThresholdCheckResult`

### Get Threshold Info
- **GET** `/threshold-info?month=12&year=2024`
- **Description:** Gets monthly threshold information
- **Response:** `MonthlyThresholdInfo`

---

## 5. Repayment Controller
**Base Path:** `/api/Repayment`

### Process Repayment
- **POST** `/process-payment`
- **Description:** Processes loan repayment (reducing balance)
- **Request Body:**
```json
{
  "loanId": "loan-id",
  "amount": 10000,
  "paymentMethod": "BANK_TRANSFER",
  "transactionReference": "TXN123456",
  "notes": "Monthly payment"
}
```
- **Response:** `RepaymentResult`

### Process Partial Payment
- **POST** `/process-partial-payment`
- **Description:** Processes partial loan payment
- **Request Body:** Same as Process Repayment
- **Response:** `RepaymentResult`

### Get Repayment History
- **GET** `/history/{loanId}`
- **Description:** Gets complete repayment history
- **Response:** `RepaymentHistory`

### Get Repayment Schedule
- **GET** `/schedule/{loanId}`
- **Description:** Gets amortization schedule
- **Response:** `List<RepaymentScheduleItem>`

---

## 6. Delinquency Controller
**Base Path:** `/api/Delinquency`

### Perform Daily Check
- **POST** `/daily-check`
- **Description:** Performs daily delinquency check (scheduled job)
- **Authorization:** Admin, System roles
- **Response:** `DelinquencyCheckResult`

### Check Loan Delinquency
- **GET** `/check-loan/{loanId}`
- **Description:** Checks delinquency status for specific loan
- **Response:** `LoanDelinquencyStatus`

### Get Overdue Loans
- **GET** `/overdue-loans?minDaysOverdue=7`
- **Description:** Gets all overdue loans
- **Authorization:** Finance, Admin, Collections
- **Response:** `List<OverdueLoan>`

### Apply Penalty
- **POST** `/apply-penalty/{loanId}`
- **Description:** Applies penalty charges to overdue loan
- **Authorization:** Finance, Admin
- **Response:** `PenaltyApplicationResult`

### Send Early Warning
- **POST** `/send-early-warning/{loanId}`
- **Description:** Sends 3-day overdue warning to borrower
- **Authorization:** Finance, Admin, Collections
- **Response:** Success message

### Notify Guarantors
- **POST** `/notify-guarantors/{loanId}`
- **Description:** Sends 7-day overdue notification to guarantors
- **Authorization:** Finance, Admin, Collections
- **Response:** Success message

### Generate Delinquency Report
- **GET** `/report?asOfDate=2024-12-31`
- **Description:** Generates comprehensive delinquency report
- **Authorization:** Finance, Admin, Management
- **Response:** `DelinquencyReport`

---

## 7. Admin Controller
**Base Path:** `/api/loans/Admin`
**Authorization:** Admin, SuperAdmin roles required

### Set Monthly Threshold
- **POST** `/set-threshold`
- **Description:** Sets monthly loan disbursement threshold
- **Request Body:**
```json
{
  "month": 12,
  "year": 2024,
  "maxLoanAmount": 3000000
}
```
- **Response:** `MonthlyThresholdResult`

### Perform Monthly Rollover
- **POST** `/monthly-rollover`
- **Description:** Processes queued applications for new month
- **Response:** `MonthlyRolloverResult`

### Get Threshold Alerts
- **GET** `/threshold-alerts`
- **Description:** Gets threshold breach alerts (75%, 90%)
- **Response:** `List<ThresholdAlert>`

### Release Threshold Allocation
- **POST** `/release-threshold-allocation`
- **Description:** Releases allocated threshold (when loan rejected)
- **Request Body:**
```json
{
  "loanAmount": 100000,
  "month": 12,
  "year": 2024
}
```
- **Response:** Success message

---

## Authentication

All endpoints require JWT Bearer token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

### Roles
- **Member**: Basic member access
- **Committee**: Loan review and approval
- **Finance**: Disbursement and repayment processing
- **Collections**: Delinquency management
- **Admin**: Administrative functions
- **SuperAdmin**: Full system access
- **System**: Scheduled job access

---

## Error Responses

All endpoints return consistent error format:

```json
{
  "error": "Error message description"
}
```

**HTTP Status Codes:**
- `200 OK`: Success
- `400 Bad Request`: Invalid request data
- `401 Unauthorized`: Missing or invalid token
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

---

## Rate Limiting

- **Default**: 100 requests per minute per user
- **Admin endpoints**: 200 requests per minute
- **Public endpoints**: 50 requests per minute

---

## Pagination

List endpoints support pagination:

```
?page=1&pageSize=20
```

---

## Webhooks (Future)

Webhook events for external integrations:
- `loan.application.submitted`
- `loan.approved`
- `loan.disbursed`
- `loan.repayment.received`
- `loan.overdue`
- `loan.closed`

---

## Support

For API support, contact: api-support@yourdomain.com

**API Version:** v1.0
**Last Updated:** 2024-11-30
