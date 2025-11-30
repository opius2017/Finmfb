# Cooperative Loan Management System - Deployment Guide

## Prerequisites

### Required Software
- .NET 8.0 SDK or later
- SQL Server 2019 or later (or Azure SQL Database)
- Redis (optional, for distributed caching)
- IIS or Azure App Service (for hosting)

### Development Tools
- Visual Studio 2022 or VS Code
- SQL Server Management Studio
- Postman (for API testing)

---

## Configuration

### 1. Database Setup

**Update Connection String in `appsettings.json`:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=CooperativeLoanDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
  }
}
```

**Run Database Migration:**
```bash
# Navigate to project directory
cd Fin-Backend

# Run the SQL migration script
sqlcmd -S YOUR_SERVER -d CooperativeLoanDB -i Migrations/CompleteCleanupCooperativeLoanManagement.sql
```

### 2. JWT Configuration

**Update JWT Settings in `appsettings.json`:**
```json
{
  "JwtSettings": {
    "SecretKey": "GENERATE_A_SECURE_KEY_HERE_MINIMUM_32_CHARACTERS",
    "Issuer": "YourCooperativeName",
    "Audience": "YourCooperativeUsers",
    "ExpirationMinutes": 60
  }
}
```

**Generate Secure Key:**
```powershell
# PowerShell command to generate secure key
$bytes = New-Object byte[] 32
[Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```

### 3. Notification Services (Optional)

**SMS Provider (Termii):**
```json
{
  "NotificationSettings": {
    "SmsProvider": "Termii",
    "SmsApiKey": "YOUR_TERMII_API_KEY"
  }
}
```

**Email Provider (SendGrid):**
```json
{
  "NotificationSettings": {
    "EmailProvider": "SendGrid",
    "EmailApiKey": "YOUR_SENDGRID_API_KEY"
  }
}
```

### 4. Redis Cache (Optional)

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

---

## Building the Application

### Development Build
```bash
dotnet restore
dotnet build
```

### Production Build
```bash
dotnet publish -c Release -o ./publish
```

---

## Running the Application

### Development
```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger: `https://localhost:5001/api-docs`

### Production (IIS)

1. **Publish the application:**
```bash
dotnet publish -c Release -o C:\inetpub\wwwroot\CooperativeLoanAPI
```

2. **Create IIS Application Pool:**
   - Name: CooperativeLoanAPI
   - .NET CLR Version: No Managed Code
   - Managed Pipeline Mode: Integrated

3. **Create IIS Website:**
   - Site Name: CooperativeLoanAPI
   - Physical Path: C:\inetpub\wwwroot\CooperativeLoanAPI
   - Binding: HTTPS on port 443
   - Application Pool: CooperativeLoanAPI

4. **Install ASP.NET Core Hosting Bundle:**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0

### Production (Azure App Service)

1. **Create Azure App Service:**
```bash
az webapp create --resource-group YourResourceGroup --plan YourAppServicePlan --name cooperative-loan-api --runtime "DOTNET|8.0"
```

2. **Configure Connection Strings:**
```bash
az webapp config connection-string set --resource-group YourResourceGroup --name cooperative-loan-api --connection-string-type SQLAzure --settings DefaultConnection="YOUR_CONNECTION_STRING"
```

3. **Deploy:**
```bash
az webapp deployment source config-zip --resource-group YourResourceGroup --name cooperative-loan-api --src ./publish.zip
```

---

## Database Seeding

### Create Initial Admin User
```sql
INSERT INTO AspNetUsers (Id, UserName, Email, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
VALUES (NEWID(), 'admin@cooperative.com', 'admin@cooperative.com', 1, 0, 0, 0, 0);

-- Add to Admin role
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT Id, (SELECT Id FROM AspNetRoles WHERE Name = 'Admin')
FROM AspNetUsers WHERE UserName = 'admin@cooperative.com';
```

### Create Sample Data (Optional)
```sql
-- Insert sample member
INSERT INTO Members (Id, MemberNumber, FirstName, LastName, Email, PhoneNumber, MembershipDate, MembershipStatus, TotalSavings, ShareCapital, IsActive, CreatedAt)
VALUES (NEWID(), 'MEM001', 'John', 'Doe', 'john.doe@email.com', '08012345678', GETUTCDATE(), 'ACTIVE', 50000, 10000, 1, GETUTCDATE());

-- Set monthly threshold
INSERT INTO MonthlyThresholds (Id, Month, Year, MaxLoanAmount, TotalDisbursed, RemainingAmount, IsActive, CreatedAt, CreatedBy)
VALUES (NEWID(), MONTH(GETUTCDATE()), YEAR(GETUTCDATE()), 3000000, 0, 3000000, 1, GETUTCDATE(), 'SYSTEM');
```

---

## Scheduled Jobs (Hangfire)

### Configure Hangfire Dashboard

Add to `Program.cs`:
```csharp
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});
```

### Schedule Recurring Jobs

```csharp
// Daily delinquency check at 1 AM
RecurringJob.AddOrUpdate<IDelinquencyManagementService>(
    "daily-delinquency-check",
    service => service.PerformDailyDelinquencyCheckAsync(),
    Cron.Daily(1));

// Monthly rollover on 1st of each month at 2 AM
RecurringJob.AddOrUpdate<IMonthlyThresholdService>(
    "monthly-rollover",
    service => service.PerformMonthlyRolloverAsync(),
    Cron.Monthly(1, 2));
```

---

## Monitoring & Logging

### Application Insights (Azure)

Add to `appsettings.json`:
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "YOUR_INSTRUMENTATION_KEY"
  }
}
```

### Log Files

Logs are written to:
- Console (stdout)
- File: `logs/cooperative-loan-system-YYYYMMDD.txt`

### Health Checks

Monitor application health:
- Endpoint: `https://your-domain.com/health`
- Returns: JSON with health status

