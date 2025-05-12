using Abstractions.DTO;
using Abstractions.Interfaces;
using BFB.DataAccess.DB2.Entities;
using IBM.Data.DB2.Core;
using Microsoft.Extensions.Logging;
using Polly;

namespace BFB.DataAccess.DB2;

public class CustomerAccountRepository : ICustomerAccountRepository
{
    private readonly BankDB2Context _context;
    private readonly RetryPolicyService _retryPolicyService;
    private readonly ILogger<CustomerAccountRepository> _logger;

    public CustomerAccountRepository(
        BankDB2Context context,
        RetryPolicyService retryPolicyService,
        ILogger<CustomerAccountRepository> logger)
    {
        _context = context;
        _retryPolicyService = retryPolicyService;
        _logger = logger;
    }

    public async Task<Abstractions.DTO.CustomerAccount> CreateAccountAsync(Abstractions.DTO.CustomerAccount account)
    {
        var policy = _retryPolicyService.GetAsyncRetryPolicy();
        var entity = MapToEntity(account);

        return await policy.ExecuteAsync(async () =>
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO CUSTOMER_ACCOUNTS (
                    CUSTOMER_ID, ACCOUNT_NUMBER, BALANCE, TYPE, 
                    CREATED_DATE, IS_ACTIVE
                ) VALUES (
                    @CustomerId, @AccountNumber, @Balance, @Type, 
                    @CreatedDate, @IsActive
                )
                SELECT * FROM CUSTOMER_ACCOUNTS WHERE ID = IDENTITY_VAL_LOCAL()";

            command.Parameters.Add(new DB2Parameter("@CustomerId", entity.CustomerId));
            command.Parameters.Add(new DB2Parameter("@AccountNumber", entity.AccountNumber));
            command.Parameters.Add(new DB2Parameter("@Balance", entity.Balance));
            command.Parameters.Add(new DB2Parameter("@Type", (int)entity.Type));
            command.Parameters.Add(new DB2Parameter("@CreatedDate", entity.CreatedDate));
            command.Parameters.Add(new DB2Parameter("@IsActive", entity.IsActive ? 1 : 0));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                entity.Id = reader.GetInt32(reader.GetOrdinal("ID"));
                return MapToDto(entity);
            }

            throw new Exception("Failed to create customer account");
        });
    }

    public async Task<bool> DeleteAccountAsync(int id)
    {
        var policy = _retryPolicyService.GetAsyncRetryPolicy();

        return await policy.ExecuteAsync(async () =>
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM CUSTOMER_ACCOUNTS WHERE ID = @Id";
            command.Parameters.Add(new DB2Parameter("@Id", id));

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        });
    }

    public async Task<Abstractions.DTO.CustomerAccount?> GetAccountByIdAsync(int id)
    {
        var policy = _retryPolicyService.GetAsyncRetryPolicy();

        return await policy.ExecuteAsync(async () =>
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT ID, CUSTOMER_ID, ACCOUNT_NUMBER, BALANCE, TYPE, 
                       CREATED_DATE, IS_ACTIVE
                FROM CUSTOMER_ACCOUNTS
                WHERE ID = @Id";
            
            command.Parameters.Add(new DB2Parameter("@Id", id));

            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                var entity = new Entities.CustomerAccount
                {
                    Id = reader.GetInt32(reader.GetOrdinal("ID")),
                    CustomerId = reader.GetInt32(reader.GetOrdinal("CUSTOMER_ID")),
                    AccountNumber = reader.GetString(reader.GetOrdinal("ACCOUNT_NUMBER")),
                    Balance = reader.GetDecimal(reader.GetOrdinal("BALANCE")),
                    Type = (Entities.AccountType)reader.GetInt32(reader.GetOrdinal("TYPE")),
                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CREATED_DATE")),
                    IsActive = reader.GetInt32(reader.GetOrdinal("IS_ACTIVE")) == 1
                };

                return MapToDto(entity);
            }

            return null;
        });
    }

    public async Task<IEnumerable<Abstractions.DTO.CustomerAccount>> GetAccountsByCustomerIdAsync(int customerId)
    {
        var policy = _retryPolicyService.GetAsyncRetryPolicy();

        return await policy.ExecuteAsync(async () =>
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT ID, CUSTOMER_ID, ACCOUNT_NUMBER, BALANCE, TYPE, 
                       CREATED_DATE, IS_ACTIVE
                FROM CUSTOMER_ACCOUNTS
                WHERE CUSTOMER_ID = @CustomerId";

            command.Parameters.Add(new DB2Parameter("@CustomerId", customerId));

            using var reader = await command.ExecuteReaderAsync();
            var accounts = new List<Abstractions.DTO.CustomerAccount>();

            while (await reader.ReadAsync())
            {
                var entity = new Entities.CustomerAccount
                {
                    Id = reader.GetInt32(reader.GetOrdinal("ID")),
                    CustomerId = reader.GetInt32(reader.GetOrdinal("CUSTOMER_ID")),
                    AccountNumber = reader.GetString(reader.GetOrdinal("ACCOUNT_NUMBER")),
                    Balance = reader.GetDecimal(reader.GetOrdinal("BALANCE")),
                    Type = (Entities.AccountType)reader.GetInt32(reader.GetOrdinal("TYPE")),
                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CREATED_DATE")),
                    IsActive = reader.GetInt32(reader.GetOrdinal("IS_ACTIVE")) == 1
                };

                accounts.Add(MapToDto(entity));
            }

            return accounts;
        });
    }

    public async Task<IEnumerable<Abstractions.DTO.CustomerAccount>> GetAllAccountsAsync()
    {
        var policy = _retryPolicyService.GetAsyncRetryPolicy();

        return await policy.ExecuteAsync(async () =>
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT ID, CUSTOMER_ID, ACCOUNT_NUMBER, BALANCE, TYPE, 
                       CREATED_DATE, IS_ACTIVE
                FROM CUSTOMER_ACCOUNTS";

            using var reader = await command.ExecuteReaderAsync();
            var accounts = new List<Abstractions.DTO.CustomerAccount>();

            while (await reader.ReadAsync())
            {
                var entity = new Entities.CustomerAccount
                {
                    Id = reader.GetInt32(reader.GetOrdinal("ID")),
                    CustomerId = reader.GetInt32(reader.GetOrdinal("CUSTOMER_ID")),
                    AccountNumber = reader.GetString(reader.GetOrdinal("ACCOUNT_NUMBER")),
                    Balance = reader.GetDecimal(reader.GetOrdinal("BALANCE")),
                    Type = (Entities.AccountType)reader.GetInt32(reader.GetOrdinal("TYPE")),
                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CREATED_DATE")),
                    IsActive = reader.GetInt32(reader.GetOrdinal("IS_ACTIVE")) == 1
                };

                accounts.Add(MapToDto(entity));
            }

            return accounts;
        });
    }

    public async Task<bool> UpdateAccountAsync(int id, Abstractions.DTO.CustomerAccount account)
    {
        var policy = _retryPolicyService.GetAsyncRetryPolicy();
        var entity = MapToEntity(account);

        return await policy.ExecuteAsync(async () =>
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE CUSTOMER_ACCOUNTS
                SET CUSTOMER_ID = @CustomerId,
                    ACCOUNT_NUMBER = @AccountNumber,
                    BALANCE = @Balance,
                    TYPE = @Type,
                    IS_ACTIVE = @IsActive
                WHERE ID = @Id";

            command.Parameters.Add(new DB2Parameter("@CustomerId", entity.CustomerId));
            command.Parameters.Add(new DB2Parameter("@AccountNumber", entity.AccountNumber));
            command.Parameters.Add(new DB2Parameter("@Balance", entity.Balance));
            command.Parameters.Add(new DB2Parameter("@Type", (int)entity.Type));
            command.Parameters.Add(new DB2Parameter("@IsActive", entity.IsActive ? 1 : 0));
            command.Parameters.Add(new DB2Parameter("@Id", id));

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        });
    }
    
    // Mapping methods
    private static Abstractions.DTO.CustomerAccount MapToDto(Entities.CustomerAccount entity)
    {
        return new Abstractions.DTO.CustomerAccount
        {
            Id = entity.Id,
            CustomerId = entity.CustomerId,
            AccountNumber = entity.AccountNumber,
            Balance = entity.Balance,
            Type = MapAccountType(entity.Type),
            CreatedDate = entity.CreatedDate,
            IsActive = entity.IsActive
        };
    }

    private static Entities.CustomerAccount MapToEntity(Abstractions.DTO.CustomerAccount dto)
    {
        return new Entities.CustomerAccount
        {
            Id = dto.Id,
            CustomerId = dto.CustomerId,
            AccountNumber = dto.AccountNumber,
            Balance = dto.Balance,
            Type = MapAccountType(dto.Type),
            CreatedDate = dto.CreatedDate,
            IsActive = dto.IsActive
        };
    }

    private static Abstractions.DTO.AccountType MapAccountType(Entities.AccountType type)
    {
        return type switch
        {
            Entities.AccountType.Checking => Abstractions.DTO.AccountType.Checking,
            Entities.AccountType.Savings => Abstractions.DTO.AccountType.Savings,
            Entities.AccountType.Investment => Abstractions.DTO.AccountType.Investment,
            Entities.AccountType.Loan => Abstractions.DTO.AccountType.Loan,
            Entities.AccountType.CreditCard => Abstractions.DTO.AccountType.CreditCard,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private static Entities.AccountType MapAccountType(Abstractions.DTO.AccountType type)
    {
        return type switch
        {
            Abstractions.DTO.AccountType.Checking => Entities.AccountType.Checking,
            Abstractions.DTO.AccountType.Savings => Entities.AccountType.Savings,
            Abstractions.DTO.AccountType.Investment => Entities.AccountType.Investment,
            Abstractions.DTO.AccountType.Loan => Entities.AccountType.Loan,
            Abstractions.DTO.AccountType.CreditCard => Entities.AccountType.CreditCard,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}