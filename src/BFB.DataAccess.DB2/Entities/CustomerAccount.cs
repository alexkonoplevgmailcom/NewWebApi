using System;

namespace BFB.DataAccess.DB2.Entities;

public class CustomerAccount
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public AccountType Type { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
}

public enum AccountType
{
    Checking,
    Savings,
    Investment,
    Loan,
    CreditCard
}