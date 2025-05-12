#!/bin/bash

# BFB Template API Test Script
# This script runs a series of curl commands to test the BankAccount API
# and validate the caching strategy

API_URL="http://localhost:5052"
ENDPOINT="/api/BankAccount"
HEADERS="-H \"Content-Type: application/json\""

echo "=== Starting BankAccount API Tests ==="
echo ""

echo "=== Test 1: GET all bank accounts (first call - should be a cache miss) ==="
curl -X GET ${API_URL}${ENDPOINT} -H "Content-Type: application/json"
echo -e "\n"
sleep 1

echo "=== Test 2: GET all bank accounts again (should be a cache hit) ==="
curl -X GET ${API_URL}${ENDPOINT} -H "Content-Type: application/json"
echo -e "\n"
sleep 1

echo "=== Test 3: GET specific bank account with ID 1 (should be a cache miss) ==="
curl -X GET ${API_URL}${ENDPOINT}/1 -H "Content-Type: application/json"
echo -e "\n"
sleep 1

echo "=== Test 4: GET same bank account again (should be a cache hit) ==="
curl -X GET ${API_URL}${ENDPOINT}/1 -H "Content-Type: application/json"
echo -e "\n"
sleep 1

echo "=== Test 5: CREATE a new bank account (should invalidate the list cache) ==="
curl -X POST ${API_URL}${ENDPOINT} -H "Content-Type: application/json" -d '{
  "accountNumber": "ACC-003",
  "ownerName": "Michael Johnson",
  "balance": 7500.25,
  "type": 2
}'
echo -e "\n"
sleep 1

echo "=== Test 6: GET all bank accounts after creation (should be a cache miss) ==="
curl -X GET ${API_URL}${ENDPOINT} -H "Content-Type: application/json"
echo -e "\n"
sleep 1

echo "=== Test 7: UPDATE bank account with ID 2 ==="
curl -X PUT ${API_URL}${ENDPOINT}/2 -H "Content-Type: application/json" -d '{
  "accountNumber": "ACC-002",
  "ownerName": "Jane Smith-Brown",
  "balance": 20000.00,
  "type": 1,
  "isActive": true
}'
echo -e "\n"
sleep 1

echo "=== Test 8: GET updated bank account (should be a cache hit for the updated account) ==="
curl -X GET ${API_URL}${ENDPOINT}/2 -H "Content-Type: application/json"
echo -e "\n"
sleep 1

echo "=== Test 9: DELETE bank account with ID 3 ==="
curl -X DELETE ${API_URL}${ENDPOINT}/3 -H "Content-Type: application/json"
echo -e "\n"
sleep 1

echo "=== Test 10: Verify deletion (should return 404 Not Found) ==="
curl -X GET ${API_URL}${ENDPOINT}/3 -H "Content-Type: application/json"
echo -e "\n"
sleep 1

echo "=== Test 11: GET all bank accounts after deletion (should be a cache miss) ==="
curl -X GET ${API_URL}${ENDPOINT} -H "Content-Type: application/json"
echo -e "\n"

echo "=== All tests completed ==="