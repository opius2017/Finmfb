# Clean Architecture Implementation Script
# This PowerShell script creates the complete folder structure for Clean Architecture

$basePath = "c:\Users\opius\Desktop\projectFin\Finmfb\Fin-Backend"

# Define folder structure
$folders = @(
    # Application Layer - Behaviors
    "Core\Application\Common\Behaviors",
    
    # Application Layer - Features (CQRS)
    "Core\Application\Features\Loans\Commands\CreateLoan",
    "Core\Application\Features\Loans\Commands\ApproveLoan",
    "Core\Application\Features\Loans\Commands\DisburseLoan",
    "Core\Application\Features\Loans\Commands\RepayLoan",
    "Core\Application\Features\Loans\Queries\GetLoan",
    "Core\Application\Features\Loans\Queries\GetLoans",
    "Core\Application\Features\Loans\Queries\GetLoanSummary",
    
    "Core\Application\Features\Customers\Commands\CreateCustomer",
    "Core\Application\Features\Customers\Commands\UpdateCustomer",
    "Core\Application\Features\Customers\Commands\CompleteKYC",
    "Core\Application\Features\Customers\Queries\GetCustomer",
    "Core\Application\Features\Customers\Queries\GetCustomers",
    
    "Core\Application\Features\Accounts\Commands\CreateAccount",
    "Core\Application\Features\Accounts\Commands\DebitAccount",
    "Core\Application\Features\Accounts\Commands\CreditAccount",
    "Core\Application\Features\Accounts\Queries\GetAccount",
    "Core\Application\Features\Accounts\Queries\GetAccountBalance",
    "Core\Application\Features\Accounts\Queries\GetAccountStatement",
    
    "Core\Application\Features\Deposits\Commands\CreateDeposit",
    "Core\Application\Features\Deposits\Commands\CalculateInterest",
    "Core\Application\Features\Deposits\Queries\GetDeposit",
    "Core\Application\Features\Deposits\Queries\GetDeposits",
    
    "Core\Application\Features\JournalEntries\Commands\CreateJournalEntry",
    "Core\Application\Features\JournalEntries\Commands\PostJournalEntry",
    "Core\Application\Features\JournalEntries\Commands\ReverseJournalEntry",
    "Core\Application\Features\JournalEntries\Queries\GetJournalEntry",
    "Core\Application\Features\JournalEntries\Queries\GetJournalEntries",
    
    # Domain Layer - Value Objects
    "Core\Domain\ValueObjects",
    
    # Domain Layer - Specifications
    "Core\Domain\Specifications\Loans",
    "Core\Domain\Specifications\Customers",
    "Core\Domain\Specifications\Accounts",
    
    # Domain Layer - Domain Services
    "Core\Domain\Services",
    
    # Domain Layer - Domain Events
    "Core\Domain\Events\Loans",
    "Core\Domain\Events\Customers",
    "Core\Domain\Events\Accounts",
    "Core\Domain\Events\Deposits",
    
    # Infrastructure - Interceptors
    "Infrastructure\Data\Interceptors",
    
    # Infrastructure - Outbox Pattern
    "Infrastructure\Messaging\Outbox",
    
    # Infrastructure - Background Jobs
    "Infrastructure\BackgroundServices\Jobs",
    
    # Infrastructure - Middleware
    "Infrastructure\Middleware",
    
    # Infrastructure - Filters
    "Infrastructure\Filters",
    
    # Infrastructure - Health Checks
    "Infrastructure\HealthChecks",
    
    # Presentation - API Versioning
    "Controllers\V1",
    "Controllers\V2"
)

Write-Host "Creating Clean Architecture folder structure..." -ForegroundColor Green

foreach ($folder in $folders) {
    $fullPath = Join-Path $basePath $folder
    if (-not (Test-Path $fullPath)) {
        New-Item -Path $fullPath -ItemType Directory -Force | Out-Null
        Write-Host "Created: $folder" -ForegroundColor Cyan
    } else {
        Write-Host "Exists: $folder" -ForegroundColor Yellow
    }
}

Write-Host "`nFolder structure created successfully!" -ForegroundColor Green
Write-Host "Total folders created: $($folders.Count)" -ForegroundColor Magenta
