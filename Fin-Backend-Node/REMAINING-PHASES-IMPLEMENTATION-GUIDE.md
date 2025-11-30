# Remaining Phases Implementation Guide

## Overview
This document provides implementation guidance for all remaining phases (2-5, 7-9, 16-25) of the enterprise backend infrastructure.

---

## PRIORITY 1: Foundation Phases (2-5)

### Phase 2: Database Setup - Remaining Tasks

#### Task 2.2: Complete Core Database Schema
The Prisma schema needs to include all entities. Here's the complete schema structure:

```prisma
// prisma/schema.prisma

// Users and Authentication
model User {
  id            String    @id @default(uuid())
  email         String    @unique
  passwordHash  String
  firstName     String
  lastName      String
  status        String    @default("ACTIVE")
  mfaEnabled    Boolean   @default(false)
  mfaSecret     String?
  roles         Role[]
  sessions      Session[]
  createdAt     DateTime  @default(now())
  updatedAt     DateTime  @updatedAt
}

model Role {
  id          String       @id @default(uuid())
  name        String       @unique
  description String?
  permissions Permission[]
  users       User[]
}

model Permission {
  id          String   @id @default(uuid())
  name        String   @unique
  description String?
  resource    String
  action      String
  roles       Role[]
}

model Session {
  id           String   @id @default(uuid())
  userId       String
  user         User     @relation(fields: [userId], references: [id])
  refreshToken String   @unique
  expiresAt    DateTime
  createdAt    DateTime @default(now())
}

// Members and Accounts
model Member {
  id          String    @id @default(uuid())
  memberNumber String   @unique
  firstName   String
  lastName    String
  email       String?
  phone       String
  dateOfBirth DateTime?
  address     String?
  city        String?
  state       String?
  country     String    @default("Nigeria")
  status      String    @default("ACTIVE")
  joinDate    DateTime  @default(now())
  branchId    String?
  branch      Branch?   @relation(fields: [branchId], references: [id])
  accounts    Account[]
  loans       Loan[]
  guarantors  Guarantor[]
  createdAt   DateTime  @default(now())
  updatedAt   DateTime  @updatedAt
}

model Branch {
  id          String    @id @default(uuid())
  name        String
  code        String    @unique
  address     String?
  city        String?
  state       String?
  status      String    @default("ACTIVE")
  members     Member[]
  accounts    Account[]
  createdAt   DateTime  @default(now())
  updatedAt   DateTime  @updatedAt
}

model Account {
  id             String        @id @default(uuid())
  accountNumber  String        @unique
  memberId       String
  member         Member        @relation(fields: [memberId], references: [id])
  type           String        // SAVINGS, SHARES, CASH
  balance        Decimal       @default(0)
  status         String        @default("ACTIVE")
  branchId       String?
  branch         Branch?       @relation(fields: [branchId], references: [id])
  transactions   Transaction[]
  createdAt      DateTime      @default(now())
  updatedAt      DateTime      @updatedAt
}

model Transaction {
  id                   String   @id @default(uuid())
  accountId            String
  account              Account  @relation(fields: [accountId], references: [id])
  type                 String   // DEBIT, CREDIT
  amount               Decimal
  description          String?
  reference            String   @unique
  status               String   @default("PENDING")
  reconciliationStatus String   @default("UNMATCHED")
  reconciledAt         DateTime?
  reconciledBy         String?
  metadata             Json?
  createdBy            String
  createdAt            DateTime @default(now())
  updatedAt            DateTime @updatedAt
}

// Loans
model LoanProduct {
  id                String   @id @default(uuid())
  name              String
  description       String?
  interestRate      Decimal
  minAmount         Decimal
  maxAmount         Decimal
  minTermMonths     Int
  maxTermMonths     Int
  calculationMethod String   // reducing_balance, flat_rate
  penaltyRate       Decimal  @default(0.01)
  isActive          Boolean  @default(true)
  loans             Loan[]
  createdAt         DateTime @default(now())
  updatedAt         DateTime @updatedAt
}

model Loan {
  id                 String          @id @default(uuid())
  memberId           String
  member             Member          @relation(fields: [memberId], references: [id])
  loanProductId      String
  loanProduct        LoanProduct     @relation(fields: [loanProductId], references: [id])
  requestedAmount    Decimal
  approvedAmount     Decimal         @default(0)
  disbursedAmount    Decimal         @default(0)
  outstandingBalance Decimal         @default(0)
  interestRate       Decimal
  termMonths         Int
  purpose            String
  collateralDescription String?
  status             String          @default("PENDING")
  applicationDate    DateTime        @default(now())
  approvalDate       DateTime?
  disbursementDate   DateTime?
  closedDate         DateTime?
  schedules          LoanSchedule[]
  payments           LoanPayment[]
  guarantors         Guarantor[]
  metadata           Json?
  createdBy          String
  createdAt          DateTime        @default(now())
  updatedAt          DateTime        @updatedAt
}

model LoanSchedule {
  id            String   @id @default(uuid())
  loanId        String
  loan          Loan     @relation(fields: [loanId], references: [id])
  paymentNumber Int
  dueDate       DateTime
  principal     Decimal
  interest      Decimal
  totalPayment  Decimal
  balance       Decimal
  paidAmount    Decimal  @default(0)
  isPaid        Boolean  @default(false)
  paidDate      DateTime?
  createdAt     DateTime @default(now())
  updatedAt     DateTime @updatedAt
}

model LoanPayment {
  id            String   @id @default(uuid())
  loanId        String
  loan          Loan     @relation(fields: [loanId], references: [id])
  amount        Decimal
  paymentDate   DateTime
  paymentMethod String
  reference     String   @unique
  notes         String?
  metadata      Json?
  createdBy     String
  createdAt     DateTime @default(now())
}

model Guarantor {
  id               String   @id @default(uuid())
  loanId           String
  loan             Loan     @relation(fields: [loanId], references: [id])
  memberId         String
  member           Member   @relation(fields: [memberId], references: [id])
  guaranteedAmount Decimal
  status           String   @default("PENDING")
  approvedAt       DateTime?
  approvedBy       String?
  createdAt        DateTime @default(now())
  updatedAt        DateTime @updatedAt
}

// Budgets
model Budget {
  id          String        @id @default(uuid())
  name        String
  description String?
  startDate   DateTime
  endDate     DateTime
  fiscalYear  Int
  totalAmount Decimal       @default(0)
  status      String        @default("DRAFT")
  branchId    String?
  items       BudgetItem[]
  actuals     BudgetActual[]
  metadata    Json?
  createdBy   String
  createdAt   DateTime      @default(now())
  updatedAt   DateTime      @updatedAt
}

model BudgetItem {
  id          String         @id @default(uuid())
  budgetId    String
  budget      Budget         @relation(fields: [budgetId], references: [id])
  name        String
  category    String
  amount      Decimal
  description String?
  actuals     BudgetActual[]
  createdAt   DateTime       @default(now())
  updatedAt   DateTime       @updatedAt
}

model BudgetActual {
  id           String     @id @default(uuid())
  budgetId     String
  budget       Budget     @relation(fields: [budgetId], references: [id])
  budgetItemId String
  budgetItem   BudgetItem @relation(fields: [budgetItemId], references: [id])
  amount       Decimal
  date         DateTime
  category     String
  description  String?
  reference    String?
  metadata     Json?
  createdBy    String
  createdAt    DateTime   @default(now())
}

// Documents
model Document {
  id           String            @id @default(uuid())
  name         String
  description  String?
  category     String
  entityType   String
  entityId     String
  filename     String
  originalName String
  mimeType     String
  size         Int
  url          String
  tags         String[]
  currentVersion Int            @default(1)
  isDeleted    Boolean          @default(false)
  deletedAt    DateTime?
  deletedBy    String?
  versions     DocumentVersion[]
  metadata     Json?
  uploadedBy   String
  createdAt    DateTime         @default(now())
  updatedAt    DateTime         @updatedAt
}

model DocumentVersion {
  id                String   @id @default(uuid())
  documentId        String
  document          Document @relation(fields: [documentId], references: [id])
  versionNumber     Int
  filename          String
  originalName      String
  mimeType          String
  size              Int
  url               String
  changeDescription String?
  uploadedBy        String
  createdAt         DateTime @default(now())
}

// Bank Integration
model BankConnection {
  id           String            @id @default(uuid())
  bankName     String
  accountNumber String
  accountName  String
  bankCode     String?
  branchId     String?
  credentials  Json?
  status       String            @default("INACTIVE")
  lastTestedAt DateTime?
  transactions BankTransaction[]
  metadata     Json?
  createdBy    String
  createdAt    DateTime          @default(now())
  updatedAt    DateTime          @updatedAt
}

model BankTransaction {
  id                   String         @id @default(uuid())
  bankConnectionId     String
  bankConnection       BankConnection @relation(fields: [bankConnectionId], references: [id])
  transactionDate      DateTime
  description          String
  reference            String
  debit                Decimal        @default(0)
  credit               Decimal        @default(0)
  balance              Decimal
  status               String         @default("UNMATCHED")
  matchedTransactionId String?
  matchedAt            DateTime?
  matchedBy            String?
  createdAt            DateTime       @default(now())
}

// Approvals
model ApprovalRequest {
  id                String     @id @default(uuid())
  transactionId     String?
  requestedBy       String
  status            String     @default("PENDING")
  approvalLevel     Int
  requiredApprovers Int
  approverRoles     String[]
  reason            String?
  completedAt       DateTime?
  approvals         Approval[]
  metadata          Json?
  createdAt         DateTime   @default(now())
  updatedAt         DateTime   @updatedAt
}

model Approval {
  id                String          @id @default(uuid())
  approvalRequestId String
  approvalRequest   ApprovalRequest @relation(fields: [approvalRequestId], references: [id])
  approvedBy        String
  approver          User            @relation(fields: [approvedBy], references: [id])
  decision          String          // APPROVED, REJECTED
  comment           String?
  approvedAt        DateTime        @default(now())
}

// Audit Logs
model AuditLog {
  id         String   @id @default(uuid())
  userId     String
  action     String
  entityType String
  entityId   String
  changes    Json?
  ipAddress  String
  userAgent  String
  createdAt  DateTime @default(now())
}

// Add indexes for performance
@@index([email], map: "idx_user_email")
@@index([memberId], map: "idx_account_member")
@@index([accountId], map: "idx_transaction_account")
@@index([memberId], map: "idx_loan_member")
@@index([loanId], map: "idx_schedule_loan")
@@index([entityType, entityId], map: "idx_document_entity")
@@index([userId, createdAt], map: "idx_audit_user_date")
```

