using Abstractions.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BFB.DataAccess.Mongo;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddMongoDB(this IServiceCollection services, IConfiguration configuration, IHostEnvironment? environment = null)
    {
        // Register MongoDbContext
        services.AddSingleton<BankMongoDbContext>();
        
        // Register memory cache if not already registered
        services.AddMemoryCache();
        
        // Register retry policy configuration
        var retryPolicyConfig = new RetryPolicyConfig();
        configuration.GetSection("RetryPolicy")?.Bind(retryPolicyConfig);
        services.AddSingleton(retryPolicyConfig);
        
        // Register retry policy service
        services.AddSingleton<RetryPolicyService>();

        // Register repositories using interfaces
        services.AddScoped<IBankBranchRepository, BankBranchRepository>();
        
        return services;
    }
}