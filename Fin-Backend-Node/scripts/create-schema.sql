-- ============================================
-- Create All Tables
-- ============================================
USE SoarMFBDb;
GO

-- ============================================
-- AUTHENTICATION & AUTHORIZATION
-- ============================================

CREATE TABLE users (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    email NVARCHAR(255) UNIQUE NOT NULL,
    password_hash NVARCHAR(255) NOT NULL,
    first_name NVARCHAR(100) NOT NULL,
    last_name NVARCHAR(100) NOT NULL,
    status NVARCHAR(20) DEFAULT 'ACTIVE',
    mfa_enabled BIT DEFAULT 0,
    mfa_secret NVARCHAR(255),
    last_login_at DATETIME2,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

CREATE INDEX idx_users_email ON users(email);

CREATE TABLE roles (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    name NVARCHAR(100) UNIQUE NOT NULL,
    description NVARCHAR(500),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE permissions (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    name NVARCHAR(100) UNIQUE NOT NULL,
    description NVARCHAR(500),
    resource NVARCHAR(100) NOT NULL,
    action NVARCHAR(50) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT uq_permissions_resource_action UNIQUE (resource, action)
);

CREATE TABLE user_roles (
    user_id NVARCHAR(36) NOT NULL,
    role_id NVARCHAR(36) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    PRIMARY KEY (user_id, role_id),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE
);

CREATE TABLE role_permissions (
    role_id NVARCHAR(36) NOT NULL,
    permission_id NVARCHAR(36) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    PRIMARY KEY (role_id, permission_id),
    FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE,
    FOREIGN KEY (permission_id) REFERENCES permissions(id) ON DELETE CASCADE
);

CREATE TABLE sessions (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    user_id NVARCHAR(36) NOT NULL,
    refresh_token NVARCHAR(500) UNIQUE NOT NULL,
    ip_address NVARCHAR(50),
    user_agent NVARCHAR(500),
    expires_at DATETIME2 NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE INDEX idx_sessions_user_id ON sessions(user_id);
CREATE INDEX idx_sessions_refresh_token ON sessions(refresh_token);

-- ============================================
-- MEMBERS & ACCOUNTS
-- ============================================

CREATE TABLE branches (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    name NVARCHAR(200) NOT NULL,
    code NVARCHAR(20) UNIQUE NOT NULL,
    address NVARCHAR(500),
    city NVARCHAR(100),
    state NVARCHAR(100),
    country NVARCHAR(100) DEFAULT 'Nigeria',
    status NVARCHAR(20) DEFAULT 'ACTIVE',
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE members (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    member_number NVARCHAR(50) UNIQUE NOT NULL,
    first_name NVARCHAR(100) NOT NULL,
    last_name NVARCHAR(100) NOT NULL,
    email NVARCHAR(255),
    phone NVARCHAR(20) NOT NULL,
    date_of_birth DATETIME2,
    address NVARCHAR(500),
    city NVARCHAR(100),
    state NVARCHAR(100),
    country NVARCHAR(100) DEFAULT 'Nigeria',
    status NVARCHAR(20) DEFAULT 'ACTIVE',
    join_date DATETIME2 DEFAULT GETDATE(),
    branch_id NVARCHAR(36),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (branch_id) REFERENCES branches(id)
);

CREATE INDEX idx_members_member_number ON members(member_number);
CREATE INDEX idx_members_branch_id ON members(branch_id);
CREATE INDEX idx_members_email ON members(email);

CREATE TABLE accounts (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    account_number NVARCHAR(50) UNIQUE NOT NULL,
    member_id NVARCHAR(36) NOT NULL,
    type NVARCHAR(20) NOT NULL, -- SAVINGS, SHARES, CASH
    balance DECIMAL(15, 2) DEFAULT 0,
    status NVARCHAR(20) DEFAULT 'ACTIVE',
    branch_id NVARCHAR(36),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (member_id) REFERENCES members(id),
    FOREIGN KEY (branch_id) REFERENCES branches(id)
);

CREATE INDEX idx_accounts_account_number ON accounts(account_number);
CREATE INDEX idx_accounts_member_id ON accounts(member_id);
CREATE INDEX idx_accounts_branch_id ON accounts(branch_id);

CREATE TABLE transactions (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    account_id NVARCHAR(36) NOT NULL,
    type NVARCHAR(20) NOT NULL, -- DEBIT, CREDIT
    amount DECIMAL(15, 2) NOT NULL,
    description NVARCHAR(500),
    reference NVARCHAR(100) UNIQUE NOT NULL,
    status NVARCHAR(20) DEFAULT 'PENDING',
    reconciliation_status NVARCHAR(20) DEFAULT 'UNMATCHED',
    reconciled_at DATETIME2,
    reconciled_by NVARCHAR(36),
    metadata NVARCHAR(MAX),
    created_by NVARCHAR(36) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (account_id) REFERENCES accounts(id)
);

CREATE INDEX idx_transactions_account_id ON transactions(account_id);
CREATE INDEX idx_transactions_reference ON transactions(reference);
CREATE INDEX idx_transactions_status ON transactions(status);
CREATE INDEX idx_transactions_created_at ON transactions(created_at);

-- ============================================
-- LOANS
-- ============================================

CREATE TABLE loan_products (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    name NVARCHAR(200) NOT NULL,
    description NVARCHAR(1000),
    interest_rate DECIMAL(5, 4) NOT NULL,
    min_amount DECIMAL(15, 2) NOT NULL,
    max_amount DECIMAL(15, 2) NOT NULL,
    min_term_months INT NOT NULL,
    max_term_months INT NOT NULL,
    calculation_method NVARCHAR(50) NOT NULL, -- reducing_balance, flat_rate
    penalty_rate DECIMAL(5, 4) DEFAULT 0.01,
    is_active BIT DEFAULT 1,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE loans (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    member_id NVARCHAR(36) NOT NULL,
    loan_product_id NVARCHAR(36) NOT NULL,
    requested_amount DECIMAL(15, 2) NOT NULL,
    approved_amount DECIMAL(15, 2) DEFAULT 0,
    disbursed_amount DECIMAL(15, 2) DEFAULT 0,
    outstanding_balance DECIMAL(15, 2) DEFAULT 0,
    interest_rate DECIMAL(5, 4) NOT NULL,
    term_months INT NOT NULL,
    purpose NVARCHAR(500) NOT NULL,
    collateral_description NVARCHAR(1000),
    status NVARCHAR(20) DEFAULT 'PENDING',
    application_date DATETIME2 DEFAULT GETDATE(),
    approval_date DATETIME2,
    disbursement_date DATETIME2,
    closed_date DATETIME2,
    metadata NVARCHAR(MAX),
    created_by NVARCHAR(36) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (member_id) REFERENCES members(id),
    FOREIGN KEY (loan_product_id) REFERENCES loan_products(id)
);

CREATE INDEX idx_loans_member_id ON loans(member_id);
CREATE INDEX idx_loans_status ON loans(status);
CREATE INDEX idx_loans_application_date ON loans(application_date);

CREATE TABLE loan_schedules (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    loan_id NVARCHAR(36) NOT NULL,
    payment_number INT NOT NULL,
    due_date DATETIME2 NOT NULL,
    principal DECIMAL(15, 2) NOT NULL,
    interest DECIMAL(15, 2) NOT NULL,
    total_payment DECIMAL(15, 2) NOT NULL,
    balance DECIMAL(15, 2) NOT NULL,
    paid_amount DECIMAL(15, 2) DEFAULT 0,
    is_paid BIT DEFAULT 0,
    paid_date DATETIME2,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (loan_id) REFERENCES loans(id) ON DELETE CASCADE
);

CREATE INDEX idx_loan_schedules_loan_id ON loan_schedules(loan_id);
CREATE INDEX idx_loan_schedules_due_date ON loan_schedules(due_date);
CREATE INDEX idx_loan_schedules_is_paid ON loan_schedules(is_paid);

CREATE TABLE loan_payments (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    loan_id NVARCHAR(36) NOT NULL,
    amount DECIMAL(15, 2) NOT NULL,
    payment_date DATETIME2 NOT NULL,
    payment_method NVARCHAR(50) NOT NULL,
    reference NVARCHAR(100) UNIQUE NOT NULL,
    notes NVARCHAR(1000),
    metadata NVARCHAR(MAX),
    created_by NVARCHAR(36) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (loan_id) REFERENCES loans(id)
);

CREATE INDEX idx_loan_payments_loan_id ON loan_payments(loan_id);
CREATE INDEX idx_loan_payments_payment_date ON loan_payments(payment_date);

CREATE TABLE guarantors (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    loan_id NVARCHAR(36) NOT NULL,
    member_id NVARCHAR(36) NOT NULL,
    guaranteed_amount DECIMAL(15, 2) NOT NULL,
    status NVARCHAR(20) DEFAULT 'PENDING',
    approved_at DATETIME2,
    approved_by NVARCHAR(36),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (loan_id) REFERENCES loans(id) ON DELETE CASCADE,
    FOREIGN KEY (member_id) REFERENCES members(id)
);

CREATE INDEX idx_guarantors_loan_id ON guarantors(loan_id);
CREATE INDEX idx_guarantors_member_id ON guarantors(member_id);

-- ============================================
-- BUDGETS
-- ============================================

CREATE TABLE budgets (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    name NVARCHAR(200) NOT NULL,
    description NVARCHAR(1000),
    start_date DATETIME2 NOT NULL,
    end_date DATETIME2 NOT NULL,
    fiscal_year INT NOT NULL,
    total_amount DECIMAL(15, 2) DEFAULT 0,
    status NVARCHAR(20) DEFAULT 'DRAFT',
    branch_id NVARCHAR(36),
    metadata NVARCHAR(MAX),
    created_by NVARCHAR(36) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (branch_id) REFERENCES branches(id)
);

CREATE INDEX idx_budgets_fiscal_year ON budgets(fiscal_year);
CREATE INDEX idx_budgets_status ON budgets(status);

CREATE TABLE budget_items (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    budget_id NVARCHAR(36) NOT NULL,
    name NVARCHAR(200) NOT NULL,
    category NVARCHAR(100) NOT NULL,
    amount DECIMAL(15, 2) NOT NULL,
    description NVARCHAR(1000),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (budget_id) REFERENCES budgets(id) ON DELETE CASCADE
);

CREATE INDEX idx_budget_items_budget_id ON budget_items(budget_id);
CREATE INDEX idx_budget_items_category ON budget_items(category);

CREATE TABLE budget_actuals (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    budget_id NVARCHAR(36) NOT NULL,
    budget_item_id NVARCHAR(36) NOT NULL,
    amount DECIMAL(15, 2) NOT NULL,
    date DATETIME2 NOT NULL,
    category NVARCHAR(100) NOT NULL,
    description NVARCHAR(500),
    reference NVARCHAR(100),
    metadata NVARCHAR(MAX),
    created_by NVARCHAR(36) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (budget_id) REFERENCES budgets(id) ON DELETE CASCADE,
    FOREIGN KEY (budget_item_id) REFERENCES budget_items(id) ON DELETE CASCADE
);

CREATE INDEX idx_budget_actuals_budget_id ON budget_actuals(budget_id);
CREATE INDEX idx_budget_actuals_budget_item_id ON budget_actuals(budget_item_id);
CREATE INDEX idx_budget_actuals_date ON budget_actuals(date);

-- ============================================
-- DOCUMENTS
-- ============================================

CREATE TABLE documents (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    name NVARCHAR(200) NOT NULL,
    description NVARCHAR(1000),
    category NVARCHAR(100) NOT NULL,
    entity_type NVARCHAR(100) NOT NULL,
    entity_id NVARCHAR(36) NOT NULL,
    filename NVARCHAR(255) NOT NULL,
    original_name NVARCHAR(255) NOT NULL,
    mime_type NVARCHAR(100) NOT NULL,
    size INT NOT NULL,
    url NVARCHAR(500) NOT NULL,
    tags NVARCHAR(MAX),
    current_version INT DEFAULT 1,
    is_deleted BIT DEFAULT 0,
    deleted_at DATETIME2,
    deleted_by NVARCHAR(36),
    metadata NVARCHAR(MAX),
    uploaded_by NVARCHAR(36) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

CREATE INDEX idx_documents_entity ON documents(entity_type, entity_id);
CREATE INDEX idx_documents_category ON documents(category);
CREATE INDEX idx_documents_is_deleted ON documents(is_deleted);

CREATE TABLE document_versions (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    document_id NVARCHAR(36) NOT NULL,
    version_number INT NOT NULL,
    filename NVARCHAR(255) NOT NULL,
    original_name NVARCHAR(255) NOT NULL,
    mime_type NVARCHAR(100) NOT NULL,
    size INT NOT NULL,
    url NVARCHAR(500) NOT NULL,
    change_description NVARCHAR(1000),
    uploaded_by NVARCHAR(36) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (document_id) REFERENCES documents(id) ON DELETE CASCADE
);

CREATE INDEX idx_document_versions_document_id ON document_versions(document_id);

-- ============================================
-- BANK INTEGRATION
-- ============================================

CREATE TABLE bank_connections (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    bank_name NVARCHAR(200) NOT NULL,
    account_number NVARCHAR(50) NOT NULL,
    account_name NVARCHAR(200) NOT NULL,
    bank_code NVARCHAR(20),
    branch_id NVARCHAR(36),
    credentials NVARCHAR(MAX),
    status NVARCHAR(20) DEFAULT 'INACTIVE',
    last_tested_at DATETIME2,
    metadata NVARCHAR(MAX),
    created_by NVARCHAR(36) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (branch_id) REFERENCES branches(id)
);

CREATE INDEX idx_bank_connections_branch_id ON bank_connections(branch_id);

CREATE TABLE bank_transactions (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    bank_connection_id NVARCHAR(36) NOT NULL,
    transaction_date DATETIME2 NOT NULL,
    description NVARCHAR(500) NOT NULL,
    reference NVARCHAR(100) NOT NULL,
    debit DECIMAL(15, 2) DEFAULT 0,
    credit DECIMAL(15, 2) DEFAULT 0,
    balance DECIMAL(15, 2) NOT NULL,
    status NVARCHAR(20) DEFAULT 'UNMATCHED',
    matched_transaction_id NVARCHAR(36),
    matched_at DATETIME2,
    matched_by NVARCHAR(36),
    created_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (bank_connection_id) REFERENCES bank_connections(id) ON DELETE CASCADE
);

CREATE INDEX idx_bank_transactions_bank_connection_id ON bank_transactions(bank_connection_id);
CREATE INDEX idx_bank_transactions_transaction_date ON bank_transactions(transaction_date);
CREATE INDEX idx_bank_transactions_status ON bank_transactions(status);

-- ============================================
-- APPROVALS
-- ============================================

CREATE TABLE approval_requests (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    transaction_id NVARCHAR(36),
    requested_by NVARCHAR(36) NOT NULL,
    status NVARCHAR(20) DEFAULT 'PENDING',
    approval_level INT NOT NULL,
    required_approvers INT NOT NULL,
    approver_roles NVARCHAR(MAX),
    reason NVARCHAR(1000),
    completed_at DATETIME2,
    metadata NVARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

CREATE INDEX idx_approval_requests_status ON approval_requests(status);
CREATE INDEX idx_approval_requests_transaction_id ON approval_requests(transaction_id);

CREATE TABLE approvals (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    approval_request_id NVARCHAR(36) NOT NULL,
    approved_by NVARCHAR(36) NOT NULL,
    decision NVARCHAR(20) NOT NULL, -- APPROVED, REJECTED
    comment NVARCHAR(1000),
    approved_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (approval_request_id) REFERENCES approval_requests(id) ON DELETE CASCADE,
    FOREIGN KEY (approved_by) REFERENCES users(id)
);

CREATE INDEX idx_approvals_approval_request_id ON approvals(approval_request_id);
CREATE INDEX idx_approvals_approved_by ON approvals(approved_by);

-- ============================================
-- AUDIT & LOGS
-- ============================================

CREATE TABLE audit_logs (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    user_id NVARCHAR(36) NOT NULL,
    action NVARCHAR(100) NOT NULL,
    entity_type NVARCHAR(100) NOT NULL,
    entity_id NVARCHAR(36) NOT NULL,
    changes NVARCHAR(MAX),
    ip_address NVARCHAR(50) NOT NULL,
    user_agent NVARCHAR(500) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE()
);

CREATE INDEX idx_audit_logs_user_id_created_at ON audit_logs(user_id, created_at);
CREATE INDEX idx_audit_logs_entity ON audit_logs(entity_type, entity_id);
CREATE INDEX idx_audit_logs_action ON audit_logs(action);

CREATE TABLE system_logs (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    level NVARCHAR(20) NOT NULL, -- INFO, WARN, ERROR
    message NVARCHAR(MAX) NOT NULL,
    context NVARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETDATE()
);

CREATE INDEX idx_system_logs_level ON system_logs(level);
CREATE INDEX idx_system_logs_created_at ON system_logs(created_at);

-- ============================================
-- REGULATORY REPORTING
-- ============================================

CREATE TABLE regulatory_reports (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    report_type NVARCHAR(50) NOT NULL,
    report_period_start DATETIME2 NOT NULL,
    report_period_end DATETIME2 NOT NULL,
    fiscal_year INT NOT NULL,
    status NVARCHAR(20) DEFAULT 'DRAFT',
    data NVARCHAR(MAX),
    file_url NVARCHAR(500),
    submission_date DATETIME2,
    submission_reference NVARCHAR(100),
    created_by NVARCHAR(36) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (created_by) REFERENCES users(id)
);

CREATE INDEX idx_regulatory_reports_type ON regulatory_reports(report_type);
CREATE INDEX idx_regulatory_reports_period ON regulatory_reports(report_period_start, report_period_end);
CREATE INDEX idx_regulatory_reports_status ON regulatory_reports(status);
CREATE INDEX idx_regulatory_reports_fiscal_year ON regulatory_reports(fiscal_year);

CREATE TABLE compliance_checklists (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    title NVARCHAR(200) NOT NULL,
    description NVARCHAR(MAX),
    category NVARCHAR(50) NOT NULL,
    frequency NVARCHAR(20) NOT NULL,
    due_date DATETIME2 NOT NULL,
    status NVARCHAR(20) DEFAULT 'PENDING',
    priority NVARCHAR(20) DEFAULT 'MEDIUM',
    responsible_person NVARCHAR(36),
    completed_at DATETIME2,
    completed_by NVARCHAR(36),
    notes NVARCHAR(MAX),
    metadata NVARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (responsible_person) REFERENCES users(id),
    FOREIGN KEY (completed_by) REFERENCES users(id)
);

CREATE INDEX idx_compliance_checklists_category ON compliance_checklists(category);
CREATE INDEX idx_compliance_checklists_status ON compliance_checklists(status);
CREATE INDEX idx_compliance_checklists_due_date ON compliance_checklists(due_date);
CREATE INDEX idx_compliance_checklists_priority ON compliance_checklists(priority);

CREATE TABLE regulatory_alerts (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    alert_type NVARCHAR(50) NOT NULL,
    severity NVARCHAR(20) NOT NULL,
    title NVARCHAR(200) NOT NULL,
    message NVARCHAR(MAX) NOT NULL,
    threshold_value DECIMAL(15, 2),
    current_value DECIMAL(15, 2),
    entity_type NVARCHAR(50),
    entity_id NVARCHAR(36),
    is_acknowledged BIT DEFAULT 0,
    acknowledged_by NVARCHAR(36),
    acknowledged_at DATETIME2,
    resolution_notes NVARCHAR(MAX),
    resolved_at DATETIME2,
    metadata NVARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (acknowledged_by) REFERENCES users(id)
);

CREATE INDEX idx_regulatory_alerts_type ON regulatory_alerts(alert_type);
CREATE INDEX idx_regulatory_alerts_severity ON regulatory_alerts(severity);
CREATE INDEX idx_regulatory_alerts_acknowledged ON regulatory_alerts(is_acknowledged);
CREATE INDEX idx_regulatory_alerts_created_at ON regulatory_alerts(created_at);

CREATE TABLE tax_calculations (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    tax_type NVARCHAR(50) NOT NULL,
    period_start DATETIME2 NOT NULL,
    period_end DATETIME2 NOT NULL,
    taxable_amount DECIMAL(15, 2) NOT NULL,
    tax_rate DECIMAL(5, 4) NOT NULL,
    tax_amount DECIMAL(15, 2) NOT NULL,
    transaction_id NVARCHAR(36),
    entity_type NVARCHAR(50),
    entity_id NVARCHAR(36),
    status NVARCHAR(20) DEFAULT 'CALCULATED',
    payment_date DATETIME2,
    payment_reference NVARCHAR(100),
    metadata NVARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

CREATE INDEX idx_tax_calculations_type ON tax_calculations(tax_type);
CREATE INDEX idx_tax_calculations_period ON tax_calculations(period_start, period_end);
CREATE INDEX idx_tax_calculations_status ON tax_calculations(status);

CREATE TABLE ecl_provisions (
    id NVARCHAR(36) PRIMARY KEY DEFAULT NEWID(),
    loan_id NVARCHAR(36) NOT NULL,
    assessment_date DATETIME2 NOT NULL,
    stage INT NOT NULL,
    probability_of_default DECIMAL(5, 4) NOT NULL,
    loss_given_default DECIMAL(5, 4) NOT NULL,
    exposure_at_default DECIMAL(15, 2) NOT NULL,
    expected_credit_loss DECIMAL(15, 2) NOT NULL,
    provision_amount DECIMAL(15, 2) NOT NULL,
    days_past_due INT DEFAULT 0,
    credit_rating NVARCHAR(10),
    significant_increase_in_risk BIT DEFAULT 0,
    notes NVARCHAR(MAX),
    metadata NVARCHAR(MAX),
    created_by NVARCHAR(36) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (loan_id) REFERENCES loans(id),
    FOREIGN KEY (created_by) REFERENCES users(id)
);

CREATE INDEX idx_ecl_provisions_loan_id ON ecl_provisions(loan_id);
CREATE INDEX idx_ecl_provisions_assessment_date ON ecl_provisions(assessment_date);
CREATE INDEX idx_ecl_provisions_stage ON ecl_provisions(stage);

PRINT 'All tables created successfully';
GO