#### Task 2.3: Create Seed Data
Create `prisma/seed.ts`:

```typescript
import { PrismaClient } from '@prisma/client';
import * as bcrypt from 'bcrypt';

const prisma = new PrismaClient();

async function main() {
  // Create roles
  const adminRole = await prisma.role.create({
    data: {
      name: 'ADMIN',
      description: 'System administrator',
    },
  });

  const managerRole = await prisma.role.create({
    data: {
      name: 'MANAGER',
      description: 'Branch manager',
    },
  });

  // Create admin user
  const hashedPassword = await bcrypt.hash('Admin@123', 10);
  await prisma.user.create({
    data: {
      email: 'admin@example.com',
      passwordHash: hashedPassword,
      firstName: 'System',
      lastName: 'Administrator',
      roles: {
        connect: [{ id: adminRole.id }],
      },
    },
  });

  // Create sample branch
  const branch = await prisma.branch.create({
    data: {
      name: 'Main Branch',
      code: 'MB001',
      city: 'Lagos',
      state: 'Lagos',
    },
  });

  // Create sample members
  for (let i = 1; i <= 10; i++) {
    await prisma.member.create({
      data: {
        memberNumber: `MEM${String(i).padStart(5, '0')}`,
        firstName: `Member${i}`,
        lastName: `Test`,
        phone: `080${String(i).padStart(8, '0')}`,
        branchId: branch.id,
      },
    });
  }

  console.log('Seed data created successfully');
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
```

