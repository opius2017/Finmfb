# Requirements Document

## Introduction

This document specifies the requirements for a comprehensive Cooperative Loan Management System designed for Nigerian MSMEs and cooperative societies. The system manages the complete loan lifecycle from application through disbursement, repayment tracking, and closure, with integrated savings management, guarantor verification, and payroll deduction reconciliation.

## Glossary

- **System**: The Cooperative Loan Management System
- **Member**: A registered member of the cooperative society eligible to apply for loans
- **Guarantor**: A member who provides collateral backing for another member's loan
- **Loan Committee (LC)**: The governance body responsible for reviewing and approving loan applications
- **Super Admin**: System administrator with full configuration privileges
- **EMI**: Equated Monthly Installment - the fixed monthly repayment amount
- **Net Savings**: Total member savings minus any locked equity from guarantor obligations
- **Free Equity**: Available savings that can be used as guarantor collateral
- **Deduction Rate Headroom**: The percentage of net salary available for loan deductions after statutory deductions
- **Loan Register**: Official serialized record of all approved and disbursed loans
- **Monthly Loan Threshold**: Maximum total value of loans that can be registered in a calendar month
- **Reducing Balance Method**: Interest calculation method where interest is charged only on the outstanding principal

## Requirements

### Requirement 1: Loan Application Management

**User Story:** As a cooperative member, I want to submit a loan application online so that I can request financial assistance without visiting the office.

#### Acceptance Criteria

1. WHEN a member accesses the loan application form, THE System SHALL display all required fields including member ID, requested amount, purpose, tenor, and guarantor selection
2. WHEN a member submits incomplete application data, THE System SHALL display validation errors indicating which required fields are missing
3. WHEN a member submits a complete application, THE System SHALL save the application with status "submitted" and generate a unique application number
4. WHEN an application is submitted, THE System SHALL send confirmation notification to the member via email and SMS
5. THE System SHALL link the application to the member's current savings balance and share capital for real-time eligibility verification

### Requirement 2: Automated Eligibility Verification

**User Story:** As a member, I want the system to automatically check my eligibility so that I know immediately if I qualify for the requested loan amount.

#### Acceptance Criteria

1. WHEN a member enters a loan amount, THE System SHALL calculate eligibility based on the loan type's savings multiplier (Normal: 200%, Commodity: 300%, Car: 500%)
2. WHEN checking eligibility, THE System SHALL verify the member's minimum savings balance meets the requirement for the requested amount
3. WHEN checking eligibility, THE System SHALL verify the member's membership duration meets the minimum requirement
4. WHEN checking eligibility for salaried workers, THE System SHALL calculate the deduction rate headroom after all statutory deductions (PAYE, Pension, NHF)
5. IF the member does not meet eligibility criteria, THEN THE System SHALL display specific reasons for ineligibility with required minimum values

### Requirement 3: Loan Calculator

**User Story:** As a member, I want to calculate my monthly repayment amount before applying so that I can plan my finances accordingly.

#### Acceptance Criteria

1. WHEN a member enters principal amount and tenor, THE System SHALL calculate the monthly EMI using the reducing balance method
2. WHEN calculating EMI, THE System SHALL apply the interest rate configured for the selected loan type
3. WHEN displaying calculation results, THE System SHALL show monthly EMI, total interest payable, and total repayment amount
4. WHEN displaying calculation results, THE System SHALL show the impact on the member's deduction rate headroom
5. THE System SHALL generate a complete amortization schedule showing principal and interest breakdown for each installment

### Requirement 4: Guarantor Management

**User Story:** As a member applying for a loan, I want to select guarantors from other members so that I can meet the collateral requirements.

#### Acceptance Criteria

1. WHEN selecting guarantors, THE System SHALL display only members with sufficient free equity to cover the guaranteed amount
2. WHEN a guarantor is selected, THE System SHALL send a digital consent request to the guarantor
3. WHEN a guarantor approves consent, THE System SHALL lock the guaranteed amount from their free equity
4. IF a guarantor declines consent, THEN THE System SHALL notify the applicant and allow selection of an alternative guarantor
5. THE System SHALL require a minimum of 2 guarantors for Normal Loans before application submission

### Requirement 5: Loan Committee Review Workflow

**User Story:** As a Loan Committee member, I want to review loan applications with complete member financial history so that I can make informed approval decisions.

#### Acceptance Criteria

1. WHEN reviewing an application, THE System SHALL display the member's last 5 loan applications and repayment history
2. WHEN reviewing an application, THE System SHALL display an internal repayment score based on savings consistency and past repayment behavior
3. WHEN reviewing an application, THE System SHALL display guarantor verification showing each guarantor's free equity and consent status
4. WHEN a committee member approves an application, THE System SHALL record the approval decision with timestamp and reviewer ID
5. WHEN a committee member rejects an application, THE System SHALL require a rejection reason and notify the member

