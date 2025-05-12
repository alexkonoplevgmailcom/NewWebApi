using Abstractions.DTO;
using Abstractions.Interfaces;
using Microsoft.Extensions.Logging;

namespace BFB.BusinessServices;

public class BankAccountService : IBankAccountService
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly ILogger<BankAccountService> _logger;

    public BankAccountService(
        IBankAccountRepository bankAccountRepository,
        ILogger<BankAccountService> logger)
    {
        _bankAccountRepository = bankAccountRepository ?? throw new ArgumentNullException(nameof(bankAccountRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<BankAccount>> GetAllBankAccountsAsync()
    {
        return await _bankAccountRepository.GetAllBankAccountsAsync();
    }

    public async Task<BankAccount?> GetBankAccountByIdAsync(int id)
    {
        return await _bankAccountRepository.GetBankAccountByIdAsync(id);
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

        // Set default values
        bankAccountDto.CreatedDate = DateTime.UtcNow;
        bankAccountDto.IsActive = true;

        return await _bankAccountRepository.CreateBankAccountAsync(bankAccountDto);
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

        // Preserve creation date from the existing account
        bankAccountDto.CreatedDate = existingAccount.CreatedDate;

        return await _bankAccountRepository.UpdateBankAccountAsync(id, bankAccountDto);
    }

    public async Task<bool> DeleteBankAccountAsync(int id)
    {
        return await _bankAccountRepository.DeleteBankAccountAsync(id);
    }
}