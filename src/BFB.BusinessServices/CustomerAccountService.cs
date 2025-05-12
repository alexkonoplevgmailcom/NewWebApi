using Abstractions.DTO;
using Abstractions.Interfaces;
using Microsoft.Extensions.Logging;

namespace BFB.BusinessServices;

public class CustomerAccountService : ICustomerAccountService
{
    private readonly ICustomerAccountRepository _accountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CustomerAccountService> _logger;

    public CustomerAccountService(
        ICustomerAccountRepository accountRepository,
        ICustomerRepository customerRepository,
        ILogger<CustomerAccountService> logger)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<CustomerAccount>> GetAllCustomerAccountsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all customer accounts");
            return await _accountRepository.GetAllAccountsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all customer accounts");
            throw;
        }
    }

    public async Task<CustomerAccount?> GetCustomerAccountByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving customer account with ID: {AccountId}", id);
            return await _accountRepository.GetAccountByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer account with ID: {AccountId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<CustomerAccount>> GetAccountsByCustomerIdAsync(int customerId)
    {
        try
        {
            _logger.LogInformation("Retrieving accounts for customer with ID: {CustomerId}", customerId);
            return await _accountRepository.GetAccountsByCustomerIdAsync(customerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts for customer with ID: {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<CustomerAccount> CreateCustomerAccountAsync(CustomerAccount customerAccount)
    {
        // Add business validation
        if (string.IsNullOrWhiteSpace(customerAccount.AccountNumber))
        {
            throw new ArgumentException("Account number cannot be empty", nameof(customerAccount));
        }

        if (customerAccount.Balance < 0)
        {
            throw new ArgumentException("Initial balance cannot be negative", nameof(customerAccount));
        }

        // Validate Customer exists
        var customer = await _customerRepository.GetCustomerByIdAsync(customerAccount.CustomerId);
        if (customer == null)
        {
            throw new ArgumentException($"Customer with ID {customerAccount.CustomerId} does not exist", nameof(customerAccount));
        }

        // Set default values
        customerAccount.CreatedDate = DateTime.UtcNow;
        customerAccount.IsActive = true;

        try
        {
            _logger.LogInformation("Creating new customer account: {AccountNumber} for customer ID: {CustomerId}", 
                customerAccount.AccountNumber, customerAccount.CustomerId);
            
            return await _accountRepository.CreateAccountAsync(customerAccount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer account: {AccountNumber} for customer ID: {CustomerId}", 
                customerAccount.AccountNumber, customerAccount.CustomerId);
            throw;
        }
    }

    public async Task<bool> UpdateCustomerAccountAsync(int id, CustomerAccount customerAccount)
    {
        // Verify that the account exists
        var existingAccount = await _accountRepository.GetAccountByIdAsync(id);
        if (existingAccount == null)
        {
            _logger.LogWarning("Customer account with ID {AccountId} not found for update", id);
            return false;
        }

        // Add business validation
        if (string.IsNullOrWhiteSpace(customerAccount.AccountNumber))
        {
            throw new ArgumentException("Account number cannot be empty", nameof(customerAccount));
        }

        // Validate Customer exists if changing customer
        if (customerAccount.CustomerId != existingAccount.CustomerId)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(customerAccount.CustomerId);
            if (customer == null)
            {
                throw new ArgumentException($"Customer with ID {customerAccount.CustomerId} does not exist", nameof(customerAccount));
            }
        }

        // Preserve creation date from the existing account
        customerAccount.CreatedDate = existingAccount.CreatedDate;

        try
        {
            _logger.LogInformation("Updating customer account with ID: {AccountId}", id);
            return await _accountRepository.UpdateAccountAsync(id, customerAccount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer account with ID: {AccountId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteCustomerAccountAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting customer account with ID: {AccountId}", id);
            return await _accountRepository.DeleteAccountAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer account with ID: {AccountId}", id);
            throw;
        }
    }
}