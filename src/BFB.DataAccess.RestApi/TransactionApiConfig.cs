namespace BFB.DataAccess.RestApi;

public class TransactionApiConfig
{
    public string BaseUrl { get; set; } = "https://api.bankfinancial.com";
    public string ApiKey { get; set; } = string.Empty;
    public int CacheExpirationMinutes { get; set; } = 5;
}