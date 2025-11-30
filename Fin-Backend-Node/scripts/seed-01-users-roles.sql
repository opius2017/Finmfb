-- ============================================
-- Seed Users, Roles, and Permissions
-- ============================================
USE SoarMFBDb;
GO

PRINT 'Seeding users, roles, and permissions...';

-- Insert Roles
INSERT INTO roles (id, name, description) VALUES
(NEWID(), 'Super Admin', 'Full system access'),
(NEWID(), 'Branch Manager', 'Manage branch operations'),
(NEWID(), 'Loan Officer', 'Process and manage loans'),
(NEWID(), 'Accountant', 'Financial operations and reporting'),
(NEWID(), 'Teller', 'Customer transactions'),
(NEWID(), 'Compliance Officer', 'Regulatory compliance and reporting');

-- Insert Permissions
INSERT INTO permissions (id, name, resource, action, description) VALUES
-- User Management
(NEWID(), 'Create Users', 'users', 'create', 'Create new users'),
(NEWID(), 'View Users', 'users', 'read', 'View user information'),
(NEWID(), 'Update Users', 'users', 'update', 'Update user information'),
(NEWID(), 'Delete Users', 'users', 'delete', 'Delete users'),
-- Member Management
(NEWID(), 'Create Members', 'members', 'create', 'Register new members'),
(NEWID(), 'View Members', 'members', 'read', 'View member information'),
(NEWID(), 'Update Members', 'members', 'update', 'Update member information'),
-- Account Management
(NEWID(), 'Create Accounts', 'accounts', 'create', 'Open new accounts'),
(NEWID(), 'View Accounts', 'accounts', 'read', 'View account information'),
(NEWID(), 'Update Accounts', 'accounts', 'update', 'Update account information'),
-- Transaction Management
(NEWID(), 'Create Transactions', 'transactions', 'create', 'Process transactions'),
(NEWID(), 'View Transactions', 'transactions', 'read', 'View transactions'),
(NEWID(), 'Approve Transactions', 'transactions', 'approve', 'Approve transactions'),
-- Loan Management
(NEWID(), 'Create Loans', 'loans', 'create', 'Process loan applications'),
(NEWID(), 'View Loans', 'loans', 'read', 'View loan information'),
(NEWID(), 'Approve Loans', 'loans', 'approve', 'Approve loan applications'),
(NEWID(), 'Disburse Loans', 'loans', 'disburse', 'Disburse approved loans'),
-- Regulatory
(NEWID(), 'Create Reports', 'regulatory', 'create', 'Generate regulatory reports'),
(NEWID(), 'View Reports', 'regulatory', 'read', 'View regulatory reports'),
(NEWID(), 'Update Reports', 'regulatory', 'update', 'Update report status'),
(NEWID(), 'Manage Compliance', 'compliance', 'create', 'Manage compliance checklist'),
(NEWID(), 'View Compliance', 'compliance', 'read', 'View compliance information'),
(NEWID(), 'Update Compliance', 'compliance', 'update', 'Update compliance status');

-- Assign permissions to Super Admin role
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
CROSS JOIN permissions p
WHERE r.name = 'Super Admin';

-- Insert Users (password: Password123!)
-- Password hash for 'Password123!' using bcrypt
DECLARE @passwordHash NVARCHAR(255) = '$2a$10$rKJ5VqZ9YqZ9YqZ9YqZ9YeK5VqZ9YqZ9YqZ9YqZ9YqZ9YqZ9YqZ9Y';

INSERT INTO users (id, email, password_hash, first_name, last_name, status) VALUES
(NEWID(), 'admin@soarmfb.ng', @passwordHash, 'Adebayo', 'Ogunlesi', 'ACTIVE'),
(NEWID(), 'manager.lagos@soarmfb.ng', @passwordHash, 'Chioma', 'Nwosu', 'ACTIVE'),
(NEWID(), 'manager.abuja@soarmfb.ng', @passwordHash, 'Ibrahim', 'Mohammed', 'ACTIVE'),
(NEWID(), 'loanofficer@soarmfb.ng', @passwordHash, 'Funmilayo', 'Adeyemi', 'ACTIVE'),
(NEWID(), 'accountant@soarmfb.ng', @passwordHash, 'Emeka', 'Okafor', 'ACTIVE'),
(NEWID(), 'compliance@soarmfb.ng', @passwordHash, 'Aisha', 'Bello', 'ACTIVE'),
(NEWID(), 'teller1@soarmfb.ng', @passwordHash, 'Blessing', 'Eze', 'ACTIVE'),
(NEWID(), 'teller2@soarmfb.ng', @passwordHash, 'Yusuf', 'Abdullahi', 'ACTIVE');

-- Assign roles to users
INSERT INTO user_roles (user_id, role_id)
SELECT u.id, r.id
FROM users u
CROSS JOIN roles r
WHERE u.email = 'admin@soarmfb.ng' AND r.name = 'Super Admin';

INSERT INTO user_roles (user_id, role_id)
SELECT u.id, r.id
FROM users u
CROSS JOIN roles r
WHERE u.email LIKE 'manager%' AND r.name = 'Branch Manager';

INSERT INTO user_roles (user_id, role_id)
SELECT u.id, r.id
FROM users u
CROSS JOIN roles r
WHERE u.email = 'loanofficer@soarmfb.ng' AND r.name = 'Loan Officer';

INSERT INTO user_roles (user_id, role_id)
SELECT u.id, r.id
FROM users u
CROSS JOIN roles r
WHERE u.email = 'accountant@soarmfb.ng' AND r.name = 'Accountant';

INSERT INTO user_roles (user_id, role_id)
SELECT u.id, r.id
FROM users u
CROSS JOIN roles r
WHERE u.email = 'compliance@soarmfb.ng' AND r.name = 'Compliance Officer';

INSERT INTO user_roles (user_id, role_id)
SELECT u.id, r.id
FROM users u
CROSS JOIN roles r
WHERE u.email LIKE 'teller%' AND r.name = 'Teller';

PRINT 'Users, roles, and permissions seeded successfully';
GO
