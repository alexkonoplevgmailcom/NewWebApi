# PopulateData.ps1
# This script populates sample data for the BFB Template API by calling its controllers

# Configuration
$apiBaseUrl = "https://localhost:7107" # Update this with your API's base URL
$headers = @{ "Content-Type" = "application/json" }

Write-Host "Starting data population for BFB Template API..." -ForegroundColor Cyan

# Test API connection
try {
    $testResponse = Invoke-RestMethod -Uri "$apiBaseUrl/api/BankAccounts" -Method Get -ErrorAction Stop
    Write-Host "Successfully connected to the API" -ForegroundColor Green
}
catch {
    Write-Host "Failed to connect to the API. Please make sure the API is running at $apiBaseUrl" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Check if data already exists to avoid duplicates
$existingBankAccounts = Invoke-RestMethod -Uri "$apiBaseUrl/api/BankAccounts" -Method Get
$existingBankBranches = Invoke-RestMethod -Uri "$apiBaseUrl/api/BankBranches" -Method Get
$existingCustomers = Invoke-RestMethod -Uri "$apiBaseUrl/api/Customers" -Method Get
$existingCustomerAccounts = Invoke-RestMethod -Uri "$apiBaseUrl/api/CustomerAccounts" -Method Get

# 1. Populate Bank Branches (equivalent to MongoDB data in Program.cs)
if ($existingBankBranches.Count -eq 0) {
    Write-Host "Populating Bank Branches..." -ForegroundColor Yellow
    
    $bankBranches = @(
        @{
            BankId = 1
            BranchName = "Downtown Branch"
            Address = "123 Main Street"
            City = "New York"
            State = "NY"
            ZipCode = "10001"
            PhoneNumber = "212-555-1234"
            IsActive = $true
            CreatedDate = (Get-Date).AddYears(-3).ToString("o")
        },
        @{
            BankId = 1
            BranchName = "Uptown Branch"
            Address = "456 Park Avenue"
            City = "New York"
            State = "NY"
            ZipCode = "10022"
            PhoneNumber = "212-555-5678"
            IsActive = $true
            CreatedDate = (Get-Date).AddYears(-2).ToString("o")
        },
        @{
            BankId = 2
            BranchName = "West Side Branch"
            Address = "789 Broadway"
            City = "New York"
            State = "NY"
            ZipCode = "10019"
            PhoneNumber = "212-555-9012"
            IsActive = $true
            CreatedDate = (Get-Date).AddYears(-1).ToString("o")
        }
    )

    foreach ($branch in $bankBranches) {
        try {
            $branchJson = $branch | ConvertTo-Json
            $response = Invoke-RestMethod -Uri "$apiBaseUrl/api/BankBranches" -Method Post -Body $branchJson -Headers $headers
            Write-Host "Created branch: $($response.BranchName)" -ForegroundColor Green
        }
        catch {
            Write-Host "Failed to create branch $($branch.BranchName): $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}
else {
    Write-Host "Bank Branches already exist, skipping population" -ForegroundColor Yellow
}

# 2. Populate Bank Accounts (equivalent to SQL data in Program.cs)
if ($existingBankAccounts.Count -eq 0) {
    Write-Host "Populating Bank Accounts..." -ForegroundColor Yellow
    
    $bankAccounts = @(
        @{
            AccountNumber = "ACC-001"
            OwnerName = "John Doe"
            Balance = 5000.00
            Type = 0  # Checking (AccountType enum)
            CreatedDate = (Get-Date).AddYears(-2).ToString("o")
            IsActive = $true
            BankId = 1
            BranchId = 1
        },
        @{
            AccountNumber = "ACC-002"
            OwnerName = "Jane Smith"
            Balance = 15000.50
            Type = 1  # Savings (AccountType enum)
            CreatedDate = (Get-Date).AddMonths(-6).ToString("o")
            IsActive = $true
            BankId = 1
            BranchId = 2
        }
    )

    foreach ($account in $bankAccounts) {
        try {
            $accountJson = $account | ConvertTo-Json
            $response = Invoke-RestMethod -Uri "$apiBaseUrl/api/BankAccounts" -Method Post -Body $accountJson -Headers $headers
            Write-Host "Created bank account: $($response.AccountNumber)" -ForegroundColor Green
        }
        catch {
            Write-Host "Failed to create bank account $($account.AccountNumber): $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}
else {
    Write-Host "Bank Accounts already exist, skipping population" -ForegroundColor Yellow
}

# 3. Populate Customers (equivalent to DB2 data in Program.cs)
if ($existingCustomers.Count -eq 0) {
    Write-Host "Populating Customers..." -ForegroundColor Yellow
    
    $customers = @(
        @{
            FirstName = "Robert"
            LastName = "Johnson"
            Email = "robert.johnson@example.com"
            PhoneNumber = "212-555-1111"
            Address = "123 Maple Avenue"
            City = "New York"
            State = "NY"
            ZipCode = "10002"
            CreatedDate = (Get-Date).AddMonths(-8).ToString("o")
            IsActive = $true
        },
        @{
            FirstName = "Lisa"
            LastName = "Williams"
            Email = "lisa.williams@example.com"
            PhoneNumber = "212-555-2222"
            Address = "456 Oak Street"
            City = "New York"
            State = "NY"
            ZipCode = "10003"
            CreatedDate = (Get-Date).AddMonths(-6).ToString("o")
            IsActive = $true
        }
    )

    $createdCustomers = @()

    foreach ($customer in $customers) {
        try {
            $customerJson = $customer | ConvertTo-Json
            $response = Invoke-RestMethod -Uri "$apiBaseUrl/api/Customers" -Method Post -Body $customerJson -Headers $headers
            Write-Host "Created customer: $($response.FirstName) $($response.LastName)" -ForegroundColor Green
            $createdCustomers += $response
        }
        catch {
            Write-Host "Failed to create customer $($customer.FirstName) $($customer.LastName): $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    # 4. Populate Customer Accounts (equivalent to DB2 account data in Program.cs)
    if ($createdCustomers.Count -gt 0) {
        Write-Host "Populating Customer Accounts..." -ForegroundColor Yellow
        
        $customer1Id = $createdCustomers[0].Id
        $customer2Id = $createdCustomers[1].Id

        $customerAccounts = @(
            @{
                CustomerId = $customer1Id
                AccountNumber = "DB2-001"
                Balance = 7500.00
                Type = 0  # Checking (AccountType enum)
                CreatedDate = (Get-Date).AddMonths(-7).ToString("o")
                IsActive = $true
            },
            @{
                CustomerId = $customer1Id
                AccountNumber = "DB2-002"
                Balance = 25000.50
                Type = 1  # Savings (AccountType enum)
                CreatedDate = (Get-Date).AddMonths(-7).ToString("o")
                IsActive = $true
            },
            @{
                CustomerId = $customer2Id
                AccountNumber = "DB2-003"
                Balance = 12750.25
                Type = 2  # Investment (AccountType enum)
                CreatedDate = (Get-Date).AddMonths(-5).ToString("o")
                IsActive = $true
            }
        )

        foreach ($account in $customerAccounts) {
            try {
                $accountJson = $account | ConvertTo-Json
                $response = Invoke-RestMethod -Uri "$apiBaseUrl/api/CustomerAccounts" -Method Post -Body $accountJson -Headers $headers
                Write-Host "Created customer account: $($response.AccountNumber)" -ForegroundColor Green
            }
            catch {
                Write-Host "Failed to create customer account $($account.AccountNumber): $($_.Exception.Message)" -ForegroundColor Red
            }
        }
    }
}
else {
    Write-Host "Customers already exist, skipping population" -ForegroundColor Yellow
    
    # Check if we need to populate customer accounts
    if ($existingCustomerAccounts.Count -eq 0) {
        Write-Host "Customers exist but no customer accounts found. Getting existing customers to create accounts..." -ForegroundColor Yellow
        $existingCustomers = Invoke-RestMethod -Uri "$apiBaseUrl/api/Customers" -Method Get
        
        if ($existingCustomers.Count -ge 2) {
            $customer1Id = $existingCustomers[0].Id
            $customer2Id = $existingCustomers[1].Id
            
            $customerAccounts = @(
                @{
                    CustomerId = $customer1Id
                    AccountNumber = "DB2-001"
                    Balance = 7500.00
                    Type = 0  # Checking (AccountType enum)
                    CreatedDate = (Get-Date).AddMonths(-7).ToString("o")
                    IsActive = $true
                },
                @{
                    CustomerId = $customer1Id
                    AccountNumber = "DB2-002"
                    Balance = 25000.50
                    Type = 1  # Savings (AccountType enum)
                    CreatedDate = (Get-Date).AddMonths(-7).ToString("o")
                    IsActive = $true
                },
                @{
                    CustomerId = $customer2Id
                    AccountNumber = "DB2-003"
                    Balance = 12750.25
                    Type = 2  # Investment (AccountType enum)
                    CreatedDate = (Get-Date).AddMonths(-5).ToString("o")
                    IsActive = $true
                }
            )

            foreach ($account in $customerAccounts) {
                try {
                    $accountJson = $account | ConvertTo-Json
                    $response = Invoke-RestMethod -Uri "$apiBaseUrl/api/CustomerAccounts" -Method Post -Body $accountJson -Headers $headers
                    Write-Host "Created customer account: $($response.AccountNumber)" -ForegroundColor Green
                }
                catch {
                    Write-Host "Failed to create customer account $($account.AccountNumber): $($_.Exception.Message)" -ForegroundColor Red
                }
            }
        }
        else {
            Write-Host "Not enough existing customers to create accounts" -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "Customer Accounts already exist, skipping population" -ForegroundColor Yellow
    }
}

Write-Host "Data population complete!" -ForegroundColor Cyan