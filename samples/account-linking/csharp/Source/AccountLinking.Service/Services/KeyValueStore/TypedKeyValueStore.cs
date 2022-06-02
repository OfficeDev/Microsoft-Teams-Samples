namespace Microsoft.Teams.Samples.AccountLinking.Service.KeyValueStorage;
using System.Text.Json;

/// <summary>
/// An IUserTokenStore is responsible for storing a token securely.
/// </summary>
public interface ITypedKeyValueStore<T>
{
    Task SetAsync(string key, T value, CancellationToken cancellationToken = default);

    Task DeleteAsync(string key, CancellationToken cancellationToken = default);

    Task<T?> GetAsync(string key, CancellationToken cancellationToken = default);
}

public class TypedKeyValueStoreAdapter<T> : ITypedKeyValueStore<T>
{
    private readonly IKeyValueStore _decorated;

    public TypedKeyValueStoreAdapter(IKeyValueStore decorated)
    {
        _decorated = decorated;
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        return _decorated.DeleteAsync(key, cancellationToken);
    }

    public async Task<T?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var valueBytes = await _decorated.GetAsync(key: key, cancellationToken: cancellationToken);
        if (valueBytes == default)
        {
            return default;
        }
        var serializedValue = System.Text.Encoding.UTF8.GetString(valueBytes);
        return JsonSerializer.Deserialize<T>(serializedValue);
    }

    public Task SetAsync(string key, T value, CancellationToken cancellationToken = default)
    {
        var serializedValue = JsonSerializer.Serialize(value);
        var valueBytes = System.Text.Encoding.UTF8.GetBytes(serializedValue);
        return _decorated.SetAsync(key, value: valueBytes, cancellationToken);
    }
}

public static class TypedKeyValueStoreAdapterExtensions
{
    public static ITypedKeyValueStore<T> ToTypedKeyValueStore<T>(this IKeyValueStore store)
    {
        return new TypedKeyValueStoreAdapter<T>(store);
    }
}