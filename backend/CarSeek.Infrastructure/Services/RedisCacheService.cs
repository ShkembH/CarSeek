using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using CarSeek.Application.Common.Interfaces;

namespace CarSeek.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _cache.GetStringAsync(key);
        if (string.IsNullOrEmpty(value))
            return default;

        return JsonSerializer.Deserialize<T>(value, _jsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var jsonValue = JsonSerializer.Serialize(value, _jsonOptions);
        var options = new DistributedCacheEntryOptions();
        
        if (expiration.HasValue)
            options.SetAbsoluteExpiration(expiration.Value);
        else
            options.SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // Default 30 minutes

        await _cache.SetStringAsync(key, jsonValue, options);
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        // Note: Redis doesn't support pattern deletion directly
        // This would need to be implemented with Redis SCAN command
        // For now, we'll just remove the specific key
        await _cache.RemoveAsync(pattern);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        var value = await _cache.GetStringAsync(key);
        return !string.IsNullOrEmpty(value);
    }
} 