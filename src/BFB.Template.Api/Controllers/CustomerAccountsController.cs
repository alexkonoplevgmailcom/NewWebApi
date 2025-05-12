using Abstractions.DTO;
using Abstractions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BFB.Template.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerAccountsController : ControllerBase
{
    private readonly ICustomerAccountService _customerAccountService;
    private readonly ILogger<CustomerAccountsController> _logger;

    public CustomerAccountsController(ICustomerAccountService customerAccountService, ILogger<CustomerAccountsController> logger)
    {
        _customerAccountService = customerAccountService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerAccount>>> GetAllAccounts()
    {
        try
        {
            var accounts = await _customerAccountService.GetAllCustomerAccountsAsync();
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all customer accounts");
            return StatusCode(500, "An error occurred while retrieving customer accounts");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerAccount>> GetAccountById(int id)
    {
        try
        {
            var account = await _customerAccountService.GetCustomerAccountByIdAsync(id);
            if (account == null)
            {
                return NotFound($"Customer account with ID {id} not found");
            }
            return Ok(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer account {Id}", id);
            return StatusCode(500, $"An error occurred while retrieving customer account {id}");
        }
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<CustomerAccount>>> GetAccountsByCustomerId(int customerId)
    {
        try
        {
            var accounts = await _customerAccountService.GetAccountsByCustomerIdAsync(customerId);
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts for customer {CustomerId}", customerId);
            return StatusCode(500, $"An error occurred while retrieving accounts for customer {customerId}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<CustomerAccount>> CreateAccount(CustomerAccount account)
    {
        try
        {
            var createdAccount = await _customerAccountService.CreateCustomerAccountAsync(account);
            return CreatedAtAction(nameof(GetAccountById), new { id = createdAccount.Id }, createdAccount);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer account");
            return StatusCode(500, "An error occurred while creating the customer account");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(int id, CustomerAccount account)
    {
        try
        {
            if (id != account.Id)
            {
                return BadRequest("ID in route does not match ID in account object");
            }

            var success = await _customerAccountService.UpdateCustomerAccountAsync(id, account);
            if (!success)
            {
                return NotFound($"Customer account with ID {id} not found");
            }
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer account {Id}", id);
            return StatusCode(500, $"An error occurred while updating customer account {id}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        try
        {
            var success = await _customerAccountService.DeleteCustomerAccountAsync(id);
            if (!success)
            {
                return NotFound($"Customer account with ID {id} not found");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer account {Id}", id);
            return StatusCode(500, $"An error occurred while deleting customer account {id}");
        }
    }
}