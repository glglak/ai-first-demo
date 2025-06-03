namespace AiFirstDemo.Infrastructure.Redis;

public interface IRedisService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task<string?> GetStringAsync(string key);
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
    Task<bool> DeleteAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task<bool> SetExpiryAsync(string key, TimeSpan expiry);
    Task<long> IncrementAsync(string key, long value = 1);
    Task<bool> HashSetAsync(string key, string field, string value);
    Task<string?> HashGetAsync(string key, string field);
    Task<bool> HashDeleteAsync(string key, string field);
    Task<Dictionary<string, string>> HashGetAllAsync(string key);
    Task<long> ListPushAsync(string key, string value);
    Task<string?> ListPopAsync(string key);
    Task<List<string>> ListRangeAsync(string key, long start = 0, long stop = -1);
    Task<bool> SortedSetAddAsync(string key, string member, double score);
    Task<List<(string Member, double Score)>> SortedSetRangeWithScoresAsync(string key, long start = 0, long stop = -1, bool descending = false);
    Task<long> SortedSetRemoveAsync(string key, string member);
    double? SortedSetScoreAsync(string key, string member);
    Task<List<string>> GetKeysAsync(string pattern);
}