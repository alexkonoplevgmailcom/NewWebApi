using Abstractions.DTO;

namespace BFB.DataAccess.DB2.Entities;

public class Account
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public int Type { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
    public int BankId { get; set; }
    public int BranchId { get; set; }

    public BankAccount ToBankAccount()
    {
        return new BankAccount
        {
            Id = Id,
            AccountNumber = AccountNumber,
            OwnerName = OwnerName,
            Balance = Balance,
            Type = MapAccountType(Type),
            CreatedDate = CreatedDate,
            IsActive = IsActive,
            BankId = BankId,
            BranchId = BranchId
        };
    }

    public static Account FromBankAccount(BankAccount bankAccount)
    {
        return new Account
        {
            Id = bankAccount.Id,
            AccountNumber = bankAccount.AccountNumber,
            OwnerName = bankAccount.OwnerName,
            Balance = bankAccount.Balance,
            Type = (int)bankAccount.Type,
            CreatedDate = bankAccount.CreatedDate,
            IsActive = bankAccount.IsActive,
            BankId = bankAccount.BankId,
            BranchId = bankAccount.BranchId
        };
    }
    
    private static Abstractions.DTO.AccountType MapAccountType(int type)
    {
        return type switch
        {
            0 => Abstractions.DTO.AccountType.Checking,
            1 => Abstractions.DTO.AccountType.Savings,
            2 => Abstractions.DTO.AccountType.Investment,
            3 => Abstractions.DTO.AccountType.Loan,
            4 => Abstractions.DTO.AccountType.CreditCard,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}