#### Task 2.4: Repository Pattern
Create `src/repositories/BaseRepository.ts`:

```typescript
import { PrismaClient } from '@prisma/client';

export abstract class BaseRepository<T> {
  constructor(protected prisma: PrismaClient) {}

  abstract findById(id: string): Promise<T | null>;
  abstract findAll(params?: any): Promise<T[]>;
  abstract create(data: any): Promise<T>;
  abstract update(id: string, data: any): Promise<T>;
  abstract delete(id: string): Promise<void>;
}
```

---

### Phase 3: Authentication - Remaining Tasks

#### Task 3.3: RBAC Middleware
Create `src/middleware/rbac.ts`:

```typescript
import { Request, Response, NextFunction } from 'express';

export const requirePermission = (resource: string, action: string) => {
  return async (req: Request, res: Response, next: NextFunction) => {
    const user = req.user;
    
    if (!user) {
      return res.status(401).json({ message: 'Not authenticated' });
    }

    // Check if user has required permission
    const hasPermission = user.roles.some(role =>
      role.permissions.some(p => 
        p.resource === resource && p.action === action
      )
    );

    if (!hasPermission) {
      return res.status(403).json({ message: 'Insufficient permissions' });
    }

    next();
  };
};
```

---

### Phase 4: API Gateway - Remaining Tasks

#### Task 4.2: Rate Limiting
Create `src/middleware/rateLimiter.ts`:

```typescript
import rateLimit from 'express-rate-limit';
import RedisStore from 'rate-limit-redis';
import Redis from 'ioredis';

const redis = new Redis(process.env.REDIS_URL);

export const apiLimiter = rateLimit({
  store: new RedisStore({
    client: redis,
    prefix: 'rl:',
  }),
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 100, // limit each IP to 100 requests per windowMs
  message: 'Too many requests, please try again later',
});
```

---

