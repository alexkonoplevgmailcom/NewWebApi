using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace BFB.DataAccess.Mongo;

public class RetryPolicyService
{
    private readonly RetryPolicyConfig _config;
    private readonly ILogger<RetryPolicyService> _logger;

    public RetryPolicyService(RetryPolicyConfig config, ILogger<RetryPolicyService> logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public AsyncRetryPolicy CreateAsyncRetryPolicy()
    {
        return Policy
            .Handle<MongoException>()
            .Or<TimeoutRejectedException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                _config.MaxRetryAttempts,
                retryAttempt => TimeSpan.FromMilliseconds(_config.RetryDelayInMilliseconds * Math.Pow(2, retryAttempt - 1)), // Exponential backoff
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Attempt {RetryCount} of {MaxRetryAttempts} failed after {RetryDelayInMs}ms. Exception: {ExceptionMessage}",
                        retryCount,
                        _config.MaxRetryAttempts,
                        timeSpan.TotalMilliseconds,
                        exception.Message);
                });
    }

    public AsyncTimeoutPolicy CreateAsyncTimeoutPolicy()
    {
        return Policy.TimeoutAsync(_config.RetryTimeoutInSeconds);
    }

    public AsyncPolicy CreateCombinedAsyncPolicy()
    {
        return CreateAsyncTimeoutPolicy().WrapAsync(CreateAsyncRetryPolicy());
    }

    public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName)
    {
        var policy = CreateCombinedAsyncPolicy();
        
        try
        {
            _logger.LogDebug("Executing operation {OperationName} with retry policy", operationName);
            return await policy.ExecuteAsync(operation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Operation {OperationName} failed after all retry attempts", operationName);
            throw;
        }
    }
}