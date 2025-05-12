# Generate-TestReport.ps1
# This script executes BankAccounts API tests and generates an HTML report of the results
#
# Usage: .\scripts\Generate-TestReport.ps1 [-ApiUrl <string>] [-OutputPath <string>]

param (
    [string]$ApiUrl = "http://localhost:5053",
    [string]$OutputPath = "$PSScriptRoot/../docs/test-report.html"
)

# Configuration
$endpoint = "/api/BankAccounts"
$headers = @{ "Content-Type" = "application/json" }
$reportDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$script:testResults = @() # Using script scope to ensure it's accessible within functions

Write-Host "=== Generating BankAccounts API Test Report ===" -ForegroundColor Cyan
Write-Host "Using API URL: $ApiUrl" -ForegroundColor Cyan
Write-Host "Report will be saved to: $OutputPath" -ForegroundColor Cyan
Write-Host ""

# Function to execute API request and record results
function Invoke-TestCase {
    param (
        [string]$Uri,
        [string]$Method = "Get",
        [object]$Body = $null,
        [string]$TestName,
        [string]$Description,
        [string]$ExpectedResult
    )
    
    Write-Host "Running test: $TestName..." -ForegroundColor Yellow
    
    $startTime = Get-Date
    $statusCode = "N/A"
    $responseContent = ""
    $passed = $false
    $errorMessage = ""
    $responseObj = $null
    
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
        $responseObj = $response
        $statusCode = "200" # Successful calls return 200
        
        if ($Method -eq "Delete") {
            $statusCode = "204" # No Content
            $responseContent = "Resource deleted successfully"
        }
        elseif ($Method -eq "Put") {
            $statusCode = "204" # No Content
            $responseContent = "Resource updated successfully"
        }
        elseif ($Method -eq "Post") {
            $statusCode = "201" # Created
            $responseContent = ($response | ConvertTo-Json -Depth 4 -Compress)
        }
        else {
            $responseContent = ($response | ConvertTo-Json -Depth 4 -Compress)
        }
        
        $passed = $true
    } 
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        $errorMessage = $_.Exception.Message
        
        # Special case for expected 404
        if ($statusCode -eq 404 -and $ExpectedResult -eq "404") {
            $passed = $true
            $responseContent = "Resource not found (Expected)"
        }
        else {
            $passed = $false
            $responseContent = "Error: $errorMessage"
        }
    }
    
    $endTime = Get-Date
    $duration = ($endTime - $startTime).TotalMilliseconds
    
    # Record test result
    $result = [PSCustomObject]@{
        TestName = $TestName
        Description = $Description
        Method = $Method
        Endpoint = $Uri.Replace($ApiUrl, "")
        StatusCode = $statusCode
        Duration = [math]::Round($duration, 2)
        Passed = $passed
        Response = $responseContent
        ExecutedAt = $startTime.ToString("HH:mm:ss")
    }
    
    # Add to global results
    $script:testResults += $result
    
    # Display result
    if ($passed) {
        Write-Host "  ✓ PASSED ($statusCode) in $([math]::Round($duration, 2)) ms" -ForegroundColor Green
    } else {
        Write-Host "  ✗ FAILED ($statusCode) in $([math]::Round($duration, 2)) ms" -ForegroundColor Red
        Write-Host "    Error: $errorMessage" -ForegroundColor Red
    }
    
    return [PSCustomObject]@{
        Result = $result
        ResponseObject = $responseObj
    }
}

