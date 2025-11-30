# Regulatory Reporting Module

## Overview

The Regulatory Reporting module provides comprehensive compliance and reporting capabilities for Nigerian financial institutions, specifically designed for Microfinance Banks (MFBs). It automates the generation of regulatory reports, manages compliance checklists, monitors regulatory alerts, and ensures adherence to CBN, FIRS, and IFRS requirements.

## Features

### 1. CBN Regulatory Reports

#### Prudential Returns
- **Endpoint**: `POST /api/v1/regulatory/reports/cbn-prudential`
- **Description**: Generates monthly prudential returns for CBN submission
- **Data Included**:
  - Total assets, liabilities, and equity
  - Total loans and non-performing loans (NPL)
  - NPL ratio calculation
  - Liquidity ratio
  - Capital adequacy ratio
  - Profit before and after tax

#### Capital Adequacy Report
- **Endpoint**: `POST /api/v1/regulatory/reports/cbn-capital-adequacy`
- **Description**: Calculates and reports capital adequacy ratio (CAR)
- **Data Included**:
  - Tier 1 and Tier 2 capital
  - Total regulatory capital
  - Risk-weighted assets
  - CAR calculation
  - Compliance status (minimum 10% for MFBs)
  - Automatic alerts if below minimum

#### Liquidity Report
- **Endpoint**: `POST /api/v1/regulatory/reports/cbn-liquidity`
- **Description**: Monitors liquidity position
- **Data Included**:
  - Liquid assets
  - Total deposits
  - Liquidity ratio
  - Compliance with minimum requirements

### 2. FIRS Tax Reports

#### VAT Returns
- **Endpoint**: `POST /api/v1/regulatory/reports/firs-vat`
- **Description**: Generates monthly VAT returns
- **Data Included**:
  - Standard-rated, zero-rated, and exempt supplies
  - Output VAT (7.5%)
  - Input VAT
  - Net VAT payable/refundable
  - Automatic tax calculation records

#### WHT Schedules
- **Description**: Withholding tax schedules for various transaction types
- **Types Supported**:
  - WHT on services (5%)
  - WHT on rent (10%)
  - WHT on dividends (10%)
  - WHT on interest (10%)

#### CIT Computation
- **Description**: Corporate Income Tax computation
- **Data Included**:
  - Turnover and cost of sales
  - Gross profit and operating expenses
  - Tax adjustments
  - Assessable profit
  - Tax payable with WHT credits

### 3. IFRS 9 ECL Reporting

#### Expected Credit Loss Calculation
- **Endpoint**: `POST /api/v1/regulatory/reports/ifrs9-ecl`
- **Description**: Calculates and reports Expected Credit Loss provisions
- **Three-Stage Model**:
  - **Stage 1**: Performing loans (0 days past due)
    - 12-month ECL
    - PD: 1%, LGD: 30%
  - **Stage 2**: Significant increase in credit risk (1-30 days past due)
    - Lifetime ECL
    - PD: 5-15%, LGD: 45%
  - **Stage 3**: Credit-impaired (>30 days past due)
    - Lifetime ECL
    - PD: 50-100%, LGD: 60%

- **Report Includes**:
  - Loan count and exposure by stage
  - Average PD and LGD by stage
  - Total ECL by stage
  - Total provision amount
  - Provision coverage ratio

### 4. Compliance Checklist

#### Checklist Management
- **Endpoint**: `GET /api/v1/regulatory/compliance/checklist`
- **Description**: Manages regulatory compliance tasks
- **Features**:
  - Create, update, and track compliance items
  - Categorize by regulator (CBN, NDIC, FIRS, IFRS, INTERNAL)
  - Set frequency (DAILY, WEEKLY, MONTHLY, QUARTERLY, ANNUALLY)
  - Assign priority (LOW, MEDIUM, HIGH, CRITICAL)
  - Assign responsible persons
  - Track completion status

#### Recurring Checklists
- **Endpoint**: `POST /api/v1/regulatory/compliance/checklist/recurring`
- **Description**: Automatically creates recurring compliance tasks
- **Auto-Generated Items**:
  - CBN Monthly Prudential Return (due 15th of next month)
  - FIRS VAT Return (due 21st of next month)
  - FIRS WHT Schedule (due 21st of next month)
  - Quarterly IFRS 9 ECL Assessment
  - Annual Financial Audit

#### Overdue Items
- **Endpoint**: `GET /api/v1/regulatory/compliance/checklist/overdue`
- **Description**: Lists all overdue compliance items
- **Auto-Updates**: Status automatically changes to OVERDUE when past due date

### 5. Regulatory Alerts

#### Alert Types
- **CAPITAL_ADEQUACY**: CAR below minimum requirement
- **LIQUIDITY_RATIO**: Liquidity ratio below threshold
- **EXPOSURE_LIMIT**: Exposure limits exceeded
- **COMPLIANCE_DEADLINE**: Upcoming compliance deadlines
- **THRESHOLD_BREACH**: Any regulatory threshold breach

#### Alert Severity
- **INFO**: Informational alerts
- **WARNING**: Requires attention
- **CRITICAL**: Immediate action required

#### Alert Management
- **Endpoint**: `GET /api/v1/regulatory/alerts`
- **Features**:
  - View all alerts with filtering
  - Acknowledge alerts
  - Add resolution notes
  - Track alert history

### 6. Compliance Dashboard

#### Dashboard Endpoint
- **Endpoint**: `GET /api/v1/regulatory/compliance/dashboard`
- **Metrics Provided**:
  - Total compliance items
  - Pending items count
  - Overdue items count
  - Items completed this month
  - Critical alerts count
  - Upcoming deadlines (next 7 days)

## API Endpoints

### Reports

