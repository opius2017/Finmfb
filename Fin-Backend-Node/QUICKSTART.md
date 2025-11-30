# Quick Start Guide - FinMFB Backend

Get the FinMFB backend up and running in minutes!

## Prerequisites

- ✅ Node.js v20+ installed
- ✅ SQL Server installed and running
- ✅ npm v10+ installed

## 🚀 Quick Setup (5 minutes)

### Step 1: Install Dependencies

```bash
cd Fin-Backend-Node
npm install
```

### Step 2: Setup Database

**Option A: Automated (Recommended)**

```powershell
# Run the setup script
.\scripts\setup-database.ps1
```

Enter your SQL Server SA password when prompted.

**Option B: Manual**

1. Create database in SQL Server:
   ```sql
   CREATE DATABASE FinMFBDb;
   ```

2. Copy and configure environment:
   ```bash
   cp .env.example .env
   ```

3. Update `DATABASE_URL` in `.env`:
   ```
   DATABASE_URL="sqlserver://localhost:1433;database=FinMFBDb;user=sa;password=YourPassword;trustServerCertificate=true;encrypt=true"
   ```

4. Run migrations and seed:
   ```bash
   npx prisma generate
   npx prisma db push
   npm run db:seed
   ```

### Step 3: Start the Server

```bash
npm run dev
```

The server will start at `http://localhost:3000`

## 🎯 Test the API

### 1. Check Health

```bash
curl http://localhost:3000/health
```

### 2. Login

```bash
curl -X POST http://localhost:3000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@finmfb.ng",
    "password": "Password123!"
  }'
```

### 3. View API Documentation

Open your browser: `http://localhost:3000/api-docs`

## 🔑 Default Login Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@finmfb.ng | Password123! |
| Manager | manager@finmfb.ng | Password123! |
| Teller | teller@finmfb.ng | Password123! |
| Loan Officer | loanofficer@finmfb.ng | Password123! |
| Accountant | accountant@finmfb.ng | Password123! |

## 📊 Explore the Data

### Using Prisma Studio

```bash
npm run db:studio
```

Opens at `http://localhost:5555` - Browse all your data visually!

### Sample Data Included

- ✅ 5 Branches (Lagos, Abuja, Port Harcourt, Kano, Ibadan)
- ✅ 20 Members with Nigerian names
- ✅ 30+ Accounts (Savings & Shares)
- ✅ 50+ Transactions
- ✅ 5 Loan Products
- ✅ 15 Loans
- ✅ 2024 Budget with 10 categories
- ✅ 3 Bank connections

## 🛠️ Common Commands

```bash
# Development
npm run dev              # Start dev server with hot reload
npm run build            # Build for production
npm start                # Start production server

# Database
npm run db:seed          # Seed database
npm run db:studio        # Open Prisma Studio
npx prisma db push       # Push schema changes
npx prisma generate      # Generate Prisma client

# Testing
npm test                 # Run tests
npm run test:watch       # Run tests in watch mode
npm run test:coverage    # Run tests with coverage

# Code Quality
npm run lint             # Lint code
npm run lint:fix         # Fix linting issues
npm run format           # Format code
```

## 🔧 Configuration

### Environment Variables

Key variables in `.env`:

```env
# Server
PORT=3000
NODE_ENV=development
API_VERSION=v1

# Database
DATABASE_URL="sqlserver://localhost:1433;database=FinMFBDb;..."

# JWT
JWT_SECRET=your-super-secret-jwt-key-change-this-in-production
JWT_EXPIRES_IN=15m
JWT_REFRESH_EXPIRES_IN=7d

# Redis (for caching & jobs)
REDIS_HOST=localhost
REDIS_PORT=6379
REDIS_PASSWORD=

# CORS
CORS_ORIGIN=http://localhost:3001,http://localhost:3000
```

## 📁 Project Structure

```
Fin-Backend-Node/
├── prisma/
│   ├── schema.prisma      # Database schema
│   └── seed.ts            # Seed data
├── src/
│   ├── config/            # Configuration files
│   ├── controllers/       # Route controllers
│   ├── middleware/        # Express middleware
│   ├── routes/            # API routes
│   ├── services/          # Business logic
│   ├── utils/             # Utility functions
│   └── server.ts          # Entry point
├── scripts/
│   └── setup-database.ps1 # Database setup script
├── .env                   # Environment variables
└── package.json           # Dependencies
```

## 🐛 Troubleshooting

### SQL Server Connection Issues

**Problem**: Can't connect to SQL Server

**Solutions**:
1. Check SQL Server is running:
   ```powershell
   Get-Service MSSQLSERVER
   ```

2. Enable SQL Server Authentication:
   - Open SSMS → Server Properties → Security
   - Select "SQL Server and Windows Authentication mode"
   - Restart SQL Server

3. Enable TCP/IP:
   - SQL Server Configuration Manager
   - Protocols for MSSQLSERVER → TCP/IP → Enable
   - Restart SQL Server

### Port Already in Use

**Problem**: Port 3000 is already in use

**Solution**: Change port in `.env`:
```env
PORT=3001
```

### Prisma Client Not Generated

**Problem**: `@prisma/client` not found

**Solution**:
```bash
npx prisma generate
```

## 📚 Next Steps

1. **Explore the API**: Check out `/api-docs` for full API documentation
2. **Read the Docs**: See `DATABASE_SETUP.md` for detailed database info
3. **Customize**: Modify the seed data in `prisma/seed.ts`
4. **Develop**: Start building your features!

## 🆘 Need Help?

- 📖 Check `DATABASE_SETUP.md` for database details
- 📖 Check `README.md` for full documentation
- 🐛 Check existing issues or create a new one
- 💬 Contact the development team

## 🎉 You're Ready!

Your FinMFB backend is now running with:
- ✅ SQL Server database
- ✅ Nigerian sample data
- ✅ RESTful API
- ✅ Authentication & Authorization
- ✅ Swagger documentation

Happy coding! 🚀