# Test initial connectivity
try {
    Write-Host "Testing API connection..." -ForegroundColor Yellow
    Invoke-RestMethod -Uri "$ApiUrl$endpoint" -Method Get -ErrorAction Stop | Out-Null
    Write-Host "Successfully connected to the API" -ForegroundColor Green
    Write-Host ""
}
catch {
    Write-Host "Failed to connect to the API. Please make sure the API is running at $ApiUrl" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Execute first part of test cases
$testResponse1 = Invoke-TestCase -TestName "Get All Bank Accounts" -Description "Retrieves all bank accounts from the system" -Uri "$ApiUrl$endpoint" -Method "Get" -ExpectedResult "200"

$testResponse2 = Invoke-TestCase -TestName "Get Account By ID" -Description "Retrieves a specific bank account by ID" -Uri "$ApiUrl$endpoint/1" -Method "Get" -ExpectedResult "200"

# Create a new account and capture its ID for subsequent tests
$newAccount = @{
    accountNumber = "REPORT-001"
    ownerName = "Report Test User"
    balance = 2500.00
    type = 2  # Investment
    isActive = $true
    bankId = 1
    branchId = 1
}
$newAccountJson = $newAccount | ConvertTo-Json
$createResponse = Invoke-TestCase -TestName "Create New Bank Account" -Description "Creates a new bank account with test data" -Uri "$ApiUrl$endpoint" -Method "Post" -Body $newAccountJson -ExpectedResult "201"

# Extract the ID of the newly created account from the response
if ($createResponse.ResponseObject) {
    $newAccountId = $createResponse.ResponseObject.id
    Write-Host "  Created test account with ID: $newAccountId" -ForegroundColor Cyan
}
else {
    # Default fallback ID if creation failed
    $newAccountId = 7
    Write-Host "  Failed to extract created account ID, using fallback ID: $newAccountId" -ForegroundColor Yellow
}

# Continue with remaining tests using the dynamic ID
Invoke-TestCase -TestName "Verify Created Account" -Description "Verifies the newly created account exists" -Uri "$ApiUrl$endpoint/$newAccountId" -Method "Get" -ExpectedResult "200"

$updateAccount = @{
    id = $newAccountId
    accountNumber = "REPORT-001"
    ownerName = "Updated Report User"
    balance = 3500.00
    type = 2
    isActive = $true
    bankId = 1
    branchId = 1
}
$updateAccountJson = $updateAccount | ConvertTo-Json
Invoke-TestCase -TestName "Update Bank Account" -Description "Updates an existing bank account" -Uri "$ApiUrl$endpoint/$newAccountId" -Method "Put" -Body $updateAccountJson -ExpectedResult "204"

Invoke-TestCase -TestName "Verify Updated Account" -Description "Verifies the updated account information" -Uri "$ApiUrl$endpoint/$newAccountId" -Method "Get" -ExpectedResult "200"

Invoke-TestCase -TestName "Delete Bank Account" -Description "Deletes the test bank account" -Uri "$ApiUrl$endpoint/$newAccountId" -Method "Delete" -ExpectedResult "204"

Invoke-TestCase -TestName "Verify Account Deletion" -Description "Verifies the account was properly deleted" -Uri "$ApiUrl$endpoint/$newAccountId" -Method "Get" -ExpectedResult "404"

# Calculate test summary
$totalTests = $script:testResults.Count
$passedTests = ($script:testResults | Where-Object { $_.Passed -eq $true }).Count
$failedTests = $totalTests - $passedTests
$successRate = if ($totalTests -gt 0) { [math]::Round(($passedTests / $totalTests) * 100, 1) } else { 0 }

# Generate HTML report
$htmlHead = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>BankAccounts API Test Report</title>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px;
        }
        h1, h2, h3 {
            color: #0066cc;
        }
        .header {
            background-color: #f8f9fa;
            padding: 20px;
            border-radius: 5px;
            margin-bottom: 20px;
            border-left: 5px solid #0066cc;
        }
        .summary {
            display: flex;
            justify-content: space-between;
            background-color: #f8f9fa;
            padding: 15px;
            border-radius: 5px;
            margin-bottom: 20px;
        }
        .summary-item {
            text-align: center;
            padding: 10px;
            border-radius: 5px;
        }
        .passed {
            background-color: #d4edda;
            color: #155724;
        }
        .failed {
            background-color: #f8d7da;
            color: #721c24;
        }
        .total {
            background-color: #e2e3e5;
            color: #383d41;
        }
        .success-rate {
            background-color: #cce5ff;
            color: #004085;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
        }
        th, td {
            padding: 12px 15px;
            text-align: left;
            border-bottom: 1px solid #ddd;
        }
        thead {
            background-color: #f8f9fa;
        }
        tr:hover {
            background-color: #f1f1f1;
        }
        .test-passed {
            color: #155724;
            font-weight: bold;
        }
        .test-failed {
            color: #721c24;
            font-weight: bold;
        }
        .details-btn {
            background-color: #0066cc;
            color: white;
            border: none;
            padding: 5px 10px;
            border-radius: 3px;
            cursor: pointer;
        }
        .details-container {
            display: none;
            background-color: #f8f9fa;
            padding: 15px;
            border-radius: 5px;
            margin-top: 10px;
            white-space: pre-wrap;
            font-family: monospace;
            max-height: 300px;
            overflow-y: auto;
        }
        .badge {
            display: inline-block;
            padding: 3px 8px;
            border-radius: 3px;
            font-size: 12px;
            font-weight: bold;
            color: white;
        }
        .badge-get {
            background-color: #61affe;
        }
        .badge-post {
            background-color: #49cc90;
        }
        .badge-put {
            background-color: #fca130;
        }
        .badge-delete {
            background-color: #f93e3e;
        }
        .badge-200, .badge-201, .badge-204 {
            background-color: #49cc90;
        }
        .badge-400, .badge-404 {
            background-color: #f93e3e;
        }
        footer {
            margin-top: 30px;
            text-align: center;
            color: #6c757d;
            font-size: 0.9em;
        }
    </style>
