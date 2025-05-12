using Abstractions.DTO;

namespace Abstractions.Interfaces;

public interface IBankAccountRepository
{
    Task<IEnumerable<BankAccount>> GetAllBankAccountsAsync();
    Task<BankAccount?> GetBankAccountByIdAsync(int id);
    Task<BankAccount> CreateBankAccountAsync(BankAccount bankAccountDto);
    Task<bool> UpdateBankAccountAsync(int id, BankAccount bankAccountDto);
    Task<bool> DeleteBankAccountAsync(int id);
}