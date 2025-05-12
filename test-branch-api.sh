#!/bin/bash

# Test script for adding more bank branches using the BankBranchController
# This script will add new branches and verify they were added correctly

API_URL="http://localhost:5058"
ENDPOINT="/api/BankBranch"
HEADERS="-H \"Content-Type: application/json\""

echo "=== Starting BankBranch API Tests ==="
echo ""

echo "=== Test 1: GET all bank branches (initial state) ==="
curl -X GET ${API_URL}${ENDPOINT} -H "Content-Type: application/json"
echo -e "\n"
sleep 1

echo "=== Test 2: CREATE a new bank branch for Bank 1 ==="
curl -X POST ${API_URL}${ENDPOINT} -H "Content-Type: application/json" -d '{
  "bankId": 1,
  "branchName": "Financial District Branch",
  "address": "555 Wall Street",
  "city": "New York",
  "state": "NY",
  "zipCode": "10005",
  "phoneNumber": "212-555-4321",
  "isActive": true
}'
echo -e "\n"
sleep 1

echo "=== Test 3: CREATE a new bank branch for Bank 2 ==="
curl -X POST ${API_URL}${ENDPOINT} -H "Content-Type: application/json" -d '{
  "bankId": 2,
  "branchName": "East Side Branch",
  "address": "123 Lexington Avenue",
  "city": "New York",
  "state": "NY",
  "zipCode": "10016",
  "phoneNumber": "212-555-8765",
  "isActive": true
}'
echo -e "\n"
sleep 1

echo "=== Test 4: GET all bank branches (after adding new branches) ==="
curl -X GET ${API_URL}${ENDPOINT} -H "Content-Type: application/json"
echo -e "\n"
sleep 1

echo "=== Test 5: GET branches for Bank 1 ==="
curl -X GET ${API_URL}${ENDPOINT}/bank/1 -H "Content-Type: application/json"
echo -e "\n"
sleep 1

echo "=== Test 6: GET branches for Bank 2 ==="
curl -X GET ${API_URL}${ENDPOINT}/bank/2 -H "Content-Type: application/json"
echo -e "\n"
sleep 1

echo "=== Test 7: UPDATE a branch ==="
# We'll use branch with ID 4 (the first new branch we added)
curl -X PUT ${API_URL}${ENDPOINT}/4 -H "Content-Type: application/json" -d '{
  "id": 4,
  "bankId": 1,
  "branchName": "Financial District Branch - Updated",
  "address": "555 Wall Street, Suite 200",
  "city": "New York",
  "state": "NY",
  "zipCode": "10005",
  "phoneNumber": "212-555-4321",
  "isActive": true
}'
echo -e "\n"
sleep 1

echo "=== Test 8: GET updated branch ==="
curl -X GET ${API_URL}${ENDPOINT}/4 -H "Content-Type: application/json"
echo -e "\n"

echo "=== All tests completed ==="