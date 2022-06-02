namespace Microsoft.Teams.Samples.AccountLinking.Service.KeyValueStorage;

/// <summary>
/// An IUserTokenStore is responsible for storing a token securely.
/// </summary>
public interface IKeyValueStore
{
    Task SetAsync(string key, byte[] value, CancellationToken cancellationToken = default);

    Task DeleteAsync(string key, CancellationToken cancellationToken = default);

    Task<byte[]?> GetAsync(string key, CancellationToken cancellationToken = default);
}