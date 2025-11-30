# Design Document

## Overview

The Cooperative Loan Management System is a comprehensive enterprise-grade solution for managing the complete loan lifecycle in Nigerian cooperative societies. The system implements best-practice governance, automated risk checks, and seamless integration with payroll systems.

## Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Presentation Layer                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │   React SPA  │  │  Mobile PWA  │  │  Admin Portal│     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                      API Gateway Layer                       │
│  ┌──────────────────────────────────────────────────────┐  │
│  │   ASP.NET Core Web API (REST + SignalR)             │  │
│  │   - Authentication/Authorization (JWT)               │  │
│  │   - Rate Limiting & Throttling                       │  │
│  │   - Request/Response Logging                         │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │ CQRS/MediatR│  │  Workflow    │  │  Calculation │     │
│  │   Handlers   │  │   Engine     │  │    Engine    │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                            │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │   Entities   │  │  Aggregates  │  │   Services   │     │
│  │  Value Objs  │  │  Domain Evts │  │  Interfaces  │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │  EF Core     │  │  External    │  │   Caching    │     │
│  │  Repository  │  │  Integrations│  │   (Redis)    │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                      Data Layer                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │  SQL Server  │  │  Blob Storage│  │  Redis Cache │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
└─────────────────────────────────────────────────────────────┘
```

### Technology Stack

**Backend:**
- ASP.NET Core 8.0 Web API
- Entity Framework Core 8.0
- MediatR (CQRS pattern)
- FluentValidation
- AutoMapper
- Hangfire (Background Jobs)
- SignalR (Real-time updates)

**Frontend:**
- React 18 with TypeScript
- Tailwind CSS
- React Query (Data fetching)
- Zustand (State management)
- React Hook Form (Forms)
- Recharts (Visualizations)

**Database:**
- SQL Server 2022
- Redis (Caching)
- Azure Blob Storage (Documents)

**Integration:**
- EPPlus (Excel processing)
- Twilio/Termii (SMS)
- SendGrid (Email)
- NIBSS/Interswitch (Bank transfers)

## Components and Interfaces

### 1. Loan Application Module

**Components:**
- `LoanApplicationController`: REST API endpoints
- `LoanApplicationService`: Business logic
- `LoanEligibilityChecker`: Eligibility validation
- `LoanCalculator`: EMI and amortization calculations
- `GuarantorValidator`: Guarantor verification

**Key Interfaces:**
```csharp
public interface ILoanApplicationService
{
    Task<LoanApplicationDto> CreateApplicationAsync(CreateLoanApplicationCommand command);
    Task<LoanEligibilityDto> CheckEligibilityAsync(CheckEligibilityQuery query);
    Task<LoanApplicationDto> SubmitApplicationAsync(Guid applicationId);
    Task<List<LoanApplicationDto>> GetApplicationsAsync(LoanApplicationFilter filter);
}

public interface ILoanCalculator
{
    decimal CalculateEMI(decimal principal, decimal annualRate, int tenorMonths);
    List<AmortizationEntry> GenerateAmortizationSchedule(LoanCalculationInput input);
    decimal CalculateTotalInterest(decimal emi, int tenor, decimal principal);
}

public interface IGuarantorService
{
    Task<bool> ValidateGuarantorEligibilityAsync(Guid memberId, decimal amount);
    Task<GuarantorConsentDto> RequestConsentAsync(GuarantorConsentRequest request);
    Task LockGuarantorEquityAsync(Guid guarantorId, decimal amount);
    Task ReleaseGuarantorEquityAsync(Guid guarantorId, decimal amount);
}
```

### 2. Loan Committee Workflow Module

**Components:**
- `LoanCommitteeController`: Committee operations
- `WorkflowEngine`: State machine for loan lifecycle
- `CommitteeReviewService`: Review and approval logic
- `RiskAssessmentService`: Credit scoring

**Key Interfaces:**
```csharp
public interface IWorkflowEngine
{
    Task<WorkflowState> TransitionAsync(Guid loanId, WorkflowAction action);
    Task<List<WorkflowAction>> GetAvailableActionsAsync(Guid loanId);
    Task<WorkflowHistory> GetHistoryAsync(Guid loanId);
}

