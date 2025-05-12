using Abstractions.DTO;
using Abstractions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BFB.Template.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankBranchesController : ControllerBase
{
    private readonly IBankBranchRepository _bankBranchRepository;
    private readonly ILogger<BankBranchesController> _logger;

    public BankBranchesController(IBankBranchRepository bankBranchRepository, ILogger<BankBranchesController> logger)
    {
        _bankBranchRepository = bankBranchRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BankBranch>>> GetAllBranches()
    {
        try
        {
            var branches = await _bankBranchRepository.GetAllBranchesAsync();
            return Ok(branches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all bank branches");
            return StatusCode(500, "An error occurred while retrieving bank branches");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BankBranch>> GetBranchById(int id)
    {
        try
        {
            var branch = await _bankBranchRepository.GetBranchByIdAsync(id);
            if (branch == null)
            {
                return NotFound($"Branch with ID {id} not found");
            }
            return Ok(branch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving branch {Id}", id);
            return StatusCode(500, $"An error occurred while retrieving branch {id}");
        }
    }

    [HttpGet("bank/{bankId}")]
    public async Task<ActionResult<IEnumerable<BankBranch>>> GetBranchesByBankId(int bankId)
    {
        try
        {
            var branches = await _bankBranchRepository.GetBranchesByBankIdAsync(bankId);
            return Ok(branches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving branches for bank {BankId}", bankId);
            return StatusCode(500, $"An error occurred while retrieving branches for bank {bankId}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<BankBranch>> CreateBranch(BankBranch branch)
    {
        try
        {
            var createdBranch = await _bankBranchRepository.CreateBranchAsync(branch);
            return CreatedAtAction(nameof(GetBranchById), new { id = createdBranch.Id }, createdBranch);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating branch");
            return StatusCode(500, "An error occurred while creating the branch");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBranch(int id, BankBranch branch)
    {
        try
        {
            if (id != branch.Id)
            {
                return BadRequest("ID in route does not match ID in branch object");
            }

            var success = await _bankBranchRepository.UpdateBranchAsync(id, branch);
            if (!success)
            {
                return NotFound($"Branch with ID {id} not found");
            }
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating branch {Id}", id);
            return StatusCode(500, $"An error occurred while updating branch {id}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBranch(int id)
    {
        try
        {
            var success = await _bankBranchRepository.DeleteBranchAsync(id);
            if (!success)
            {
                return NotFound($"Branch with ID {id} not found");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting branch {Id}", id);
            return StatusCode(500, $"An error occurred while deleting branch {id}");
        }
    }
}