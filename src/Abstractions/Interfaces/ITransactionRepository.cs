using System.Threading.Tasks;
using System.Collections.Generic;
using Abstractions.DTO;

namespace Abstractions.Interfaces;

public interface ITransactionRepository
{
    Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(int accountId);
    Task<Transaction?> GetTransactionByIdAsync(int transactionId);
    Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(int accountId, DateTime startDate, DateTime endDate);
    Task<Transaction> CreateTransactionAsync(Transaction transaction);
}