using Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BFB.DataAccess.DB2;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddDB2DataAccess(this IServiceCollection services)
    {
        // Register DB2 context and repositories
        services.AddSingleton<BankDB2Context>();
        services.AddSingleton<RetryPolicyService>();
        
        // Register repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerAccountRepository, CustomerAccountRepository>();
        
        return services;
    }
}