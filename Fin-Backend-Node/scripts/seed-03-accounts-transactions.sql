-- ============================================
-- Seed Accounts and Transactions
-- ============================================
USE SoarMFBDb;
GO

PRINT 'Seeding accounts and transactions...';

-- Get first user ID for created_by
DECLARE @userId NVARCHAR(36) = (SELECT TOP 1 id FROM users WHERE email = 'admin@soarmfb.ng');

-- Insert Accounts for each member
INSERT INTO accounts (id, account_number, member_id, type, balance, status, branch_id)
SELECT 
    NEWID(),
    'SAV' + RIGHT('0000000' + CAST(ROW_NUMBER() OVER (ORDER BY m.member_number) AS VARCHAR), 7),
    m.id,
    'SAVINGS',
    CAST((RAND(CHECKSUM(NEWID())) * 500000 + 50000) AS DECIMAL(15,2)),
    'ACTIVE',
    m.branch_id
FROM members m;

-- Insert SHARES accounts for some members
INSERT INTO accounts (id, account_number, member_id, type, balance, status, branch_id)
SELECT 
    NEWID(),
    'SHR' + RIGHT('0000000' + CAST(ROW_NUMBER() OVER (ORDER BY m.member_number) AS VARCHAR), 7),
    m.id,
    'SHARES',
    CAST((RAND(CHECKSUM(NEWID())) * 100000 + 10000) AS DECIMAL(15,2)),
    'ACTIVE',
    m.branch_id
FROM members m
WHERE m.member_number IN ('MEM001', 'MEM003', 'MEM005', 'MEM007', 'MEM009');

-- Insert sample transactions for the first 5 accounts
DECLARE @accountId NVARCHAR(36);
DECLARE @counter INT = 1;

DECLARE account_cursor CURSOR FOR
SELECT TOP 5 id FROM accounts ORDER BY account_number;

OPEN account_cursor;
FETCH NEXT FROM account_cursor INTO @accountId;

WHILE @@FETCH_STATUS = 0 AND @counter <= 5
BEGIN
    -- Deposit transactions
    INSERT INTO transactions (id, account_id, type, amount, description, reference, status, created_by, created_at)
    VALUES 
    (NEWID(), @accountId, 'CREDIT', 50000.00, 'Initial Deposit', 'TXN' + FORMAT(GETDATE(), 'yyyyMMdd') + RIGHT('000000' + CAST(@counter * 10 + 1 AS VARCHAR), 6), 'COMPLETED', @userId, DATEADD(DAY, -30, GETDATE())),
    (NEWID(), @accountId, 'CREDIT', 25000.00, 'Cash Deposit', 'TXN' + FORMAT(GETDATE(), 'yyyyMMdd') + RIGHT('000000' + CAST(@counter * 10 + 2 AS VARCHAR), 6), 'COMPLETED', @userId, DATEADD(DAY, -20, GETDATE())),
    (NEWID(), @accountId, 'CREDIT', 15000.00, 'Transfer In', 'TXN' + FORMAT(GETDATE(), 'yyyyMMdd') + RIGHT('000000' + CAST(@counter * 10 + 3 AS VARCHAR), 6), 'COMPLETED', @userId, DATEADD(DAY, -10, GETDATE()));
    
    -- Withdrawal transactions
    INSERT INTO transactions (id, account_id, type, amount, description, reference, status, created_by, created_at)
    VALUES 
    (NEWID(), @accountId, 'DEBIT', 10000.00, 'Cash Withdrawal', 'TXN' + FORMAT(GETDATE(), 'yyyyMMdd') + RIGHT('000000' + CAST(@counter * 10 + 4 AS VARCHAR), 6), 'COMPLETED', @userId, DATEADD(DAY, -15, GETDATE())),
    (NEWID(), @accountId, 'DEBIT', 5000.00, 'Transfer Out', 'TXN' + FORMAT(GETDATE(), 'yyyyMMdd') + RIGHT('000000' + CAST(@counter * 10 + 5 AS VARCHAR), 6), 'COMPLETED', @userId, DATEADD(DAY, -5, GETDATE()));
    
    SET @counter = @counter + 1;
    FETCH NEXT FROM account_cursor INTO @accountId;
END

CLOSE account_cursor;
DEALLOCATE account_cursor;

PRINT 'Accounts and transactions seeded successfully';
GO
