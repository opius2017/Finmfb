-- ============================================
-- Seed Loan Products and Loans
-- ============================================
USE SoarMFBDb;
GO

PRINT 'Seeding loan products and loans...';

-- Get first user ID for created_by
DECLARE @userId NVARCHAR(36) = (SELECT TOP 1 id FROM users WHERE email = 'loanofficer@soarmfb.ng');

-- Insert Loan Products
INSERT INTO loan_products (id, name, description, interest_rate, min_amount, max_amount, min_term_months, max_term_months, calculation_method, penalty_rate, is_active) VALUES
(NEWID(), 'Personal Loan', 'Short-term personal loans for individuals', 0.18, 50000, 500000, 3, 12, 'reducing_balance', 0.02, 1),
(NEWID(), 'Business Loan', 'Working capital loans for small businesses', 0.15, 100000, 2000000, 6, 24, 'reducing_balance', 0.02, 1),
(NEWID(), 'Agricultural Loan', 'Loans for farmers and agribusiness', 0.12, 50000, 1000000, 6, 18, 'reducing_balance', 0.015, 1),
(NEWID(), 'Education Loan', 'Loans for educational purposes', 0.10, 50000, 500000, 12, 36, 'reducing_balance', 0.01, 1),
(NEWID(), 'Emergency Loan', 'Quick loans for emergencies', 0.20, 20000, 200000, 1, 6, 'flat_rate', 0.03, 1);

-- Get loan product IDs
DECLARE @personalLoanId NVARCHAR(36) = (SELECT id FROM loan_products WHERE name = 'Personal Loan');
DECLARE @businessLoanId NVARCHAR(36) = (SELECT id FROM loan_products WHERE name = 'Business Loan');
DECLARE @agriLoanId NVARCHAR(36) = (SELECT id FROM loan_products WHERE name = 'Agricultural Loan');

-- Insert Loans for some members
INSERT INTO loans (id, member_id, loan_product_id, requested_amount, approved_amount, disbursed_amount, outstanding_balance, interest_rate, term_months, purpose, status, application_date, approval_date, disbursement_date, created_by)
SELECT 
    NEWID(),
    m.id,
    @personalLoanId,
    200000.00,
    200000.00,
    200000.00,
    150000.00,
    0.18,
    12,
    'Home renovation',
    'ACTIVE',
    DATEADD(MONTH, -6, GETDATE()),
    DATEADD(MONTH, -6, DATEADD(DAY, 3, GETDATE())),
    DATEADD(MONTH, -6, DATEADD(DAY, 5, GETDATE())),
    @userId
FROM members m WHERE m.member_number = 'MEM001';

INSERT INTO loans (id, member_id, loan_product_id, requested_amount, approved_amount, disbursed_amount, outstanding_balance, interest_rate, term_months, purpose, status, application_date, approval_date, disbursement_date, created_by)
SELECT 
    NEWID(),
    m.id,
    @businessLoanId,
    500000.00,
    500000.00,
    500000.00,
    400000.00,
    0.15,
    18,
    'Business expansion',
    'ACTIVE',
    DATEADD(MONTH, -8, GETDATE()),
    DATEADD(MONTH, -8, DATEADD(DAY, 2, GETDATE())),
    DATEADD(MONTH, -8, DATEADD(DAY, 4, GETDATE())),
    @userId
FROM members m WHERE m.member_number = 'MEM003';

INSERT INTO loans (id, member_id, loan_product_id, requested_amount, approved_amount, disbursed_amount, outstanding_balance, interest_rate, term_months, purpose, status, application_date, approval_date, disbursement_date, created_by)
SELECT 
    NEWID(),
    m.id,
    @agriLoanId,
    300000.00,
    300000.00,
    300000.00,
    250000.00,
    0.12,
    12,
    'Farm equipment purchase',
    'ACTIVE',
    DATEADD(MONTH, -4, GETDATE()),
    DATEADD(MONTH, -4, DATEADD(DAY, 3, GETDATE())),
    DATEADD(MONTH, -4, DATEADD(DAY, 5, GETDATE())),
    @userId
FROM members m WHERE m.member_number = 'MEM005';

-- Insert pending loan applications
INSERT INTO loans (id, member_id, loan_product_id, requested_amount, interest_rate, term_months, purpose, status, application_date, created_by)
SELECT 
    NEWID(),
    m.id,
    @personalLoanId,
    150000.00,
    0.18,
    12,
    'Medical expenses',
    'PENDING',
    DATEADD(DAY, -2, GETDATE()),
    @userId
FROM members m WHERE m.member_number = 'MEM007';

INSERT INTO loans (id, member_id, loan_product_id, requested_amount, interest_rate, term_months, purpose, status, application_date, created_by)
SELECT 
    NEWID(),
    m.id,
    @businessLoanId,
    800000.00,
    0.15,
    24,
    'Inventory purchase',
    'PENDING',
    DATEADD(DAY, -1, GETDATE()),
    @userId
FROM members m WHERE m.member_number = 'MEM009';

PRINT 'Loan products and loans seeded successfully';
GO
