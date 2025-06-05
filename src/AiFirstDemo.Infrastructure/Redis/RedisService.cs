using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AiFirstDemo.Infrastructure.Redis;

public class RedisService : IRedisService
{
    private readonly IDatabase? _database;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<RedisService>? _logger;
    private readonly bool _isConnected;

    public RedisService(IConnectionMultiplexer? connectionMultiplexer, ILogger<RedisService>? logger = null)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        if (connectionMultiplexer == null)
        {
            _logger?.LogError("Redis ConnectionMultiplexer is null - Redis connection is REQUIRED");
            throw new InvalidOperationException("Redis connection is required. ConnectionMultiplexer cannot be null.");
        }

        try
        {
            _database = connectionMultiplexer.GetDatabase();
            _isConnected = true;
            _logger?.LogInformation("✅ Redis service initialized with active connection");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "❌ Failed to get Redis database");
            throw new InvalidOperationException("Failed to initialize Redis database", ex);
        }
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = await _database!.StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>(value!, _jsonOptions) : null;
    }

    public async Task<string?> GetStringAsync(string key)
    {
        try
        {
            var value = await _database!.StringGetAsync(key);
            return value.HasValue ? value.ToString() : null;
        }
        catch (RedisTimeoutException ex)
        {
            _logger?.LogWarning(ex, "Redis timeout getting key {Key}. This might indicate network issues with Azure Redis.", key);
            throw; // Re-throw timeout exceptions to surface Redis issues
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Redis error getting key {Key}.", key);
            throw; // Re-throw to surface Redis connection issues
        }
    }

    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        return await _database!.StringSetAsync(key, json, expiry);
    }

    public async Task<bool> DeleteAsync(string key)
    {
        return await _database!.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        if (!_isConnected || _database == null)
        {
            _logger?.LogDebug("Redis not connected, returning false for exists check: {Key}", key);
            return false;
        }

        return await _database.KeyExistsAsync(key);
    }

    public async Task<bool> SetExpiryAsync(string key, TimeSpan expiry)
    {
        if (!_isConnected || _database == null)
        {
            _logger?.LogDebug("Redis not connected, simulating set expiry for key: {Key}", key);
            return true;
        }

        return await _database.KeyExpireAsync(key, expiry);
    }

    public async Task<long> IncrementAsync(string key, long value = 1)
    {
        try
        {
            return await _database!.StringIncrementAsync(key, value);
        }
        catch (RedisTimeoutException ex)
        {
            _logger?.LogError(ex, "Redis timeout incrementing key {Key}. This indicates network issues with Azure Redis.", key);
            throw; // Re-throw to surface Redis issues
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Redis error incrementing key {Key}.", key);
            throw; // Re-throw to surface Redis connection issues
        }
    }

    public async Task<bool> HashSetAsync(string key, string field, string value)
    {
        if (!_isConnected || _database == null)
        {
            _logger?.LogDebug("Redis not connected, simulating hash set for key: {Key}, field: {Field}", key, field);
            return true;
        }

        return await _database.HashSetAsync(key, field, value);
    }

    public async Task<string?> HashGetAsync(string key, string field)
    {
        if (!_isConnected || _database == null)
        {
            _logger?.LogDebug("Redis not connected, returning null for hash get: {Key}, field: {Field}", key, field);
            return null;
        }

        var value = await _database.HashGetAsync(key, field);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task<bool> HashDeleteAsync(string key, string field)
    {
        if (!_isConnected || _database == null)
        {
            _logger?.LogDebug("Redis not connected, simulating hash delete for key: {Key}, field: {Field}", key, field);
            return true;
        }

        return await _database.HashDeleteAsync(key, field);
    }

    public async Task<Dictionary<string, string>> HashGetAllAsync(string key)
    {
        if (!_isConnected || _database == null)
        {
            _logger?.LogDebug("Redis not connected, returning empty dictionary for hash get all: {Key}", key);
            return new Dictionary<string, string>();
        }

        var hash = await _database.HashGetAllAsync(key);
        return hash.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
    }

    public async Task<long> ListPushAsync(string key, string value)
    {
        if (!_isConnected || _database == null)
        {
            _logger?.LogDebug("Redis not connected, simulating list push for key: {Key}", key);
            return 1;
        }

        return await _database.ListLeftPushAsync(key, value);
    }

    public async Task<string?> ListPopAsync(string key)
    {
        if (!_isConnected || _database == null)
        {
            _logger?.LogDebug("Redis not connected, returning null for list pop: {Key}", key);
            return null;
        }

        var value = await _database.ListRightPopAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task<List<string>> ListRangeAsync(string key, long start = 0, long stop = -1)
    {
        if (!_isConnected || _database == null)
        {
            _logger?.LogDebug("Redis not connected, returning empty list for range: {Key}", key);
            return new List<string>();
        }

        var values = await _database.ListRangeAsync(key, start, stop);
        return values.Select(x => x.ToString()).ToList();
    }

    public async Task<bool> SortedSetAddAsync(string key, string member, double score)
    {
        if (!_isConnected || _database == null)
        {
            _logger?.LogDebug("Redis not connected, simulating sorted set add for key: {Key}", key);
            return true;
        }

        return await _database.SortedSetAddAsync(key, member, score);
    }

    public async Task<List<(string Member, double Score)>> SortedSetRangeWithScoresAsync(string key, long start = 0, long stop = -1, bool descending = false)
    {
        if (!_isConnected || _database == null)
        {
            _logger?.LogDebug("Redis not connected, returning empty list for sorted set range: {Key}", key);
            return new List<(string, double)>();
        }

        var order = descending ? Order.Descending : Order.Ascending;
        var values = await _database.SortedSetRangeByRankWithScoresAsync(key, start, stop, order);
        return values.Select(x => (x.Element.ToString(), x.Score)).ToList();
    }

    public async Task<long> SortedSetRemoveAsync(string key, string member)
    {
        if (!_isConnected || _database == null)
        {
            _logger?.LogDebug("Redis not connected, simulating sorted set remove for key: {Key}", key);
            return 1;
        }

        return await _database.SortedSetRemoveAsync(key, member) ? 1 : 0;
    }

    public double? SortedSetScore(string key, string member)
    {
        if (!_isConnected || _database == null)
        {
            _logger?.LogDebug("Redis not connected, returning null for sorted set score: {Key}, member: {Member}", key, member);
            return null;
        }

        var score = _database.SortedSetScore(key, member);
        return score;
    }

    public async Task<List<string>> GetKeysAsync(string pattern)
    {
        if (!_isConnected || _database == null)
        {
            _logger?.LogDebug("Redis not connected, returning empty list for keys pattern: {Pattern}", pattern);
            return new List<string>();
        }

        try
        {
            var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern);
            return keys.Select(k => k.ToString()).ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting keys with pattern: {Pattern}", pattern);
            return new List<string>();
        }
    }
}