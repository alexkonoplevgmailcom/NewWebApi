using System.Net.Http.Json;
using System.Text.Json;
using Abstractions.DTO;
using Abstractions.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BFB.DataAccess.RestApi;

public class TransactionRepository : ITransactionRepository
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly TransactionApiConfig _apiConfig;
    private readonly ILogger<TransactionRepository> _logger;
    
    // Cache keys
    private const string TransactionByIdCacheKeyPrefix = "Transaction_";
    private const string TransactionsByAccountIdCacheKeyPrefix = "TransactionsByAccount_";
    
    public TransactionRepository(
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        TransactionApiConfig apiConfig,
        ILogger<TransactionRepository> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _apiConfig = apiConfig ?? throw new ArgumentNullException(nameof(apiConfig));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(int accountId)
    {
        var cacheKey = $"{TransactionsByAccountIdCacheKeyPrefix}{accountId}";
        
        // Try to get from cache first
        if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<Transaction>? cachedTransactions) && cachedTransactions != null)
        {
            _logger.LogInformation("CACHE HIT: Retrieved transactions for account {AccountId} from cache", accountId);
            return cachedTransactions;
        }
        
        _logger.LogInformation("CACHE MISS: Getting transactions for account {AccountId} from API", accountId);
        
        // Create a client using the factory - this ensures proper lifecycle management
        var client = _httpClientFactory.CreateClient("TransactionApiClient");
        
        // If not in cache, get from API
        var response = await client.GetAsync($"api/accounts/{accountId}/transactions");
        response.EnsureSuccessStatusCode();
        
        var transactions = await response.Content.ReadFromJsonAsync<IEnumerable<Transaction>>() 
                          ?? throw new InvalidOperationException("Failed to deserialize transactions");
        
        // Store in cache
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(_apiConfig.CacheExpirationMinutes));
        
        _memoryCache.Set(cacheKey, transactions, cacheEntryOptions);
        
        return transactions;
    }

    public async Task<Transaction?> GetTransactionByIdAsync(int transactionId)
    {
        var cacheKey = $"{TransactionByIdCacheKeyPrefix}{transactionId}";
        
        // Try to get from cache first
        if (_memoryCache.TryGetValue(cacheKey, out Transaction? cachedTransaction) && cachedTransaction != null)
        {
            _logger.LogInformation("CACHE HIT: Retrieved transaction {TransactionId} from cache", transactionId);
            return cachedTransaction;
        }
        
        _logger.LogInformation("CACHE MISS: Getting transaction {TransactionId} from API", transactionId);
        
        // Create a client using the factory
        var client = _httpClientFactory.CreateClient("TransactionApiClient");
        
        // If not in cache, get from API
        var response = await client.GetAsync($"api/transactions/{transactionId}");
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogInformation("Transaction {TransactionId} not found", transactionId);
            return null;
        }
        
        response.EnsureSuccessStatusCode();
        
        var transaction = await response.Content.ReadFromJsonAsync<Transaction>()
                         ?? throw new InvalidOperationException("Failed to deserialize transaction");
        
        // Store in cache
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(_apiConfig.CacheExpirationMinutes));
        
        _memoryCache.Set(cacheKey, transaction, cacheEntryOptions);
        
        return transaction;
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(int accountId, DateTime startDate, DateTime endDate)
    {
        // This endpoint typically doesn't get cached since the date range can vary
        _logger.LogInformation("Getting transactions for account {AccountId} from {StartDate} to {EndDate}", 
            accountId, startDate, endDate);
        
        // Create a client using the factory
        var client = _httpClientFactory.CreateClient("TransactionApiClient");
        
        var response = await client.GetAsync(
            $"api/accounts/{accountId}/transactions?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
        
        response.EnsureSuccessStatusCode();
        
        var transactions = await response.Content.ReadFromJsonAsync<IEnumerable<Transaction>>()
                          ?? throw new InvalidOperationException("Failed to deserialize transactions");
        
        return transactions;
    }

    public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
    {
        _logger.LogInformation("Creating new transaction for account {AccountId}", transaction.AccountId);
        
        // Create a client using the factory
        var client = _httpClientFactory.CreateClient("TransactionApiClient");
        
        var response = await client.PostAsJsonAsync("api/transactions", transaction);
        response.EnsureSuccessStatusCode();
        
        var createdTransaction = await response.Content.ReadFromJsonAsync<Transaction>()
                                ?? throw new InvalidOperationException("Failed to deserialize created transaction");
        
        // Invalidate relevant cache entries
        var accountTransactionsCacheKey = $"{TransactionsByAccountIdCacheKeyPrefix}{transaction.AccountId}";
        _memoryCache.Remove(accountTransactionsCacheKey);
        
        return createdTransaction;
    }
}