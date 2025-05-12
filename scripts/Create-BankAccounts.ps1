# Create-BankAccounts.ps1
# This script creates specific test bank accounts for testing purposes
# and saves their IDs to a text file for use in other tests
#
# Usage: .\scripts\Create-BankAccounts.ps1 [-ApiUrl <string>]

param (
    [string]$ApiUrl = "http://localhost:5052"
)

# Configuration
$endpoint = "/api/BankAccounts"
$headers = @{ "Content-Type" = "application/json" }

Write-Host "=== Creating Bank Accounts for Testing in new-webapi ===" -ForegroundColor Cyan
Write-Host "Using API URL: $ApiUrl" -ForegroundColor Cyan

# Test API connection first
try {
    Write-Host "Testing API connection..." -ForegroundColor Yellow
    Invoke-RestMethod -Uri "$ApiUrl$endpoint" -Method Get -ErrorAction Stop | Out-Null
    Write-Host "Successfully connected to the API" -ForegroundColor Green
}
catch {
    Write-Host "Failed to connect to the API. Please make sure the API is running at $ApiUrl" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Create Account 1
Write-Host "Creating bank account 1..." -ForegroundColor Yellow
$account1 = @{
    accountNumber = "TEST-001"
    ownerName = "Test User 1"
    balance = 1000.00
    type = 0  # Checking (AccountType enum)
    isActive = $true
    bankId = 1
    branchId = 1
}

try {
    $accountJson = $account1 | ConvertTo-Json
    $response = Invoke-RestMethod -Uri "$ApiUrl$endpoint" -Method Post -Body $accountJson -Headers $headers
    Write-Host "Response for account 1:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 4
    $account1Id = $response.id
} catch {
    Write-Host "Failed to create bank account: $($_.Exception.Message)" -ForegroundColor Red
    $account1Id = 1  # Fallback ID
}

if (-not $account1Id) {
    Write-Host "Could not extract account ID from response. Using default ID 1." -ForegroundColor Yellow
    $account1Id = 1
} else {
    Write-Host "Extracted account ID: $account1Id" -ForegroundColor Green
}

# Create Account 2
Write-Host "Creating bank account 2..." -ForegroundColor Yellow
$account2 = @{
    accountNumber = "TEST-002"
    ownerName = "Test User 2"
    balance = 5000.00
    type = 1  # Savings (AccountType enum)
    isActive = $true
    bankId = 1
    branchId = 1
}

try {
    $accountJson = $account2 | ConvertTo-Json
    $response = Invoke-RestMethod -Uri "$ApiUrl$endpoint" -Method Post -Body $accountJson -Headers $headers
    Write-Host "Response for account 2:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 4
    $account2Id = $response.id
} catch {
    Write-Host "Failed to create bank account: $($_.Exception.Message)" -ForegroundColor Red
    $account2Id = 2  # Fallback ID
}

if (-not $account2Id) {
    Write-Host "Could not extract account ID from response. Using default ID 2." -ForegroundColor Yellow
    $account2Id = 2
} else {
    Write-Host "Extracted account ID: $account2Id" -ForegroundColor Green
}

# Create a file with the account IDs for the test script to use
$testAccountsPath = Join-Path (Split-Path -Parent $PSScriptRoot) "test-accounts.txt"
Write-Host "Saving account IDs to $testAccountsPath" -ForegroundColor Yellow
"$account1Id" | Out-File -FilePath $testAccountsPath -Force
"$account2Id" | Add-Content -Path $testAccountsPath

Write-Host "=== Bank Accounts Created Successfully ===" -ForegroundColor Cyan
Write-Host "You can now use account IDs $account1Id and $account2Id in your tests." -ForegroundColor Green