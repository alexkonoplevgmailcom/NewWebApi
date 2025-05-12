namespace BFB.DataAccess.RestApi;

public class RetryPolicyConfig
{
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelayInMilliseconds { get; set; } = 500;
    public int TimeoutInSeconds { get; set; } = 30;
}