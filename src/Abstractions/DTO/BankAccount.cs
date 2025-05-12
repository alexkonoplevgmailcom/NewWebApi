namespace Abstractions.DTO;

public class BankAccount
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public AccountType Type { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
    public int BankId { get; set; }
    public int BranchId { get; set; }
}

