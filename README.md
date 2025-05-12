# Banking Facility Backend (BFB) - New WebAPI

A robust banking facility backend system that provides RESTful APIs for managing bank accounts. The system is built using ASP.NET Core and utilizes Microsoft SQL Server for data storage.

## Features

- Microsoft SQL Server for bank account management
- REST API endpoints for bank account operations
- Built-in caching mechanism
- Retry policies for improved reliability
- Swagger/OpenAPI documentation

## Prerequisites

- .NET 8.0 SDK or later
- Docker and Docker Compose
- Microsoft SQL Server 2019 or later

## Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/bfb.git
cd bfb
```

2. Start the required databases using Docker Compose:
```bash
docker-compose up -d
```

3. Update the connection strings in `appsettings.json` if needed:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BFBTemplateDB;..."
  }
}
```

4. Build and run the application:
```bash
dotnet restore
dotnet build
dotnet run --project src/BFB.BankAccountApi
```

## Configuration

The application can be configured through the following files:

- `appsettings.json`: Main configuration file
- `appsettings.Development.json`: Development-specific settings
- `docker-compose.yml`: Database container configurations

Key configuration sections:

```json
{
  "RetryPolicy": {
    "MaxRetryAttempts": 3,
    "RetryTimeoutInSeconds": 30,
    "RetryDelayInMilliseconds": 500
  }
}
```

## API Endpoints

### Bank Account Endpoints

| Method | Endpoint | Description | Request/Response Details |
|--------|----------|-------------|------------------------|
| GET | `/api/bankaccounts` | Retrieves all bank accounts. Returns 200 OK on success. | **Response:** Array of bank account objects:<br>```json<br>[<br>  {<br>    "id": 1,<br>    "accountNumber": "1234567890",<br>    "ownerName": "John Doe",<br>    "balance": 1000.00,<br>    "type": 0,<br>    "createdDate": "2023-01-01T00:00:00Z",<br>    "isActive": true,<br>    "bankId": 1,<br>    "branchId": 1<br>  }<br>]<br>``` |
| GET | `/api/bankaccounts/{id}` | Retrieves a specific bank account by ID. Returns 200 OK on success, 404 Not Found if account doesn't exist. | **Response:** Single bank account object |
| POST | `/api/bankaccounts` | Creates a new bank account. Returns 201 Created on success, 400 Bad Request if validation fails. | **Request:**<br>```json<br>{<br>  "accountNumber": "1234567890",<br>  "ownerName": "John Doe",<br>  "balance": 1000.00,<br>  "type": 0,<br>  "bankId": 1,<br>  "branchId": 1<br>}<br>``` |
| PUT | `/api/bankaccounts/{id}` | Updates an existing bank account. Returns 204 No Content on success, 404 Not Found if account doesn't exist. | **Request:** Same as POST request format |
| DELETE | `/api/bankaccounts/{id}` | Deletes a bank account. Returns 204 No Content on success, 404 Not Found if account doesn't exist. | No request body required |

### Common HTTP Status Codes

- 200 OK: Successful GET operations
- 201 Created: Successful POST operations
- 204 No Content: Successful PUT, DELETE operations
- 400 Bad Request: Invalid input
- 404 Not Found: Resource not found
- 500 Internal Server Error: Server-side errors

For detailed request/response schemas, please refer to the Swagger documentation at `/swagger` when running the API locally.

## Development

### Running Tests

```bash
dotnet test
```

### Populating Sample Data

Use the provided PowerShell script to populate sample data:

```powershell
.\scripts\PopulateData.ps1
```

### Testing the API

You can test the API using the provided PowerShell script:

```powershell
.\scripts\Test-Api.ps1
```

### Creating Test Accounts

To create test accounts for your environment:

```powershell
.\scripts\Create-BankAccounts.ps1
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.