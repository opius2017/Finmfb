-- ============================================
-- Seed Branches and Members
-- ============================================
USE SoarMFBDb;
GO

PRINT 'Seeding branches and members...';

-- Insert Branches
INSERT INTO branches (id, name, code, address, city, state, country, status) VALUES
(NEWID(), 'Lagos Island Branch', 'LIS001', '15 Marina Road', 'Lagos', 'Lagos', 'Nigeria', 'ACTIVE'),
(NEWID(), 'Ikeja Branch', 'IKJ002', '42 Allen Avenue', 'Ikeja', 'Lagos', 'Nigeria', 'ACTIVE'),
(NEWID(), 'Abuja Central Branch', 'ABJ003', '23 Ahmadu Bello Way', 'Abuja', 'FCT', 'Nigeria', 'ACTIVE'),
(NEWID(), 'Port Harcourt Branch', 'PHC004', '18 Aba Road', 'Port Harcourt', 'Rivers', 'Nigeria', 'ACTIVE'),
(NEWID(), 'Kano Branch', 'KAN005', '7 Murtala Mohammed Way', 'Kano', 'Kano', 'Nigeria', 'ACTIVE');

-- Get branch IDs for reference
DECLARE @lagosId NVARCHAR(36) = (SELECT id FROM branches WHERE code = 'LIS001');
DECLARE @ikejaId NVARCHAR(36) = (SELECT id FROM branches WHERE code = 'IKJ002');
DECLARE @abujaId NVARCHAR(36) = (SELECT id FROM branches WHERE code = 'ABJ003');
DECLARE @phId NVARCHAR(36) = (SELECT id FROM branches WHERE code = 'PHC004');
DECLARE @kanoId NVARCHAR(36) = (SELECT id FROM branches WHERE code = 'KAN005');

-- Insert Members (Lagos Branch)
INSERT INTO members (id, member_number, first_name, last_name, email, phone, date_of_birth, address, city, state, branch_id, status) VALUES
(NEWID(), 'MEM001', 'Oluwaseun', 'Adebayo', 'seun.adebayo@email.com', '08012345678', '1985-03-15', '12 Ikorodu Road', 'Lagos', 'Lagos', @lagosId, 'ACTIVE'),
(NEWID(), 'MEM002', 'Ngozi', 'Okonkwo', 'ngozi.okonkwo@email.com', '08023456789', '1990-07-22', '45 Herbert Macaulay Street', 'Lagos', 'Lagos', @lagosId, 'ACTIVE'),
(NEWID(), 'MEM003', 'Musa', 'Ibrahim', 'musa.ibrahim@email.com', '08034567890', '1988-11-30', '8 Awolowo Road', 'Ikeja', 'Lagos', @ikejaId, 'ACTIVE'),
(NEWID(), 'MEM004', 'Fatima', 'Yusuf', 'fatima.yusuf@email.com', '08045678901', '1992-05-18', '23 Gimbiya Street', 'Abuja', 'FCT', @abujaId, 'ACTIVE'),
(NEWID(), 'MEM005', 'Chinedu', 'Eze', 'chinedu.eze@email.com', '08056789012', '1987-09-25', '15 Aba Road', 'Port Harcourt', 'Rivers', @phId, 'ACTIVE'),
(NEWID(), 'MEM006', 'Aisha', 'Bello', 'aisha.bello@email.com', '08067890123', '1991-12-10', '34 Zoo Road', 'Kano', 'Kano', @kanoId, 'ACTIVE'),
(NEWID(), 'MEM007', 'Tunde', 'Bakare', 'tunde.bakare@email.com', '08078901234', '1986-04-08', '67 Broad Street', 'Lagos', 'Lagos', @lagosId, 'ACTIVE'),
(NEWID(), 'MEM008', 'Amina', 'Suleiman', 'amina.suleiman@email.com', '08089012345', '1993-08-14', '12 Ahmadu Bello Way', 'Abuja', 'FCT', @abujaId, 'ACTIVE'),
(NEWID(), 'MEM009', 'Emeka', 'Nnamdi', 'emeka.nnamdi@email.com', '08090123456', '1989-02-20', '29 Azikiwe Road', 'Port Harcourt', 'Rivers', @phId, 'ACTIVE'),
(NEWID(), 'MEM010', 'Blessing', 'Okoro', 'blessing.okoro@email.com', '08001234567', '1994-06-17', '56 Allen Avenue', 'Ikeja', 'Lagos', @ikejaId, 'ACTIVE'),
(NEWID(), 'MEM011', 'Yusuf', 'Garba', 'yusuf.garba@email.com', '08011234568', '1984-10-05', '18 Ibrahim Taiwo Road', 'Kano', 'Kano', @kanoId, 'ACTIVE'),
(NEWID(), 'MEM012', 'Chioma', 'Nwankwo', 'chioma.nwankwo@email.com', '08021234569', '1995-01-28', '41 Opebi Road', 'Ikeja', 'Lagos', @ikejaId, 'ACTIVE'),
(NEWID(), 'MEM013', 'Ibrahim', 'Musa', 'ibrahim.musa@email.com', '08031234570', '1987-07-12', '9 Shehu Shagari Way', 'Abuja', 'FCT', @abujaId, 'ACTIVE'),
(NEWID(), 'MEM014', 'Funmilayo', 'Adeyemi', 'funmi.adeyemi@email.com', '08041234571', '1991-03-09', '22 Adeola Odeku Street', 'Lagos', 'Lagos', @lagosId, 'ACTIVE'),
(NEWID(), 'MEM015', 'Abdullahi', 'Hassan', 'abdullahi.hassan@email.com', '08051234572', '1988-11-23', '14 Bompai Road', 'Kano', 'Kano', @kanoId, 'ACTIVE'),
(NEWID(), 'MEM016', 'Grace', 'Obi', 'grace.obi@email.com', '08061234573', '1992-09-16', '33 Trans Amadi Road', 'Port Harcourt', 'Rivers', @phId, 'ACTIVE'),
(NEWID(), 'MEM017', 'Mohammed', 'Abubakar', 'mohammed.abubakar@email.com', '08071234574', '1986-05-30', '27 Wuse Zone 4', 'Abuja', 'FCT', @abujaId, 'ACTIVE'),
(NEWID(), 'MEM018', 'Nneka', 'Okafor', 'nneka.okafor@email.com', '08081234575', '1993-12-07', '19 Victoria Island', 'Lagos', 'Lagos', @lagosId, 'ACTIVE'),
(NEWID(), 'MEM019', 'Sani', 'Usman', 'sani.usman@email.com', '08091234576', '1990-08-21', '11 Maiduguri Road', 'Kano', 'Kano', @kanoId, 'ACTIVE'),
(NEWID(), 'MEM020', 'Chiamaka', 'Nnadi', 'chiamaka.nnadi@email.com', '08002234577', '1994-04-14', '38 GRA Phase 2', 'Port Harcourt', 'Rivers', @phId, 'ACTIVE');

PRINT 'Branches and members seeded successfully';
GO
