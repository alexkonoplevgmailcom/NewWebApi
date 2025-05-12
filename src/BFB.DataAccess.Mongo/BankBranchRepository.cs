using Abstractions.DTO;
using Abstractions.Interfaces;
using BFB.DataAccess.Mongo.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace BFB.DataAccess.Mongo;

public class BankBranchRepository : IBankBranchRepository
{
    private readonly BankMongoDbContext _mongoContext;
    private readonly IMemoryCache _memoryCache;
    private readonly RetryPolicyService _retryPolicyService;
    private readonly ILogger<BankBranchRepository> _logger;
    
    // Cache keys
    private const string AllBranchesCacheKey = "AllBranches";
    private const string BranchByIdCacheKeyPrefix = "Branch_";
    private const string BranchesByBankIdCachePrefix = "BranchesByBank_";
    
    // Cache expiration time
    private static readonly TimeSpan CacheExpirationTime = TimeSpan.FromMinutes(10);
    
    public BankBranchRepository(
        BankMongoDbContext mongoContext, 
        IMemoryCache memoryCache, 
        RetryPolicyService retryPolicyService,
        ILogger<BankBranchRepository> logger)
    {
        _mongoContext = mongoContext ?? throw new ArgumentNullException(nameof(mongoContext));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _retryPolicyService = retryPolicyService ?? throw new ArgumentNullException(nameof(retryPolicyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<BankBranch>> GetAllBranchesAsync()
    {
        // Try to get from cache first
        if (_memoryCache.TryGetValue(AllBranchesCacheKey, out IEnumerable<BankBranch>? cachedBranches) && cachedBranches != null)
        {
            _logger.LogInformation("CACHE HIT: Retrieved all branches from cache");
            return cachedBranches;
        }
        
        _logger.LogInformation("CACHE MISS: Getting all branches from database");
        
        // If not in cache, get from database with retry policy
        var branches = await _retryPolicyService.ExecuteWithRetryAsync(async () => 
        {
            _logger.LogDebug("Retrieving all branches from MongoDB");
            var branchEntities = await _mongoContext.Branches.Find(Builders<Branch>.Filter.Empty).ToListAsync();
            return branchEntities.Select(MapToDto).ToList();
        }, "GetAllBranches");
        
        // Store in cache
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(CacheExpirationTime);
            
        _memoryCache.Set(AllBranchesCacheKey, branches, cacheEntryOptions);
        _logger.LogInformation("CACHE UPDATE: Stored {Count} branches in cache", branches.Count());
        
        return branches;
    }

    public async Task<BankBranch?> GetBranchByIdAsync(int id)
    {
        string cacheKey = $"{BranchByIdCacheKeyPrefix}{id}";
        
        // Try to get from cache first
        if (_memoryCache.TryGetValue(cacheKey, out BankBranch? cachedBranch) && cachedBranch != null)
        {
            _logger.LogInformation("CACHE HIT: Retrieved branch {Id} from cache", id);
            return cachedBranch;
        }
        
        _logger.LogInformation("CACHE MISS: Getting branch {Id} from database", id);
        
        // If not in cache, get from database with retry policy
        var branch = await _retryPolicyService.ExecuteWithRetryAsync(async () => 
        {
            _logger.LogDebug("Retrieving branch {Id} from MongoDB", id);
            var filter = Builders<Branch>.Filter.Eq(b => b.Id, id);
            var branchEntity = await _mongoContext.Branches.Find(filter).FirstOrDefaultAsync();
            return branchEntity != null ? MapToDto(branchEntity) : null;
        }, $"GetBranchById_{id}");
        
        if (branch == null)
        {
            _logger.LogInformation("Branch with ID {Id} not found", id);
            return null;
        }
        
        // Store in cache
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(CacheExpirationTime);
            
        _memoryCache.Set(cacheKey, branch, cacheEntryOptions);
        _logger.LogInformation("CACHE UPDATE: Stored branch {Id} in cache", id);
        
        return branch;
    }

    public async Task<IEnumerable<BankBranch>> GetBranchesByBankIdAsync(int bankId)
    {
        string cacheKey = $"{BranchesByBankIdCachePrefix}{bankId}";
        
        // Try to get from cache first
        if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<BankBranch>? cachedBranches) && cachedBranches != null)
        {
            _logger.LogInformation("CACHE HIT: Retrieved branches for bank {BankId} from cache", bankId);
            return cachedBranches;
        }
        
        _logger.LogInformation("CACHE MISS: Getting branches for bank {BankId} from database", bankId);
        
        // If not in cache, get from database with retry policy
        var branches = await _retryPolicyService.ExecuteWithRetryAsync(async () => 
        {
            _logger.LogDebug("Retrieving branches for bank {BankId} from MongoDB", bankId);
            var filter = Builders<Branch>.Filter.Eq(b => b.BankId, bankId);
            var branchEntities = await _mongoContext.Branches.Find(filter).ToListAsync();
            return branchEntities.Select(MapToDto).ToList();
        }, $"GetBranchesByBankId_{bankId}");
        
        // Store in cache
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(CacheExpirationTime);
            
        _memoryCache.Set(cacheKey, branches, cacheEntryOptions);
        _logger.LogInformation("CACHE UPDATE: Stored {Count} branches for bank {BankId} in cache", branches.Count(), bankId);
        
        return branches;
    }

    public async Task<BankBranch> CreateBranchAsync(BankBranch branchDto)
    {
        // Ensure we have a valid ID by finding the highest existing ID and incrementing
        var highestId = await _retryPolicyService.ExecuteWithRetryAsync(async () => 
        {
            var branches = await _mongoContext.Branches.Find(Builders<Branch>.Filter.Empty)
                .SortByDescending(b => b.Id)
                .Limit(1)
                .ToListAsync();
                
            return branches.Any() ? branches.First().Id : 0;
        }, "GetHighestBranchId");
        
        var branchEntity = MapToEntity(branchDto);
        branchEntity.Id = highestId + 1;
        branchEntity.CreatedDate = DateTime.UtcNow;
        
        await _retryPolicyService.ExecuteWithRetryAsync(async () => 
        {
            await _mongoContext.Branches.InsertOneAsync(branchEntity);
            return true;
        }, "CreateBranch");
        
        var createdBranch = MapToDto(branchEntity);
        
        // Invalidate caches
        _memoryCache.Remove(AllBranchesCacheKey);
        _memoryCache.Remove($"{BranchesByBankIdCachePrefix}{createdBranch.BankId}");
        _logger.LogInformation("CACHE INVALIDATE: Removed all branches and bank-specific branches from cache after creating branch {Id}", createdBranch.Id);
        
        // Add new item to cache
        string cacheKey = $"{BranchByIdCacheKeyPrefix}{createdBranch.Id}";
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(CacheExpirationTime);
        _memoryCache.Set(cacheKey, createdBranch, cacheEntryOptions);
        _logger.LogInformation("CACHE UPDATE: Stored new branch {Id} in cache", createdBranch.Id);
        
        return createdBranch;
    }

    public async Task<bool> UpdateBranchAsync(int id, BankBranch branchDto)
    {
        var branchEntity = MapToEntity(branchDto);
        
        var result = await _retryPolicyService.ExecuteWithRetryAsync(async () => 
        {
            var filter = Builders<Branch>.Filter.Eq(b => b.Id, id);
            var updateResult = await _mongoContext.Branches.ReplaceOneAsync(filter, branchEntity);
            return updateResult.ModifiedCount > 0;
        }, $"UpdateBranch_{id}");
        
        if (!result)
        {
            _logger.LogInformation("Branch with ID {Id} not found for update", id);
            return false;
        }
        
        // Invalidate caches
        string cacheKey = $"{BranchByIdCacheKeyPrefix}{id}";
        _memoryCache.Remove(cacheKey);
        _memoryCache.Remove(AllBranchesCacheKey);
        _memoryCache.Remove($"{BranchesByBankIdCachePrefix}{branchDto.BankId}");
        _logger.LogInformation("CACHE INVALIDATE: Removed branch {Id}, all branches, and bank-specific branches from cache after update", id);
        
        // Add updated item to cache
        var updatedBranch = MapToDto(branchEntity);
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(CacheExpirationTime);
        _memoryCache.Set(cacheKey, updatedBranch, cacheEntryOptions);
        _logger.LogInformation("CACHE UPDATE: Stored updated branch {Id} in cache", id);
        
        return true;
    }

    public async Task<bool> DeleteBranchAsync(int id)
    {
        // First get the branch to know its BankId for cache invalidation
        var branch = await GetBranchByIdAsync(id);
        
        if (branch == null)
        {
            _logger.LogInformation("Branch with ID {Id} not found for deletion", id);
            return false;
        }
        
        var result = await _retryPolicyService.ExecuteWithRetryAsync(async () => 
        {
            var filter = Builders<Branch>.Filter.Eq(b => b.Id, id);
            var deleteResult = await _mongoContext.Branches.DeleteOneAsync(filter);
            return deleteResult.DeletedCount > 0;
        }, $"DeleteBranch_{id}");
        
        if (!result)
        {
            _logger.LogInformation("Branch with ID {Id} not found for deletion", id);
            return false;
        }
        
        // Invalidate caches
        string cacheKey = $"{BranchByIdCacheKeyPrefix}{id}";
        _memoryCache.Remove(cacheKey);
        _memoryCache.Remove(AllBranchesCacheKey);
        _memoryCache.Remove($"{BranchesByBankIdCachePrefix}{branch.BankId}");
        _logger.LogInformation("CACHE INVALIDATE: Removed branch {Id}, all branches, and bank-specific branches from cache after deletion", id);
        
        return true;
    }

    // Maps from the database entity to the DTO
    private static BankBranch MapToDto(Branch branch)
    {
        return new BankBranch
        {
            Id = branch.Id,
            BankId = branch.BankId,
            BranchName = branch.BranchName,
            Address = branch.Address,
            City = branch.City,
            State = branch.State,
            ZipCode = branch.ZipCode,
            PhoneNumber = branch.PhoneNumber,
            IsActive = branch.IsActive,
            CreatedDate = branch.CreatedDate
        };
    }

    // Maps from the DTO to a database entity
    private static Branch MapToEntity(BankBranch branch)
    {
        return new Branch
        {
            Id = branch.Id,
            BankId = branch.BankId,
            BranchName = branch.BranchName,
            Address = branch.Address,
            City = branch.City,
            State = branch.State,
            ZipCode = branch.ZipCode,
            PhoneNumber = branch.PhoneNumber,
            IsActive = branch.IsActive,
            CreatedDate = branch.CreatedDate
        };
    }
}