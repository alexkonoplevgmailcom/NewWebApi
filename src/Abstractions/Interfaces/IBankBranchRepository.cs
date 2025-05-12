using Abstractions.DTO;

namespace Abstractions.Interfaces;

public interface IBankBranchRepository
{
    Task<IEnumerable<BankBranch>> GetAllBranchesAsync();
    Task<BankBranch?> GetBranchByIdAsync(int id);
    Task<IEnumerable<BankBranch>> GetBranchesByBankIdAsync(int bankId);
    Task<BankBranch> CreateBranchAsync(BankBranch branch);
    Task<bool> UpdateBranchAsync(int id, BankBranch branch);
    Task<bool> DeleteBranchAsync(int id);
}