### Requirement 6: Monthly Loan Threshold Management

**User Story:** As a Super Admin, I want to set monthly loan registration thresholds so that the cooperative maintains adequate liquidity.

#### Acceptance Criteria

1. THE System SHALL allow Super Admin to configure the maximum total value of loans that can be registered per calendar month
2. WHEN approved loans are queued for registration, THE System SHALL check the total against the monthly threshold
3. IF the total approved amount exceeds the monthly threshold, THEN THE System SHALL automatically move excess applications to the next month's queue
4. WHEN a new month begins, THE System SHALL automatically process queued applications against the new month's threshold
5. THE System SHALL enforce a maximum threshold limit of ₦3,000,000.00 per month

### Requirement 7: Loan Registration and Serial Number Assignment

**User Story:** As a finance officer, I want approved loans to be registered with unique serial numbers so that we maintain an official loan register.

#### Acceptance Criteria

1. WHEN a loan passes the threshold check, THE System SHALL assign a unique serial number in the format LH/YYYY/NNN
2. WHEN assigning serial numbers, THE System SHALL increment sequentially within each calendar year
3. WHEN a loan is registered, THE System SHALL add it to the read-only Loan Register with registration date
4. WHEN a loan is registered, THE System SHALL generate the complete amortization schedule
5. THE System SHALL prevent modification of registered loan records except by authorized personnel

### Requirement 8: Loan Disbursement

**User Story:** As a finance officer, I want to disburse approved loans to members' bank accounts so that they receive funds promptly.

#### Acceptance Criteria

1. WHEN disbursing a cash loan, THE System SHALL generate a Loan Agreement document with all terms and conditions
2. WHEN disbursing a cash loan, THE System SHALL record the bank transfer transaction ID and update the loan status to "disbursed"
3. WHEN disbursing a commodity loan, THE System SHALL generate a Commodity Request Voucher instead of cash transfer
4. WHEN a loan is disbursed, THE System SHALL send confirmation notification to the member with disbursement details
5. THE System SHALL update the loan ledger with the principal amount and set the first payment due date

### Requirement 9: Repayment Tracking

**User Story:** As a finance officer, I want the system to automatically track loan repayments so that member accounts are always up to date.

#### Acceptance Criteria

1. WHEN a repayment is received, THE System SHALL allocate the payment to interest first, then principal using the reducing balance method
2. WHEN processing repayments, THE System SHALL update the principal repaid, interest earned, and remaining balance
3. WHEN a scheduled payment is received, THE System SHALL mark the corresponding installment in the amortization schedule as "paid"
4. WHEN processing payroll deductions, THE System SHALL reconcile actual deductions against the expected deduction schedule
5. THE System SHALL calculate and apply late fees automatically for payments received after the due date

### Requirement 10: Delinquency Management

**User Story:** As a collections officer, I want the system to identify overdue loans automatically so that I can take timely action.

#### Acceptance Criteria

1. WHEN a payment is 3 days past due, THE System SHALL automatically send reminder notifications to the member via SMS and email
2. WHEN a payment is 7 days past due, THE System SHALL send notifications to all guarantors
3. WHEN a loan becomes delinquent, THE System SHALL calculate and apply penalty charges according to configured rates
4. WHEN a loan is delinquent, THE System SHALL update the member's repayment score negatively
5. THE System SHALL generate a delinquency report showing all overdue loans with days overdue and amounts

### Requirement 11: Deduction Schedule Management

**User Story:** As a finance officer, I want to generate monthly deduction schedules for the payroll department so that loan repayments are deducted from salaries.

#### Acceptance Criteria

1. WHEN generating a deduction schedule, THE System SHALL include member ID, name, monthly savings, ongoing loan EMIs, and new loan EMIs
2. WHEN generating a deduction schedule, THE System SHALL calculate the total monthly deduction as the sum of savings and all loan EMIs
3. WHEN generating a deduction schedule, THE System SHALL export the data in Excel format with all required columns
4. WHEN actual deductions are uploaded, THE System SHALL import the Excel file and match records by member ID
5. WHEN reconciling deductions, THE System SHALL identify discrepancies between expected and actual amounts and generate a variance report

### Requirement 12: Loan Configuration Management

**User Story:** As a Super Admin, I want to configure loan parameters so that the system adapts to changing cooperative policies.

#### Acceptance Criteria

1. THE System SHALL allow Super Admin to set the annual interest rate for each loan type independently
2. THE System SHALL allow Super Admin to configure the maximum deduction rate percentage (e.g., 45% of net salary)
3. THE System SHALL allow Super Admin to set the loan-to-savings multiplier for each loan type
4. THE System SHALL allow Super Admin to override the standard multiplier for specific applications with committee approval
5. THE System SHALL allow Super Admin to configure the maximum tenor (in months) for each loan type

