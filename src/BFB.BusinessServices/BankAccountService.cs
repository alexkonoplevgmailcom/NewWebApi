using Abstractions.DTO;
using Abstractions.Interfaces;
using Microsoft.Extensions.Logging;

namespace BFB.BusinessServices;

public class BankAccountService : IBankAccountService
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IBankBranchRepository _bankBranchRepository;
    private readonly ILogger<BankAccountService> _logger;

    public BankAccountService(
        IBankAccountRepository bankAccountRepository,
        IBankBranchRepository bankBranchRepository,
        ILogger<BankAccountService> logger)
    {
        _bankAccountRepository = bankAccountRepository ?? throw new ArgumentNullException(nameof(bankAccountRepository));
        _bankBranchRepository = bankBranchRepository ?? throw new ArgumentNullException(nameof(bankBranchRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<BankAccount>> GetAllBankAccountsAsync()
    {
        var accounts = await _bankAccountRepository.GetAllBankAccountsAsync();
        
        // Enhance accounts with branch information
        await EnrichAccountsWithBranchInformation(accounts);
        
        return accounts;
    }

    public async Task<BankAccount?> GetBankAccountByIdAsync(int id)
    {
        var account = await _bankAccountRepository.GetBankAccountByIdAsync(id);
        
        if (account != null)
        {
            // Enhance the account with branch information
            await EnrichAccountWithBranchInformation(account);
        }
        
        return account;
    }

    public async Task<BankAccount> CreateBankAccountAsync(BankAccount bankAccountDto)
    {
        // Add business validation
        if (string.IsNullOrWhiteSpace(bankAccountDto.AccountNumber))
        {
            throw new ArgumentException("Account number cannot be empty", nameof(bankAccountDto));
        }

        if (string.IsNullOrWhiteSpace(bankAccountDto.OwnerName))
        {
            throw new ArgumentException("Owner name cannot be empty", nameof(bankAccountDto));
        }

        if (bankAccountDto.Balance < 0)
        {
            throw new ArgumentException("Initial balance cannot be negative", nameof(bankAccountDto));
        }

        // Validate BranchId if provided
        if (bankAccountDto.BranchId > 0)
        {
            var branch = await _bankBranchRepository.GetBranchByIdAsync(bankAccountDto.BranchId);
            if (branch == null)
            {
                throw new ArgumentException($"Branch with ID {bankAccountDto.BranchId} does not exist", nameof(bankAccountDto));
            }
            
            // Ensure BankId matches the branch's BankId
            bankAccountDto.BankId = branch.BankId;
        }

        // Set default values
        bankAccountDto.CreatedDate = DateTime.UtcNow;
        bankAccountDto.IsActive = true;

        var createdAccount = await _bankAccountRepository.CreateBankAccountAsync(bankAccountDto);
        
        // Enhance the newly created account with branch information
        await EnrichAccountWithBranchInformation(createdAccount);
        
        return createdAccount;
    }

    public async Task<bool> UpdateBankAccountAsync(int id, BankAccount bankAccountDto)
    {
        // Verify that the account exists
        var existingAccount = await _bankAccountRepository.GetBankAccountByIdAsync(id);
        if (existingAccount == null)
        {
            return false;
        }

        // Add business validation
        if (string.IsNullOrWhiteSpace(bankAccountDto.AccountNumber))
        {
            throw new ArgumentException("Account number cannot be empty", nameof(bankAccountDto));
        }

        if (string.IsNullOrWhiteSpace(bankAccountDto.OwnerName))
        {
            throw new ArgumentException("Owner name cannot be empty", nameof(bankAccountDto));
        }

        // Validate BranchId if it's changing
        if (bankAccountDto.BranchId > 0 && bankAccountDto.BranchId != existingAccount.BranchId)
        {
            var branch = await _bankBranchRepository.GetBranchByIdAsync(bankAccountDto.BranchId);
            if (branch == null)
            {
                throw new ArgumentException($"Branch with ID {bankAccountDto.BranchId} does not exist", nameof(bankAccountDto));
            }
            
            // Ensure BankId matches the branch's BankId
            bankAccountDto.BankId = branch.BankId;
        }

        // Preserve creation date from the existing account
        bankAccountDto.CreatedDate = existingAccount.CreatedDate;

        return await _bankAccountRepository.UpdateBankAccountAsync(id, bankAccountDto);
    }

    public async Task<bool> DeleteBankAccountAsync(int id)
    {
        return await _bankAccountRepository.DeleteBankAccountAsync(id);
    }
    
    // Helper methods to enrich account information with branch details
    private async Task EnrichAccountWithBranchInformation(BankAccount account)
    {
        if (account.BranchId <= 0)
        {
            return;
        }
        
        try
        {
            var branch = await _bankBranchRepository.GetBranchByIdAsync(account.BranchId);
            if (branch != null)
            {
                // We could extend the BankAccount DTO to include branch details,
                // but for now we're just ensuring the BankId is consistent
                if (account.BankId != branch.BankId)
                {
                    _logger.LogWarning("BankId mismatch for account {AccountId}: Account BankId={AccountBankId}, Branch BankId={BranchBankId}",
                        account.Id, account.BankId, branch.BankId);
                    
                    account.BankId = branch.BankId;
                }
            }
            else
            {
                _logger.LogWarning("Branch with ID {BranchId} not found for account {AccountId}", account.BranchId, account.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving branch information for account {AccountId}", account.Id);
        }
    }
    
    private async Task EnrichAccountsWithBranchInformation(IEnumerable<BankAccount> accounts)
    {
        foreach (var account in accounts)
        {
            await EnrichAccountWithBranchInformation(account);
        }
    }
}