# Cooperative Loan Management System - API Setup Guide

## Overview
Complete setup guide for the Cooperative Loan Management System REST API.

---

## Prerequisites

- .NET 8.0 SDK or later
- SQL Server 2019 or later
- Visual Studio 2022 or VS Code
- Postman (for testing)

---

## Installation Steps

### 1. Clone and Restore Packages

```bash
cd Fin-Backend
dotnet restore
```

### 2. Configure Database Connection

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CooperativeLoanDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-characters-long",
    "Issuer": "CooperativeLoanAPI",
    "Audience": "CooperativeLoanClients",
    "ExpiryMinutes": 60
  }
}
```

### 3. Run Database Migrations

```bash
# Apply the cooperative loan management migration
sqlcmd -S localhost -d CooperativeLoanDB -i Migrations/CompleteCleanupCooperativeLoanManagement.sql
```

### 4. Update Program.cs

Replace your `Program.cs` with the content from `Program.cs.example`:

```bash
copy Program.cs.example Program.cs
```

### 5. Build the Project

```bash
dotnet build
```

### 6. Run the Application

```bash
dotnet run
```

The API will start at:
- **HTTPS**: `https://localhost:5001`
- **HTTP**: `http://localhost:5000`
- **Swagger UI**: `https://localhost:5001/api-docs`

---

## Dependency Injection Setup

All services are automatically registered in `ServiceCollectionExtensions.cs`:

### Registered Services:

**Core Services:**
- `ILoanCalculatorService` - EMI calculations, amortization
- `ILoanEligibilityService` - Eligibility checking
- `IGuarantorService` - Guarantor management
- `ILoanCommitteeService` - Committee workflow
- `ILoanRegisterService` - Loan registration
- `IMonthlyThresholdService` - Threshold management
- `ILoanDisbursementService` - Loan disbursement
- `ILoanRepaymentService` - Repayment processing
- `IDelinquencyManagementService` - Delinquency detection

**Infrastructure:**
- `IRepository<T>` - Generic repository
- `IUnitOfWork` - Unit of work pattern
- `ModularApplicationDbContext` - EF Core context

---

## Swagger/OpenAPI Configuration

### Access Swagger UI

Navigate to: `https://localhost:5001/api-docs`

### Features:
- ✅ Interactive API documentation
- ✅ Try-it-out functionality
- ✅ JWT authentication support
- ✅ Request/response examples
- ✅ Model schemas
- ✅ Grouped by controller

### Using JWT in Swagger:

1. Click the **Authorize** button (top right)
2. Enter: `Bearer your-jwt-token-here`
3. Click **Authorize**
4. All subsequent requests will include the token

---

## Testing the API

### 1. Health Check

```bash
curl https://localhost:5001/health
```

### 2. Get API Info

```bash
curl https://localhost:5001/
```

### 3. Check Loan Eligibility

```bash
curl -X POST https://localhost:5001/api/LoanApplication/check-eligibility \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "requestedAmount": 100000,
    "loanType": "NORMAL",
    "interestRate": 15,
    "tenorMonths": 12,
    "memberId": "member-123",
    "memberTotalSavings": 50000,
    "membershipDate": "2020-01-01"
  }'
```

### 4. Calculate Loan

```bash
curl -X POST https://localhost:5001/api/LoanApplication/calculate \
  -H "Content-Type: application/json" \
  -d '{
    "principal": 100000,
    "interestRate": 15,
    "tenorMonths": 12
  }'
```

---

## Authentication Setup

### Generate JWT Token (Example)