### Phase 5: Caching - Remaining Tasks

#### Task 5.1: Redis Connection
Create `src/config/redis.ts`:

```typescript
import Redis from 'ioredis';
import { logger } from '@utils/logger';

const redis = new Redis({
  host: process.env.REDIS_HOST || 'localhost',
  port: parseInt(process.env.REDIS_PORT || '6379'),
  password: process.env.REDIS_PASSWORD,
  retryStrategy: (times) => {
    const delay = Math.min(times * 50, 2000);
    return delay;
  },
});

redis.on('connect', () => {
  logger.info('Redis connected');
});

redis.on('error', (err) => {
  logger.error('Redis error:', err);
});

export default redis;
```

---

## PRIORITY 2: Business Logic Phases (7-9)

### Phase 7: Workflow Automation Engine
**Key Components**:
- Workflow definition schema
- State machine for workflow execution
- Approval routing logic
- Notification dispatcher
- Scheduled task executor (cron jobs)

### Phase 8: Background Job Processing
**Key Components**:
- BullMQ setup with Redis
- Job processors for reports, imports, emails
- Retry logic with exponential backoff
- Job monitoring API

### Phase 9: Member and Account Management APIs
**Key Components**:
- Member CRUD endpoints
- Account opening/closing workflows
- KYC verification workflow
- Member search and listing

---

## PRIORITY 3: External Integrations (16-17)

### Phase 16: Payment Gateway Integration
**Supported Gateways**:
- Paystack
- Flutterwave

**Key Components**:
- Payment initialization
- Webhook handling
- Payment verification
- Payment tracking

### Phase 17: Notification Service
**Channels**:
- Email (SMTP/SendGrid/AWS SES)
- SMS
- Push notifications (Firebase)

**Key Components**:
- Template engine
- Notification queue
- Delivery tracking
- Preference management

---

## PRIORITY 4: Production Readiness (18-21)

### Phase 18: Audit Logging
- Already partially implemented
- Need to add audit query APIs
- Data retention policies

### Phase 19: Security Hardening
- Input validation (Zod - already done)
- Security headers (Helmet.js)
- CSRF protection
- XSS prevention
- SQL injection prevention (Prisma handles this)

### Phase 20: Performance Optimization
- Database indexes (add to schema)
- Query optimization
- Response caching
- Connection pooling

### Phase 21: Monitoring
- Prometheus metrics
- OpenTelemetry tracing
- Winston logging (already done)
- Health check endpoints

---

## PRIORITY 5: DevOps (22-24)

### Phase 22: CI/CD
GitHub Actions workflow example:

```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - uses: actions/setup-node@v2
        with:
          node-version: '18'
      - run: npm ci
      - run: npm run test
      - run: npm run build
```

### Phase 23: Kubernetes
Basic deployment manifest:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: fin-backend
spec:
  replicas: 3
  selector:
    matchLabels:
      app: fin-backend
  template:
    metadata:
      labels:
        app: fin-backend
    spec:
      containers:
      - name: fin-backend
        image: fin-backend:latest
        ports:
        - containerPort: 3000
        env:
        - name: DATABASE_URL
          valueFrom:
            secretKeyRef:
              name: db-secret
              key: url
```

### Phase 24: Disaster Recovery
- Automated database backups
- Point-in-time recovery
- Geo-replication
- Regular DR drills

---

## PRIORITY 6: Documentation (25)

### Required Documentation
1. **API Documentation**: Complete Swagger/OpenAPI specs
2. **Deployment Guide**: Step-by-step deployment instructions
3. **Developer Guide**: Setup, architecture, coding standards
4. **Admin Guide**: System administration, monitoring, troubleshooting

---

## Implementation Timeline Estimate

### Week 1-2: Foundation (Phases 2-5)
- Complete database schema
- Finish RBAC
- Add rate limiting
- Complete caching

### Week 3-4: Business Logic (Phases 7-9)
- Workflow engine
- Background jobs
- Member/Account APIs

### Week 5: Integrations (Phases 16-17)
- Payment gateways
- Notifications

### Week 6-7: Production Readiness (Phases 18-21)
- Security hardening
- Performance optimization
- Monitoring

### Week 8: DevOps (Phases 22-24)
- CI/CD pipeline
- Kubernetes deployment
- DR setup

### Week 9: Documentation (Phase 25)
- Complete all documentation

---

## Quick Start Commands

```bash
# Database setup
npx prisma generate
npx prisma migrate dev
npx prisma db seed

# Run development server
npm run dev

# Run tests
npm run test

# Build for production
npm run build

# Start production server
npm start
```

---

**This guide provides the roadmap for completing all remaining phases. Each phase should be implemented incrementally with proper testing and documentation.**