---

## Security Checklist

- [ ] Change default JWT secret key
- [ ] Enable HTTPS only
- [ ] Configure CORS for production domains
- [ ] Set up firewall rules for database
- [ ] Enable SQL Server encryption
- [ ] Configure rate limiting
- [ ] Set up API key rotation
- [ ] Enable audit logging
- [ ] Configure backup strategy
- [ ] Set up monitoring alerts

---

## Performance Optimization

### Database Indexes
All required indexes are created by the migration script.

### Caching Strategy
- Member data: 15 minutes
- Loan configurations: 1 hour
- Threshold data: 5 minutes

### Connection Pooling
Configured automatically by Entity Framework Core.

---

## Backup Strategy

### Database Backup
```sql
-- Full backup daily
BACKUP DATABASE CooperativeLoanDB
TO DISK = 'C:\Backups\CooperativeLoanDB_Full.bak'
WITH FORMAT, INIT, COMPRESSION;

-- Transaction log backup hourly
BACKUP LOG CooperativeLoanDB
TO DISK = 'C:\Backups\CooperativeLoanDB_Log.trn'
WITH COMPRESSION;
```

### Application Backup
- Backup `appsettings.json` (without secrets)
- Backup custom configurations
- Backup SSL certificates

---

## Troubleshooting

### Common Issues

**1. Database Connection Failed**
- Check connection string
- Verify SQL Server is running
- Check firewall rules
- Verify user permissions

**2. JWT Authentication Failed**
- Verify secret key is configured
- Check token expiration
- Verify issuer and audience match

**3. Swagger Not Loading**
- Check if running in correct environment
- Verify XML documentation is generated
- Check route prefix configuration

**4. Performance Issues**
- Enable Redis caching
- Check database indexes
- Review query performance
- Monitor memory usage

---

## Support

For deployment support:
- Email: devops@yourdomain.com
- Documentation: https://docs.yourdomain.com
- Issue Tracker: https://github.com/yourorg/cooperative-loan-system/issues

---

## Version History

- **v1.0** (2024-11-30): Initial release
  - Complete loan lifecycle management
  - Guarantor and committee workflows
  - Disbursement and repayment processing
  - Delinquency management
  - REST API with Swagger documentation
