# PopulateData.ps1
# This script populates sample data for the New WebAPI by calling the BankAccounts controller
# It creates two bank accounts with different types if they don't already exist in the system
#
# Usage: .\scripts\PopulateData.ps1 [-ApiUrl <string>]

param (
    [string]$ApiUrl = "http://localhost:5052"
)

# Configuration
$endpoint = "/api/BankAccounts"
$headers = @{ "Content-Type" = "application/json" }

Write-Host "=== Starting Data Population for BankAccounts API ===" -ForegroundColor Cyan
Write-Host "Using API URL: $ApiUrl" -ForegroundColor Cyan

# Test API connection
try {
    Write-Host "Testing API connection..." -ForegroundColor Yellow
    $testResponse = Invoke-RestMethod -Uri "$ApiUrl$endpoint" -Method Get -ErrorAction Stop
    Write-Host "Successfully connected to the API" -ForegroundColor Green
}
catch {
    Write-Host "Failed to connect to the API. Please make sure the API is running at $ApiUrl" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Check if data already exists to avoid duplicates
$existingBankAccounts = Invoke-RestMethod -Uri "$ApiUrl$endpoint" -Method Get
$accountCount = if ($existingBankAccounts -is [array]) { $existingBankAccounts.Count } else { if ($existingBankAccounts) { 1 } else { 0 } }

# Populate Bank Accounts 
if ($accountCount -eq 0) {
    Write-Host "Populating Bank Accounts..." -ForegroundColor Yellow
    
    $bankAccounts = @(
        @{
            AccountNumber = "ACC-001"
            OwnerName = "John Doe"
            Balance = 5000.00
            Type = 0  # Checking (AccountType enum)
            IsActive = $true
            BankId = 1
            BranchId = 1
        },
        @{
            AccountNumber = "ACC-002"
            OwnerName = "Jane Smith"
            Balance = 15000.50
            Type = 1  # Savings (AccountType enum)
            IsActive = $true
            BankId = 1
            BranchId = 2
        }
    )

    $successCount = 0
    foreach ($account in $bankAccounts) {
        try {
            $accountJson = $account | ConvertTo-Json
            $response = Invoke-RestMethod -Uri "$ApiUrl$endpoint" -Method Post -Body $accountJson -Headers $headers
            Write-Host "Created bank account: $($response.AccountNumber) (ID: $($response.Id))" -ForegroundColor Green
            $successCount++
        }
        catch {
            Write-Host "Failed to create bank account $($account.AccountNumber): $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    
    Write-Host "Successfully created $successCount out of $($bankAccounts.Count) bank accounts" -ForegroundColor $(if ($successCount -eq $bankAccounts.Count) { "Green" } else { "Yellow" })
}
else {
    Write-Host "Found $accountCount existing bank account(s), skipping population" -ForegroundColor Yellow
}

Write-Host "=== Data Population Complete! ===" -ForegroundColor Cyan