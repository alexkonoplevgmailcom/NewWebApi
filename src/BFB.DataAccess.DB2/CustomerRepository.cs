using Abstractions.DTO;
using Abstractions.Interfaces;
using BFB.DataAccess.DB2.Entities;
using IBM.Data.DB2.Core;
using Microsoft.Extensions.Logging;
using Polly;

namespace BFB.DataAccess.DB2;

public class CustomerRepository : ICustomerRepository
{
    private readonly BankDB2Context _context;
    private readonly RetryPolicyService _retryPolicyService;
    private readonly ILogger<CustomerRepository> _logger;

    public CustomerRepository(
        BankDB2Context context,
        RetryPolicyService retryPolicyService,
        ILogger<CustomerRepository> logger)
    {
        _context = context;
        _retryPolicyService = retryPolicyService;
        _logger = logger;
    }

    public async Task<Abstractions.DTO.Customer> CreateCustomerAsync(Abstractions.DTO.Customer customer)
    {
        var policy = _retryPolicyService.GetAsyncRetryPolicy();
        var entity = MapToEntity(customer);

        return await policy.ExecuteAsync(async () =>
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO CUSTOMERS (
                    FIRST_NAME, LAST_NAME, EMAIL, PHONE_NUMBER, 
                    ADDRESS, CITY, STATE, ZIP_CODE, CREATED_DATE, IS_ACTIVE
                ) VALUES (
                    @FirstName, @LastName, @Email, @PhoneNumber, 
                    @Address, @City, @State, @ZipCode, @CreatedDate, @IsActive
                )
                SELECT * FROM CUSTOMERS WHERE ID = IDENTITY_VAL_LOCAL()";

            command.Parameters.Add(new DB2Parameter("@FirstName", entity.FirstName));
            command.Parameters.Add(new DB2Parameter("@LastName", entity.LastName));
            command.Parameters.Add(new DB2Parameter("@Email", entity.Email));
            command.Parameters.Add(new DB2Parameter("@PhoneNumber", entity.PhoneNumber));
            command.Parameters.Add(new DB2Parameter("@Address", entity.Address));
            command.Parameters.Add(new DB2Parameter("@City", entity.City));
            command.Parameters.Add(new DB2Parameter("@State", entity.State));
            command.Parameters.Add(new DB2Parameter("@ZipCode", entity.ZipCode));
            command.Parameters.Add(new DB2Parameter("@CreatedDate", entity.CreatedDate));
            command.Parameters.Add(new DB2Parameter("@IsActive", entity.IsActive ? 1 : 0));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                entity.Id = reader.GetInt32(reader.GetOrdinal("ID"));
                return MapToDto(entity);
            }

            throw new Exception("Failed to create customer");
        });
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        var policy = _retryPolicyService.GetAsyncRetryPolicy();

        return await policy.ExecuteAsync(async () =>
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM CUSTOMERS WHERE ID = @Id";
            command.Parameters.Add(new DB2Parameter("@Id", id));

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        });
    }

    public async Task<IEnumerable<Abstractions.DTO.Customer>> GetAllCustomersAsync()
    {
        var policy = _retryPolicyService.GetAsyncRetryPolicy();

        return await policy.ExecuteAsync(async () =>
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT ID, FIRST_NAME, LAST_NAME, EMAIL, PHONE_NUMBER, 
                       ADDRESS, CITY, STATE, ZIP_CODE, CREATED_DATE, IS_ACTIVE
                FROM CUSTOMERS";

            using var reader = await command.ExecuteReaderAsync();
            var customers = new List<Abstractions.DTO.Customer>();

            while (await reader.ReadAsync())
            {
                var entity = new Entities.Customer
                {
                    Id = reader.GetInt32(reader.GetOrdinal("ID")),
                    FirstName = reader.GetString(reader.GetOrdinal("FIRST_NAME")),
                    LastName = reader.GetString(reader.GetOrdinal("LAST_NAME")),
                    Email = reader.GetString(reader.GetOrdinal("EMAIL")),
                    PhoneNumber = reader.GetString(reader.GetOrdinal("PHONE_NUMBER")),
                    Address = reader.GetString(reader.GetOrdinal("ADDRESS")),
                    City = reader.GetString(reader.GetOrdinal("CITY")),
                    State = reader.GetString(reader.GetOrdinal("STATE")),
                    ZipCode = reader.GetString(reader.GetOrdinal("ZIP_CODE")),
                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CREATED_DATE")),
                    IsActive = reader.GetInt32(reader.GetOrdinal("IS_ACTIVE")) == 1
                };

                customers.Add(MapToDto(entity));
            }

            return customers;
        });
    }

    public async Task<Abstractions.DTO.Customer?> GetCustomerByIdAsync(int id)
    {
        var policy = _retryPolicyService.GetAsyncRetryPolicy();

        return await policy.ExecuteAsync(async () =>
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT ID, FIRST_NAME, LAST_NAME, EMAIL, PHONE_NUMBER, 
                       ADDRESS, CITY, STATE, ZIP_CODE, CREATED_DATE, IS_ACTIVE
                FROM CUSTOMERS
                WHERE ID = @Id";
            
            command.Parameters.Add(new DB2Parameter("@Id", id));

            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                var entity = new Entities.Customer
                {
                    Id = reader.GetInt32(reader.GetOrdinal("ID")),
                    FirstName = reader.GetString(reader.GetOrdinal("FIRST_NAME")),
                    LastName = reader.GetString(reader.GetOrdinal("LAST_NAME")),
                    Email = reader.GetString(reader.GetOrdinal("EMAIL")),
                    PhoneNumber = reader.GetString(reader.GetOrdinal("PHONE_NUMBER")),
                    Address = reader.GetString(reader.GetOrdinal("ADDRESS")),
                    City = reader.GetString(reader.GetOrdinal("CITY")),
                    State = reader.GetString(reader.GetOrdinal("STATE")),
                    ZipCode = reader.GetString(reader.GetOrdinal("ZIP_CODE")),
                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CREATED_DATE")),
                    IsActive = reader.GetInt32(reader.GetOrdinal("IS_ACTIVE")) == 1
                };

                return MapToDto(entity);
            }

            return null;
        });
    }

    public async Task<bool> UpdateCustomerAsync(int id, Abstractions.DTO.Customer customer)
    {
        var policy = _retryPolicyService.GetAsyncRetryPolicy();
        var entity = MapToEntity(customer);

        return await policy.ExecuteAsync(async () =>
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE CUSTOMERS
                SET FIRST_NAME = @FirstName,
                    LAST_NAME = @LastName,
                    EMAIL = @Email,
                    PHONE_NUMBER = @PhoneNumber,
                    ADDRESS = @Address,
                    CITY = @City,
                    STATE = @State,
                    ZIP_CODE = @ZipCode,
                    IS_ACTIVE = @IsActive
                WHERE ID = @Id";

            command.Parameters.Add(new DB2Parameter("@FirstName", entity.FirstName));
            command.Parameters.Add(new DB2Parameter("@LastName", entity.LastName));
            command.Parameters.Add(new DB2Parameter("@Email", entity.Email));
            command.Parameters.Add(new DB2Parameter("@PhoneNumber", entity.PhoneNumber));
            command.Parameters.Add(new DB2Parameter("@Address", entity.Address));
            command.Parameters.Add(new DB2Parameter("@City", entity.City));
            command.Parameters.Add(new DB2Parameter("@State", entity.State));
            command.Parameters.Add(new DB2Parameter("@ZipCode", entity.ZipCode));
            command.Parameters.Add(new DB2Parameter("@IsActive", entity.IsActive ? 1 : 0));
            command.Parameters.Add(new DB2Parameter("@Id", id));

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        });
    }
    
    // Mapping methods
    private static Abstractions.DTO.Customer MapToDto(Entities.Customer entity)
    {
        return new Abstractions.DTO.Customer
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            PhoneNumber = entity.PhoneNumber,
            Address = entity.Address,
            City = entity.City,
            State = entity.State,
            ZipCode = entity.ZipCode,
            CreatedDate = entity.CreatedDate,
            IsActive = entity.IsActive
        };
    }

    private static Entities.Customer MapToEntity(Abstractions.DTO.Customer dto)
    {
        return new Entities.Customer
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            ZipCode = dto.ZipCode,
            CreatedDate = dto.CreatedDate,
            IsActive = dto.IsActive
        };
    }
}