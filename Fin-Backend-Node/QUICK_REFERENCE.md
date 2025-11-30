# Quick Reference Card

## Database Connection
```
Server: localhost
Database: SoarMFBDb
Auth: Windows Authentication (Trusted Connection)
```

## Test Credentials
**Password for all users:** `Password123!`

| Role | Email | Name |
|------|-------|------|
| Super Admin | admin@soarmfb.ng | Adebayo Ogunlesi |
| Branch Manager | manager.lagos@soarmfb.ng | Chioma Nwosu |
| Loan Officer | loanofficer@soarmfb.ng | Funmilayo Adeyemi |
| Accountant | accountant@soarmfb.ng | Emeka Okafor |
| Compliance | compliance@soarmfb.ng | Aisha Bello |
| Teller | teller1@soarmfb.ng | Blessing Eze |

## Quick Start
```bash
# 1. Setup database (already done!)
cd Fin-Backend-Node\scripts
.\setup-and-seed-database.ps1

# 2. Install dependencies
cd ..
npm install

# 3. Start server
npm run dev

# 4. Access API
# http://localhost:3000
# http://localhost:3000/api/docs
```

## Sample API Calls

### Login
```bash
curl -X POST http://localhost:3000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@soarmfb.ng","password":"Password123!"}'
```

### Get Members
```bash
curl http://localhost:3000/api/v1/members \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Generate CBN Report
```bash
curl -X POST http://localhost:3000/api/v1/regulatory/reports/cbn-prudential \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"periodStart":"2024-01-01","periodEnd":"2024-01-31"}'
```

## Sample Data Summary
- **Users:** 8 (various roles)
- **Branches:** 5 (Lagos, Ikeja, Abuja, PH, Kano)
- **Members:** 20 (Nigerian names/addresses)
- **Accounts:** 25 (Savings & Shares)
- **Transactions:** 25 (last 30 days)
- **Loans:** 5 (3 active, 2 pending)
- **Loan Products:** 5 (Personal, Business, Agri, Education, Emergency)

## Useful SQL Queries

### View All Members
```sql
SELECT member_number, first_name, last_name, email, phone, city 
FROM members 
ORDER BY member_number;
```

### View Account Balances
```sql
SELECT a.account_number, m.first_name + ' ' + m.last_name as name, 
       a.type, a.balance
FROM accounts a
JOIN members m ON a.member_id = m.id
ORDER BY a.balance DESC;
```

### View Active Loans
```sql
SELECT m.member_number, m.first_name + ' ' + m.last_name as name,
       l.disbursed_amount, l.outstanding_balance, l.status
FROM loans l
JOIN members m ON l.member_id = m.id
WHERE l.status = 'ACTIVE';
```

## File Locations
- **Scripts:** `Fin-Backend-Node/scripts/`
- **Docs:** `Fin-Backend-Node/docs/`
- **API:** `Fin-Backend-Node/src/`
- **Tests:** `Fin-Backend-Node/src/**/__tests__/`

## Documentation
- **Setup Guide:** `DATABASE_SETUP_GUIDE.md`
- **Completion Report:** `DATABASE_SETUP_COMPLETE.md`
- **Regulatory Docs:** `docs/REGULATORY_REPORTING.md`
- **Quick Start:** `docs/REGULATORY_QUICK_START.md`
- **API Docs:** http://localhost:3000/api/docs

## Support
- Check logs in `logs/` directory
- Review error messages in console
- Verify `.env` configuration
- Ensure SQL Server is running
