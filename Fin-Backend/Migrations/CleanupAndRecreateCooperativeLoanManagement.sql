-- Migration: Complete cleanup and recreation of Cooperative Loan Management tables
-- Date: 2024-11-30
-- Description: Drops all related tables and recreates them with correct structure

-- Step 1: Drop all foreign key constraints first
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_LoanRegisters_Loans')
    ALTER TABLE LoanRegisters DROP CONSTRAINT FK_LoanRegisters_Loans;
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CommitteeReviews_LoanApplications')
    ALTER TABLE CommitteeReviews DROP CONSTRAINT FK_CommitteeReviews_LoanApplications;
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CommitteeReviews_Members')
    ALTER TABLE CommitteeReviews DROP CONSTRAINT FK_CommitteeReviews_Members;
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_GuarantorConsents_LoanApplications')
    ALTER TABLE GuarantorConsents DROP CONSTRAINT FK_GuarantorConsents_LoanApplications;
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_GuarantorConsents_Members')
    ALTER TABLE GuarantorConsents DROP CONSTRAINT FK_GuarantorConsents_Members;
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Loans_LoanApplications')
    ALTER TABLE Loans DROP CONSTRAINT FK_Loans_LoanApplications;
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Loans_Members')
    ALTER TABLE Loans DROP CONSTRAINT FK_Loans_Members;
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_LoanApplications_Members')
    ALTER TABLE LoanApplications DROP CONSTRAINT FK_LoanApplications_Members;
GO

-- Step 2: Drop all tables in reverse dependency order
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'LoanRegisters')
    DROP TABLE LoanRegisters;
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CommitteeReviews')
    DROP TABLE CommitteeReviews;
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'GuarantorConsents')
    DROP TABLE GuarantorConsents;
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Loans')
    DROP TABLE Loans;
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'LoanApplications')
    DROP TABLE LoanApplications;
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'MonthlyThresholds')
    DROP TABLE MonthlyThresholds;
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Members')
    DROP TABLE Members;
GO

PRINT 'All existing tables dropped successfully';
GO

-- Step 3: Recreate all tables with correct structure

-- Create Members table
CREATE TABLE Members (
    Id NVARCHAR(450) NOT NULL,
    MemberNumber NVARCHAR(50) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    PhoneNumber NVARCHAR(20),
    DateOfBirth DATETIME2,
    Gender NVARCHAR(20),
    Address NVARCHAR(500),
    City NVARCHAR(100),
    State NVARCHAR(100),
    Country NVARCHAR(100),
    PostalCode NVARCHAR(20),
    MembershipDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    MembershipStatus NVARCHAR(50) NOT NULL DEFAULT 'ACTIVE',
    ShareCapital DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalSavings DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalLoans DECIMAL(18,2) NOT NULL DEFAULT 0,
    OutstandingLoanBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreditScore INT,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(450),
    UpdatedBy NVARCHAR(450),
    CONSTRAINT PK_Members PRIMARY KEY (Id),
    CONSTRAINT UQ_Members_MemberNumber UNIQUE (MemberNumber)
);

CREATE INDEX IX_Members_MemberNumber ON Members(MemberNumber);
CREATE INDEX IX_Members_Email ON Members(Email);
CREATE INDEX IX_Members_MembershipStatus ON Members(MembershipStatus);
GO

PRINT 'Members table created';
GO

-- Create LoanApplications table
CREATE TABLE LoanApplications (
    Id NVARCHAR(450) NOT NULL,
    ApplicationNumber NVARCHAR(50) NOT NULL,
    MemberId NVARCHAR(450) NOT NULL,
    LoanProductId NVARCHAR(450),
    RequestedAmount DECIMAL(18,2) NOT NULL,
    ApprovedAmount DECIMAL(18,2),
    LoanPurpose NVARCHAR(500) NOT NULL,
    RepaymentPeriodMonths INT NOT NULL,
    InterestRate DECIMAL(5,2) NOT NULL,
    ApplicationDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ApplicationStatus NVARCHAR(50) NOT NULL DEFAULT 'PENDING',
    ReviewedBy NVARCHAR(450),
    ReviewedAt DATETIME2,
    ReviewNotes NVARCHAR(MAX),
    ApprovedBy NVARCHAR(450),
    ApprovedAt DATETIME2,
    RejectionReason NVARCHAR(MAX),
    RequiredGuarantors INT NOT NULL DEFAULT 2,
    GuarantorsProvided INT NOT NULL DEFAULT 0,
    CommitteeReviewStatus NVARCHAR(50),
    CommitteeReviewDate DATETIME2,
    DisbursementDate DATETIME2,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(450),
    UpdatedBy NVARCHAR(450),
    CONSTRAINT PK_LoanApplications PRIMARY KEY (Id),
    CONSTRAINT UQ_LoanApplications_ApplicationNumber UNIQUE (ApplicationNumber),
    CONSTRAINT FK_LoanApplications_Members FOREIGN KEY (MemberId) REFERENCES Members(Id)
);

