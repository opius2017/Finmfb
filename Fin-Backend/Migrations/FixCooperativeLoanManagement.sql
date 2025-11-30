-- Migration Fix: Drop and recreate tables with correct foreign key constraints
-- Date: 2024-11-30
-- Description: Fixes foreign key constraint issues

-- Drop tables in reverse order of dependencies
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

-- Now recreate LoanApplications with correct structure
CREATE TABLE LoanApplications (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    ApplicationNumber NVARCHAR(50) NOT NULL UNIQUE,
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
    CONSTRAINT FK_LoanApplications_Members FOREIGN KEY (MemberId) REFERENCES Members(Id)
);

CREATE INDEX IX_LoanApplications_ApplicationNumber ON LoanApplications(ApplicationNumber);
CREATE INDEX IX_LoanApplications_MemberId ON LoanApplications(MemberId);
CREATE INDEX IX_LoanApplications_ApplicationStatus ON LoanApplications(ApplicationStatus);
CREATE INDEX IX_LoanApplications_ApplicationDate ON LoanApplications(ApplicationDate);
GO

-- Create Loans table
CREATE TABLE Loans (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    LoanNumber NVARCHAR(50) NOT NULL UNIQUE,
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
    CONSTRAINT FK_Loans_LoanApplications FOREIGN KEY (LoanApplicationId) REFERENCES LoanApplications(Id),
    CONSTRAINT FK_Loans_Members FOREIGN KEY (MemberId) REFERENCES Members(Id)
);

CREATE INDEX IX_Loans_LoanNumber ON Loans(LoanNumber);
CREATE INDEX IX_Loans_MemberId ON Loans(MemberId);
CREATE INDEX IX_Loans_LoanStatus ON Loans(LoanStatus);
CREATE INDEX IX_Loans_Classification ON Loans(Classification);
CREATE INDEX IX_Loans_DisbursementDate ON Loans(DisbursementDate);
GO

-- Create GuarantorConsents table
CREATE TABLE GuarantorConsents (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
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
    CONSTRAINT FK_GuarantorConsents_LoanApplications FOREIGN KEY (LoanApplicationId) REFERENCES LoanApplications(Id),
    CONSTRAINT FK_GuarantorConsents_Members FOREIGN KEY (GuarantorMemberId) REFERENCES Members(Id)
);

CREATE INDEX IX_GuarantorConsents_LoanApplicationId ON GuarantorConsents(LoanApplicationId);
CREATE INDEX IX_GuarantorConsents_GuarantorMemberId ON GuarantorConsents(GuarantorMemberId);
CREATE INDEX IX_GuarantorConsents_ConsentStatus ON GuarantorConsents(ConsentStatus);
GO

-- Create CommitteeReviews table
CREATE TABLE CommitteeReviews (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
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
    CONSTRAINT FK_CommitteeReviews_LoanApplications FOREIGN KEY (LoanApplicationId) REFERENCES LoanApplications(Id),
    CONSTRAINT FK_CommitteeReviews_Members FOREIGN KEY (ReviewerMemberId) REFERENCES Members(Id)
);

CREATE INDEX IX_CommitteeReviews_LoanApplicationId ON CommitteeReviews(LoanApplicationId);
CREATE INDEX IX_CommitteeReviews_ReviewerMemberId ON CommitteeReviews(ReviewerMemberId);
CREATE INDEX IX_CommitteeReviews_ReviewDate ON CommitteeReviews(ReviewDate);
GO

-- Create LoanRegisters table
CREATE TABLE LoanRegisters (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
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
    CONSTRAINT FK_LoanRegisters_Loans FOREIGN KEY (LoanId) REFERENCES Loans(Id)
);

CREATE INDEX IX_LoanRegisters_LoanId ON LoanRegisters(LoanId);
CREATE INDEX IX_LoanRegisters_RegisterDate ON LoanRegisters(RegisterDate);
CREATE INDEX IX_LoanRegisters_EntryType ON LoanRegisters(EntryType);
GO

PRINT 'Cooperative Loan Management tables fixed and recreated successfully';