public interface ICommitteeReviewService
{
    Task<CommitteeReviewDto> SubmitReviewAsync(SubmitReviewCommand command);
    Task<MemberCreditProfileDto> GetCreditProfileAsync(Guid memberId);
    Task<RepaymentScoreDto> CalculateRepaymentScoreAsync(Guid memberId);
}
```

### 3. Loan Register & Threshold Module

**Components:**
- `LoanRegisterService`: Registration and serial numbers
- `ThresholdManager`: Monthly threshold tracking
- `QueueManager`: Application queue management

**Key Interfaces:**
```csharp
public interface ILoanRegisterService
{
    Task<LoanDto> RegisterLoanAsync(Guid applicationId);
    Task<string> GenerateSerialNumberAsync(int year, int month);
    Task<LoanRegisterDto> GetMonthlyRegisterAsync(int year, int month);
    Task<bool> CheckThresholdAsync(decimal amount, int year, int month);
}

public interface IThresholdManager
{
    Task<MonthlyThresholdDto> GetThresholdAsync(int year, int month);
    Task UpdateThresholdAsync(int year, int month, decimal maxAmount);
    Task<List<QueuedLoanDto>> GetQueuedLoansAsync(int year, int month);
    Task ProcessMonthlyRolloverAsync();
}
```

### 4. Disbursement Module

**Components:**
- `DisbursementController`: Disbursement operations
- `BankIntegrationService`: Bank transfer integration
- `LoanAgreementGenerator`: Document generation
- `CommodityVoucherService`: Voucher generation

**Key Interfaces:**
```csharp
public interface IDisbursementService
{
    Task<DisbursementDto> DisburseCashLoanAsync(DisburseCashLoanCommand command);
    Task<CommodityVoucherDto> DisburseCommodityLoanAsync(DisburseCommodityLoanCommand command);
    Task<byte[]> GenerateLoanAgreementAsync(Guid loanId);
}

public interface IBankIntegrationService
{
    Task<BankTransferResult> TransferFundsAsync(BankTransferRequest request);
    Task<TransferStatus> GetTransferStatusAsync(string transactionId);
}
```

### 5. Repayment & Reconciliation Module

**Components:**
- `RepaymentController`: Repayment operations
- `RepaymentProcessor`: Payment allocation logic
- `DeductionScheduleService`: Schedule generation
- `ReconciliationEngine`: Variance detection

**Key Interfaces:**
```csharp
public interface IRepaymentService
{
    Task<RepaymentDto> RecordRepaymentAsync(RecordRepaymentCommand command);
    Task<List<RepaymentDto>> GetRepaymentsAsync(Guid loanId);
    Task ProcessPayrollDeductionsAsync(List<ActualDeduction> deductions);
}

public interface IDeductionScheduleService
{
    Task<DeductionScheduleDto> GenerateScheduleAsync(int year, int month);
    Task<byte[]> ExportScheduleToExcelAsync(int year, int month);
    Task ImportActualDeductionsAsync(Stream excelFile, int year, int month);
    Task<ReconciliationReportDto> ReconcileAsync(int year, int month);
}
```

### 6. Delinquency Management Module

**Components:**
- `DelinquencyController`: Delinquency operations
- `DelinquencyDetector`: Overdue loan identification
- `NotificationService`: SMS/Email alerts
- `PenaltyCalculator`: Late fee calculation

**Key Interfaces:**
```csharp
public interface IDelinquencyService
{
    Task<List<DelinquentLoanDto>> GetDelinquentLoansAsync();
    Task ProcessDelinquencyChecksAsync();
    Task SendDelinquencyNotificationsAsync(Guid loanId);
    Task<decimal> CalculatePenaltyAsync(Guid loanId);
}

