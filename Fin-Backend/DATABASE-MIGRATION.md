# Database Migration for Core Accounting Engine

To create and apply the database migrations for the new core accounting engine, follow these steps:

## Step 1: Install the EF Core Tools

If you haven't already installed the Entity Framework Core tools, you can do so with the following command:

```bash
dotnet tool install --global dotnet-ef
```

Or update the tools if already installed:

```bash
dotnet tool update --global dotnet-ef
```

## Step 2: Add a Migration for the Core Accounting Engine

Run the following command from the project root directory:

```bash
dotnet ef migrations add CoreAccountingEngine --project Fin-Backend/FinTech.WebAPI.csproj --context ApplicationDbContext --output-dir Infrastructure/Data/Migrations
```

This will create a new migration that includes all the database changes for the core accounting engine.

## Step 3: Apply the Migration to the Database

Run the following command to apply the migration to the database:

```bash
dotnet ef database update --project Fin-Backend/FinTech.WebAPI.csproj --context ApplicationDbContext
```

## Step 4: Verify the Database Schema

After applying the migration, verify that the following tables have been created in the database:

- `accounting.ChartOfAccounts`
- `accounting.JournalEntries`
- `accounting.JournalEntryLines`
- `accounting.FinancialPeriods`
- `accounting.FiscalYears`

## Step 5: Seed Initial Data (Optional)

You may want to seed initial data for testing or configuration purposes:

```csharp
// Example code to seed a fiscal year with financial periods
public static async Task SeedAccountingData(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var fiscalYearService = scope.ServiceProvider.GetRequiredService<IFiscalYearService>();
    
    // Create a fiscal year for the current year with monthly periods
    var currentYear = DateTime.UtcNow.Year;
    var startDate = new DateTime(currentYear, 1, 1);
    var endDate = new DateTime(currentYear, 12, 31);
    
    await fiscalYearService.CreateStandardFiscalYearAsync(
        currentYear, 
        startDate, 
        endDate, 
        "System", 
        true);
}
```

This can be added to the `Program.cs` file to be executed after the application has started.