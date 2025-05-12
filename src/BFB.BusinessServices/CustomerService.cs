using Abstractions.DTO;
using Abstractions.Interfaces;
using Microsoft.Extensions.Logging;

namespace BFB.BusinessServices;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ICustomerRepository customerRepository,
        ILogger<CustomerService> logger)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all customers");
            return await _customerRepository.GetAllCustomersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all customers");
            throw;
        }
    }

    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving customer with ID: {CustomerId}", id);
            return await _customerRepository.GetCustomerByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer with ID: {CustomerId}", id);
            throw;
        }
    }

    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        // Add business validation
        if (string.IsNullOrWhiteSpace(customer.FirstName))
        {
            throw new ArgumentException("First name cannot be empty", nameof(customer));
        }

        if (string.IsNullOrWhiteSpace(customer.LastName))
        {
            throw new ArgumentException("Last name cannot be empty", nameof(customer));
        }

        if (string.IsNullOrWhiteSpace(customer.Email))
        {
            throw new ArgumentException("Email cannot be empty", nameof(customer));
        }

        // Set default values
        customer.CreatedDate = DateTime.UtcNow;
        customer.IsActive = true;

        try
        {
            _logger.LogInformation("Creating new customer: {FirstName} {LastName}", customer.FirstName, customer.LastName);
            return await _customerRepository.CreateCustomerAsync(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer: {FirstName} {LastName}", customer.FirstName, customer.LastName);
            throw;
        }
    }

    public async Task<bool> UpdateCustomerAsync(int id, Customer customer)
    {
        // Verify that the customer exists
        var existingCustomer = await _customerRepository.GetCustomerByIdAsync(id);
        if (existingCustomer == null)
        {
            _logger.LogWarning("Customer with ID {CustomerId} not found for update", id);
            return false;
        }

        // Add business validation
        if (string.IsNullOrWhiteSpace(customer.FirstName))
        {
            throw new ArgumentException("First name cannot be empty", nameof(customer));
        }

        if (string.IsNullOrWhiteSpace(customer.LastName))
        {
            throw new ArgumentException("Last name cannot be empty", nameof(customer));
        }

        if (string.IsNullOrWhiteSpace(customer.Email))
        {
            throw new ArgumentException("Email cannot be empty", nameof(customer));
        }

        // Preserve creation date from the existing customer
        customer.CreatedDate = existingCustomer.CreatedDate;

        try
        {
            _logger.LogInformation("Updating customer with ID: {CustomerId}", id);
            return await _customerRepository.UpdateCustomerAsync(id, customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer with ID: {CustomerId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting customer with ID: {CustomerId}", id);
            return await _customerRepository.DeleteCustomerAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer with ID: {CustomerId}", id);
            throw;
        }
    }
}