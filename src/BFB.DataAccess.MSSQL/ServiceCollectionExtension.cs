using Abstractions.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BFB.DataAccess.MSSQL;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddMSSQL(this IServiceCollection services, IConfiguration configuration, IHostEnvironment? environment = null)
    {
        // Check if we're in development environment
        bool isDevelopment = environment?.IsDevelopment() ?? false;
        
        // Register DbContext
        if (isDevelopment)
        {
            // Use in-memory database for development/testing
            services.AddDbContext<BankDbContext>(options =>
                options.UseInMemoryDatabase("BankInMemoryDb"));
        }
        else
        {
            // Use SQL Server for production
            services.AddDbContext<BankDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        }

        // Register memory cache
        services.AddMemoryCache();
        
        // Register retry policy configuration
        var retryPolicyConfig = new RetryPolicyConfig();
        configuration.GetSection("RetryPolicy")?.Bind(retryPolicyConfig);
        services.AddSingleton(retryPolicyConfig);
        
        // Register retry policy service
        services.AddSingleton<RetryPolicyService>();

        // Register repositories using interfaces
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();

        return services;
    }
}