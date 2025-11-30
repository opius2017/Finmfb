# Windows Authentication Setup Guide

This guide explains how to set up the FinMFB database using Windows Authentication (no password required).

## Prerequisites

- ✅ SQL Server 2022 installed
- ✅ Windows user account with SQL Server access
- ✅ PowerShell (comes with Windows)
- ✅ Node.js v20+

## Quick Setup

### Step 1: Run the Setup Script

Open PowerShell **as Administrator** and run:

```powershell
cd Fin-Backend-Node
.\scripts\setup-database.ps1
```

The script will:
1. ✅ Connect to SQL Server using your Windows credentials
2. ✅ Create the `FinMFBDb` database
3. ✅ Update your `.env` file with Windows Authentication
4. ✅ Run Prisma migrations
5. ✅ Seed the database with Nigerian sample data

### Step 2: Verify Setup

Check that SQL Server is running:

```powershell
Get-Service -Name 'MSSQL*'
```

You should see services with status "Running".

### Step 3: Start the Application

```bash
npm run dev
```

The server will start at `http://localhost:3000`

## Connection String Format

For Windows Authentication, the connection string looks like:

```
sqlserver://localhost:1433;database=FinMFBDb;integratedSecurity=true;trustServerCertificate=true;encrypt=true
```

Key parameters:
- `integratedSecurity=true` - Uses Windows Authentication
- `trustServerCertificate=true` - Trusts the SQL Server certificate
- `encrypt=true` - Encrypts the connection

## Troubleshooting

### Issue: "Login failed for user"

**Solution**: Ensure your Windows user has SQL Server access.

1. Open SQL Server Management Studio (SSMS)
2. Connect using Windows Authentication
3. Expand Security → Logins
4. Right-click → New Login
5. Click "Search" and add your Windows user
6. Go to "Server Roles" tab
7. Check "sysadmin" (for development)
8. Click OK

### Issue: "Cannot connect to SQL Server"

**Solutions**:

1. **Check SQL Server is running:**
   ```powershell
   Get-Service -Name 'MSSQLSERVER'
   # If stopped, start it:
   Start-Service -Name 'MSSQLSERVER'
   ```

2. **Enable Windows Authentication:**
   - Open SSMS
   - Right-click server → Properties
   - Go to Security page
   - Select "Windows Authentication mode" or "SQL Server and Windows Authentication mode"
   - Restart SQL Server

3. **Enable TCP/IP:**
   - Open SQL Server Configuration Manager
   - Expand "SQL Server Network Configuration"
   - Click "Protocols for MSSQLSERVER"
   - Right-click "TCP/IP" → Enable
   - Restart SQL Server service

### Issue: "Access Denied"

**Solution**: Run PowerShell as Administrator

1. Right-click PowerShell
2. Select "Run as Administrator"
3. Run the setup script again

### Issue: "Prisma Client not found"

**Solution**: Generate Prisma client manually

```bash
npx prisma generate
```

## Manual Database Creation

If the script doesn't work, you can create the database manually:

### Using SSMS (SQL Server Management Studio)

1. Open SSMS
2. Connect using Windows Authentication
3. Right-click "Databases" → New Database
4. Enter name: `FinMFBDb`
5. Click OK

### Using PowerShell

```powershell
# Connect to SQL Server
$connection = New-Object System.Data.SqlClient.SqlConnection
$connection.ConnectionString = "Server=localhost;Integrated Security=True;TrustServerCertificate=True;"
$connection.Open()

# Create database
$command = $connection.CreateCommand()
$command.CommandText = "CREATE DATABASE FinMFBDb COLLATE Latin1_General_100_CI_AS_SC_UTF8;"
$command.ExecuteNonQuery()

$connection.Close()
Write-Host "Database created successfully!"
```

Then run:

```bash
# Update .env file with connection string
# DATABASE_URL="sqlserver://localhost:1433;database=FinMFBDb;integratedSecurity=true;trustServerCertificate=true;encrypt=true"

# Run migrations
npx prisma generate
npx prisma db push

# Seed database
npm run db:seed
```

## Verifying Windows User Access

