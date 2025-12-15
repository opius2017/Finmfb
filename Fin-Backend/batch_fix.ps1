# Batch fix remaining 27 errors

$files = @{
    "GeneralLedgerService" = "c:/Users/opius/Desktop/projectFin/Finmfb/Fin-Backend/Core/Application/Services/Accounting/GeneralLedgerService.cs"
    "LoanService" = "c:/Users/opius/Desktop/projectFin/Finmfb/Fin-Backend/Core/Application/Services/Loans/LoanService.cs"
    "LoanRepaymentService" = "c:/Users/opius/Desktop/projectFin/Finmfb/Fin-Backend/Core/Application/Services/Loans/LoanRepaymentService.cs"
    "LoanRegisterService" = "c:/Users/opius/Desktop/projectFin/Finmfb/Fin-Backend/Core/Application/Services/Loans/LoanRegisterService.cs"
    "GuarantorService" = "c:/Users/opius/Desktop/projectFin/Finmfb/Fin-Backend/Core/Application/Services/Loans/GuarantorService.cs"
    "RegulatoryMappingService" = "c:/Users/opius/Desktop/projectFin/Finmfb/Fin-Backend/Core/Application/Services/Accounting/RegulatoryMappingService.cs"
    "LoanCollateralService" = "c:/Users/opius/Desktop/projectFin/Finmfb/Fin-Backend/Core/Application/Services/Loans/LoanCollateralService.cs"
    "PeriodClosingService" = "c:/Users/opius/Desktop/projectFin/Finmfb/Fin-Backend/Core/Application/Services/Accounting/PeriodClosingService.cs"
}

# Fix decimal ?? 0 -> decimal ?? 0m
foreach ($key in $files.Keys) {
    $file = $files[$key]
    $content = Get-Content $file -Raw
    $content = $content -replace '(\?\? 0)([^m\d])', '$1m$2'
    $content = $content -replace '(\?\? 0)$', '$1m'
    Set-Content $file -Value $content -NoNewline
}

Write-Host "Fixed decimal ?? 0 issues"

# Fix specific GeneralLedgerService issues
$glFile = $files["GeneralLedgerService"]
$content = Get-Content $glFile -Raw

# Fix line 201: asOfDate.HasValue ? asOfDate.Value : DateTime.UtcNow -> asOfDate ?? DateTime.UtcNow
$content = $content -replace 'asOfDate\.HasValue \? asOfDate\.Value : DateTime\.UtcNow', 'asOfDate ?? DateTime.UtcNow'

# Fix line 214 & 223: Add .ToList()
$content = $content -replace '(return await GetAccountBalancesAsync\([^;]+;)', '$1.ToList();'

Set-Content $glFile -Value $content -NoNewline
Write-Host "Fixed GeneralLedgerService issues"

Write-Host "Batch fixes complete!"
