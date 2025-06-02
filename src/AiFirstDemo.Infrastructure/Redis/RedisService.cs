using StackExchange.Redis;
using System.Text.Json;

namespace AiFirstDemo.Infrastructure.Redis;

public class RedisService : IRedisService
{
    private readonly IDatabase _database;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisService(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = await _database.StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>(value!, _jsonOptions) : null;
    }

    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        return await _database.StringSetAsync(key, json, expiry);
    }

    public async Task<bool> DeleteAsync(string key)
    {
        return await _database.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }

    public async Task<long> IncrementAsync(string key, long value = 1)
    {
        return await _database.StringIncrementAsync(key, value);
    }

    public async Task<bool> HashSetAsync(string key, string field, string value)
    {
        return await _database.HashSetAsync(key, field, value);
    }

    public async Task<string?> HashGetAsync(string key, string field)
    {
        var value = await _database.HashGetAsync(key, field);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task<bool> HashDeleteAsync(string key, string field)
    {
        return await _database.HashDeleteAsync(key, field);
    }

    public async Task<Dictionary<string, string>> HashGetAllAsync(string key)
    {
        var hash = await _database.HashGetAllAsync(key);
        return hash.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
    }

    public async Task<long> ListPushAsync(string key, string value)
    {
        return await _database.ListLeftPushAsync(key, value);
    }

    public async Task<string?> ListPopAsync(string key)
    {
        var value = await _database.ListRightPopAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task<List<string>> ListRangeAsync(string key, long start = 0, long stop = -1)
    {
        var values = await _database.ListRangeAsync(key, start, stop);
        return values.Select(x => x.ToString()).ToList();
    }

    public async Task<bool> SortedSetAddAsync(string key, string member, double score)
    {
        return await _database.SortedSetAddAsync(key, member, score);
    }

    public async Task<List<(string Member, double Score)>> SortedSetRangeWithScoresAsync(string key, long start = 0, long stop = -1, bool descending = false)
    {
        var order = descending ? Order.Descending : Order.Ascending;
        var values = await _database.SortedSetRangeByRankWithScoresAsync(key, start, stop, order);
        return values.Select(x => (x.Element.ToString(), x.Score)).ToList();
    }

    public async Task<long> SortedSetRemoveAsync(string key, string member)
    {
        return await _database.SortedSetRemoveAsync(key, member) ? 1 : 0;
    }

    public async Task<double?> SortedSetScoreAsync(string key, string member)
    {
        var score = await _database.SortedSetScoreAsync(key, member);
        return score;
    }
}