CREATE INDEX IX_LoanApplications_ApplicationNumber ON LoanApplications(ApplicationNumber);
CREATE INDEX IX_LoanApplications_MemberId ON LoanApplications(MemberId);
CREATE INDEX IX_LoanApplications_ApplicationStatus ON LoanApplications(ApplicationStatus);
CREATE INDEX IX_LoanApplications_ApplicationDate ON LoanApplications(ApplicationDate);
GO

PRINT 'LoanApplications table created';
GO

-- Create Loans table
CREATE TABLE Loans (
    Id NVARCHAR(450) NOT NULL,
    LoanNumber NVARCHAR(50) NOT NULL,
    LoanApplicationId NVARCHAR(450) NOT NULL,
    MemberId NVARCHAR(450) NOT NULL,
    PrincipalAmount DECIMAL(18,2) NOT NULL,
    InterestRate DECIMAL(5,2) NOT NULL,
    RepaymentPeriodMonths INT NOT NULL,
    MonthlyInstallment DECIMAL(18,2) NOT NULL,
    TotalRepayableAmount DECIMAL(18,2) NOT NULL,
    OutstandingBalance DECIMAL(18,2) NOT NULL,
    PrincipalPaid DECIMAL(18,2) NOT NULL DEFAULT 0,
    InterestPaid DECIMAL(18,2) NOT NULL DEFAULT 0,
    DisbursementDate DATETIME2 NOT NULL,
    MaturityDate DATETIME2 NOT NULL,
    LoanStatus NVARCHAR(50) NOT NULL DEFAULT 'ACTIVE',
    PaymentFrequency NVARCHAR(50) NOT NULL DEFAULT 'MONTHLY',
    NextPaymentDate DATETIME2,
    LastPaymentDate DATETIME2,
    DaysInArrears INT NOT NULL DEFAULT 0,
    ArrearsAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    PenaltyAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Classification NVARCHAR(50) NOT NULL DEFAULT 'PERFORMING',
    ProvisionAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(450),
    UpdatedBy NVARCHAR(450),
    CONSTRAINT PK_Loans PRIMARY KEY (Id),
    CONSTRAINT UQ_Loans_LoanNumber UNIQUE (LoanNumber),
    CONSTRAINT FK_Loans_LoanApplications FOREIGN KEY (LoanApplicationId) REFERENCES LoanApplications(Id),
    CONSTRAINT FK_Loans_Members FOREIGN KEY (MemberId) REFERENCES Members(Id)
);

CREATE INDEX IX_Loans_LoanNumber ON Loans(LoanNumber);
CREATE INDEX IX_Loans_MemberId ON Loans(MemberId);
CREATE INDEX IX_Loans_LoanStatus ON Loans(LoanStatus);
CREATE INDEX IX_Loans_Classification ON Loans(Classification);
CREATE INDEX IX_Loans_DisbursementDate ON Loans(DisbursementDate);
GO

PRINT 'Loans table created';
GO

-- Create GuarantorConsents table
CREATE TABLE GuarantorConsents (
    Id NVARCHAR(450) NOT NULL,
    LoanApplicationId NVARCHAR(450) NOT NULL,
    GuarantorMemberId NVARCHAR(450) NOT NULL,
    GuaranteedAmount DECIMAL(18,2) NOT NULL,
    ConsentStatus NVARCHAR(50) NOT NULL DEFAULT 'PENDING',
    ConsentDate DATETIME2,
    RejectionReason NVARCHAR(MAX),
    GuarantorNotes NVARCHAR(MAX),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(450),
    UpdatedBy NVARCHAR(450),
    CONSTRAINT PK_GuarantorConsents PRIMARY KEY (Id),
    CONSTRAINT FK_GuarantorConsents_LoanApplications FOREIGN KEY (LoanApplicationId) REFERENCES LoanApplications(Id),
    CONSTRAINT FK_GuarantorConsents_Members FOREIGN KEY (GuarantorMemberId) REFERENCES Members(Id)
);

