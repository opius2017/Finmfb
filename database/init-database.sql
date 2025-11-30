-- Cooperative Loan Management System - Database Initialization Script
-- Run this script to set up the database for first-time deployment

USE master;
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CooperativeLoanDB')
BEGIN
    CREATE DATABASE CooperativeLoanDB;
    PRINT 'Database CooperativeLoanDB created successfully';
END
ELSE
BEGIN
    PRINT 'Database CooperativeLoanDB already exists';
END
GO

USE CooperativeLoanDB;
GO

-- Create Hangfire database if it doesn't exist
USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CooperativeLoanHangfire')
BEGIN
    CREATE DATABASE CooperativeLoanHangfire;
    PRINT 'Database CooperativeLoanHangfire created successfully';
END
ELSE
BEGIN
    PRINT 'Database CooperativeLoanHangfire already exists';
END
GO

-- Set database options
ALTER DATABASE CooperativeLoanDB SET RECOVERY SIMPLE;
ALTER DATABASE CooperativeLoanDB SET AUTO_SHRINK OFF;
ALTER DATABASE CooperativeLoanDB SET AUTO_CREATE_STATISTICS ON;
ALTER DATABASE CooperativeLoanDB SET AUTO_UPDATE_STATISTICS ON;
GO

PRINT 'Database initialization completed successfully';
PRINT 'Next steps:';
PRINT '1. Run EF Core migrations: dotnet ef database update';
PRINT '2. Seed initial data if needed';
PRINT '3. Configure backup jobs';
GO
