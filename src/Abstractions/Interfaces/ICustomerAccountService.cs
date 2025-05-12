using Abstractions.DTO;

namespace Abstractions.Interfaces;

public interface ICustomerAccountService
{
    Task<IEnumerable<CustomerAccount>> GetAllCustomerAccountsAsync();
    Task<CustomerAccount?> GetCustomerAccountByIdAsync(int id);
    Task<IEnumerable<CustomerAccount>> GetAccountsByCustomerIdAsync(int customerId);
    Task<CustomerAccount> CreateCustomerAccountAsync(CustomerAccount customerAccount);
    Task<bool> UpdateCustomerAccountAsync(int id, CustomerAccount customerAccount);
    Task<bool> DeleteCustomerAccountAsync(int id);
}