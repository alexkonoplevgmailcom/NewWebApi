using Abstractions.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using System;

namespace BFB.DataAccess.RestApi;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRestApiDataAccess(this IServiceCollection services, IConfiguration configuration, IHostEnvironment? environment = null)
    {
        // Register API configuration
        var apiConfig = new TransactionApiConfig();
        configuration.GetSection("TransactionApi")?.Bind(apiConfig);
        services.AddSingleton(apiConfig);
        
        // Register retry policy configuration
        var retryPolicyConfig = new RetryPolicyConfig();
        configuration.GetSection("RetryPolicy")?.Bind(retryPolicyConfig);
        services.AddSingleton(retryPolicyConfig);
        
        // Register retry policy service
        services.AddSingleton<RetryPolicyService>();
        
        // Register memory cache if not already registered
        services.AddMemoryCache();
        
        // Configure named HttpClient with policies using HttpClientFactory
        services.AddHttpClient("TransactionApiClient", (serviceProvider, client) => 
        {
            var config = serviceProvider.GetRequiredService<TransactionApiConfig>();
            client.BaseAddress = new Uri(config.BaseUrl);
            client.DefaultRequestHeaders.Add("X-API-Key", config.ApiKey);
            client.DefaultRequestHeaders.Add("User-Agent", "BFB-Template-API");
            client.Timeout = TimeSpan.FromSeconds(retryPolicyConfig.TimeoutInSeconds);
        })
        .AddPolicyHandler((serviceProvider, _) => 
        {
            var policyService = serviceProvider.GetRequiredService<RetryPolicyService>();
            return policyService.CreateCombinedPolicy();
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5)); // Set lifetime to avoid DNS changes issues
        
        // Register the repository with the named client
        services.AddTransient<ITransactionRepository, TransactionRepository>();
        
        return services;
    }
}