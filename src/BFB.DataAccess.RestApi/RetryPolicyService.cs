using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using System.Net;

namespace BFB.DataAccess.RestApi;

public class RetryPolicyService
{
    private readonly RetryPolicyConfig _config;
    private readonly ILogger<RetryPolicyService> _logger;

    public RetryPolicyService(RetryPolicyConfig config, ILogger<RetryPolicyService> logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IAsyncPolicy<HttpResponseMessage> CreateHttpRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                _config.MaxRetryAttempts,
                retryAttempt => TimeSpan.FromMilliseconds(_config.RetryDelayInMilliseconds * Math.Pow(2, retryAttempt - 1)), // Exponential backoff
                (outcome, timeSpan, retryCount, context) =>
                {
                    if (outcome.Exception != null)
                    {
                        _logger.LogWarning(
                            outcome.Exception,
                            "Attempt {RetryCount} of {MaxRetryAttempts} failed after {RetryDelayInMs}ms due to exception: {ExceptionMessage}",
                            retryCount,
                            _config.MaxRetryAttempts,
                            timeSpan.TotalMilliseconds,
                            outcome.Exception.Message);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Attempt {RetryCount} of {MaxRetryAttempts} failed after {RetryDelayInMs}ms with status code: {StatusCode}",
                            retryCount,
                            _config.MaxRetryAttempts,
                            timeSpan.TotalMilliseconds,
                            outcome.Result?.StatusCode);
                    }
                });
    }

    public IAsyncPolicy<HttpResponseMessage> CreateHttpTimeoutPolicy()
    {
        return Policy
            .TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromSeconds(_config.TimeoutInSeconds), 
                TimeoutStrategy.Pessimistic,
                (context, timeSpan, task) =>
                {
                    _logger.LogWarning("HTTP request timeout after {TimeoutInSeconds}s", timeSpan.TotalSeconds);
                    return Task.CompletedTask;
                });
    }

    public IAsyncPolicy<HttpResponseMessage> CreateCombinedPolicy()
    {
        return Policy.WrapAsync(CreateHttpRetryPolicy(), CreateHttpTimeoutPolicy());
    }
}