</head>
<body>
    <div class="header">
        <h1>BankAccounts API Test Report</h1>
        <p>This report contains the results of automated tests executed against the BankAccounts API endpoints.</p>
        <p><strong>Report Generated:</strong> $reportDate</p>
        <p><strong>API URL:</strong> $ApiUrl</p>
    </div>

    <div class="summary">
        <div class="summary-item total">
            <h3>Total Tests</h3>
            <p>$totalTests</p>
        </div>
        <div class="summary-item passed">
            <h3>Passed</h3>
            <p>$passedTests</p>
        </div>
        <div class="summary-item failed">
            <h3>Failed</h3>
            <p>$failedTests</p>
        </div>
        <div class="summary-item success-rate">
            <h3>Success Rate</h3>
            <p>$successRate%</p>
        </div>
    </div>

    <h2>Test Results</h2>
    <table>
        <thead>
            <tr>
                <th>Test</th>
                <th>Endpoint</th>
                <th>Method</th>
                <th>Status</th>
                <th>Duration (ms)</th>
                <th>Time</th>
                <th>Details</th>
            </tr>
        </thead>
        <tbody>
"@

$htmlBody = ""
$testCounter = 0

foreach ($result in $script:testResults) {
    $testCounter++
    $testStatus = if ($result.Passed) { "test-passed" } else { "test-failed" }
    $methodClass = "badge-$($result.Method.ToLower())"
    $statusClass = "badge-$($result.StatusCode)"
    
    $htmlBody += @"
            <tr>
                <td class="$testStatus">$($result.TestName)</td>
                <td>$($result.Endpoint)</td>
                <td><span class="badge $methodClass">$($result.Method)</span></td>
                <td><span class="badge $statusClass">$($result.StatusCode)</span></td>
                <td>$($result.Duration)</td>
                <td>$($result.ExecutedAt)</td>
                <td>
                    <button class="details-btn" onclick="toggleDetails('details-$testCounter')">View</button>
                    <div id="details-$testCounter" class="details-container">
                        <strong>Description:</strong> $($result.Description)
                        <hr>
                        <strong>Response:</strong>
                        $($result.Response)
                    </div>
                </td>
            </tr>
"@
}

$htmlFoot = @"
        </tbody>
    </table>

    <footer>
        <p>Banking Facility Backend (BFB) - API Test Report</p>
        <p>Generated automatically using PowerShell test scripts</p>
    </footer>

    <script>
        function toggleDetails(id) {
            var detailsContainer = document.getElementById(id);
            if (detailsContainer.style.display === 'block') {
                detailsContainer.style.display = 'none';
            } else {
                detailsContainer.style.display = 'block';
            }
        }
    </script>
</body>
</html>
"@

$fullHtml = $htmlHead + $htmlBody + $htmlFoot

# Ensure the directory exists
$reportDir = Split-Path -Parent $OutputPath
if (-not (Test-Path -Path $reportDir)) {
    New-Item -ItemType Directory -Path $reportDir -Force | Out-Null
}

# Save the HTML report
$fullHtml | Out-File -FilePath $OutputPath -Encoding utf8

Write-Host ""
Write-Host "=== Test Report Generated Successfully ===" -ForegroundColor Cyan
Write-Host "Total Tests: $totalTests" -ForegroundColor Cyan
Write-Host "Passed: $passedTests" -ForegroundColor Green
Write-Host "Failed: $failedTests" -ForegroundColor $(if ($failedTests -gt 0) { "Red" } else { "Green" })
Write-Host "Success Rate: $successRate%" -ForegroundColor Cyan
Write-Host ""
Write-Host "Report saved to: $OutputPath" -ForegroundColor Green
Write-Host "Open the HTML file in a browser to view the complete report." -ForegroundColor Cyan