-- Cooperative Loan Management System - Database Migration Script
-- Generated: December 2024
-- Purpose: Create tables for cooperative loan management features

USE [FinMFB]
GO

-- =============================================
-- 1. Members Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Members')
BEGIN
    CREATE TABLE [dbo].[Members] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [MemberNumber] NVARCHAR(50) NOT NULL UNIQUE,
        [FirstName] NVARCHAR(100) NOT NULL,
        [LastName] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(255) NOT NULL,
        [PhoneNumber] NVARCHAR(20) NOT NULL,
        [PayrollPin] NVARCHAR(50) NULL,
        
        -- Savings and Equity
        [TotalSavings] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [MonthlyContribution] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [ShareCapital] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [FreeEquity] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [LockedEquity] DECIMAL(18,2) NOT NULL DEFAULT 0,
        
        -- Membership
        [MembershipDate] DATETIME2 NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [Status] NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
        
        -- Salary Information
        [GrossSalary] DECIMAL(18,2) NULL,
        [NetSalary] DECIMAL(18,2) NULL,
        [StatutoryDeductions] DECIMAL(18,2) NULL,
        
        -- Credit Profile
        [CreditScore] DECIMAL(5,2) NULL,
        [RiskRating] NVARCHAR(20) NULL,
        [ActiveLoansCount] INT NOT NULL DEFAULT 0,
        [TotalOutstandingLoans] DECIMAL(18,2) NOT NULL DEFAULT 0,
        
        -- Audit Fields
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [LastModifiedDate] DATETIME2 NULL,
        [LastModifiedBy] NVARCHAR(100) NULL,
        
        CONSTRAINT [CK_Members_TotalSavings] CHECK ([TotalSavings] >= 0),
        CONSTRAINT [CK_Members_FreeEquity] CHECK ([FreeEquity] >= 0),
        CONSTRAINT [CK_Members_LockedEquity] CHECK ([LockedEquity] >= 0)
    );
    
    CREATE INDEX [IX_Members_MemberNumber] ON [dbo].[Members]([MemberNumber]);
    CREATE INDEX [IX_Members_Email] ON [dbo].[Members]([Email]);
    CREATE INDEX [IX_Members_PhoneNumber] ON [dbo].[Members]([PhoneNumber]);
    CREATE INDEX [IX_Members_IsActive] ON [dbo].[Members]([IsActive]);
    CREATE INDEX [IX_Members_MembershipDate] ON [dbo].[Members]([MembershipDate]);
    
    PRINT 'Members table created successfully';
END
GO

-- =============================================
-- 2. GuarantorConsents Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GuarantorConsents')
BEGIN
    CREATE TABLE [dbo].[GuarantorConsents] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [ApplicationId] UNIQUEIDENTIFIER NOT NULL,
        [GuarantorMemberId] UNIQUEIDENTIFIER NOT NULL,
        [ApplicantMemberId] UNIQUEIDENTIFIER NOT NULL,
        [GuaranteedAmount] DECIMAL(18,2) NOT NULL,
        [ConsentToken] NVARCHAR(100) NOT NULL UNIQUE,
        [Status] NVARCHAR(20) NOT NULL,
        [RequestedAt] DATETIME2 NOT NULL,
        [RespondedAt] DATETIME2 NULL,
        [ExpiresAt] DATETIME2 NULL,
        [DeclineReason] NVARCHAR(500) NULL,
        [Notes] NVARCHAR(1000) NULL,
        
        -- Audit Fields
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [LastModifiedDate] DATETIME2 NULL,
        [LastModifiedBy] NVARCHAR(100) NULL,
        
        CONSTRAINT [CK_GuarantorConsents_Amount] CHECK ([GuaranteedAmount] > 0)
    );
    
    CREATE INDEX [IX_GuarantorConsents_ApplicationId] ON [dbo].[GuarantorConsents]([ApplicationId]);
    CREATE INDEX [IX_GuarantorConsents_GuarantorMemberId] ON [dbo].[GuarantorConsents]([GuarantorMemberId]);
    CREATE INDEX [IX_GuarantorConsents_Status] ON [dbo].[GuarantorConsents]([Status]);
    CREATE INDEX [IX_GuarantorConsents_ConsentToken] ON [dbo].[GuarantorConsents]([ConsentToken]);
    CREATE INDEX [IX_GuarantorConsents_ExpiresAt] ON [dbo].[GuarantorConsents]([ExpiresAt]);
    
    PRINT 'GuarantorConsents table created successfully';
END
GO

