using Abstractions.DTO;

namespace BFB.DataAccess.DB2.Entities;

public class Branch
{
    public int Id { get; set; }
    public int BankId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }

    public BankBranch ToBankBranch()
    {
        return new BankBranch
        {
            Id = Id,
            BankId = BankId,
            BranchName = BranchName,
            Address = Address,
            City = City,
            State = State,
            ZipCode = ZipCode,
            PhoneNumber = PhoneNumber,
            IsActive = IsActive,
            CreatedDate = CreatedDate
        };
    }

    public static Branch FromBankBranch(BankBranch bankBranch)
    {
        return new Branch
        {
            Id = bankBranch.Id,
            BankId = bankBranch.BankId,
            BranchName = bankBranch.BranchName,
            Address = bankBranch.Address,
            City = bankBranch.City,
            State = bankBranch.State,
            ZipCode = bankBranch.ZipCode,
            PhoneNumber = bankBranch.PhoneNumber,
            IsActive = bankBranch.IsActive,
            CreatedDate = bankBranch.CreatedDate
        };
    }
}