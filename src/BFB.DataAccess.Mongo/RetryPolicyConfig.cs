namespace BFB.DataAccess.Mongo;

public class RetryPolicyConfig
{
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryTimeoutInSeconds { get; set; } = 30;
    public int RetryDelayInMilliseconds { get; set; } = 500;
}