-- =============================================
-- 3. CommitteeReviews Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CommitteeReviews')
BEGIN
    CREATE TABLE [dbo].[CommitteeReviews] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [ApplicationId] UNIQUEIDENTIFIER NOT NULL,
        [ReviewerUserId] NVARCHAR(100) NOT NULL,
        [ReviewerName] NVARCHAR(200) NOT NULL,
        [Decision] NVARCHAR(30) NOT NULL,
        [ReviewDate] DATETIME2 NOT NULL,
        [Comments] NVARCHAR(2000) NULL,
        [RecommendedAction] NVARCHAR(500) NULL,
        
        -- Credit Assessment
        [CreditScore] DECIMAL(5,2) NULL,
        [RiskRating] NVARCHAR(20) NULL,
        [RepaymentScore] DECIMAL(5,2) NULL,
        [SavingsConsistency] BIT NULL,
        [PreviousLoanPerformance] BIT NULL,
        
        -- Recommended Terms
        [RecommendedAmount] DECIMAL(18,2) NULL,
        [RecommendedTenor] INT NULL,
        [RecommendedInterestRate] DECIMAL(5,2) NULL,
        
        -- Audit Fields
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [LastModifiedDate] DATETIME2 NULL,
        [LastModifiedBy] NVARCHAR(100) NULL
    );
    
    CREATE INDEX [IX_CommitteeReviews_ApplicationId] ON [dbo].[CommitteeReviews]([ApplicationId]);
    CREATE INDEX [IX_CommitteeReviews_ReviewerUserId] ON [dbo].[CommitteeReviews]([ReviewerUserId]);
    CREATE INDEX [IX_CommitteeReviews_Decision] ON [dbo].[CommitteeReviews]([Decision]);
    CREATE INDEX [IX_CommitteeReviews_ReviewDate] ON [dbo].[CommitteeReviews]([ReviewDate]);
    
    PRINT 'CommitteeReviews table created successfully';
END
GO

-- =============================================
-- 4. LoanRegisters Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LoanRegisters')
BEGIN
    CREATE TABLE [dbo].[LoanRegisters] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [SerialNumber] NVARCHAR(50) NOT NULL UNIQUE,
        [LoanId] UNIQUEIDENTIFIER NOT NULL,
        [ApplicationId] UNIQUEIDENTIFIER NOT NULL,
        [MemberId] UNIQUEIDENTIFIER NOT NULL,
        [MemberNumber] NVARCHAR(50) NOT NULL,
        [MemberName] NVARCHAR(200) NOT NULL,
        [PrincipalAmount] DECIMAL(18,2) NOT NULL,
        [InterestRate] DECIMAL(5,2) NOT NULL,
        [TenorMonths] INT NOT NULL,
        [MonthlyEMI] DECIMAL(18,2) NOT NULL,
        [RegistrationDate] DATETIME2 NOT NULL,
        [DisbursementDate] DATETIME2 NOT NULL,
        [MaturityDate] DATETIME2 NOT NULL,
        [RegistrationYear] INT NOT NULL,
        [RegistrationMonth] INT NOT NULL,
        [SequenceNumber] INT NOT NULL,
        [LoanType] NVARCHAR(50) NOT NULL,
        [RegisteredBy] NVARCHAR(100) NOT NULL,
        [Status] NVARCHAR(20) NOT NULL,
        [Notes] NVARCHAR(1000) NULL,
        
        -- Audit Fields
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [LastModifiedDate] DATETIME2 NULL,
        [LastModifiedBy] NVARCHAR(100) NULL,
        
        CONSTRAINT [CK_LoanRegisters_PrincipalAmount] CHECK ([PrincipalAmount] > 0),
        CONSTRAINT [CK_LoanRegisters_InterestRate] CHECK ([InterestRate] >= 0),
        CONSTRAINT [CK_LoanRegisters_Tenor] CHECK ([TenorMonths] > 0),
        CONSTRAINT [CK_LoanRegisters_Month] CHECK ([RegistrationMonth] BETWEEN 1 AND 12)
    );
    
    CREATE INDEX [IX_LoanRegisters_SerialNumber] ON [dbo].[LoanRegisters]([SerialNumber]);
    CREATE INDEX [IX_LoanRegisters_LoanId] ON [dbo].[LoanRegisters]([LoanId]);
    CREATE INDEX [IX_LoanRegisters_ApplicationId] ON [dbo].[LoanRegisters]([ApplicationId]);
    CREATE INDEX [IX_LoanRegisters_MemberId] ON [dbo].[LoanRegisters]([MemberId]);
    CREATE INDEX [IX_LoanRegisters_YearMonth] ON [dbo].[LoanRegisters]([RegistrationYear], [RegistrationMonth]);
    CREATE INDEX [IX_LoanRegisters_SequenceNumber] ON [dbo].[LoanRegisters]([SequenceNumber]);
    CREATE INDEX [IX_LoanRegisters_RegistrationDate] ON [dbo].[LoanRegisters]([RegistrationDate]);
    CREATE INDEX [IX_LoanRegisters_Status] ON [dbo].[LoanRegisters]([Status]);
    
    PRINT 'LoanRegisters table created successfully';
