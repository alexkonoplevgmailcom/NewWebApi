using Abstractions.DTO;

namespace Abstractions.Interfaces;

public interface ICustomerAccountRepository
{
    Task<IEnumerable<CustomerAccount>> GetAllAccountsAsync();
    Task<CustomerAccount?> GetAccountByIdAsync(int id);
    Task<IEnumerable<CustomerAccount>> GetAccountsByCustomerIdAsync(int customerId);
    Task<CustomerAccount> CreateAccountAsync(CustomerAccount account);
    Task<bool> UpdateAccountAsync(int id, CustomerAccount account);
    Task<bool> DeleteAccountAsync(int id);
}