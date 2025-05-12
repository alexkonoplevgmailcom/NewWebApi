# Run-All-Tests.ps1
# This script runs all data population and test scripts in the proper sequence
# for comprehensive testing of the BankAccounts API
#
# Usage: .\scripts\Run-All-Tests.ps1 [-ApiUrl <string>] [-SkipPopulate] [-SkipCreateAccounts]

param (
    [string]$ApiUrl = "http://localhost:5052",
    [switch]$SkipPopulate,
    [switch]$SkipCreateAccounts
)

Write-Host "=== BankAccounts API Master Test Script ===" -ForegroundColor Cyan
Write-Host "Using API URL: $ApiUrl" -ForegroundColor Cyan
Write-Host ""

$scriptPath = $PSScriptRoot
$startTime = Get-Date

# Step 1: Run PopulateData.ps1 to ensure we have base data
if (-not $SkipPopulate) {
    Write-Host "Step 1: Populating initial data..." -ForegroundColor Magenta
    & "$scriptPath\PopulateData.ps1" -ApiUrl $ApiUrl
    Write-Host ""
} else {
    Write-Host "Step 1: Skipping initial data population (SkipPopulate flag set)" -ForegroundColor Magenta
    Write-Host ""
}

# Step 2: Create test accounts
if (-not $SkipCreateAccounts) {
    Write-Host "Step 2: Creating test accounts..." -ForegroundColor Magenta
    & "$scriptPath\Create-BankAccounts.ps1" -ApiUrl $ApiUrl
    Write-Host ""
} else {
    Write-Host "Step 2: Skipping test account creation (SkipCreateAccounts flag set)" -ForegroundColor Magenta
    Write-Host ""
}

# Step 3: Run API tests
Write-Host "Step 3: Running API tests..." -ForegroundColor Magenta
& "$scriptPath\Test-Api.ps1" -ApiUrl $ApiUrl
Write-Host ""

$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host "=== All scripts completed successfully! ===" -ForegroundColor Green
Write-Host "Total execution time: $($duration.Minutes) minutes and $($duration.Seconds) seconds" -ForegroundColor Cyan
Write-Host ""
Write-Host "To run individual scripts:" -ForegroundColor Yellow
Write-Host "  * Populate Data:     .\scripts\PopulateData.ps1 -ApiUrl $ApiUrl" -ForegroundColor Yellow
Write-Host "  * Create Test Accts: .\scripts\Create-BankAccounts.ps1 -ApiUrl $ApiUrl" -ForegroundColor Yellow
Write-Host "  * Test API:          .\scripts\Test-Api.ps1 -ApiUrl $ApiUrl" -ForegroundColor Yellow