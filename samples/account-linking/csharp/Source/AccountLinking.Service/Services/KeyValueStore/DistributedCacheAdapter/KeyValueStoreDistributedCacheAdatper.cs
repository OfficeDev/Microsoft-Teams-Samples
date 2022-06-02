using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Microsoft.Teams.Samples.AccountLinking.Service.KeyValueStorage;

/// <summary>
/// The KeyValueStoreDistributedCacheAdatper is a naive implementation of the <see cref="ITokenStore"> that stores the values into 
/// an <see cref="IDistributedCache">
/// 
/// This should not be used in production, but be treated as a minimal example of the semantics for user token storage.
/// </summary>
public sealed class KeyValueStoreDistributedCacheAdatper : IKeyValueStore
{
    private readonly IDistributedCache _cache;

    private readonly ILogger<KeyValueStoreDistributedCacheAdatper> _logger;

    private readonly DistributedCacheEntryOptions _cacheEntryOptions;

    public KeyValueStoreDistributedCacheAdatper(
        ILogger<KeyValueStoreDistributedCacheAdatper> logger,
        IOptions<KeyValueStoreDistributedCacheAdatperOptions> options,
        IDistributedCache cache)
    {
        _logger = logger;
        _cache = cache;
        _cacheEntryOptions = options.Value.CacheEntryOptions;
    }

     public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        return _cache.RemoveAsync(key, cancellationToken);
    }

    public Task<byte[]?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        return _cache.GetAsync(key, cancellationToken);
    }

    public async Task SetAsync(string key, byte[] value, CancellationToken cancellationToken = default)
    {
        await _cache.SetAsync(key, value, _cacheEntryOptions, cancellationToken);
    }
}

public class KeyValueStoreDistributedCacheAdatperOptions
{
    public DistributedCacheEntryOptions CacheEntryOptions { get; set; } = new DistributedCacheEntryOptions();
}