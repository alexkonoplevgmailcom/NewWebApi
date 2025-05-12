using System.Threading.Tasks;
using System.Collections.Generic;

namespace Abstractions.DTO;

public class Transaction
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public decimal BalanceAfterTransaction { get; set; }
    public string Reference { get; set; } = string.Empty;
}