public interface INotificationService
{
    Task SendSMSAsync(string phoneNumber, string message);
    Task SendEmailAsync(string email, string subject, string body);
    Task SendPushNotificationAsync(Guid userId, string message);
}
```

### 7. Commodity Store Module

**Components:**
- `CommodityStoreController`: Store operations
- `InventoryService`: Stock management
- `FulfillmentService`: Order processing

**Key Interfaces:**
```csharp
public interface ICommodityStoreService
{
    Task<List<CommodityItemDto>> GetAvailableItemsAsync();
    Task<CommodityRequestDto> CreateRequestAsync(CreateCommodityRequestCommand command);
    Task<CommodityVoucherDto> FulfillRequestAsync(Guid requestId);
    Task UpdateInventoryAsync(Guid itemId, int quantity);
}
```

## Data Models

### Core Entities

**LoanApplication**
```csharp
public class LoanApplication : AuditableEntity
{
    public Guid Id { get; set; }
    public string ApplicationNumber { get; set; }
    public Guid MemberId { get; set; }
    public LoanType LoanType { get; set; }
    public decimal RequestedAmount { get; set; }
    public string Purpose { get; set; }
    public int Tenor { get; set; }
    public LoanApplicationStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    
    // Navigation properties
    public Member Member { get; set; }
    public List<Guarantor> Guarantors { get; set; }
    public List<CommitteeReview> Reviews { get; set; }
    public Loan Loan { get; set; }
}
```

**Loan**
```csharp
public class Loan : AuditableEntity
{
    public Guid Id { get; set; }
    public string LoanNumber { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid MemberId { get; set; }
    public LoanType LoanType { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int Tenor { get; set; }
    public decimal MonthlyEMI { get; set; }
    public decimal OutstandingBalance { get; set; }
    public LoanStatus Status { get; set; }
    public DateTime DisbursementDate { get; set; }
    public DateTime MaturityDate { get; set; }
    
    // Navigation properties
    public LoanApplication Application { get; set; }
    public Member Member { get; set; }
    public List<AmortizationSchedule> AmortizationSchedule { get; set; }
    public List<Repayment> Repayments { get; set; }
}
```

**AmortizationSchedule**
```csharp
public class AmortizationSchedule
{
    public Guid Id { get; set; }
    public Guid LoanId { get; set; }
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal PrincipalDue { get; set; }
    public decimal InterestDue { get; set; }
    public decimal TotalDue { get; set; }
    public decimal PrincipalPaid { get; set; }
    public decimal InterestPaid { get; set; }
    public decimal ClosingBalance { get; set; }
    public InstallmentStatus Status { get; set; }
    public DateTime? PaidDate { get; set; }
    
    public Loan Loan { get; set; }
}
```

### Database Schema

```sql
-- Core Tables
CREATE TABLE Members (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    MemberNumber NVARCHAR(50) NOT NULL UNIQUE,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    PayrollPin NVARCHAR(50),
    TotalSavings DECIMAL(18,2) NOT NULL DEFAULT 0,
    MonthlyContribution DECIMAL(18,2) NOT NULL DEFAULT 0,
    ShareCapital DECIMAL(18,2) NOT NULL DEFAULT 0,
    FreeEquity DECIMAL(18,2) NOT NULL DEFAULT 0,
    LockedEquity DECIMAL(18,2) NOT NULL DEFAULT 0,
    MembershipDate DATE NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE LoanApplications (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ApplicationNumber NVARCHAR(50) NOT NULL UNIQUE,
    MemberId UNIQUEIDENTIFIER NOT NULL,
    LoanType NVARCHAR(20) NOT NULL,
    RequestedAmount DECIMAL(18,2) NOT NULL,
    Purpose NVARCHAR(500) NOT NULL,
    Tenor INT NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    SubmittedAt DATETIME2,
    ApprovedAt DATETIME2,
    RejectedAt DATETIME2,
    RejectionReason NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (MemberId) REFERENCES Members(Id)
);

CREATE TABLE Loans (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    LoanNumber NVARCHAR(50) NOT NULL UNIQUE,
    ApplicationId UNIQUEIDENTIFIER NOT NULL,
    MemberId UNIQUEIDENTIFIER NOT NULL,
    LoanType NVARCHAR(20) NOT NULL,
    PrincipalAmount DECIMAL(18,2) NOT NULL,
    InterestRate DECIMAL(5,2) NOT NULL,
    Tenor INT NOT NULL,
    MonthlyEMI DECIMAL(18,2) NOT NULL,
    TotalInterest DECIMAL(18,2) NOT NULL,
    TotalRepayment DECIMAL(18,2) NOT NULL,
    OutstandingBalance DECIMAL(18,2) NOT NULL,
    PrincipalPaid DECIMAL(18,2) NOT NULL DEFAULT 0,
    InterestPaid DECIMAL(18,2) NOT NULL DEFAULT 0,
    PenaltiesPaid DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status NVARCHAR(20) NOT NULL,
    DisbursementDate DATE NOT NULL,
    FirstPaymentDate DATE NOT NULL,
    MaturityDate DATE NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (ApplicationId) REFERENCES LoanApplications(Id),
    FOREIGN KEY (MemberId) REFERENCES Members(Id)
);

CREATE TABLE AmortizationSchedules (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    LoanId UNIQUEIDENTIFIER NOT NULL,
    InstallmentNumber INT NOT NULL,
    DueDate DATE NOT NULL,
    OpeningBalance DECIMAL(18,2) NOT NULL,
    PrincipalDue DECIMAL(18,2) NOT NULL,
    InterestDue DECIMAL(18,2) NOT NULL,
    TotalDue DECIMAL(18,2) NOT NULL,
    PrincipalPaid DECIMAL(18,2) NOT NULL DEFAULT 0,
    InterestPaid DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalPaid DECIMAL(18,2) NOT NULL DEFAULT 0,
    ClosingBalance DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    PaidDate DATETIME2,
    FOREIGN KEY (LoanId) REFERENCES Loans(Id),
    CONSTRAINT UK_Loan_Installment UNIQUE (LoanId, InstallmentNumber)
);

CREATE TABLE Guarantors (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ApplicationId UNIQUEIDENTIFIER NOT NULL,
    MemberId UNIQUEIDENTIFIER NOT NULL,
    GuaranteedAmount DECIMAL(18,2) NOT NULL,
    ConsentStatus NVARCHAR(20) NOT NULL,
    ConsentDate DATETIME2,
    ConsentToken NVARCHAR(100),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (ApplicationId) REFERENCES LoanApplications(Id),
    FOREIGN KEY (MemberId) REFERENCES Members(Id)
);

CREATE TABLE Repayments (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    LoanId UNIQUEIDENTIFIER NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    PrincipalAmount DECIMAL(18,2) NOT NULL,
    InterestAmount DECIMAL(18,2) NOT NULL,
    PenaltyAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    PaymentDate DATETIME2 NOT NULL,
    PaymentMethod NVARCHAR(20) NOT NULL,
    TransactionReference NVARCHAR(100),
    Notes NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (LoanId) REFERENCES Loans(Id)
);

-- Indexes for performance
CREATE INDEX IX_LoanApplications_MemberId ON LoanApplications(MemberId);
CREATE INDEX IX_LoanApplications_Status ON LoanApplications(Status);
CREATE INDEX IX_Loans_MemberId ON Loans(MemberId);
CREATE INDEX IX_Loans_Status ON Loans(Status);
CREATE INDEX IX_Loans_DisbursementDate ON Loans(DisbursementDate);
CREATE INDEX IX_AmortizationSchedules_LoanId ON AmortizationSchedules(LoanId);
CREATE INDEX IX_AmortizationSchedules_DueDate ON AmortizationSchedules(DueDate);
CREATE INDEX IX_Repayments_LoanId ON Repayments(LoanId);
CREATE INDEX IX_Repayments_PaymentDate ON Repayments(PaymentDate);
```

## Error Handling

### Error Response Format
```json
{
  "success": false,
  "error": {
    "code": "ELIGIBILITY_CHECK_FAILED",
    "message": "Member does not meet eligibility criteria",
    "details": [
      "Insufficient savings balance. Required: ₦100,000, Available: ₦50,000",
      "Membership duration below minimum. Required: 6 months, Current: 3 months"
    ]
  },
  "timestamp": "2025-01-15T10:30:00Z",
  "traceId": "abc123-def456"
}
```

### Exception Hierarchy
- `DomainException`: Base for all domain exceptions
  - `EligibilityException`: Eligibility check failures
  - `ThresholdExceededException`: Monthly threshold breaches
  - `InsufficientEquityException`: Guarantor equity issues
  - `InvalidStateTransitionException`: Workflow violations

## Testing Strategy

### Unit Tests
- Loan calculation engine (EMI, amortization)
- Eligibility checker logic
- Workflow state transitions
- Penalty calculations
- Reconciliation algorithms

### Integration Tests
- API endpoint testing
- Database operations
- External service integrations
- Workflow end-to-end
- Deduction reconciliation

### Performance Tests
- Load testing (1000 concurrent users)
- Database query optimization
- API response times (<200ms)
- Batch processing performance

### Security Tests
- Authentication/Authorization
- SQL injection prevention
- XSS prevention
- CSRF protection
- Data encryption validation