```
POST   /api/v1/regulatory/reports/cbn-prudential
POST   /api/v1/regulatory/reports/cbn-capital-adequacy
POST   /api/v1/regulatory/reports/firs-vat
POST   /api/v1/regulatory/reports/ifrs9-ecl
GET    /api/v1/regulatory/reports
GET    /api/v1/regulatory/reports/:id
PATCH  /api/v1/regulatory/reports/:id/status
```

### Compliance Checklist

```
GET    /api/v1/regulatory/compliance/checklist
POST   /api/v1/regulatory/compliance/checklist
PATCH  /api/v1/regulatory/compliance/checklist/:id/status
GET    /api/v1/regulatory/compliance/checklist/overdue
POST   /api/v1/regulatory/compliance/checklist/recurring
GET    /api/v1/regulatory/compliance/dashboard
```

### Alerts

```
GET    /api/v1/regulatory/alerts
PATCH  /api/v1/regulatory/alerts/:id/acknowledge
```

## Request Examples

### Generate CBN Prudential Return

```bash
POST /api/v1/regulatory/reports/cbn-prudential
Authorization: Bearer <token>
Content-Type: application/json

{
  "periodStart": "2024-01-01",
  "periodEnd": "2024-01-31"
}
```

### Generate IFRS 9 ECL Report

```bash
POST /api/v1/regulatory/reports/ifrs9-ecl
Authorization: Bearer <token>
Content-Type: application/json

{
  "assessmentDate": "2024-01-31"
}
```

### Create Compliance Checklist Item

```bash
POST /api/v1/regulatory/compliance/checklist
Authorization: Bearer <token>
Content-Type: application/json

{
  "title": "Submit CBN Monthly Return",
  "description": "Prepare and submit monthly prudential return to CBN",
  "category": "CBN",
  "frequency": "MONTHLY",
  "dueDate": "2024-02-15",
  "priority": "HIGH",
  "responsiblePerson": "user-id-here"
}
```

### Update Checklist Status

```bash
PATCH /api/v1/regulatory/compliance/checklist/:id/status
Authorization: Bearer <token>
Content-Type: application/json

{
  "status": "COMPLETED",
  "notes": "Report submitted successfully via CBN portal"
}
```

### Acknowledge Alert

```bash
PATCH /api/v1/regulatory/alerts/:id/acknowledge
Authorization: Bearer <token>
Content-Type: application/json

{
  "resolutionNotes": "Capital injection completed to restore CAR above minimum"
}
```

## Scheduled Jobs

### Compliance Deadline Check
- **Frequency**: Daily at 9:00 AM
- **Function**: Checks for upcoming deadlines and creates alerts
- **Alert Timing**: 3 days before due date

### Recurring Checklist Creation
- **Frequency**: Monthly on 1st day
- **Function**: Creates recurring compliance tasks for the month

### Overdue Items Check
- **Frequency**: Daily at 10:00 AM
- **Function**: Updates status of overdue items

## Database Schema

### regulatory_reports
- Stores all generated regulatory reports
- Includes report data as JSON
- Tracks submission status and references

### compliance_checklists
- Manages compliance tasks and deadlines
- Tracks completion status and responsible persons
- Supports recurring task patterns

### regulatory_alerts
- Stores all regulatory alerts
- Tracks acknowledgment and resolution
- Links to related entities

### tax_calculations
- Records all tax calculations
- Tracks payment and filing status
- Supports multiple tax types

### ecl_provisions
- Stores IFRS 9 ECL calculations
- Links to loan records
- Tracks staging and provision amounts

## Permissions Required

- `regulatory:create` - Generate reports
- `regulatory:read` - View reports and alerts
- `regulatory:update` - Update report status, acknowledge alerts
- `compliance:create` - Create checklist items
- `compliance:read` - View checklist and dashboard
- `compliance:update` - Update checklist status

## Compliance Standards

### CBN Requirements
- Minimum Capital Adequacy Ratio: 10%
- Minimum Liquidity Ratio: 20%
- NPL Ratio Threshold: 5%
- Monthly reporting deadline: 15th of following month

### FIRS Requirements
- VAT Rate: 7.5%
- WHT Rates: 5-10% depending on transaction type
- Monthly filing deadline: 21st of following month

### IFRS 9 Requirements
- Three-stage impairment model
- Lifetime ECL for Stage 2 and 3
- 12-month ECL for Stage 1
- Quarterly assessment recommended

## Best Practices

1. **Regular Monitoring**: Check compliance dashboard daily
2. **Timely Reporting**: Generate reports at least 3 days before deadline
3. **Alert Response**: Acknowledge and resolve critical alerts immediately
4. **Documentation**: Add detailed notes when completing checklist items
5. **Audit Trail**: All actions are logged for regulatory audit purposes
6. **Data Accuracy**: Verify report data before submission
7. **Backup**: Export reports before filing with regulators

## Troubleshooting

### Report Generation Fails
- Check database connectivity
- Verify sufficient data for the period
- Review error logs for specific issues

### Alerts Not Appearing
- Verify scheduled jobs are running
- Check alert thresholds configuration
- Review job processor logs

### Checklist Items Not Created
- Ensure recurring job is scheduled
- Verify user permissions
- Check for duplicate items

## Future Enhancements

- [ ] NDIC regulatory reports
- [ ] Automated e-filing integration with CBN and FIRS
- [ ] Advanced analytics and trend analysis
- [ ] Predictive compliance risk scoring
- [ ] Multi-currency support for international operations
- [ ] Real-time regulatory updates and notifications
- [ ] Integration with external audit systems

## Support

For issues or questions regarding regulatory reporting:
- Review API documentation
- Check system logs
- Contact compliance team
- Refer to CBN/FIRS guidelines
