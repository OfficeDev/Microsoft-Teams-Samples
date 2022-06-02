using Microsoft.Extensions.Caching.Distributed;

public class SubkeyDistributedCache : IDistributedCache
{
    private readonly IDistributedCache _decorated;
    private readonly string _prefix;

    public SubkeyDistributedCache(string prefix, IDistributedCache decorated)
    {
        _prefix = prefix;
        _decorated = decorated;
    }

    public byte[] Get(string key)
    {
        return _decorated.Get(GetKey(key));
    }

    public Task<byte[]> GetAsync(string key, CancellationToken token = default)
    {
        return _decorated.GetAsync(GetKey(key), token);
    }

    public void Refresh(string key)
    {
        _decorated.Refresh(GetKey(key));
    }

    public Task RefreshAsync(string key, CancellationToken token = default)
    {
        return _decorated.RefreshAsync(GetKey(key), token);
    }

    public void Remove(string key)
    {
        _decorated.Remove(GetKey(key));
    }

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        return _decorated.RemoveAsync(GetKey(key), token);
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        _decorated.Set(GetKey(key), value, options);
    }

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        return _decorated.SetAsync(GetKey(key), value, options, token);
    }

    private string GetKey(string inKey)
    {
        return $"{_prefix}:{inKey}";
    }
}

public static class SubDistributedCacheExtensions
{
    public static IDistributedCache CreateSubCache(this IDistributedCache cache, string prefix)
    {
        return new SubkeyDistributedCache(prefix, cache);
    }
}