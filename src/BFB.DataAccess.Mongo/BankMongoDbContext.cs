using BFB.DataAccess.Mongo.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BFB.DataAccess.Mongo;

public class BankMongoDbContext
{
    private readonly IMongoDatabase _database;

    public BankMongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoConnection") ?? "mongodb://localhost:27017";
        var databaseName = configuration["MongoDB:DatabaseName"] ?? "BankDatabase";
        var useAuthentication = configuration.GetValue<bool>("MongoDB:UseAuthentication");
        
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
        
        // Ensure collections exist
        try
        {
            if (!CollectionExists(BranchCollectionName))
            {
                _database.CreateCollection(BranchCollectionName);
            }
        }
        catch (MongoDB.Driver.MongoCommandException ex) when (ex.Message.Contains("requires authentication"))
        {
            // If authentication failed but we're in development, we can just log and continue
            // The collection will be created when the first document is inserted
            Console.WriteLine("MongoDB authentication failed: " + ex.Message);
            Console.WriteLine("Collection will be created automatically when first document is inserted.");
        }
    }

    public IMongoCollection<Branch> Branches => _database.GetCollection<Branch>(BranchCollectionName);
    
    private const string BranchCollectionName = "branches";
    
    private bool CollectionExists(string collectionName)
    {
        try
        {
            var filter = new BsonDocument("name", collectionName);
            var collections = _database.ListCollections(new ListCollectionsOptions { Filter = filter });
            return collections.Any();
        }
        catch
        {
            // If we can't check if the collection exists (e.g., due to auth issues),
            // we'll assume it doesn't and let MongoDB create it when needed
            return false;
        }
    }
}