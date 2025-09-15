Get-ChildItem -Path "src\Core\Domain" -Recurse -Filter "*.cs" | 
ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $content = $content -replace "FinTech\.Core\.Domain\.", "FinTech.Domain."
    $content | Set-Content $_.FullName -NoNewline
}
Get-ChildItem -Path "API", "Application", "Infrastructure" -Recurse -Filter "*.cs" | 
ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $content = $content -replace "FinTech\.Core\.Domain\.", "FinTech.Domain."
    $content | Set-Content $_.FullName -NoNewline
}