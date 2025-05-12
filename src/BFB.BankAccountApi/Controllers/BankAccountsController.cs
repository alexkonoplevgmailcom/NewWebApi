using Abstractions.DTO;
using Abstractions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BFB.BankAccountApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankAccountsController : ControllerBase
{
    private readonly IBankAccountService _bankAccountService;
    private readonly ILogger<BankAccountsController> _logger;

    public BankAccountsController(IBankAccountService bankAccountService, ILogger<BankAccountsController> logger)
    {
        _bankAccountService = bankAccountService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BankAccount>>> GetAllAccounts()
    {
        try
        {
            var accounts = await _bankAccountService.GetAllBankAccountsAsync();
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all bank accounts");
            return StatusCode(500, "An error occurred while retrieving bank accounts");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BankAccount>> GetAccountById(int id)
    {
        try
        {
            var account = await _bankAccountService.GetBankAccountByIdAsync(id);
            if (account == null)
            {
                return NotFound($"Bank account with ID {id} not found");
            }
            return Ok(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bank account {Id}", id);
            return StatusCode(500, $"An error occurred while retrieving bank account {id}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<BankAccount>> CreateAccount(BankAccount account)
    {
        try
        {
            var createdAccount = await _bankAccountService.CreateBankAccountAsync(account);
            return CreatedAtAction(nameof(GetAccountById), new { id = createdAccount.Id }, createdAccount);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bank account");
            return StatusCode(500, "An error occurred while creating the bank account");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(int id, BankAccount account)
    {
        try
        {
            if (id != account.Id)
            {
                return BadRequest("ID in route does not match ID in account object");
            }

            var success = await _bankAccountService.UpdateBankAccountAsync(id, account);
            if (!success)
            {
                return NotFound($"Bank account with ID {id} not found");
            }
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating bank account {Id}", id);
            return StatusCode(500, $"An error occurred while updating bank account {id}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        try
        {
            var success = await _bankAccountService.DeleteBankAccountAsync(id);
            if (!success)
            {
                return NotFound($"Bank account with ID {id} not found");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting bank account {Id}", id);
            return StatusCode(500, $"An error occurred while deleting bank account {id}");
        }
    }
}