END
GO

-- =============================================
-- 5. MonthlyThresholds Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MonthlyThresholds')
BEGIN
    CREATE TABLE [dbo].[MonthlyThresholds] (
        [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
        [Year] INT NOT NULL,
        [Month] INT NOT NULL,
        [MaximumAmount] DECIMAL(18,2) NOT NULL,
        [AllocatedAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [RemainingAmount] DECIMAL(18,2) NOT NULL,
        [TotalApplicationsApproved] INT NOT NULL DEFAULT 0,
        [TotalApplicationsRegistered] INT NOT NULL DEFAULT 0,
        [TotalApplicationsQueued] INT NOT NULL DEFAULT 0,
        [Status] NVARCHAR(20) NOT NULL,
        [ClosedAt] DATETIME2 NULL,
        [ClosedBy] NVARCHAR(100) NULL,
        [Notes] NVARCHAR(1000) NULL,
        
        -- Audit Fields
        [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [LastModifiedDate] DATETIME2 NULL,
        [LastModifiedBy] NVARCHAR(100) NULL,
        
        CONSTRAINT [UQ_MonthlyThresholds_YearMonth] UNIQUE ([Year], [Month]),
        CONSTRAINT [CK_MonthlyThresholds_MaxAmount] CHECK ([MaximumAmount] > 0),
        CONSTRAINT [CK_MonthlyThresholds_Month] CHECK ([Month] BETWEEN 1 AND 12),
        CONSTRAINT [CK_MonthlyThresholds_Allocated] CHECK ([AllocatedAmount] >= 0),
        CONSTRAINT [CK_MonthlyThresholds_Remaining] CHECK ([RemainingAmount] >= 0)
    );
    
    CREATE INDEX [IX_MonthlyThresholds_Year] ON [dbo].[MonthlyThresholds]([Year]);
    CREATE INDEX [IX_MonthlyThresholds_Status] ON [dbo].[MonthlyThresholds]([Status]);
    CREATE INDEX [IX_MonthlyThresholds_ClosedAt] ON [dbo].[MonthlyThresholds]([ClosedAt]);
    
    PRINT 'MonthlyThresholds table created successfully';
END
GO

-- =============================================
-- 6. Insert Default Data
-- =============================================

-- Insert default threshold for current month
DECLARE @CurrentYear INT = YEAR(GETUTCDATE());
DECLARE @CurrentMonth INT = MONTH(GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[MonthlyThresholds] WHERE [Year] = @CurrentYear AND [Month] = @CurrentMonth)
BEGIN
    INSERT INTO [dbo].[MonthlyThresholds] (
        [Id], [Year], [Month], [MaximumAmount], [AllocatedAmount], 
        [RemainingAmount], [Status], [CreatedDate]
    )
    VALUES (
        NEWID(), @CurrentYear, @CurrentMonth, 3000000.00, 0.00, 
        3000000.00, 'Open', GETUTCDATE()
    );
    
    PRINT 'Default threshold for current month created';
END
GO

-- =============================================
-- 7. Create Stored Procedures
-- =============================================

-- Procedure to get next serial number
IF OBJECT_ID('sp_GetNextLoanSerialNumber', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetNextLoanSerialNumber;
GO

CREATE PROCEDURE sp_GetNextLoanSerialNumber
    @Year INT,
    @Month INT,
    @SerialNumber NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @NextSequence INT;
    
    -- Get the next sequence number for the year
    SELECT @NextSequence = ISNULL(MAX([SequenceNumber]), 0) + 1
    FROM [dbo].[LoanRegisters]
    WHERE [RegistrationYear] = @Year;
    
    -- Format: LH/YYYY/NNN
    SET @SerialNumber = 'LH/' + CAST(@Year AS NVARCHAR(4)) + '/' + RIGHT('000' + CAST(@NextSequence AS NVARCHAR(3)), 3);
    
    RETURN @NextSequence;
END
GO

-- Procedure to check threshold availability
IF OBJECT_ID('sp_CheckThresholdAvailability', 'P') IS NOT NULL
    DROP PROCEDURE sp_CheckThresholdAvailability;
GO

CREATE PROCEDURE sp_CheckThresholdAvailability
    @Year INT,
    @Month INT,
    @RequestedAmount DECIMAL(18,2),
    @CanAllocate BIT OUTPUT,
    @RemainingAmount DECIMAL(18,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        @RemainingAmount = [RemainingAmount],
        @CanAllocate = CASE 
            WHEN [Status] = 'Open' AND [RemainingAmount] >= @RequestedAmount THEN 1 
            ELSE 0 
        END
    FROM [dbo].[MonthlyThresholds]
    WHERE [Year] = @Year AND [Month] = @Month;
    
    -- If no threshold exists, create one
    IF @RemainingAmount IS NULL
    BEGIN
        INSERT INTO [dbo].[MonthlyThresholds] (
            [Id], [Year], [Month], [MaximumAmount], [AllocatedAmount], 
            [RemainingAmount], [Status], [CreatedDate]
        )
        VALUES (
            NEWID(), @Year, @Month, 3000000.00, 0.00, 
            3000000.00, 'Open', GETUTCDATE()
        );
        
        SET @RemainingAmount = 3000000.00;
        SET @CanAllocate = CASE WHEN @RequestedAmount <= 3000000.00 THEN 1 ELSE 0 END;
    END
END
GO

-- =============================================
-- 8. Create Views
-- =============================================

-- View for monthly register summary
IF OBJECT_ID('vw_MonthlyRegisterSummary', 'V') IS NOT NULL
    DROP VIEW vw_MonthlyRegisterSummary;
GO

CREATE VIEW vw_MonthlyRegisterSummary
AS
SELECT 
    [RegistrationYear],
    [RegistrationMonth],
    COUNT(*) AS [TotalLoans],
    SUM([PrincipalAmount]) AS [TotalPrincipal],
    AVG([InterestRate]) AS [AverageInterestRate],
    AVG([TenorMonths]) AS [AverageTenor],
    MIN([RegistrationDate]) AS [FirstRegistration],
    MAX([RegistrationDate]) AS [LastRegistration]
FROM [dbo].[LoanRegisters]
GROUP BY [RegistrationYear], [RegistrationMonth];
GO

-- View for threshold utilization
IF OBJECT_ID('vw_ThresholdUtilization', 'V') IS NOT NULL
    DROP VIEW vw_ThresholdUtilization;
GO

CREATE VIEW vw_ThresholdUtilization
AS
SELECT 
    [Year],
    [Month],
    [MaximumAmount],
    [AllocatedAmount],
    [RemainingAmount],
    CAST(([AllocatedAmount] / [MaximumAmount] * 100) AS DECIMAL(5,2)) AS [UtilizationPercentage],
    [TotalApplicationsRegistered],
    [TotalApplicationsQueued],
    [Status]
FROM [dbo].[MonthlyThresholds];
GO

-- =============================================
-- 9. Grant Permissions (if needed)
-- =============================================

-- Grant permissions to application user
-- GRANT SELECT, INSERT, UPDATE ON [dbo].[Members] TO [YourAppUser];
-- GRANT SELECT, INSERT, UPDATE ON [dbo].[GuarantorConsents] TO [YourAppUser];
-- GRANT SELECT, INSERT, UPDATE ON [dbo].[CommitteeReviews] TO [YourAppUser];
-- GRANT SELECT, INSERT, UPDATE ON [dbo].[LoanRegisters] TO [YourAppUser];
-- GRANT SELECT, INSERT, UPDATE ON [dbo].[MonthlyThresholds] TO [YourAppUser];
-- GRANT EXECUTE ON sp_GetNextLoanSerialNumber TO [YourAppUser];
-- GRANT EXECUTE ON sp_CheckThresholdAvailability TO [YourAppUser];

PRINT '==============================================';
PRINT 'Cooperative Loan Management System';
PRINT 'Database Migration Completed Successfully!';
PRINT '==============================================';
PRINT '';
PRINT 'Tables Created:';
PRINT '  - Members';
PRINT '  - GuarantorConsents';
PRINT '  - CommitteeReviews';
PRINT '  - LoanRegisters';
PRINT '  - MonthlyThresholds';
PRINT '';
PRINT 'Stored Procedures Created:';
PRINT '  - sp_GetNextLoanSerialNumber';
PRINT '  - sp_CheckThresholdAvailability';
PRINT '';
PRINT 'Views Created:';
PRINT '  - vw_MonthlyRegisterSummary';
PRINT '  - vw_ThresholdUtilization';
PRINT '';
PRINT 'Next Steps:';
PRINT '  1. Review the created tables and indexes';
PRINT '  2. Test the stored procedures';
PRINT '  3. Configure application connection string';
PRINT '  4. Start the backend API';
PRINT '  5. Test API endpoints via Swagger';
PRINT '==============================================';
GO
