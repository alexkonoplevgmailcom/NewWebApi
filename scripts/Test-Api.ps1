# Test-Api.ps1
# This script runs a series of tests against the BankAccounts API endpoints
# to validate functionality and caching behavior
#
# Usage: .\scripts\Test-Api.ps1 [-ApiUrl <string>] [-WaitTime <int>]

param (
    [string]$ApiUrl = "http://localhost:5052",
    [int]$WaitTime = 1  # Seconds to wait between tests
)

# Configuration
$endpoint = "/api/BankAccounts"
$headers = @{ "Content-Type" = "application/json" }

Write-Host "=== Starting BankAccounts API Tests ===" -ForegroundColor Cyan
Write-Host "Using API URL: $ApiUrl" -ForegroundColor Cyan
Write-Host ""

# Function to handle REST API calls with consistent error handling
function Invoke-ApiRequest {
    param (
        [string]$Uri,
        [string]$Method = "Get",
        [object]$Body = $null,
        [string]$TestName
    )
    
    Write-Host "=== $TestName ===" -ForegroundColor Yellow
    
    try {
        $params = @{
            Uri = $Uri
            Method = $Method
            Headers = $headers
            ErrorAction = "Stop"
        }
        
        if ($Body) {
            $params.Body = $Body
        }
        
        $response = Invoke-RestMethod @params
        
        if ($Method -ne "Delete") {
            $response | ConvertTo-Json -Depth 4
        } else {
            Write-Host "Delete successful" -ForegroundColor Green
        }
        
        return $response
    } catch {
        Write-Host "Error ($($_.Exception.Response.StatusCode.value__)): $($_.Exception.Message)" -ForegroundColor Red
        return $null
    } finally {
        Start-Sleep -Seconds $WaitTime
    }
}

# Test 1: GET all bank accounts (first call - should be a cache miss)
Invoke-ApiRequest -Uri "$ApiUrl$endpoint" -TestName "Test 1: GET all bank accounts (first call - should be a cache miss)"

# Test 2: GET all bank accounts again (should be a cache hit)
Invoke-ApiRequest -Uri "$ApiUrl$endpoint" -TestName "Test 2: GET all bank accounts again (should be a cache hit)"

# Test 3: GET specific bank account with ID 1 (should be a cache miss)
Invoke-ApiRequest -Uri "$ApiUrl$endpoint/1" -TestName "Test 3: GET specific bank account with ID 1 (should be a cache miss)"

# Test 4: GET same bank account again (should be a cache hit)
Invoke-ApiRequest -Uri "$ApiUrl$endpoint/1" -TestName "Test 4: GET same bank account again (should be a cache hit)"

# Test 5: CREATE a new bank account (should invalidate the list cache)
$newAccount = @{
    accountNumber = "ACC-003"
    ownerName = "Michael Johnson"
    balance = 7500.25
    type = 2  # Investment (AccountType enum)
    isActive = $true
    bankId = 1
    branchId = 3
}
$newAccountJson = $newAccount | ConvertTo-Json
$newAccountResponse = Invoke-ApiRequest -Uri "$ApiUrl$endpoint" -Method "Post" -Body $newAccountJson -TestName "Test 5: CREATE a new bank account (should invalidate the list cache)"
$createdId = if ($newAccountResponse) { $newAccountResponse.id } else { 3 } # Default to 3 if creation failed

# Test 6: GET all bank accounts after creation (should be a cache miss)
Invoke-ApiRequest -Uri "$ApiUrl$endpoint" -TestName "Test 6: GET all bank accounts after creation (should be a cache miss)"

# Test 7: UPDATE bank account with ID 2
$updateAccount = @{
    id = 2
    accountNumber = "ACC-002"
    ownerName = "Jane Smith-Brown"
    balance = 20000.00
    type = 1  # Savings (AccountType enum)
    isActive = $true
    bankId = 1
    branchId = 2
}
$updateAccountJson = $updateAccount | ConvertTo-Json
Invoke-ApiRequest -Uri "$ApiUrl$endpoint/2" -Method "Put" -Body $updateAccountJson -TestName "Test 7: UPDATE bank account with ID 2"

# Test 8: GET updated bank account (should be a cache hit for the updated account)
Invoke-ApiRequest -Uri "$ApiUrl$endpoint/2" -TestName "Test 8: GET updated bank account (should be a cache hit for the updated account)"

# Test 9: DELETE bank account with ID from Test 5
Invoke-ApiRequest -Uri "$ApiUrl$endpoint/$createdId" -Method "Delete" -TestName "Test 9: DELETE bank account with ID $createdId"

# Test 10: Verify deletion (should return 404 Not Found)
Invoke-ApiRequest -Uri "$ApiUrl$endpoint/$createdId" -TestName "Test 10: Verify deletion (should return 404 Not Found)"

# Test 11: GET all bank accounts after deletion (should be a cache miss)
Invoke-ApiRequest -Uri "$ApiUrl$endpoint" -TestName "Test 11: GET all bank accounts after deletion (should be a cache miss)"

Write-Host "=== All tests completed ===" -ForegroundColor Cyan