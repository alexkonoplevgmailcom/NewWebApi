#!/bin/bash

# Transaction API Test Script
# This script runs a series of curl commands to test the Transaction API

API_URL="http://localhost:5053"
TRANSACTION_ENDPOINT="/api/Transactions"
HEADERS="-H \"Content-Type: application/json\""

# Read the account IDs from the file if it exists
if [ -f "test-accounts.txt" ]; then
  ACCOUNT1_ID=$(head -n 1 test-accounts.txt)
  ACCOUNT2_ID=$(tail -n 1 test-accounts.txt)
  echo "Using account IDs from file: ${ACCOUNT1_ID} and ${ACCOUNT2_ID}"
else
  echo "WARNING: test-accounts.txt not found. Please run create-bank-accounts.sh first."
  echo "Using default account ID 1 which may not exist."
  ACCOUNT1_ID=1
  ACCOUNT2_ID=2
fi

echo "=== Starting Transaction API Tests ==="
echo ""

echo "=== Test 1: GET transactions for account ID ${ACCOUNT1_ID} ==="
curl -X GET ${API_URL}${TRANSACTION_ENDPOINT}/account/${ACCOUNT1_ID} -H "Content-Type: application/json"
echo -e "\n"
sleep 1

echo "=== Test 2: GET specific transaction with ID 1 ==="
curl -X GET ${API_URL}${TRANSACTION_ENDPOINT}/1 -H "Content-Type: application/json"
echo -e "\n"
sleep 1

echo "=== Test 3: GET transactions for account ID ${ACCOUNT1_ID} within date range ==="
curl -X GET "${API_URL}${TRANSACTION_ENDPOINT}/account/${ACCOUNT1_ID}/date-range?startDate=2023-01-01&endDate=$(date +%Y-%m-%d)" -H "Content-Type: application/json"
echo -e "\n"
sleep 1

echo "=== Test 4: CREATE a deposit transaction ==="
curl -X POST ${API_URL}${TRANSACTION_ENDPOINT} -H "Content-Type: application/json" -d '{
  "accountId": '${ACCOUNT1_ID}',
  "transactionType": "Deposit",
  "amount": 500.00,
  "description": "Test deposit"
}'
echo -e "\n"
sleep 1

echo "=== Test 5: CREATE a withdrawal transaction ==="
curl -X POST ${API_URL}${TRANSACTION_ENDPOINT} -H "Content-Type: application/json" -d '{
  "accountId": '${ACCOUNT1_ID}',
  "transactionType": "Withdrawal",
  "amount": -250.00,
  "description": "Test withdrawal"
}'
echo -e "\n"
sleep 1

echo "=== Test 6: CREATE a transaction with insufficient funds (should fail) ==="
curl -X POST ${API_URL}${TRANSACTION_ENDPOINT} -H "Content-Type: application/json" -d '{
  "accountId": '${ACCOUNT1_ID}',
  "transactionType": "Withdrawal",
  "amount": -999999.99,
  "description": "This should fail"
}'
echo -e "\n"
sleep 1

echo "=== Test 7: CREATE a transaction with invalid account (should fail) ==="
curl -X POST ${API_URL}${TRANSACTION_ENDPOINT} -H "Content-Type: application/json" -d '{
  "accountId": 999,
  "transactionType": "Deposit",
  "amount": 100.00,
  "description": "This should fail"
}'
echo -e "\n"

echo "=== All transaction tests completed ==="