CREATE INDEX IX_GuarantorConsents_LoanApplicationId ON GuarantorConsents(LoanApplicationId);
CREATE INDEX IX_GuarantorConsents_GuarantorMemberId ON GuarantorConsents(GuarantorMemberId);
CREATE INDEX IX_GuarantorConsents_ConsentStatus ON GuarantorConsents(ConsentStatus);
GO

PRINT 'GuarantorConsents table created';
GO

-- Create CommitteeReviews table
CREATE TABLE CommitteeReviews (
    Id NVARCHAR(450) NOT NULL,
    LoanApplicationId NVARCHAR(450) NOT NULL,
    ReviewerMemberId NVARCHAR(450) NOT NULL,
    ReviewDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ReviewDecision NVARCHAR(50) NOT NULL,
    RecommendedAmount DECIMAL(18,2),
    ReviewComments NVARCHAR(MAX),
    VotingWeight INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(450),
    UpdatedBy NVARCHAR(450),
    CONSTRAINT PK_CommitteeReviews PRIMARY KEY (Id),
    CONSTRAINT FK_CommitteeReviews_LoanApplications FOREIGN KEY (LoanApplicationId) REFERENCES LoanApplications(Id),
    CONSTRAINT FK_CommitteeReviews_Members FOREIGN KEY (ReviewerMemberId) REFERENCES Members(Id)
);

CREATE INDEX IX_CommitteeReviews_LoanApplicationId ON CommitteeReviews(LoanApplicationId);
CREATE INDEX IX_CommitteeReviews_ReviewerMemberId ON CommitteeReviews(ReviewerMemberId);
CREATE INDEX IX_CommitteeReviews_ReviewDate ON CommitteeReviews(ReviewDate);
GO

PRINT 'CommitteeReviews table created';
GO

-- Create LoanRegisters table
CREATE TABLE LoanRegisters (
    Id NVARCHAR(450) NOT NULL,
    LoanId NVARCHAR(450) NOT NULL,
    RegisterDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    EntryType NVARCHAR(50) NOT NULL,
    Description NVARCHAR(MAX),
    Amount DECIMAL(18,2) NOT NULL,
    Balance DECIMAL(18,2) NOT NULL,
    TransactionReference NVARCHAR(100),
    RecordedBy NVARCHAR(450),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(450),
    UpdatedBy NVARCHAR(450),
    CONSTRAINT PK_LoanRegisters PRIMARY KEY (Id),
    CONSTRAINT FK_LoanRegisters_Loans FOREIGN KEY (LoanId) REFERENCES Loans(Id)
);

CREATE INDEX IX_LoanRegisters_LoanId ON LoanRegisters(LoanId);
CREATE INDEX IX_LoanRegisters_RegisterDate ON LoanRegisters(RegisterDate);
CREATE INDEX IX_LoanRegisters_EntryType ON LoanRegisters(EntryType);
GO

PRINT 'LoanRegisters table created';
GO

-- Create MonthlyThresholds table
CREATE TABLE MonthlyThresholds (
    Id NVARCHAR(450) NOT NULL,
    Month INT NOT NULL,
    Year INT NOT NULL,
    MaxLoanAmount DECIMAL(18,2) NOT NULL,
    TotalDisbursed DECIMAL(18,2) NOT NULL DEFAULT 0,
    RemainingAmount DECIMAL(18,2) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CreatedBy NVARCHAR(450),
    UpdatedBy NVARCHAR(450),
    CONSTRAINT PK_MonthlyThresholds PRIMARY KEY (Id),
    CONSTRAINT UQ_MonthlyThresholds_Month_Year UNIQUE (Month, Year)
);

CREATE INDEX IX_MonthlyThresholds_Year_Month ON MonthlyThresholds(Year, Month);
CREATE INDEX IX_MonthlyThresholds_IsActive ON MonthlyThresholds(IsActive);
GO

PRINT 'MonthlyThresholds table created';
GO

PRINT '========================================';
PRINT 'All Cooperative Loan Management tables created successfully!';
PRINT '========================================';
