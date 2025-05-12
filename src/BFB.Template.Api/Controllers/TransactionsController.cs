using Abstractions.DTO;
using Abstractions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BFB.Template.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(
        ITransactionService transactionService,
        ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Transaction>> GetTransactionById(int id)
    {
        try
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction with ID {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the transaction");
        }
    }

    [HttpGet("account/{accountId}")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByAccountId(int accountId)
    {
        try
        {
            var transactions = await _transactionService.GetTransactionsByAccountIdAsync(accountId);
            return Ok(transactions);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for account ID {AccountId}", accountId);
            return StatusCode(500, "An error occurred while retrieving transactions");
        }
    }

    [HttpGet("account/{accountId}/date-range")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByDateRange(
        int accountId, 
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        try
        {
            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(accountId, startDate, endDate);
            return Ok(transactions);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions for account ID {AccountId} from {StartDate} to {EndDate}", 
                accountId, startDate, endDate);
            return StatusCode(500, "An error occurred while retrieving transactions");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Transaction>> CreateTransaction(Transaction transaction)
    {
        try
        {
            var createdTransaction = await _transactionService.CreateTransactionAsync(transaction);
            return CreatedAtAction(nameof(GetTransactionById), new { id = createdTransaction.Id }, createdTransaction);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction for account ID {AccountId}", transaction.AccountId);
            return StatusCode(500, "An error occurred while creating the transaction");
        }
    }
}