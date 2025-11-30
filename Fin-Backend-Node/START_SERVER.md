# Start Server Guide

## Quick Start

### Option 1: Development Mode (Recommended)
```bash
npm run dev
```

### Option 2: Production Build
```bash
npm run build
npm start
```

## What Happens When You Start

1. **Server starts** on port 3000
2. **Database connection** is established to SoarMFBDb
3. **API endpoints** become available
4. **Swagger documentation** is accessible

## Access Points

- **API Base URL:** http://localhost:3000
- **API Documentation:** http://localhost:3000/api/docs
- **Health Check:** http://localhost:3000/health
- **Readiness Check:** http://localhost:3000/ready

## Test the Server

### 1. Health Check
```bash
curl http://localhost:3000/health
```

Expected response:
```json
{
  "status": "healthy",
  "timestamp": "2024-11-30T...",
  "uptime": 123.456,
  "environment": "development"
}
```

### 2. Login
```bash
curl -X POST http://localhost:3000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"admin@soarmfb.ng\",\"password\":\"Password123!\"}"
```

Expected response:
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "...",
    "user": {
      "id": "...",
      "email": "admin@soarmfb.ng",
      "firstName": "Adebayo",
      "lastName": "Ogunlesi"
    }
  }
}
```

### 3. Get Members (with token)
```bash
curl http://localhost:3000/api/v1/members \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

## Available Scripts

```bash
# Development with hot reload
npm run dev

# Build TypeScript
npm run build

# Start production server
npm start

# Run tests
npm test

# Run tests with coverage
npm run test:coverage

# Lint code
npm run lint

# Format code
npm run format
```

## Troubleshooting

### Port 3000 already in use
Change the PORT in `.env` file:
```env
PORT=3001
```

### Database connection error
1. Verify SQL Server is running
2. Check connection string in `.env`
3. Test connection: `sqlcmd -S localhost -E -Q "SELECT @@VERSION"`

### Module not found errors
```bash
npm install
```

### TypeScript errors
```bash
npm run build
```

## Next Steps

1. ✅ Server is running
2. ✅ Database is connected
3. ✅ API is accessible
4. 📝 Test the endpoints
5. 📝 Explore the API documentation
6. 📝 Build your frontend

## API Endpoints Summary

### Authentication
- POST `/api/v1/auth/login` - Login
- POST `/api/v1/auth/register` - Register
- POST `/api/v1/auth/refresh` - Refresh token
- POST `/api/v1/auth/logout` - Logout

### Members
- GET `/api/v1/members` - List members
- POST `/api/v1/members` - Create member
- GET `/api/v1/members/:id` - Get member
- PUT `/api/v1/members/:id` - Update member

### Accounts
- GET `/api/v1/accounts` - List accounts
- POST `/api/v1/accounts` - Create account
- GET `/api/v1/accounts/:id` - Get account
- GET `/api/v1/accounts/:id/transactions` - Get transactions

### Loans
- GET `/api/v1/loans` - List loans
- POST `/api/v1/loans` - Create loan application
- GET `/api/v1/loans/:id` - Get loan
- POST `/api/v1/loans/:id/approve` - Approve loan
- POST `/api/v1/loans/:id/disburse` - Disburse loan

### Regulatory
- POST `/api/v1/regulatory/reports/cbn-prudential` - Generate CBN report
- POST `/api/v1/regulatory/reports/firs-vat` - Generate VAT return
- POST `/api/v1/regulatory/reports/ifrs9-ecl` - Generate ECL report
- GET `/api/v1/regulatory/compliance/dashboard` - Compliance dashboard

For complete API documentation, visit: http://localhost:3000/api/docs
