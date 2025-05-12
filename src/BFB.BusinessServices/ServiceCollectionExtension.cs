using Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BFB.BusinessServices;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // Register business services
        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICustomerAccountService, CustomerAccountService>();
        services.AddScoped<ITransactionService, TransactionService>();

        return services;
    }
}