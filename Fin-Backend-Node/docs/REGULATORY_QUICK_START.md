# Regulatory Reporting - Quick Start Guide

## Setup (5 minutes)

### 1. Run Database Migration

```bash
# Connect to your SQL Server database
sqlcmd -S localhost -d FinTechDB -U sa -P YourPassword

# Run the migration
:r prisma/migrations/add_regulatory_reporting.sql
GO
```

### 2. Configure Permissions

```sql
-- Add regulatory permissions
INSERT INTO permissions (id, name, resource, action, description, created_at, updated_at)
VALUES 
  (NEWID(), 'regulatory:create', 'regulatory', 'create', 'Generate regulatory reports', GETDATE(), GETDATE()),
  (NEWID(), 'regulatory:read', 'regulatory', 'read', 'View regulatory reports', GETDATE(), GETDATE()),
  (NEWID(), 'regulatory:update', 'regulatory', 'update', 'Update regulatory reports', GETDATE(), GETDATE()),
  (NEWID(), 'compliance:create', 'compliance', 'create', 'Create compliance items', GETDATE(), GETDATE()),
  (NEWID(), 'compliance:read', 'compliance', 'read', 'View compliance items', GETDATE(), GETDATE()),
  (NEWID(), 'compliance:update', 'compliance', 'update', 'Update compliance items', GETDATE(), GETDATE());

-- Assign to admin role (replace 'admin-role-id' with actual role ID)
INSERT INTO role_permissions (role_id, permission_id, created_at)
SELECT 'admin-role-id', id, GETDATE()
FROM permissions
WHERE resource IN ('regulatory', 'compliance');
```

### 3. Start the Server

```bash
npm run dev
```

## Quick Test (2 minutes)

### 1. Get Access Token

```bash
curl -X POST http://localhost:3000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "your-password"
  }'
```

Save the `access_token` from the response.

### 2. Generate a Report

```bash
# Generate CBN Prudential Return
curl -X POST http://localhost:3000/api/v1/regulatory/reports/cbn-prudential \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -d '{
    "periodStart": "2024-01-01",
    "periodEnd": "2024-01-31"
  }'
```

### 3. View Compliance Dashboard

```bash
curl -X GET http://localhost:3000/api/v1/regulatory/compliance/dashboard \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

## Common Use Cases

### Generate Monthly CBN Report

```javascript
const response = await fetch('/api/v1/regulatory/reports/cbn-prudential', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({
    periodStart: '2024-01-01',
    periodEnd: '2024-01-31'
  })
});

const report = await response.json();
console.log('Report generated:', report.data);
```

### Create Compliance Task

```javascript
const response = await fetch('/api/v1/regulatory/compliance/checklist', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({
    title: 'Submit CBN Monthly Return',
    description: 'Prepare and submit monthly prudential return',
    category: 'CBN',
    frequency: 'MONTHLY',
    dueDate: '2024-02-15',
    priority: 'HIGH'
  })
});
```

### Check for Alerts

```javascript
const response = await fetch('/api/v1/regulatory/alerts?isAcknowledged=false', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

const alerts = await response.json();
console.log('Unacknowledged alerts:', alerts.data);
```

### Generate IFRS 9 ECL Report

```javascript
const response = await fetch('/api/v1/regulatory/reports/ifrs9-ecl', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({
    assessmentDate: '2024-01-31'
  })
});

const eclReport = await response.json();
console.log('ECL Report:', eclReport.data);
```

## Scheduled Jobs Setup

Add these to your job scheduler (e.g., Bull, Agenda):

```typescript
import { Queue } from 'bull';
import { processComplianceDeadlines, processRecurringChecklists } from './jobs/processors/complianceProcessor';

const complianceQueue = new Queue('compliance', {
  redis: { host: 'localhost', port: 6379 }
});

// Check compliance deadlines daily at 9 AM
complianceQueue.add('check-deadlines', {}, {
  repeat: { cron: '0 9 * * *' }
});

// Create recurring checklists on 1st of each month
complianceQueue.add('create-recurring', {}, {
  repeat: { cron: '0 0 1 * *' }
});

// Process jobs
complianceQueue.process('check-deadlines', processComplianceDeadlines);
complianceQueue.process('create-recurring', processRecurringChecklists);
```

## Monitoring

### Check System Health

```bash
curl http://localhost:3000/health
```

### View Recent Reports

```bash
curl -X GET "http://localhost:3000/api/v1/regulatory/reports?fiscalYear=2024" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### Check Overdue Compliance Items

```bash
curl -X GET http://localhost:3000/api/v1/regulatory/compliance/checklist/overdue \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

## Troubleshooting

### Issue: "Permission denied"
**Solution**: Ensure user has required permissions (`regulatory:create`, `compliance:read`, etc.)

### Issue: "Report generation failed"
**Solution**: Check that you have sufficient data in the database for the reporting period

### Issue: "Database connection error"
**Solution**: Verify database connection string in `.env` file

### Issue: "No alerts appearing"
**Solution**: Ensure scheduled jobs are running and check job logs

## Next Steps

1. **Import Postman Collection**: Use `docs/postman/Regulatory_Reporting.postman_collection.json`
2. **Read Full Documentation**: See `docs/REGULATORY_REPORTING.md`
3. **Run Tests**: `npm test -- RegulatoryReportingService`
4. **Configure Alerts**: Set up email/SMS notifications for critical alerts
5. **Customize Reports**: Modify report templates to match your institution's requirements

## Support

- **Documentation**: `docs/REGULATORY_REPORTING.md`
- **API Docs**: http://localhost:3000/api/docs
- **Test Collection**: `docs/postman/Regulatory_Reporting.postman_collection.json`

## Key Endpoints Reference

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/regulatory/reports/cbn-prudential` | POST | Generate CBN Prudential Return |
| `/regulatory/reports/cbn-capital-adequacy` | POST | Generate Capital Adequacy Report |
| `/regulatory/reports/firs-vat` | POST | Generate VAT Return |
| `/regulatory/reports/ifrs9-ecl` | POST | Generate IFRS 9 ECL Report |
| `/regulatory/compliance/dashboard` | GET | View compliance dashboard |
| `/regulatory/compliance/checklist` | GET/POST | Manage compliance checklist |
| `/regulatory/alerts` | GET | View regulatory alerts |

Happy reporting! 🎉
