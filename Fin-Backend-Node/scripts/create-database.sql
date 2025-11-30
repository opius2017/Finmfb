-- ============================================
-- Create Database
-- ============================================
USE master;
GO

-- Drop database if exists
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'SoarMFBDb')
BEGIN
    ALTER DATABASE SoarMFBDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE SoarMFBDb;
END
GO

-- Create database
CREATE DATABASE SoarMFBDb;
GO

USE SoarMFBDb;
GO

PRINT 'Database SoarMFBDb created successfully';
GO
