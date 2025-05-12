using Abstractions.DTO;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace BFB.DataAccess.RestApi.Entities;

/// <summary>
/// Mock server for simulating the remote transactions API.
/// This class can be used for testing without relying on the actual external API.
/// </summary>
public class MockTransactionApiServer
{
    private readonly ConcurrentDictionary<int, Transaction> _transactions = new();
    private readonly ConcurrentDictionary<int, List<int>> _accountTransactions = new();
    private int _nextTransactionId = 1;
    
    // Setup some initial mock data
    public MockTransactionApiServer()
    {
        // Create sample transactions for testing
        SeedMockData();
    }

    private void SeedMockData()
    {
        // Create some test transactions for account 1
        var account1Transactions = new List<Transaction>
        {
            CreateTransaction(1, "Deposit", 1000.00m, "Initial deposit", DateTime.Now.AddDays(-30)),
            CreateTransaction(1, "Withdrawal", -250.00m, "ATM Withdrawal", DateTime.Now.AddDays(-25)),
            CreateTransaction(1, "Deposit", 2500.00m, "Salary payment", DateTime.Now.AddDays(-15)),
            CreateTransaction(1, "Payment", -129.99m, "Utility bill", DateTime.Now.AddDays(-10)),
            CreateTransaction(1, "Transfer", -500.00m, "Transfer to savings", DateTime.Now.AddDays(-5))
        };
        
        // Create some test transactions for account 2
        var account2Transactions = new List<Transaction>
        {
            CreateTransaction(2, "Deposit", 5000.00m, "Initial deposit", DateTime.Now.AddDays(-60)),
            CreateTransaction(2, "Withdrawal", -1500.00m, "Car repair", DateTime.Now.AddDays(-45)),
            CreateTransaction(2, "Deposit", 3000.00m, "Bonus payment", DateTime.Now.AddDays(-30)),
            CreateTransaction(2, "Payment", -899.99m, "Electronics purchase", DateTime.Now.AddDays(-20)),
            CreateTransaction(2, "Deposit", 500.00m, "Refund", DateTime.Now.AddDays(-2))
        };
    }

    private Transaction CreateTransaction(int accountId, string type, decimal amount, string description, DateTime timestamp)
    {
        var id = _nextTransactionId++;
        var transaction = new Transaction
        {
            Id = id,
            AccountId = accountId,
            TransactionType = type,
            Amount = amount,
            Description = description,
            Timestamp = timestamp,
            Reference = $"REF{id:D6}",
            BalanceAfterTransaction = CalculateBalance(accountId, amount) // This is simplified
        };
        
        _transactions[id] = transaction;
        
        if (!_accountTransactions.TryGetValue(accountId, out var transactions))
        {
            transactions = new List<int>();
            _accountTransactions[accountId] = transactions;
        }
        
        transactions.Add(id);
        
        return transaction;
    }

    private decimal CalculateBalance(int accountId, decimal amount)
    {
        // In a real system, this would calculate the actual balance
        // For this mock, we'll just use a simple approach
        if (!_accountTransactions.TryGetValue(accountId, out var transactionIds))
        {
            return amount;
        }
        
        decimal balance = 0;
        foreach (var id in transactionIds)
        {
            if (_transactions.TryGetValue(id, out var transaction))
            {
                balance += transaction.Amount;
            }
        }
        
        return balance + amount;
    }

    // Simulates the API endpoint: GET api/accounts/{accountId}/transactions
    public HttpResponseMessage GetTransactionsByAccountId(int accountId)
    {
        if (!_accountTransactions.TryGetValue(accountId, out var transactionIds))
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
        
        var transactions = transactionIds
            .Select(id => _transactions[id])
            .OrderByDescending(t => t.Timestamp)
            .ToList();
        
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Content = new StringContent(JsonSerializer.Serialize(transactions));
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        
        return response;
    }

    // Simulates the API endpoint: GET api/transactions/{id}
    public HttpResponseMessage GetTransactionById(int id)
    {
        if (!_transactions.TryGetValue(id, out var transaction))
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
        
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Content = new StringContent(JsonSerializer.Serialize(transaction));
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        
        return response;
    }

    // Simulates the API endpoint: GET api/accounts/{accountId}/transactions?startDate={startDate}&endDate={endDate}
    public HttpResponseMessage GetTransactionsByDateRange(int accountId, DateTime startDate, DateTime endDate)
    {
        if (!_accountTransactions.TryGetValue(accountId, out var transactionIds))
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
        
        var transactions = transactionIds
            .Select(id => _transactions[id])
            .Where(t => t.Timestamp >= startDate && t.Timestamp <= endDate)
            .OrderByDescending(t => t.Timestamp)
            .ToList();
        
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Content = new StringContent(JsonSerializer.Serialize(transactions));
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        
        return response;
    }

    // Simulates the API endpoint: POST api/transactions
    public HttpResponseMessage CreateTransaction(Transaction transaction)
    {
        if (transaction == null)
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
        
        var id = _nextTransactionId++;
        transaction.Id = id;
        transaction.Timestamp = DateTime.Now;
        transaction.BalanceAfterTransaction = CalculateBalance(transaction.AccountId, transaction.Amount);
        transaction.Reference = $"REF{id:D6}";
        
        _transactions[id] = transaction;
        
        if (!_accountTransactions.TryGetValue(transaction.AccountId, out var transactions))
        {
            transactions = new List<int>();
            _accountTransactions[transaction.AccountId] = transactions;
        }
        
        transactions.Add(id);
        
        var response = new HttpResponseMessage(HttpStatusCode.Created);
        response.Content = new StringContent(JsonSerializer.Serialize(transaction));
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        
        return response;
    }
}