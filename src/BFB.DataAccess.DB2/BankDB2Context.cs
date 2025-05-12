using IBM.Data.DB2.Core;
using Microsoft.Extensions.Configuration;

namespace BFB.DataAccess.DB2;

public class BankDB2Context
{
    private readonly string _connectionString;

    public BankDB2Context(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DB2Connection") ?? 
            "Server=localhost:50000;Database=BankDB;UID=db2inst1;PWD=db2inst1-pwd;";
    }

    public DB2Connection CreateConnection()
    {
        var connection = new DB2Connection(_connectionString);
        return connection;
    }

    public async Task InitializeDatabaseAsync()
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();

        // Create Customers table if it doesn't exist
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS CUSTOMERS (
                    ID INT NOT NULL GENERATED ALWAYS AS IDENTITY (START WITH 1, INCREMENT BY 1),
                    FIRST_NAME VARCHAR(50) NOT NULL,
                    LAST_NAME VARCHAR(50) NOT NULL,
                    EMAIL VARCHAR(100) NOT NULL,
                    PHONE_NUMBER VARCHAR(20) NOT NULL,
                    ADDRESS VARCHAR(200) NOT NULL,
                    CITY VARCHAR(50) NOT NULL,
                    STATE VARCHAR(50) NOT NULL,
                    ZIP_CODE VARCHAR(20) NOT NULL,
                    CREATED_DATE TIMESTAMP NOT NULL,
                    IS_ACTIVE SMALLINT NOT NULL,
                    PRIMARY KEY (ID)
                )";
            await command.ExecuteNonQueryAsync();
        }

        // Create CustomerAccounts table if it doesn't exist
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS CUSTOMER_ACCOUNTS (
                    ID INT NOT NULL GENERATED ALWAYS AS IDENTITY (START WITH 1, INCREMENT BY 1),
                    CUSTOMER_ID INT NOT NULL,
                    ACCOUNT_NUMBER VARCHAR(50) NOT NULL,
                    BALANCE DECIMAL(18, 2) NOT NULL,
                    TYPE INT NOT NULL,
                    CREATED_DATE TIMESTAMP NOT NULL,
                    IS_ACTIVE SMALLINT NOT NULL,
                    PRIMARY KEY (ID),
                    FOREIGN KEY (CUSTOMER_ID) REFERENCES CUSTOMERS(ID)
                )";
            await command.ExecuteNonQueryAsync();
        }
    }
}