### Requirement 13: Commodity Store Integration

**User Story:** As a member, I want to browse available items in the cooperative store and request them as commodity loans so that I can acquire goods on credit.

#### Acceptance Criteria

1. WHEN browsing the store, THE System SHALL display all available items with name, description, price, and stock quantity
2. WHEN a member requests a commodity item, THE System SHALL use the item's selling price as the principal loan amount
3. WHEN processing a commodity request, THE System SHALL check eligibility against the 300% savings multiplier rule
4. WHEN a commodity request is approved, THE System SHALL generate a voucher and reduce the item stock quantity
5. WHEN a commodity loan is fulfilled, THE System SHALL activate the repayment schedule identical to cash loans

### Requirement 14: Member Savings Management

**User Story:** As a member, I want to adjust my monthly savings contribution so that I can increase my loan eligibility.

#### Acceptance Criteria

1. THE System SHALL allow members to request increases or decreases to their monthly savings contribution
2. WHEN a member requests a savings adjustment, THE System SHALL verify the new amount does not exceed the deduction rate ceiling
3. WHEN a savings adjustment is requested, THE System SHALL require management approval before implementation
4. WHEN a savings adjustment is approved, THE System SHALL include the new amount in the next month's deduction schedule
5. THE System SHALL maintain mandatory monthly savings contributions even when a member has an active loan

### Requirement 15: Loan Closure and Clearance

**User Story:** As a member, I want to receive a clearance certificate when I complete my loan repayment so that I have proof of full settlement.

#### Acceptance Criteria

1. WHEN the final loan payment is received, THE System SHALL verify the outstanding balance is zero
2. WHEN a loan is fully repaid, THE System SHALL generate a Loan Clearance Certificate with unique certificate number
3. WHEN a loan is closed, THE System SHALL release all guarantor liabilities and unlock their equity
4. WHEN a loan is closed, THE System SHALL update the loan status to "Closed" and archive all records
5. THE System SHALL send the clearance certificate to the member via email and make it available for download

### Requirement 16: Reporting and Analytics

**User Story:** As a cooperative manager, I want to view loan portfolio reports so that I can monitor the health of our lending operations.

#### Acceptance Criteria

1. THE System SHALL generate a loan portfolio summary showing total loans, outstanding balance, and repayment rate
2. THE System SHALL generate a delinquency report showing all overdue loans grouped by days overdue
3. THE System SHALL generate a monthly disbursement report showing all loans disbursed in the period
4. THE System SHALL generate a collections report showing all repayments received in the period
5. THE System SHALL generate a loan register report showing all registered loans with serial numbers and status

### Requirement 17: Audit Trail and Compliance

**User Story:** As an auditor, I want to view complete audit trails for all loan transactions so that I can verify compliance with regulations.

#### Acceptance Criteria

1. THE System SHALL log all loan application submissions with timestamp and user ID
2. THE System SHALL log all approval and rejection decisions with reviewer ID and comments
3. THE System SHALL log all disbursements with transaction IDs and bank details
4. THE System SHALL log all repayment transactions with payment method and reference
5. THE System SHALL maintain immutable audit logs that cannot be modified or deleted

### Requirement 18: Notification System

**User Story:** As a member, I want to receive timely notifications about my loan status so that I stay informed throughout the process.

#### Acceptance Criteria

1. WHEN an application is submitted, THE System SHALL send confirmation notification to the member
2. WHEN an application is approved or rejected, THE System SHALL send decision notification to the member
3. WHEN a loan is disbursed, THE System SHALL send disbursement confirmation with amount and terms
4. WHEN a payment is due in 3 days, THE System SHALL send payment reminder notification
5. WHEN a payment is overdue, THE System SHALL send delinquency notification to member and guarantors

### Requirement 19: Security and Access Control

**User Story:** As a system administrator, I want role-based access control so that users only access features appropriate to their role.

#### Acceptance Criteria

1. THE System SHALL restrict loan application submission to authenticated members only
2. THE System SHALL restrict loan approval functions to Loan Committee members only
3. THE System SHALL restrict configuration changes to Super Admin role only
4. THE System SHALL restrict disbursement functions to authorized finance officers only
5. THE System SHALL log all access attempts and maintain a security audit trail

### Requirement 20: Data Validation and Integrity

**User Story:** As a system user, I want the system to validate all data entries so that errors are prevented before they cause issues.

#### Acceptance Criteria

1. THE System SHALL validate that loan amounts are positive numbers and within configured limits
2. THE System SHALL validate that tenor values are within the allowed range for the selected loan type
3. THE System SHALL validate that guarantor free equity is sufficient before allowing selection
4. THE System SHALL validate that monthly deduction totals do not exceed the member's deduction rate ceiling
5. THE System SHALL prevent duplicate loan registrations for the same application
