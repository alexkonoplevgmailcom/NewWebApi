using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BFB.DataAccess.Mongo.Entities;

public class Branch
{
    [BsonId]
    public int Id { get; set; }
    
    [BsonElement("bankId")]
    public int BankId { get; set; }
    
    [BsonElement("branchName")]
    public string BranchName { get; set; } = string.Empty;
    
    [BsonElement("address")]
    public string Address { get; set; } = string.Empty;
    
    [BsonElement("city")]
    public string City { get; set; } = string.Empty;
    
    [BsonElement("state")]
    public string State { get; set; } = string.Empty;
    
    [BsonElement("zipCode")]
    public string ZipCode { get; set; } = string.Empty;
    
    [BsonElement("phoneNumber")]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [BsonElement("isActive")]
    public bool IsActive { get; set; }
    
    [BsonElement("createdDate")]
    public DateTime CreatedDate { get; set; }
}