```csharp
// In your AuthController
var token = GenerateJwtToken(user);

private string GenerateJwtToken(User user)
{
    var securityKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]));
    
    var credentials = new SigningCredentials(
        securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

    var token = new JwtSecurityToken(
        issuer: Configuration["Jwt:Issuer"],
        audience: Configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.Now.AddMinutes(60),
        signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

---

## Role-Based Access Control

### Available Roles:
- **Member** - Basic member access
- **Committee** - Loan review and approval
- **Finance** - Disbursement and repayment
- **Collections** - Delinquency management
- **Admin** - Administrative functions
- **SuperAdmin** - Full system access
- **System** - Scheduled jobs

### Example Usage:

```csharp
[Authorize(Roles = "Finance,Admin")]
public async Task<IActionResult> DisburseLoan([FromBody] DisbursementRequest request)
{
    // Only Finance and Admin roles can access
}
```

---

## Background Jobs (Hangfire)

### Setup Hangfire:

1. Uncomment Hangfire configuration in `ServiceCollectionExtensions.cs`
2. Uncomment Hangfire dashboard in `Program.cs`
3. Install package:

```bash
dotnet add package Hangfire.AspNetCore
dotnet add package Hangfire.SqlServer
```

### Schedule Jobs:

```csharp
// Daily delinquency check at 1 AM
RecurringJob.AddOrUpdate<IDelinquencyManagementService>(
    "daily-delinquency-check",
    service => service.PerformDailyDelinquencyCheckAsync(),
    "0 1 * * *");

// Monthly rollover on 1st of each month at 2 AM
RecurringJob.AddOrUpdate<IMonthlyThresholdService>(
    "monthly-rollover",
    service => service.PerformMonthlyRolloverAsync(),
    "0 2 1 * *");
```

### Access Hangfire Dashboard:

Navigate to: `https://localhost:5001/hangfire`

---

## Logging

### Serilog Configuration

Logs are written to:
- **Console** - Real-time logging
- **File** - `logs/cooperative-loan-YYYYMMDD.txt`

### Log Levels:
- **Information** - General application flow
- **Warning** - Unexpected events
- **Error** - Errors and exceptions
- **Fatal** - Critical failures

### Example Log Output:

```
[2024-11-30 10:30:45 INF] Processing repayment of ₦10,000.00 for loan LOAN-123
[2024-11-30 10:30:46 INF] Repayment processed successfully. Transaction ID: TXN-456
```

---

## Performance Optimization

### Caching

Enable Redis caching:

```bash
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

Uncomment Redis configuration in `ServiceCollectionExtensions.cs`.

### Database Indexing

All critical indexes are included in the migration scripts:
- Member lookups
- Loan serial numbers
- Application status
- Payment dates

---

## Deployment

### Azure App Service

```bash
# Publish
dotnet publish -c Release -o ./publish

# Deploy to Azure
az webapp deployment source config-zip \
  --resource-group YourResourceGroup \
  --name YourAppName \
  --src publish.zip
```

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FinTech.WebAPI.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinTech.WebAPI.dll"]
```

Build and run:

```bash
docker build -t cooperative-loan-api .
docker run -p 8080:80 cooperative-loan-api
```

---

## Troubleshooting

### Common Issues:

**1. Database Connection Failed**
- Check SQL Server is running
- Verify connection string
- Ensure database exists

**2. JWT Authentication Failed**
- Check JWT secret key length (min 32 characters)
- Verify token format: `Bearer <token>`
- Check token expiry

**3. Swagger Not Loading**
- Ensure XML documentation is enabled in project file
- Check Swagger route: `/api-docs`
- Verify middleware order in Program.cs

**4. Service Not Found (DI Error)**
- Check service registration in `ServiceCollectionExtensions.cs`
- Verify interface and implementation names match
- Ensure correct lifetime (Scoped/Singleton/Transient)

---

## API Versioning

Future versions can be added:

```csharp
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class LoanApplicationController : ControllerBase
{
    // v1 and v2 endpoints
}
```

---

## Support

For technical support:
- **Email**: api-support@yourdomain.com
- **Documentation**: https://docs.yourdomain.com
- **GitHub Issues**: https://github.com/yourorg/cooperative-loan

---

## License

Proprietary - All Rights Reserved

**Version**: 1.0  
**Last Updated**: 2024-11-30
