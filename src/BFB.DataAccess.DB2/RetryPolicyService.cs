using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace BFB.DataAccess.DB2;

public class RetryPolicyConfig
{
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelayMilliseconds { get; set; } = 500;
}

public class RetryPolicyService
{
    private readonly RetryPolicyConfig _config;
    private readonly ILogger<RetryPolicyService> _logger;

    public RetryPolicyService(IConfiguration configuration, ILogger<RetryPolicyService> logger)
    {
        _config = new RetryPolicyConfig();
        configuration.GetSection("RetryPolicy")?.Bind(_config);
        _logger = logger;
    }

    public AsyncRetryPolicy GetAsyncRetryPolicy()
    {
        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                _config.MaxRetryAttempts,
                retryAttempt => TimeSpan.FromMilliseconds(_config.RetryDelayMilliseconds * Math.Pow(2, retryAttempt - 1)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception, "Error executing DB2 operation. Retrying ({RetryCount}/{MaxRetryAttempts}) after {RetryDelay}ms", 
                        retryCount, _config.MaxRetryAttempts, timeSpan.TotalMilliseconds);
                });
    }
}