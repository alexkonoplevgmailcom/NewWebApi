#!/bin/bash

# Script to create bank accounts for testing
API_URL="http://localhost:5053"
BANK_ACCOUNT_ENDPOINT="/api/BankAccounts"
HEADERS="Content-Type: application/json"

echo "=== Creating Bank Accounts for Testing ==="

# Create Account 1
echo "Creating bank account 1..."
RESPONSE=$(curl -s -X POST "${API_URL}${BANK_ACCOUNT_ENDPOINT}" \
  -H "Content-Type: application/json" \
  -d '{
    "accountNumber": "TEST-001",
    "ownerName": "Test User 1",
    "balance": 1000.00,
    "type": 0,
    "createdDate": "2023-01-01T00:00:00",
    "isActive": true,
    "bankId": 1,
    "branchId": 1
  }')

echo "Response for account 1: ${RESPONSE}"

# Manually set the account ID for testing
ACCOUNT1_ID=1
echo "Using account ID: ${ACCOUNT1_ID}"

# Create Account 2
echo "Creating bank account 2..."
RESPONSE2=$(curl -s -X POST "${API_URL}${BANK_ACCOUNT_ENDPOINT}" \
  -H "Content-Type: application/json" \
  -d '{
    "accountNumber": "TEST-002",
    "ownerName": "Test User 2",
    "balance": 5000.00,
    "type": 1,
    "createdDate": "2023-01-01T00:00:00",
    "isActive": true,
    "bankId": 1,
    "branchId": 1
  }')

echo "Response for account 2: ${RESPONSE2}"

# Manually set the account ID for testing
ACCOUNT2_ID=2
echo "Using account ID: ${ACCOUNT2_ID}"

# Create a file with the account IDs for the test script to use
echo "Saving account IDs to test-accounts.txt"
echo "${ACCOUNT1_ID}" > test-accounts.txt
echo "${ACCOUNT2_ID}" >> test-accounts.txt

echo "=== Bank Accounts Created Successfully ==="
echo "You can now use account IDs ${ACCOUNT1_ID} and ${ACCOUNT2_ID} in your tests."