To check if your Windows user has SQL Server access:

```sql
-- Run in SSMS
SELECT 
    name AS LoginName,
    type_desc AS LoginType,
    is_disabled AS IsDisabled
FROM sys.server_principals
WHERE type IN ('U', 'G')  -- U = Windows User, G = Windows Group
ORDER BY name;
```

Look for your Windows username (format: `DOMAIN\Username` or `COMPUTERNAME\Username`)

## Adding Windows User to SQL Server

If your user is not in SQL Server:

```sql
-- Replace with your Windows username
CREATE LOGIN [COMPUTERNAME\YourUsername] FROM WINDOWS;
GO

-- Grant sysadmin role (for development)
ALTER SERVER ROLE sysadmin ADD MEMBER [COMPUTERNAME\YourUsername];
GO
```

Or use PowerShell:

```powershell
# Get current Windows user
$currentUser = "$env:USERDOMAIN\$env:USERNAME"
Write-Host "Current user: $currentUser"

# Add to SQL Server (run in SSMS or sqlcmd)
# CREATE LOGIN [$currentUser] FROM WINDOWS;
# ALTER SERVER ROLE sysadmin ADD MEMBER [$currentUser];
```

## Environment Variables

Your `.env` file should contain:

```env
# Database with Windows Authentication
DATABASE_URL="sqlserver://localhost:1433;database=FinMFBDb;integratedSecurity=true;trustServerCertificate=true;encrypt=true"

# Other settings...
NODE_ENV=development
PORT=3000
API_VERSION=v1
```

## Testing the Connection

Test your database connection:

```bash
# Using Prisma Studio
npm run db:studio
```

This will open a web interface at `http://localhost:5555` where you can browse your database.

## Security Notes

### Development vs Production

**Development (Windows Authentication):**
- ✅ No password needed
- ✅ Uses your Windows credentials
- ✅ Convenient for local development
- ⚠️ Only works on Windows

**Production (SQL Server Authentication):**
- ✅ Works on any platform
- ✅ Can use managed identities in cloud
- ✅ Better for containerized deployments
- ⚠️ Requires password management

### Best Practices

1. **Development**: Use Windows Authentication
2. **Production**: Use SQL Server Authentication with strong passwords or managed identities
3. **Never commit** `.env` file to version control
4. **Use environment variables** for sensitive data
5. **Rotate passwords** regularly in production
6. **Enable SSL/TLS** for production databases

## Common SQL Server Versions

The connection string works with:
- ✅ SQL Server 2022
- ✅ SQL Server 2019
- ✅ SQL Server 2017
- ✅ SQL Server 2016
- ✅ SQL Server Express (any version)

## Next Steps

After successful setup:

1. **Start the server:**
   ```bash
   npm run dev
   ```

2. **Access API documentation:**
   ```
   http://localhost:3000/api-docs
   ```

3. **Login with default credentials:**
   - Email: `admin@finmfb.ng`
   - Password: `Password123!`

4. **Browse database:**
   ```bash
   npm run db:studio
   ```

## Support

For more help:
- 📖 See `DATABASE_SETUP.md` for detailed database information
- 📖 See `QUICKSTART.md` for quick start guide
- 🔍 Check SQL Server error logs in Event Viewer
- 💬 Contact the development team

## Useful Commands

```powershell
# Check SQL Server status
Get-Service -Name 'MSSQL*'

# Start SQL Server
Start-Service -Name 'MSSQLSERVER'

# Stop SQL Server
Stop-Service -Name 'MSSQLSERVER'

# Restart SQL Server
Restart-Service -Name 'MSSQLSERVER'

# Check SQL Server version
sqlcmd -Q "SELECT @@VERSION"

# List databases
sqlcmd -Q "SELECT name FROM sys.databases"

# Check current user
sqlcmd -Q "SELECT SYSTEM_USER, USER_NAME()"
```

## Success! 🎉

Once setup is complete, you'll have:
- ✅ SQL Server 2022 database
- ✅ Windows Authentication configured
- ✅ Nigerian sample data loaded
- ✅ API server ready to run

Happy coding! 🚀
